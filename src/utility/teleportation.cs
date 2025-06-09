using Peaky.Coroutines;
using System.Collections;
using System.Diagnostics;

namespace Underworld
{

    public class Teleportation : UWClass
    {
        //Constants for level change events
        const int EnterLevelMode = 0;
        const int ExitLevelMode = 1;
        const int LoadSaveMode = 2;
        /// <summary>
        /// Function to call when teleporting.
        /// </summary>
        public delegate void TeleportCallBack();


        /// <summary>
        /// To prevent teleporting again when the teleport destination in inside a teleport trap
        /// </summary>
        public static bool JustTeleported;
        static int TeleportLevel = -1;//make these private.
        static int TeleportTileX = -1;
        static int TeleportTileY = -1;
        static int TeleportRotation = 0;//not yet implemented.


        /// <summary>
        /// A function to call once teleported.
        /// </summary>
        public static TeleportCallBack CodeToRunOnTeleport;


        /// <summary>
        /// Processes a pending teleportation. Run only on game loop update and not during a trap chain.
        /// </summary>
        public static void HandleTeleportation()
        {
            if (TeleportLevel != -1)
            {
                //int itemToTransfer = -1;
                if (playerdat.ObjectInHand != -1)
                {//handle moving an object in hand through levels. 
                    Debug.Print("Moving an object through a level while holding it! Dropping it there.");
                    var obj = UWTileMap.current_tilemap.LevelObjects[playerdat.ObjectInHand];

                    //drop objects at the player tile so they cannot be taken out of the dream 
                    //TODO. special case for the telekinesis wand in Scintillus.
                    playerdat.ObjectInHand = -1;
                    uimanager.instance.mousecursor.SetCursorToCursor();
                    var tile = UWTileMap.current_tilemap.Tiles[playerdat.playerObject.tileX, playerdat.playerObject.tileY];
                    obj.zpos = (short)(tile.floorHeight << 3);
                    obj.xpos = 3;
                    obj.ypos = 3;
                    obj.next = tile.indexObjectList;
                    tile.indexObjectList = obj.index;
                    pickup.DropSpecialCases(obj.item_id);

                }

                //remove player from the level
                playerdat.PlacePlayerInTile(newTileX: -1, newTileY: -1, previousTileX: playerdat.playerObject.tileX, previousTileY: playerdat.playerObject.tileY);//removes the player from the tile map data


                //move player object to the save file and clear the data out.
                for (int i = 0; i <= 0x1A; i++)
                {
                    playerdat.pdat[playerdat.PlayerObjectStoragePTR + i] = playerdat.playerObject.DataBuffer[playerdat.playerObject.PTR + i];
                    playerdat.playerObject.DataBuffer[playerdat.playerObject.PTR + i] = 0;
                }

                //handle leave level events
                LevelChangeEvents(ExitLevelMode, playerdat.dungeon_level);

                playerdat.dungeon_level = TeleportLevel;

                //switch level
                UWTileMap.LoadTileMap(
                        newLevelNo: playerdat.dungeon_level - 1,
                        datafolder: playerdat.currentfolder,
                        newGameSession: false);

                //copy back the player object
                //copy player object from the save file back into the tilemap objects
                for (int i = 0; i <= 0x1A; i++)
                {
                    playerdat.playerObject.DataBuffer[playerdat.playerObject.PTR + i] = playerdat.pdat[playerdat.PlayerObjectStoragePTR + i];
                }

                if ((TeleportTileX != -1) && (TeleportTileY != -1))
                {
                    //move to new tile on different level
                    InitialisePlayerOnLevelOrPositionChange(TeleportTileX, TeleportTileY);
                }

                //Handle enter level events
                LevelChangeEvents(EnterLevelMode, playerdat.dungeon_level);

                if (_RES == GAME_UW2)
                {//SCD.ARK then needs to be processed in UW2
                    Debug.Print("Processing SCD due to level transition");
                    scd.ProcessSCDArk(1);
                }
            }
            else
            {
                //teleport within a map
                if ((TeleportTileX != -1) && (TeleportTileY != -1))
                {
                    //move to new tile
                    //var targetTile = UWTileMap.current_tilemap.Tiles[TeleportTileX, TeleportTileY];
                    InitialisePlayerOnLevelOrPositionChange(tileX: TeleportTileX, tileY: TeleportTileY, removefromtile: true);
                }
            }


            if ((TeleportTileX != -1) || (TeleportTileY != -1) || (TeleportLevel != -1))
            {
                JustTeleported = true;
                _ = Coroutine.Run(
                PauseTeleport(),
                main.instance
                );
            }
            TeleportLevel = -1;
            TeleportTileX = -1;
            TeleportTileY = -1;
            if (CodeToRunOnTeleport != null)
            {//handle a callback.
                CodeToRunOnTeleport();
                CodeToRunOnTeleport = null;//although this should be handled by the method itself
            }
        }



