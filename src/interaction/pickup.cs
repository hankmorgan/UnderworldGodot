using Godot;
using Peaky.Coroutines;
using System.Collections;

namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the pickup verb
    /// </summary>
    public class pickup : UWClass
    {
        public static bool Drop(int index, uwObject[] objList, Vector3 dropPosition, int tileX, int tileY)
        {
            var t = UWTileMap.current_tilemap.Tiles[tileX, tileY];
            if (t.tileType == UWTileMap.TILE_SOLID)
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_there_is_no_place_to_put_that_));
                return false;
            }
            else
            {
                //translate drop position into xpos, ypos and zpos on the tile.
                //for the moment just use 4,4 and floorheight.
                var obj = objList[index];
                obj.xpos = uwObject.FloatXYToXYPos(-dropPosition.X);
                obj.ypos = uwObject.FloatXYToXYPos(dropPosition.Z);
                obj.zpos = uwObject.FloatZToZPos(dropPosition.Y); //(short)(t.floorHeight<<2);

                obj.tileX = tileX; obj.tileY = tileY;
                obj.next = t.indexObjectList;
                t.indexObjectList = (short)index;
                //remove the old
                // why was this here 
                //ObjectCreator.RemoveObject(obj);  
                //create the new              
                ObjectCreator.RenderObject(obj, UWTileMap.current_tilemap);
                return true;
            }
        }

        static bool CanBePickedUpOverrides(int item_id)
        {
            if (_RES != GAME_UW2)
            {
                if (item_id == 458)
                {
                    return true;
                }
            }
            return false;
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

                if (!commonObjDat.CanBePickedUp(obj.item_id) && !CanBePickedUpOverrides(obj.item_id))
                {//object cannot be picked up
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_cannot_pick_that_up_));
                    return false;
                }
                if (obj.ObjectQuantity > 1)
                {
                    //prompt for quantity in coroutine.
                    _ = Peaky.Coroutines.Coroutine.Run(
                            DoPickupQty(index, objList, obj),
                            main.instance
                        );
                    return true;
                }
                else
                {
                    //single instance object
                    DoPickup(index, objList, obj);
                }
            }
            return true;
        }

        /// <summary>
        /// Handles pickup up stacks of objects which need to be split
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerator DoPickupQty(int index, uwObject[] objList, uwObject obj)
        {
            MessageDisplay.WaitingForTypedInput = true;

            uimanager.instance.TypedInput.Text = obj.ObjectQuantity.ToString();
            uimanager.instance.scroll.Clear();
            uimanager.AddToMessageScroll("Move how many? {TYPEDINPUT}|");

            while (MessageDisplay.WaitingForTypedInput)
            {
                yield return new WaitOneFrame();
            }

            var response = uimanager.instance.TypedInput.Text;
            if (int.TryParse(response, out int result))
            {
                if (result > 0)
                {
                    if (obj.ObjectQuantity <= result)
                    {//at least all of the stack is seleced
                        DoPickup(index, objList, obj);
                    }
                    else
                    {
                        //if <quantity selected, split objects, pickup object of that quantity.
                        var newObjIndex = ObjectCreator.SpawnObjectInHand(obj.item_id); //spawning in hand is very handy here
                        var newObj = UWTileMap.current_tilemap.LevelObjects[newObjIndex];
                        newObj.link = (short)result;
                        newObj.quality = obj.quality;
                        newObj.owner = obj.owner;
                        //TODO. see if other object properties need copying.                    
                        obj.link = (short)(obj.link - result);//reduce the other object.
                    }
                    yield return true;
                }
                else
                {
                    //<0
                    yield return false;
                }                
            }
            else
            {
                //invalid input. cancel                
                yield return false;
            }        
            yield return false;
        }

        private static void DoPickup(int index, uwObject[] objList, uwObject obj)
        {
            //check for pickup triggers linked to this object
            trigger.PickupTrigger(objList, obj);

            //player is trying to pick something up
            playerdat.ObjectInHand = index;
            uimanager.instance.mousecursor.SetCursorToObject(obj.item_id);

            //remove from it's tile
            var tile = UWTileMap.current_tilemap.Tiles[obj.tileX, obj.tileY];
            int nextObjectIndex = tile.indexObjectList;
            if (nextObjectIndex == index)
            {//object is first in list, easy swap
                tile.indexObjectList = obj.next;
            }
            else
            {
                while (nextObjectIndex != 0)
                {
                    var nextObj = objList[nextObjectIndex];
                    if (nextObj.next == index)
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
            obj.tileX = 99; obj.tileY = 99;
            if (obj.instance != null)
            {
                obj.instance.uwnode.Position = obj.GetCoordinate(obj.tileX, obj.tileY);
            }

            //now handle some special cases
            if (_RES != GAME_UW2)
            {
                if (obj.item_id == 458)//silver tree
                {
                    silvertree.PickupTree(obj);
                }
            }
        }        
    } //end class
}//end namespace