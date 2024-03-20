using Godot;
using System.Collections.Generic;
using System.Linq;


namespace Underworld
{


    /// <summary>
    /// For drawing the level map. It works somehow..
    /// </summary>
    public class tileMapRender : UWClass
    {

        public static Node3D worldnode;
        const int TILE_SOLID = 0;
        const int TILE_OPEN = 1;

        //Note the order of these 4 tiles are actually different in SHOCK. I swap them around in BuildTileMapShock for consistancy

        const int TILE_DIAG_SE = 2;
        const int TILE_DIAG_SW = 3;
        const int TILE_DIAG_NE = 4;
        const int TILE_DIAG_NW = 5;

        const int TILE_SLOPE_N = 6;
        const int TILE_SLOPE_S = 7;
        const int TILE_SLOPE_E = 8;
        const int TILE_SLOPE_W = 9;

        //Visible faces indices
        const int vTOP = 0;
        const int vEAST = 1;
        const int vBOTTOM = 2;
        const int vWEST = 3;
        const int vNORTH = 4;
        const int vSOUTH = 5;


        //BrushFaces
        const int fSELF = 128;
        const int fCEIL = 64;
        const int fNORTH = 32;
        const int fSOUTH = 16;
        const int fEAST = 8;
        const int fWEST = 4;


        //headings in UW go clockwise from 9 o'clock.
        //

        public const int heading0 = 0; //in uw PI
        public const int heading1 = 45; // uw 3PI/4
        public const int heading2 = 90;  //in uw PI/2
        public const int heading3 = 135;  //PI/4     
        public const int heading4 = 180; // in uw 0
        public const int heading5 = 225; // 7PI/4
        public const int Heading6 = 270;// in uw 3PI/2
        public const int Heading7 = 315; // 5PI/4
 
 
        public static bool EnableCollision = true;
        public static bool SkipRender = false;

        //static int UW_CEILING_HEIGHT;
        static int CEILING_HEIGHT;

        const int CEIL_ADJ = 0;
        const int FLOOR_ADJ = 0; //-2;

        public static TextureLoader mapTextures;

        static tileMapRender()
        {
            if (mapTextures==null)
            {
                mapTextures = new();
            }
        }

        public static void GenerateLevelFromTileMap(Node3D parent, UWTileMap Level, uwObject[] objList, bool UpdateOnly)
        {
            worldnode = parent;
            CEILING_HEIGHT = UWTileMap.UW_CEILING_HEIGHT;

            if (!UpdateOnly)
            {
                //Clear out the children in the transform
                foreach (var child in parent.GetChildren())
                {
                    child.QueueFree();
                }
            }
            if (SkipRender){return;}
            for (int y = 0; y <= UWTileMap.TileMapSizeY; y++)
            {
                for (int x = 0; x <= UWTileMap.TileMapSizeX; x++)
                {
                    RenderTile(parent, x, y, Level.Tiles[x, y]);
                }
            }

            RenderCeiling(parent, 0, 0, CEILING_HEIGHT, CEILING_HEIGHT + CEIL_ADJ, Level.UWCeilingTexture, "CEILING", Level);
        }



        /// <summary>
        /// Renders the a tile
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="t">T.</param>
        public static Node3D RenderTile(Node3D parent, int x, int y, TileInfo t)
        {                      
            //Picks the tile to render based on tile type/flags.
            switch (t.tileType)
            {
                case TILE_SOLID:    //0
                    {   //solid                       
                        return RenderSolidTile(parent, x, y, t);
                    }
                case TILE_OPEN:     //1
                    {//open
                        return RenderOpenTile(parent, x, y, t);    //floor
                    }
                case TILE_DIAG_SE:
                    {//diag se
                       RenderDiagSETile(parent, x, y, t); //floor                      
                        return null;
                    }

                case TILE_DIAG_SW:
                    {   //diag sw
                        RenderDiagSWTile(parent, x, y, t); //floor
                        return null;
                    }

                case TILE_DIAG_NE:
                    {   //diag ne
                        RenderDiagNETile(parent, x, y, t); //floor
                        return null;
                    }

                case TILE_DIAG_NW:
                    {//diag nw
                        RenderDiagNWTile(parent, x, y, t); //floor
                        return null;
                    }

                case TILE_SLOPE_N:  //6
                    {//slope n
                        RenderSlopeNTile(parent, x, y, t); //floor
                        return null;
                    }
                case TILE_SLOPE_S: //slope s	7
                    {
                        RenderSlopeSTile(parent, x, y, t);  //floor
                        return null;
                    }
                case TILE_SLOPE_E:      //slope e 8	
                    {
                        RenderSlopeETile(parent, x, y, t);//floor
                        return null;
                    }
                case TILE_SLOPE_W:  //9
                    { //slope w                        
                        RenderSlopeWTile(parent, x, y, t); //floor                                  
                        return null;
                    }                    
            }
            return null;
        }


        static Node3D RenderSolidTile(Node3D parent, int x, int y, TileInfo t)
        {
            if (t.Render == true)
            {
                string TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");
                t.VisibleFaces[vTOP] = false;
                t.VisibleFaces[vBOTTOM] = false;
                return RenderCuboid(parent, x, y, t, FLOOR_ADJ, CEILING_HEIGHT + CEIL_ADJ, TileName);
            }
            return null;
        }


        /// <summary>
        /// Renders an open tile with no slopes
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="t">T.</param>
        /// <param name="Water">If set to <c>true</c> water.</param>
        /// <param name="invert">If set to <c>true</c> invert.</param>
        static Node3D RenderOpenTile(Node3D parent, int x, int y, TileInfo t)
        {
            if (t.Render == true)
            {
                string TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");

                //Bottom face 
                if (t.TerrainChange)
                {                    
                    return RenderCuboid(parent, x, y, t, -16, t.floorHeight, TileName);
                }
                else
                {
                    TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");
                    return RenderCuboid(parent, x, y, t, 0, t.floorHeight, TileName);
                }
            }
            return null;
        }

