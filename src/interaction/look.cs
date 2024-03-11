namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the look verb
    /// </summary>
    public class look : UWClass
    {
        public static int LoreCheck(uwObject obj)
        {            
            if (
                (obj.majorclass!=5)
                &&
                (obj.majorclass!=6)
                &&
                (commonObjDat.rendertype(obj.item_id) != 2)
                )
                {//can be identified
                    if ((obj.heading & 0x4)==0)
                    {//no attempt has been made yet. try and id now
                        var result = (int)playerdat.SkillCheck(playerdat.Lore,8);
                        result++;
                        if (result==0)
                        {
                            result = 1;
                        }
                        obj.heading = (short)(4 | result); //store result and flag that attempt was made.
                        return result; //1,2 or 3
                    }
                    else
                    {
                        return obj.heading & 0x3; //return previous result
                    }
                }
            return 1;//fail or cannot be identified
        }

        public static bool LookAt(int index, uwObject[] objList, bool WorldObject)
        {

            bool result = false;
            trap.ObjectThatStartedChain = index;
            if (index <= objList.GetUpperBound(0))
            {
                var obj = objList[index];
                switch (obj.majorclass)
                {
                    case 1:
                        {
                            result = npc.LookAt(obj);
                            break;
                        }
                    case 2:
                        {
                            result = LookMajorClass2(obj, objList);
                            break;
                        }
                    case 4:
                        {
                            result = LookMajorClass4(obj, objList, WorldObject);
                            break;
                        }
                    case 5:
                        {
                            result = LookMajorClass5(obj, objList);
                            break;
                        }

                }
                if ((obj.is_quant == 0) && (obj.link != 0))
                {
                    var linkedObj = objList[obj.link];
                    if (linkedObj.item_id == 419)
                    {
                        trigger.LookTrigger(
                            srcObject: obj,
                            triggerIndex: obj.link,
                            objList: objList);
                        return true;
                    }
                }
                if (!result)
                {
                
                    //default string  when no overriding action has occured           
                    //uimanager.AddToMessageScroll(GameStrings.GetObjectNounUW(obj.item_id));
                    GeneralLookDescription(
                        obj: obj,
                        objList: objList, 
                        lorecheckresult: LoreCheck(obj));
                }
            }

            return false;
        }

        public static bool LookMajorClass2(uwObject obj, uwObject[] objList)
        {
            // switch (obj.minorclass)
            // {
            //     case 3:
            //         return food.LookAt(obj);
            // }
            return false;
        }

        public static bool LookMajorClass4(uwObject obj, uwObject[] objList, bool WorldObject)
        {
            switch (obj.minorclass)
            {
                case 0: //keys up to 0xE
                    {
                        if (obj.classindex <= 0xE)
                        {//TODO LOCKPICK is in the middle of all these
                            return doorkey.LookAt(obj, WorldObject);
                        }
                        break;
                    }
                case 3: //readables (up to index 8)
                    {
                        if (_RES != GAME_UW2)
                        {
                            switch (obj.classindex)
                            {
                                case 0xB:
                                    return false;
                                default:
                                    return Readable.LookAt(obj, objList);
                            }
                        }
                        else
                        {//uw2
                            switch (obj.classindex)
                            {
                                case 0x9://a_bit of a map                                   
                                case 0xA://a_map                   
                                case 0xB://a_dead plant 
                                case 0xC://a_dead plant  
                                case 0xD://a_bottle 
                                case 0xE://a_stick 
                                case 0xF://a_resilient sphere 
                                    return false;
                                default:
                                    return Readable.LookAt(obj, objList);
                            }
                        }
                    }
            }
            return false;
        }

        public static bool LookMajorClass5(uwObject obj, uwObject[] objList)
        {
            switch (obj.minorclass)
            {
                case 2: //misc objects including readables
                    {
                        switch (obj.classindex)
                        {
                            case 5:
                                return gravestone.Use(obj);
                            case 6: // a readable sign.
                                return writing.LookAt(obj);
                            case 0xE:
                            case 0xF:
                                return tmap.LookAt(obj);
                            default:
                                return false;
                        }
                    }

            }

            return false;
        }

        /// <summary>
        /// A look description based on the item id
        /// </summary>
        /// <param name="item_id"></param>
        /// <param name="qty"></param>
        /// <returns></returns>
        public static bool GenericLookDescription(int item_id, int qty = 1)
        {
            string output;
            if (commonObjDat.PrintableLook(item_id))
            {
                output = "You see ";
            }
            else
            {
                System.Diagnostics.Debug.Print("No print description");
                return true;
            }
            string objectname = GameStrings.GetObjectNounUW(item_id, qty);
            var article = GetArticle(objectname);
            output += $"{article}{objectname}";
            uimanager.AddToMessageScroll($"{output}");
            return true;
        }

        public static bool GeneralLookDescription(uwObject obj, uwObject[] objList, int lorecheckresult, bool OutputConvo = false)
        {
            string output;
            if (commonObjDat.PrintableLook(obj.item_id))
            {
                output = GameStrings.GetString(1, GameStrings.str_you_see_);
            }
            else
            {
                System.Diagnostics.Debug.Print("No print description");
                return true;
            }

            var qualityclass = commonObjDat.qualityclass(obj.item_id);
            int qty = 0;
            if (obj.is_quant == 1)
            {
                if (obj.link < 512)
                {
                    qty = obj.link;
                }
            }

            string objectname = GameStrings.GetObjectNounUW(obj.item_id, qty);

            var finalclass = -1;
            if (obj.quality > 0)
            {
                if (obj.quality == 1)
                {
                    var isLight = (obj.item_id & 0x1F8);
                    if (isLight != 0x90)
                    {
                        finalclass = 1 + (obj.quality >> 4);
                    }
                }
                else
                {
                    if (qualityclass == 3)
                    {
                        finalclass = 5;
                    }
                    else
                    {
                        finalclass = 1 + (obj.quality >> 4);
                    }
                }
            }

            var qualitystringid = commonObjDat.qualitytype(obj.item_id) * 6 + finalclass;
            var qualitystring = GameStrings.GetString(5, qualitystringid);
            if (qualitystring.Length > 0) { qualitystring += " "; }

            var article = "a ";

            string qtystring = "";
            if (qty > 1)
            {
                qtystring = $"{qty} ";
            }

            if (qty >= 2)
            {
                article = ""; //GetArticle(qtystring);
            }
            else
            {
                if (qualitystring.Length > 0)
                {
                    article = GetArticle(qualitystring);
                }
                else
                {
                    article = GetArticle(objectname);
                }
            }
            //enchantments
            var magicenchantment = MagicEnchantment.GetSpellEnchantment(obj, objList);
            string enchantmenttext = "";
            string magical = "";
            if (magicenchantment != null)
            {
                System.Diagnostics.Debug.Print($"{magicenchantment.NameEnchantment(obj, objList)}");
                switch (lorecheckresult)
                {
                    case 2://just magical
                        magical = "magical "; break;
                    case 3: // full description
                        enchantmenttext = magicenchantment.NameEnchantment(obj, objList);
                        if (enchantmenttext == "")
                        {
                            enchantmenttext = " of unnamed";
                        }
                        else
                        {
                            enchantmenttext = $" of {enchantmenttext}";
                        }
                        break;
                }
            }

            if (objectname.StartsWith("some "))
            {
                output += $"{qtystring}{qualitystring}{magical}{objectname}";
            }
            else
            {
                output += $"{article}{qtystring}{qualitystring}{magical}{objectname}";
            }

            var ownership = "";
            if (
                commonObjDat.canhaveowner(obj.item_id)
            &&
                (
                    (_RES == GAME_UW2) && (obj.race <= 30)
                    ||
                    (_RES != GAME_UW2) && (obj.race <= 27)
                )
            )
            {
                if (obj.owner > 0)
                {
                    ownership = $" belonging to{GameStrings.GetString(1, 370 + obj.race)}";
                }
            }

            if (OutputConvo)
            {
                uimanager.AddToMessageScroll(
                    stringToAdd: $"{output}{enchantmenttext}{ownership}",
                    option: 2,
                    mode: MessageDisplay.MessageDisplayMode.TemporaryMessage);
            }
            else
            {
                uimanager.AddToMessageScroll($"{output}{enchantmenttext}{ownership}");
            }

            return true;
        }

        private static string GetArticle(string noun)
        {
            if (
                (noun.ToUpper().StartsWith("A"))
                ||
                (noun.ToUpper().StartsWith("E"))
                ||
                (noun.ToUpper().StartsWith("I"))
                ||
                (noun.ToUpper().StartsWith("O"))
                ||
                (noun.ToUpper().StartsWith("U"))
                )
            {
                return "an ";
            }
            else
            {
                return "a ";
            }
        }
    }//end class
}//end namespace