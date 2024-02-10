using Godot;
namespace Underworld
{
    public class runestone:objectInstance
    {
        public runestone(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }
        public static runestone CreateInstance(Node3D parent, uwObject obj, GRLoader grObjects, string name)
        {
            var r = new runestone(obj);
            if (obj.isInventory)
            {
                ObjectCreator.CreateSprite(grObjects, obj.item_id, parent, name);
            }
            else
            {//Use default rune sprite when displayed in world
                ObjectCreator.CreateSprite(grObjects, 224, parent, name);
            }
            return r;
        }
    }//end class
}//end namespace