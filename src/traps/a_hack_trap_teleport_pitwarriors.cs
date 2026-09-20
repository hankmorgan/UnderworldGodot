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

                            npc.moveNPCToTile(critter: fighter, destTileX: x, destTileY: y);  //this is bugged because the calcs give a world coordinate. not a tile x/y
                        }
                    }
                }
            }
        }

        static int GetPitWarriorPositionOffset(int XCoord, int YCoord)
        {
            return 0;
        }
    }//end class
}//end namespace