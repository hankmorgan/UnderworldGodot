using System;
using System.Diagnostics;

namespace Underworld
{
    public class oilflask : objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (!WorldObject)
            {
                //flag we are using the oil flask         
                useon.CurrentItemBeingUsed = new useon(obj, WorldObject);
                //print use message
                uimanager.AddToMessageScroll(useon.CurrentItemBeingUsed.GeneralUseOnString);
            }
            return true;
        }

        public static bool UseOn(uwObject OilFlaskObject, uwObject targetObject)
        {//assuming both objects are in inventory
            int stringno;
            if ((targetObject.item_id == 0x90) | (targetObject.item_id == 94))
            {//target is a lantern
                stringno = 0;
            }
            else
            {//target is not a lantern
                stringno = 4;
            }

            switch (targetObject.item_id)
            {
                case 0xCC://use on pieces of wood
                case 0xCD:
                    {
                        if (targetObject.ObjectQuantity > 1)
                        {//remove one instance and make the held object in hand a new torch
                            targetObject.link--;
                            ObjectCreator.SpawnObjectInHand(0x91);
                        }
                        else
                        {//turn the target into a torch
                            targetObject.item_id = 0x91;
                        }
                        targetObject.quality = 40;
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_dousing_a_cloth_with_oil_and_applying_it_to_the_wood_you_make_a_torch_));
                        uimanager.UpdateInventoryDisplay();
                    return true;
                    }
                case 0x90:
                case 0x91://lantern or torch
                    {
                        if (targetObject.ObjectQuantity>1)
                        {
                            targetObject.link--;
                            ObjectCreator.SpawnObjectInHand(targetObject.item_id);
                            var obj = UWTileMap.current_tilemap.LevelObjects[playerdat.ObjectInHand];
                            ApplyOilToUnlit(obj, stringno);
                        }
                        else
                        {
                            ApplyOilToUnlit(targetObject, stringno);
                        }
                        return true;
                    }
                case 0x94:
                case 0x95:
                    {
                        if (_RES == GAME_UW2)
                            {
                                uimanager.AddToMessageScroll(GameStrings.GetString(1, stringno + 193));//oil on flame warning
                            }
                            else
                            {
                                uimanager.AddToMessageScroll(GameStrings.GetString(1, stringno + 178));//oil on flame warning
                            }
                        return true;
                    }
            }
            return false;
        }

        /// <summary>
        /// Uses oil to increase the quality of the lantern or torch.
        /// </summary>
        /// <param name="targetObject"></param>
        /// <param name="stringno"></param>
        private static void ApplyOilToUnlit(uwObject targetObject, int stringno)
        {
            if (targetObject.quality == 63)
            {
                if (_RES == GAME_UW2)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, stringno + 195));//already full message
                }
                else
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, stringno + 180));//already full message
                }
            }
            else
            {
                targetObject.quality = (short)Math.Min(targetObject.quality + 32, 63);
                if (_RES == GAME_UW2)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, stringno + 194));//refresh light message
                }
                else
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, stringno + 179));//refresh message
                }
            }
        }
    }//end class
}//end namespace