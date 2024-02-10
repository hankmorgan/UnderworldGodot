namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the pickup verb
    /// </summary>
    public class pickup : UWClass
    {
        public static bool PickUp(int index, uwObject[] objList, bool WorldObject = true)
        {
            if (playerdat.ObjectInHand != -1)
            {
                //player is holding an object. Drop/throw it.
            }
            else
            {
                var obj = objList[index];
                //player is trying to pick something up
                playerdat.ObjectInHand = index;
                uimanager.instance.mousecursor.SetCursorArt(obj.item_id);

                //remove from it's tile
                var tile = UWTileMap.current_tilemap.Tiles[obj.tileX, obj.tileY];
                int nextObjectIndex = tile.indexObjectList;
                if (nextObjectIndex==index)
                {//object is first in list, easy swap
                    tile.indexObjectList = obj.next;
                }
                else
                {
                    while(nextObjectIndex!=0)
                    {
                        var nextObj = objList[nextObjectIndex];
                        if (nextObj.next==index)
                        {
                            nextObj.next = obj.next;
                            nextObjectIndex = 0;
                        }
                        else
                        {
                            nextObjectIndex = nextObj.next;
                        }                       
                    }
                }                
                obj.tileX=99;obj.tileY=99;
                if (obj.instance!=null)
                {
                    obj.instance.uwnode.Position = obj.GetCoordinate(obj.tileX, obj.tileY);
                }
            }
            return true;
        }
    }
}//end namespace