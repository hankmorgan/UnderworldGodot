using System.Diagnostics;

namespace Underworld
{
    public partial class SpellCasting : UWClass
    {

        /// <summary>
        /// Misc active spells and others
        /// </summary>
        /// <param name="minorclass"></param>
        /// <param name="stability"></param>
        public static void CastClassB_Spells(int minorclass, int stability)
        {
            switch (minorclass)
            {
                case 0: //speed
                    {
                        PlayerActiveStatusEffectSpells(0xB, 2, stability);//Note the change in mapping here.
                        playerdat.PlayerStatusUpdate();
                        break;
                    }
                case 1://Portal
                    {
                        if (_RES == GAME_UW2)
                        {
                            Portal();
                        }
                        else
                        {
                            //DETECT MONSTERS
                            tracking.DetectMonsters(8, 0x2D);
                        }
                        break;
                    }
                case 2:
                    {
                        if (_RES == GAME_UW2)
                        {
                            //Debug.Print("RESTORATION");
                            Restoration();
                        }
                        else
                        {
                            Debug.Print("Strengthen door->Note this spell appears to be bugged and does nothing???");
                        }
                        break;
                    }

                case 3:
                    {
                        if (_RES == GAME_UW2)
                        {
                            Locate();
                        }
                        else
                        {
                            Debug.Print("Remove Trap UW1 untested. Since I can't find any instances of traps in UW1!");
                            currentSpell = new RunicMagic(11, minorclass);
                            uimanager.instance.mousecursor.SetCursorToCursor(10);
                            break;
                        }
                        break;
                    }

                case 4:
                    {
                        if (_RES != GAME_UW2)
                        {//no uw2 spells here, Name Enchantment                        
                            currentSpell = new RunicMagic(11, minorclass);
                            uimanager.instance.mousecursor.SetCursorToCursor(10);
                        }
                        break;
                    }
                case 5:
                    {
                        if (_RES != GAME_UW2)
                        {//no uw2 spells here, Unlock/Open spell                        
                            currentSpell = new RunicMagic(11, minorclass);
                            uimanager.instance.mousecursor.SetCursorToCursor(10);
                        }
                        break;
                    }
                case 6:
                    {//Cure poison
                        playerdat.play_poison = 0;
                        break;
                    }
                case 7:
                    {
                        //roaming sight
                        RoamingSight(stability);
                        break;
                    }
                case 8://telekenesis
                    {
                        Telekinesis(stability);
                        break;
                    }
                case 9://tremor
                    {
                        Debug.Print("!!!!EARTHQUAKKKKEEE!!!!");
                        break;
                    }
                case 0xA://gate travel (UW1)
                    {
                        if (_RES != GAME_UW2)
                        {
                            GateTravelUW1();
                        }
                        break;
                    }
                case 0xB://Freeze time
                    {
                        FreezeTime(stability);
                        break;
                    }
                case 0xC:   //armageddon
                    {
                        Armageddon();
                        break;
                    }
                case 0xD://dispel hunger
                    {
                        DispelHunger();
                        break;
                    }
            }
        }

