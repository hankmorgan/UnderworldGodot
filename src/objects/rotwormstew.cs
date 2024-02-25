using System.Runtime.CompilerServices;

namespace Underworld
{
    /// <summary>
    /// Class for the cooking of rotworm stew
    /// </summary>
    public class rotwormstew : objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (WorldObject){return false;}
            return food.SpecialFoodCases(obj,!WorldObject);
        }

        public static bool MixRotwormStew()
        {
            var bowl = objectsearch.FindMatchInObjectList(2, 0, 0xE, playerdat.InventoryObjects);
            if (bowl == null)
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_need_a_bowl_to_mix_the_ingredients_));
            }
            else
            {//check contents
                var ingredientsToTest = container.ListObjects(bowl.index, playerdat.InventoryObjects);
                if (ingredientsToTest == null)
                {
                    ErrorWrongIngredients();
                }
                else
                {
                    if (ingredientsToTest.GetUpperBound(0) == 2)
                    {//contains only 3 ingredients of qty 1
                        if (
                            CheckForIngredient(ingredientsToTest, 184)
                            ||
                            CheckForIngredient(ingredientsToTest, 190)
                            ||
                            CheckForIngredient(ingredientsToTest, 217)
                            )
                            {
                                if (uimanager.OpenedContainerIndex == bowl.index)
                                {
                                    container.Close(bowl.index, playerdat.InventoryObjects);
                                }
                                //matching
                                for (int i = 0; i<=ingredientsToTest.GetUpperBound(0);i++)
                                {
                                    playerdat.RemoveFromInventory(ingredientsToTest[i], false);
                                }  
                                bowl.item_id = 283;
                                bowl.link = 0;    //should be already 0 at this point if the above removes worked                          
                                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_mix_the_ingredients_into_a_stew_));
                                uimanager.UpdateInventoryDisplay();
                            }
                        else
                        {
                            ErrorWrongIngredients();
                        }
                    }
                    else
                    {
                        ErrorWrongIngredients();
                    }
                }
            }
            return true;
        }

        static bool CheckForIngredient(int[] ingredientsToTest, int DesiredIngredient)
        {
            for (int i = 0; i <= ingredientsToTest.GetUpperBound(0); i++)
            {
                var ingredient = playerdat.InventoryObjects[ingredientsToTest[i]];
                if (ingredient != null)
                {
                    if (ingredient.item_id == DesiredIngredient)
                    {
                        if (ingredient.ObjectQuantity == 1)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        static void ErrorWrongIngredients()
        {
            uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_the_bowl_does_not_contain_the_correct_ingredients_));
        }
    } //end class
}//end namespace