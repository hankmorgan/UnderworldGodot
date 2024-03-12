namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the use verb when holding another object
    /// </summary>
    public class useon : UWClass
    {
        public static useon CurrentItemBeingUsed;
        public int index = 0;

        public bool WorldObject;

        public uwObject itemBeingUsed;

        public useon(uwObject obj, bool _worldobject )
        {
            index = obj.index;
            WorldObject = _worldobject;
            itemBeingUsed = obj;
            //change the mouse cursor
            uimanager.instance.mousecursor.SetCursorToObject(obj.item_id);
        }


        public static bool UseOn(int index, uwObject[] targetobjList, useon srcObject)
        {
            // if (playerdat.ObjectInHand==-1)
            // {
            //     return false;
            // }
            //var ObjInHand = UWTileMap.current_tilemap.LevelObjects[playerdat.ObjectInHand];//playerdat.InventoryObjects[playerdat.ObjectInHand];
            var ObjInHand = srcObject.itemBeingUsed;
            if (index==-1){return false;}
            trap.ObjectThatStartedChain = index;
            bool result = false;
            if (index <= targetobjList.GetUpperBound(0))
            {
                var targetObj = targetobjList[index];
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
                        objList: targetobjList);
                }
            }
            // if (!result)
            // {
            //    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_cannot_use_that_));
            // }

            //Clear the cursor icons
            useon.CurrentItemBeingUsed = null;
           // playerdat.ObjectInHand = -1;
            uimanager.instance.mousecursor.SetCursorToCursor();

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