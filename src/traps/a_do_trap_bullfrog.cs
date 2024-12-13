namespace Underworld
{
    /// <summary>
    /// For controlling the UW1 bullfrog puzzle
    /// </summary>
    public class a_do_trap_bullfrog : hack_trap
    {
        public static void Activate(uwObject trapObj, int triggerX, int triggerY)
        {
            Bullfrog(
                mode: trapObj.owner, 
                triggerX: triggerX, triggerY: triggerY);
        }


        public static void Bullfrog(int mode, int triggerX, int triggerY)
        {
            if (playerdat.dungeon_level !=4)
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, 191));// there is a pained whining sound
                return;
            }
            else
            {
                switch (mode)
                {
                    case 0: //raise/lower
                    case 1:
                        break;
                    case 2: //set Y variable
                        {
                            var tmp = playerdat.GetGameVariable(25);
                            tmp++;
                            tmp = tmp & 0x7;
                            playerdat.SetGameVariable(25,tmp);
                            break;
                        }
                    case 3: // set X variable
                        {
                            var tmp = playerdat.GetGameVariable(24);
                            tmp++;
                            tmp = tmp & 0x7;
                            playerdat.SetGameVariable(24,tmp);
                            break;
                        }
                    case 4://reset tiles
                        playerdat.SetGameVariable(26, 0x3F);
                        TileInfo.ChangeTile(
                            StartTileX: 0x30, StartTileY: 0x30, 
                            newHeight: 4, 
                            DimX:7, DimY:7);
                        break;
                }
            }
        }
        
    }//end class
}//end namespace
