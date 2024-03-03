namespace Underworld
{

    /// <summary>
    /// Runic magic spell lookup tables and logic. 
    /// For spells cast directly by the player.
    /// </summary>
    public class RunicMagic : UWClass
    {
        public static RunicMagic[] SpellList;

        public int SpellIndex;
        public int RuneSequence;
        public int SpellMajorClass;
        public int SpellMinorClass;

        public RunicMagic(int _index, int _major, int _minor, int _RuneSequence)
        {
            SpellIndex = _index;
            SpellMajorClass = _major;
            SpellMinorClass = _minor;
            RuneSequence = _RuneSequence;
        }

        public string spellname
        {
            get
            {
                return GameStrings.GetString(6, 256 + SpellIndex);
            }
        }

        public int SpellLevel
        {
            get
            {
                var div = 6; 
                if (_RES==GAME_UW2){div=8;}
                
                var result = 1 + (SpellIndex / div);
                if (result > 8)
                {
                    return 1;
                }
                else
                {
                    return result;
                }
            }
        }

        public int ManaCost
        {
            get
            {
                return SpellLevel * 3;
            }
        }
        public static RunicMagic CurrentSpell()
        {
            var searchrunes = (playerdat.GetSelectedRune(0) << 10)
                                + (playerdat.GetSelectedRune(1) << 5)
                                + playerdat.GetSelectedRune(2);

            foreach (var spell in SpellList)
            {
                if (spell != null)
                {
                    if ((spell.RuneSequence == searchrunes) && (spell.RuneSequence != 25368))
                    {
                        return spell;
                    }
                }
            }
            return null;// no spell.
        }
        static RunicMagic()
        {
            switch (_RES)
            {
                case GAME_UW2:
                    SpellList = new RunicMagic[69];
                    SpellList[0] = new(0, 8, 1, 8599);
                    SpellList[1] = new(1, 3, 1, 1480);
                    SpellList[2] = new(2, 5, 1, 14648);
                    SpellList[3] = new(3, 2, 2, 1298);
                    SpellList[4] = new(4, 7, 13, 22840);
                    SpellList[5] = new(5, 0, 131, 8568);
                    SpellList[6] = new(6, 1, 6, 20591);
                    SpellList[7] = new(7, 11, 3, 1732);
                    SpellList[8] = new(8, 7, 1, 16472);
                    SpellList[9] = new(9, 3, 10, 16386);
                    SpellList[10] = new(10, 5, 5, 15049);
                    SpellList[11] = new(11, 4, 2, 8236);
                    SpellList[12] = new(12, 8, 2, 8361);
                    SpellList[13] = new(13, 1, 2, 17519);
                    SpellList[14] = new(14, 1, 1, 20984);
                    SpellList[15] = new(15, 11, 13, 16791);
                    SpellList[16] = new(16, 7, 0, 9624);
                    SpellList[17] = new(17, 11, 6, 440);
                    SpellList[18] = new(18, 7, 8, 302);
                    SpellList[19] = new(19, 5, 2, 14552);
                    SpellList[20] = new(20, 0, 133, 16760);
                    SpellList[21] = new(21, 6, 133, 322);
                    SpellList[22] = new(22, 11, 0, 18031);
                    SpellList[23] = new(23, 1, 68, 24056);
                    SpellList[24] = new(24, 6, 135, 8197);
                    SpellList[25] = new(25, 4, 4, 8600);
                    SpellList[26] = new(26, 3, 11, 13623);
                    SpellList[27] = new(27, 7, 10, 312);
                    SpellList[28] = new(28, 3, 70, 18616);
                    SpellList[29] = new(29, 2, 67, 8792);
                    SpellList[30] = new(30, 7, 7, 22936);
                    SpellList[31] = new(31, 3, 5, 6735);
                    SpellList[32] = new(32, 1, 3, 20719);
                    SpellList[33] = new(33, 5, 3, 15544);
                    SpellList[34] = new(34, 7, 11, 15063);
                    SpellList[35] = new(35, 7, 12, 4856);
                    SpellList[36] = new(36, 7, 2, 76);
                    SpellList[37] = new(37, 8, 3, 8809);
                    SpellList[38] = new(38, 7, 9, 18007);
                    SpellList[39] = new(39, 11, 8, 14839);
                    SpellList[40] = new(40, 7, 3, 16950);
                    SpellList[41] = new(41, 6, 66, 21958);
                    SpellList[42] = new(42, 0, 134, 21771);
                    SpellList[43] = new(43, 7, 15, 22063);
                    SpellList[44] = new(44, 4, 15, 21772);
                    SpellList[45] = new(45, 3, 68, 22091);
                    SpellList[46] = new(46, 13, 7, 22680);
                    SpellList[47] = new(47, 7, 5, 143);
                    SpellList[48] = new(48, 6, 131, 21526);
                    SpellList[49] = new(49, 7, 14, 21975);
                    SpellList[50] = new(50, 6, 129, 14352);
                    SpellList[51] = new(51, 6, 134, 9464);
                    SpellList[52] = new(52, 8, 5, 10252);
                    SpellList[53] = new(53, 11, 9, 22007);
                    SpellList[54] = new(54, 11, 1, 21655);
                    SpellList[55] = new(55, 8, 6, 14822);
                    SpellList[56] = new(56, 6, 132, 5368);
                    SpellList[57] = new(57, 1, 5, 21743);
                    SpellList[58] = new(58, 11, 11, 632);
                    SpellList[59] = new(59, 2, 69, 8882);
                    SpellList[60] = new(60, 11, 2, 21912);
                    SpellList[61] = new(61, 11, 7, 14838);
                    SpellList[62] = new(62, 7, 6, 21804);
                    SpellList[63] = new(63, 11, 12, 21826);
                    SpellList[64] = new(64, 6, 5, 25368);
                    SpellList[65] = new(65, 5, 4, 25368);
                    SpellList[66] = new(66, 11, 13, 25368);
                    SpellList[67] = new(67, 10, 3, 25368);
                    SpellList[68] = new(68, 10, 9, 25368);

                    break;
                default:
                    SpellList = new RunicMagic[53];
                    SpellList[0] = new(0, 0, 131, 8568);
                    SpellList[1] = new(1, 2, 2, 1298);
                    SpellList[2] = new(2, 5, 1, 14648);
                    SpellList[3] = new(3, 8, 1, 8599);
                    SpellList[4] = new(4, 3, 2, 18680);
                    SpellList[5] = new(5, 1, 1, 20984);
                    SpellList[6] = new(6, 3, 1, 600);
                    SpellList[7] = new(7, 1, 2, 17519);
                    SpellList[8] = new(8, 4, 2, 8236);
                    SpellList[9] = new(9, 11, 1, 22936);
                    SpellList[10] = new(10, 7, 1, 16472);
                    SpellList[11] = new(11, 8, 3, 8504);
                    SpellList[12] = new(12, 11, 0, 18031);
                    SpellList[13] = new(13, 3, 3, 1611);
                    SpellList[14] = new(14, 0, 133, 16760);
                    SpellList[15] = new(15, 5, 2, 14552);
                    SpellList[16] = new(16, 11, 2, 18744);
                    SpellList[17] = new(17, 2, 67, 8792);
                    SpellList[18] = new(18, 1, 68, 24056);
                    SpellList[19] = new(19, 4, 4, 8600);
                    SpellList[20] = new(20, 1, 3, 7672);
                    SpellList[21] = new(21, 7, 4, 13720);
                    SpellList[22] = new(22, 3, 70, 18616);
                    SpellList[23] = new(23, 11, 3, 312);
                    SpellList[24] = new(24, 5, 3, 15544);
                    SpellList[25] = new(25, 7, 2, 76);
                    SpellList[26] = new(26, 11, 4, 15063);
                    SpellList[27] = new(27, 3, 5, 6735);
                    SpellList[28] = new(28, 11, 5, 4856);
                    SpellList[29] = new(29, 11, 6, 440);
                    SpellList[30] = new(30, 4, 15, 21772);
                    SpellList[31] = new(31, 6, 66, 21958);
                    SpellList[32] = new(32, 11, 10, 22063);
                    SpellList[33] = new(33, 7, 5, 143);
                    SpellList[34] = new(34, 0, 134, 21771);
                    SpellList[35] = new(35, 11, 8, 14839);
                    SpellList[36] = new(36, 1, 5, 21743);
                    SpellList[37] = new(37, 7, 3, 8593);
                    SpellList[38] = new(38, 8, 4, 10648);
                    SpellList[39] = new(39, 3, 68, 22091);
                    SpellList[40] = new(40, 6, 3, 21526);
                    SpellList[41] = new(41, 6, 129, 14352);
                    SpellList[42] = new(42, 2, 69, 8882);
                    SpellList[43] = new(43, 11, 9, 22007);
                    SpellList[44] = new(44, 11, 7, 14838);
                    SpellList[45] = new(45, 6, 68, 5368);
                    SpellList[46] = new(46, 11, 11, 632);
                    SpellList[47] = new(47, 11, 12, 21826);
                    SpellList[48] = new(48, 6, 5, 25368);
                    SpellList[49] = new(49, 5, 4, 25368);
                    SpellList[50] = new(50, 11, 13, 25368);
                    SpellList[51] = new(51, 10, 3, 25368);
                    SpellList[52] = new(52, 10, 9, 25368);
                    break;
            }
        }


        public static void CastRunicSpell()
        {
            var spell = RunicMagic.CurrentSpell();
            if (spell != null)
            {
                if ((spell.TestIfPlayerCanCastSpell()) | (true))//force this to be true for test and development
                {
                    //apply mana cost
                    playerdat.play_mana = System.Math.Max(0, playerdat.play_mana - spell.ManaCost);

                    uimanager.AddToMessageScroll($"DEBUG PRINT {spell.spellname}");
                    //do the skill check
                    var chkresult = playerdat.SkillCheck(playerdat.Casting, spell.SpellLevel * 3);
                    switch (chkresult)
                    {
                        case playerdat.SkillCheckResult.CritFail:
                            {
                                //In a crit fail the spell class turns into a curse and subclass into spell level/2
                                SpellCasting.CastSpell(
                                    majorclass: 9,
                                    minorclass: spell.SpellLevel / 2,
                                    caster: null, target: null,
                                    tileX: playerdat.tileX, tileY: playerdat.tileY,
                                    CastOnEquip: false,
                                    PlayerCast: true);
                                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_the_spell_backfires_));
                                break;
                            }
                        case playerdat.SkillCheckResult.Fail:
                            {
                                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_the_incantation_failed_));
                                break;
                            }
                        default:
                            SpellCasting.CastSpell(
                                majorclass: spell.SpellMajorClass,
                                minorclass: spell.SpellMinorClass,
                                caster: null, target: null,
                                tileX: playerdat.tileX, tileY: playerdat.tileY,
                                CastOnEquip:false,
                                PlayerCast: true);
                            break;
                    }
                }
            }
            else
            {
                uimanager.AddToMessageScroll("Not a spell.");
            }
        }

        /// <summary>
        /// Checks Mana,level and plot requirements to cast a spell.
        /// </summary>
        /// <returns></returns>
        public bool TestIfPlayerCanCastSpell()
        {
            if (_RES == GAME_UW2)
            {
                if (worlds.GetWorldNo(playerdat.dungeon_level) == 0)
                {//check if spell is allowed in britannia
                    if (SpellLevel >= 3)
                    {
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_the_incantation_failed_));//incantation failed
                        return false;
                    }
                }
            }
            if (((playerdat.play_level + 1) / 2) < SpellLevel)
            {
                if (_RES == GAME_UW2)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, 225)); //you are not experienced enough
                }
                else
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, 210));
                }
                return false;
            }
            else
            {
                if (playerdat.play_mana < ManaCost)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_do_not_have_enough_mana_to_cast_the_spell_));
                    return false;
                }
            }
            return true;
        }

    }//end class
}//end namespace