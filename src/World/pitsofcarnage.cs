using System.Diagnostics;
namespace Underworld
{
    public class pitsofcarnage:UWClass
    {
        public static bool IsAvatarInPitFightGlobal = false;//used in babl_hack.


        public static uwObject CreateRandomPitFighter(int tileX, int tileY, int isPowerfullProbability )
        {
            var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
            bool isPowerfull = false;

            int itemIdToSpawn;

            if (Rng.r.Next(0x10)==0)
            {   //spawn a mage
                itemIdToSpawn = 0x75 + Rng.r.Next(3);
            }
            else
            {   //spawn a fighter
                itemIdToSpawn = 0x78 + Rng.r.Next(2);
            }

            if (Rng.r.Next(3)< isPowerfullProbability )
            {
                isPowerfull = true;
                var rngPowerfull = Rng.r.Next(5);
                if (rngPowerfull <2)
                {
                    switch (rngPowerfull)
                    {
                        case 0:
                            itemIdToSpawn = 0x5C; // great troll
                            break;
                        case 1:
                            itemIdToSpawn = 0x7B;// human
                            break;
                    }
                    isPowerfull = false;
                }
            }

            var obj = ObjectCreator.spawnObjectInTile(
                itemid: itemIdToSpawn, 
                tileX: tileX, tileY: tileY, 
                xpos: (short)Rng.r.Next(1,8), 
                ypos: (short)Rng.r.Next(1,8), 
                zpos: (short)(tile.floorHeight<<3), 
                WhichList: ObjectFreeLists.ObjectListType.MobileList);

            obj.npc_whoami = 0x66;
            if (isPowerfull)
            {
                obj.IsPowerfull = 1;
            }
            else
            {
                obj.IsPowerfull = 0;
            }

            obj.UnkBit_0XD_Bit8 = 1;
            obj.npc_attitude = 0;
            obj.npc_goal = 5;
            obj.npc_gtarg = 1;
            obj.UnkBit_0XA_Bit7 = 1;
            
            return obj;
        }


        /// <summary>
        /// Checks if the specified npc is fighting against the avatar in the pits
        /// </summary>
        /// <returns></returns>
        public static bool IsDuelingAgainstCritter(int index)
        {
            for (int i = 0; i < 5; i++)
            {
                var currentfighter = playerdat.GetPitFighter(i);  //playerdat.GetAt(0x361 + i);
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
            if (playerdat.IsFightingInPit)
            {               
                int firstFighterIndex = -1;
                IsAvatarInPitFightGlobal = true;
                for (int si = 0; si<5;si++)
                {
                    var fighterindex = playerdat.GetPitFighter(si);
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
                            playerdat.SetPitFighter(si,0);//
                        }
                    }                    
                }
                if (firstFighterIndex != -1)
                {
                    var firstfighter = UWTileMap.current_tilemap.LevelObjects[playerdat.GetPitFighter(firstFighterIndex)];
                    if (!skipConversation)
                    {
                        talk.Talk(firstfighter.index, UWTileMap.current_tilemap.LevelObjects,true);
                    }                    
                    playerdat.SetPitFighter(firstFighterIndex, 0);
                    playerdat.IsFightingInPit = false;
                    playerdat.SetQuest(129,0); //win loss record
                    playerdat.SetQuest(133,0);  //jospurs debt.
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
            if (playerdat.GetQuest(129) > playerdat.GetXClock(14))
            {
                playerdat.SetXClock(14, playerdat.GetQuest(129));
            }
            var defeatedfighter = UWTileMap.current_tilemap.LevelObjects[character];
            defeatedfighter.npc_goal = (byte)npc.npc_goals.npc_goal_fear_6;
            RemovePitFighter(character);
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
                byte currentfighter = playerdat.GetPitFighter(i);
                if (currentfighter != 0)
                {
                    fightercounter++;
                    if (currentfighter == fighterToRemove)
                    {
                        playerdat.SetPitFighter(i, 0);
                        fighterRemoved = true;
                    }
                }
            }
            if (fightercounter <= 1)
            {//no longer fighting
                playerdat.IsFightingInPit = false;
            }
            return fighterRemoved;
        }

    }//end class
}//end namepsace