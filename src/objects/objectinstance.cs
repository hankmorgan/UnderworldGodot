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
                    var adjust = Vector3.Zero;
                    if ((obj.IsStatic == false) && (obj.majorclass != 1))
                    {
                        var x_adj = 0f;
                        var y_adj = 0f;
                        var z_adj = 0f;
                        if ((obj.CoordinateX & 0x1F) != 0)
                        {
                            x_adj = 0.0046875f * ((float)(obj.CoordinateX & 0x1F));
                        }
                        if ((obj.CoordinateY & 0x1F) != 0)
                        {
                            y_adj = 0.0046875f * ((float)(obj.CoordinateY & 0x1F));
                        }
                        if ((obj.CoordinateZ & 0x7) != 0)
                        {
                            z_adj = (float)(0.001875f * (float)(obj.CoordinateZ & 0x7));
                        }
                        adjust = new Vector3(
                            x: -x_adj,
                            z: y_adj,
                            y: z_adj);
                    }
                    obj.instance.uwnode.Position = adjust + obj.GetCoordinate(obj.tileX, obj.tileY);
                }
            }
            else
            {
                //has no instance. Force a full redrawl.
                if (UWTileMap.ValidTile(obj.tileX, obj.tileY))
                {
                    if (obj == playerdat.playerObject)
                    {
                        return;
                    }
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
            toPlace.zpos = (short)(Tile.floorHeight << 3);
            Reposition(toPlace);
        }

    }//end class
}//end namespace