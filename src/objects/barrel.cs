using Godot;
using Microsoft.VisualBasic;
namespace Underworld
{
    public class barrel : model3D
    {

        public static barrel CreateInstance(Node3D parent, uwObject obj, string name)
        {
            var b = new barrel(obj);
            var modelNode = b.Generate3DModel(parent, name);
            SetModelRotation(parent, b);
            return b;
        }

        public barrel(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }
        public override int NoOfMeshes()
        {
            return 10;
        }


        public override int[] ModelTriangles(int meshNo)
        {
            //return new int[] { 2, 1, 0, 3, 2, 0, 4, 2, 3, 5, 4, 3, 6, 4, 5, 7, 6, 5, 1, 9, 8, 0, 1, 8, 11, 10, 1, 2, 11, 1, 12, 11, 2, 4, 12, 2, 13, 12, 4, 6, 13, 4, 10, 14, 9, 1, 10, 9, 16, 15, 10, 11, 16, 10, 17, 16, 11, 12, 17, 11, 18, 17, 12, 13, 18, 12, 19, 14, 10, 15, 19, 10, 14, 19, 21, 20, 14, 21, 21, 22, 23, 20, 21, 23, 14, 20, 24, 9, 14, 24, 20, 23, 25, 24, 20, 25, 9, 24, 26, 8, 9, 26, 24, 25, 27, 26, 24, 27, 29, 18, 13, 28, 29, 13, 22, 29, 28, 23, 22, 28, 28, 13, 6, 30, 28, 6, 23, 28, 30, 25, 23, 30, 30, 6, 7, 31, 30, 7, 25, 30, 31, 27, 25, 31, 22, 21, 19, 29, 22, 19, 18, 29, 19, 17, 18, 19, 16, 17, 19, 15, 16, 19, 5, 3, 0, 7, 5, 0, 31, 7, 0, 27, 31, 0, 26, 27, 0, 8, 26, 0 };

            switch (meshNo)
            {
                case 0: //lid
                    {
                        int[] tris = new int[21];
                        tris[0] = 15;
                        tris[1] = 21;
                        tris[2] = 19;

                        tris[3] = 15;
                        tris[4] = 22;
                        tris[5] = 21;

                        tris[6] = 15;
                        tris[7] = 29;
                        tris[8] = 22;

                        tris[9] = 15;
                        tris[10] = 29;
                        tris[11] = 22;

                        tris[12] = 15;
                        tris[13] = 18;
                        tris[14] = 29;

                        tris[15] = 15;
                        tris[16] = 17;
                        tris[17] = 18;

                        tris[18] = 15;
                        tris[19] = 16;
                        tris[20] = 17;

                        return tris;
                    }

                case 1: // rib 1
                    {
                        int[] tris = new int[18];
                        tris[0] = 0;
                        tris[1] = 1;
                        tris[2] = 9;
                        tris[3] = 9;
                        tris[4] = 8;
                        tris[5] = 0;

                        tris[6] = 1;
                        tris[7] = 10;
                        tris[8] = 14;
                        tris[9] = 14;
                        tris[10] = 9;
                        tris[11] = 1;

                        tris[12] = 10;
                        tris[13] = 15;
                        tris[14] = 19;
                        tris[15] = 19;
                        tris[16] = 14;
                        tris[17] = 10;

                        return tris;
                    }
                case 2: // rib 2
                    {
                        int[] tris = new int[18];
                        tris[0] = 26;
                        tris[1] = 24;
                        tris[2] = 25;
                        tris[3] = 25;
                        tris[4] = 27;
                        tris[5] = 26;

                        tris[6] = 24;
                        tris[7] = 20;
                        tris[8] = 23;
                        tris[9] = 23;
                        tris[10] = 25;
                        tris[11] = 24;

                        tris[12] = 20;
                        tris[13] = 21;
                        tris[14] = 22;
                        tris[15] = 22;
                        tris[16] = 23;
                        tris[17] = 20;

                        return tris;
                    }
                case 3: // rib 3
                    {
                        int[] tris = new int[18];
                        tris[0] = 31;
                        tris[1] = 30;
                        tris[2] = 6;
                        tris[3] = 6;
                        tris[4] = 7;
                        tris[5] = 31;

                        tris[6] = 30;
                        tris[7] = 28;
                        tris[8] = 13;
                        tris[9] = 13;
                        tris[10] = 6;
                        tris[11] = 30;

                        tris[12] = 28;
                        tris[13] = 29;
                        tris[14] = 18;
                        tris[15] = 18;
                        tris[16] = 13;
                        tris[17] = 28;

                        return tris;
                    }

                case 4: // rib 4
                    {
                        int[] tris = new int[18];
                        tris[0] = 5;
                        tris[1] = 4;
                        tris[2] = 2;
                        tris[3] = 2;
                        tris[4] = 3;
                        tris[5] = 5;

                        tris[6] = 4;
                        tris[7] = 12;
                        tris[8] = 11;
                        tris[9] = 11;
                        tris[10] = 2;
                        tris[11] = 4;

                        tris[12] = 12;
                        tris[13] = 17;
                        tris[14] = 16;
                        tris[15] = 16;
                        tris[16] = 11;
                        tris[17] = 12;

                        return tris;
                    }
                case 5: // rib 5
                    {
                        int[] tris = new int[18];
                        tris[0] = 3;
                        tris[1] = 2;
                        tris[2] = 1;
                        tris[3] = 1;
                        tris[4] = 0;
                        tris[5] = 3;

                        tris[6] = 2;
                        tris[7] = 11;
                        tris[8] = 10;
                        tris[9] = 10;
                        tris[10] = 1;
                        tris[11] = 2;

                        tris[12] = 11;
                        tris[13] = 16;
                        tris[14] = 15;
                        tris[15] = 15;
                        tris[16] = 10;
                        tris[17] = 11;

                        return tris;
                    }
                case 6: // rib 6
                    {
                        int[] tris = new int[18];
                        tris[0] = 8;
                        tris[1] = 9;
                        tris[2] = 24;
                        tris[3] = 24;
                        tris[4] = 26;
                        tris[5] = 8;

                        tris[6] = 9;
                        tris[7] = 14;
                        tris[8] = 20;
                        tris[9] = 20;
                        tris[10] = 24;
                        tris[11] = 9;

                        tris[12] = 14;
                        tris[13] = 19;
                        tris[14] = 21;
                        tris[15] = 21;
                        tris[16] = 20;
                        tris[17] = 14;

                        return tris;
                    }
                case 7: // rib 7
                    {
                        int[] tris = new int[18];
                        tris[0] = 27;
                        tris[1] = 25;
                        tris[2] = 30;
                        tris[3] = 30;
                        tris[4] = 31;
                        tris[5] = 27;

                        tris[6] = 25;
                        tris[7] = 23;
                        tris[8] = 28;
                        tris[9] = 28;
                        tris[10] = 30;
                        tris[11] = 25;

                        tris[12] = 23;
                        tris[13] = 22;
                        tris[14] = 29;
                        tris[15] = 29;
                        tris[16] = 28;
                        tris[17] = 23;

                        return tris;
                    }
                case 8: // rib 8
                    {
                        int[] tris = new int[18];
                        tris[0] = 7;
                        tris[1] = 6;
                        tris[2] = 4;
                        tris[3] = 4;
                        tris[4] = 5;
                        tris[5] = 7;

                        tris[6] = 6;
                        tris[7] = 13;
                        tris[8] = 12;
                        tris[9] = 12;
                        tris[10] = 4;
                        tris[11] = 6;

                        tris[12] = 13;
                        tris[13] = 18;
                        tris[14] = 17;
                        tris[15] = 17;
                        tris[16] = 12;
                        tris[17] = 13;

                        return tris;
                    }
                case 9: // underneath
                    {
                        int[] tris = new int[21];
                        tris[0] = 0;
                        tris[1] = 5;
                        tris[2] = 3;

                        tris[3] = 0;
                        tris[4] = 7;
                        tris[5] = 5;

                        tris[6] = 0;
                        tris[7] = 31;
                        tris[8] = 7;

                        tris[9] = 0;
                        tris[10] = 27;
                        tris[11] = 31;

                        tris[12] = 0;
                        tris[13] = 26;
                        tris[14] = 27;

                        tris[15] = 0;
                        tris[16] = 8;
                        tris[17] = 26;

                        tris[18] = 0;
                        tris[19] = 26;
                        tris[20] = 8;

                        return tris;
                    }
            }

            return base.ModelTriangles(meshNo);
        }


