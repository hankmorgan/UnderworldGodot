using Godot;
namespace Underworld
{
    /// <summary>
    /// Blank class for managing instances of objects.
    /// </summary>
    public abstract class objectInstance:UWClass
    {
        /// <summary>
        /// Reference to the uwobject that this is an instance of.
        /// </summary>
        public uwObject uwobject;

        /// <summary>
        /// Conversion of the object dimensions into a vector for scaling sprites
        /// </summary>
        public Vector2 FrameSize;

        /// <summary>
        /// The node used for this object.
        /// </summary>
        public Node3D uwnode;


        public static void Redraw (uwObject obj)
        {
            if (obj.instance != null)
            {
                if (obj.instance.uwnode != null)
                {
                    obj.instance.uwnode.QueueFree();
                }
                obj.instance = null;
            }
            ObjectCreator.RenderObject(
                obj: obj, 
                a_tilemap: UWTileMap.current_tilemap);
        }

    }//end class
}//end namespace