using System.Diagnostics;

namespace Underworld
{

    /// <summary>
    /// Trap that creates an item in Britannia that suits the player skillset
    /// </summary>
    public class a_hack_trap_transformpotion : trap
    {
        public static void Activate(uwObject trapObj, uwObject triggerObj)
        {
            //change the objects in the player inventory.
            foreach (var obj in playerdat.InventoryObjects)
            {
                DoPotionTransform(obj, false);
            }
            if (playerdat.ObjectInHand != -1)
            {//also what is in their hand
                var obj = UWTileMap.current_tilemap.LevelObjects[playerdat.ObjectInHand];
                DoPotionTransform(obj, true);
                uimanager.instance.mousecursor.SetCursorToObject(obj.item_id);
            }
            //tranforms all red potions into poison on the tile and in the player inventory.
            var tileX = triggerObj.quality; var tileY = triggerObj.owner;
            if (UWTileMap.ValidTile(tileX, tileY))
            {
                //and in the tile the trigger is point at
                var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                IterateObjectsToTransform(tile.indexObjectList);
            }
        }

        private static void IterateObjectsToTransform(int ListHead)
        {
            var next = ListHead;
            while (next != 0)
            {
                var nextObj = UWTileMap.current_tilemap.LevelObjects[next];
                DoPotionTransform(
                    objToChange: nextObj,
                    WorldObject: true);

                if ((nextObj.is_quant == 0) && (nextObj.link != 0))
                {
                    IterateObjectsToTransform(nextObj.link);
                }
                next = nextObj.next;
            }
        }

        static void DoPotionTransform(uwObject objToChange, bool WorldObject)
        {
            if (objToChange == null)
            {
                return;
            }
            if (objToChange.item_id == 229)
            {
                if (objToChange.is_quant == 1) //?
                {
                    if (objToChange.flags2 == 1)
                    {
                        if (objToChange.link > 0)
                        {
                            Debug.Print($"Transform {objToChange.a_name}");
                            //create a damage trap
                            var slot = ObjectCreator.PrepareNewObject(0x180);
                            var dmgtrap = UWTileMap.current_tilemap.LevelObjects[slot];
                            dmgtrap.quality = (short)(1 + Rng.r.Next(0, 8));
                            dmgtrap.owner = 1;

                            objToChange.is_quant = 0;
                            if (!WorldObject)
                            {
                                slot = playerdat.AddObjectToPlayerInventory(slot, false);
                            }
                            objToChange.item_id = 228;
                            objToChange.link = (short)slot;
                            if (WorldObject)
                            {
                                //redraw object on map
                                if (objToChange.instance != null)
                                {
                                    objToChange.instance.uwnode.QueueFree();
                                    ObjectCreator.RenderObject(objToChange, UWTileMap.current_tilemap);
                                }
                            }
                            else
                            {
                                uimanager.UpdateInventoryDisplay();
                            }
                        }
                    }
                }
            }
        }
    }//end clas
}//end namespace