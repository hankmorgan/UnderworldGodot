using System;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    public class food : objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (_RES == GAME_UW2)
            {
                switch (obj.classindex)
                {
                    case 0: //a_piece of meat&pieces of meat
                    case 1: //a_piece of meat&pieces of meat
                    case 2: //a_piece of cheese&pieces of cheese
                    case 3: //an_apple
                    case 4: //an_ear of corn&ears of corn
                    case 5: //a_loaf of bread&loaves of bread
                    case 6: //a_fish&fish
                    case 7: //some_popcorn&bunches of popcorn
                    case 8: //a_pastry&pastries

                    case 0xA: //a_honeycomb
                    case 0xE: //some_meat-on-a-stick&pieces of meat-on-a-stick
                    case 0xF: //a_nutritious wafer
                        EatFood(obj, !WorldObject); break;
                    case 0xB: //a_bottle of ale&bottles of ale
                    case 0xC: //a_bottle of water&bottles of water
                    case 0xD: //a_bottle of wine&bottles of wine
                        DrinkLiquid(obj, !WorldObject); break;
                    case 9: //a_mushroom  //this needs to do a shrooms skill check on it
                        TakeShrooms(obj);
                        break;
                }
                return true;
            }
            else
            {
                switch (obj.classindex)
                {
                    case 0:// a_piece of meat&pieces of meat
                    case 1:// a_loaf of bread&loaves of bread
                    case 2:// a_piece of cheese&pieces of cheese
                    case 3:// an_apple
                    case 4:// an_ear of corn&ears of corn
                    case 5:// a_loaf of bread&loaves of bread
                    case 6:// a_fish&fish
                    case 7:// some_popcorn&bunches of popcorn

                    case 9:// a_toadstool
                        EatFood(obj, !WorldObject); break;
                    case 0xA:// a_bottle of ale&bottles of ale
                    case 0xB:// a_red potion
                    case 0xC:// a_green potion
                    case 0xD:// a_bottle of water&bottles of water
                    case 0xE:// a_flask of port&flasks of port
                        DrinkLiquid(obj, !WorldObject); break;
                    case 0xF:// a_bottle of wine&bottles of wine
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x7F)); break;
                    case 8:// a_mushroom
                        TakeShrooms(obj); break;

                }
            }
            return true;
        }

        static void EatFood(uwObject obj, bool UsedFromInventory)
        {
            var nutrition = foodObjectDat.nutrition(obj.item_id);
            if (nutrition >= 0)
            {
                if (playerdat.play_hunger + nutrition > 255)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_are_too_full_to_eat_that_now_));
                    return;
                }
                else
                {
                    var objname = "That " + GameStrings.GetObjectNounUW(obj.item_id, 1);
                    //TODO play eating sound. (sound played depends on how hungry the player is)
                    var r = new Random();
                    playerdat.play_hunger = (byte)(playerdat.play_hunger + (byte)nutrition);

                    //Taste string
                    var taste = (obj.quality + r.Next(0, 0x14)) >> 4;
                    if (taste > 4) { taste = 4; }

                    //;  tasted putrid. \n
                    //;  tasted a little rancid. \n
                    //;  tasted kind of bland. \n
                    //;  tasted pretty good. \n
                    //;  tasted great. \n
                    if (_RES == GAME_UW2)
                    {
                        uimanager.AddToMessageScroll($"{objname}{GameStrings.GetString(1, 0xBB + taste)}");
                    }
                    else
                    {
                        uimanager.AddToMessageScroll($"{objname}{GameStrings.GetString(1, 0xAC + taste)}");
                    }

                    Consume(obj, UsedFromInventory);
                    //TODO Leave left overs in the player cursor.  
                }
            }
        }

        static void DrinkLiquid(uwObject obj, bool UsedFromInventory)
        {
            var nutrition = -foodObjectDat.nutrition(obj.item_id);
            //play drinking sound.

            //intoxication change is -nutrition
            playerdat.intoxication += (nutrition);
            //do a strength skill check against drunkeness
            var result = playerdat.SkillCheck(playerdat.STR, playerdat.intoxication);
            //print message base on the skill check
            switch (result)
            {
                case playerdat.SkillCheckResult.CritFail:
                    {
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x102));// You wake feeling somewhat unstable but better
                        //TODO Screenshake
                        break;
                    }
                case playerdat.SkillCheckResult.CritSucess:
                    {//The drink makes you feel a little better for now.
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x101));
                        playerdat.play_hp += 2;
                        break;
                    }
                case playerdat.SkillCheckResult.Fail:
                    {
                        //TODO Screenshake
                        break;
                    }
            }
            Consume(obj, UsedFromInventory);
        }

        static void TakeShrooms(uwObject obj)
        {

        }

        /// <summary>
        /// Special cases where eating non-regular food objects have an effect caused by dragging them to the mouth
        /// </summary>
        /// <param name="obj"></param>
        public static bool SpecialFoodCases(uwObject obj, bool UsedFromInventory)
        {
            var tastestring = 0;
            var nutrition = 0;
            if (_RES == GAME_UW2)
            {//uw2

                switch (obj.item_id)
                {
                    case 0x92://Candle
                        tastestring = 245;
                        break;
                    case 0xCE://throny flower
                        tastestring = 251;
                        nutrition = 4;
                        break;
                    case 0x110://eyeball
                        tastestring = 246; // i think
                        break;
                    case 0xCF:
                    case 0x114://plant
                        tastestring = 248;
                        nutrition = 23;
                        break;
                    case 0x125://leeches
                        tastestring = 244;
                        break;
                    default:
                        return false; //cannot be eaten
                }
            }
            else
            {//uw1
                switch (obj.item_id)
                {
                    case 0x92://Candle
                        tastestring = 230;
                        break;
                    case 0xCE: //plant
                        tastestring = 236;
                        nutrition = 4;
                        break;
                    case 0xCF:  //plant
                        tastestring = 233;
                        nutrition = 23;
                        break;
                    case 0xD9://rotworm corpse
                        tastestring = 234;
                        nutrition = 4;
                        break;
                    case 0x11B: //rotworm stew
                        tastestring = 235;
                        nutrition = 64;
                        break;
                    case 0x125://leech
                        tastestring = 229;
                        break;
                    default:
                        return false;
                }
            }
            if (nutrition == 0)
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, tastestring));
            }
            else
            {
                if (playerdat.play_hunger + nutrition <= 255)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, tastestring));
                    playerdat.play_hunger = (byte)(playerdat.play_hunger + nutrition);
                }
                else
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_are_too_full_to_eat_that_now_));
                    return true;
                }
            }

            Consume(obj, UsedFromInventory);

            return true;
        }


        /// <summary>
        /// Removes or reduces the qty of the eaten object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="UsedFromInventory"></param>
        private static void Consume(uwObject obj, bool UsedFromInventory)
        {
            if (obj.ObjectQuantity > 1)
            {
                obj.link--;
            }
            else
            {
                //Remove Object From Inventory or world
                if (UsedFromInventory)
                {
                    playerdat.RemoveFromInventory(obj.index);
                    uimanager.UpdateInventoryDisplay();
                }
                else
                {
                    if (playerdat.ObjectInHand == obj.index)
                    {
                        playerdat.ObjectInHand = -1;
                        uimanager.instance.mousecursor.ResetCursor();
                    }
                    ObjectCreator.RemoveObject(obj);                    
                }
            }
        }
    }//end class
}//end namespace