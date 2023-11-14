using System.Collections.Generic;

namespace Underworld
{
    public class container : objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (WorldObject)
            {
                //container used in the world
                return false;
            }
            else
            {
                //container use in inventory. Browse into it.
                if (obj.classindex<=0xB)
                {
                    //set to opened version by setting bit 0 to 1.
                    obj.item_id |= 0x1;
                    if ((playerdat.OpenedContainer>=0) && (playerdat.OpenedContainer<=10))
                    {
                        //redraw slot.
                        uimanager.RefreshSlot(uimanager.CurrentSlot, playerdat.isFemale);
                    }
                }
                playerdat.OpenedContainer = obj.index;
                uimanager.SetOpenedContainer(obj.index, uwObject.GetObjectSprite(obj));

                var objects = GetObjects(obj.index,playerdat.InventoryObjects);
                for (int o = 0; o<=objects.GetUpperBound(0);o++)
                {
                    if (objects[o]!=-1)
                    {
                        //render object at this slot
                        var objFound = playerdat.InventoryObjects[objects[o]];
                        uimanager.SetBackPack(o, uwObject.GetObjectSprite(objFound));
                        playerdat.SetBackPackIndex(o,objFound);
                    }
                    else
                    {
                        uimanager.SetBackPack(o, -1);
                        playerdat.SetBackPackIndex(o, null);
                    }
                }
                return true;
            }   
        }

        /// <summary>
        /// Closes the container on the paperdoll
        /// </summary>
        /// <param name="obj"></param>
        public static void Close(int index, uwObject[] objList)
        {
            var obj = objList[index];
            if (obj==null){return;}
            //Check the paperdoll
            for (int p = 0; p<19; p++)
            {
               if (playerdat.GetInventorySlotListHead(p) == obj.index)
               {
                if(obj.item_id<=0xB)
                {//return to closed version of the container.
                    obj.item_id &=0x1fe;
                }
                
                playerdat.OpenedContainer=-1;//
                uimanager.SetOpenedContainer(obj.index, -1);
                //Draw the paperdoll inventory.
                for (int i=0; i<8;i++)
                    {
                        uimanager.SetBackPack(i,uwObject.GetObjectSprite(playerdat.BackPackObject(i)) );
                    }	
                return;
               }
            }
            foreach (var objToCheck in playerdat.InventoryObjects)
            {
               
            }
        }

        /// <summary>
        /// Returns an array of object indices at the specified offset and length
        /// </summary>
        /// <param name="ContainerIndex"></param>
        /// <param name="objList"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int[] GetObjects(int ContainerIndex, uwObject[] objList, int start = 0, int count = 8)
        {
            var Objects = ListObjects(ContainerIndex, objList);
            var output = new int[count];
            int i = 0;
            for (int o = start; o < start + count; o++)
            {
                if (o <= Objects.GetUpperBound(0))
                {
                    if (i < count)
                    {
                        output[i++] = Objects[o];
                    }
                    else
                    {
                        output[i++] = -1;
                    }
                }
                else
                {
                    output[i++] = -1;
                }
            }
            return output;
        }

        /// <summary>
        /// Returns a list of all objects in the container main level
        /// </summary>
        /// <param name="Container"></param>
        /// <returns></returns>
        public static int[] ListObjects(int ContainerIndex, uwObject[] objList)
        {
            var Container = objList[ContainerIndex];
            if (Container == null)
            {
                return null;
            }
            if (Container.link != 0)
            {
                var OutputList = new List<int>();
                var nextObj = objList[Container.link];
                while (nextObj != null)
                {
                    OutputList.Add(nextObj.index);
                    if (nextObj.next != 0)
                    {
                        nextObj = objList[nextObj.next];
                    }
                    else
                    {
                        nextObj = null;
                    }
                }
                return OutputList.ToArray();
            }
            return null;
        }
    }
}//end namespace