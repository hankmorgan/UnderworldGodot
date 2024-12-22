using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM : UWClass
    {   
        public static void do_inv_delete(uwObject talker)
        {
            var arg0 = at(at(stackptr-1));
            Debug.Print($"do_inv_delete {arg0}");
            
            var next = talker.link;
            var previous = 0;
            while (next!=0)
            {
                var nextObject = UWTileMap.current_tilemap.LevelObjects[next];
                if(nextObject.item_id == arg0)
                {
                    if (previous==0)
                        {//deleting from talkers link
                            talker.link = nextObject.next;
                            nextObject.next = 0;                            
                        }
                    else
                        {//deleting from next
                            var previousObj = UWTileMap.current_tilemap.LevelObjects[previous];
                            previousObj.next = nextObject.next;
                            nextObject.next = 0;
                        }
                    ObjectFreeLists.ReleaseFreeObject(nextObject);
                    result_register = 1;
                    return;
                }
                previous = nextObject.index;
                next = nextObject.next;
            }
            result_register = 0;
        }
    }//end class
}//end namespace