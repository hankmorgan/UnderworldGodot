using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Specific code for the Scintillus Academy
    /// </summary>
    public class academy : UWClass
    {
        /// <summary>
        /// Index of the academy wand object.
        /// </summary>
        static int FoundAcademyWand = 0;

        /// <summary>
        /// Searches for the telekinesis wand and moves it back to where it should be. Function replicates 2 scenarios where this can be bypasss
        /// 1. Player holds the wand in their hand
        /// 2. Player placed the wand in a container that is dropped on the ground.
        /// </summary>
        public static void RemoveAcademyWand()
        {
            FoundAcademyWand = 0;
            if (playerdat.ObjectInHand != -1)
            {//player is holding an object, this probably does not get called since I force objects to be dropped when transitioning levels currently.
             //This kind of replicates a vanilla bug where the holding the wand in hand allows for bypassing the check. Root cause to be determined..
                var obj = UWTileMap.current_tilemap.LevelObjects[playerdat.ObjectInHand];
                CallBacks.RunCodeOnObjectsInChain(FindTelekinesisWand, obj, UWTileMap.current_tilemap.LevelObjects);
                if (FoundAcademyWand > 0)
                {
                    //found the wand in the chain of objects held by the player in hand. Move it out of that                     
                    //MoveTelekinesisWandToTile();
                    return;
                }
            }

            //try and find it in the map
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    var tile = UWTileMap.current_tilemap.Tiles[x, y];
                    if (tile.indexObjectList != 0)
                    {
                        var obj = UWTileMap.current_tilemap.LevelObjects[tile.indexObjectList];
                        if (CallBacks.RunCodeOnObjectsInChain(
                            methodToCall: FindTelekinesisWand,
                            obj: obj,
                            objList: UWTileMap.current_tilemap.LevelObjects,
                            RunOnLinks: false))
                        {
                            if (FoundAcademyWand > 0)
                            {
                                MoveTelekinesisWandToTile(
                                    inventory: false,
                                    tileX: x,
                                    tileY: y);
                                return;
                            }
                        }
                    }
                }


                //Finally try and find it on the players inventory and remove it from there
                foreach (var obj_inv in playerdat.InventoryObjects)
                {
                    if (obj_inv != null)
                    {
                        CallBacks.RunCodeOnObject(FindTelekinesisWand, obj_inv);
                        if (FoundAcademyWand > 0)
                        {
                            MoveTelekinesisWandToTile(inventory: true);
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if obj is a match for the telekinesis wand
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        static bool FindTelekinesisWand(uwObject obj)
        {
            if (obj.doordir == 1)
            {
                if (obj.item_id == 0x9B)
                {
                    var ench = MagicEnchantment.GetSpellEnchantment(obj, UWTileMap.current_tilemap.LevelObjects);
                    if (ench.IsFlagBit2Set)
                    {
                        if (ench.SpellMajorClass == -1)
                        {
                            if (ench.SpellMinorClass == 0x27)
                            {
                                FoundAcademyWand = obj.index;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Handles the wand movement to the home tile
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        static void MoveTelekinesisWandToTile(bool inventory = false, int tileX = 99, int tileY = 99)
        {
            Debug.Print($"Moving {FoundAcademyWand} to tile");
            if (inventory)
            {
                //pickup and then drop
                var desttile = UWTileMap.current_tilemap.Tiles[31, 29];
                var foundwand = playerdat.InventoryObjects[FoundAcademyWand];
                var newIndex = uimanager.DoPickup(foundwand, ChangeHand: false);
                var dropcoordinate = uwObject.GetCoordinate(desttile.tileX, desttile.tileY, 3, 3, (short)desttile.floorHeight<<3);
                pickup.Drop_old(
                    index: newIndex,
                    objList: UWTileMap.current_tilemap.LevelObjects,
                    dropPosition: dropcoordinate,
                    tileX: desttile.tileX, tileY: desttile.tileY);
            }
            else
            {
                //found on map
                if (UWTileMap.ValidTile(tileX, tileY))
                {
                    var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                    if (tile.indexObjectList != 0)
                    {
                        if (ObjectRemover_OLD.RemoveObjectFromLinkedList(tile.indexObjectList, FoundAcademyWand, UWTileMap.current_tilemap.LevelObjects, tile.Ptr + 2))
                        {
                            var wand = UWTileMap.current_tilemap.LevelObjects[FoundAcademyWand];
                            var desttile = UWTileMap.current_tilemap.Tiles[31, 29];
                            wand.next = desttile.indexObjectList;
                            desttile.indexObjectList = wand.index;
                            wand.tileX = desttile.tileX; wand.tileY = desttile.tileY;
                            wand.xpos = 3; wand.ypos = 0; wand.zpos = (short)((short)desttile.floorHeight<<3);
                            objectInstance.Reposition(wand);//moot but do it anyway.
                        }
                    }
                }
            }
        }
    }//end class
}//end namespace