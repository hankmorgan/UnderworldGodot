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

                //Motion params
                motion.playerMotionParams.x_0 = (short)playerdat.XCoordinate;
                motion.playerMotionParams.y_2 = (short)playerdat.YCoordinate;
                motion.playerMotionParams.z_4 = (short)playerdat.Z;

                motion.playerMotionParams.index_20 = 1;
                motion.playerMotionParams.unk_24 = 8;

                motion.playerMotionParams.tilestate25 = (byte)(playerdat.RelatedToMotionState >> 3);

                motion.PlayerCameraYaw_dseg_8294 = (short)playerdat.heading_full;

                motion.UpdateMotionStateAndSwimming(playerdat.RelatedToMotionState & 0x7);                

                for (int i = 0; i < 8; i++)
                {//Init the backpack indices
                    uimanager.SetBackPackIndex(i, BackPackObject(i));
                }
                RenderingServer.GlobalShaderParameterSet("cutoffdistance", shade.GetViewingDistance(lightlevel));
            }
            else
            {
                //starting a new player
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
                //main.gamecam.Rotate(Vector3.Up, (float)Math.PI);
            }//end standard setup.

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

            // playerObject.npc_xhome = (short)playerdat.tileX;
            // playerObject.npc_yhome = (short)playerdat.tileY;

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
            foodhealthbonus = 0;
            //Unimplemented common items
            //Tilestate CLEAR GROUNDED, SWIMING and ON LAVA FLAGS
            //SWIMDAMAGECOUNTER = 0
            //UNK 0xB6 in UW1
            //Weight Carried?

            //default game options
            MusicEnabled = true;
            SoundEffectsEnabled = true;
            // pdat[0xB6] bits 4-5 = UW1 graphics detail level
            // (0=Low, 1=Medium, 2=High, 3=Very High). DOS UW.EXE chargen
            // sets this to Very High; without it, DOS-loaded port saves
            // render walls/floor/ceiling as untextured flat polygons.
            // Hank's 9969989 added the UW2 offset to the accessor so this
            // single call covers both games.
            DetailLevel = 3;
            if (_RES != GAME_UW2)
            {
                // pdat[0xD3] = ShadeCutOff (shade-table index, per @hankmorgan
                // on PR #33: the global is named ShadeCutOff_dseg_67d6_8622
                // in the UW2 disasm, set by OpenAndApplyShadesDat_ovr142_0;
                // the UW1 equivalent is ovr142_0 at UW1_asm.asm:385926-386218).
                // Indexes shades.dat: 0=Darkness, 1=Burning Match,
                // 2=Candlelight, 3=Light, 4=Magic Lantern, 5=Night Vision,
                // 6=Daylight, 7=Sunlight. SHADES.DAT is exactly 96 bytes
                // (8 entries × 12 bytes); valid range 0..7. DOS chargen
                // default is 3 (Light). The function does NOT bound-check
                // the input — passing 8 reads off EOF; DOS tolerates
                // because subsequent gameplay overwrites the shading params
                // before they're rendered. Without ANY value here (port
                // default 0 = Darkness; functions but DOS Journey-Onward
                // hangs in some load paths) DOS won't load. 3 is the
                // semantically correct chargen value.
                SetAt(0xD3, 0x03);
            }

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
                    SetXClock(x, 0);
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