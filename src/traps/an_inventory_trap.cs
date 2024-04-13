using System.Diagnostics;

namespace Underworld
{
    public class an_inventory_trap : trap
    {
        public static short Activate(uwObject trapObj, uwObject[] objList)
        {
            var itemid = ((trapObj.quality & 0x3F) << 5) | (trapObj.owner & 0x3F);
            var majorclass = itemid >> 6;
            var minorclass = (itemid & 0x30) >> 4;
            var classindex = itemid & 0xF;

            var match = objectsearch.FindMatchInObjectList(majorclass, minorclass, classindex, playerdat.InventoryObjects);
            if (match != null)
            {
                if ((trapObj.xpos != 0) && (_RES == GAME_UW2))
                {
                    //check if object is in the proper slot for it's type in UW2. Probably checking fraznium gloves/crown
                    if (! IsObjectValidForItsSlot(match))
                    {
                        return 0;// object is not in a valid paperdoll slot
                    }
                }

                if (trapObj.zpos > 0)
                { //A qty of object is required.
                    if (trapObj.zpos <= match.ObjectQuantity)
                    {
                        return trapObj.link;
                    }
                    else
                    {
                        Debug.Print("you have the object but just not enough of it. TEST ME");
                        return 0;//not enough of the object qty
                    }
                }
                else
                {
                    return trapObj.link;
                }
            }
            else
            {
                return 0;
            }
        }


        /// <summary>
        /// Checks if an object is in an appropiate paperdoll slot
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        static bool IsObjectValidForItsSlot(uwObject match)
        {
            //gets the paperdoll slot the object is in.
            int slot = -1;
            if (playerdat.Helm == match.index)
            {
                return true;
            }
            else if (playerdat.ChestArmour == match.index)
            {
                return true;
            }
            else if (playerdat.Gloves == match.index)
            {
                return true;
            }
            else if (playerdat.Leggings == match.index)
            {
                return true;
            }
            else if (playerdat.Boots == match.index)
            {
                return true;
            }
            else if (playerdat.RightHand == match.index)
            {
                slot = 7;
            }
            else if (playerdat.LeftHand == match.index)
            {
                slot = 8;
            }
            else if (playerdat.RightRing == match.index)
            {
                return true;
            }
            else if (playerdat.LeftRing == match.index)
            {
                return true;
            }

            if (slot == -1)
            {
                return false;
            }
            else
            {
                if (playerdat.isLefty)
                {
                    if (slot == 8)
                    {
                        //valid for weapons,
                        if (
                            (match.majorclass == 0) && (match.minorclass == 0)
                            ||
                            (match.majorclass == 0) && (match.minorclass == 1) && (match.classindex < 0xB)
                        )
                        {
                            return true;
                        }
                    }
                    if (slot == 7)
                    {
                        //valid for shields
                        if (
                            (match.majorclass == 0) && (match.minorclass == 3) && (match.classindex >= 0xB)
                           ||
                           (_RES != GAME_UW2) && (match.majorclass == 0) && (match.minorclass == 3) && (match.classindex == 7)
                            )
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (slot == 7)
                    {
                        //valid for weapons,
                        if (
                            (match.majorclass == 0) && (match.minorclass == 0)
                            ||
                            (match.majorclass == 0) && (match.minorclass == 1) && (match.classindex < 0xB)
                        )
                        {
                            return true;
                        }
                    }
                    if (slot == 8)
                    {
                        //valid for shields
                        if (
                            (match.majorclass == 0) && (match.minorclass == 3) && (match.classindex >= 0xB)
                           ||
                           (_RES != GAME_UW2) && (match.majorclass == 0) && (match.minorclass == 3) && (match.classindex == 7)
                            )
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }//end class
}//end namespace
