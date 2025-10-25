
namespace Underworld
{

    /// <summary>
    /// Trap that transforms the Castle (water fountain turns off, decays plants into wilting plants and eventually mushrooms, etc)
    /// </summary>
    public class a_hack_trap_godry : trap
    {
        public static int BitField;//need to find out where this gets stored.
        public static void Activate(uwObject trapObj, int triggerX, int triggerY)
        {
            var var6_xclock = playerdat.GetXClock(1);           
            if ((BitField & (1 << var6_xclock)) == 0)
            {
                BitField |= (1 << var6_xclock);//set the bit
                var di_x = triggerX;
                var var8X = trapObj.xpos + triggerX;
                var varAY = trapObj.ypos + triggerY;
                while (di_x <= var8X)
                {
                    var si_y = triggerY;
                    while (si_y <= varAY)
                    {
                        if (UWTileMap.ValidTile(di_x, si_y))
                        {
                            var tile = UWTileMap.current_tilemap.Tiles[di_x, si_y];
                            if (tile.indexObjectList != 0)
                            {
                                var headObj = UWTileMap.current_tilemap.LevelObjects[tile.indexObjectList];
                                CallBacks.RunCodeOnObjectsInChain(SpawnMushroomsAndOtherObjects, headObj, UWTileMap.current_tilemap.LevelObjects);
                            }

                            if (var6_xclock >= 13)
                            {//randomly spawn new mushrooms.
                                if (Rng.r.Next(100) < 43)
                                {
                                    ObjectCreator.spawnObjectInTile(
                                        itemid: 0xB9,
                                        tileX: di_x, tileY: si_y,
                                        xpos: (short)(1 + Rng.r.Next(6)),
                                        ypos: (short)(1 + Rng.r.Next(6)),
                                        zpos: (short)(tile.floorHeight << 3));
                                }
                            }
                            else
                            {//xclock<13 and > 3
                                if (var6_xclock >= 3)
                                {//After visiting ice caverns and the cold spreads to Britannia.
                                    RemoveFountains(trapObj, di_x, si_y);
                                }
                            }
                        }
                        si_y++;
                    }
                    di_x++;
                }
            }
        }

        /// <summary>
        /// Turns off the fountain in Britannia.
        /// </summary>
        /// <param name="trapObj"></param>
        /// <param name="di_x"></param>
        /// <param name="si_y"></param>
        private static void RemoveFountains(uwObject trapObj, int di_x, int si_y)
        {
            //remove the fountain.
            var fountainWaterSpout = objectsearch.FindMatchInTile(
                tileX: di_x, tileY: si_y,
                majorclass: 7,
                minorclass: 0,
                classindex: 9);
            if (fountainWaterSpout != null)
            {
                // var ovl = AnimationOverlay.FindOverlay(fountainWaterSpout.index);
                // if (ovl != null)
                // {
                //     //AnimationOverlay.EndOverlay_DEPRECIATED(ovl);
                
                AnimationOverlay.RemoveAnimationOverlay(fountainWaterSpout.index);
                ObjectRemover.DeleteObjectFromTile_DEPRECIATED(di_x, si_y, fountainWaterSpout.index, true, true);
                //}

                TileInfo.ChangeTile(di_x, si_y, newFloorTexture: trapObj.owner);//turn the base pedestal into ice
            }

            var fountain = objectsearch.FindMatchInTile(
                tileX: di_x, tileY: si_y,
                majorclass: 4,
                minorclass: 2,
                classindex: 0xE);
            if (fountain != null)
            {
                fountain.quality = 0; // so it cannot be drank from?
            }
        }

        /// <summary>
        /// Degrades Plants into Mushrooms and Debris
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool SpawnMushroomsAndOtherObjects(uwObject obj)
        {
            if (Rng.r.Next(3) == 0)
            {
                if (obj.item_id == 0xC1)//grass
                {
                    //In vanilla the game calculates the probabilities of turning this grass into mushrooms, debris etc but does not actually apply to this type object.
                }
                else
                {
                    var si_newitemid = 0;
                    if (obj.item_id == 0xD9)//a plant degrades into a wilting plant.
                    {
                        si_newitemid = 0xDA;
                    }
                    else
                    {
                        if (obj.item_id == 0xDA)//a_plant (wilting) degrades into grass or debris.
                        {
                            if (Rng.r.Next(0, 2) == 0)
                            {
                                si_newitemid = 0xC1;//grass
                            }
                            else
                            {
                                si_newitemid = 0xD6;//debris
                            }
                            if (Rng.r.Next(0, 2) == 0)// 50:50 chance of the degraded item to turn to a mushroom.
                            {
                                si_newitemid = 0xB9;// change to mushroom.
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    if (si_newitemid != 0)
                    {
                        obj.item_id = si_newitemid;
                    }
                    //randomise object position in the tile.
                    if ((obj.xpos != 0) && (obj.xpos != 7))
                    {
                        obj.xpos = (short)Rng.r.Next(8);
                    }
                    if ((obj.ypos != 0) && (obj.ypos != 7))
                    {
                        obj.ypos = (short)Rng.r.Next(8);
                    }
                    objectInstance.RedrawFull(obj);
                }
            }
            return false;
        }
    }//end class
}//end namespace