using System.Diagnostics;
using Godot;
namespace Underworld
{

    public partial class ConversationVM : UWClass
    {
        /// <summary>
        /// Gronks a door open.
        /// </summary>
        public static void gronk_door()
        {
            var tileX = GetConvoStackValueAtPtr(stackptr - 3);
            var tileY = GetConvoStackValueAtPtr(stackptr - 2);
            var mode = GetConvoStackValueAtPtr(stackptr - 1);

            Debug.Print($"Gronkdoor {tileX}, {tileY}, {mode}");
            var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
            var doorobj = objectsearch.FindMatchInObjectList(
             ListHeadIndex: tile.indexObjectList,
             majorclass: 5,
             minorclass: 0,
             classindex: -1,
             objList: UWTileMap.current_tilemap.LevelObjects
             );
            if (doorobj == null)
                {
                //search for a moving door that matches
                doorobj = objectsearch.FindMatchInObjectList(
                    ListHeadIndex: tile.indexObjectList,
                    majorclass: 7,
                    minorclass: 0,
                    classindex: 0xF,
                    objList: UWTileMap.current_tilemap.LevelObjects
                );
                }
            if (doorobj == null)
                {
                    result_register = 0;
                    return;
                }
            else
            {
                switch (mode)
                {
                    case 2:
                        door.ToggleDoor((door)doorobj.instance);
                        break;
                    case 1: //try clase
                        door.CloseDoor((door)doorobj.instance); 
                        break;
                    case 0:
                    default:  //try open
                        door.OpenDoor((door)doorobj.instance); 
                        break;
                }
            }
            result_register = 1;
        }

    }//end class

}//end namespace