        /// <summary>
        /// Handles the reseting of the player motion params when teleporting to a new tile, starting a new game or changing levels
        /// </summary>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        /// <param name="arg4"></param>
        /// <param name="removefromtile"></param>
        public static void InitialisePlayerOnLevelOrPositionChange(int tileX, int tileY, int arg4 = -1, bool removefromtile = false)
        {
            var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];//place object in tile will refresh player object on next game tick.

            if (removefromtile)
            {
                playerdat.PlacePlayerInTile(-1, -1, -1, -1);
            }

            Loader.setAt(UWMotionParamArray.PlayerMotionHandler_dseg_67d6_26AA, 0, 16, 0);
            motion.playerMotionParams.unk_14 = 0;
            motion.playerMotionParams.unk_10_Z = 0;
            motion.playerMotionParams.unk_e_Y = 0;

            motion.playerMotionParams.unk_c_X = 0;
            motion.playerMotionParams.unk_a_pitch = 0;
            motion.playerMotionParams.unk_8_y = 0;

            motion.playerMotionParams.unk_6_x = 0;

            motion.playerMotionParams.x_0 = (short)((tileX << 8) + 0x80);

            motion.playerMotionParams.y_2 = (short)((tileY << 8) + 0x80);


            motion.playerMotionParams.unk_24 = 8;
            motion.playerMotionParams.index_20 = 1;
            motion.playerMotionParams.z_4 = (short)(tile.floorHeight * 0x40);
            if ((motion.TileTraversalFlags_dseg_67d6_1BA6[tile.tileType] & 0x20) != 0)
            {
                motion.playerMotionParams.z_4 += 0x20;
            }

            if (arg4 != -1)
            {
                if (1000 - commonObjDat.height(127) > motion.playerMotionParams.z_4)
                {
                    motion.playerMotionParams.z_4 = (short)(1000 - (commonObjDat.height(127) << 3));
                    motion.playerMotionParams.unk_10_Z = -4;
                }
            }
            playerdat.playerObject.zpos = (short)(motion.playerMotionParams.z_4 >> 3);
            playerdat.playerObject.xpos = 6;
            playerdat.playerObject.ypos = 6;
            playerdat.playerObject.npc_animation = 1;
            playerdat.playerObject.npc_xhome = (short)tileX;
            playerdat.playerObject.npc_yhome = (short)tileY;
            playerdat.playerObject.tileX = tileX;
            playerdat.playerObject.tileY = tileY;
            playerdat.playerObject.next = 0;

            MotionCalcArray.PtrToMotionCalc = new byte[0x20];

            MotionCalcArray.MotionArrayObjectIndexA = 1;
            MotionCalcArray.Radius8 = (byte)commonObjDat.radius(127);
            MotionCalcArray.Height9 = (byte)commonObjDat.height(127);
            MotionCalcArray.x0 = (ushort)(3 + (tileX << 3));
            MotionCalcArray.y2 = (ushort)(3 + (tileY << 3));
            MotionCalcArray.z4 = (ushort)(motion.playerMotionParams.z_4 >> 3);

            motion.ProcessMotionTileHeights_seg028_2941_385(motion.playerMotionParams.unk_24);
            motion.playerMotionParams.tilestate25 = motion.GetTileState(MotionCalcArray.UnkC_terrain | MotionCalcArray.UnkE);

            motion.ProcessPlayerTileState(motion.playerMotionParams.tilestate25, 0);
            motion.WalkOnSpecialTerrain();
            motion.Examine_dseg_D3 = 1;

            playerdat.PlacePlayerInTile(newTileX: tileX, newTileY: tileY, previousTileX: -1, previousTileY: -1);


            // playerdat.playerObject.zpos = (short)(tile.floorHeight << 3);
            // playerdat.playerObject.xpos = 3; playerdat.playerObject.ypos = 3;
            // playerdat.tileX = tileX; playerdat.tileY = tileY;

            playerdat.PositionPlayerObject();

