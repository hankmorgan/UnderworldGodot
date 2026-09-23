using SerdesNet;

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


        /// <summary>
        /// Removes all locks from a container or door. Called when a door is broken open or when a locked container is spilled.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="RemoveAll"></param>
        /// <returns></returns>
        public static bool RemoveAllLocks(uwObject obj, bool RemoveAll)
        {
            bool HasRemoved = false;
            if (obj.is_quant == 0)
            {
                if (obj.link != 0)
                {
                    while (true) //loop until all locks(when removeall==true) or until the first lock has been removed (when removeall = false), or until there are no locks
                    {
                        var lockObj = objectsearch.FindMatchInObjectChain(
                            ListHeadIndex: obj.index, 
                            majorclass: 4, minorclass: 0, classindex: 0xF, 
                            objList: UWTileMap.current_tilemap.LevelObjects);
                        if (lockObj != null)
                        {
                            if (ObjectRemover_OLD.RemoveObjectFromLinkedList(listhead: obj.link, toRemove: lockObj.index, objlist: UWTileMap.current_tilemap.LevelObjects, OffsetToListHeadConnection: obj.PTR + 6))
                            {
                                ObjectFreeLists.ReleaseFreeObject(lockObj);
                            }
                            if (RemoveAll)
                            {
                                return HasRemoved; //this value will be false in this scenario.
                            }
                            else
                            {
                                HasRemoved = true;
                            }
                        }
                        else
                        {
                            return HasRemoved;
                        }
                    }
                }
            }
            return HasRemoved;
        }

    }//end class
}//end namespace