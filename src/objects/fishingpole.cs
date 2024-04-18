namespace Underworld
{
    public class fishingpole : objectInstance
    {

        public static bool use(uwObject obj, bool WorldObject)
        {
            if (WorldObject)
            {                    
                return false;
            }
            else
            {
                //go fish
                var tile = UWTileMap.GetTileInDirection(1.2f);
                if (tile!=null)
                {
                    if (tile.tileType == UWTileMap.TILE_SOLID)
                    {
                        ErrorCannotFishThere();
                    }
                    else
                    {
                        //get floor texture,
                        var terrain = 0;
                        if (_RES==GAME_UW2)
                        {//TOTEST
                            var floorterrain = tileMapRender.FloorTexture(tile);
                            terrain = TerrainDatLoader.Terrain[floorterrain];
                            terrain = (terrain & 0xC0) >> 6;
                        }
                        else
                        {
                            var floorterrain = tileMapRender.FloorTexture(tile);
                            terrain = TerrainDatLoader.Terrain[floorterrain+46];
                            terrain >>= 4;
                        }
                       
                        if (terrain==1)
                        {//water
                            if ((playerdat.zpos>>3) >= (tile.floorHeight -2))
                            {
                                var catchfish = (playerdat.Track + 7)/8 >=Rng.r.Next(0, 5); 
                                //Note: in UW1 this is simply a rng(0-4)==0 check. 
                                //Using the UW2 implementation here as it provides a slight reason to use tracking skill
                                if (catchfish)
                                {
                                    if (playerdat.CanCarryWeight(182))
                                    {
                                        uimanager.AddToMessageScroll(GameStrings.GetString(1,GameStrings.str_you_catch_a_lovely_fish_));
                                        ObjectCreator.SpawnObjectInHand(182);
                                    }
                                    else
                                    {
                                        uimanager.AddToMessageScroll(GameStrings.GetString(1,GameStrings.str_you_feel_a_nibble_but_the_fish_gets_away_));
                                    }
                                }
                                else
                                {
                                    uimanager.AddToMessageScroll(GameStrings.GetString(1,GameStrings.str_no_luck_this_time_));
                                }
                            }
                            else
                            {
                                ErrorCannotFishThere();
                            }
                        }
                        else
                        {
                            ErrorCannotFishThere();
                        }                        
                    }
                }
                else
                {
                    ErrorCannotFishThere();
                }
                return true;
            }
        }

        private static void ErrorCannotFishThere()
        {
            //tile is probably out of bounds.
            uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_cannot_fish_there__perhaps_somewhere_else_));
        }
    }//end class
}//end namespace