using Peaky.Coroutines;
using System.Collections;
using System.Diagnostics;

namespace Underworld
{

    public class Teleportation : UWClass
    {

        /// <summary>
        /// To prevent teleporting again when the teleport destination in inside a teleport trap
        /// </summary>
        public static bool JustTeleported;
        static int TeleportLevel = -1;//make these private.
        static int TeleportTileX = -1;
        static int TeleportTileY = -1;
        static int TeleportRotation = 0;//not yet implemented.


        /// <summary>
        /// Processes a pending teleportation. Run only on game loop update and not during a trap chain.
        /// </summary>
        public static void HandleTeleportation()
        {
            if (TeleportLevel != -1)
            {
                int itemToTransfer = -1;
                if (playerdat.ObjectInHand != -1)
                {//handle moving an object in hand through levels. Temporarily add to inventory data.
                    itemToTransfer = playerdat.AddObjectToPlayerInventory(playerdat.ObjectInHand, false);
                }
                playerdat.dungeon_level = TeleportLevel;
                //switch level
                UWTileMap.LoadTileMap(
                        newLevelNo: playerdat.dungeon_level - 1,
                        datafolder: playerdat.currentfolder,
                        newGameSession: false);

                if (itemToTransfer != -1)
                {//takes object back out of inventory.
                    uimanager.DoPickup(itemToTransfer);
                }
            }
            if ((TeleportTileX != -1) && (TeleportTileY != -1))
            {
                //move to new tile
                var targetTile = UWTileMap.current_tilemap.Tiles[TeleportTileX, TeleportTileY];
                playerdat.zpos = targetTile.floorHeight << 3;
                playerdat.xpos = 3; playerdat.ypos = 3;
                playerdat.tileX = TeleportTileX; playerdat.tileY = TeleportTileY;
                main.gamecam.Position = uwObject.GetCoordinate(
                    tileX: playerdat.tileX,
                    tileY: playerdat.tileY,
                    _xpos: playerdat.xpos,
                    _ypos: playerdat.ypos,
                    _zpos: playerdat.camerazpos);
            }

            if ((TeleportTileX != -1) || (TeleportTileY != -1) || (TeleportLevel != -1))
            {
                JustTeleported = true;
                _ = Peaky.Coroutines.Coroutine.Run(
                PauseTeleport(),
                main.instance
                );
            }
            TeleportLevel = -1;
            TeleportTileX = -1;
            TeleportTileY = -1;
        }



        /// <summary>
        /// Handles a request to teleport a charcter. Does not do the final teleport but queues it up for the HandleTeleport function to process.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        /// <param name="newLevel"></param>
        public static int Teleport(int character, int tileX, int tileY, int newLevel, int heading)
        {
            if (_RES == GAME_UW2)
            {
                return TeleportUW2(character, tileX, tileY, newLevel);
            }
            else
            {
                return TeleportUW1(character, tileX, tileY, newLevel);
            }
        }

        /// <summary>
        /// Applies specific UW2 rules for teleportation.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        /// <param name="newLevel"></param>
        static int TeleportUW2(int character, int tileX, int tileY, int newLevel)
        {
            if (character == 0)
            {
                if (playerdat.DreamPlantEaten)
                {
                    if (playerdat.DreamingInVoid)
                    {
                        return 2;
                    }
                }
                if (playerdat.GetQuest(112) != 0)//avatar fighting in the castle
                {
                    if (newLevel != 0)
                    {
                        if (newLevel != 1)
                        {
                            ChangeUW2Quests();
                            goto ProcessTeleport;
                        }
                    }
                    if ((tileY != 38) || (tileX<42)  || (tileX >43))
                    {//when not in tile 38,43?
                        ChangeUW2Quests();
                        goto ProcessTeleport;
                    }                    
                }   

            ProcessTeleport:
                //todo
                if (newLevel != playerdat.dungeon_level)
                {
                    if (character!=0)
                    {
                        //non PC teleport to a new level. Should not happen.
                        return 2;
                    }
                }

                //queue up the teleport.
                if (newLevel==0)
                {
                    TeleportLevel = -1;
                }
                else
                {
                    TeleportLevel = newLevel;
                }                
                TeleportTileX = tileX;
                TeleportTileY = tileY;

                if (newLevel !=0)
                {
                    if (newLevel == playerdat.dungeon_level)
                    {
                        TeleportLevel = -1;//same level.
                    }                    
                }
                
                bool coward = false;
                if (playerdat.IsFightingInPit)
                    {
                        
                        if (newLevel!=playerdat.dungeon_level)
                        {
                            //avatar is a coward
                            coward = true;
                        }
                        else
                        {
                            //Check if avatar is a coward by checking if they are still in an arena.
                            
                        }
                    }
                if (coward)
                {
                    Debug.Print ("avatar is a coward!");
                }

                //TODO: Handle moonstones.
                return 0x10;
            }
            else
            {
                return 2;
            }
        }


        static void ChangeUW2Quests()
        {
            playerdat.SetQuest(112, 0);
            playerdat.SetQuest(124, 1);
        }

        static int TeleportUW1(int character, int tileX, int tileY, int newLevel)
        {
            if (newLevel !=0)
            {
                if (newLevel == playerdat.dungeon_level)
                {
                    TeleportLevel = -1;//same level.
                }    
                else
                {
                    TeleportLevel = newLevel;
                }                
            }
            else
            {
                TeleportLevel = -1;
            }
            TeleportTileX = tileX;
            TeleportTileY = tileY;
            return 2;
        }


        /// <summary>
        /// Puts a block on sucessive level transitions due to teleport placing player in a new move trigger
        /// </summary>
        /// <returns></returns>
        public static IEnumerator PauseTeleport()
        {
            JustTeleported = true;
            yield return new WaitForSeconds(1);
            JustTeleported = false;
            yield return 0;
        }

    }
}