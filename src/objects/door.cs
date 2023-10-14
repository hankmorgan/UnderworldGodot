using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
namespace Underworld
{
    public class door : model3D
    {
        const float doorwidth = 0.8f;
        const float doorframewidth = 1.2f;
        const float doorSideWidth = (doorframewidth - doorwidth) / 2f;
        const float doorheight = 7f * 0.15f;

        public int texture;

        public static door CreateInstance(Node3D parent, uwObject obj, TileMap a_tilemap)
        {
            var n = new door(obj);            
            var modelNode = n.Generate3DModel(parent);	
            //modelNode.Rotate(Vector3.Up,(float)Math.PI);
            n.texture = a_tilemap.texture_map[a_tilemap.Tiles[obj.tileX, obj.tileY].wallTexture];	
            return n;
        }
        public door(uwObject _uwobject)
        {
            uwobject = _uwobject;            
        }

        public override int NoOfMeshes()
        {
            return 1;
        }

        public override Vector3[] ModelVertices()
        {
            Vector3[] v = new Vector3[20];
            v[0] = new Vector3(-0.1875f, 0f, 0f);
            v[1] = new Vector3(-0.1875f, 0.8125f, 0f);
            v[2] = new Vector3(0.3125f, 0.8125f, 0f);
            v[3] = new Vector3(0.3125f, 0f, 0f);
            v[4] = new Vector3(-0.1875f, 0f, 0.3125f);
            v[5] = new Vector3(-0.1875f, 0.8125f, 0.3125f);
            v[6] = new Vector3(0.3125f, 0.8125f, 0.3125f);
            v[7] = new Vector3(0.3125f, 0f, 0.3125f);
            v[8] = new Vector3(-0.4375f, 0f, 0f);
            v[9] = new Vector3(-0.4375f, 0f, 0.3125f);
            v[10] = new Vector3(0.5625f, 0f, 0f);
            v[11] = new Vector3(0.5625f, 0f, 0.3125f);
            v[12] = new Vector3(-0.4375f, 0.8125f, 0f);
            v[13] = new Vector3(-0.4375f, 0.8125f, 0.3125f);
            v[14] = new Vector3(0.5625f, 0.8125f, 0f);
            v[15] = new Vector3(0.5625f, 0.8125f, 0.3125f);
            v[16] = new Vector3(-0.4375f, 4f, 0f);
            v[17] = new Vector3(-0.4375f, 4f, 0.3125f);
            v[18] = new Vector3(0.5625f, 4f, 0f);
            v[19] = new Vector3(0.5625f, 4f, 0.3125f);
            return v;
        }

        public override int[] ModelTriangles(int meshNo)
        {
                int[] tris = new int[6];
                    tris[0] = 16 ;
                    tris[1] = 18;
                    tris[2] = 14;
                    tris[3] = 14;
                    tris[4] = 12;
                    tris[5] = 16;
                return tris;
        }

        public override ShaderMaterial GetMaterial(int textureno)
        {//Get the material texture from tmobj

            return tileMapRender.mapTextures.GetMaterial(this.uwobject.tileX);

        }


    }//end class
} // end namespace