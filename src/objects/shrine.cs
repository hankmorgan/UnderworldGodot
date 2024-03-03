using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using Peaky.Coroutines;

namespace Underworld
{
    public class shrine : model3D
    {
        public shrine(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public static shrine CreateInstance(Node3D parent, uwObject obj, string name)
        {
            var s = new shrine(obj);
            var modelNode = s.Generate3DModel(parent, name);
            SetModelRotation(parent, s);
            return s;
        }

        public override Vector3[] ModelVertices()
        {
            Vector3[] ModelVerts = new Vector3[80];
            ModelVerts[0] = new Vector3(-0.09765625f, 0.125f, -0.0390625f);
            ModelVerts[1] = new Vector3(-0.1289063f, 0.09375f, 0.0703125f);
            ModelVerts[2] = new Vector3(-0.1601563f, 0f, 0.1015625f);
            ModelVerts[3] = new Vector3(-0.09765625f, 0.125f, 0.0390625f);
            ModelVerts[4] = new Vector3(-0.1992188f, 0.4921875f, -0.0390625f);
            ModelVerts[5] = new Vector3(-0.1992188f, 0.4921875f, 0.0390625f);
            ModelVerts[6] = new Vector3(-0.1992188f, 0.640625f, 0.0390625f);
            ModelVerts[7] = new Vector3(-0.1992188f, 0.640625f, -0.0390625f);
            ModelVerts[8] = new Vector3(-0.0625f, 0.625f, 0.0390625f);
            ModelVerts[9] = new Vector3(-0.0625f, 0.625f, -0.0390625f);
            ModelVerts[10] = new Vector3(0.0625f, 0.625f, -0.0390625f);
            ModelVerts[11] = new Vector3(0.0625f, 0.625f, 0.0390625f);
            ModelVerts[12] = new Vector3(0.1992188f, 0.640625f, 0.0390625f);
            ModelVerts[13] = new Vector3(0.1992188f, 0.640625f, -0.0390625f);
            ModelVerts[14] = new Vector3(0.1992188f, 0.4921875f, 0.0390625f);
            ModelVerts[15] = new Vector3(0.1992188f, 0.4921875f, -0.0390625f);
            ModelVerts[16] = new Vector3(0.09765625f, 0.125f, -0.0390625f);
            ModelVerts[17] = new Vector3(0.04296875f, 0.5273438f, -0.0390625f);
            ModelVerts[18] = new Vector3(0.04296875f, 0.5273438f, 0.0390625f);
            ModelVerts[19] = new Vector3(0.09765625f, 0.125f, 0.0390625f);
            ModelVerts[20] = new Vector3(0.09375f, 0.7109375f, -0.0390625f);
            ModelVerts[21] = new Vector3(0.09375f, 0.7109375f, 0.0390625f);
            ModelVerts[22] = new Vector3(0.09765625f, 0.7460938f, -0.0390625f);
            ModelVerts[23] = new Vector3(0.09765625f, 0.7460938f, 0.0390625f);
            ModelVerts[24] = new Vector3(0.09765625f, 0.78125f, -0.0390625f);
            ModelVerts[25] = new Vector3(0.09765625f, 0.78125f, 0.0390625f);
            ModelVerts[26] = new Vector3(0.078125f, 0.8242188f, -0.0390625f);
            ModelVerts[27] = new Vector3(0.078125f, 0.8242188f, 0.0390625f);
            ModelVerts[28] = new Vector3(0.04296875f, 0.8476563f, 0.0390625f);
            ModelVerts[29] = new Vector3(0.04296875f, 0.8476563f, -0.0390625f);
            ModelVerts[30] = new Vector3(0f, 0.8632813f, -0.0390625f);
            ModelVerts[31] = new Vector3(0f, 0.8632813f, 0.0390625f);
            ModelVerts[32] = new Vector3(0f, 0.6679688f, -0.0390625f);
            ModelVerts[33] = new Vector3(0f, 0.6679688f, 0.0390625f);
            ModelVerts[34] = new Vector3(0.0234375f, 0.703125f, 0.0390625f);
            ModelVerts[35] = new Vector3(0.0234375f, 0.703125f, -0.0390625f);
            ModelVerts[36] = new Vector3(0.03515625f, 0.734375f, 0.0390625f);
            ModelVerts[37] = new Vector3(0.03515625f, 0.734375f, -0.0390625f);
            ModelVerts[38] = new Vector3(0.03515625f, 0.7617188f, 0.0390625f);
            ModelVerts[39] = new Vector3(0.03515625f, 0.7617188f, -0.0390625f);
            ModelVerts[40] = new Vector3(0.02734375f, 0.7773438f, 0.0390625f);
            ModelVerts[41] = new Vector3(0.02734375f, 0.7773438f, -0.0390625f);
            ModelVerts[42] = new Vector3(0.015625f, 0.7851563f, 0.0390625f);
            ModelVerts[43] = new Vector3(0.015625f, 0.7851563f, -0.0390625f);
            ModelVerts[44] = new Vector3(0f, 0.7890625f, 0.0390625f);
            ModelVerts[45] = new Vector3(0f, 0.7890625f, -0.0390625f);
            ModelVerts[46] = new Vector3(-0.03125f, 0.7773438f, -0.0390625f);
            ModelVerts[47] = new Vector3(-0.03125f, 0.7773438f, 0.0390625f);
            ModelVerts[48] = new Vector3(-0.015625f, 0.7851563f, 0.0390625f);
            ModelVerts[49] = new Vector3(-0.015625f, 0.7851563f, -0.0390625f);
            ModelVerts[50] = new Vector3(-0.03515625f, 0.7617188f, -0.0390625f);
            ModelVerts[51] = new Vector3(-0.03515625f, 0.7617188f, 0.0390625f);
            ModelVerts[52] = new Vector3(-0.03515625f, 0.734375f, -0.0390625f);
            ModelVerts[53] = new Vector3(-0.03515625f, 0.734375f, 0.0390625f);
            ModelVerts[54] = new Vector3(-0.0234375f, 0.703125f, -0.0390625f);
            ModelVerts[55] = new Vector3(-0.0234375f, 0.703125f, 0.0390625f);
            ModelVerts[56] = new Vector3(-0.08203125f, 0.8242188f, -0.0390625f);
            ModelVerts[57] = new Vector3(-0.08203125f, 0.8242188f, 0.0390625f);
            ModelVerts[58] = new Vector3(-0.04296875f, 0.8476563f, 0.0390625f);
            ModelVerts[59] = new Vector3(-0.04296875f, 0.8476563f, -0.0390625f);
            ModelVerts[60] = new Vector3(-0.09765625f, 0.78125f, -0.0390625f);
            ModelVerts[61] = new Vector3(-0.09765625f, 0.78125f, 0.0390625f);
            ModelVerts[62] = new Vector3(-0.09765625f, 0.7460938f, -0.0390625f);
            ModelVerts[63] = new Vector3(-0.09765625f, 0.7460938f, 0.0390625f);
            ModelVerts[64] = new Vector3(-0.09375f, 0.7109375f, -0.0390625f);
            ModelVerts[65] = new Vector3(-0.09375f, 0.7109375f, 0.0390625f);
            ModelVerts[66] = new Vector3(0.1289063f, 0.09375f, 0.0703125f);
            ModelVerts[67] = new Vector3(-0.1289063f, 0.09375f, -0.0703125f);
            ModelVerts[68] = new Vector3(0.1289063f, 0.09375f, -0.0703125f);
            ModelVerts[69] = new Vector3(0.1601563f, 0f, 0.1015625f);
            ModelVerts[70] = new Vector3(-0.1601563f, 0f, -0.1015625f);
            ModelVerts[71] = new Vector3(0.1601563f, 0f, -0.1015625f);
            ModelVerts[72] = new Vector3(-0.04296875f, 0.5273438f, 0.0390625f);
            ModelVerts[73] = new Vector3(-0.04296875f, 0.5273438f, -0.0390625f);
            ModelVerts[74] = new Vector3(0.1992188f, 0.59375f, -0.0390625f);
            ModelVerts[75] = new Vector3(0.1992188f, 0.59375f, 0.0390625f);
            ModelVerts[76] = new Vector3(-0.0859375f, 0.203125f, -0.0390625f);
            ModelVerts[77] = new Vector3(0.0859375f, 0.203125f, -0.0390625f);
            ModelVerts[78] = new Vector3(-0.1015625f, 0.09375f, -0.0703125f);
            ModelVerts[79] = new Vector3(0.1015625f, 0.09375f, -0.0703125f);

            return ModelVerts;
        }

        public override int[] ModelTriangles(int meshNo)
        {
            return new int[] { 44, 31, 28, 44, 58, 31, 42, 28, 27, 40, 27, 25, 38, 25, 23, 36, 23, 21, 34, 21, 11, 48, 57, 58, 47, 61, 57, 51, 63, 61, 53, 65, 63, 55, 8, 65, 8, 55, 33, 8, 33, 11, 11, 33, 34, 65, 53, 55, 53, 63, 51, 51, 51, 51, 51, 61, 47, 47, 57, 48, 48, 58, 44, 44, 28, 42, 42, 27, 40, 40, 25, 38, 38, 23, 36, 36, 21, 34, 8, 5, 6, 8, 72, 5, 11, 12, 14, 11, 14, 18, 72, 8, 18, 18, 8, 11, 3, 72, 18, 19, 3, 18, 1, 3, 19, 66, 1, 19, 69, 2, 1, 69, 1, 66, 45, 30, 59, 49, 59, 56, 46, 56, 60, 50, 60, 62, 52, 62, 64, 54, 64, 9, 45, 29, 30, 43, 24, 26, 39, 22, 24, 37, 20, 22, 35, 10, 20, 9, 10, 32, 9, 7, 4, 9, 4, 73, 10, 15, 13, 10, 17, 15, 9, 73, 10, 10, 73, 17, 17, 73, 76, 17, 76, 77, 76, 0, 77, 77, 0, 16, 0, 67, 16, 16, 67, 68, 67, 70, 68, 68, 70, 71, 32, 10, 35, 35, 20, 37, 37, 22, 39, 39, 24, 43, 43, 29, 45, 45, 59, 49, 46, 49, 56, 50, 46, 60, 52, 50, 62, 54, 52, 64, 32, 54, 9, 2, 70, 1, 70, 67, 1, 67, 0, 1, 1, 0, 3, 3, 0, 73, 3, 73, 72, 72, 73, 4, 72, 4, 5, 5, 4, 6, 6, 4, 7, 8, 6, 7, 8, 7, 9, 65, 8, 9, 65, 9, 64, 63, 65, 64, 63, 64, 62, 61, 63, 62, 61, 62, 60, 57, 61, 60, 57, 60, 56, 58, 57, 56, 58, 56, 59, 31, 58, 59, 31, 59, 30, 28, 31, 30, 28, 30, 29, 27, 28, 29, 27, 29, 26, 25, 27, 26, 25, 26, 24, 23, 25, 24, 23, 24, 22, 21, 23, 22, 21, 22, 20, 11, 21, 20, 11, 20, 10, 12, 11, 10, 12, 10, 13, 14, 12, 13, 14, 13, 15, 18, 14, 15, 18, 15, 17, 19, 18, 17, 19, 17, 16, 66, 19, 16, 66, 16, 68, 69, 66, 68, 69, 68, 71, 2, 69, 71, 2, 71, 70, 45, 44, 42, 43, 42, 40, 41, 40, 38, 39, 38, 36, 37, 36, 34, 35, 34, 33, 32, 33, 55, 54, 55, 53, 52, 53, 51, 50, 51, 47, 46, 47, 48, 49, 48, 44, 42, 43, 45, 40, 41, 43, 38, 39, 41, 36, 37, 39, 34, 35, 37, 33, 32, 35, 55, 54, 32, 53, 52, 54, 51, 50, 52, 47, 46, 50, 48, 49, 46, 44, 45, 49, 26, 29, 43 };
        }

        public override int ModelColour(int meshNo)
        {
            return 80; // agold colour
        }


        /// <summary>
        /// Uses a shrine
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool Use(uwObject obj)
        {//node.Set("variable_name", value);
            main.gamecam.Set("MOVE", false);
            MessageDisplay.WaitingForTypedInput = true;
            uimanager.instance.TypedInput.Text = "";
            uimanager.instance.scroll.Clear();
            uimanager.AddToMessageScroll("Chant the mantra {TYPEDINPUT}|");
            //Add a waiting co-routine to finish this interaction

            _ = Peaky.Coroutines.Coroutine.Run(
                   ShrineWaitForInput(),
               main.instance
               );

            return true;
        }

        static IEnumerator ShrineWaitForInput()
        {
            while (MessageDisplay.WaitingForTypedInput)
            {
                yield return new WaitOneFrame();
            }

            //parse input
            var toMatch = uimanager.instance.TypedInput.Text.ToUpper();
            var mantra = -1;
            for (int mIndex = 0; mIndex <= 25; mIndex++)
            {
                if (GameStrings.GetString(2, 51 + mIndex).ToUpper() == toMatch)
                {
                    mantra = mIndex;
                    break;
                }
            }
            if (mantra == -1)
            { //no match
                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_that_is_not_a_mantra_));
                yield return 0;
            }
            else
            {
                if (mantra <= 19)
                {
                    if (playerdat.SkillPoints > 0)
                    {
                        AdvanceGreatly(mantra);
                    }
                    else
                    {
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_are_not_ready_to_advance_));
                    }
                }
                else
                {
                    ProcessSpecialMantra(mantra-20);//quest mantras and the mantras that increase multiple skills
                }
            }

            //refresh ui elements
            uimanager.RefreshStatsDisplay();
            uimanager.RefreshManaFlask();

            yield return 0;
        }

        /// <summary>
        /// Process the special quest mantras and the mantras that update a set of skills
        /// </summary>
        /// <param name="mantra"></param>
        private static void ProcessSpecialMantra(int mantra)
        {
            {
                var NoOfSkillsToImprove = -1;
                var BaseSkillOffset = -1;
                var NoOfSkillsInGroup = -1;
                switch (mantra)
                {
                    case 0://inform avatar where the cup of wonder is relative to position
                        if (!playerdat.GotCupOfWonder)
                        {
                            DetectCupOfWonder(); 
                        }
                        break;
                    case 1://key of truth
                        if (!playerdat.GotKeyOfTruth)
                        {
                            SpawnKeyOfTruth();
                            uimanager.AddToMessageScroll(GameStrings.GetString(1,GameStrings.str_none_of_your_skills_improved_)); 
                        }
                        break;
                    case 2:// the unused NO mantra
                        break;
                    case 3://attack skills
                        NoOfSkillsInGroup = 7; BaseSkillOffset = 0; NoOfSkillsToImprove = 3; break;
                    case 4://magic skills
                        NoOfSkillsInGroup = 3; BaseSkillOffset = 7; NoOfSkillsToImprove = 2; break;
                    case 5://other skills
                        NoOfSkillsInGroup = 10; BaseSkillOffset = 10; NoOfSkillsToImprove = 4; break;
                }

                if (NoOfSkillsToImprove > 0)
                {
                    if (playerdat.SkillPoints > 0)
                    {
                        var SkillsHaveImproved = false;
                        var SkillsImproved = new List<string>();
                        while (NoOfSkillsToImprove > 0)
                        {
                            //pick a random skill
                            var skillNo = Rng.r.Next(BaseSkillOffset, BaseSkillOffset + NoOfSkillsInGroup);
                            var result = playerdat.IncreaseSkill(skillNo);
                            if (result != 0)
                            {
                                SkillsHaveImproved = true;
                                SkillsImproved.Add(GameStrings.GetString(2, 0x1F + skillNo));
                            }
                            NoOfSkillsToImprove--;
                        }

                        if (SkillsHaveImproved)
                        {//print messsage
                            var index = 1;
                            var msg = $"{GameStrings.GetString(1, GameStrings.str_you_have_advanced_in_)}";
                            foreach (var skillname in SkillsImproved)
                            {
                                if (SkillsImproved.Count > 1)
                                {
                                    if (index == SkillsImproved.Count)
                                    {
                                        //insert and
                                        msg += " and ";
                                    }
                                    else
                                    {
                                        if (index > 1)
                                        {
                                            msg += ", ";
                                        }                                        
                                    }
                                    msg += skillname;
                                }
                                index++;
                            }
                            playerdat.SkillPoints--; //decrement points
                            uimanager.AddToMessageScroll(msg);
                        }
                        else
                        {
                            uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_are_not_ready_to_advance_));
                        }
                    }
                    else
                    {
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_are_not_ready_to_advance_));
                    }
                }
            }
        }


        /// <summary>
        /// Advances a single skill greatly.
        /// </summary>
        /// <param name="skillNo"></param>
        private static void AdvanceGreatly(int skillNo)
        {
            var result1 = playerdat.IncreaseSkill(skillNo);//test attack increase
            var result2 = playerdat.IncreaseSkill(skillNo);
            var skillname = GameStrings.GetString(2, 0x1F + skillNo);

            if ((result1 != 0) || (result2 != 0))
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_knowledge_and_understanding_fill_you_));
                playerdat.SkillPoints--;

                var msg = $"{GameStrings.GetString(1, GameStrings.str_you_have_advanced_greatly_in_)}{skillname}";
                uimanager.AddToMessageScroll(msg);
            }
            else
            {//no skill gain
                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_cannot_advance_any_further_in_that_skill_));
            }
        }

        /// <summary>
        /// Identifies where the cup of wonder can be found
        /// </summary>
        private static void DetectCupOfWonder()
        {
            var targetDungeon = 3;

            var cupstring = GameStrings.GetString(1, 35);//the cup of wonder is
            string direction = GameStrings.GetString(1, 36 + GetDirectionHeadingToCup());
            string Level;
            if (playerdat.dungeon_level==targetDungeon)
            {
                Level ="very near";
            }
            else
            {
                Level = GameStrings.GetString(1, playerdat.dungeon_level + 0x33 - targetDungeon);
            }

            uimanager.AddToMessageScroll($"{cupstring}{direction} and {Level}");
        }

        /// <summary>
        /// Gets the direction string no to the targetx/y from the player
        /// </summary>
        /// <param name="targetX"></param>
        /// <param name="targetY"></param>
        /// <returns></returns>
        private static int GetDirectionHeadingToCup(int targetX = 24, int targetY = 45)
        {
            var x = targetX - playerdat.tileX;
            var y = targetY - playerdat.tileY;

            if (Math.Abs(x)/2 > Math.Abs(y))
            {
                if (x<=0)
                {
                    return 6;
                }
                else
                {
                    return 2;
                }
            }
            else
            {
                if (Math.Abs(y)/2 <= Math.Abs(x))
                {
                    if (x>=0)
                    {
                        if (y>=0)
                        {
                            return 0;
                        }
                        else
                        {
                            return 2;
                        }
                    }
                    else
                    {
                        if (y<=0)
                        {
                            return 5;
                        }
                        else
                        {
                            return 7;
                        }
                    }
                }
                else
                {
                    if (y<=0)
                    {
                        return 4;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// Spawns the key of truth.
        /// </summary>
        public static void SpawnKeyOfTruth()
        {
            if (playerdat.ObjectInHand==-1)
            {//player not holding anything
                ObjectCreator.SpawnObjectInHand(225);
                playerdat.GotKeyOfTruth = true;
            }
        }
    } //end class
} //end namespace