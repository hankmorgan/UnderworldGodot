using System;
using System.Diagnostics;

namespace Underworld
{
    public class Food : objectInstance
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
                        TakeShrooms(obj, !WorldObject);
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
                        potion.QuaffPotion(obj, !WorldObject);
                        break;
                    case 0xD:// a_bottle of water&bottles of water
                    case 0xE:// a_flask of port&flasks of port
                        DrinkLiquid(obj, !WorldObject); break;
                    case 0xF:// a_bottle of wine&bottles of wine
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x7F)); break;
                    case 8:// a_mushroom
                        TakeShrooms(obj, !WorldObject); break;
                }
            }
            return true;
        }

        /// <summary>
        /// Handles eating of regular food items.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="UsedFromInventory"></param>
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
                    //update hunger
                    playerdat.ChangeHunger(nutrition);

                    PlayEatingSounds();
                    TasteDescriptionString(obj);

                    int foodItemID = obj.item_id;
                    ObjectCreator.Consume(obj, UsedFromInventory);
                    if (_RES == GAME_UW2)
                    {
                        SpawnLeftOvers(foodItemID);
                    }
                }
            }
        }

        /// <summary>
        /// Generates the text describing how the food tasted
        /// </summary>
        /// <param name="obj"></param>
        private static void TasteDescriptionString(uwObject obj)
        {
            var objname = "That " + GameStrings.GetObjectNounUW(obj.item_id, 1);
            // Build the string for the taste description.
            var r = new Random();
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
        }


        /// <summary>
        /// Play eating sound. (sound played depends on how hungry the player is)  
        /// </summary>
        private static void PlayEatingSounds()
        {
           
            if (playerdat.play_hunger >= 192)
            {
                Debug.Print("Play Sound Effect 0x25 (eating)");
            }
            else
            {
                if (playerdat.play_hunger > 90)
                {
                    Debug.Print("Play Sound Effect 0x1F (eating)");
                }
                else
                {
                    Debug.Print("Play Sound Effect 0x21h (eating)");
                }
            }
        }


        /// <summary>
        /// Spawns leftovers in the players hand after eating certain foods and drinks
        /// </summary>
        /// <param name="foodItemId"></param>
        private static void SpawnLeftOvers(int foodItemId)
        {
            var leftOvers = -1;
            if (_RES == GAME_UW2)
            {
                switch (foodItemId)
                {
                    case 0xB0://piece of meat
                    case 0xB1:
                        leftOvers = 0xC5; break;
                    case 0xBA://candle
                        leftOvers = 0xD2; break;
                    case 0xBB: //bottles
                    case 0xBC:
                    case 0xBD:
                        leftOvers = 0x13D; break;
                    case 0xBE://meat on a stick
                        leftOvers = 0x13E; break;                        
                    default:
                        //leftOvers = 0;
                        break;
                }
            }
            if (leftOvers != -1)
            {
                //create the left overs in the players hand
                ObjectCreator.SpawnObjectInHand(leftOvers);
            }
        }



        /// <summary>
        /// Handles the drinking of alcohol and water(?)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="UsedFromInventory"></param>
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
                        sleep.Sleep(-2);
                        if (playerdat.play_hp > 0)
                        {
                            uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x102));// You wake feeling somewhat unstable but better
                        }
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
            var foodItemID = obj.item_id;
            ObjectCreator.Consume(obj, UsedFromInventory);
            if (_RES == GAME_UW2)
            {
                SpawnLeftOvers(foodItemID);
            }
        }

        /// <summary>
        /// Applies the effects of mushroom consumption.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="UsedFromInventory"></param>
        static void TakeShrooms(uwObject obj, bool UsedFromInventory)
        {
            var SkillCheckResult = playerdat.SkillCheck(playerdat.INT, 0x14);
            var manachange = Rng.r.Next(1, 4) * (int)SkillCheckResult;

            playerdat.ManaRegenChange(manachange);

            playerdat.shrooms = Math.Min(3, playerdat.shrooms + 1);
            uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_the_mushroom_causes_your_head_to_spin_and_your_vision_to_blur_));
            ObjectCreator.Consume(obj, UsedFromInventory);
            playerdat.PlayerStatusUpdate();
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
                    playerdat.ChangeHunger(nutrition);
                }
                else
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_are_too_full_to_eat_that_now_));
                    return true;
                }
            }

            if ((_RES == GAME_UW2) && (obj.item_id == 0x114))
            {
                if (!playerdat.DreamingInVoid)
                {
                    playerdat.DreamPlantCounter = (2 + Rng.r.Next(4)) & 0x7;
                }
            }
           
            var foodItemID = obj.item_id;
            
            ObjectCreator.Consume(obj, UsedFromInventory);
            
            if (_RES == GAME_UW2)
            {
                SpawnLeftOvers(foodItemID);
            }
            return true;
        }


        public static bool IsFood(uwObject obj)
        {

            if (obj.majorclass == 2)
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
                        case 0xB: //a_bottle of ale&bottles of ale
                        case 0xC: //a_bottle of water&bottles of water
                        case 0xD: //a_bottle of wine&bottles of wine
                        case 9: //a_mushroom  //this needs to do a shrooms skill check on it
                            return true;
                    }
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
                        case 0xA:// a_bottle of ale&bottles of ale
                        case 0xB:// a_red potion
                        case 0xC:// a_green potion                      
                        case 0xD:// a_bottle of water&bottles of water
                        case 0xE:// a_flask of port&flasks of port                       
                        case 0xF:// a_bottle of wine&bottles of wine                       
                        case 8:// a_mushroom
                            return true;
                    }
                }
            }


            if (_RES == GAME_UW2)
            {//uw2
                switch (obj.item_id)
                {
                    case 0x92://Candle                     
                    case 0xCE://throny flower
                    case 0x110://eyeball
                    case 0xCF:
                    case 0x114://plant
                    case 0x125://leeches
                        return true;
                }
            }
            else
            {//uw1
                switch (obj.item_id)
                {
                    case 0x92://Candle
                    case 0xCE: //plant                     
                    case 0xCF:  //plant
                    case 0xD9://rotworm corpse                       
                    case 0x11B: //rotworm stew                      
                    case 0x125://leech
                        return true;
                }
            }
            return false;
        }       

    }//end class
}//end namespace