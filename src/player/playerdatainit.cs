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
                InitPlayerObject();
                main.gamecam.Position = uwObject.GetCoordinate(
                    tileX: tileX, 
                    tileY: tileY, 
                    _xpos: xpos, 
                    _ypos: ypos, 
                    _zpos: camerazpos);
                main.gamecam.Rotation = Vector3.Zero;
                main.gamecam.Rotate(Vector3.Up, (float)(Math.PI));//align to the north.
                main.gamecam.Rotate(Vector3.Up, (float)(-heading / 127f * Math.PI));

                for (int i = 0; i < 8; i++)
                {//Init the backpack indices
                    uimanager.SetBackPackIndex(i, BackPackObject(i));
                }
                RenderingServer.GlobalShaderParameterSet("cutoffdistance", shade.GetViewingDistance(lightlevel));
            }
            else
            {                
                play_hunger = 60;
                dungeon_level = uwsettings.instance.level + 1;
                play_level = 1;

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
                tileX = -(int)(main.gamecam.Position.X / 1.2f);
                tileY = (int)(main.gamecam.Position.Z / 1.2f);

                AutomapEnabled = true;
                uimanager.SetHelm(isFemale, -1);
                uimanager.SetArmour(isFemale, -1);
                uimanager.SetBoots(isFemale, -1);
                uimanager.SetLeggings(isFemale, -1);
                uimanager.SetGloves(isFemale, -1);
                uimanager.SetRightShoulder(-1);
                uimanager.SetLeftShoulder(-1);
                uimanager.SetRightHand(-1);
                uimanager.SetLeftHand(-1);
                for (int i = 0; i < 8; i++)
                {
                    uimanager.SetBackPackArt(i, -1);
                }
                SetSelectedRune(0,24); SetSelectedRune(1,24); SetSelectedRune(2,24);
                main.gamecam.Rotate(Vector3.Up, (float)Math.PI);
            }

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
            //remove opened container
            uimanager.OpenedContainerIndex = -1;
            uimanager.EnableDisable(uimanager.instance.OpenedContainer, false);
            
            //Reset some state globals
            SpellCasting.currentSpell = null;
            useon.CurrentItemBeingUsed = null;
            usingpole = false;
            musicalinstrument.PlayingInstrument = false;
            previousMazeNavigation = false;
            Teleportation.CodeToRunOnTeleport = null;
            pitsofcarnage.IsAvatarInPitFightGlobal = false;

            //load the correct skin tones for weapon animations
            switch(Body)
            {
                case 0:
                case 2:
                case 3:
                case 4:
                    uimanager.grWeapon = new WeaponsLoader(0); break;
                default:
                    uimanager.grWeapon = new WeaponsLoader(1); break;
            }

            //Clear cached UW2 SCD data.
            if (_RES == GAME_UW2)
            {
                scd.scd_data = null;
            }
        }
    }//end class
}//end namespace