namespace Underworld
{
    //The pits of carnage arena fights are fairly complex.
    public partial class playerdat : Loader
    {
        static bool IsAvatarInPitFightGlobal = true;//used in babl_hack.
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
                SetAt(0x65, currval);
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

        private static void SetPitFighter(int i, byte indexToSet)
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
                        fighterObj.npc_goal = 1;
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
                    playerdat.IsFightingInPit = false;
                    playerdat.SetQuest(129,0); //win loss record
                    playerdat.SetQuest(133,0);  //jospurs debt.
                }
            }
        }
    }//end class
}//end namespace