using Godot;
namespace Underworld
{
    public partial class trigger:UWClass
    {
        public static bool CreateMoveTrigger(uwObject obj, Node3D parent)
        {
            var h = ((float)commonObjDat.height(obj.item_id)/128f)  * 0.15f;
            var r = ((float)commonObjDat.radius(obj.item_id)/8f) * 1.2f;
            var ar = new movetrigger();
            ar.uwObjectIndex = obj.index;
            var col = new CollisionShape3D();
            var box = new BoxShape3D();
            box.Set("extents", new Vector3(r,h,r));
            col.Shape = box;
            ar.AddChild(col);
            var del = new movetrigger.MoveTriggerEnteredEventHandler(movetrigger.body_entered);
            ar.Connect(signal: 
                "body_entered", 
                callable: new Callable(ar, nameof(del)));
            parent.AddChild(ar);
            return true;
        }

        // [Signal]
        // public delegate void MoveTriggerEventHandler(int obj);

        // public void MoveTriggerEventHandler(int obj)
        // {
        //     Debug.Print(obj);
        // }


    }//end class    
}//end namespace
