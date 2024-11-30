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
            var var4_neededcolour = playerdat.GetGameVariable(0x1CF);
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
                                        tmap.Redraw(tmapObject);//force redraw.
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
                        currentTile.floorTexture = (short)(playerdat.GetGameVariable(100 + si) & 0xF); main.DoRedraw = true; currentTile.Redraw = true;
                        di = 0;

                        while (di < 5)
                        {
                            si = 0;
                        ovr110_41CC:
                            if (di - 5 < si)
                            {
                                var oddtile = UWTileMap.current_tilemap.Tiles[49 + si, 51 + di];
                                var var8 = oddtile.floorTexture;
                                if (var6 != 0xFF)
                                {
                                    if (var6 != var8)
                                    {
                                        si--;
                                        goto ovr110_41CC;
                                    }
                                }
                                else
                                {
                                    var6 = var8;
                                    si--;
                                    goto ovr110_41CC;
                                }
                                //ovr110_41DB;
                                ShowMoongate = (var6 == var8);
                                // if (var6==var8)
                                // {
                                //     ShowMoongate = true;
                                // }
                                // else
                                // {
                                //     ShowMoongate = false;
                                // }

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
                                            anotheroddtile.wallTexture = (short)var6; main.DoRedraw = true; anotheroddtile.Redraw = true;
                                            si--;
                                            goto ovr110_4228;
                                        }
                                        else
                                        {
                                            di++;
                                        }
                                    }

                                    //do handling of showing moongates.
                                    var move = UWTileMap.current_tilemap.LevelObjects[972];
                                    move.zpos = 96;
                                    objectInstance.Reposition(move);

                                    var teleport = UWTileMap.current_tilemap.LevelObjects[973];

                                    objectInstance.Redraw(teleport);

                                    if (var6 != 5)
                                    {
                                        var moon = UWTileMap.current_tilemap.LevelObjects[666];
                                        moon.invis = 0;
                                        objectInstance.Redraw(moon);
                                        var anothermove = UWTileMap.current_tilemap.LevelObjects[633];
                                        anothermove.zpos = moon.zpos;
                                        objectInstance.Reposition(anothermove);
                                    }
                                    else
                                    {//set teleport destination to the shrine.
                                        teleport.quality = 32;
                                        teleport.owner = 25;
                                        teleport.heading = 0;
                                    }
                                    var anothermoon = UWTileMap.current_tilemap.LevelObjects[974];
                                    var newlink = qbertmoongatelinks[var6] | 0x200;
                                    anothermoon.link = (short)newlink;
                                    anothermoon.invis = 0;
                                    objectInstance.Redraw(anothermoon);
                                    if (var4_neededcolour == 0)
                                    {
                                        HideMoonGateAndTrigger();
                                    }
                                    return;
                                }
                                else
                                {
                                    if (var4_neededcolour == 0)
                                    {
                                        HideMoonGateAndTrigger();
                                    }
                                    return;
                                }
                            }
                            else
                            {
                                di++;
                            }
                        }

                        return;
                    }
                    else
                    {
                        //ovr110_43B4
                        if (playerdat.GetGameVariable(100) > 0 && playerdat.GetGameVariable(100) < 10)
                        {//sets floor to default colour
                            currentTile.floorTexture = (short)(playerdat.GetGameVariable(100) & 0xF); main.DoRedraw = true; currentTile.Redraw = true;
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
            objectInstance.Redraw(moon);
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
            if (main.JustTeleported)
            {
                main.JustTeleported = false;
                return;
            }
            if (newLevel != playerdat.dungeon_level)
            {
                main.TeleportLevel = newLevel;
            }
            else
            {
                main.TeleportLevel = -1;
            }
            Debug.Print($"{teleportX},{teleportY},{newLevel}");
            main.TeleportTileX = teleportX;
            main.TeleportTileY = teleportY;
            //TODO: include heading after teleport
        }
    } //end class
}//end namespace