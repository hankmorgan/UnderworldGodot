using System.Diagnostics;

namespace Underworld
{
    //for handling loop updates for the player.
    public partial class playerdat : Loader
    {        
        static bool PlayerInDeathMode;
        /// <summary>
        /// Handles player death.
        /// </summary>
        static void PlayerDeath()
        {
            if ( PlayerInDeathMode)
            {
                return;
            }
            PlayerInDeathMode = true;

            if(_RES == GAME_UW2)
            {
                //If in Britannia.
                //Check if was killed by a castle inhabitant.get sent to jail. otherwise die.
                //else If in other dimension.
                //Check if dreaming in void. then so wake up where you were sleeping
                //If dueling in the pits then lose duel
                //respawn in gem room.
                if (dungeon_level==1)
                {
                    //playerObject.ProjectileSourceID = 235; for testing against LB
                    //Check if was killed by an ally
                    if(playerObject.ProjectileSourceID !=0)
                    {
                        var killer = UWTileMap.current_tilemap.LevelObjects[playerObject.ProjectileSourceID];
                        if (killer!=null)
                        {   //was avatar killed by a castle inhabitant or Syria or a castle guard.
                            if (
                                ((killer.npc_whoami >=0x81) && (killer.npc_whoami<0x8F))
                                ||
                                (killer.npc_whoami == 0xA8)
                                ||
                                (killer.npc_whoami == 0x95)
                                )
                            {
                                if (killer.UnkBit_0XA_Bit7 == 0)
                                {
                                    //send to jail
                                    Teleportation.CodeToRunOnTeleport = GoToJail;
                                    Teleportation.Teleport(
                                        character: 0, 
                                        tileX: 42, tileY: 38, 
                                        newLevel: 1, 
                                        heading: 0);
                                    return;
                                }                                
                            }
                        }                        
                    }

                }

                if (DreamingInVoid)
                {
                    //check if died while dreaming
                    Debug.Print("Wake up!");
                    uimanager.ReturnToMainMenu();
                    return;
                }

                if (CurrentWorld != 0)
                {
                    //respawn in the gem chamber                        
                    if (IsFightingInPit)
                    {
                        SetQuest(129,0);//arena win record
                        SetQuest(133,0);//jospur debt
                        //clear fighters
                        for(int i = 0; i<5;i++)
                        {
                            SetPitFighter(i,0);//clear opponents.
                        }
                        IsFightingInPit = false;
                    }

                    
                    Teleportation.CodeToRunOnTeleport = ResurrectAtBlackrockGem;
                    Teleportation.Teleport(
                        character: 0, 
                        tileX: 28, tileY: 37, 
                        newLevel: 5, 
                        heading: 0);                    
                }
                else
                {
                    //die fully
                    Debug.Print("Return to main menu");
                    uimanager.ReturnToMainMenu();
                }

            }
            else
            {//uw1

                //if silver tree planted. do the tree animation.
                //respawn at tree level via a delegate function.
                //otherwise return to main menu
                //Start death animation.

                Debug.Print("Do Death Animation");
                if (SilverTreeDungeon !=0)
                {
                    Debug.Print("Do Silver Tree Animation");
                    Teleportation.CodeToRunOnTeleport = ResurrectAtSilverTree;
                    Teleportation.Teleport(
                        character: 0, 
                        tileX: 32, tileY: 32, 
                        newLevel: SilverTreeDungeon, 
                        heading: 0);
                    
                }
                else
                {//die fully
                    Debug.Print("Return to main menu");
                    uimanager.ReturnToMainMenu();
                }
            }
        }



        /// <summary>
        /// In UW1 finds the silver tree on a level and jumps to it's location, heals the player. Runs as a teleport callback
        /// </summary>
        static void ResurrectAtSilverTree()
        {
            Teleportation.CodeToRunOnTeleport = null;
            play_hp = max_hp;
            PlayerInDeathMode = false;
            var tree = objectsearch.FindMatchInFullObjectList(7,0,0xA,UWTileMap.current_tilemap.LevelObjects);
            if (tree != null)
            {
                if (UWTileMap.ValidTile(tree.tileX, tree.tileY))
                {
                    Teleportation.Teleport(
                        character: 0, 
                        tileX: tree.tileX, tileY: tree.tileY, 
                        newLevel: 0, heading: 0);
                }
            }
        }

        static void ResurrectAtBlackrockGem()
        {
            Teleportation.CodeToRunOnTeleport = null;
            play_hp = max_hp;
            PlayerInDeathMode = false;
            uimanager.AddToMessageScroll(GameStrings.GetString(1,0x169));
        }

        static void GoToJail()
        {        
            Teleportation.CodeToRunOnTeleport = null;    
            play_hp = max_hp;
            PlayerInDeathMode = false;        
            //TODO: make humans friendly, set quests, run a trap, advance the clock and move LB to the prison to release you.        
        }

    }//end class
}//end namespace