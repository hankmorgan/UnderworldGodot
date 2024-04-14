using System;
using Godot;

namespace Underworld
{
    public class bridge : model3D
    {
        UWTileMap tilemap;
        public static bridge CreateInstance(Node3D parent, uwObject obj, string name, UWTileMap a_tilemap)
        {
            var b = new bridge(obj, a_tilemap);
            //TODO: some bridges can be invisible. instead of a model with texture do a 3d collider with no texture
            var modelNode = b.Generate3DModel(parent, name);
            modelNode.Rotate(Vector3.Up, (float)Math.PI/2);
            SetModelRotation(parent, b);
            centreInTile(parent, b);
            //DisplayModelPoints(b,modelNode);
            //mark bridge on automap
            if (UWTileMap.ValidTile(obj.tileX, obj.tileY))
            {
                var textureindex = (obj.enchantment<<3) | (int)obj.flags;
                if (textureindex<=2)
                {
                    UWTileMap.current_tilemap.Tiles[obj.tileX, obj.tileY].hasBridge = true;
                }                
            }            
            return b;
        }

        public bridge(uwObject _uwobject, UWTileMap _tilemap)
        {            
            uwobject = _uwobject;
            tilemap = _tilemap;
        }

        public override int NoOfMeshes()
        {
            return 2;
        }

        public override Vector3[] ModelVertices()
        {
            Vector3[] Verts = new Vector3[8];
         
            float x0 = -0.6f;
            float x1 = 0.6f;
            
            float z0 = -0.075f; 
            float z1 = 0.075f;
            
            float y0 = -0.6f;
            float y1 =  0.6f;
           
            //x1
            Verts[0] = new Vector3(x0, z0, y0);
            Verts[1] = new Vector3(x0, z0, y1);
            Verts[2] = new Vector3(x0, z1, y0);
            Verts[3] = new Vector3(x0, z1, y1);

            Verts[4] = new Vector3(x1, z0, y0);
            Verts[5] = new Vector3(x1, z0, y1);
            Verts[6] = new Vector3(x1, z1, y0);
            Verts[7] = new Vector3(x1, z1, y1);

            return Verts;
        }

        public override Vector2[] ModelUVs(Vector3[] verts)
        {
            var uvs= base.ModelUVs(verts);

            uvs[6] = new Vector2(0f,0f);  //top
            uvs[7] = new Vector2(1f,0f);
            uvs[2] = new Vector2(0f,1f);
            uvs[3] = new Vector2(1f,1f);

            uvs[0] = new Vector2(0f,0f);
            uvs[1] = new Vector2(1f,0f);
            uvs[4] = new Vector2(0f,1f);
            uvs[5] = new Vector2(1f,1f);
            return uvs;
        }

        public override int[] ModelTriangles(int meshNo)
        {
            switch (meshNo)
            {
                case 0:
                {
                    int[] tris = new int[12];// top and bottom surface
                    tris[0] = 2;
                    tris[1] = 6;
                    tris[2] = 7;
                    tris[3] = 7;
                    tris[4] = 3;
                    tris[5] = 2;

                    tris[6] = 4;
                    tris[7] = 0;
                    tris[8] = 1;
                    tris[9] = 1;
                    tris[10] = 5;
                    tris[11] = 4;

                    return tris;
                }
                case 1: // trim
                {
                    int[] tris = new int[24];
                    tris[0] = 0;
                    tris[1] = 4;
                    tris[2] = 6;
                    tris[3] = 6;
                    tris[4] = 2;
                    tris[5] = 0;

                    tris[6] = 4;
                    tris[7] = 5;
                    tris[8] = 7;
                    tris[9] = 7;
                    tris[10] = 6;
                    tris[11] = 4;

                    tris[12] = 5;
                    tris[13] = 1;
                    tris[14] = 3;
                    tris[15] = 3;
                    tris[16] = 7;
                    tris[17] = 5;

                    tris[18] = 1;
                    tris[19] = 0;
                    tris[20] = 2;
                    tris[21] = 2;
                    tris[22] = 3;
                    tris[23] = 1;
                    return tris;
                }
               
            }
            return base.ModelTriangles(meshNo);
        }

        public override int ModelColour(int meshNo)
        {
            switch (meshNo)
                {
                    case 0:
                        return 0;// this will be set later.
                    case 1:
                        return 126;
                }
            return base.ModelColour(meshNo);
        }

        public override ShaderMaterial GetMaterial(int textureno, int surface)
        {//Get the material texture from tmobj
        switch (surface)
            {
                case 0:
                {
                    var textureindex = (uwobject.enchantment<<3) | (int)uwobject.flags;
                    if (textureindex >= 2)
                    {
                        if (_RES==GAME_UW2)
                        {
                            textureindex = textureindex-2; //tilemap.texture_map[textureindex-2];
                        }
                        else
                        {
                            textureindex = textureindex-2+48; //tilemap.texture_map[textureindex-2+48];
                        }
                        return tileMapRender.mapTextures.GetMaterial(textureindex, tilemap.texture_map);
                    }
                    else
                    {
                    return GetTmObj.GetMaterial(30 + textureindex);
                    }
                }
            }
            return base.GetMaterial(textureno,surface);
        }
    }//end class
}// end namespace