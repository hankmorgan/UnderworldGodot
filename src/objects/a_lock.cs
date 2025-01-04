namespace Underworld
{
    public class a_lock : objectInstance
    {
        
        public static bool GetIsLocked(uwObject parentObject)
        {
            //if (isOpen) { return false; }
            var lockobj = LockObject(parentObject);
            if (lockobj == null)
            {
                return false;
            }
            else
            {
                return (lockobj.flags & 0x01) == 1;
            }
        }
            
        public static void SetIsLocked(uwObject parentObject, bool value, int character = 0)
        {
            var lockobj = LockObject(parentObject);
            if (lockobj == null) { return; }
            if (value)
            {//lock
                lockobj.flags |= 1;  //set flag bit 0
            }
            else
            {//unlock
                lockobj.flags &= 0xE;  //clear flag bit 0
                //run unlock trap
                trigger.TriggerObjectLink(
                    character: character, 
                    ObjectUsed: lockobj, 
                    triggerType: (int)triggerObjectDat.UNLOCK_TRIGGER_TYPE, 
                    triggerX: parentObject.tileX, 
                    triggerY: parentObject.tileY, 
                    objList: UWTileMap.current_tilemap.LevelObjects);
            }
        }

        /// <summary>
        /// Gets the lock attached to the parent object(typicall a door)
        /// </summary>
        /// <param name="parentObject"></param>
        /// <returns></returns>
        public static uwObject LockObject(uwObject parentObject)
        {
            return objectsearch.FindMatchInObjectChain(
                ListHeadIndex: parentObject.link,
                majorclass: 4,
                minorclass: 0,
                classindex: 0xF,
                objList: UWTileMap.current_tilemap.LevelObjects,
                SkipLinks: true);
        }


        /// <summary>
        /// What index key will open the lock attacked to this object.
        /// </summary>
        public static int KeyIndex(uwObject parentObject)
        {
            var lockobj = LockObject(parentObject);
            if (lockobj == null)
            {
                return -1;
            }
            return lockobj.link & 0x3F;
        }

    }//end class
}//end namespace