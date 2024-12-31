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
            else
            {
                //has no instance. Force a full redrawl.
                if (UWTileMap.ValidTile(obj.tileX, obj.tileY))
                {
                    RedrawFull(obj);
                }
            }
        }

        public static void RefreshSprite(uwObject objToRefresh)
        {//assumes sprite to sprite refresh
            if (objToRefresh.instance != null)
            {
                if (objToRefresh.instance.uwnode != null)
                {
                    var nd = (uwMeshInstance3D)objToRefresh.instance.uwnode.GetChild(0);
                    if (nd != null)
                    {
                        nd.Mesh.SurfaceSetMaterial(0, ObjectCreator.grObjects.GetMaterial(objToRefresh.item_id));
                    }
                }
            }
        }

        public static void PlaceObjectInTile(int tileX, int tileY, uwObject toPlace)
        {
            var Tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
            toPlace.next = Tile.indexObjectList;
            Tile.indexObjectList = toPlace.index;
            toPlace.tileX = tileX; 
            toPlace.tileY = tileY;
            toPlace.zpos = (short)(Tile.floorHeight<<3);
            Reposition(toPlace);
        }

    }//end class
}//end namespace