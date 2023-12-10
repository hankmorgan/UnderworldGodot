namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the use verb when holding another object
    /// </summary>
    public class useon : UWClass
    {
        
        public static bool UseOn(int index, uwObject[] objList, bool WorldObject = true)
        {
            if (playerdat.ObjectInHand==-1)
            {
                return false;
            }
            var ObjInHand = playerdat.InventoryObjects[playerdat.ObjectInHand];

            if (index==-1){return false;}
            trap.ObjectThatStartedChain = index;
            bool result = false;
            if (index <= objList.GetUpperBound(0))
            {
                var targetObj = objList[index];
                switch (ObjInHand.majorclass)
                {
                    case 2:
                        {
                            //result = UseOnMajorClass2(obj, objList, WorldObject);
                            break;
                        }
                    case 4:
                        {
                            result = UseOnMajorClass4(ObjInHand, targetObj);
                            break;
                        }
                    case 5:
                        {
                            //result = UseOnMajorClass5(obj, objList, WorldObject);
                            break;
                        }
                }
                //Check for use trigger on this action and try activate if so.
                if ((targetObj.is_quant == 0) && (targetObj.link != 0))
                {
                    trigger.UseTrigger(
                        srcObject: targetObj,
                        triggerIndex: targetObj.link,
                        objList: objList);
                }
            }
            // if (!result)
            // {
            //    messageScroll.AddString(GameStrings.GetString(1, GameStrings.str_you_cannot_use_that_));
            // }

            //Clear the cursor icons
            playerdat.ObjectInHand = -1;
            uimanager.instance.mousecursor.ResetCursor();

            return false;
        }


        public static bool UseOnMajorClass4(uwObject objInHand,uwObject targetObject)
        {
            switch (objInHand.minorclass)
            {
                case 0: //keys up to 0xE
                    {
                        if (objInHand.classindex<=0xE)
                        {
                            return doorkey.UseOn(objInHand, targetObject);
                        }
                        break;
                    }
            }
            return false;
        }

    } //end class
} //end namespace