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
            if (playerdat.dungeon_level != 4)
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
                        {
                            if (playerdat.GetGameVariable(26) <= 1)
                            {
                                // number of attempts have ran out. trap needs to be reset before any new changes
                                uimanager.AddToMessageScroll(GameStrings.GetString(1, 192));// there is an empty clicking sound
                                playerdat.SetGameVariable(26, 1);//attempt counter set to 1
                                return;
                            }
                            else
                            {
                                var gamevar24 = playerdat.GetGameVariable(24);
                                var gamevar25 = playerdat.GetGameVariable(25);
                                var centreX = gamevar24 + 0x30;
                                var centreY = gamevar25 + 0x30;
                                var cornerX = centreX - 1;
                                var cornerY = centreY - 1;
                                var dimX = 2;
                                var dimY = 2;

                                playerdat.SetGameVariable(26, playerdat.GetGameVariable(26) - 1);//decrement attempt counter

                                if (gamevar24 == 0)
                                {
                                    cornerX = centreX;
                                }
                                if ((gamevar24 == 7) || (gamevar24 == 0))
                                {
                                    dimX = 1;
                                }

                                if (gamevar25 == 0)
                                {
                                    cornerY = centreY;
                                }
                                if ((gamevar25 == 7) || (gamevar25 == 0))
                                {
                                    dimY = 1;
                                }

                                TileInfo.ChangeTile(
                                    StartTileX: cornerX, 
                                    StartTileY: cornerY, 
                                    DimX: dimX, 
                                    DimY: dimY, 
                                    HeightAdjustFlag: (mode<<1)+1);

                                TileInfo.ChangeTile(
                                    StartTileX: centreX, 
                                    StartTileY: centreY, 
                                    HeightAdjustFlag: (mode<<1)+1);
                            }

                            break;
                        }
                    case 2: //set Y variable
                        {
                            var tmp = playerdat.GetGameVariable(25);
                            tmp++;
                            tmp = tmp & 0x7;
                            playerdat.SetGameVariable(25, tmp);
                            break;
                        }
                    case 3: // set X variable
                        {
                            var tmp = playerdat.GetGameVariable(24);
                            tmp++;
                            tmp = tmp & 0x7;
                            playerdat.SetGameVariable(24, tmp);
                            break;
                        }
                    case 4://reset tiles
                        {
                            uimanager.AddToMessageScroll(GameStrings.GetString(1, 193));// Reset Activated
                            playerdat.SetGameVariable(26, 0x3F);
                            TileInfo.ChangeTile(
                                StartTileX: 0x30, StartTileY: 0x30,
                                newHeight: 4,
                                DimX: 7, DimY: 7);
                            break;
                        }
                }
            }
        }

    }//end class
}//end namespace
