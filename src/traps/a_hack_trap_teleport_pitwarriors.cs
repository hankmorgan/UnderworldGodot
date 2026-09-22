using System;

namespace Underworld
{
    /// <summary>
    /// Trap which flicks switches randomly flicks switches in a range of tiles. Used in Talorus Bly Skup chamber
    /// </summary>
    public class a_hack_trap_teleportpitwarriors : hack_trap
    {
        public static void Activate()
        {
            if (playerdat.IsFightingInPit)
            {
                LoopPitWarriors();
            }
        }

        static void LoopPitWarriors()
        {
            var si = 0;
            for (si = 0; si < 5; si++)
            {
                if (playerdat.GetPitFighter(si) != 0)
                {
                    MovePitWarrior(UWTileMap.current_tilemap.LevelObjects[playerdat.GetPitFighter(si)]);
                }
            }
        }

        static void MovePitWarrior(uwObject fighter)
        {
            if (fighter == null)
            {
                return;
            }
            else
            {
                var xCoord = fighter.xpos + (fighter.npc_xhome << 3);
                var yCoord = fighter.ypos + (fighter.npc_yhome << 3);
                var FighterTile = UWTileMap.current_tilemap.Tiles[fighter.tileX, fighter.tileY];
                var terrain = TerrainDatLoader.GetTerrainDataBit67_unshifted(FighterTile) >> 6;
                if (terrain == 2)  //Lava
                {
                    if (!uwObject.CheckIfInFrontOfPlayer(xcoord: xCoord, ycoord: yCoord))
                    {
                        //player cannot see the warrior.
                        var di = GetPitWarriorPositionOffset(XCoord: xCoord + 0xEA, YCoord: yCoord + 0xEA);
                        var si = di;
                        if (di >= 4)
                        {
                            si = 7 - di;
                        }
                        var x = 0x19 - (si << 1);
                        x = (x << 3) + 3;

                        si = (di + 2) % 8;
                        if (si > 3)
                        {
                            si = 7 - si;
                        }

                        //Ovr107_12BF
                        var y = 0x13 + (si << 2);
                        y = (y << 3) + 3;

                        if (!uwObject.CheckIfInFrontOfPlayer(x, y))
                        {

                            npc.moveNPCToTile(critter: fighter, destTileX: x>>3, destTileY: y>>3);  //in vanilla this is bugged because the calcs give a world coordinate. not a tile x/y. corrected by the >>3
                        }
                    }
                }
            }
        }

        static int GetPitWarriorPositionOffset(int XCoord, int YCoord)
        {
            if (YCoord<=0)
            {
                //ovr167_13F
                if (Math.Abs(XCoord) >= Math.Abs(YCoord))
                {
                    //ovr167_162
                    if (XCoord<=0)
                    {
                        //ovr167_166
                        return 7;
                    }   
                    else
                    {
                        return 4;
                    }                
                }
                else
                {
                    //ovr167_156
                    if (XCoord<=0)
                    {
                        //ovr167_156
                        return 6;
                    }
                    else
                    {
                        //ovr167_15B
                        return 5;
                    }
                }
            }
            else
            {
                if (Math.Abs(XCoord) >= Math.Abs(YCoord))
                {
                    //ovr167_12D
                    if (XCoord >= 0)
                    {
                        //ovr167_136
                        return 0;
                    }
                    else
                    {
                        //ovr167_131
                        return 3;
                    }
                }
                else
                {
                    //ovr167_11F
                    if (XCoord >= 0)
                    {
                        //ovr167_128
                        return 1;
                    }
                    else
                    {
                        //ovr167_123
                        return 2;                        
                    }
                }
            }
        }
    }//end class
}//end namespace