        /// <summary>
        /// Renders the a cuboid with no slopes
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="t">T.</param>
        /// <param name="Bottom">Bottom.</param>
        /// <param name="Top">Top.</param>
        /// <param name="TileName">Tile name.</param>
        static Node3D RenderCeiling(Node3D parent, int x, int y, int Bottom, int Top, int CeilingTexture, string TileName, Underworld.UWTileMap map)
        {
            //return null;

            //Draw a cube with no slopes.
            int NumberOfVisibleFaces = 1;

            //Allocate enough verticea and UVs for the faces
            Vector3[] verts = new Vector3[NumberOfVisibleFaces * 4];
            Vector2[] uvs = new Vector2[NumberOfVisibleFaces * 4];
            float floorHeight = (float)(Top * 0.15f);
            float baseHeight = (float)(Bottom * 0.15f);

            //Now create the mesh
            var a_mesh = new ArrayMesh();

            int[] MatsToUse = new int[NumberOfVisibleFaces];
            //Now allocate the visible faces to triangles.
            int FaceCounter = 0;//Tracks which number face we are now on.
            float PolySize = Top - Bottom;
            float uv0 = (float)(Bottom * 0.125f);
            float uv1 = -(PolySize / 8.0f) + (uv0);
            // float offset = 0f;

            //bottom wall vertices
            MatsToUse[FaceCounter] = map.texture_map[CeilingTexture];
            verts[0 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 1.2f * 64);
            verts[1 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 0f);
            verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * 64, baseHeight, 0f);
            verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * 64, baseHeight, 1.2f * 64);
            //Change default UVs
            uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, 0.0f);
            uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, 1.0f * 64);
            uvs[2 + (4 * FaceCounter)] = new Vector2(64, 1.0f * 64);
            uvs[3 + (4 * FaceCounter)] = new Vector2(64, 0.0f);

            var normals = new List<Vector3>();
            foreach (var vert in verts)
            {
                normals.Add(vert.Normalized());
            }

            //Apply the uvs and create my tris
            // mesh.vertices = verts;
            // mesh.uv = uvs;
            FaceCounter = 0;
            int[] indices = new int[6];
            indices[0] = 0 + (4 * FaceCounter);
            indices[1] = 1 + (4 * FaceCounter);
            indices[2] = 2 + (4 * FaceCounter);
            indices[3] = 0 + (4 * FaceCounter);
            indices[4] = 2 + (4 * FaceCounter);
            indices[5] = 3 + (4 * FaceCounter);
            //mesh.SetTriangles(indices, FaceCounter);
            AddSurfaceToMesh(verts, uvs, MatsToUse, 0, a_mesh, normals, indices);

            return CreateMeshInstance(parent, x, y, TileName, a_mesh);
        }

        private static Node3D CreateMeshInstance(Node3D parent, int x, int y, string TileName, ArrayMesh a_mesh)
        {
            var final_mesh = new MeshInstance3D();
            parent.AddChild(final_mesh);
            final_mesh.Position = new Vector3(x * -1.2f, 0.0f, y * 1.2f);
            final_mesh.Name = TileName;
            final_mesh.Mesh = a_mesh;
            if (EnableCollision)
            {
                //final_mesh.CreateConvexCollision(clean: false);
                final_mesh.CreateTrimeshCollision();
            }
            return final_mesh;
        }


        static void RenderDiagSETile(Node3D parent, int x, int y, TileInfo t)
        {
            //int BLeftX; int BLeftY; int BLeftZ; int TLeftX; int TLeftY; int TLeftZ; int TRightX; int TRightY; int TRightZ;

            if (t.Render == true)
            {
                //the wall part
                string TileName = "Wall_" + x.ToString("D2") + "_" + y.ToString("D2");
                RenderDiagSEPortion(parent, FLOOR_ADJ, CEILING_HEIGHT + CEIL_ADJ, t, TileName);
            
                //it's floor
                //RenderDiagNWPortion( FLOOR_ADJ, t.floorHeight, t,"DiagNW1");
                bool PreviousNorth = t.VisibleFaces[vNORTH];
                bool PreviousWest = t.VisibleFaces[vWEST];
                t.VisibleFaces[vNORTH] = false;
                t.VisibleFaces[vWEST] = false;
                RenderDiagOpenTile(parent, x, y, t);
                t.VisibleFaces[vNORTH] = PreviousNorth;
                t.VisibleFaces[vWEST] = PreviousWest;                    
            }
            return;
        }

        /// <summary>
        /// Renders the diag SW tile.
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="t">T.</param>
        /// <param name="Water">If set to <c>true</c> water.</param>
        /// <param name="invert">If set to <c>true</c> invert.</param>
        static void RenderDiagSWTile(Node3D parent, int x, int y, TileInfo t)
        {
            if (t.Render == true)
            {
                //Its wall
                string TileName = "Wall_" + x.ToString("D2") + "_" + y.ToString("D2");
                RenderDiagSWPortion(parent, FLOOR_ADJ, CEILING_HEIGHT + CEIL_ADJ, t, TileName);
                //it's floor
                bool PreviousNorth = t.VisibleFaces[vNORTH];
                bool PreviousEast = t.VisibleFaces[vEAST];
                t.VisibleFaces[vNORTH] = false;
                t.VisibleFaces[vEAST] = false;
                RenderDiagOpenTile(parent, x, y, t);
                t.VisibleFaces[vNORTH] = PreviousNorth;
                t.VisibleFaces[vEAST] = PreviousEast; 
            }
            return;
        }


        /// <summary>
        /// Renders the diag NE tile.
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="t">T.</param>
        /// <param name="Water">If set to <c>true</c> water.</param>
        /// <param name="invert">If set to <c>true</c> invert.</param>
        static void RenderDiagNETile(Node3D parent, int x, int y, TileInfo t)
        {
            if (t.Render == true)
            {
                string TileName = "Wall_" + x.ToString("D2") + "_" + y.ToString("D2");
                RenderDiagNEPortion(parent, FLOOR_ADJ, CEILING_HEIGHT + CEIL_ADJ, t, TileName);
                //it's floor
                bool PreviousSouth = t.VisibleFaces[vSOUTH];
                bool PreviousWest = t.VisibleFaces[vWEST];
                t.VisibleFaces[vSOUTH] = false;
                t.VisibleFaces[vWEST] = false;
                RenderDiagOpenTile(parent, x, y, t);
                t.VisibleFaces[vSOUTH] = PreviousSouth;
                t.VisibleFaces[vWEST] = PreviousWest;
            }
            return;
        }


        /// <summary>
        /// Renders the diag NW tile.
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="t">T.</param>
        /// <param name="Water">If set to <c>true</c> water.</param>
        /// <param name="invert">If set to <c>true</c> invert.</param>
        static void RenderDiagNWTile(Node3D parent, int x, int y, TileInfo t)
        {
            if (t.Render == true)
            {
                //It's wall.
                string TileName = "Wall_" + x.ToString("D2") + "_" + y.ToString("D2");
                RenderDiagNWPortion(parent, FLOOR_ADJ, CEILING_HEIGHT + CEIL_ADJ, t, TileName);

                //it's floor
                bool PreviousSouth = t.VisibleFaces[vSOUTH];
                bool PreviousEast = t.VisibleFaces[vEAST];
                t.VisibleFaces[vSOUTH] = false;
                t.VisibleFaces[vEAST] = false;
                RenderDiagOpenTile(parent, x, y, t);
                t.VisibleFaces[vSOUTH] = PreviousSouth;
                t.VisibleFaces[vEAST] = PreviousEast;
            }
            return;
        }

        public static Node3D RenderCuboid(Node3D parent, int x, int y, TileInfo t, int Bottom, int Top, string TileName)
        {
            //Draw a cube with no slopes.
            int NumberOfVisibleFaces = 0;
            //Get the number of faces
            for (int i = 0; i < 6; i++)
            {
                if (t.VisibleFaces[i] == true)
                {
                    NumberOfVisibleFaces++;
                }
            }
            //Allocate enough vertices and UVs for the faces
            Vector3[] verts = new Vector3[NumberOfVisibleFaces * 4];
            Vector2[] uvs = new Vector2[NumberOfVisibleFaces * 4];
            float floorHeight = (float)(Top * 0.15f);
            float baseHeight = (float)(Bottom * 0.15f);
            float dimX = t.DimX;
            float dimY = t.DimY;

            int[] MatsToUse = new int[NumberOfVisibleFaces];    //was material
            //Now allocate the visible faces to triangles.
            int FaceCounter = 0;//Tracks which number face we are now on.
            float PolySize = Top - Bottom;
            float uv0 = (float)(Bottom * 0.125f);
            float uv1 = -(PolySize / 8.0f) + (uv0);  //this was positive in unity. I had to flip it to get it to work in godot. Possibly a issue from unity that I never sp
            for (int i = 0; i < 6; i++)
            {
                if (t.VisibleFaces[i] == true)
                {
                    switch (i)
                    {
                        case vTOP:
                            {
                                //Set the verts	
                                MatsToUse[FaceCounter] = FloorTexture(t);

                                verts[0 + (4 * FaceCounter)] = new Vector3(0.0f, floorHeight, 0.0f);
                                verts[1 + (4 * FaceCounter)] = new Vector3(0.0f, floorHeight, 1.2f * dimY);
                                verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 1.2f * dimY);
                                verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 0.0f);

                                //Allocate UVs
                                uvs[0 + (4 * FaceCounter)] = new Vector2(-1.0f * dimX, 0.0f);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(-1.0f * dimX, 1.0f * dimY);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(0.0f, 1.0f * dimY);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(0.0f, 0.0f);                               

                                break;
                            }

                        case vNORTH:
                            {
                                //north wall vertices                                
                                MatsToUse[FaceCounter] = WallTexture(fNORTH, t);
                                verts[0 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 1.2f * dimY);
                                verts[1 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 1.2f * dimY);
                                verts[2 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 1.2f * dimY);
                                verts[3 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 1.2f * dimY);

                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, uv1);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(dimX, uv1 );
                                uvs[3 + (4 * FaceCounter)] = new Vector2(dimX, uv0);


                                break;
                            }

                        case vWEST:
                            {
                                //west wall vertices
                                MatsToUse[FaceCounter] = WallTexture(fWEST, t);
                                verts[0 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 1.2f * dimY);
                                verts[1 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 1.2f * dimY);
                                verts[2 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 0f);
                                verts[3 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 0f);
                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, uv1);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(dimY, uv1);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(dimY, uv0);

                                break;
                            }

                        case vEAST:
                            {
                                //east wall vertices                                
                                MatsToUse[FaceCounter] = WallTexture(fEAST, t);
                                verts[0 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 0f);
                                verts[1 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 0f);
                                verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 1.2f * dimY);
                                verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 1.2f * dimY);
                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, uv1);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(dimY, uv1);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(dimY, uv0);

                                break;
                            }

                        case vSOUTH:
                            {                               
                                MatsToUse[FaceCounter] = WallTexture(fSOUTH, t);
                                //south wall vertices
                                verts[0 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 0f);
                                verts[1 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 0f);
                                verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 0f);
                                verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 0f);
                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, uv1);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(dimX, uv1);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(dimX, uv0);

                                break;
                            }
                        case vBOTTOM:
                            {
                                //bottom wall vertices
                                MatsToUse[FaceCounter] = FloorTexture(t);
                                verts[0 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 1.2f * dimY);
                                verts[1 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 0f);
                                verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 0f);
                                verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 1.2f * dimY);
                                //Change default UVs
                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, 0.0f);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, 1.0f * dimY);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(dimX, 1.0f * dimY);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(dimX, 0.0f);
                                break;
                            }
                    }
                    FaceCounter++;
                }
            }

            //Generate normals
            var normals = new List<Vector3>();
            foreach (var vert in verts)
            {
                normals.Add(vert.Normalized());
            }

            // mesh.vertices = verts;
            // mesh.uv = uvs;

            //Create the overall mesh
            var a_mesh = new ArrayMesh(); //= Mesh as ArrayMesh;

            //add indiviual surfaces to the mesh.

            FaceCounter = 0;
            int[] indices = new int[6];
            for (int i = 0; i < 6; i++)
            {
                if (t.VisibleFaces[i] == true)
                {
                    indices[0] = 0 + (4 * FaceCounter);
                    indices[1] = 1 + (4 * FaceCounter);
                    indices[2] = 2 + (4 * FaceCounter);
                    indices[3] = 0 + (4 * FaceCounter);
                    indices[4] = 2 + (4 * FaceCounter);
                    indices[5] = 3 + (4 * FaceCounter);
                    //mesh.SetTriangles(indices, FaceCounter);

                    //Create the surface.
                    AddSurfaceToMesh(verts, uvs, MatsToUse, FaceCounter, a_mesh, normals, indices);
                    FaceCounter++;
                }
            }


            return CreateMeshInstance(parent, x, y, TileName, a_mesh);
            //GetTree().Root.CallDeferred("add_child", final_mesh);



            // mr.materials = MatsToUse;//mats;
            // mesh.RecalculateNormals();
            // mesh.RecalculateBounds();
            // // mf.mesh = mesh;
            // if (EnableCollision)
            // {
            //     // MeshCollider mc = a_tile.AddComponent<MeshCollider>();
            //     // mc.sharedMesh = null;
            //     // mc.sharedMesh = mesh;
            // }
            // return final_mesh;
        }

        /// <summary>
        /// Renders the slope N tile.
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="t">T.</param>
        /// <param name="Water">If set to <c>true</c> water.</param>
        /// <param name="invert">If set to <c>true</c> invert.</param>
        static void RenderSlopeNTile(Node3D parent, int x, int y, TileInfo t)
        {
            if (t.Render == true)
            {
                string TileName;
                //A floor
                TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");
                RenderSlopedCuboid(
                    parent: parent, 
                    x: x, 
                    y: y, 
                    t: t, 
                    Bottom: FLOOR_ADJ, 
                    Top: t.floorHeight, 
                    SlopeDir: TILE_SLOPE_N, 
                    Steepness: t.TileSlopeSteepness, 
                    TileName: TileName);
            }
            return;
        }

        /// <summary>
        /// Renders the slope S tile.
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="t">T.</param>
        /// <param name="Water">If set to <c>true</c> water.</param>
        /// <param name="invert">If set to <c>true</c> invert.</param>
        static void RenderSlopeSTile(Node3D parent, int x, int y, TileInfo t)
        {
            if (t.Render == true)
            {
                string TileName;
                //A floor
                TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");
                RenderSlopedCuboid(
                    parent: parent, 
                    x: x, 
                    y: y, 
                    t: t, 
                    Bottom: FLOOR_ADJ, 
                    Top: t.floorHeight, 
                    SlopeDir: TILE_SLOPE_S, 
                    Steepness: t.TileSlopeSteepness, 
                    TileName: TileName);
            }
            return;
        }

        /// <summary>
        /// Renders the slope W tile.
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="t">T.</param>
        /// <param name="Water">If set to <c>true</c> water.</param>
        /// <param name="invert">If set to <c>true</c> invert.</param>
        static void RenderSlopeWTile(Node3D parent, int x, int y, TileInfo t)
        {
            if (t.Render == true)
            {
                string TileName;
                //A floor
                TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");
                RenderSlopedCuboid(
                    parent: parent, 
                    x: x, 
                    y: y, 
                    t: t, 
                    Bottom: FLOOR_ADJ, 
                    Top: t.floorHeight, 
                    SlopeDir: TILE_SLOPE_W, 
                    Steepness: t.TileSlopeSteepness, 
                    TileName: TileName);
            }
            return;
        }

        /// <summary>
        /// Renders the slope E tile.
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="t">T.</param>
        /// <param name="Water">If set to <c>true</c> water.</param>
        /// <param name="invert">If set to <c>true</c> invert.</param>
        static void RenderSlopeETile(Node3D parent, int x, int y, TileInfo t)
        {
            if (t.Render == true)
            {
                string TileName;
                //A floor
                TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");
                RenderSlopedCuboid(
                    parent: parent, 
                    x: x, 
                    y: y, 
                    t: t, 
                    Bottom: FLOOR_ADJ, 
                    Top: t.floorHeight, 
                    SlopeDir: TILE_SLOPE_E, 
                    Steepness: t.TileSlopeSteepness, 
                    TileName: TileName);
            }
            return;
        }



        // /// <summary>
        // /// Use to calculate texture offsets.
        // /// </summary>
        // /// <param name="face"></param>
        // /// <param name="t"></param>
        // /// <returns>0 always since this is UW and this is only needed for shock tiles</returns>
        // static float CalcCeilOffset(int face, TileInfo t)
        // {
        //     // int ceilOffset = t.ceilingHeight;

        //     // if (_RES != GAME_SHOCK)
        //     // {
        //     return 0;
        //     // }
        //     // else
        //     // {
        //     //     switch (face)
        //     //     {
        //     //         case fEAST:
        //     //             ceilOffset = t.shockEastCeilHeight; break;
        //     //         case fWEST:
        //     //             ceilOffset = t.shockWestCeilHeight; break;
        //     //         case fSOUTH:
        //     //             ceilOffset = t.shockSouthCeilHeight; break;
        //     //         case fNORTH:
        //     //             ceilOffset = t.shockNorthCeilHeight; break;
        //     //     }
        //     //     float shock_ceil = CurrentTileMap().SHOCK_CEILING_HEIGHT;
        //     //     float floorOffset = shock_ceil - ceilOffset - 8;  //The floor of the tile if it is 1 texture tall.
        //     //     while (floorOffset >= 8)  //Reduce the offset to 0 to 7 since textures go up in steps of 1/8ths
        //     //     {
        //     //         floorOffset -= 8;
        //     //     }
        //     //     return floorOffset * 0.125f;
        //     // }
        // }

        /// <summary>
        /// Gets the wall texture for the specified face
        /// </summary>
        /// <returns>The texture.</returns>
        /// <param name="face">Face.</param>
        /// <param name="t">T.</param>
        public static int WallTexture(int face, TileInfo t)
        {
            int wallTexture;
            //int ceilOffset = 0;
            wallTexture = t.wallTexture;
            switch (face)
            {
                case fSOUTH:
                    wallTexture = t.South;
                    break;
                case fNORTH:
                    wallTexture = t.North;
                    break;
                case fEAST:
                    wallTexture = t.East;
                    break;
                case fWEST:
                    wallTexture = t.West;
                    break;
            }
            if ((wallTexture < 0) || (wallTexture > 512))
            {
                wallTexture = 0;
            }
            // if (debugtextures)
            // {
            //     return wallTexture;
            // }
            return t.map.texture_map[wallTexture];
        }

        /// <summary>
        /// Returns the floor texture from the texture map.
        /// </summary>
        /// <returns>The texture.</returns>
        /// <param name="face">Face.</param>
        /// <param name="t">T.</param>
        public static int FloorTexture(TileInfo t)
        {
            int floorTexture;
            // if (debugtextures)
            // {
            //     return t.floorTexture;
            // }

                //floorTexture = t.floorTexture;
            switch (_RES)
            {
                //case GAME_SHOCK:
                case GAME_UW2:
                    floorTexture = t.map.texture_map[t.floorTexture];
                    //floorTexture = t.floorTexture;
                    break;
                default:
                    floorTexture = t.map.texture_map[t.floorTexture + 48];
                    break;
            }
            if ((floorTexture < 0) || (floorTexture > 512))
            {
                floorTexture = 0;
            }
            return floorTexture;
        }


        /// <summary>
        /// Renders the diag NE portion.
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="Bottom">Bottom.</param>
        /// <param name="Top">Top.</param>
        /// <param name="t">T.</param>
        /// <param name="TileName">Tile name.</param>
        static Node3D RenderDiagNEPortion(Node3D parent, int Bottom, int Top, TileInfo t, string TileName)
        {
            //Does a thing.
            //Does a thing.
            //Draws 3 meshes. Outward diagonal wall. Back and side if visible.

            int NumberOfVisibleFaces = 1;//Will always have the diag.
                                         //Get the number of faces
            for (int i = 0; i < 6; i++)
            {
                if ((i == vSOUTH) || (i == vWEST))
                {
                    if (t.VisibleFaces[i] == true)
                    {
                        NumberOfVisibleFaces++;
                    }
                }
            }
            //Allocate enough verticea and UVs for the faces
            int[] MatsToUse = new int[NumberOfVisibleFaces];
            Vector3[] verts = new Vector3[NumberOfVisibleFaces * 4];
            Vector2[] uvs = new Vector2[NumberOfVisibleFaces * 4];

            float floorHeight = (float)(Top * 0.15f);
            float baseHeight = (float)(Bottom * 0.15f);
            float dimX = t.DimX;
            //    //Now create the mesh
            //    GameObject Tile = new GameObject(TileName)
            //    {
            //        layer = LayerMask.NameToLayer("MapMesh")
            //    };
            //    Tile.transform.parent = parent.transform;
            //    Tile.transform.position = new Vector3(t.tileX * 1.2f, 0.0f, t.tileY * 1.2f);

            //    Tile.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            //    MeshFilter mf = Tile.AddComponent<MeshFilter>();
            //    MeshRenderer mr = Tile.AddComponent<MeshRenderer>();
            //MeshCollider mc = Tile.AddComponent<MeshCollider>();
            //mc.sharedMesh=null;

            //    Mesh mesh = new Mesh
            //    {
            //        subMeshCount = NumberOfVisibleFaces//Should be no of visible faces
            //    };

            var a_mesh = new ArrayMesh();

            //Now allocate the visible faces to triangles.
            int FaceCounter = 0;//Tracks which number face we are now on.
            float PolySize = Top - Bottom;
            float uv0 = (float)(Bottom * 0.125f);
            float uv1 = -(PolySize / 8.0f) + (uv0);
            //Set the diagonal face first
            MatsToUse[FaceCounter] = WallTexture(fSELF, t);
            verts[0] = new Vector3(-1.2f, baseHeight, 0f);
            verts[1] = new Vector3(-1.2f, floorHeight, 0f);
            verts[2] = new Vector3(0f, floorHeight, 1.2f);
            verts[3] = new Vector3(0f, baseHeight, 1.2f);

            uvs[0] = new Vector2(0.0f, uv0);
            uvs[1] = new Vector2(0.0f, uv1);
            uvs[2] = new Vector2(1, uv1);
            uvs[3] = new Vector2(1, uv0);
            FaceCounter++;

            for (int i = 0; i < 6; i++)
            {
                if ((t.VisibleFaces[i] == true) && ((i == vSOUTH) || (i == vWEST)))
                {//Will only render north or west if needed.
                 //float dimY = t.DimY;
                    switch (i)
                    {
                        case vSOUTH:
                            {
                                //south wall vertices
                                MatsToUse[FaceCounter] = WallTexture(fSOUTH, t);
                                verts[0 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 0f);
                                verts[1 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 0f);
                                verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 0f);
                                verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 0f);
                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, uv1);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(dimX, uv1);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(dimX, uv0);
                                break;
                            }

                        case vWEST:
                            {
                                //west wall vertices
                                MatsToUse[FaceCounter] = WallTexture(fWEST, t);
                                verts[0 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 1.2f);
                                verts[1 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 1.2f);
                                verts[2 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 0f);
                                verts[3 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 0f);
                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, uv1);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(1, uv1);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(1, uv0);

                                break;
                            }
                    }
                    FaceCounter++;
                }
            }

            FaceCounter = 0;
            int[] indices = new int[6];
            //Tris for diagonal.
            //Create normals
            var normals = new List<Vector3>();
            foreach (var vert in verts)
            {
                normals.Add(vert.Normalized());
            }
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;
            // mesh.SetTriangles(indices, 0);
            AddSurfaceToMesh(verts, uvs, MatsToUse, FaceCounter, a_mesh, normals, indices);

            FaceCounter = 1;

            for (int i = 0; i < 6; i++)
            {
                if ((i == vSOUTH) || (i == vWEST))
                {
                    if (t.VisibleFaces[i] == true)
                    {
                        indices[0] = 0 + (4 * FaceCounter);
                        indices[1] = 1 + (4 * FaceCounter);
                        indices[2] = 2 + (4 * FaceCounter);
                        indices[3] = 0 + (4 * FaceCounter);
                        indices[4] = 2 + (4 * FaceCounter);
                        indices[5] = 3 + (4 * FaceCounter);
                        //mesh.SetTriangles(indices, FaceCounter);
                        AddSurfaceToMesh(verts, uvs, MatsToUse, FaceCounter, a_mesh, normals, indices);
                        FaceCounter++;
                    }
                }
            }

            return CreateMeshInstance(parent, t.tileX, t.tileY, TileName, a_mesh);
            //    mr.materials = MatsToUse;
            //    mesh.RecalculateNormals();
            //    mesh.RecalculateBounds();
            //    mf.mesh = mesh;
            //    if (EnableCollision)
            //    {
            //        MeshCollider mc = Tile.AddComponent<MeshCollider>();
            //        mc.sharedMesh = null;
            //        mc.sharedMesh = mesh;
            //    }
            //mc.sharedMesh=mesh;
            // return;
        }


        /// <summary>
        /// Renders the diag SE portion.
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="Bottom">Bottom.</param>
        /// <param name="Top">Top.</param>
        /// <param name="t">T.</param>
        /// <param name="TileName">Tile name.</param>
        static Node3D RenderDiagSEPortion(Node3D parent, int Bottom, int Top, TileInfo t, string TileName)
        {
            //Does a thing.
            //Draws 3 meshes. Outward diagonal wall. Back and side if visible.

            int NumberOfVisibleFaces = 1;//Will always have the diag.
                                         //Get the number of faces
            for (int i = 0; i < 6; i++)
            {
                if ((i == vNORTH) || (i == vWEST))
                {
                    if (t.VisibleFaces[i] == true)
                    {
                        NumberOfVisibleFaces++;
                    }
                }
            }
            //Allocate enough vertice and UVs for the faces
            int[] MatsToUse = new int[NumberOfVisibleFaces];
            Vector3[] verts = new Vector3[NumberOfVisibleFaces * 4];
            Vector2[] uvs = new Vector2[NumberOfVisibleFaces * 4];
            float floorHeight = (float)(Top * 0.15f);
            float baseHeight = (float)(Bottom * 0.15f);

            //    //Now create the mesh
            //    GameObject Tile = new GameObject(TileName)
            //    {
            //        layer = LayerMask.NameToLayer("MapMesh")
            //    };
            //    Tile.transform.parent = parent.transform;
            //    Tile.transform.position = new Vector3(t.tileX * 1.2f, 0.0f, t.tileY * 1.2f);

            //    Tile.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            //    MeshFilter mf = Tile.AddComponent<MeshFilter>();
            //    MeshRenderer mr = Tile.AddComponent<MeshRenderer>();
            //MeshCollider mc = Tile.AddComponent<MeshCollider>();
            ///mc.sharedMesh=null;

            var a_mesh = new ArrayMesh();


            //    Mesh mesh = new Mesh
            //    {
            //        subMeshCount = NumberOfVisibleFaces//Should be no of visible faces
            //    };

            //Now allocate the visible faces to triangles.
            int FaceCounter = 0;//Tracks which number face we are now on.
            MatsToUse[FaceCounter] = WallTexture(fSELF, t);
            float PolySize = Top - Bottom;
            float uv0 = (float)(Bottom * 0.125f);
            float uv1 = -(PolySize / 8.0f) + (uv0);
            //Set the diagonal face first

            verts[0] = new Vector3(0f, baseHeight, 0f);
            verts[1] = new Vector3(0f, floorHeight, 0f);
            verts[2] = new Vector3(-1.2f, floorHeight, 1.2f);
            verts[3] = new Vector3(-1.2f, baseHeight, 1.2f);

            uvs[0] = new Vector2(0.0f, uv0);
            uvs[1] = new Vector2(0.0f, uv1);
            uvs[2] = new Vector2(1, uv1);
            uvs[3] = new Vector2(1, uv0);
            FaceCounter++;

            for (int i = 0; i < 6; i++)
            {
                if ((t.VisibleFaces[i] == true) && ((i == vNORTH) || (i == vWEST)))
                {//Will only render north or west if needed.
                    switch (i)
                    {
                        case vNORTH:
                            {
                                //north wall vertices
                                MatsToUse[FaceCounter] = WallTexture(fNORTH, t);
                                verts[0 + (4 * FaceCounter)] = new Vector3(-1.2f, baseHeight, 1.2f);
                                verts[1 + (4 * FaceCounter)] = new Vector3(-1.2f, floorHeight, 1.2f);
                                verts[2 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 1.2f);
                                verts[3 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 1.2f);

                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, uv1);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(1, uv1);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(1, uv0);

                                break;
                            }
                        case vWEST:
                            {
                                //west wall vertices
                                MatsToUse[FaceCounter] = WallTexture(fWEST, t);
                                verts[0 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 1.2f);
                                verts[1 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 1.2f);
                                verts[2 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 0f);
                                verts[3 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 0f);
                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, uv1);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(1, uv1);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(1, uv0);

                                break;
                            }
                    }
                    FaceCounter++;
                }
            }

            FaceCounter = 0;
            //Create normals
            var normals = new List<Vector3>();
            foreach (var vert in verts)
            {
                normals.Add(vert.Normalized());
            }

            //Apply the uvs and create my tris
            //    mesh.vertices = verts;
            //    mesh.uv = uvs;
            int[] indices = new int[6];
            //Tris for diagonal.

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;
            AddSurfaceToMesh(verts, uvs, MatsToUse, FaceCounter, a_mesh, normals, indices);

            // mesh.SetTriangles(indices, 0);
            FaceCounter = 1;

            for (int i = 0; i < 6; i++)
            {
                if ((i == vNORTH) || (i == vWEST))
                {
                    if (t.VisibleFaces[i] == true)
                    {
                        indices[0] = 0 + (4 * FaceCounter);
                        indices[1] = 1 + (4 * FaceCounter);
                        indices[2] = 2 + (4 * FaceCounter);
                        indices[3] = 0 + (4 * FaceCounter);
                        indices[4] = 2 + (4 * FaceCounter);
                        indices[5] = 3 + (4 * FaceCounter);
                        //mesh.SetTriangles(indices, FaceCounter);
                        AddSurfaceToMesh(verts, uvs, MatsToUse, FaceCounter, a_mesh, normals, indices);
                        FaceCounter++;
                    }
                }
            }
            return CreateMeshInstance(parent, t.tileX, t.tileY, TileName, a_mesh);
        }


        /// <summary>
        /// Renders an open tile with no slopes
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="t">T.</param>
        /// <param name="Water">If set to <c>true</c> water.</param>
        /// <param name="invert">If set to <c>true</c> invert.</param>
        static Node3D RenderDiagOpenTile(Node3D parent, int x, int y, TileInfo t)
        {
            if (t.Render == true)
            {
                string TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");

                //Bottom face 
                if (t.TerrainChange)
                {                    
                    return RenderPrism(parent, x, y, t, -16, t.floorHeight, TileName);
                }
                else
                {
                    TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");
                    return RenderPrism(parent, x, y, t, 0, t.floorHeight, TileName);
                }
            }
            return null;
        }

        /// <summary>
        /// Renders the diag SW portion.
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="Bottom">Bottom.</param>
        /// <param name="Top">Top.</param>
        /// <param name="t">T.</param>
        /// <param name="TileName">Tile name.</param>
        static Node3D RenderDiagSWPortion(Node3D parent, int Bottom, int Top, TileInfo t, string TileName)
        {

            //Does a thing.
            //Does a thing.
            //Draws 3 meshes. Outward diagonal wall. Back and side if visible.
            int NumberOfVisibleFaces = 1;//Will always have the diag.
                                         //Get the number of faces
            for (int i = 0; i < 6; i++)
            {
                if ((i == vNORTH) || (i == vEAST))
                {
                    if (t.VisibleFaces[i] == true)
                    {
                        NumberOfVisibleFaces++;
                    }
                }
            }
            //Allocate enough verticea and UVs for the faces
            int[] MatsToUse = new int[NumberOfVisibleFaces];
            Vector3[] verts = new Vector3[NumberOfVisibleFaces * 4];
            Vector2[] uvs = new Vector2[NumberOfVisibleFaces * 4];
            float floorHeight = (float)(Top * 0.15f);
            float baseHeight = (float)(Bottom * 0.15f);
            float dimX = t.DimX;
            float dimY = t.DimY;
            //Now create the mesh
            // GameObject Tile = new GameObject(TileName)
            // {
            //     layer = LayerMask.NameToLayer("MapMesh")
            // };
            // Tile.transform.parent = parent.transform;
            // Tile.transform.position = new Vector3(t.tileX * 1.2f, 0.0f, t.tileY * 1.2f);

            // Tile.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            // MeshFilter mf = Tile.AddComponent<MeshFilter>();
            // MeshRenderer mr = Tile.AddComponent<MeshRenderer>();
            //MeshCollider mc = Tile.AddComponent<MeshCollider>();
            //mc.sharedMesh=null;

            // Mesh mesh = new Mesh
            // {
            //     subMeshCount = NumberOfVisibleFaces//Should be no of visible faces
            // };

            //Now allocate the visible faces to triangles.
            int FaceCounter = 0;//Tracks which number face we are now on.

            MatsToUse[FaceCounter] = WallTexture(fSELF, t);

            float PolySize = Top - Bottom;
            float uv0 = (float)(Bottom * 0.125f);
            float uv1 = -(PolySize / 8.0f) + (uv0);
            //Set the diagonal face first
            verts[0] = new Vector3(0f, baseHeight, 1.2f);
            verts[1] = new Vector3(0f, floorHeight, 1.2f);
            verts[2] = new Vector3(-1.2f, floorHeight, 0f);
            verts[3] = new Vector3(-1.2f, baseHeight, 0f);

            uvs[0] = new Vector2(0.0f, uv0);
            uvs[1] = new Vector2(0.0f, uv1);
            uvs[2] = new Vector2(1, uv1);
            uvs[3] = new Vector2(1, uv0);
            FaceCounter++;

            for (int i = 0; i < 6; i++)
            {
                if ((t.VisibleFaces[i] == true) && ((i == vNORTH) || (i == vEAST)))
                {//Will only render north or west if needed.
                    switch (i)
                    {
                        case vNORTH:
                            {
                                //north wall vertices
                                MatsToUse[FaceCounter] = WallTexture(fNORTH, t);
                                verts[0 + (4 * FaceCounter)] = new Vector3(-1.2f, baseHeight, 1.2f);
                                verts[1 + (4 * FaceCounter)] = new Vector3(-1.2f, floorHeight, 1.2f);
                                verts[2 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 1.2f);
                                verts[3 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 1.2f);

                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, uv1);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(1, uv1);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(1, uv0);

                                break;
                            }

                        case vEAST:
                            {
                                //east wall vertices
                                MatsToUse[FaceCounter] = WallTexture(fEAST, t);
                                verts[0 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 0f);
                                verts[1 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 0f);
                                verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 1.2f * dimY);
                                verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 1.2f * dimY);
                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, uv1);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(dimY, uv1);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(dimY, uv0);

                                break;
                            }
                    }
                    FaceCounter++;
                }
            }

            var a_mesh = new ArrayMesh(); //= Mesh as ArrayMesh;
            //create normals
            var normals = new List<Vector3>();
            foreach (var vert in verts)
            {
                normals.Add(vert.Normalized());
            }

            //Apply the uvs and create my tris
            // mesh.vertices = verts;
            // mesh.uv = uvs;
            int[] indices = new int[6];
            //Tris for diagonal.
            FaceCounter = 0;
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;
            //mesh.SetTriangles(indices, 0);
            AddSurfaceToMesh(verts, uvs, MatsToUse, FaceCounter, a_mesh, normals, indices);

            FaceCounter = 1;

            for (int i = 0; i < 6; i++)
            {
                if ((i == vNORTH) || (i == vEAST))
                {
                    if (t.VisibleFaces[i] == true)
                    {
                        indices[0] = 0 + (4 * FaceCounter);
                        indices[1] = 1 + (4 * FaceCounter);
                        indices[2] = 2 + (4 * FaceCounter);
                        indices[3] = 0 + (4 * FaceCounter);
                        indices[4] = 2 + (4 * FaceCounter);
                        indices[5] = 3 + (4 * FaceCounter);
                        //mesh.SetTriangles(indices, FaceCounter);
                        AddSurfaceToMesh(verts, uvs, MatsToUse, FaceCounter, a_mesh, normals, indices);
                        FaceCounter++;
                    }
                }
            }

            // mr.materials = MatsToUse;
            // mesh.RecalculateNormals();
            // mesh.RecalculateBounds();
            // mf.mesh = mesh;
            // if (EnableCollision)
            // {
            //     MeshCollider mc = Tile.AddComponent<MeshCollider>();
            //     mc.sharedMesh = null;
            //     mc.sharedMesh = mesh;
            // }

            return CreateMeshInstance(parent, t.tileX, t.tileY, TileName, a_mesh);

            //mc.sharedMesh=mesh;

        }
        /// <summary>
        /// Renders the diag NW portion.
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="Bottom">Bottom.</param>
        /// <param name="Top">Top.</param>
        /// <param name="t">T.</param>
        /// <param name="TileName">Tile name.</param>
        static Node3D RenderDiagNWPortion(Node3D parent, int Bottom, int Top, TileInfo t, string TileName)
        {
            //Does a thing.
            //Does a thing.
            //Draws 3 meshes. Outward diagonal wall. Back and side if visible.


            int NumberOfVisibleFaces = 1;//Will always have the diag.
                                         //Get the number of faces
            for (int i = 0; i < 6; i++)
            {
                if ((i == vSOUTH) || (i == vEAST))
                {
                    if (t.VisibleFaces[i] == true)
                    {
                        NumberOfVisibleFaces++;
                    }
                }
            }
            //Allocate enough verticea and UVs for the faces
            int[] MatsToUse = new int[NumberOfVisibleFaces];
            Vector3[] verts = new Vector3[NumberOfVisibleFaces * 4];
            Vector2[] uvs = new Vector2[NumberOfVisibleFaces * 4];
            float floorHeight = (float)(Top * 0.15f);
            float baseHeight = (float)(Bottom * 0.15f);
            float dimX = t.DimX;
            float dimY = t.DimY;

            var a_mesh = new ArrayMesh();


            //Now allocate the visible faces to triangles.
            int FaceCounter = 0;//Tracks which number face we are now on.
            float PolySize = Top - Bottom;
            float uv0 = (float)(Bottom * 0.125f);
            float uv1 = -(PolySize / 8.0f) + (uv0);
            //Set the diagonal face first
            MatsToUse[FaceCounter] = WallTexture(fSELF, t);

            verts[0] = new Vector3(-1.2f, baseHeight, 1.2f);
            verts[1] = new Vector3(-1.2f, floorHeight, 1.2f);
            verts[2] = new Vector3(0f, floorHeight, 0f);
            verts[3] = new Vector3(0f, baseHeight, 0f);

            uvs[0] = new Vector2(0.0f, uv0);
            uvs[1] = new Vector2(0.0f, uv1);
            uvs[2] = new Vector2(1, uv1);
            uvs[3] = new Vector2(1, uv0);
            FaceCounter++;

            for (int i = 0; i < 6; i++)
            {
                if ((t.VisibleFaces[i] == true) && ((i == vSOUTH) || (i == vEAST)))
                {//Will only render north or west if needed.
                    switch (i)
                    {
                        case vEAST:
                            {
                                //east wall vertices                                
                                MatsToUse[FaceCounter] = WallTexture(fEAST, t);
                                verts[0 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 0f);
                                verts[1 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 0f);
                                verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 1.2f * dimY);
                                verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 1.2f * dimY);
                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, uv1);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(dimY, uv1);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(dimY, uv0);
                                break;
                            }

                        case vSOUTH:
                            {
                                //south wall vertices
                                MatsToUse[FaceCounter] = WallTexture(fSOUTH, t);
                                verts[0 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 0f);
                                verts[1 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 0f);
                                verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 0f);
                                verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 0f);
                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, uv1);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(dimX, uv1);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(dimX, uv0);

                                break;
                            }
                    }
                    FaceCounter++;
                }
            }
            FaceCounter = 0;
            //create normals from verts
            var normals = new List<Vector3>();
            foreach (var vert in verts)
            {
                normals.Add(vert.Normalized());
            }
            //Apply the uvs and create my tris
            //    mesh.vertices = verts;
            //    mesh.uv = uvs;
            int[] indices = new int[6];
            //Tris for diagonal.

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;
            //mesh.SetTriangles(indices, 0);

            AddSurfaceToMesh(verts, uvs, MatsToUse, FaceCounter, a_mesh, normals, indices);

            FaceCounter = 1;

            for (int i = 0; i < 6; i++)
            {
                if ((i == vSOUTH) || (i == vEAST))
                {
                    if (t.VisibleFaces[i] == true)
                    {
                        indices[0] = 0 + (4 * FaceCounter);
                        indices[1] = 1 + (4 * FaceCounter);
                        indices[2] = 2 + (4 * FaceCounter);
                        indices[3] = 0 + (4 * FaceCounter);
                        indices[4] = 2 + (4 * FaceCounter);
                        indices[5] = 3 + (4 * FaceCounter);
                        // mesh.SetTriangles(indices, FaceCounter);
                        AddSurfaceToMesh(verts, uvs, MatsToUse, FaceCounter, a_mesh, normals, indices);
                        FaceCounter++;
                    }
                }
            }
            return CreateMeshInstance(parent, t.tileX, t.tileY, TileName, a_mesh);
        }




        /// <summary>
        /// Renders a cuboid with sloped tops
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="t">T.</param>
        /// <param name="Water">If set to <c>true</c> water.</param>
        /// <param name="Bottom">Bottom.</param>
        /// <param name="Top">Top.</param>
        /// <param name="SlopeDir">Slope dir.</param>
        /// <param name="Steepness">Steepness.</param>
        /// <param name="TileName">Tile name.</param>
        static Node3D RenderSlopedCuboid(Node3D parent, int x, int y, TileInfo t, int Bottom, int Top, int SlopeDir, int Steepness, string TileName)
        {

            //Draws a cube with sloped tops

            //Heigh adjustements for the slopes
            float AdjustUpperNorth = 0f;
            float AdjustUpperSouth = 0f;
            float AdjustUpperEast = 0f;
            float AdjustUpperWest = 0f;

            switch (SlopeDir)
            {
                case TILE_SLOPE_N:
                    AdjustUpperNorth = Steepness * 0.15f;
                    break;
                case TILE_SLOPE_S:
                    AdjustUpperSouth = Steepness * 0.15f;
                    break;
                case TILE_SLOPE_E:
                    AdjustUpperEast = Steepness * 0.15f;
                    break;
                case TILE_SLOPE_W:
                    AdjustUpperWest = Steepness * 0.15f;
                    break;
            }

            int NumberOfVisibleFaces = 0;
            int NumberOfSlopedFaces = 0;
            int SlopesAdded = 0;
            //Get the number of faces
            for (int i = 0; i < 6; i++)
            {
                if (t.VisibleFaces[i] == true)
                {
                    NumberOfVisibleFaces++;
                    if (
                            (((SlopeDir == TILE_SLOPE_N) || (SlopeDir == TILE_SLOPE_S)) && ((i == vWEST) || (i == vEAST)))
                            ||
                            (((SlopeDir == TILE_SLOPE_E) || (SlopeDir == TILE_SLOPE_W)) && ((i == vNORTH) || (i == vSOUTH)))
                        )
                    {
                        NumberOfSlopedFaces++;  //SHould only be to a max of two
                    }
                }
            }
            //Allocate enough vertices and UVs for the faces
            int[] MatsToUse = new int[NumberOfVisibleFaces + NumberOfSlopedFaces];
            Vector3[] verts = new Vector3[NumberOfVisibleFaces * 4 + +NumberOfSlopedFaces * 3];
            Vector2[] uvs = new Vector2[NumberOfVisibleFaces * 4 + NumberOfSlopedFaces * 3];
            float floorHeight = (float)(Top * 0.15f);
            float baseHeight = (float)(Bottom * 0.15f);
            float dimX = t.DimX;
            float dimY = t.DimY;


            //Now create the mesh
            var a_mesh = new ArrayMesh();

            //Now allocate the visible faces to triangles.
            int FaceCounter = 0;//Tracks which number face we are now on.
                                //float PolySize= Top-Bottom;
                                //float uv0= (float)(Bottom*0.125f);
                                //float uv1=(PolySize / 8.0f) + (uv0);
            CalcUVForSlopedCuboid(Top, Bottom, out float uv0, out float uv1);
            float slopeHeight;
            float uv0Slope;
            float uv1Slope;

            CalcUVForSlopedCuboid(Top + Steepness, Bottom, out uv0Slope, out uv1Slope);
            slopeHeight = floorHeight;

            for (int i = 0; i < 6; i++)
            {
                if (t.VisibleFaces[i] == true)
                {
                    switch (i)
                    {
                        case vTOP:
                            {

                                //Set the verts	
                                MatsToUse[FaceCounter] = FloorTexture(t);

                                verts[0 + (4 * FaceCounter)] = new Vector3(0.0f, floorHeight + AdjustUpperWest + AdjustUpperSouth, 0.0f);
                                verts[1 + (4 * FaceCounter)] = new Vector3(0.0f, floorHeight + AdjustUpperWest + AdjustUpperNorth, 1.2f * dimY);
                                verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight + AdjustUpperNorth + AdjustUpperEast, 1.2f * dimY);
                                verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight + AdjustUpperSouth + AdjustUpperEast, 0.0f);
                                //Allocate UVs
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, 1.0f * dimY);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(1.0f * dimX, 1.0f * dimY);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(1.0f * dimX, 0.0f);
                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, 0.0f);                           
                                
                                break;
                            }

                        case vNORTH:
                            {
                                //north wall vertices
                                MatsToUse[FaceCounter] = WallTexture(fNORTH, t);
                                switch (SlopeDir)
                                {
                                    case TILE_SLOPE_N:
                                        verts[0 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 1.2f * dimY);
                                        verts[1 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight + AdjustUpperNorth + AdjustUpperEast, 1.2f * dimY);
                                        verts[2 + (4 * FaceCounter)] = new Vector3(0f, floorHeight + AdjustUpperNorth + AdjustUpperWest, 1.2f * dimY);
                                        verts[3 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 1.2f * dimY);
                                        uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0Slope);//bottom uv
                                        uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, -uv1Slope);//top uv
                                        uvs[2 + (4 * FaceCounter)] = new Vector2(dimX, -uv1Slope);//top uv
                                        uvs[3 + (4 * FaceCounter)] = new Vector2(dimX, uv0Slope);//bottom uv
                                        break;

                                    default:

                                        verts[0 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 1.2f * dimY); //bottom right (1,1)
                                        verts[1 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 1.2f * dimY); //top left (0,0)
                                        verts[2 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 1.2f * dimY);//top right (1,0)
                                        verts[3 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 1.2f * dimY); //bottom left ()
                                        uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0 );//0,0?
                                        uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, -uv1);//0,1?
                                        uvs[2 + (4 * FaceCounter)] = new Vector2(dimX, -uv1);//1,1?
                                        uvs[3 + (4 * FaceCounter)] = new Vector2(dimX, uv0);//1,0?
                                        break;
                                }
                                if ((SlopeDir == TILE_SLOPE_E) || (SlopeDir == TILE_SLOPE_W))
                                {//Insert my verts for this slope														
                                    int index = uvs.GetUpperBound(0) - ((NumberOfSlopedFaces - SlopesAdded) * 3) + 1;
                                    MatsToUse[MatsToUse.GetUpperBound(0) - NumberOfSlopedFaces + SlopesAdded + 1] = MatsToUse[FaceCounter];

                                    switch (SlopeDir)
                                    {
                                        case TILE_SLOPE_E:
                                            {
                                                verts[index + 0] = new Vector3(-1.2f * dimX, slopeHeight, 1.2f * dimY);
                                                verts[index + 1] = new Vector3(-1.2f * dimX, slopeHeight + AdjustUpperNorth + AdjustUpperEast, 1.2f * dimY);
                                                verts[index + 2] = new Vector3(0f, slopeHeight + AdjustUpperNorth + AdjustUpperWest, 1.2f * dimY);
                                                float uv0edge;
                                                float uv1edge;
                                                //float uvToUse;
                                                CalcUVForSlopedCuboid(Top + Steepness, Top, out uv0edge, out uv1edge);
                                                // if (offset == 0) { uvToUse = +uv0edge; } else { uvToUse = uv0edge - offset; }
                                                uvs[index + 0] = new Vector2(0, -uv0edge);//0, vertical alignment
                                                uvs[index + 1] = new Vector2(0, -(uv0edge + Steepness * 0.125f)); //vertical + scale
                                                uvs[index + 2] = new Vector2(1, -uv0edge);   //1, vertical alignment	
                                                break;
                                            }

                                        case TILE_SLOPE_W:
                                            {

                                                verts[index + 0] = new Vector3(-1.2f * dimX, slopeHeight + AdjustUpperNorth + AdjustUpperEast, 1.2f * dimY);
                                                verts[index + 1] = new Vector3(0f, slopeHeight + AdjustUpperNorth + AdjustUpperWest, 1.2f * dimY);
                                                verts[index + 2] = new Vector3(0f, slopeHeight, 1.2f * dimY);
                                                float uv0edge = 0;
                                                //float uvToUse;
                                                //if (offset == 0) { uvToUse = +uv0edge; } else { uvToUse = uv0edge - offset; }
                                                float uv1edge;
                                                CalcUVForSlopedCuboid(Top + Steepness, Top, out uv0edge, out uv1edge);
                                                //if (offset == 0) { uvToUse = +uv0edge; } else { uvToUse = uv0edge - offset; }
                                                uvs[index + 0] = new Vector2(0, -uv0edge);//0, vertical alignment
                                                uvs[index + 1] = new Vector2(1, -(uv0edge + Steepness * 0.125f)); //vertical + scale
                                                uvs[index + 2] = new Vector2(1, -uv0edge);   //1, vertical alignment	
                                                break;
                                            }
                                    }
                                    SlopesAdded++;
                                }
                                break;
                            }//end north


                        case vSOUTH:
                            {
                                //south wall vertices
                                MatsToUse[FaceCounter] = WallTexture(fSOUTH, t);
                                switch (SlopeDir)
                                {
                                    case TILE_SLOPE_S:
                                        verts[0 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 0f);
                                        verts[1 + (4 * FaceCounter)] = new Vector3(0f, floorHeight + AdjustUpperSouth + AdjustUpperWest, 0f);
                                        verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight + AdjustUpperSouth + AdjustUpperEast, 0f);
                                        verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 0f);
                                        uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0Slope);//bottom uv
                                        uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, -uv1Slope);//top uv
                                        uvs[2 + (4 * FaceCounter)] = new Vector2(dimX, -uv1Slope);//top uv
                                        uvs[3 + (4 * FaceCounter)] = new Vector2(dimX, uv0Slope);//bottom uv
                                        break;
                                    default:
                                        verts[0 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 0f);
                                        verts[1 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 0f);
                                        verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 0f);
                                        verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 0f);
                                        uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                        uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, -uv1);
                                        uvs[2 + (4 * FaceCounter)] = new Vector2(dimX, -uv1);
                                        uvs[3 + (4 * FaceCounter)] = new Vector2(dimX, uv0);
                                        break;
                                }

                                if ((SlopeDir == TILE_SLOPE_E) || (SlopeDir == TILE_SLOPE_W))
                                {//Insert my verts for this slope
                                    int index = uvs.GetUpperBound(0) - ((NumberOfSlopedFaces - SlopesAdded) * 3) + 1;
                                    MatsToUse[MatsToUse.GetUpperBound(0) - NumberOfSlopedFaces + SlopesAdded + 1] = MatsToUse[FaceCounter];
                                    switch (SlopeDir)
                                    {
                                        case TILE_SLOPE_W:
                                            {

                                                verts[index + 0] = new Vector3(0f, slopeHeight, 0f);
                                                verts[index + 1] = new Vector3(0f, slopeHeight + AdjustUpperSouth + AdjustUpperWest, 0f);
                                                verts[index + 2] = new Vector3(-1.2f * dimX, slopeHeight + AdjustUpperSouth + AdjustUpperEast, 0f);
                                                float uv0edge;
                                                float uv1edge;
                                               // float uvToUse;
                                                CalcUVForSlopedCuboid(Top + Steepness, Top, out uv0edge, out uv1edge);
                                                //if (offset == 0) { uvToUse = +uv0edge; } else { uvToUse = uv0edge - offset; }
                                                uvs[index + 0] = new Vector2(0, -uv0edge);//0, vertical alignment
                                                uvs[index + 1] = new Vector2(0, -(uv0edge + Steepness * 0.125f)); //vertical + scale
                                                uvs[index + 2] = new Vector2(1, -uv0edge);   //1, vertical alignment	
                                                break;
                                            }

                                        case TILE_SLOPE_E:
                                            {

                                                verts[index + 0] = new Vector3(0f, slopeHeight, 0f);
                                                verts[index + 1] = new Vector3(-1.2f * dimX, slopeHeight + AdjustUpperSouth + AdjustUpperEast, 0f);
                                                verts[index + 2] = new Vector3(-1.2f * dimX, slopeHeight, 0f);
                                                float uv0edge;
                                                float uv1edge;
                                                //float uvToUse;
                                                CalcUVForSlopedCuboid(Top + Steepness, Top, out uv0edge, out uv1edge);
                                                //if (offset == 0) { uvToUse = +uv0edge; } else { uvToUse = uv0edge - offset; }
                                                uvs[index + 0] = new Vector2(0, -uv0edge);//0, vertical alignment
                                                uvs[index + 1] = new Vector2(1, -(uv0edge + Steepness * 0.125f)); //vertical + scale
                                                uvs[index + 2] = new Vector2(1, -uv0edge);   //1, vertical alignment	
                                                break;
                                            }
                                    }
                                    SlopesAdded++;
                                }
                                break;
                            }//end south

                        case vWEST:
                            {                                
                                MatsToUse[FaceCounter] = WallTexture(fWEST, t);

                                switch (SlopeDir)
                                {
                                    case TILE_SLOPE_W:
                                        verts[0 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 1.2f * dimY);
                                        verts[1 + (4 * FaceCounter)] = new Vector3(0f, floorHeight + AdjustUpperWest + AdjustUpperNorth, 1.2f * dimY);
                                        verts[2 + (4 * FaceCounter)] = new Vector3(0f, floorHeight + AdjustUpperWest + AdjustUpperSouth, 0f);
                                        verts[3 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 0f);
                                        uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0Slope);//bottom uv
                                        uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, -uv1Slope);//top uv
                                        uvs[2 + (4 * FaceCounter)] = new Vector2(dimX, -uv1Slope);//top uv
                                        uvs[3 + (4 * FaceCounter)] = new Vector2(dimX, uv0Slope);//bottom uv
                                        break;
                                    default:
                                        verts[0 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 1.2f * dimY);
                                        verts[1 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 1.2f * dimY);
                                        verts[2 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 0f);
                                        verts[3 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 0f);
                                        uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                        uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, -uv1);
                                        uvs[2 + (4 * FaceCounter)] = new Vector2(dimY, -uv1);
                                        uvs[3 + (4 * FaceCounter)] = new Vector2(dimY, uv0);

                                        break;
                                }

                                if ((SlopeDir == TILE_SLOPE_N) || (SlopeDir == TILE_SLOPE_S))
                                {//Insert my verts for this slope
                                    MatsToUse[MatsToUse.GetUpperBound(0) - NumberOfSlopedFaces + SlopesAdded + 1] = MatsToUse[FaceCounter];
                                    int index = uvs.GetUpperBound(0) - ((NumberOfSlopedFaces - SlopesAdded) * 3) + 1;
                                    switch (SlopeDir)
                                    {
                                        case TILE_SLOPE_N:
                                            {

                                                verts[index + 0] = new Vector3(0f, slopeHeight, 1.2f * dimY);
                                                verts[index + 1] = new Vector3(0f, slopeHeight + AdjustUpperWest + AdjustUpperNorth, 1.2f * dimY);
                                                verts[index + 2] = new Vector3(0f, slopeHeight + AdjustUpperWest + AdjustUpperSouth, 0f);
                                                float uv0edge;
                                                float uv1edge;
                                                //float uvToUse;
                                                CalcUVForSlopedCuboid(Top + Steepness, Top, out uv0edge, out uv1edge);
                                                //if (offset == 0) { uvToUse = +uv0edge; } else { uvToUse = uv0edge - offset; }
                                                uvs[index + 0] = new Vector2(0, -uv0edge);//0, vertical alignment
                                                uvs[index + 1] = new Vector2(0, -(uv0edge + Steepness * 0.125f)); //vertical + scale
                                                uvs[index + 2] = new Vector2(1, -uv0edge);   //1, vertical alignment		
                                                break;
                                            }

                                        case TILE_SLOPE_S:
                                            {
                                                //ceil n west
                                                verts[index + 0] = new Vector3(0f, slopeHeight + AdjustUpperWest + AdjustUpperNorth, 1.2f * dimY);
                                                verts[index + 1] = new Vector3(0f, slopeHeight + AdjustUpperWest + AdjustUpperSouth, 0f);
                                                verts[index + 2] = new Vector3(0f, slopeHeight, 0f);
                                                float uv0edge;
                                                float uv1edge;
                                                //float uvToUse;
                                                CalcUVForSlopedCuboid(Top + Steepness, Top, out uv0edge, out uv1edge);
                                               // if (offset == 0) { uvToUse = +uv0edge; } else { uvToUse = uv0edge - offset; }
                                                uvs[index + 0] = new Vector2(0, -uv0edge);//0, vertical alignment
                                                uvs[index + 1] = new Vector2(1, -(uv0edge + Steepness * 0.125f)); //vertical + scale
                                                uvs[index + 2] = new Vector2(1, -uv0edge);   //1, vertical alignment	
                                                break;
                                            }
                                    }
                                    SlopesAdded++;
                                }
                                break;

                            }//end west

                        case vEAST:
                            {
                                //east wall vertices                               
                                MatsToUse[FaceCounter] = WallTexture(fEAST, t);
                                switch (SlopeDir)
                                {
                                    case TILE_SLOPE_E:
                                        verts[0 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 0f);
                                        verts[1 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight + AdjustUpperEast + AdjustUpperSouth, 0f);
                                        verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight + AdjustUpperEast + AdjustUpperNorth, 1.2f * dimY);
                                        verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 1.2f * dimY);
                                        uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0Slope);//bottom uv
                                        uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, -uv1Slope);//top uv
                                        uvs[2 + (4 * FaceCounter)] = new Vector2(dimX, -uv1Slope);//top uv
                                        uvs[3 + (4 * FaceCounter)] = new Vector2(dimX, uv0Slope);//bottom uv
                                        break;
                                    default:
                                        verts[0 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 0f);
                                        verts[1 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 0f);
                                        verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 1.2f * dimY);
                                        verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 1.2f * dimY);
                                        uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);//0
                                        uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, -uv1);//1
                                        uvs[2 + (4 * FaceCounter)] = new Vector2(dimY, -uv1);//1
                                        uvs[3 + (4 * FaceCounter)] = new Vector2(dimY, uv0);//0
                                        break;
                                }
                                if ((SlopeDir == TILE_SLOPE_N) || (SlopeDir == TILE_SLOPE_S))
                                {//Insert my verts for this slope

                                    MatsToUse[MatsToUse.GetUpperBound(0) - NumberOfSlopedFaces + SlopesAdded + 1] = MatsToUse[FaceCounter];
                                    int index = uvs.GetUpperBound(0) - ((NumberOfSlopedFaces - SlopesAdded) * 3) + 1;
                                    switch (SlopeDir)
                                    {
                                        case TILE_SLOPE_S:
                                            {
                                                //ceil_n east		
                                                verts[index + 0] = new Vector3(-1.2f * dimX, slopeHeight, 0f);
                                                verts[index + 1] = new Vector3(-1.2f * dimX, slopeHeight + AdjustUpperEast + AdjustUpperSouth, 0f);
                                                verts[index + 2] = new Vector3(-1.2f * dimX, slopeHeight + AdjustUpperEast + AdjustUpperNorth, 1.2f * dimY);
                                                float uv0edge;
                                                float uv1edge;
                                                //float uvToUse;
                                                CalcUVForSlopedCuboid(Top + Steepness, Top, out uv0edge, out uv1edge);
                                                //if (offset == 0) { uvToUse = +uv0edge; } else { uvToUse = uv0edge - offset; }
                                                uvs[index + 0] = new Vector2(0, -uv0edge);//0, vertical alignment
                                                uvs[index + 1] = new Vector2(0, -(uv0edge + Steepness * 0.125f)); //vertical + scale
                                                uvs[index + 2] = new Vector2(1, -uv0edge);   //1, vertical alignment	
                                                break;
                                            }

                                        case TILE_SLOPE_N:
                                            {
                                                //hey east on tile s ceil
                                                verts[index + 0] = new Vector3(-1.2f * dimX, slopeHeight + AdjustUpperEast + AdjustUpperSouth, 0f);
                                                verts[index + 1] = new Vector3(-1.2f * dimX, slopeHeight + AdjustUpperEast + AdjustUpperNorth, 1.2f * dimY);
                                                verts[index + 2] = new Vector3(-1.2f * dimX, slopeHeight, 1.2f * dimY);
                                                float uv0edge;
                                                float uv1edge;
                                                //float uvToUse;
                                                //if (t.shockEastOffset==0){uvToUse=+uv1edge;}else{uvToUse=-uv0edge;}
                                                //uvToUse=uv0edge;
                                                //fixed
                                                CalcUVForSlopedCuboid(Top + Steepness, Top, out uv0edge, out uv1edge);
                                                //if (offset == 0) { uvToUse = +uv0edge; } else { uvToUse = uv0edge - offset; }
                                                uvs[index + 0] = new Vector2(0, -uv0edge);//0, vertical alignment  0,1
                                                uvs[index + 1] = new Vector2(1, -(uv0edge + Steepness * 0.125f)); //vertical + scale  1,1
                                                uvs[index + 2] = new Vector2(1, -uv0edge);   //1, vertical alignment	  1,0
                                                break;
                                            }
                                    }
                                    SlopesAdded++;
                                }

                                break;
                            }//end east


                        case vBOTTOM:
                            {
                                //bottom wall vertices.
                                MatsToUse[FaceCounter] = FloorTexture(t);

                                verts[0 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 1.2f * dimY);
                                verts[1 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 0f);
                                verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 0f);
                                verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 1.2f * dimY);

                                //Change default UVs
                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, 0.0f);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, 1.0f * dimY);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(dimX, 1.0f * dimY);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(dimX, 0.0f);
                                break;
                            }
                    }
                    FaceCounter++;
                }
            }

            //Apply the uvs and create my tris
            // mesh.vertices = verts;
            // mesh.uv = uvs;
            FaceCounter = 0;

            var normals = new List<Vector3>();
            foreach (var vert in verts)
            {
                normals.Add(vert.Normalized());
            }

            int[] indices = new int[6];
            int LastIndex = 0;
            for (int i = 0; i < 6; i++)
            {
                if (t.VisibleFaces[i] == true)
                {
                    indices[0] = 0 + (4 * FaceCounter);
                    indices[1] = 1 + (4 * FaceCounter);
                    indices[2] = 2 + (4 * FaceCounter);
                    indices[3] = 0 + (4 * FaceCounter);
                    indices[4] = 2 + (4 * FaceCounter);
                    indices[5] = 3 + (4 * FaceCounter);
                    LastIndex = 3 + (4 * FaceCounter);
                    AddSurfaceToMesh(verts, uvs, MatsToUse, FaceCounter, a_mesh, normals, indices);
                    FaceCounter++;
                }
            }
            //Insert any sloped tris at the end
            indices = new int[3];
            //FaceCounter=0;
            SlopesAdded = 0;
            LastIndex++;
            for (int i = 0; i < 6; i++)
            {
                if (t.VisibleFaces[i] == true)
                {
                    if (
                            (((SlopeDir == TILE_SLOPE_N) || (SlopeDir == TILE_SLOPE_S)) && ((i == vWEST) || (i == vEAST)))
                            ||
                            (((SlopeDir == TILE_SLOPE_E) || (SlopeDir == TILE_SLOPE_W)) && ((i == vNORTH) || (i == vSOUTH)))
                    )
                    {
                        indices[0] = 0 + LastIndex + (3 * SlopesAdded);
                        indices[1] = 1 + LastIndex + (3 * SlopesAdded);
                        indices[2] = 2 + LastIndex + (3 * SlopesAdded);
                        //mesh.SetTriangles(indices, FaceCounter + SlopesAdded);
                        AddSurfaceToMesh(verts, uvs, MatsToUse, FaceCounter + SlopesAdded, a_mesh, normals, indices);
                        SlopesAdded++;
                    }
                }
            }

            return CreateMeshInstance(parent, x, y, TileName, a_mesh);
            // mr.materials = MatsToUse;
            // mesh.RecalculateNormals();
            // mesh.RecalculateBounds();
            // mf.mesh = mesh;
            // if (EnableCollision)
            // {
            //     MeshCollider mc = Tile.AddComponent<MeshCollider>();
            //     mc.sharedMesh = null;
            //     mc.sharedMesh = mesh;
            // }
            //mc.sharedMesh=mesh;

        }







        /// <summary>
        /// Renders the floor of a diag tile
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="t">T.</param>
        /// <param name="Water">If set to <c>true</c> water.</param>
        /// <param name="Bottom">Bottom.</param>
        /// <param name="Top">Top.</param>
        /// <param name="TileName">Tile name.</param>
        static Node3D RenderPrism(Node3D parent, int x, int y, TileInfo t, int Bottom, int Top, string TileName)
        {

            //Draw a cube with no slopes.
            int NumberOfVisibleFaces = 0;
            //Get the number of faces
            for (int i = 0; i < 6; i++)
            {
                if (t.VisibleFaces[i] == true)
                {
                    NumberOfVisibleFaces++;
                }
            }
            //Allocate enough verticea and UVs for the faces
            Vector3[] verts = new Vector3[NumberOfVisibleFaces * 4];
            Vector2[] uvs = new Vector2[NumberOfVisibleFaces * 4];
            float floorHeight = (float)(Top * 0.15f);
            float baseHeight = (float)(Bottom * 0.15f);
            float dimX = t.DimX;
            float dimY = t.DimY;


            int[] MatsToUse = new int[NumberOfVisibleFaces];
            //Now allocate the visible faces to triangles.
            int FaceCounter = 0;//Tracks which number face we are now on.
            float PolySize = Top - Bottom;
            float uv0 = (float)(Bottom * 0.125f);
            float uv1 = -(PolySize / 8.0f) + (uv0);
            //int vertCountOffset=0;
            for (int i = 0; i < 6; i++)
            {
                if (t.VisibleFaces[i] == true)
                {
                    switch (i)
                    {
                        case vTOP:
                            {
                                //Set the verts	
                                MatsToUse[FaceCounter] = FloorTexture(t);
                                verts[0 + (4 * FaceCounter)] = new Vector3(0.0f, floorHeight, 0.0f);
                                verts[1 + (4 * FaceCounter)] = new Vector3(0.0f, floorHeight, 1.2f * dimY);
                                verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 1.2f * dimY);
                                verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 0.0f);
                                //Allocate UVs
                                uvs[3 + (4 * FaceCounter)] = new Vector2(0.0f, -1.0f * dimY); //0,1
                                uvs[0 + (4 * FaceCounter)] = new Vector2(-1.0f * dimX, -1.0f * dimY); //1,1
                                uvs[1 + (4 * FaceCounter)] = new Vector2(-1.0f * dimX, 0.0f);  //1,0
                                uvs[2 + (4 * FaceCounter)] = new Vector2(0.0f, 0.0f);         //0,0
                                
                               
                                break;
                            }

                        case vNORTH:
                            {
                                //north wall vertices                               
                                MatsToUse[FaceCounter] = WallTexture(fNORTH, t);
                                verts[0 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 1.2f * dimY);
                                verts[1 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 1.2f * dimY);
                                verts[2 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 1.2f * dimY);
                                verts[3 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 1.2f * dimY);

                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, uv1);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(dimX, uv1);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(dimX, uv0);
                                break;
                            }

                        case vWEST:
                            {
                                //west wall vertices                                
                                MatsToUse[FaceCounter] = WallTexture(fWEST, t);
                                verts[0 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 1.2f * dimY);
                                verts[1 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 1.2f * dimY);
                                verts[2 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 0f);
                                verts[3 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 0f);
                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, uv1);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(dimY, uv1);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(dimY, uv0);

                                break;
                            }

                        case vEAST:
                            {
                                //east wall vertices                                
                                MatsToUse[FaceCounter] = WallTexture(fEAST, t);
                                verts[0 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 0f);
                                verts[1 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 0f);
                                verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 1.2f * dimY);
                                verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 1.2f * dimY);
                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, uv1);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(dimY, uv1);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(dimY, uv0);

                                break;
                            }

                        case vSOUTH:
                            {
                                MatsToUse[FaceCounter] = WallTexture(fSOUTH, t);
                                //south wall vertices
                                verts[0 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 0f);
                                verts[1 + (4 * FaceCounter)] = new Vector3(0f, floorHeight, 0f);
                                verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, floorHeight, 0f);
                                verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 0f);
                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, uv0);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, uv1);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(dimX, uv1);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(dimX, uv0);

                                break;
                            }
                        case vBOTTOM:
                            {
                                //bottom wall vertices
                                MatsToUse[FaceCounter] = FloorTexture(t);
                                verts[0 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 1.2f * dimY);
                                verts[1 + (4 * FaceCounter)] = new Vector3(0f, baseHeight, 0f);
                                verts[2 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 0f);
                                verts[3 + (4 * FaceCounter)] = new Vector3(-1.2f * dimX, baseHeight, 1.2f * dimY);
                                //Change default UVs
                                uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, 0.0f);
                                uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, 1.0f * dimY);
                                uvs[2 + (4 * FaceCounter)] = new Vector2(dimX, 1.0f * dimY);
                                uvs[3 + (4 * FaceCounter)] = new Vector2(dimX, 0.0f);
                                break;
                            }
                    }
                    FaceCounter++;
                }
            }


            var a_mesh = new ArrayMesh(); //= Mesh as ArrayMesh;
                                          //create normals from verts
            var normals = new List<Vector3>();
            foreach (var vert in verts)
            {
                normals.Add(vert.Normalized());
            }

            //Apply the uvs and create my tris
            // mesh.vertices = verts;
            // mesh.uv = uvs;
            FaceCounter = 0;
            int curFace = 0;
            int[] indices;// = new int[6];

            for (int i = 0; i < 6; i++)
            {
                if (curFace == vTOP)
                {
                    indices = new int[3];
                }
                else
                {
                    indices = new int[6];
                }

                if (t.VisibleFaces[i] == true)
                {
                    if (i == vTOP)
                    {
                        switch (t.tileType)
                        {
                            case TILE_DIAG_NE:
                                indices[0] = 1 + (4 * FaceCounter);
                                indices[1] = 2 + (4 * FaceCounter);
                                indices[2] = 3 + (4 * FaceCounter);
                                break;
                            case TILE_DIAG_SE:
                                indices[0] = 0 + (4 * FaceCounter);
                                indices[1] = 2 + (4 * FaceCounter);
                                indices[2] = 3 + (4 * FaceCounter);
                                break;
                            case TILE_DIAG_SW:
                                indices[0] = 0 + (4 * FaceCounter);
                                indices[1] = 1 + (4 * FaceCounter);
                                indices[2] = 3 + (4 * FaceCounter);
                                break;
                            case TILE_DIAG_NW:
                            default:
                                indices[0] = 0 + (4 * FaceCounter);
                                indices[1] = 1 + (4 * FaceCounter);
                                indices[2] = 2 + (4 * FaceCounter);
                                break;
                        }

                        //tris[3]=0+(4*FaceCounter);
                        //tris[4]=2+(4*FaceCounter);
                        //tris[5]=3+(4*FaceCounter);
                        //mesh.SetTriangles(indices, FaceCounter);
                        AddSurfaceToMesh(verts, uvs, MatsToUse, FaceCounter, a_mesh, normals, indices);
                    }
                    else
                    {
                        indices[0] = 0 + (4 * FaceCounter);
                        indices[1] = 1 + (4 * FaceCounter);
                        indices[2] = 2 + (4 * FaceCounter);
                        indices[3] = 0 + (4 * FaceCounter);
                        indices[4] = 2 + (4 * FaceCounter);
                        indices[5] = 3 + (4 * FaceCounter);
                        // mesh.SetTriangles(indices, FaceCounter);
                        AddSurfaceToMesh(verts, uvs, MatsToUse, FaceCounter, a_mesh, normals, indices);
                    }
                    FaceCounter++;
                    curFace++;
                }
            }

            return CreateMeshInstance(parent, x, y, TileName, a_mesh);

            // mr.materials = MatsToUse;//mats;
            // mesh.RecalculateNormals();
            // mesh.RecalculateBounds();
            // mf.mesh = mesh;
            // if (EnableCollision)
            // {
            //     MeshCollider mc = Tile.AddComponent<MeshCollider>();
            //     mc.sharedMesh = null;
            //     mc.sharedMesh = mesh;
            // }
            // return Tile;
        }


        /// <summary>
        /// Adds a surface built from the various uv, vertices and materials arrays to a mesh
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="uvs"></param>
        /// <param name="MatsToUse"></param>
        /// <param name="FaceCounter"></param>
        /// <param name="a_mesh"></param>
        /// <param name="normals"></param>
        /// <param name="indices"></param>
        private static void AddSurfaceToMesh(Vector3[] verts, Vector2[] uvs, int[] MatsToUse, int FaceCounter, ArrayMesh a_mesh, List<Vector3> normals, int[] indices, int faceCounterAdj = 0)
        {
            var surfaceArray = new Godot.Collections.Array();
            surfaceArray.Resize((int)Mesh.ArrayType.Max);

            surfaceArray[(int)Mesh.ArrayType.Vertex] = verts; //.ToArray();
            surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs; //.ToArray();
            surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
            surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

            //Add the new surface to the mesh
            a_mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
            //MatsToUse[FaceCounter]=38; //TEMP to allow fixing uvs
            a_mesh.SurfaceSetMaterial(FaceCounter + faceCounterAdj, mapTextures.GetMaterial(MatsToUse[FaceCounter])); //  surfacematerial.Get(MatsToUse[FaceCounter]));
        }


        static void CalcUVForSlopedCuboid(int Top, int Bottom, out float uv0, out float uv1)
        {
            float PolySize = Top - Bottom;
            uv0 = (float)(Bottom * 0.125f);
            uv1 = +(PolySize / 8.0f) + (uv0);
        }         



    } //end class

} //end namespace