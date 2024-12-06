using Godot;
namespace Underworld
{
    /// <summary>
    /// Blank class for managing instances of objects.
    /// </summary>
    public abstract class objectInstance : UWClass
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


        /// <summary>
        /// Forces redraw of an object
        /// </summary>
        /// <param name="obj"></param>
        public static void RedrawFull(uwObject obj)
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

        /// <summary>
        /// Forces object to move nodes to its updated position
        /// </summary>
        /// <param name="obj"></param>
        public static void Reposition(uwObject obj)
        {
            if (obj.instance != null)
            {
                if (obj.instance.uwnode != null)
                {
                    obj.instance.uwnode.Position = obj.GetCoordinate(obj.tileX, obj.tileY);
                }
            }
        }

    }//end class
}//end namespace