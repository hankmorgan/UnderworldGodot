using Godot;
namespace Underworld
{
    public class runestone:objectInstance
    {
        public runestone(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }
        public static runestone CreateInstance(Node3D parent, uwObject obj, GRLoader grObjects)
        {
            var r = new runestone(obj);
            if (obj.isInventory)
            {
                 ObjectCreator.CreateSprite(grObjects, obj.item_id, parent);
            }
            else
            {//Use default rune sprite when displayed in world
                ObjectCreator.CreateSprite(grObjects, 224,parent);
            }
           
            return r;
        }
    }//end class
}//end namespace