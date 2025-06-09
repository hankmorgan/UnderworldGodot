using System;
using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// The skill of detecting monsters at range.
    /// </summary>
    public class tracking : UWClass
    {
        /// <summary>
        /// Detects monsters within a range.
        /// </summary>
        /// <param name="range"></param>
        /// <param name="skillvalue"></param>
        public static void DetectMonsters(int range, int skillvalue)
        {
            int[] Candidates = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] detectioncounter = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            int index_var15 = 0;

            for (int i = 0; i < UWTileMap.current_tilemap.NoOfActiveMobiles; i++)
            {
                var critterIndex = UWTileMap.current_tilemap.GetActiveMobileAtIndex(i);
                if (critterIndex != 0)
                {
                    var critter = UWTileMap.current_tilemap.LevelObjects[critterIndex];
                    if (critter != null)
                    {
                        if (critter.majorclass == 1)
                        {
                            var xVector = critter.npc_xhome - playerdat.playerObject.tileX;
                            var yVector = critter.npc_yhome - playerdat.playerObject.tileY;
                            if ((Math.Abs(xVector) <= range) && (Math.Abs(yVector) <= range))
                            {
                                int headingindex = 0;
                                var skillcheckresult = playerdat.SkillCheck(skillvalue, 0xF - critterObjectDat.maybestealth(critter.item_id));
                                if (skillcheckresult >= playerdat.SkillCheckResult.Success)
                                {
                                    headingindex = Pathfind.ConvertVectorToHeading(xVector, yVector);
                                    detectioncounter[headingindex]++;
                                }
                                if (skillcheckresult == playerdat.SkillCheckResult.CritSucess)
                                {
                                    Candidates[headingindex] = critter.classindex;
                                }
                            }
                        }
                    }
                }
            }

            int maxNoOfDetections_var16 = 0;
            for (int i = 0; i <= detectioncounter.GetUpperBound(0); i++)
            {
                if (detectioncounter[i] > maxNoOfDetections_var16)
                {
                    maxNoOfDetections_var16 = detectioncounter[i];
                }
            }
            if (maxNoOfDetections_var16 != 0)
            {
                int var17;
                if (maxNoOfDetections_var16 >= 4)
                {
                    var17 = 3;
                }
                else
                {
                    var17 = maxNoOfDetections_var16;
                }
                index_var15 = 0;
                for (index_var15 = 0; index_var15 <= detectioncounter.GetUpperBound(0); index_var15++)
                {
                    if (detectioncounter[index_var15] == maxNoOfDetections_var16)
                    {
                        //Detect creature string
                        DetectCreatureString(index_var15, detectioncounter[index_var15]);
                        if (Candidates[index_var15] == 0)
                        {
                            //Do some more strings incl directions
                        }
                        maxNoOfDetections_var16 = var17;
                        var17 = index_var15;
                        break; //out of for loop
                    }
                }

                index_var15 = Rng.r.Next(8);
                for (int var18 = 0; var18 <= detectioncounter.GetUpperBound(0); var18++)
                {
                    if (index_var15 != var17)
                    {
                        if (detectioncounter[index_var15] > maxNoOfDetections_var16)
                        {
                            DetectCreatureString(index_var15, detectioncounter[index_var15]);
                            return;
                        }
                    }
                }

            }
            else
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_detect_no_monster_activity_));
            }
        }

        /// <summary>
        /// Gets the string for "you detect a/few/many creatures"
        /// </summary>
        /// <param name="heading"></param>
        /// <param name="noOfDetections"></param>
        public static void DetectCreatureString(int heading, int noOfDetections)
        {
            Debug.Print($"{noOfDetections} at {heading}");
            //var al = -(heading + 1);
            int detections_stringNo;
            if (noOfDetections > 1)
            {
                detections_stringNo = 1;
            }
            else
            {
                detections_stringNo = 0;
            }
            if (noOfDetections > 4)
            {
                detections_stringNo++;
            }
            if (_RES == GAME_UW2)
            {
                detections_stringNo += 0x3F;
            }
            else
            {
                detections_stringNo += 0x3B;
            }

            var detections = GameStrings.GetString(1, detections_stringNo);//you detect a/few/many
            PrintDetectCreatureDirections(detections, 0, 0, 0, 0, 0, 0, -(heading + 1));
        }

        /// <summary>
        /// Prints the string that tells you where critters are in relation to your position.;
        /// </summary>
        /// <param name="detectionsprefix"></param>
        /// <param name="playerXHome"></param>
        /// <param name="playerYHome"></param>
        /// <param name="RelativeDistance_arg8"></param>
        /// <param name="ObjectX"></param>
        /// <param name="ObjectY"></param>
        /// <param name="argE"></param>
        /// <param name="maybe_heading_arg10"></param>
        static void PrintDetectCreatureDirections(string detectionsprefix, int playerXHome, int playerYHome, int RelativeDistance_arg8, int ObjectX, int ObjectY, int argE, int maybe_heading_arg10)
        {
            string Output = "";
            int DirectionOffset;
            int Heightoffset;
            if (_RES == GAME_UW2)
            {
                DirectionOffset = 0x28;
                Heightoffset = 0x37;
            }
            else
            {
                DirectionOffset = 0x24;
                Heightoffset = 0x33;
            }
            int var1 = 0;
            //uimanager.AddToMessageScroll(detectionsprefix);
            Output += detectionsprefix;

            if (maybe_heading_arg10 >= 0)
            {
                if (Math.Abs(playerXHome - ObjectX) + Math.Abs(playerYHome - ObjectY) > maybe_heading_arg10)
                {
                    var h = Pathfind.ConvertTilePointsToHeading(playerXHome, playerYHome, ObjectX, ObjectY);
                    Output += GameStrings.GetString(1, DirectionOffset + h);
                    var1 = 1;
                }
            }
            else
            {
                Output += GameStrings.GetString(1, DirectionOffset - maybe_heading_arg10 - 1);
                var1 = 1;
            }

            if ((RelativeDistance_arg8 != argE) && (RelativeDistance_arg8 != 0))
            {
                if (var1 != 0)
                {
                    uimanager.AddToMessageScroll(" and ");
                }
                Output += GameStrings.GetString(1, RelativeDistance_arg8 + Heightoffset);
            }
            else
            {
                //ovr167_229
                if ((var1 == 0) && (RelativeDistance_arg8 != 0))
                {
                    Output += "very near";
                }
            }
            uimanager.AddToMessageScroll(Output);
        }
    }//end class
}//end namespace