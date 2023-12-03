using System;
using System.Diagnostics;

namespace Underworld
{
    public class food : objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (WorldObject) { return false; }
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
                    case 9: //a_mushroom
                    case 0xA: //a_honeycomb
                    case 0xE: //some_meat-on-a-stick&pieces of meat-on-a-stick
                    case 0xF: //a_nutritious wafer
                        EatFood(obj); break;
                    case 0xB: //a_bottle of ale&bottles of ale
                    case 0xC: //a_bottle of water&bottles of water
                    case 0xD: //a_bottle of wine&bottles of wine
                        DrinkLiquid(obj); break;
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
                    case 8:// a_mushroom
                    case 9:// a_toadstool
                        EatFood(obj); break;
                    case 0xA:// a_bottle of ale&bottles of ale
                    case 0xB:// a_red potion
                    case 0xC:// a_green potion
                    case 0xD:// a_bottle of water&bottles of water
                    case 0xE:// a_flask of port&flasks of port
                        DrinkLiquid(obj); break;
                    case 0xF:// a_bottle of wine&bottles of wine
                        messageScroll.AddString(GameStrings.GetString(1, 0x7F)); break;

                }
            }
            return true;
        }

        static void EatFood(uwObject obj)
        {
            var nutrition = foodObjectDat.nutrition(obj.item_id);
            if (nutrition >= 0)
            {
                if (playerdat.play_hunger + nutrition > 255)
                {
                    messageScroll.AddString(GameStrings.GetString(1, GameStrings.str_you_are_too_full_to_eat_that_now_));
                    return;
                }
                else
                {
                    var objname = "That " + GameStrings.GetObjectNounUW(obj.item_id,1);
                    //TODO play eating sound. (sound played depends on how hungry the player is)
                    var r = new Random();
                    playerdat.play_hunger += nutrition;

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
                        messageScroll.AddString($"{objname}{GameStrings.GetString(1, 0xBB + taste)}");
                    }
                    else
                    {
                        messageScroll.AddString($"{objname}{GameStrings.GetString(1, 0xAC + taste)}");
                    }

                    if (obj.ObjectQuantity > 1)
                    {
                        obj.link--;
                    }
                    else
                    {
                        //Remove Object From Inventory
                        playerdat.RemoveFromInventory(obj.index);
                    }
                    uimanager.UpdateInventoryDisplay();
                    //TODO Leave left overs in the player cursor.  
                }
            }
        }

        static void DrinkLiquid(uwObject obj)
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
                        messageScroll.AddString(GameStrings.GetString(1, 0x102));// You wake feeling somewhat unstable but better
                        //TODO Screenshake
                        break;
                    }
                case playerdat.SkillCheckResult.CritSucess:
                    {//The drink makes you feel a little better for now.
                        messageScroll.AddString(GameStrings.GetString(1, 0x101));
                        break;
                    }
                case playerdat.SkillCheckResult.Fail:
                    {
                        //TODO Screenshake
                        break;
                    }
            }
        }
    }//end class
}//end namespace