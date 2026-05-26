using Godot;
namespace Underworld
{
    public class glowing_rock : objectInstance
    {
        public static bool Use(uwObject obj, uwObject UsingObjectOrCharacter, bool WorldObject)
        {
            if (!WorldObject)
            {
                return false;
            }
            if (UsingObjectOrCharacter == null)
            {
                return true;
            }
            if (UsingObjectOrCharacter.index != 1)
            {
                return true;//just in case Blinky, Pinky, Inky and Clyde happen to collide and cause this interaction to occur! Or if you throw an object at the rock
            }
            bool DestroyZanium = false;
            var zanium = objectsearch.FindMatchInFullObjectList(4, 2, 9, playerdat.InventoryObjects);
            if (zanium != null)
            {    //player has zanium. add to the pile.
                zanium.link++;
                uimanager.UpdateInventoryDisplay();
                DestroyZanium = true;
            }
            else
            {
                //try object in hand
                if (playerdat.ObjectInHand != -1)
                {
                    var objInHand = UWTileMap.current_tilemap.LevelObjects[playerdat.ObjectInHand];
                    if (objInHand!=null)
                    {
                        //increase counter on object.
                        objInHand.link++;
                        DestroyZanium = true;
                    }
                }
            }
            if (DestroyZanium)
            {
                
                if (obj !=null)
                {
                    ObjectRemover_OLD.DeleteObjectFromTile_DEPRECIATED(obj.tileX,obj.tileY,obj.index);
                }               
            }
            return true;
        }

        //public glowing_rock_collision collision;

        // public glowing_rock(uwObject _uwobject)
        // {
        //     uwobject = _uwobject; 
        //     _uwobject.instance = this;
        // }

        // public static bool CreateGlowingRock(GRLoader grObjects, uwObject obj, Node3D parent, string name)
        // {
        //     var rock = new glowing_rock(obj);        
        //     var h = ((float)commonObjDat.height(obj.item_id) / 128f) * 0.15f;
        //     h = h * 10; //increase size a bit. not sure what height it should be, but a bigger box makes it easier to test.
        //     var r = ((float)commonObjDat.radius(obj.item_id) / 8f) * 1.2f;
        //     // rock.collision = new glowing_rock_collision();
        //     // rock.collision.uwObjectIndex = obj.index;
        //     // var col = new CollisionShape3D();
        //     // var box = new BoxShape3D();
        //     // box.Set("extents", new Vector3(r, h, r));
        //     // col.Shape = box;
        //     // rock.collision.AddChild(col);
        //     // parent.AddChild(rock.collision);
        //     // rock.collision.BodyEntered += rock.collision.movetrigger_entered;

        //     //add a sprite
        //     ObjectCreator.CreateSpriteInstance(grObjects,obj,parent,name);
        //     return true;
        // }
    }
}