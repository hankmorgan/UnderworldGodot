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

                            //do drop, change to tree and set up an animo
                            return true;
                    }
                }
            }
            uimanager.AddToMessageScroll(GameStrings.GetString(1,10));
            return true;   
        }

       
    }//end class
}//end namespace