namespace Underworld
{
    /// <summary>
    /// Class for managing the object free lists
    /// </summary>
    public class ObjectFreeLists : UWClass
    {
        public enum ObjectListType
        {
            StaticList = 0,
            MobileList = 1
        };

        /// <summary>
        /// Gets an object slot that can be allocated for a new object
        /// </summary>
        /// <param name="WhichList"></param>
        /// <returns></returns>
        public static int GetAvailableObjectSlot(ObjectListType WhichList = ObjectListType.StaticList)
        {
            //look up object free list
            switch (WhichList)
            {
                case ObjectListType.StaticList:
                    //Move PTR down, get object at that point.
                    if (UWTileMap.current_tilemap.StaticFreeListPtr<=1)
                    {
                        return 0;
                    }
                    UWTileMap.current_tilemap.StaticFreeListPtr--;
                    //Debug.Print($"Allocating Static {UWTileMap.current_tilemap.StaticFreeListObject} Pointer decremented to {UWTileMap.current_tilemap.StaticFreeListPtr}");
                    return UWTileMap.current_tilemap.StaticFreeListObject;
                case ObjectListType.MobileList:
                    if (UWTileMap.current_tilemap.MobileFreeListPtr<=1)
                    {
                        return 0;
                    }
                    UWTileMap.current_tilemap.MobileFreeListPtr--;
                    //Debug.Print($"Allocating Mobile {UWTileMap.current_tilemap.MobileFreeListObject} Pointer decremented to {UWTileMap.current_tilemap.MobileFreeListPtr}");
                    //add to the active mobiles list                    
                    var newslot = UWTileMap.current_tilemap.MobileFreeListObject;
                    UWTileMap.current_tilemap.SetActiveMobileAtIndex(UWTileMap.current_tilemap.NoOfActiveMobiles, newslot);
                    UWTileMap.current_tilemap.NoOfActiveMobiles++;
                    return newslot;

            }
            return 0;
        }

        /// <summary>
        /// Removes object from the game world and allocates free ptrs Does not update object chains
        /// </summary>
        /// <param name="obj"></param>
        public static void ReleaseFreeObject(uwObject obj)
        {
            //remove from world
            if (obj.index < 256)
            {//mobile
                UWTileMap.current_tilemap.MobileFreeListObject = obj.index;
                UWTileMap.current_tilemap.MobileFreeListPtr++;
                //Debug.Print($"Freeing Mobile {obj.index} Pointer incremented to {UWTileMap.current_tilemap.MobileFreeListPtr}");
                for (int i = 0; i < UWTileMap.current_tilemap.NoOfActiveMobiles; i++)
                {
                    var atSlot = UWTileMap.current_tilemap.GetActiveMobileAtIndex(i);
                    if (atSlot == obj.index)
                    {
                        UWTileMap.current_tilemap.NoOfActiveMobiles--;
                        if (i < UWTileMap.current_tilemap.NoOfActiveMobiles)
                        {
                            var atEnd = UWTileMap.current_tilemap.GetActiveMobileAtIndex(UWTileMap.current_tilemap.NoOfActiveMobiles);
                            //shift down the object at the end of this list
                            UWTileMap.current_tilemap.SetActiveMobileAtIndex(i, atEnd);
                        }
                        break;
                    }
                }
            }
            else
            {//static
                UWTileMap.current_tilemap.StaticFreeListObject = obj.index;
                UWTileMap.current_tilemap.StaticFreeListPtr++;
                //Debug.Print($"Freeing Static {obj.index} {obj.a_name} Pointer incremented to {UWTileMap.current_tilemap.StaticFreeListPtr}");
            }

            if (obj.instance != null)
            {
                if (obj.instance.uwnode != null)
                {
                    obj.instance.uwnode.QueueFree();
                }
                obj.instance = null;
            }
            obj.item_id = 0;//set to default
                            // obj.link = 0; obj.next = 0; //force remove from chains. Full updates to chains should have been done before calling this function
            obj.tileX = 99; obj.tileY = 99;
        }


    }
}
