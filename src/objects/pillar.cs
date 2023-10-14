using System;
using Godot;

namespace Underworld
{
    public class pillar: model3D
    {
        Vector3 position;
        public static GRLoader tmObj; //3d model textures.

        public static pillar CreateInstance(Node3D parent, uwObject obj, Vector3 position)
        {
            var n = new pillar(obj, position);  
            var modelNode = n.Generate3DModel(parent);	
            modelNode.Rotate(Vector3.Up,(float)Math.PI);	
            return n;                  
        }


        public pillar(uwObject _uwobject, Vector3 _position)
        {
            uwobject =_uwobject;
            position = _position;
        }

        public override int NoOfMeshes()
        {
            return 6;
        }


        public override int[] ModelTriangles(int meshNo)
        {//builds a cuboid with
            int FaceCounter = 0;
            int[] tris = new int[6];
            for (int i = 0; i < NoOfMeshes(); i++)
            {
                if (i == meshNo)
                {
                    tris[0] = 0 + (4 * FaceCounter);
                    tris[1] = 1 + (4 * FaceCounter);
                    tris[2] = 2 + (4 * FaceCounter);
                    tris[3] = 0 + (4 * FaceCounter);
                    tris[4] = 2 + (4 * FaceCounter);
                    tris[5] = 3 + (4 * FaceCounter);
                    return tris;
                }
                FaceCounter++;
            }
            return base.ModelTriangles(meshNo);
        }

        public override Vector3[] ModelVertices()
        {
            Vector3[] Verts = new Vector3[24];
            float x1 = 0.03f;
            float x0 = -0.03f;
            float y0 = -0.03f;
            float y1 = 0.03f;
            float z1 = (float)( CEILING_HEIGHT * 0.15f) - position.Y;
            float z0 = -position.Y;
            int t = 0;

            Verts[t++] = new Vector3(x0, z0, y1);
Verts[t++] = new Vector3(x0, z1, y1);
Verts[t++] = new Vector3(x1, z1, y1);
Verts[t++] = new Vector3(x1, z0, y1);

Verts[t++] = new Vector3(x1, z0, y0);
Verts[t++] = new Vector3(x1, z1, y0);
Verts[t++] = new Vector3(x0, z1, y0);
Verts[t++] = new Vector3(x0, z0, y0);

Verts[t++] = new Vector3(x1, z0, y1);
Verts[t++] = new Vector3(x1, z1, y1);
Verts[t++] = new Vector3(x1, z1, y0);
Verts[t++] = new Vector3(x1, z0, y0);

Verts[t++] = new Vector3(x0, z0, y0);
Verts[t++] = new Vector3(x0, z0, y1);
Verts[t++] = new Vector3(x1, z0, y1);
Verts[t++] = new Vector3(x1, z0, y0);

Verts[t++] = new Vector3(x0, z0, y0);
Verts[t++] = new Vector3(x0, z1, y0);
Verts[t++] = new Vector3(x0, z1, y1);
Verts[t++] = new Vector3(x0, z0, y1);

Verts[t++] = new Vector3(x1, z1, y0);
Verts[t++] = new Vector3(x1, z1, y1);
Verts[t++] = new Vector3(x0, z1, y1);
Verts[t++] = new Vector3(x0, z1, y0);



            
            return Verts;
        }

        public override Vector2[] ModelUVs(Vector3[] verts)
        {
            Vector2[] UVs = new Vector2[24];
            for (int j = 0; j < 6; j++)
                {
                    UVs[(j * 4) + 0] = new Vector2(0f, 0f);
                    UVs[(j * 4) + 1] = new Vector2(0f, CEILING_HEIGHT);
                    UVs[(j * 4) + 2] = new Vector2(1f, CEILING_HEIGHT);
                    UVs[(j * 4) + 3] = new Vector2(1f, 0f);
                }
            return UVs;
        }

        public override int ModelColour(int meshNo)
        {
            return uwobject.flags & 0x3;
        }

        public override ShaderMaterial GetMaterial(int textureno)
        {//Get the material texture from tmobj
            if (tmObj==null)
            {
                tmObj = new GRLoader(GRLoader.TMOBJ_GR, GRLoader.GRShaderMode.TextureShader);
                tmObj.RenderGrey=true;
            }
           return tmObj.GetMaterial((byte)textureno);
        }
    }
}//end namespace