using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Object enchantment and spells. For spells cast by objects (exclude runic magic)
    /// </summary>
    public class MagicEnchantment : UWClass
    {

        public int SpellMajorClass;
        public int SpellMinorClass;
        public bool IsFlagBit2Set;

        public MagicEnchantment(int _major, int _minor, bool _isflag2set)
        {
            SpellMajorClass = _major;
            SpellMinorClass = _minor;
            IsFlagBit2Set = _isflag2set;
        }

        public static bool IsPotion(uwObject obj)
        {
            if (_RES==GAME_UW2)
            {
                return (obj.item_id>=224 && obj.item_id<=231);
            }
            else
            {
                return false;// this is a uw2 only behaviour.
                //return (obj.item_id>=187 && obj.item_id<=188);
            }
        }

        public string NameEnchantment(uwObject obj, uwObject[] objList, int LoreCheck = 3)
        {
            var stringNo = 0;
            if (IsPotion(obj))
            {
                if (obj.is_quant == 0)
                {//check for poisoned potions
                    var damagetrap = objectsearch.FindMatchInObjectChain(obj.link, 6,0,0,objList);
                    if (damagetrap  != null)
                    {
                        return " of poison";
                    }
                }
            }
            //if not a poisoned potion check for other causes.
            if (LoreCheck == 3)
            {//passed lorecheck
                if (SpellMajorClass == 0xC)
                {
                    stringNo = 0x1C0;
                    if (obj.minorclass >=2)
                    {
                        stringNo+=16;
                    }
                    stringNo+= SpellMinorClass;
                }
                else
                {
                    if (SpellMajorClass==9)
                        {
                            return "";
                        }
                    else
                        {
                            if (IsFlagBit2Set)
                            {
                                if (SpellMajorClass>0)
                                {
                                    stringNo = SpellMinorClass + (SpellMajorClass<<4);
                                }
                                else
                                {
                                    stringNo = 0x100 + SpellMinorClass;
                                }
                                
                            }
                            else
                            {
                                stringNo = SpellMajorClass + (SpellMajorClass<<4);
                            }                            
                        }
                }
                Debug.Print($"{SpellMajorClass},{SpellMinorClass}->{stringNo}");
                return GameStrings.GetString(6,stringNo);
            }
            else
            {
                //failed lore check
                return "";
            }

        }

        public static MagicEnchantment GetSpellEnchantment(uwObject obj, uwObject[] objList)
        {
            if (obj.majorclass == 6)
            {
                return null;
            }
            else
            {                
                if ((obj.is_quant == 1) || (obj.is_quant == 0 && obj.link == 0))
                {
                    if (obj.is_quant == 0)
                    {
                        //check obj itself.
                        obj = null; //no match
                    }
                    else
                    {
                        if (obj.enchantment == 0)
                            {
                                obj = null; //no match
                            }
                        else
                            {//has enchantment set
                                if (obj.majorclass == 5)
                                {
                                    obj = null;
                                }
                                else
                                {
                                    //obj is the object to test.
                                }
                            }
                    }
                }
                else
                {//try and find a linked spell
                    obj = objectsearch.FindMatchInObjectChain(
                        ListHeadIndex: obj.link, 
                        majorclass: 4, 
                        minorclass: 2, 
                        classindex: 0, 
                        objList: objList);
                }

                if (obj!=null)
                {
                    var major =-1; var minor=-1; 
                    bool flag2;
                    if (obj.flags2 ==1)
                    {
                        flag2 = true;
                        major = (obj.link & 0x1FF)>>6;
                        if (major == 0)
                        {
                            major = -1;
                        }
                        else
                        {
                            major += 0xC;
                        }
                        minor = obj.link & 0x3F;
                    }
                    else
                    {
                        flag2 = false;
                        major = (obj.link & 0x1FF)>>4; //note this >>4 is different from the above >> 6
                        minor = obj.link & 0xF;
                    }

                    return new MagicEnchantment(
                        _major: major,
                        _minor: minor, 
                        _isflag2set: flag2);
                }
                else
                {
                    return null;
                }
            }
        }       
    }//end class
}//end namespace