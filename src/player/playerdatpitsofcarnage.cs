using System.Diagnostics;

namespace Underworld
{
    //The pits of carnage arena fights are fairly complex.
    public partial class playerdat : Loader
    {
        public static bool IsAvatarInPitFightGlobal = false;//used in babl_hack.
        public static bool IsFightingInPit
        {
            get
            {
                return ((GetAt(0x64) >> 2) & 0x1) == 1;
            }
            set
            {
                var currval = GetAt(0x64);
                if (value)
                {//set
                    currval |= 4;
                }
                else
                {//clear
                    currval &= 0xFB;
                }
                SetAt(0x64, currval);
            }
        }

        /// <summary>
        /// Removes a pit fighter from player data and flags that the player is no longer fighting in the pit if that is the case.
        /// </summary>
        /// <param name="fighterToRemove"></param>
        public static bool RemovePitFighter(int fighterToRemove)
        {
            int fightercounter = 0;
            bool fighterRemoved = false;
            for (int i = 0; i < 5; i++)
            {
                byte currentfighter = GetPitFighter(i);
                if (currentfighter != 0)
                {
                    fightercounter++;
                    if (currentfighter == fighterToRemove)
                    {
                        SetPitFighter(i, 0);
                        fighterRemoved = true;
                    }
                }
            }
            if (fightercounter <= 1)
            {//no longer fighting
                IsFightingInPit = false;
            }
            return fighterRemoved;
        }

        public static void SetPitFighter(int i, byte indexToSet)
        {
            SetAt(0x361 + i, indexToSet);
        }


        private static byte GetPitFighter(int i)
        {
            return GetAt(0x361 + i);
        }        


        /// <summary>
        /// Checks if the specified npc is fighting against the avatar in the pits
        /// </summary>
        /// <returns></returns>
        public static bool IsDuelingAgainstCritter(int index)
        {
            for (int i = 0; i < 5; i++)
            {
                var currentfighter = GetAt(0x361 + i);
                if (currentfighter != 0)
                {
                    if (currentfighter == index)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// When avatar flees combat in the pit.
        /// </summary>
        /// <param name="skipConversation"></param>
        public static void AvatarIsACoward(bool skipConversation = false)
        {
            if (IsFightingInPit)
            {               
                int firstFighterIndex = -1;
                IsAvatarInPitFightGlobal = true;
                for (int si = 0; si<5;si++)
                {
                    var fighterindex = GetPitFighter(si);
                    if (fighterindex !=0 )
                    {
                        var fighterObj = UWTileMap.current_tilemap.LevelObjects[fighterindex];
                        fighterObj.npc_attitude = 1;
                        fighterObj.ProjectileSourceID = 0;//clear last hit
                        fighterObj.npc_goal = (byte)npc.npc_goals.npc_goal_goto_1;
                        if (firstFighterIndex == -1)
                        {
                            firstFighterIndex = si;
                        }
                        else
                        {
                            SetPitFighter(si,0);//
                        }
                    }                    
                }
                if (firstFighterIndex != -1)
                {
                    var firstfighter = UWTileMap.current_tilemap.LevelObjects[GetPitFighter(firstFighterIndex)];
                    if (!skipConversation)
                    {
                        talk.Talk(firstfighter.index, UWTileMap.current_tilemap.LevelObjects,true);
                    }                    
                    SetPitFighter(firstFighterIndex, 0);
                    IsFightingInPit = false;
                    SetQuest(129,0); //win loss record
                    SetQuest(133,0);  //jospurs debt.
                }
            }
        }

        /// <summary>
        /// Defeats a pit fighter that has fled the arena.
        /// </summary>
        /// <param name="character"></param>
        public static void DefeatLivingPitFighter(int character)
        {
            Debug.Print("Untested removal of defeated living cowardly pit fighter");
            if (GetQuest(129) >GetXClock(14))
            {
                SetXClock(14,GetQuest(129));
            }
            var defeatedfighter = UWTileMap.current_tilemap.LevelObjects[character];
            defeatedfighter.npc_goal = (byte)npc.npc_goals.npc_goal_fear_6;
            RemovePitFighter(character);
        }

    }//end class
}//end namespace