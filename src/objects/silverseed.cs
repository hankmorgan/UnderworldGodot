namespace Underworld
{
    public class silverseed : objectInstance
    {

        public static bool use(uwObject obj, bool WorldObject)
        {
            if (WorldObject)
            {
                return false;
            }
            if (playerdat.dungeon_level == 9)
            {
                return false;
            }

            int x0 =motion.playerMotionParams.x_0 >> 5; int y0 = motion.playerMotionParams.y_2 >> 5;
            motion.GetCoordinateInDirection(motion.PlayerHeadingMinor_dseg_8294 >> 8, 0xB, ref x0, ref y0);//note vanilla is a global full heading value>>8. This heading value gives the same result.
            if (UWTileMap.ValidTile(x0 >> 3, y0 >> 3))
            {
                var tile = UWTileMap.current_tilemap.Tiles[x0 >> 3, y0 >> 3];
                if (tile.tileType == UWTileMap.TILE_OPEN)
                {
                    if (motion.TestIfObjectFitsInTile(0x1CA, 0, x0, y0, tile.floorHeight << 3, 0, 0))
                    {
                        var floorterrain = UWTileMap.current_tilemap.texture_map[tileMapRender.FloorTexture_MapIndex(tile)] - 210;
                        switch (floorterrain)
                        {//i think these are right. but there is some added complexity to how UW1 loads terrain data that I'm missing.
                            case >= 5 and <= 11:
                            case >= 18 and <= 22:
                            case >= 27 and <= 31:
                            case >= 35 and <= 40:
                                //plant it

                                var newobj = ObjectCreator.spawnObjectInTile(458, tile.tileX, tile.tileY, (short)(x0 & 7),(short)(y0 & 7), (short)(tile.floorHeight<<3), ObjectFreeLists.ObjectListType.StaticList, true);
                                if (newobj!=null)
                                {
                                    uimanager.AddToMessageScroll(GameStrings.GetString(1, 12));
                                    playerdat.SilverTreeDungeon = playerdat.dungeon_level;
                                    playerdat.RemoveFromInventory(
                                        index: obj.index,
                                        updateUI: true);                                    
                                    animo.CreateAnimoLink(newobj, 0xFFFF);
                                    newobj.owner = (short)animationObjectDat.startFrame(newobj.item_id);
                                    return true;
                                }
                                break;
                        }
                    }
                }
            }
            uimanager.AddToMessageScroll(GameStrings.GetString(1, 10));
            return true;
        }


    }//end class
}//end namespace