        public override Vector2[] ModelUVs(Vector3[] verts)
        {
            //first every second rib
            var uvs = base.ModelUVs(verts);
            //rib 1
            uvs[15] = new Vector2(0, 0);
            uvs[19] = new Vector2(1, 0);
            uvs[10] = new Vector2(0, 1f);
            uvs[14] = new Vector2(1, 1f);
            uvs[1] = new Vector2(0, 0f);
            uvs[9] = new Vector2(1, 0f);
            uvs[0] = new Vector2(0, 1f);
            uvs[8] = new Vector2(1, 1f);

            //rib 2
            uvs[21] = new Vector2(0, 0);
            uvs[22] = new Vector2(1, 0);
            uvs[20] = new Vector2(0, 1f);
            uvs[23] = new Vector2(1, 1f);
            uvs[24] = new Vector2(0, 0f);
            uvs[25] = new Vector2(1, 0f);
            uvs[26] = new Vector2(0, 1f);
            uvs[27] = new Vector2(1, 1f);

            //rib 3
            uvs[29] = new Vector2(0, 0);
            uvs[18] = new Vector2(1, 0);
            uvs[28] = new Vector2(0, 1f);
            uvs[13] = new Vector2(1, 1f);
            uvs[30] = new Vector2(0, 0f);
            uvs[6] = new Vector2(1, 0f);
            uvs[31] = new Vector2(0, 1f);
            uvs[7] = new Vector2(1, 1f);

            //rib 4
            uvs[17] = new Vector2(0, 0);
            uvs[16] = new Vector2(1, 0);
            uvs[12] = new Vector2(0, 1f);
            uvs[11] = new Vector2(1, 1f);
            uvs[4] = new Vector2(0, 0f);
            uvs[2] = new Vector2(1, 0f);
            uvs[5] = new Vector2(0, 1f);
            uvs[3] = new Vector2(1, 1f);

            //then the next set of ribs

            return uvs;
        }

