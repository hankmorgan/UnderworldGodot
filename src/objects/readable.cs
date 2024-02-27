namespace Underworld
{
    public class Readable: objectInstance
    {

        /// <summary>
        /// The use interaction. Reads the object unless there is a special effect or magic to be cast
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool Use(uwObject obj, uwObject[] objlist)
        {
            if (_RES==GAME_UW2)
            {//TODO implement the UW2 logic.
                return LookAt(obj, objlist);
            }
            else
            {
                if (obj.enchantment == 0 || (obj.enchantment == 1 && obj.majorclass==5))
                {
                    if(obj.flags1==0)
                    {
                        if ((obj.link & 0x1FF) < 0x100)
                            {
                                return LookAt(obj, objlist);//default read
                            }
                        else
                            {
                                //rotworm stew and only rotworm stew
                                return rotwormstew.MixRotwormStew();
                            }                        
                    }
                    else
                    {
                        var cutsno = (obj.link & 0x1ff) + 0x100; 
                        uimanager.AddToMessageScroll($"Display Cutscene {cutsno}", colour: 2);
                        return true;
                    }
                }
                else
                {
                     uimanager.AddToMessageScroll($"Cast a spell from this object", colour: 2);
                     return true;
                }
            }
        }

        /// <summary>
        /// The read interation.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool LookAt(uwObject obj, uwObject[] objList)
        {
            if (obj.is_quant == 1)
                {
                look.GeneralLookDescription(obj: obj, objList: objList);
                uimanager.AddToMessageScroll(GameStrings.GetString(3, obj.link-0x200));
                }
            return true;
        }

    }//end class
}//end namespace