            // main.gamecam.Position = uwObject.GetCoordinate(
            //     tileX: playerdat.tileX,
            //     tileY: playerdat.tileY,
            //     _xpos: playerdat.xpos,
            //     _ypos: playerdat.ypos,
            //     _zpos: playerdat.camerazpos);
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
                if (playerdat.DreamPlantCounter > 0)
                {
                    if (playerdat.DreamingInVoid)
                    {
                        if (newLevel != 0)
                        {
                            if (worlds.GetWorldNo(newLevel) != 8)
                            {
                                return 2;
                            }
                        }
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
                    if ((tileY != 38) || (tileX < 42) || (tileX > 43))
                    {//when not in tile 38,43?
                        ChangeUW2Quests();
                        goto ProcessTeleport;
                    }
                }

            ProcessTeleport:
                //todo
                if (newLevel != playerdat.dungeon_level)
                {
                    if (character != 0)
                    {
                        //non PC teleport to a new level. Should not happen.
                        return 2;
                    }
                }

                //queue up the teleport.
                if (newLevel == 0)
                {
                    TeleportLevel = -1;
                }
                else
                {
                    TeleportLevel = newLevel;
                }
                TeleportTileX = tileX;
                TeleportTileY = tileY;

                if (newLevel != 0)
                {
                    if (newLevel == playerdat.dungeon_level)
                    {
                        TeleportLevel = -1;//same level.
                    }
                }

                bool coward = false;
                if (playerdat.IsFightingInPit)
                {

                    if (newLevel != playerdat.dungeon_level)
                    {
                        //avatar is a coward
                        coward = true;
                    }
                    else
                    {
                        //Check if avatar is a coward by checking if they are still in an arena.
                        Debug.Print("todo check if avatar is a coward and has left the arena!");
                    }
                }
                if (coward)
                {
                    pitsofcarnage.AvatarIsACoward(skipConversation: true);
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
            playerdat.SetQuest(112, 0);//checked when talking to LB. You have been fighting with the others
            playerdat.SetQuest(124, 1);//is referenced in teleport_trap in relation to quest 112 fighting in the castle. 
        }

        static int TeleportUW1(int character, int tileX, int tileY, int newLevel)
        {
            if (newLevel != 0)
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


        /// <summary>
        /// Moves the player to the position of the moonstone after teleportation using the Gate Travel Spell
        /// </summary>
        public static void JumpToMoonStoneOnLevel()
        {
            //find the moonstone
            var moonstone = objectsearch.FindMatchInFullObjectList(4, 2, 6, UWTileMap.current_tilemap.LevelObjects);
            if (moonstone != null)
            {
                if (UWTileMap.ValidTile(moonstone.tileX, moonstone.tileY))
                {
                    InitialisePlayerOnLevelOrPositionChange(moonstone.tileX, moonstone.tileY);
                }
            }
        }


        /// <summary>
        /// Handle events that happen when player leaves and enters dungeon levels
        /// </summary>
        /// <param name="mode">0 = new game/enter level, 1 = leave level, 2 = unused and 3 = load save game</param>
        /// <param name="dungeon">Either the new or previous dungeon.</param>
        static void LevelChangeEvents(int mode, int dungeon)
        {
            if (_RES == GAME_UW2)
            {
                LevelChangeEventsUW2(mode, dungeon);
            }
            else
            {
                LevelChangeEventsUW1(mode, dungeon);
            }
        }

        /// <summary>
        /// Handle events that happen when player leaves and enters dungeon levels in UW2
        /// </summary>
        /// <param name="mode">0 = new game/enter level, 1 = leave level, 2 = unused and 3 = load save game</param>
        /// <param name="dungeon">Either the new or previous dungeon.</param>
        static void LevelChangeEventsUW2(int mode, int dungeon)
        {
            if (playerdat.armageddon && mode == EnterLevelMode)
            {//entering a level under the influence of armageddon
                Debug.Print("Armageddon flag has been set. Resetting map!");
                UWTileMap.ResetMap(0);
            }
            else
            {
                if (mode == EnterLevelMode)
                {
                    playerdat.LastDamagedNPCIndex = 0; playerdat.LastDamagedNPCType = -1;
                    CallBacks.RunCodeOnAllMobiles(ClearUnkBit_0x15_7_OnMobiles, null);
                    if (playerdat.play_drawn == 0)
                    {
                        //reselect level theme music.
                        Debug.Print("Todo Refresh music themes");
                    }
                    if (dungeon == 1)
                    {
                        playerdat.playerObject.ProjectileSourceID = 0;
                        Debug.Print($"Reentering britannia. make sure door at {TeleportTileX},{TeleportTileY} is set to closed if moving");
                    }
                }
                else
                {
                    if (mode == ExitLevelMode)
                    {
                        //Calm NPCs?
                        Debug.Print("Todo 'Calm NPCS'");
                    }
                }

                //World specific events
                switch ((worlds.world_ids)worlds.GetWorldNo(dungeon))
                {
                    case worlds.world_ids.PrisonTower:
                        {
                            if (mode == ExitLevelMode)
                            {
                                //do a check to see if player has been fighting goblins and set quest 60 is that is the case.
                                prisontower.PrisonTowerQuest60();

                                if (dungeon == 10)
                                {
                                    trigger.TriggerTrapInTile(0x1F, 0x24);
                                    trigger.TriggerTrapInTile(0x23, 0x22);
                                }
                            }
                            break;
                        }
                    case worlds.world_ids.Killorn:
                        //handle killorn is crashing
                        if ((playerdat.GetQuest(50) == 1) && (mode != EnterLevelMode))
                        {
                            if (dungeon == 17)
                            {
                                killorn.KilornIsCrashing(isEnteringLevel: true);
                            }
                        }
                        break;
                    case worlds.world_ids.Academy:
                        //Handle removing the puzzle wand from inventory on level 3 and returning it to it's home position
                        if ((dungeon == 43) && (mode == ExitLevelMode))
                        {
                            academy.RemoveAcademyWand();
                        }
                        break;
                    case worlds.world_ids.Tomb:
                        //Handle killing undead when Loth has been put to rest.
                        if ((playerdat.GetQuest(7) == 1) && (mode == EnterLevelMode))
                        {
                            //loth is dead
                            tomb.KillLothsLiches(-1);
                        }
                        break;
                    case worlds.world_ids.Ethereal:
                        //Handle automap enabled settings
                        if (mode == EnterLevelMode)
                        {
                            playerdat.AutomapEnabled_backup = playerdat.AutomapEnabled;// needs to be backedup due to rarely seen mechanics involving getting lost
                            playerdat.AutomapEnabled = false;
                        }
                        else
                        {
                            if (mode == ExitLevelMode)
                            {
                                playerdat.AutomapEnabled = playerdat.AutomapEnabled_backup;
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Handle events that happen when player leaves and enters dungeon levels in UW1
        /// </summary>
        /// <param name="mode">0 = new game/enter level, 1 = leave level, 2 = unused and 3 = load save game</param>
        /// <param name="dungeon">Either the new or previous dungeon.</param>
        static void LevelChangeEventsUW1(int mode, int dungeon)
        {
            if (playerdat.armageddon && mode == EnterLevelMode)
            {//entering a level under the influence of armageddon
                UWTileMap.ResetMap(0);
            }
            else
            {
                if (mode == EnterLevelMode)
                {
                    playerdat.LastDamagedNPCIndex = 0; playerdat.LastDamagedNPCType = -1;
                    CallBacks.RunCodeOnAllMobiles(ClearUnkBit_0x15_7_OnMobiles, null);
                }
                else
                {
                    if (mode == ExitLevelMode)
                    {
                        //Calm NPCs?
                        Debug.Print("Todo 'Calm NPCS'");
                    }
                }
                switch ((worlds.UW1_Dungeons)dungeon)
                {
                    case worlds.UW1_Dungeons.Tybal:
                        {
                            if (!playerdat.isOrbDestroyed)
                            {
                                switch (mode)
                                {
                                    case EnterLevelMode:
                                        //entering lair while orb is not yet destroyed
                                        playerdat.backup_mana = playerdat.max_mana;
                                        playerdat.max_mana = 0;
                                        playerdat.play_mana = 0;
                                        break;
                                    case ExitLevelMode://restore drained mana.
                                        playerdat.max_mana = playerdat.backup_mana;
                                        playerdat.play_mana = (playerdat.max_mana >> 2);
                                        break;
                                }
                            }
                            break;
                        }
                    case worlds.UW1_Dungeons.Ethereal:
                        {
                            switch (mode)
                            {
                                case EnterLevelMode:
                                    if (playerdat.AutomapEnabled)
                                    {//yes this is correct. Backup mana is repurposed for this use on the void.
                                        playerdat.backup_mana = 1;
                                    }
                                    else
                                    {
                                        playerdat.backup_mana = 0;
                                    }
                                    break;
                                case ExitLevelMode:
                                    playerdat.AutomapEnabled = (playerdat.backup_mana > 0);
                                    break;
                                case LoadSaveMode:
                                    playerdat.AutomapEnabled = false;
                                    break;
                            }
                            break;
                        }
                }
            }
        }


        static void ClearUnkBit_0x15_7_OnMobiles(uwObject obj, int[] paramsarray)
        {
            obj.UnkBit_0X15_Bit7 = 0;
        }

    }//end class
}//end namespace