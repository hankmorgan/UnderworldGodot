using System.Diagnostics;
using Godot;

namespace Underworld
{
    public class tmap : model3D
    {
        Node3D tmapnode;

        //Depth of the flat's drawn face. Builtin models 20 and 22 both sit
        //16 model units (0.0625 tile = 0.075 world units) proud of their
        //origin plane. The door case needs a little more so it does not
        //fight the door texture.
        float tmapDepth = 0.075f;

        //Item 0x016E is the small texture map (builtin model 20); 0x016F is
        //the full tile one (model 22).
        bool smallTmap = false;

        public tmap(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public static tmap CreateInstance(Node3D parent, uwObject obj, UWTileMap a_tilemap, string name)
        {
            int tileX = obj.tileX;
            int tileY = obj.tileY;
            var t = new tmap(obj);
            t.smallTmap = (obj.item_id == 0x016E);

            //A tmap sharing a tile with a door (UW1 level 4) must sit slightly
            //further out so it does not z-fight the door.
            if (UWTileMap.ValidTile(obj.tileX, obj.tileY))
            {
                var tile = UWTileMap.current_tilemap.Tiles[obj.tileX, obj.tileY];
                var door = objectsearch.FindMatchInObjectChain(tile.indexObjectList, 5, 0, -1, UWTileMap.current_tilemap.LevelObjects);
                if (door != null)
                {
                    if ((door.xpos == obj.xpos) && (door.ypos == obj.ypos))
                    {
                        Debug.Print($"Tmap {obj.index} shares space with door {door.index}");
                        t.tmapDepth = 0.1f;
                    }
                }
            }

            t.tmapnode = t.Generate3DModel(parent, name);

            SetModelRotation(parent, t);

            var tileType = UWTileMap.ValidTile(obj.tileX, obj.tileY)
                ? a_tilemap.Tiles[obj.tileX, obj.tileY].tileType : (short)-1;
            if (IsDiagonalTile(tileType))
                PlaceOnDiagonalWall(parent, t, tileX, tileY, tileType);
            else
                PlaceOnWall(parent, t, tileX, tileY);

            return t;
        }

        static bool IsDiagonalTile(short tileType) =>
            tileType is UWTileMap.TILE_DIAG_SE or UWTileMap.TILE_DIAG_SW
                or UWTileMap.TILE_DIAG_NE or UWTileMap.TILE_DIAG_NW;

        /// <summary>
        /// The direction from the wall into the room for a flat's heading.
        /// World +X is west and world +Z is north in this port.
        /// </summary>
        static Vector3 RoomDirection(int heading)
        {
            const float d = 0.70710678f;
            switch (heading)
            {
                case 0: return new Vector3(0f, 0f, -1f); //south
                case 1: return new Vector3(d, 0f, -d);  //south-west
                case 2: return new Vector3(1f, 0f, 0f);  //west
                case 3: return new Vector3(d, 0f, d);   //north-west
                case 4: return new Vector3(0f, 0f, 1f);  //north
                case 5: return new Vector3(-d, 0f, d);  //north-east
                case 6: return new Vector3(-1f, 0f, 0f); //east
                default: return new Vector3(-d, 0f, -d); //south-east
            }
        }

        /// <summary>
        /// Places a flat on the wall face its heading belongs to. The object's
        /// xpos/ypos only give an approximate position in the tile, so the flat
        /// used to float short of the wall (more on north/east walls than on
        /// south/west). Cardinal walls are at the tile boundary.
        /// </summary>
        static void PlaceOnWall(Node3D parent, tmap t, int tileX, int tileY)
        {
            const float clearance = 0.01f; //sit just in front of the wall to avoid z-fighting
            float tx = tileX * 1.2f;
            float ty = tileY * 1.2f;
            //The drawn face is t.tmapDepth along the node's local +Z, and local
            //+Z points into the room. Offset the node back by that amount so the
            //face lands on the wall plane.
            float roomFactor = clearance - t.tmapDepth;
            float y = parent.Position.Y;
            var room = RoomDirection(t.uwobject.heading);

            switch (t.uwobject.heading)
            {
                case 0: //north wall, faces south
                    parent.Position = new Vector3(-(tx + 0.6f), y, (ty + 1.2f) + (roomFactor * room.Z));
                    break;
                case 4: //south wall, faces north
                    parent.Position = new Vector3(-(tx + 0.6f), y, ty + (roomFactor * room.Z));
                    break;
                case 2: //east wall, faces west
                    parent.Position = new Vector3(-(tx + 1.2f) + (roomFactor * room.X), y, ty + 0.6f);
                    break;
                case 6: //west wall, faces east
                    parent.Position = new Vector3(-tx + (roomFactor * room.X), y, ty + 0.6f);
                    break;
                default: //diagonal heading: keep the flat centred over the tile centre
                    parent.Position = new Vector3(
                        -(tx + 0.6f) + (roomFactor * room.X),
                        y,
                        ty + 0.6f + (roomFactor * room.Z));
                    break;
            }
        }

        /// <summary>
        /// Diagonal walls cross the tile centre, so place the flat at the
        /// object's position and push it along the wall normal until the
        /// nearest corner of its real quad is just in front of the wall.
        /// This mirrors the diagonal handling added in #56 but uses the
        /// actual quad size instead of assuming a full tile.
        /// </summary>
        static void PlaceOnDiagonalWall(Node3D parent, tmap t, int tileX, int tileY, short tileType)
        {
            parent.Position = t.uwobject.GetCoordinate();
            const float s = 0.707106781f;
            var inward = new Vector3(
                tileType is UWTileMap.TILE_DIAG_SE or UWTileMap.TILE_DIAG_NE ? -s : s,
                0f,
                tileType is UWTileMap.TILE_DIAG_SE or UWTileMap.TILE_DIAG_SW ? -s : s).Normalized();
            var tileCenter = new Vector3(-(tileX * 1.2f + 0.6f), parent.Position.Y, tileY * 1.2f + 0.6f);
            float minDepth = float.MaxValue;
            foreach (var local in t.ModelVertices())
            {
                minDepth = Mathf.Min(minDepth, (parent.GlobalTransform * local - tileCenter).Dot(inward));
            }
            parent.Position += inward * Mathf.Clamp(0.01f - minDepth, -0.12f, 0.12f);
        }

        public static bool LookAt(uwObject obj)
        {
            int textureindex = UWTileMap.current_tilemap.texture_map[obj.owner];
            uimanager.AddToMessageScroll(GameStrings.TextureDescription(textureindex));
            if ((textureindex == 142) && ((_RES != GAME_UW2)))
            {//This is a window into the abyss.
                uimanager.DisplayCutsImage(cutsfile: "cs400.n01", imageNo: playerdat.dungeon_level - 1, targetControl: uimanager.CutsSmall);
            }
            return true; //prevents the default you cannot use message
        }


        public override Vector3[] ModelVertices()
        {
            //Model 20 (0x016E) spans 0.25 of a tile (0.3 world units), model 22
            //(0x016F) spans a full tile (1.2). Both quads sit 0.0625 tile proud
            //of the origin and are not centred on it, so the node is centred on
            //the visible quad here rather than on the model origin.
            float halfWidth = smallTmap ? 0.15f : 0.6f;
            float height = smallTmap ? 0.3f : 1.2f;
            Vector3[] v = new Vector3[4];
            v[0] = new Vector3(-halfWidth, 0f, tmapDepth);
            v[1] = new Vector3(halfWidth, 0f, tmapDepth);
            v[2] = new Vector3(halfWidth, height, tmapDepth);
            v[3] = new Vector3(-halfWidth, height, tmapDepth);
            return v;
        }

        public override int[] ModelTriangles(int meshNo)
        {
            //face
            int[] tris = new int[6];
            tris[0] = 1;
            tris[1] = 0;
            tris[2] = 3;
            tris[3] = 3;
            tris[4] = 2;
            tris[5] = 1;
            return tris;
        }

        public override Vector2[] ModelUVs(Vector3[] verts)
        {
            Vector2[] v = new Vector2[4];
            v[0] = new Vector2(0, 1);
            v[1] = new Vector2(1, 1);
            v[2] = new Vector2(1, 0);
            v[3] = new Vector2(0, 0);
            return v;
        }


        public override ShaderMaterial GetMaterial(int textureno, int surface)
        {//Get the material texture from tmobj   
            if (surface != 6)
            {
                return tileMapRender.mapTexturesWalls.GetMaterialForObject(
                    textureno: uwobject.owner,
                    texturemap: UWTileMap.current_tilemap.texture_map,
                    obj: uwobject);
            }
            else
            {
                return base.GetMaterial(0, 6);
            }
        }
    } //end class
}//end namespace
