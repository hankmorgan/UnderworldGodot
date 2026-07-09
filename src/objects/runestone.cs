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
            ObjectCreator.CreateSprite(gr: grObjects, obj: obj, spriteNo: 224, parent: parent, name: name);
            return r;
        }

        /// <summary>
        /// Checks if item id is a runestone.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static bool IsRunestone(int item_id)
        {
            return item_id>=232 && item_id<=255;
        }
    }//end class
}//end namespace