        public override Vector3[] ModelVertices()
        {
            Vector3[] ModelVerts = new Vector3[32];
            ModelVerts[0] = new Vector3(-0.0390625f, 0f, -0.09375f);
            ModelVerts[1] = new Vector3(-0.05859375f, 0.1171875f, -0.1210938f);
            ModelVerts[2] = new Vector3(0.046875f, 0.1171875f, -0.1289063f);
            ModelVerts[3] = new Vector3(0.03515625f, 0f, -0.09765625f);
            ModelVerts[4] = new Vector3(0.125f, 0.1171875f, -0.05859375f);
            ModelVerts[5] = new Vector3(0.09375f, 0f, -0.0390625f);
            ModelVerts[6] = new Vector3(0.1289063f, 0.1171875f, 0.046875f);
            ModelVerts[7] = new Vector3(0.1015625f, 0f, 0.03515625f);
            ModelVerts[8] = new Vector3(-0.09765625f, 0f, -0.03515625f);
            ModelVerts[9] = new Vector3(-0.1289063f, 0.1171875f, -0.046875f);
            ModelVerts[10] = new Vector3(-0.05859375f, 0.234375f, -0.1210938f);
            ModelVerts[11] = new Vector3(0.046875f, 0.234375f, -0.1289063f);
            ModelVerts[12] = new Vector3(0.125f, 0.234375f, -0.05859375f);
            ModelVerts[13] = new Vector3(0.1289063f, 0.234375f, 0.046875f);
            ModelVerts[14] = new Vector3(-0.1289063f, 0.234375f, -0.046875f);
            ModelVerts[15] = new Vector3(-0.0390625f, 0.3515625f, -0.09375f);
            ModelVerts[16] = new Vector3(0.03515625f, 0.3515625f, -0.09765625f);
            ModelVerts[17] = new Vector3(0.09375f, 0.3515625f, -0.0390625f);
            ModelVerts[18] = new Vector3(0.1015625f, 0.3515625f, 0.03515625f);
            ModelVerts[19] = new Vector3(-0.09765625f, 0.3515625f, -0.03515625f);
            ModelVerts[20] = new Vector3(-0.1210938f, 0.234375f, 0.05859375f);
            ModelVerts[21] = new Vector3(-0.09375f, 0.3515625f, 0.04296875f);
            ModelVerts[22] = new Vector3(-0.03515625f, 0.3515625f, 0.1015625f);
            ModelVerts[23] = new Vector3(-0.046875f, 0.234375f, 0.1289063f);
            ModelVerts[24] = new Vector3(-0.1210938f, 0.1171875f, 0.05859375f);
            ModelVerts[25] = new Vector3(-0.046875f, 0.1171875f, 0.1289063f);
            ModelVerts[26] = new Vector3(-0.09375f, 0f, 0.04296875f);
            ModelVerts[27] = new Vector3(-0.03515625f, 0f, 0.1015625f);
            ModelVerts[28] = new Vector3(0.05859375f, 0.234375f, 0.125f);
            ModelVerts[29] = new Vector3(0.04296875f, 0.3515625f, 0.09375f);
            ModelVerts[30] = new Vector3(0.05859375f, 0.1171875f, 0.125f);
            ModelVerts[31] = new Vector3(0.04296875f, 0f, 0.09375f);
            return ModelVerts;
        }

        public override int ModelColour(int meshNo)
        {
            return 1;
        }

        public override ShaderMaterial GetMaterial(int textureno, int surface)
        {//Get the material texture from tmobj    
            switch (surface)
            {
                case 0: //lid and base
                case 9:
                    return base.GetMaterial(0, 6);
                default: //ribs
                    return GetTmObj.GetMaterial((byte)ModelColour(surface));
            }
        }
    } // end class
}//end namespace