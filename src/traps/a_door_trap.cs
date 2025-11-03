using System.Diagnostics;

namespace Underworld
{
    public class a_door_trap : trap
    {
        public static void Activate(uwObject trapObj, int triggerX, int triggerY, uwObject[] objList)
        {
            if (trapObj == null)
            {
                Debug.Print("Null door trap");
                return;
            }
            var tile = UWTileMap.current_tilemap.Tiles[triggerX, triggerY];
            var doorObj = objectsearch.FindMatchInObjectChain(
                ListHeadIndex: tile.indexObjectList,
                majorclass: 5,
                minorclass: 0,
                classindex: -1,
                objList: objList);
            if (doorObj == null)
            {//check for a moving door
                doorObj = objectsearch.FindMatchInObjectChain(
                    ListHeadIndex: tile.indexObjectList,
                    majorclass: 7,
                    minorclass: 0,
                    classindex: 0xF,
                    objList: objList);
                if (doorObj != null)
                {
                    //found a moving door
                    if (doorObj.owner >= 8)
                    {
                        if ((trapObj.quality == 1) || (trapObj.quality == 3))
                        {
                            Debug.Print("Moving Door Trap Open (not implemented) This will glitch out doorways.");
                            door.OpenDoor(doorObj);
                        }
                    }
                    else
                    {
                        if ((trapObj.quality == 2) || (trapObj.quality == 3))
                        {
                            //try close
                            Debug.Print("Moving Door Trap Close (not implemented) This will glitch out doorways.");
                            door.CloseDoor(doorObj);
                        }
                    }
                }
            }
            else
            {
                //found a static door do the actions on it
                switch (trapObj.quality)
                {
                    case 1://Try Open
                        Debug.Print("Door Trap Open");
                        door.OpenDoor(doorObj);
                        break;
                    case 2://Try Close
                        Debug.Print("Door Trap Close");
                        door.CloseDoor(doorObj);
                        break;
                    case 3: //Try toggle
                        Debug.Print("Door Trap Toggle");
                        door.ToggleDoor(doorObj);
                        break;
                }
            }

            if (_RES == GAME_UW2)
            {
                //unlike UW1, UW2 has additional actions in the door trap when it's owner value is not 0. These will copy and link a lock that is located further up the chain.

                Debug.Print($"TrapObj has owner {trapObj.owner}");
                if (doorObj != null)
                {
                    if (trapObj.owner != 0)
                    {
                        var lockObject = objectsearch.FindMatchInObjectChain(
                            ListHeadIndex: doorObj.link, 
                            majorclass: 4, minorclass: 0, classindex: 0xF, 
                            objList: objList,
                            SkipLinks: true);
                        if (lockObject!=null)
                        {//Remove the current lock
                            // if(doorObj.link == trapObj.index)
                            // {

                            // }
                            // else
                            // {
                            if (ObjectRemover_OLD.RemoveObjectFromLinkedList(doorObj.link, lockObject.index, objList, doorObj.PTR+6))
                            {
                                ObjectFreeLists.ReleaseFreeObject(lockObject);
                            }
                            //}                           
                        }

                        if ((trapObj.is_quant==0) && (trapObj.link!=0))
                        {//apply the template lock
                            lockObject = objList[trapObj.link];
                            var freeslot = ObjectFreeLists.GetAvailableObjectSlot(ObjectFreeLists.ObjectListType.StaticList);
                            if(freeslot!=0)
                            {
                                uwObject destObject = objList[freeslot];
                                //copy data
                                for (int i=0; i<8;i++)
                                {
                                    destObject.DataBuffer[destObject.PTR+i] = lockObject.DataBuffer[lockObject.PTR+i];
                                }
                                //insert into object list as linked to the door
                                destObject.next = doorObj.link;
                                doorObj.link = destObject.index;                                                                
                            }
                        }
                    }
                }
            }




            // if (doorObj == null)
            // {
            //     Debug.Print("No Door found for door trap");
            // }
            // if (doorObj.instance==null)
            // {
            //      Debug.Print("No Door instance found for door trap");
            // }
            // if(trapObj!=null)
            // {

            //}

        }
    }//end class
}//end namespace