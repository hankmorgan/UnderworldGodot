using Godot;
using System;
namespace Underworld
{
    public partial class playerdat : Loader
    {
        //loads a player.dat file and initialses ui and cameras.

        public static void LoadPlayerDat(string datafolder)
        {
            if (datafolder.ToUpper() != "DATA")
            {
                //load player dat from a save file
                Load(datafolder);
                //Debug.Print($"You are at x:{X} y:{Y} z:{Z}");
                //Debug.Print($"You are at x:{tileX} {xpos} y:{tileY} {ypos} z:{zpos}");
                main.gamecam.Position = uwObject.GetCoordinate(tileX, tileY, xpos, ypos, camerazpos);
                main.gamecam.Rotation = Vector3.Zero;
                main.gamecam.Rotate(Vector3.Up, (float)(Math.PI));//align to the north.
                main.gamecam.Rotate(Vector3.Up, (float)(-heading / 127f * Math.PI));

                for (int i = 0; i < 8; i++)
                {//Init the backpack indices
                    uimanager.SetBackPackIndex(i, BackPackObject(i));
                }
                RenderingServer.GlobalShaderParameterSet("cutoffdistance", shade.GetViewingDistance(lightlevel));
                main.DrawPlayerPositionSprite(ObjectCreator.grObjects);
            }
            else
            {

                //Random r = new Random();
                InitEmptyPlayer();
                playerdat.STR = Rng.r.Next(10,31);
                playerdat.INT = Rng.r.Next(10,31);
                playerdat.DEX = Rng.r.Next(10,31);
                for (int s = 0; s<20;s++)
                {
                    playerdat.SetSkillValue(s, Rng.r.Next(1,12));
                }
                max_hp = 60;
                play_hp = 60;
                max_hp = 60;
                play_hp = 60;
                tileX = -(int)(main.gamecam.Position.X / 1.2f);
                tileY = (int)(main.gamecam.Position.Z / 1.2f);
                dungeon_level = uwsettings.instance.level + 1;
                play_level = 1;

                var isfemale = Rng.r.Next(0, 2) == 1;
                isFemale = isfemale;
                uimanager.SetHelm(isfemale, -1);
                uimanager.SetArmour(isfemale, -1);
                uimanager.SetBoots(isfemale, -1);
                uimanager.SetLeggings(isfemale, -1);
                uimanager.SetGloves(isfemale, -1);
                uimanager.SetRightShoulder(-1);
                uimanager.SetLeftShoulder(-1);
                uimanager.SetRightHand(-1);
                uimanager.SetLeftHand(-1);
                for (int i = 0; i < 8; i++)
                {
                    uimanager.SetBackPackArt(i, -1);
                }
                Body = Rng.r.Next(0, 4);
                CharName = "GRONK";
                playerdat.SetSelectedRune(0,24);playerdat.SetSelectedRune(1,24);playerdat.SetSelectedRune(2,24);

                switch (_RES)
                {
                    case GAME_UW2:
                        main.gamecam.Position = new Vector3(-23f, 4.3f, 58.2f);

                        break;
                    default:
                        main.gamecam.Position = new Vector3(-38f, 4.2f, 2.2f);
                        //cam.Position = new Vector3(-14.9f, 0.78f, 5.3f);
                        break;
                }
                main.gamecam.Rotate(Vector3.Up, (float)Math.PI);
            }

            //CharNameStringNo = GameStrings.AddString(0x125, CharName);

            //Load bablglobals
            bglobal.LoadGlobals(datafolder);

            //Draw UI
            uimanager.SetBody(Body, isFemale);
            uimanager.RedrawSelectedRuneSlots();
            uimanager.RefreshHealthFlask();
            uimanager.RefreshManaFlask();
            //set paperdoll
            uimanager.UpdateInventoryDisplay();
            uimanager.ConversationText.Text = "";
            //Load rune slots
            for (int i = 0; i < 24; i++)
            {
                uimanager.SetRuneInBag(i, GetRune(i));
            }
            
            SpellCasting.currentSpell = null;
            useon.CurrentItemBeingUsed = null;
            playerdat.usingpole = false;
            musicalinstrument.PlayingInstrument = false;

            //Set the playerlight level;            
            //uwsettings.instance.lightlevel = light.BrightestLight();
        }
    }//end class
}//end namespace