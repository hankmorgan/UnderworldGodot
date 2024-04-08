using System.Diagnostics;
namespace Underworld
{
    public partial class trigger : UWClass
    {
        
        /// <summary>
        /// Check for pickup trigger in the object chain (container). If one found trigger it first and remove it from the chain.
        /// </summary>
        /// <param name="objList"></param>
        /// <param name="obj"></param>
        public static void PickupTrigger(uwObject[] objList, uwObject obj)
        {            
            var index = obj.index;
            var PickupTrigger = objectsearch.FindMatchInObjectChain(index, 6, 2, 1, objList, true);
            if (PickupTrigger != null)
            {
                trigger.StartTriggerChainEvents();
                Debug.Print($"Pickup trigger found {PickupTrigger.index} -> {PickupTrigger.link}");
                trap.ActivateTrap(
                    triggerObj: PickupTrigger,
                    trapIndex: PickupTrigger.link,
                    objList: objList);

                bool FoundTriggerPrevious = false;
                //Find it's in the object chain
                if (obj.link == PickupTrigger.index && obj.is_quant == 0)
                {//check first object that is linked to this container.
                    obj.link = PickupTrigger.next;
                    FoundTriggerPrevious = true;
                }
                else
                {//search in the object chain to find it. Assumes first level of object chain.
                    var next = obj.link;
                    while (next != 0 && !FoundTriggerPrevious)
                    {
                        var nextObj = objList[next];
                        if (nextObj.next == PickupTrigger.index)
                        {
                            FoundTriggerPrevious = true;
                            nextObj.next = PickupTrigger.next;
                        }
                        next = nextObj.next;
                    }
                }
                if (FoundTriggerPrevious)
                {
                    ObjectCreator.RemoveObject(PickupTrigger);
                }
                else
                {
                    Debug.Print("Unable to find the pickup trigger to remove it.");
                }
            }
            trigger.EndTriggerChainEvents();
        }
    }//end class
}//end namespace