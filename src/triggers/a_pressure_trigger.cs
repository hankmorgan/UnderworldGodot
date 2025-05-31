using System.Data.Common;
using System.Diagnostics;

namespace Underworld
{
    public partial class trigger : UWClass
    {
        static short TotalWeight = 0;

        /// <summary>
        /// Processes a change in height of an object that may potentially be on a pressure trigger or pressure release trigger
        /// Side effect updates object zpos to zParam.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="tile"></param>
        /// <param name="zParam"></param>
        public static void PressureTriggerZChange(uwObject obj, TileInfo tile, int zParam)
        {
            bool result;
            var originalzpos = obj.zpos;
            obj.zpos = (short)zParam;

            //pressure releae
            result = trigger.RunPressureEnterExitTriggersInTile(
                triggeringObject: obj,
                tile: tile,
                ZParam: originalzpos,
                triggerType: (int)triggerObjectDat.triggertypes.PRESSURE_RELEASE);

            if (result)
            {
                result = trigger.RunPressureEnterExitTriggersInTile(
                    triggeringObject: obj,
                    tile: tile,
                    ZParam: zParam,
                    triggerType: (int)triggerObjectDat.triggertypes.PRESSURE);
            }


        }

        public static short CheckWeightOnPressureTrigger(short ListHead, short MinWeight, short TileHeight, short PlayerInventoryWeightAdjustment)
        {

            if (MinWeight != -2)
            {
                TotalWeight = 0;
            }

            var next = ListHead;
            while (next != 0)
            {
                var NextObject = UWTileMap.current_tilemap.LevelObjects[next];
                if (
                    (TileHeight == 0)
                    ||
                    (TileHeight == NextObject.zpos)
                )
                {
                    TotalWeight += (short)(NextObject.ObjectQuantity * commonObjDat.mass(NextObject.item_id));
                    if (PlayerInventoryWeightAdjustment != -1)
                    {
                        if (NextObject == playerdat.playerObject)
                        {
                            TotalWeight += (short)PlayerInventoryWeightAdjustment;
                        }
                    }
                    else
                    {
                        //recursive
                        if ((NextObject.is_quant == 0) && (NextObject.link != 0))
                        {
                            CheckWeightOnPressureTrigger(ListHead: NextObject.link, MinWeight: -2, TileHeight: TileHeight, PlayerInventoryWeightAdjustment: PlayerInventoryWeightAdjustment);
                        }
                    }
                    if (MinWeight >= 0)
                    {
                        if (TotalWeight >= MinWeight)
                        {
                            return 1;
                        }
                    }
                }
                next = NextObject.next;
            }

            if (MinWeight < 0)
            {
                return TotalWeight;
            }
            else
            {
                if (TotalWeight <= MinWeight)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }

        }

        /// <summary>
        /// Update the texture on the pressure plate. Note this function is evaluated at save/load
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="pressuretrigger"></param>
        static void ChangePressureTriggerTexture(TileInfo tile, uwObject pressuretrigger)
        {
            var si_yset = pressuretrigger.ypos & 0x1;
            short di_notyset;
            if (si_yset == 0)
            {
                di_notyset = 1;
            }
            else
            {
                di_notyset = 0;
            }

            var next = tile.indexObjectList;
            while (next != 0)
            {
                var NextObject = UWTileMap.current_tilemap.LevelObjects[next];
                if ((NextObject.OneF0Class & 0x1E) == 0x1A)
                {
                    if ((triggerObjectDat.triggertype(NextObject.item_id) & 0x7) == 7)
                    {
                        //is a pressure release/pressure trigger 
                        //THis is probably done because on save game pressure triggers are re-evaluated.
                        Debug.Print($"Setting ypos of {NextObject.index} {NextObject.a_name} from {NextObject.ypos} to {((NextObject.ypos & 0x6) + di_notyset)}");
                        NextObject.ypos = (short)((NextObject.ypos & 0x6) + di_notyset);
                    }
                }
                next = NextObject.next;
            }

            //evaluate new texture
            if (((pressuretrigger.ypos & 2) >> 0x1) != 0)
            {
                si_yset = tile.floorTexture;
                if (di_notyset == 0)
                {
                    si_yset--;
                }
                else
                {
                    si_yset++;
                }
                Debug.Print($"Changing Floor texture from {tile.floorTexture} to {si_yset}");
                tile.floorTexture = (short)si_yset;
                tile.Redraw = true;
                main.DoRedraw = true;
            }
        }

    }//end class
}//end namespace