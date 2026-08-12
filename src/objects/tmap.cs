using Godot;

namespace Underworld
{
    public class tmap:model3D
    {
        //int texture;
        Node3D tmapnode;
        float tmapOffset
        {
            get
            {
                //check if tmap shares space with a door, this deals with a tmap that is over a door in level 4 of UW1
                if (UWTileMap.ValidTile(uwobject.tileX, uwobject.tileY))
                {
                    var tile = UWTileMap.current_tilemap.Tiles[uwobject.tileX, uwobject.tileY];

                    var door = objectsearch.FindMatchInObjectChain(tile.indexObjectList, 5, 0, -1, UWTileMap.current_tilemap.LevelObjects);
                    if (door!=null)
                    {
                        if ((door.xpos == uwobject.xpos) && (door.ypos == uwobject.ypos))
                        {
                            //Debug.Print($"Tmap {obj.index} shares space with door {door.index}");
                            return 0.1f;
                        }
                    }

                    if (IsDiagonalTile(tile.tileType))
                        return 0.03f;
                }

                switch (uwobject.heading)
                {
                    case 0:
                        {
                            if (uwobject.ypos == 7)
                            {
                                return -0.13f;
                            }
                            break;
                        }
                    case 2:
                        {
                            if (uwobject.xpos == 7)
                            {
                                return -0.13f;
                            }
                            break;
                        }
                    case 4:
                        {
                            if (uwobject.ypos == 0)
                            {
                                return +0.13f;
                            }
                            break;
                        }
                    case 6:
                        {
                            if (uwobject.xpos == 0)
                            {
                                return +0.13f;
                            }
                            break;
                        }
                }
                return 0.07f; //= 0.07f;//how far out the tmap extrudes from it's origin
            }
        }

        static bool IsDiagonalTile(short tileType) =>
            tileType is UWTileMap.TILE_DIAG_SE or UWTileMap.TILE_DIAG_SW
                or UWTileMap.TILE_DIAG_NE or UWTileMap.TILE_DIAG_NW;

        static void PlaceOnDiagonalWall(Node3D parent, uwObject obj, float extrude, short tileType)
        {
            parent.Position = obj.GetCoordinate();
            const float s = 0.707106781f;
            var inward = new Vector3(
                tileType is UWTileMap.TILE_DIAG_SE or UWTileMap.TILE_DIAG_NE ? -s : s,
                0f,
                tileType is UWTileMap.TILE_DIAG_SE or UWTileMap.TILE_DIAG_SW ? -s : s).Normalized();
            var tileCenter = new Vector3(-(obj.tileX * 1.2f + 0.6f), parent.Position.Y, obj.tileY * 1.2f + 0.6f);
            float minDepth = float.MaxValue;
            foreach (var local in new Vector3[]
            {
                new(-0.6f, 0f, extrude), new(0.6f, 0f, extrude),
                new(0.6f, 1.2f, extrude), new(-0.6f, 1.2f, extrude),
            })
                minDepth = Mathf.Min(minDepth, (parent.GlobalTransform * local - tileCenter).Dot(inward));
            parent.Position += inward * Mathf.Clamp(0.025f - minDepth, -0.12f, 0.12f);
        }

        public tmap(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public static tmap CreateInstance(Node3D parent, uwObject obj, UWTileMap a_tilemap, string name)
        {
            var t = new tmap(obj);
            
            //t.texture = obj.owner; //a_tilemap.texture_map[obj.owner];    
            t.tmapnode = t.Generate3DModel(parent, name);
           
            SetModelRotation(parent,t);
            var tileType = UWTileMap.ValidTile(obj.tileX, obj.tileY)
                ? a_tilemap.Tiles[obj.tileX, obj.tileY].tileType : (short)-1;
            if (IsDiagonalTile(tileType))
                PlaceOnDiagonalWall(parent, obj, t.tmapOffset, tileType);
            else
                centreAlongAxis(parent, t);

            // //adjust to be closer to walls
            // if (obj.xpos == 0)
            // {
            //     parent.Position += new Vector3(+0.05f, 0f, 0f);
            // }
            // if (obj.ypos == 0)
            // {
            //     parent.Position += new Vector3(0f, 0f, -0.05f);
            // }
            // if (obj.xpos == 7)
            // {
            //     parent.Position += new Vector3(-0.05f, 0f, 0f);
            // }
            // if (obj.ypos == 7)
            // {
            //     parent.Position += new Vector3(0f, 0f, +0.05f);
            // }



            //DisplayModelPoints(t, parent);
            return t;
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
            var offset = tmapOffset;
            Vector3[] v = new Vector3[4];
            v[0] = new Vector3(-0.6f, 0f, offset);//0.0625f);
            v[1] = new Vector3(0.6f, 0f, offset);//0.0625f);
            v[2] = new Vector3(0.6f, 1.2f, offset);//0.0625f);
            v[3] = new Vector3(-0.6f, 1.2f, offset);//..0.0625f);
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
            v[0] = new Vector2(0,1); 
            v[1] = new Vector2(1,1); 
            v[2] = new Vector2(1,0); 
            v[3]  = new Vector2(0,0); 
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
