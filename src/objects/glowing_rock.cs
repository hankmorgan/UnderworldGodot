using Godot;
namespace Underworld
{
    public class glowing_rock : objectInstance
    {
        public glowing_rock_collision collision;

        public glowing_rock(uwObject _uwobject)
        {
            uwobject = _uwobject; 
            _uwobject.instance = this;
        }

        public static bool CreateGlowingRock(GRLoader grObjects, uwObject obj, Node3D parent, string name)
        {
            var rock = new glowing_rock(obj);        
            var h = ((float)commonObjDat.height(obj.item_id) / 128f) * 0.15f;
            h = h * 10; //increase size a bit. not sure what height it should be, but a bigger box makes it easier to test.
            var r = ((float)commonObjDat.radius(obj.item_id) / 8f) * 1.2f;
            rock.collision = new glowing_rock_collision();
            rock.collision.uwObjectIndex = obj.index;
            var col = new CollisionShape3D();
            var box = new BoxShape3D();
            box.Set("extents", new Vector3(r, h, r));
            col.Shape = box;
            rock.collision.AddChild(col);
            parent.AddChild(rock.collision);
            rock.collision.BodyEntered += rock.collision.movetrigger_entered;

            //add a sprite
            ObjectCreator.CreateSpriteInstance(grObjects,obj,parent,name);
            return true;
        }
    }
}