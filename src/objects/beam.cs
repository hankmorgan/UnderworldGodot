using System;
using Godot;

namespace Underworld
{
    public class beam : model3D
    {
        public static beam CreateInstance(Node3D parent, uwObject obj, string name)
        {
            var b = new beam(obj);
            var modelNode = b.Generate3DModel(parent, name);
            modelNode.Rotate(Vector3.Up, (float)Math.PI);
            SetModelRotation(parent, b);
            //DisplayModelPoints(b, parent);           
            return b;
        }

        public beam(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public override int[] ModelTriangles(int meshNo)
        {
            //return new int[] { 4, 7, 6, 4, 6, 5, 6, 7, 2, 7, 3, 2, 5, 2, 1, 5, 6, 2, 7, 0, 3, 7, 4, 0, 4, 1, 0, 4, 5, 1, 0, 1, 2, 0, 2, 3, 8, 18, 21, 8, 19, 18, 8, 21, 19, 19, 21, 18, 15, 25, 24, 15, 24, 27, 15, 24, 25, 15, 27, 24, 26, 15, 25, 15, 26, 14, 14, 13, 15, 13, 12, 15, 13, 26, 25, 13, 25, 12, 9, 8, 10, 10, 8, 11, 9, 20, 19, 9, 19, 8, 11, 8, 15, 12, 11, 15, 22, 23, 24, 22, 24, 27, 31, 22, 27, 31, 27, 30, 31, 30, 23, 23, 30, 24, 17, 21, 18, 17, 16, 21, 16, 28, 29, 16, 29, 21, 30, 27, 21, 30, 21, 29, 28, 17, 29, 17, 18, 29, 20, 10, 19, 10, 11, 19 };

            var tris = new int[36];
            tris[0] = 7;
            tris[1] = 6;
            tris[2]= 2;
            tris[3] = 2;
            tris[4] = 3;
            tris[5] = 7;

            tris[6] = 6;
            tris[7] = 5;
            tris[8] = 1;
            tris[9] = 1;
            tris[10] = 2;
            tris[11] = 6;

            tris[12] = 0;
            tris[13] = 1;
            tris[14] = 5;
            tris[15] = 5;
            tris[16] = 4;
            tris[17] = 0;

            tris[18] = 3;
            tris[19] = 0;
            tris[20] = 4;
            tris[21] = 4;
            tris[22] = 7;
            tris[23] = 3;

            //ends
            tris[24] = 3;
            tris[25] = 2;
            tris[26] = 1;
            tris[27] = 1;
            tris[28] = 0;
            tris[29] = 3;

            tris[30] = 4;
            tris[31] = 5;
            tris[32] = 6;
            tris[33] = 6;
            tris[34] = 7;
            tris[35] = 4;

            return tris;
            

        }

        public override Vector3[] ModelVertices()
        {
            Vector3[] ModelVerts = new Vector3[8];
            ModelVerts[0] = new Vector3(0f, 0f, 0f);
            ModelVerts[1] = new Vector3(0f, 0.0625f, 0f);
            ModelVerts[2] = new Vector3(0.0625f, 0.0625f, 0f);
            ModelVerts[3] = new Vector3(0.0625f, 0f, 0f);
            ModelVerts[4] = new Vector3(0f, 0f, 0.5f);
            ModelVerts[5] = new Vector3(0f, 0.0625f, 0.5f);
            ModelVerts[6] = new Vector3(0.0625f, 0.0625f, 0.5f);
            ModelVerts[7] = new Vector3(0.0625f, 0f, 0.5f);
            for (int i=0; i<=ModelVerts.GetUpperBound(0); i++)
            {
                ModelVerts[i] = ModelVerts[i] * new Vector3(1.4f,1.4f,1.4f);
            }
            return ModelVerts;
        }

        public override int ModelColour(int meshNo)
        {
            if (_RES == GAME_UW2)
            {
                return 32;
            }
            else
            {
                return 30;
            }
        }

        public override Vector2[] ModelUVs(Vector3[] verts)
        {
            var uvs= base.ModelUVs(verts);
            uvs[2] = new Vector2(0,0);
            uvs[3] = new Vector2(1,0);
            uvs[6] = new Vector2(0,2);
            uvs[7] = new Vector2(1,2);
            
            uvs[0] = new Vector2(0,0);
            uvs[1] = new Vector2(1,0);
            uvs[4] = new Vector2(0,2);
            uvs[5] = new Vector2(1,2);
            
           
            return uvs;
        }

        public override ShaderMaterial GetMaterial(int textureno, int surface)
        {
            switch(surface)
            {
                case 0:
                    return GetTmObj.GetMaterial((byte)ModelColour(surface));
            }
            return base.GetMaterial(textureno, surface);
        }

    }
}//end namespace