        private static void Locate()
        {
            //Debug.Print("LOCATE");//turn automapping back on if player is lost
            if ((playerdat.AutomapEnabled == false) && (worlds.GetWorldNo(playerdat.dungeon_level) != 8))
            {
                playerdat.AutomapEnabled = true;
                uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x12A));//your position is revealed
            }
            else
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x142));//no noticeable effect
            }
        }


        private static void RoamingSight(int stability)
        {
            //Needs extra logic for UW2 (eg check if in void)
            PlayerActiveStatusEffectSpells(0xB, 1, stability);//Note the change in mapping here.
            playerdat.PlayerStatusUpdate();
        }


        private static void Telekinesis(int stability)
        {
            PlayerActiveStatusEffectSpells(0xB, 3, stability);//Note the change in mapping here.
            playerdat.PlayerStatusUpdate();
        }


        private static void FreezeTime(int stability)
        {
            PlayerActiveStatusEffectSpells(0xB, 0, stability);//Note the change in mapping here.
            playerdat.PlayerStatusUpdate();
        }


        private static void Armageddon()
        {
            Debug.Print("ARMAGEDDON--> Everything vanishes");
            //Clear player inventory.
            playerdat.ClearInventory();
            //reset map
            UWTileMap.ResetMap(0);
            //clear rune bag
            for (int r = 0; r < 24; r++)
            {
                playerdat.SetRune(r, false);
                uimanager.SetRuneInBag(r, false);
            }
            //clear selected runes
            for (int r = 0; r < 3; r++)
            {
                playerdat.SetSelectedRune(r, 24);
            }
            playerdat.NoOfSelectedRunes = 0;
            uimanager.RedrawSelectedRuneSlots();
            //Set weightcarried to 0
            //playerdat.WeightCarried = 0;

            //set flag
            playerdat.armageddon = true;

            if (_RES != GAME_UW2)
            {
                playerdat.SilverTreeDungeon = 0; //stops player resurrection
            }

            //refresh status                        
            playerdat.PlayerStatusUpdate();
        }


        private static void DispelHunger()
        {
            playerdat.play_hunger = 0xC0;
            playerdat.maybefoodhealthbonus = 0;
            uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x134));
        }


        private static void Restoration()
        {
            playerdat.play_poison = 0;
            playerdat.shrooms = 0;
            if (playerdat.ParalyseTimer > 0)
            {
                playerdat.ParalyseTimer = 0;
                main.gamecam.Set("MOVE", true);
            }
            playerdat.intoxication = 0;
            playerdat.play_hp = playerdat.max_hp;
            playerdat.play_hunger = 0xFF;
            playerdat.play_fatigue = 0;
            playerdat.Unknown0x3DValue = 0;
            playerdat.PlayerStatusUpdate();

            uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x133));

        }

        /// <summary>
        /// Teleports the player to a tile 2 units away from them.
        /// </summary>
        private static void Portal()
        {
            var distance = 2;//distance being 2 here is probably a vanilla bug. seems like the expected behavour is the game would test 2 then 1 and then 0.
            while (distance <= 2)
            {
                int x0 = playerdat.tileX_depreciated; int y0 = playerdat.tileY_depreciated;
                motion.GetCoordinateInDirection((playerdat.playerObject.heading << 5) + playerdat.playerObject.npc_heading, distance, ref x0, ref y0);
                var tile = UWTileMap.current_tilemap.Tiles[x0, y0];

                if (tile.floorHeight - (playerdat.playerObject.zpos << 3) <= 2)
                {
                    if (motion.TestIfObjectFitsInTile(playerdat.playerObject.item_id, 1, 4 + (x0 << 3), 4 + (y0 << 3), playerdat.playerObject.zpos, 1, 8))
                    {
                        Teleportation.Teleport(0, tile.tileX, tile.tileY, 0, playerdat.heading_major);
                        return;
                    }
                }
                distance++;
            }
            uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x132));//there is not enough space
        }


        /// <summary>
        /// Teleports to the Moonstone in UW1.
        /// </summary>
        private static void GateTravelUW1()
        {
            if (playerdat.GetMoonstone(0) == 0)
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x111));//moonstone is unavailable.
            }
            else
            {
                Teleportation.CodeToRunOnTeleport = Teleportation.JumpToMoonStoneOnLevel;
                Teleportation.Teleport(
                    character: 0,
                    tileX: 32, tileY: 32,
                    newLevel: playerdat.GetMoonstone(0),
                    heading: 0);
            }
            ;
        }


        /// <summary>
        /// Note these enchantments don't line up exactly with the class B spell cast list. This is the correct behaviour
        /// </summary>
        /// <param name="minorclass"></param>
        public static void CastClassBEnchantment(int minorclass)
        {
            switch (minorclass)
            {
                case 0://freezetime
                    playerdat.FreezeTimeEnchantment = true; break;
                case 1://roaming sight
                    playerdat.RoamingSightEnchantment = true; break;
                case 2://speed
                    playerdat.SpeedEnchantment = true; break;
                case 3://telekenesis
                    playerdat.TelekenesisEnchantment = true; break;
                case 0xE://health regen
                    playerdat.HealthRegenEnchantment = true; break;
                case 0xF://Mana regen       
                    playerdat.ManaRegenEnchantment = true; break;
            }
        }


        /// <summary>
        /// Special case for UW1 spells that are class B but have callbacks
        /// </summary>
        /// <param name="minorclass"></param>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        public static void CastClassB_SpellsOnCallBack(int minorclass, int index, uwObject[] objList)
        {
            if (_RES != GAME_UW2)
            {
                switch (minorclass)
                {
                    case 2://strengthen door.
                        Debug.Print("Strengthen door callback, this spell does nothing in vanilla!");
                        //probably a suitable fix here would be to apply an owner value to the door, or restore the quality to max.
                        break;
                    case 3://remove trap
                        trapdisarming.TrapDisarmSpell(index, objList);
                        break;
                    case 4://Name enchantment
                        NameEnchantment(index, objList);
                        break;
                    case 5://unlock
                        Unlock(index, objList);
                        break;
                }
            }
            else
            {
                //not in uw2/
            }
        }
    }//end class
}//end namespace