namespace Underworld
{
    public class silverseed:objectInstance
    {

        public static bool use(uwObject obj, bool WorldObject)
        {
            if (WorldObject)
            {                    
                return false;
            }

            var tile = UWTileMap.GetTileInDirection(0.6f);
            if (tile!=null)
            {
                if (tile.tileType==UWTileMap.TILE_OPEN)
                {
                    var floorterrain = tileMapRender.FloorTexture(tile)-210;
                    switch (floorterrain)
                    {//i think these are right.
                        case >=5 and <=11:
                        case >=18 and <=22:
                        case >=27 and <=31:
                        case >=35 and <=40:                             
                             //plantit
                            uimanager.AddToMessageScroll(GameStrings.GetString(1,12));
                            playerdat.SilverTreeLevel = playerdat.dungeon_level;
                            
                            var newindex = playerdat.AddInventoryObjectToWorld(obj.index, true, false);
                            //var position = UWTileMap.GetPositionInDirection(0.6f);
                            
                            var newobj = UWTileMap.current_tilemap.LevelObjects[newindex];
                            newobj.zpos = (short)(tile.floorHeight<<2);
                            newobj.xpos =3; newobj.ypos =3;
                            newobj.item_id = 458; //set to tree.                            
                            pickup.Drop(
                                index: newindex,
                                objList: UWTileMap.current_tilemap.LevelObjects,
                                dropPosition:  newobj.GetCoordinate(tile.tileX,tile.tileY),
                                tileX: tile.tileX,
                                tileY: tile.tileY); 

                            animo.CreateAnimoLink(newobj, 0xFFFF);
                            newobj.owner = (short)animationObjectDat.startFrame(newobj.item_id);
                            return true;
                    }
                }
            }
            uimanager.AddToMessageScroll(GameStrings.GetString(1,10));
            return true;   
        }

       
    }//end class
}//end namespace