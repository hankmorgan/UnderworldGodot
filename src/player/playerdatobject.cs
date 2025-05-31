namespace Underworld
{
    public partial class playerdat : Loader
    {
        public static uwObject playerObject
        {
            get
            {
                if(UWTileMap.current_tilemap ==null)
                {
                    //return the local copy in pdat
                    return new uwObject
                        {
                                //isInventory = false,
                                IsStatic = false,
                                index = 1,
                                PTR = PlayerObjectStoragePTR,
                                DataBuffer = pdat
                        };
                }
                else
                {//return the tilemap version of the object.
                    return UWTileMap.current_tilemap.LevelObjects[1];
                }                
            }
        }

        /// <summary>
        /// place where player object data is copied to in the save file.
        /// </summary>
        public static int PlayerObjectStoragePTR
        {
            get
            {
                if (_RES==GAME_UW2)
                {
                    return 0x380;
                }
                else
                {
                    return 0xD5;
                }
            }
        }
        /// <summary>
        /// Remove Player from tile and add in new one
        /// </summary>
        /// <param name="newTileX"></param>
        /// <param name="newTileY"></param>
        public static void PlacePlayerInTile(int newTileX, int newTileY, int previousTileX = -1, int previousTileY = -1)
        {//todo move this object into the tilemap objects per vanilla behaviour.            
            if (UWTileMap.ValidTile(previousTileX, previousTileY))
            {
                //take the player object out of the previous tile
                var tile = UWTileMap.current_tilemap.Tiles[previousTileX, previousTileY];
                ObjectRemover.RemoveObjectFromLinkedList(tile.indexObjectList, 1, UWTileMap.current_tilemap.LevelObjects, tile.Ptr + 2);
                trigger.RunPressureEnterExitTriggersInTile(
                    triggeringObject: playerObject,
                    tile: tile,
                    ZParam: motion.playerMotionParams.z_4 >> 3,
                    triggerType: (int)triggerObjectDat.triggertypes.EXIT);

            }
            if (UWTileMap.ValidTile(newTileX, newTileY))
            {
                var tile = UWTileMap.current_tilemap.Tiles[newTileX, newTileY];
                var obj = UWTileMap.current_tilemap.LevelObjects[1];//the player object.
                obj.next = tile.indexObjectList;
                tile.indexObjectList = 1;//insert into the tile object list so it can be subject to collisions.
                obj.tileX = newTileX; obj.tileY = newTileY;
                trigger.RunPressureEnterExitTriggersInTile(
                    triggeringObject: playerObject,
                    tile: tile,
                    ZParam: motion.playerMotionParams.z_4 >> 3,
                    triggerType: (int)triggerObjectDat.triggertypes.ENTER);
            }

            //TODO update lighting, pressure triggers

        }
    }//end class
}//end namespace