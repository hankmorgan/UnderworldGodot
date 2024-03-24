using System;
using System.Diagnostics;

namespace Underworld
{
    public class a_delete_object_trap : trap
    {
        public static void Activate(uwObject trapObj, uwObject[] objList)
        {
            var tileX = trapObj.quality; var tileY = trapObj.owner;
            var tile = UWTileMap.current_tilemap.Tiles[tileX,tileY];
            var indexToDelete = trapObj.link;
            
            if (indexToDelete!=0)
            {
                var objectToDelete = objList[indexToDelete];
                if (objectToDelete!=null)
                {
                    if (tile.indexObjectList == indexToDelete)
                    {
                        tile.indexObjectList = objectToDelete.next;
                        objectToDelete.next = 0;
                        ObjectCreator.RemoveObject(objectToDelete);
                        return;
                    }
                    else
                    {
                        //search
                        var next = tile.indexObjectList;
                        
                        while (next!=0)
                        {
                            var nextObject =  objList[next];
                            if (nextObject.next == indexToDelete)
                            {
                                nextObject.next = objectToDelete.next;
                                objectToDelete.next = 0;
                                ObjectCreator.RemoveObject(objectToDelete);
                                return;
                            }
                            next = nextObject.next;
                        }
                        Debug.Print($"Was unable to find {indexToDelete} to delete it in {tileX},{tileY}");
                    }
                }
            }            
        }
    }//end class
}//end namespace