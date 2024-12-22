using System.Diagnostics;

namespace Underworld
{
    public class a_create_object_trap : trap
    {

        /// <summary>
        /// Spawns a copy of the object at trap link. UW1 and UW2 have some differing logic here.
        /// </summary>
        /// <param name="triggerObj"></param>
        /// <param name="trapObj"></param>
        /// <param name="objList"></param>
        public static void Activate(uwObject trapObj, int triggerX, int triggerY, uwObject[] objList)
        {
            if ((trapObj.link != 0) && (trapObj.is_quant == 0))
            {

                var template = objList[trapObj.link];
                Debug.Print($"Cloning {template.index} {template.a_name}");
                if (template != null)
                {
                    if (_RES == GAME_UW2)
                    {
                        CreateObjectUW2(template, triggerX, triggerY, objList);
                    }
                    else
                    {
                        CreateObjectUW1(template, trapObj, triggerX, triggerY, objList);
                    }
                }
            }
        }

        static void CreateObjectUW1(uwObject template, uwObject trapObj, int triggerX, int triggerY, uwObject[] objList)
        {
            if (Rng.r.Next(0, 63) >= trapObj.quality)
            {
                //there appears to be a logic check here first that runs in the area around the template. 
                //not currently implemented as I suspect it is a check that the template is on the map and/or maybe the player 
                //do spawn
                DoCreateObject(triggerX, triggerY, template);
            }
        }

        /// <summary>
        /// UW2 create object has additional logic relating to loths tomb after loth is freed 
        /// and can spawn leveled creatures if the template object is an adventurer
        /// </summary>
        /// <param name="triggerObj"></param>
        /// <param name="trapObj"></param>
        /// <param name="template"></param>
        /// <param name="objList"></param>
        static void CreateObjectUW2(uwObject template, int triggerX, int triggerY, uwObject[] objList)
        {
            if (worlds.GetWorldNo(playerdat.dungeon_level) == 6)//not in loths tomb
            {
                if (playerdat.GetQuest(7) == 1)
                {
                    return; //loth has been freed. no more object creation in this world
                }
            }

            if (template.item_id == 127)//adventurer
            {//UNTESTED
                //spawn a random leveled creature. this is possibly caused by sleeping near the trap
                //traps with this condition existing in the britannia dungeons
                var spawnRngFactor = worlds.GetWorldNo(playerdat.dungeon_level);
                spawnRngFactor++;
                spawnRngFactor *= 3;
                spawnRngFactor += playerdat.GetXClock(1);
                spawnRngFactor /= 9;

                var item_id = 0;
                bool CritterFound = false;
                //loop rng until we get a critter with random str>1
                do
                {
                    var playRngFactor = playerdat.play_level;
                    playRngFactor++;
                    if (playRngFactor <= 0x10)
                    {
                        playRngFactor = 0x10;
                    }
                    if (playRngFactor <= 0)
                    {
                        playRngFactor = 1;
                    }
                    var rnd1 = Rng.r.Next(0, spawnRngFactor);
                    rnd1 <<= 4;

                    var rnd2 = Rng.r.Next(0, playRngFactor);

                    item_id = 0x40 + rnd1 + rnd2;

                    var str = critterObjectDat.strength(item_id);
                    if (str == 0)
                    {
                        CritterFound = false;
                    }
                    else
                    {
                        CritterFound = Rng.r.Next(0, str) == 0;
                    }
                } while (!CritterFound);

                Debug.Print($"This trap will spawn {GameStrings.GetSimpleObjectNameUW(item_id)}");

                //I think the logic now is to temporarily make the template the new item id, 
                template.item_id = item_id;
                //but also initialise its defaults depending on the critter defaults
                ObjectCreator.InitialiseCritter(template);
                //Create the object 
                //template.MobileUnk_0xA |=0x80; //set bit
                template.UnkBit_0XA_Bit7 = 1;
                DoCreateObject(triggerX, triggerY, template);
                //and then revert it back to a the adventurer template.
                template.item_id = 127;

            }
            else
            {
                DoCreateObject(triggerX, triggerY, template);
            }
        }


        /// <summary>
        /// Standard create object
        /// </summary>
        /// <param name="triggerObj"></param>
        /// <param name="template"></param>
        private static void DoCreateObject(int triggerX, int triggerY, uwObject template)
        {
            int slot;
            if (template.IsStatic)
            {
                //static object spawn
                slot = ObjectFreeLists.GetAvailableObjectSlot(ObjectFreeLists.ObjectListType.StaticList);
            }
            else
            {
                //mobile object spawn
                slot = ObjectFreeLists.GetAvailableObjectSlot(ObjectFreeLists.ObjectListType.MobileList);
            }
            var newobj = UWTileMap.current_tilemap.LevelObjects[slot];
            //copy from template to new obj
            if (template.IsStatic)
            {
                for (int i = 0; i < 8; i++)
                {
                    newobj.DataBuffer[newobj.PTR + i] = template.DataBuffer[template.PTR + i];
                }
            }
            else
            {
                for (int i = 0; i < 27; i++)
                {
                    newobj.DataBuffer[newobj.PTR + i] = template.DataBuffer[template.PTR + i];
                }
            }

            //set new position and spawn
            newobj.tileX = triggerX;//triggerObj.quality;
            newobj.tileY = triggerY;//triggerObj.owner;
            if (UWTileMap.ValidTile(newobj.tileX, newobj.tileY))
            {
                var tile = UWTileMap.current_tilemap.Tiles[newobj.tileX, newobj.tileY];
                UWTileMap.GetRandomXYZForTile(tile, out int newxpos, out int newypos, out int newzpos);
                newobj.xpos = (short)newxpos;//obj.xpos;
                newobj.ypos = (short)newypos;///obj.ypos;
                newobj.zpos = (short)newzpos; //obj.zpos;
                newobj.next = tile.indexObjectList;//link to tile
                tile.indexObjectList = newobj.index;
            }
            ObjectCreator.RenderObject(newobj, UWTileMap.current_tilemap);
        }


    }//end class
}//end namespace