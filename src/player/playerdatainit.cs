using Godot;
using System;
namespace Underworld
{
    public partial class playerdat : Loader
    {
        //loads a player.dat file and initialses ui and cameras.

        /// <summary>
        /// Loads a player.dat file and applies values to interface and game state
        /// </summary>
        /// <param name="datafolder"></param>
        public static void LoadPlayerDat(string datafolder)
        {
            if (datafolder.ToUpper() != "DATA")
            {
                //load player dat from a save file
                Load(datafolder);
                PositionPlayerObject();

                for (int i = 0; i < 8; i++)
                {//Init the backpack indices
                    uimanager.SetBackPackIndex(i, BackPackObject(i));
                }
                RenderingServer.GlobalShaderParameterSet("cutoffdistance", shade.GetViewingDistance(lightlevel));
            }
            else
            {
                //default start locations.                
                switch (_RES)
                {
                    case GAME_UW2:
                        main.gamecam.Position = new Vector3(-23f, 4.3f, 58.2f);
                        break;
                    default:
                        main.gamecam.Position = new Vector3(-38f, 4.2f, 2.2f);
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

            playerObject.npc_xhome = (short)playerdat.tileX;
            playerObject.npc_yhome = (short)playerdat.tileY;

            //load the correct skin tones for weapon animations
            switch (Body)
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


            //Motion params
            motion.playerMotionParams.x_0 = (short)playerdat.X;
            motion.playerMotionParams.y_2 = (short)playerdat.Y;
            motion.playerMotionParams.z_4 = (short)playerdat.Z;

            motion.playerMotionParams.index_20 = 1;
            motion.playerMotionParams.unk_24 = 8;

            motion.playerMotionParams.tilestate25 = (byte)(playerdat.RelatedToMotionState >> 3);

            motion.PlayerHeadingMinor_dseg_8294 = (short)playerdat.heading_full;

            motion.UpdateMotionStateAndSwimming(playerdat.RelatedToMotionState & 0x7);

            //TODO process detail and music/sound options

        }

        /// <summary>
        /// Positions the player game camera based on x/y/z pos and current tileX/Y
        /// Note this does not provide enough precision to fully reflect all the positions the player camera can be  at.
        /// </summary>
        public static void PositionPlayerObject()
        {
            var x_adj = 0f;
            var y_adj = 0f;
            if ((motion.playerMotionParams.x_0 & 0x1F) != 0)
            {
                x_adj = 0.15F / ((float)(motion.playerMotionParams.x_0 & 0x1F));
            }
            if ((motion.playerMotionParams.y_2 & 0x1F) != 0)
            {
                y_adj = 0.15F / ((float)(motion.playerMotionParams.y_2 & 0x1F));
            }
            Vector3 adjust = new Vector3(
                x: x_adj,
                z: y_adj,
                y: 0); //y-up
            main.gamecam.Position = adjust + uwObject.GetCoordinate(
                tileX: playerObject.tileX,
                tileY: playerObject.tileY,
                _xpos: playerObject.xpos,
                _ypos: playerObject.ypos,
                _zpos: playerObject.zpos + commonObjDat.height(127), CentreInGrid: false);
            main.gamecam.Rotation = Vector3.Zero;
            main.gamecam.Rotate(Vector3.Up, (float)(Math.PI));//align to the north.
            main.gamecam.Rotate(Vector3.Up, (float)(-heading_major / 127f * Math.PI));
        }



        /// <summary>
        /// Initialises the data for a new player
        /// </summary>
        /// <param name="new_charname"></param>
        public static void InitEmptyPlayer(string new_charname = "Gronk")
        {
            var InventoryPtr = 0x138;
            if (_RES == GAME_UW2)
            {
                InventoryPtr = 0x3E3;
            }
            pdat = new byte[InventoryPtr + 1];

            Array.Resize(ref pdat, InventoryPtr + 512 * 8);

            Rng.InitRng();

            //Common items
            CharName = new_charname;
            play_level = 1;
            Exp = 0;
            SkillPoints = 1;
            SkillPointsTotal = 0;
            AutomapEnabled = true;
            play_poison = 0;
            ActiveSpellEffectCount = 0;
            SetSelectedRune(0, 24);
            SetSelectedRune(1, 24);
            SetSelectedRune(2, 24);
            NoOfSelectedRunes = 0;
            armageddon = false;
            intoxication = 0;
            shrooms = 0;
            play_fatigue = 0x30;
            for (int r = 0; r < 24; r++)
            {
                SetRune(r, false);
            }
            play_hunger = 0xC0;
            for (int s = 0; s < 0x14; s++)
            {
                SetSkillValue(s, 0);
            }

            dungeon_level = 1;

            Unknown0x3DValue = 0;//unknown purpose. 
            maybefoodhealthbonus = 0;
            //Unimplemented common items
            //Tilestate CLEAR GROUNDED, SWIMING and ON LAVA FLAGS
            //SWIMDAMAGECOUNTER = 0
            //UNK 0xB6 in UW1
            //Weight Carried?


            //Game specific
            if (_RES == GAME_UW2)
            {
                ClockValue = 0x465000;
                SetXClock(0, 0xF);//this gets overwritten later?
                //initialise the two moonstones
                SetMoonstone(1, 3);
                SetMoonstone(1, 45);
                DreamPlantCounter = 0;
                DreamingInVoid = false;
                SetQuest(144, 8);
                SetQuest(145, 0); //guardian dreams
                //clear quests
                for (int q = 0; q <= 15; q++)
                {
                    SetQuest(q, 0);
                }
                for (int q = 128; q <= 142; q++)
                {
                    SetQuest(q, 0);
                }
                for (int v = 0; v <= 128; v++)
                {
                    SetGameVariable(v, 0);
                }
                for (int l = 0; l < 80; l++)
                {
                    SetLevelLore(l, 0);
                }
                for (int x = 0; x <= 15; x++)
                {
                    SetXClock(X, 0);
                }
            }
            else
            {
                ClockValue = 0x10B3000;
                SilverTreeDungeon = 0;
                SetMoonstone(0, 2);
                isOrbDestroyed = false;
                GotCupOfWonder = false;
                GotKeyOfTruth = false;
                CanTalismansBeDestroyed = false;
                IsGaramonBuried = false;
                MazeNavigation = false;
                SetQuest(36, 8); //no of talismans to destroy
                for (int q = 0; q <= 35; q++)
                {
                    SetQuest(q, 0);
                }
                SetQuest(37, 0); //garamon dreams
                for (int v = 0; v <= 63; v++)
                {
                    SetGameVariable(v, 0);
                }
                for (int l = 0; l < 9; l++)
                {
                    SetLevelLore(l, 0);
                }
                SetGameVariable(26, 53);//bullfrog retries
            }
        }

    }//end class
}//end namespace