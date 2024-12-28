using System;
using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Trap which controls the behaviour of qbert pyramid and the moongates in the ethereal void in UW2
    /// </summary>
    public class a_hack_trap_qbert : hack_trap
    {
        static int[] qbertmoongatelinks = new int[] { 0x21, 0x7f, 0x4F, 0x5b, 0x10, 0x29, 0xC2 };
        public static void Activate(uwObject trapObj, uwObject[] objList)
        {
            HackTrapQbert(trapObj.owner);
        }

        static void HackTrapQbert(int owner)
        {
            InitialiseQbert();
            switch (owner)
            {
                case < 10: // when <10 starting a pyramid color or going to a pyramid
                    StartPyramid(owner);
                    break;
                case >= 10 and < 20:
                    //Teleport to a location
                    TeleportToLocation(owner: owner);
                    break;
                case >= 20 and < 63:
                    //spawns a hacktrap
                    {
                        UseColouredRoomOrb(owner);
                    }
                    break;
                case 63://stepping on tile
                    StepOnPyramidTile();
                    break;
            }
        }

        /// <summary>
        /// When using the orb in a colour room
        /// </summary>
        /// <param name="owner"></param>
        private static void UseColouredRoomOrb(int owner)
        {
            owner -= 30; //? 
            var si = 0;
            while (si < 7)
            {
                if (playerdat.GetGameVariable(100 + si) == owner)
                {
                    //do hacktrap
                    var ax = 6 - si;
                    var dx = 7 - si;
                    ax = ax * dx;
                    ax = ax / 2;
                    var Y = 0x1D - ax;
                    //TODO spawn a camera hack trap in player object tile
                    Debug.Print($"Point a camera at {0x20},{Y}");
                    return;
                }
                si++;
            }
        }

        private static void TeleportToTopOfPyramid()
        {
            DoTeleport(
                teleportX: 49,
                teleportY: 51,
                newLevel: 69,
                heading: 61);
        }



        /// <summary>
        /// Sets game variables at first run of trap execution
        /// </summary>
        private static void InitialiseQbert()
        {
            if (playerdat.GetGameVariable(100) != 6)
            {
                playerdat.SetGameVariable(100, 6);
                for (int i = 1; i < 7; i++)
                {
                    playerdat.SetGameVariable(100 + i, 0xFF);
                }
            }
        }

        private static void StepOnPyramidTile()
        {
            bool ShowMoongate;
            var var4_puzzleactive = playerdat.GetGameVariable(114);
            int var8 = 0;
            var currentTile = UWTileMap.current_tilemap.Tiles[playerdat.tileX, playerdat.tileY];
            if (currentTile.Ptr == 0x33C4)//should be tile 49,51 -> pyramid top
            {
                var si = 29;
                while (si <= 35)
                {
                    var searchTile = UWTileMap.current_tilemap.Tiles[si, 4];
                    if (searchTile.indexObjectList != 0)
                    {
                        var tmapObject = objectsearch.FindMatchInObjectListChain(
                            ListHeadIndex: searchTile.indexObjectList,
                            majorclass: 5, minorclass: 2, classindex: 0xE,
                            objList: UWTileMap.current_tilemap.LevelObjects);

                        if (tmapObject != null)
                        {
                            var di = 0;
                            while (di < 7)
                            {
                                if (playerdat.GetGameVariable(100 + di) == 0xFF)
                                {
                                    break;
                                }
                                else
                                {
                                    if (playerdat.GetGameVariable(100 + di) == tmapObject.owner)
                                    {
                                        Debug.Print($"Changing owner of {tmapObject.a_name} on tile {si},{4} to 5, Previous value was {tmapObject.owner}");
                                        tmapObject.owner = 5;//changes texture
                                        tmap.RedrawFull(tmapObject);//force redraw.
                                    }
                                }
                                di++;
                            }
                        }
                    }
                    si++;
                }
            }

            //not tile 49,51 or after handling tile 49,51
            if (currentTile.Ptr / 4 != playerdat.GetGameVariable(0x1D1))
            {
                //a new tile has been stood on
                playerdat.SetGameVariable(0x1D1, (int)(currentTile.Ptr / 4));
                var var6 = 0xFF;
                var di = currentTile.floorTexture;
                var si = 0;
            ovr110_4132:
                //var TargetTexture = 0xFF;
                if ((playerdat.GetGameVariable(100 + si) == di) || (playerdat.GetGameVariable(100 + si) == 0xFF))
                {
                    if (playerdat.GetGameVariable(100 + si) == di)
                    {
                        //ovr110_415D
                        si++;
                        if (playerdat.GetGameVariable(100 + si) == 0xFF)
                        {
                            si = 0;
                        }
                        TileInfo.ChangeTile(
                            StartTileX: currentTile.tileX, StartTileY: currentTile.tileY, 
                            newFloorTexture:(playerdat.GetGameVariable(100 + si) & 0xF));
                       // currentTile.floorTexture = (short)(playerdat.GetGameVariable(100 + si) & 0xF); Teleportation.DoRedraw = true; currentTile.Redraw = true;
                        di = 0;

                    ovr110_41D6:
                        if (di < 5)
                        {
                            si = 0;
                        ovr110_41CC:
                            if (di - 5 < si)
                            {
                                //ovr110:4193
                                var oddtile = UWTileMap.current_tilemap.Tiles[49 + si, 51 + di];
                                var8 = oddtile.floorTexture;
                                if (var6 != 0xFF)
                                {//ovr110:41C3
                                    if (var6 == var8)
                                    {
                                        si--;
                                        goto ovr110_41CC;
                                    }
                                    else
                                    {
                                        goto ovr110_41DB;//tile matching has failed
                                    }
                                }
                                else
                                {
                                    var6 = oddtile.floorTexture;
                                    si--;
                                    goto ovr110_41CC;
                                }
                            }
                            else
                            {
                                di++;
                                goto ovr110_41D6;
                            }
                        }
                        ovr110_41DB:
                        ShowMoongate = (var6 == var8);

                        if (ShowMoongate)
                        {
                            di = 0;
                            while (di < 6)
                            {
                                si = 0;
                            ovr110_4228:
                                if (di - 6 < si)
                                {
                                    var anotheroddtile = UWTileMap.current_tilemap.Tiles[49 + si, 51 + di];
                                    TileInfo.ChangeTile(
                                        StartTileX: anotheroddtile.tileX, StartTileY: anotheroddtile.tileY, 
                                        newWallTexture: var6);
                                    //anotheroddtile.wallTexture = (short)var6; Teleportation.DoRedraw = true; anotheroddtile.Redraw = true;
                                    si--;
                                    goto ovr110_4228;
                                }
                                else
                                {
                                    di++;
                                }
                            }

                            //do handling of showing moongates.
                            var obj_teleport_973 = UWTileMap.current_tilemap.LevelObjects[973];
                            
                            
                            var obj_movetrigger_633 = UWTileMap.current_tilemap.LevelObjects[633];
                            var obj_movetrigger_972 = UWTileMap.current_tilemap.LevelObjects[972];
                            
                            var obj_666_moongate = UWTileMap.current_tilemap.LevelObjects[666];
                            var obj_974_moongate = UWTileMap.current_tilemap.LevelObjects[974];
                            
                            
                            obj_movetrigger_972.zpos = 96;
                            objectInstance.Reposition(obj_movetrigger_972);                            

                            if (var6 != 5)
                            {
                                obj_teleport_973.quality = 4;
                                obj_teleport_973.owner = (short)(4 + (var6*6));

                                if (var6 == playerdat.GetGameVariable(108))
                                {
                                    obj_666_moongate.invis = 0;
                                    objectInstance.RedrawFull(obj_666_moongate);                                
                                    
                                    obj_movetrigger_633.zpos = obj_666_moongate.zpos;
                                    objectInstance.Reposition(obj_movetrigger_633);
                                }
                            }
                            else
                            {//set teleport destination to the sigil of binding.
                                obj_teleport_973.quality = 32;
                                obj_teleport_973.owner = 25;
                                obj_teleport_973.heading = 0;
                            }
                            
                            var newlink = qbertmoongatelinks[var6] | 0x200;
                            obj_974_moongate.link = (short)newlink;
                            obj_974_moongate.invis = 0;
                            objectInstance.RedrawFull(obj_974_moongate);
                            
                            if (var4_puzzleactive == 0)
                            {
                                HideMoonGateAndTrigger();
                            }
                        }
                        else
                        {//hidemoongate
                            if (var4_puzzleactive == 0)
                            {
                                HideMoonGateAndTrigger();
                            }
                        }
                        return;
                    }
                    else
                    {
                        //ovr110_43B4
                        if (playerdat.GetGameVariable(100) > 0 && playerdat.GetGameVariable(100) < 10)
                        {//sets floor to default colour
                            TileInfo.ChangeTile(
                                StartTileX: currentTile.tileX, StartTileY: currentTile.tileY, 
                                newFloorTexture: (playerdat.GetGameVariable(100) & 0xF));
                            //currentTile.floorTexture = (short)(playerdat.GetGameVariable(100) & 0xF); Teleportation.DoRedraw = true; currentTile.Redraw = true;
                        }
                    }
                }
                else
                {
                    si++;
                    goto ovr110_4132;
                }
            }
        }

        private static void HideMoonGateAndTrigger()
        {
            var move = UWTileMap.current_tilemap.LevelObjects[972];
            move.zpos = 0;
            objectInstance.Reposition(move);

            var moon = UWTileMap.current_tilemap.LevelObjects[974];
            moon.invis = 1;
            objectInstance.RedrawFull(moon);
        }


        private static void StartPyramid(int owner)
        {
            var si = 0;
            while (si <= 4)
            {
                var gamevarvalue = playerdat.GetGameVariable(100 + si);
                if (gamevarvalue == 0xFF)
                {//freeslot
                    playerdat.SetGameVariable(
                        variableno: 100 + si,
                        value: owner);
                    if (si == 4)
                    {//final colour
                        playerdat.SetGameVariable(
                        variableno: 100 + si + 1,
                        value: 5);
                    }

                    TeleportToTopOfPyramid();
                    return;
                }
                else
                {
                    if (gamevarvalue == owner)
                    {
                        TeleportToTopOfPyramid();
                        //TODO handle hiding moongates that may already be there.
                        return;
                    }
                    si++;
                }
            }
        }

        /// <summary>
        /// Based on the trap owner uses random hidden tiles on the map to determine where the moongate leads to.
        /// </summary>
        /// <param name="owner"></param>
        private static void TeleportToLocation(int owner)
        {
            var counter = 0;
            var tile_a = UWTileMap.current_tilemap.Tiles[owner + 46, 1];
            var newLevel = 69;
            var randomrange = (int)tile_a.wallTexture;

            //ovr110_4480:
            while (counter <= 4)
            {
                var tmpRngRange = (randomrange * 3) >> 1;
                var RngResult_var14 = Rng.r.Next(tmpRngRange);
                if (RngResult_var14 >= randomrange)
                {
                    RngResult_var14 = 1;
                }
                //Debug.Print($"Rng for qbert is {RngResult_var14}");
                var tile_b = UWTileMap.current_tilemap.Tiles[owner + 46, RngResult_var14 + 32];
                var tile_c = UWTileMap.current_tilemap.Tiles[owner + 46, RngResult_var14 + 2];
                var teleportX = tile_b.wallTexture;
                var teleportY = tile_c.wallTexture;

                var heading = ((tile_c.floorTexture + 8) << 2) + ((tile_b.floorTexture & 0x8) >> 3);
                //if ((tile_b.floorTexture & 0x7) != 0)
                //{
                newLevel -= (tile_b.floorTexture & 0x7);
                //}
                counter++;
                if (counter >= 4)
                {//after 4 to find a faraway teleport destination give up and just teleport to the found locaiton.
                    DoTeleport(
                        teleportX: teleportX,
                        teleportY: teleportY,
                        newLevel: newLevel,
                        heading: heading);
                    return;
                }
                else
                {
                    if ((Math.Abs(teleportX - playerdat.tileX) >= 3) || (Math.Abs(teleportY - playerdat.tileY) >= 3))
                    {//only teleport if destination is away from the current location.
                        DoTeleport(
                            teleportX: teleportX,
                            teleportY: teleportY,
                            newLevel: newLevel,
                            heading: heading);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Performs the teleport for qbert.
        /// </summary>
        /// <param name="teleportX"></param>
        /// <param name="teleportY"></param>
        /// <param name="newLevel"></param>
        /// <param name="heading"></param>
        static void DoTeleport(int teleportX, int teleportY, int newLevel, int heading)
        {
            // if (Teleportation.JustTeleported)
            // {
            //     Teleportation.JustTeleported = false;
            //     return;
            // }
            // if (newLevel != playerdat.dungeon_level)
            // {
            //     Teleportation.TeleportLevel = newLevel;
            // }
            // else
            // {
            //     Teleportation.TeleportLevel = -1;
            // }
            // Debug.Print($"{teleportX},{teleportY},{newLevel}");
            // Teleportation.TeleportTileX = teleportX;
            // Teleportation.TeleportTileY = teleportY;

            Teleportation.Teleport(character: 0, 
                tileX: teleportX, 
                tileY: teleportY, 
                newLevel: newLevel, 
                heading: heading);

            //TODO: include heading after teleport
        }
    } //end class
}//end namespace