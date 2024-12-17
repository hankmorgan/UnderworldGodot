namespace Underworld
{
    /// <summary>
    /// Trap which changes the platforms in the terraforming puzzle in Scintillus level 3
    /// </summary>
    public class a_hack_trap_terraformplatforms : hack_trap
    {
        public static void Activate(uwObject trapObj, int triggerX, int triggerY)
        {
            var HeightArg4 = ((trapObj.owner & 3)+1)<<1;

            var Bitfield_arg8 = (int)(trapObj.zpos<<9)
                        | (int)(trapObj.heading<<6)
                        | (int)(trapObj.ypos<<3)
                        | (int)(trapObj.xpos);

            var HeightArg6 = HeightArg4 + 2 + ((trapObj.owner & 0x1c) >>1);

            RisingPlatforms(
                triggerX: triggerX, 
                triggerY: triggerY, 
                heightArg4:HeightArg4,
                heightArg6:HeightArg6,
                Bitfield_arg8:Bitfield_arg8);

        }//end activate

        static void RisingPlatforms(int triggerX, int triggerY, int heightArg4, int heightArg6, int Bitfield_arg8)
        {
            var di = 0;

            while (di<5)
            {
                var var6 = 0;
                while(var6<3)
                {
                    if ((Bitfield_arg8 & (1<<(di*3+var6))) !=0)
                    {
                        var tileX = triggerX + di * 3;
                        var tileY = triggerY + var6 * 3;
                        if(UWTileMap.ValidTile(tileX, tileY))
                        {
                            var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                            int newHeight;
                            if (tile.floorHeight > heightArg4)
                            {
                                newHeight = heightArg4;
                            }
                            else
                            {
                                newHeight = heightArg6;
                            }
                            TileInfo.ChangeTile(
                                StartTileX: tileX, 
                                StartTileY: tileY,
                                newHeight: newHeight);
                        }
                    }
                    var6++;
                }//end while 1
                di++;
            }//end while 2
        }//end func rising platforms
    }//end class
}//end namespace