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
                case 1://
                    {
                        if (_RES == GAME_UW2)
                        {
                            Debug.Print("PORTAL");
                        }
                        else
                        {
                            Debug.Print("DETECT MONSTER");
                        }
                        break;
                    }
                case 2:
                    {
                        if (_RES == GAME_UW2)
                        {
                            Debug.Print("RESTORATION");
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
                            //Debug.Print("LOCATE");//turn automapping back on if player is lost
                            if ((playerdat.AutomapEnabled==false) && (worlds.GetWorldNo(playerdat.dungeon_level)!=8))
                            {
                                playerdat.AutomapEnabled = true;
                                uimanager.AddToMessageScroll(GameStrings.GetString(1,0x12A));//your position is revealed
                            }
                            else
                            {
                                uimanager.AddToMessageScroll(GameStrings.GetString(1,0x142));//no noticeable effect
                            }
                        }
                        else
                        {
                            Debug.Print("Remove Trap");
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
                        //Needs extra logic for UW2 (eg check if in void)
                        PlayerActiveStatusEffectSpells(0xB, 1, stability);//Note the change in mapping here.
                        playerdat.PlayerStatusUpdate();
                        break;
                    }
                case 8://telekenesis
                    {
                        PlayerActiveStatusEffectSpells(0xB, 3, stability);//Note the change in mapping here.
                        playerdat.PlayerStatusUpdate();
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
                        PlayerActiveStatusEffectSpells(0xB, 0, stability);//Note the change in mapping here.
                        playerdat.PlayerStatusUpdate();
                        break;
                    }
                case 0xC:   //armageddon
                    {
                        Debug.Print("ARMAGEDDON--> Everything vanishes");
                        //Clear player inventory.
                        playerdat.ClearInventory();
                        //reset map
                        UWTileMap.ResetMap(0);
                        //clear rune bag
                        for (int r=0; r<24;r++)
                        {
                            playerdat.SetRune(r,false);
                            uimanager.SetRuneInBag(r, false);
                        }
                        //clear selected runes
                        for (int r =0; r<3; r++)
                        {
                            playerdat.SetSelectedRune(r,24);
                        }
                        playerdat.NoOfSelectedRunes = 0;
                        uimanager.RedrawSelectedRuneSlots();
                        //Set weightcarried to 0
                        //playerdat.WeightCarried = 0;

                        //set flag
                        playerdat.armageddon = true;

                        if(_RES!=GAME_UW2)
                        {
                            playerdat.SilverTreeDungeon = 0; //stops player resurrection
                        }

                        //refresh status                        
                        playerdat.PlayerStatusUpdate();                        
                        break;
                    }
                case 0xD://dispel hunger
                    {
                        playerdat.play_hunger = 0xC0;
                        playerdat.maybefoodhealthbonus = 0;
                        uimanager.AddToMessageScroll(GameStrings.GetString(1,0x134));
                        break;
                    }
            }
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
            };
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
        /// <param name="caster"></param>
        public static void CastClassB_SpellsOnCallBack(int minorclass, int index, uwObject[] objList, int caster = 1)
        {
            if (_RES!=GAME_UW2)
            {
                switch (minorclass)
                {
                    case 2://strengthen door.
                        Debug.Print("Strengthen door callback, this spell does nothing in vanilla!");
                        //probably a suitable fix here would be to apply an owner value to the door, or restore the quality to max.
                        break;
                    case 3://remove trap
                        Debug.Print("Remove trap callback");
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