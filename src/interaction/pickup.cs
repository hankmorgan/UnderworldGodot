using Godot;
namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the pickup verb
    /// </summary>
    public class pickup : UWClass
    {
        public static bool Drop(int index, uwObject[] objList, Vector3 dropPosition, int tileX, int tileY)
        {
            var t = UWTileMap.current_tilemap.Tiles[tileX,tileY];
            if (t.tileType == UWTileMap.TILE_SOLID)
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1,GameStrings.str_there_is_no_place_to_put_that_));
                return false;
            }
            else
            {
                //translate dropposition into xpos, ypos and zpos on the tile.
                //for the moment just use 4,4 and floorheight.
                var obj = objList[index];
                obj.xpos= uwObject.FloatXYToXYPos(-dropPosition.X); 
                obj.ypos= uwObject.FloatXYToXYPos(dropPosition.Z);                
                obj.zpos = uwObject.FloatZToZPos(dropPosition.Y); //(short)(t.floorHeight<<2);
                
                obj.tileX= tileX; obj.tileY = tileY;
                obj.next = t.indexObjectList;
                t.indexObjectList = (short)index;
                if (obj.instance!=null)
                {
                    obj.instance.uwnode.Position = obj.GetCoordinate(tileX,tileY);
                }
                playerdat.ObjectInHand=-1;
                uimanager.instance.mousecursor.ResetCursor();
                return true;

            }
        }

        public static bool PickUp(int index, uwObject[] objList, bool WorldObject = true)
        {
            if (playerdat.ObjectInHand != -1)
            {
                //??
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
                obj.next = 0; //ensure end of chain.               
                obj.tileX=99;obj.tileY=99;
                if (obj.instance!=null)
                {
                    obj.instance.uwnode.Position = obj.GetCoordinate(obj.tileX, obj.tileY);
                }
            }
            return true;
        }    
    } //end class
}//end namespace