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


        static void ChangePressureTriggerTexture(uwObject pressuretrigger)
        {
            Debug.Print("Change texture on pressure trigger");
        }

    }//end class
}//end namespace