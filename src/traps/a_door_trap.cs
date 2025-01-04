using System.Diagnostics;

namespace Underworld
{
    public class a_door_trap:trap
    {
        public static void Activate(uwObject trapObj, int triggerX, int triggerY, uwObject[] objList)
        {
            var tile = UWTileMap.current_tilemap.Tiles[triggerX, triggerY];
            var doorObj = objectsearch.FindMatchInObjectChain(
                ListHeadIndex: tile.indexObjectList, 
                majorclass: 5, 
                minorclass: 0, 
                classindex: -1, 
                objList: objList);
            if (doorObj==null)
            {//check for a moving door
                doorObj = objectsearch.FindMatchInObjectChain(
                    ListHeadIndex: tile.indexObjectList, 
                    majorclass: 7, 
                    minorclass: 0, 
                    classindex: 0xF, 
                    objList: objList);
            }
            if (doorObj == null)
            {
                Debug.Print("No Door found for door trap");
            }
            if (doorObj.instance==null)
            {
                 Debug.Print("No Door instance found for door trap");
            }
            if(trapObj!=null)
            {
                switch (trapObj.quality)
                {
                    // case 0: //Try Lock
                    //     Debug.Print("Door Trap Lock");
                    //     break;
                    case 1://Try Open
                        Debug.Print("Door Trap Open");
                        door.OpenDoor((door)doorObj.instance);
                        break;
                    case 2://Try Close
                        Debug.Print("Door Trap Close");
                        door.CloseDoor((door)doorObj.instance);
                        break;
                    case 3: //Try toggle
                        Debug.Print("Door Trap Toggle");
                        door.ToggleDoor((door)doorObj.instance);
                        break;
                    default:
                        Debug.Print($"Unknown doortrap quality {trapObj.quality}");
                        break;                        
                }
            }
        }
    }//end class
}//end namespace