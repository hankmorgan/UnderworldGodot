using Godot;
namespace Underworld
{
    /// <summary>
    /// Blank class for managing instances of objects.
    /// </summary>
    public class objectInstance:UWClass
    {
        /// <summary>
        /// Reference to the uwobject that this is an instance of.
        /// </summary>
        public uwObject uwobject;

        /// <summary>
        /// Conversion of the object dimensions into a vector for scaling sprites
        /// </summary>
        public Vector2 FrameSize;
    }

}