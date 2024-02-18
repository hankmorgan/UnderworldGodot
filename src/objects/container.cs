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
                //container used in inventory. Browse into it.
                if (obj.classindex <= 0xB)
                {
                    //set to opened version by setting bit 0 to 1.
                    obj.item_id |= 0x1;
                }
                uimanager.OpenedContainerIndex = obj.index;
                uimanager.SetOpenedContainer(obj.index, uwObject.GetObjectSprite(obj));
                uimanager.BackPackStart = 0;
                DisplayContainerObjects(obj);
                return true;
            }
        }

        /// <summary>
        /// Displays the range (start to start+count) of container objects on the paper doll backback slots
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        public static void DisplayContainerObjects(uwObject obj, int start = 0, int count = 8)
        {
            int occupiedslots = 0;
            var objects = GetObjects(
                ContainerIndex: obj.index, 
                objList: playerdat.InventoryObjects,
                OccupiedSlots: out occupiedslots,
                start:start,
                count:count
                );

            if (objects==null)
            {
                //empty container
                for (int o=0;o<8;o++)
                {
                    uimanager.SetBackPackIndex(o, null);
                }
            }
            else
            {
                for (int o = 0; o <= objects.GetUpperBound(0); o++)
                {
                    if (objects[o] != -1)
                    {
                        //render object at this slot
                        var objFound = playerdat.InventoryObjects[objects[o]];
                        uimanager.SetBackPackIndex(o, objFound);
                    }
                    else
                    {
                        uimanager.SetBackPackIndex(o, null);
                    }
                }
            }

            uimanager.EnableDisable(uimanager.instance.ArrowUp, start !=0);
            uimanager.EnableDisable(uimanager.instance.ArrowDown, occupiedslots == 8);

            uimanager.UpdateInventoryDisplay();
        }

        /// <summary>
        /// Closes the container on the paperdoll
        /// </summary>
        /// <param name="obj"></param>
        public static void Close(int index, uwObject[] objList)
        {
            var obj = objList[index];
            if (obj == null) { return; }
            if (obj.classindex <= 0xB)
                {//return to closed version of the container.
                    obj.item_id &= 0x1fe;                        
                }
            //Check the paperdoll
            for (int p = 0; p < 19; p++)
            {
                if (playerdat.GetInventorySlotListHead(p) == obj.index)
                { //object is on the paperdoll. I can close and return to the top level
                    uimanager.RefreshSlot(p);
                    uimanager.OpenedContainerIndex = -1;//clear slot graphics
                    uimanager.SetOpenedContainer(obj.index, -1);
                    //Draw the paperdoll inventory.
                    for (int i = 0; i < 8; i++)
                    {
                        uimanager.SetBackPackArt(i, uwObject.GetObjectSprite(playerdat.BackPackObject(i)), uwObject.GetObjectQuantity(playerdat.BackPackObject(i)));
                        uimanager.SetBackPackIndex(i, playerdat.BackPackObject(i));
                    }
                    uimanager.EnableDisable(uimanager.instance.ArrowUp, false);
                    uimanager.EnableDisable(uimanager.instance.ArrowDown, false);
                    return;
                }
            }
            foreach (var objToCheck in playerdat.InventoryObjects)
            {//if this far down then I need to find the container that the closing container sits in
                if (objToCheck!=null)
                {
                    var result = objectsearch.GetContainingObject(
                        ListHead:objToCheck.index, 
                        ToFind: uimanager.OpenedContainerIndex,
                        objList: playerdat.InventoryObjects);
                    if (result!=-1)
                    {//container found. Browse into it by using it
                        Use(objList[result],false);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Returns an array of object indices at the specified offset and length
        /// </summary>
        /// <param name="ContainerIndex"></param>
        /// <param name="objList"></param>
        /// <param name="OccupiedSlots">No of objects still in this list after start+count</param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int[] GetObjects(int ContainerIndex, uwObject[] objList, out int OccupiedSlots, int start = 0, int count = 8)
        {
            var Objects = ListObjects(ContainerIndex, objList);            
            OccupiedSlots = 0;
            if (Objects==null)
                {
                return null;
                }
            var output = new int[count];
            int i = 0;
            for (int o = start; o < start + count; o++)
            {
                if (o <= Objects.GetUpperBound(0))
                {
                    if (i < count)
                    {
                        output[i++] = Objects[o];
                        OccupiedSlots++;
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