namespace Underworld
{
    /// <summary>
    /// For call back area effect code/events that take place over a range of tiles, list of objects
    /// </summary>
    /// EG Flagging trespass, homing dart targeting, theft
    public class CallBacks : UWClass
    {

        public delegate bool AreaEffectCallBack(int x, int y, uwObject obj, TileInfo tile, int srcIndex);
        public delegate bool SingleObjectCallBack(uwObject obj);
        public delegate void NPCChangeCallBack(uwObject critter, int[] paramsarray);
        public delegate void CutsceneCallBack();

        /// <summary>
        /// Finds objects in area around tile and runs a function against them
        /// </summary>
        /// <param name="methodToCall"></param>
        /// <param name="Rng_arg0"></param>
        /// <param name="srcItemIndex"></param>
        /// <param name="typeOfTargetArg8"></param>
        /// <param name="tileX0"></param>
        /// <param name="tileY0"></param>
        /// <param name="xWidth"></param>
        /// <param name="yHeight"></param>
        public static void RunCodeOnTargetsInArea(AreaEffectCallBack methodToCall, int Rng_arg0, int srcItemIndex, int typeOfTargetArg8, int tileX0, int tileY0, int xWidth, int yHeight)
        {
            int varE = 0;
            if (
                (tileX0 < 64) && (tileY0 < 64)
                &&
                (tileX0 + xWidth >= 0) && (tileY0 + yHeight >= 0)
                )
            {
                if (tileX0 >= 0)
                {
                    if (tileX0 + xWidth >= 64)
                    {
                        xWidth = xWidth - (tileX0 + xWidth - 64);
                    }
                }
                else
                {//tilex0<0
                    xWidth = xWidth + tileX0;
                    tileX0 = 0;
                }

                if (tileY0 >= 0)
                {
                    if (tileY0 + yHeight >= 64)
                    {
                        yHeight = yHeight - (tileY0 + yHeight - 64);
                    }
                }
                else
                {//tilex0<0
                    yHeight = yHeight + tileY0;
                    tileY0 = 0;
                }
                if (xWidth > 0 && yHeight > 0)
                {
                    for (int di_X = tileX0; di_X <= tileX0 + xWidth; di_X++)
                    {
                        if (di_X >= tileX0 + xWidth)
                        {//why is UW doing this??
                            if (typeOfTargetArg8 == 0x40)
                            {
                                if (Rng_arg0 > 0)
                                {
                                    if (varE >= 4)
                                    {
                                        return;
                                    }
                                    varE++;
                                }
                                else
                                {
                                    return;
                                }
                            }
                            else
                            {
                                return;
                            }

                        }
                        for (int si_Y = tileY0; si_Y <= tileY0 + yHeight; si_Y++)
                        {
                            if (UWTileMap.ValidTile(di_X, si_Y))
                            {
                                var currenttile = UWTileMap.current_tilemap.Tiles[di_X, si_Y];
                                if (
                                    (typeOfTargetArg8 == 0x40 || typeOfTargetArg8 == 0x80)
                                    &&
                                    (currenttile.tileType > 0)
                                )
                                {
                                    //TODO set linked object = 0

                                    //do rng call based on the distance form the centre    
                                    var rnd = Rng.r.Next(0, 3 + xWidth * yHeight);
                                    if ((rnd > Rng_arg0) || (typeOfTargetArg8 == 0x80))
                                    {
                                        if (methodToCall(di_X, si_Y, null, currenttile, srcItemIndex))
                                        {
                                            Rng_arg0--;
                                            if (Rng_arg0 == 0)
                                            {
                                                return;
                                            }
                                        }
                                    }
                                }
                                if (typeOfTargetArg8 != 0x40)
                                {
                                    var listIndex = currenttile.indexObjectList;
                                    while (listIndex != 0)
                                    {
                                        var nextObj = UWTileMap.current_tilemap.LevelObjects[listIndex];
                                        if (
                                            (typeOfTargetArg8 == 0x80)
                                            ||
                                            (
                                                (typeOfTargetArg8 == 0)
                                                &&
                                                (nextObj.majorclass == 1)
                                                &&
                                                (nextObj.index != srcItemIndex)
                                            )
                                            ||
                                            (typeOfTargetArg8 == 0xC0)
                                        )
                                        {
                                            if (methodToCall(di_X, si_Y, nextObj, currenttile, srcItemIndex))
                                            {
                                                Rng_arg0--;
                                                if (Rng_arg0 == 0)
                                                {
                                                    return;
                                                }
                                            }
                                        }
                                        listIndex = nextObj.next;
                                    }//while loop                                        
                                }

                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calls method on each object in the chain starting at first obj
        /// </summary>
        /// <param name="methodToCall"></param>
        /// <param name="obj"></param>
        /// <param name="objList"></param>
        /// <returns></returns>
        public static bool RunCodeOnObjectsInChain(SingleObjectCallBack methodToCall, uwObject obj, uwObject[] objList)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                while (obj != null)
                {
                    if (methodToCall(obj))
                    {
                        return true;
                    }
                    else
                    {
                        //check the link on the object
                        if (obj.is_quant == 0 && obj.link > 0)
                        {
                            var linkedObject = objList[obj.link];
                            if (RunCodeOnObjectsInChain(methodToCall, linkedObject, objList))
                            {
                                return true;
                            }
                            else
                            {
                                //fall down and get next
                            }
                        }
                    }


                    //get next
                    if (obj.next != 0)
                    {
                        obj = objList[obj.next];
                    }
                    else
                    {
                        return false;
                    }
                }//end while
                return false;
            }
        }


        /// <summary>
        /// Runs a function on all matching NPCs that have the whoami
        /// </summary>
        /// <param name="methodToCall"></param>
        /// <param name="whoami"></param>
        /// <param name="paramsArray"></param>
        /// <param name="loopAll"></param>
        public static void RunCodeOnNPCS(NPCChangeCallBack methodToCall, int whoami, int[] paramsArray, bool loopAll)
        {
            for (int i = 0; i < UWTileMap.current_tilemap.NoOfActiveMobiles; i++)
            {
                var index = UWTileMap.current_tilemap.GetActiveMobileAtIndex(i);
                if ((index != 0) && (index < 256))
                {
                    var obj = UWTileMap.current_tilemap.LevelObjects[index];
                    if (obj.majorclass == 1)
                    {
                        if (obj.npc_whoami == whoami)
                        {
                            methodToCall(
                                critter: obj,
                                paramsarray: paramsArray);
                            if (!loopAll)
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Runs a function on all matching npcs of a race defined by their critter data
        /// </summary>
        /// <param name="methodToCall"></param>
        /// <param name="race"></param>
        /// <param name="paramsArray"></param>
        public static void RunCodeOnRace(NPCChangeCallBack methodToCall, int race, int[] paramsArray, bool loopAll)
        {
            for (int i = 0; i < UWTileMap.current_tilemap.NoOfActiveMobiles; i++)
            {
                var index = UWTileMap.current_tilemap.GetActiveMobileAtIndex(i);
                if ((index != 0) && (index < 256))
                {
                    var obj = UWTileMap.current_tilemap.LevelObjects[index];
                    if (obj.majorclass == 1)//NPC
                    {
                        if (critterObjectDat.race(obj.item_id)== race)
                        {
                            methodToCall(obj, paramsArray);
                            if (!loopAll)
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

    }//end class
}//end namespace