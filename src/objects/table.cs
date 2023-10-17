using System.ComponentModel.DataAnnotations;
using Godot;
using Microsoft.VisualBasic;

namespace Underworld
{
    public class table : model3D
    {

        public table(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public static table CreateInstance(Node3D parent, uwObject obj)
        {
            var n = new table(obj);
            var modelNode = n.Generate3DModel(parent);
            //modelNode.Rotate(Vector3.Up, (float)Math.PI); 
            SetModelRotation(parent, n);

            //DisplayModelPoints(n, parent, 88);
            return n;
        }

        public override int NoOfMeshes()
        {
            return 6;
        }
        public override Vector3[] ModelVertices()
        {
            Vector3[] ModelVerts = new Vector3[88];
            ModelVerts[0] = new Vector3(-0.2109375f, 0.328125f, -0.2109375f);
            ModelVerts[1] = new Vector3(-0.203125f, 0.3398438f, -0.1953125f);
            ModelVerts[2] = new Vector3(0.15625f, 0f, -0.15625f);
            ModelVerts[3] = new Vector3(0.203125f, 0.3398438f, -0.1953125f);
            ModelVerts[4] = new Vector3(0.2109375f, 0.328125f, -0.2109375f);
            ModelVerts[5] = new Vector3(0.2109375f, 0.2851563f, -0.2109375f);
            ModelVerts[6] = new Vector3(-0.2109375f, 0.2851563f, -0.2109375f);
            ModelVerts[7] = new Vector3(0.265625f, 0.3398438f, -0.1328125f);
            ModelVerts[8] = new Vector3(0.28125f, 0.328125f, -0.140625f);
            ModelVerts[9] = new Vector3(0.28125f, 0.2851563f, -0.140625f);
            ModelVerts[10] = new Vector3(0.265625f, 0.3398438f, 0.1328125f);
            ModelVerts[11] = new Vector3(0.28125f, 0.328125f, 0.140625f);
            ModelVerts[12] = new Vector3(0.28125f, 0.2851563f, 0.140625f);
            ModelVerts[13] = new Vector3(0.203125f, 0.3398438f, 0.1953125f);
            ModelVerts[14] = new Vector3(0.2109375f, 0.328125f, 0.2109375f);
            ModelVerts[15] = new Vector3(0.2109375f, 0.2851563f, 0.2109375f);
            ModelVerts[16] = new Vector3(-0.2109375f, 0.328125f, 0.2109375f);
            ModelVerts[17] = new Vector3(-0.2109375f, 0.2851563f, 0.2109375f);
            ModelVerts[18] = new Vector3(-0.203125f, 0.3398438f, 0.1953125f);
            ModelVerts[19] = new Vector3(0.1601563f, 0.2851563f, -0.1601563f);
            ModelVerts[20] = new Vector3(0.1484375f, 0f, -0.15625f);
            ModelVerts[21] = new Vector3(0.140625f, 0.2851563f, -0.1601563f);
            ModelVerts[22] = new Vector3(0.1757813f, 0.2851563f, -0.15625f);
            ModelVerts[23] = new Vector3(0.1679688f, 0f, -0.1484375f);
            ModelVerts[24] = new Vector3(0.1757813f, 0.2851563f, -0.125f);
            ModelVerts[25] = new Vector3(0.1679688f, 0f, -0.1328125f);
            ModelVerts[26] = new Vector3(0.1601563f, 0.2851563f, -0.1210938f);
            ModelVerts[27] = new Vector3(0.15625f, 0f, -0.125f);
            ModelVerts[28] = new Vector3(0.140625f, 0.2851563f, -0.1210938f);
            ModelVerts[29] = new Vector3(0.1484375f, 0f, -0.125f);
            ModelVerts[30] = new Vector3(0.125f, 0.2851563f, -0.125f);
            ModelVerts[31] = new Vector3(0.1328125f, 0f, -0.1328125f);
            ModelVerts[32] = new Vector3(0.125f, 0.2851563f, -0.15625f);
            ModelVerts[33] = new Vector3(0.1328125f, 0f, -0.1484375f);
            ModelVerts[34] = new Vector3(0.140625f, 0.2851563f, 0.1210938f);
            ModelVerts[35] = new Vector3(0.1484375f, 0f, 0.125f);
            ModelVerts[36] = new Vector3(0.1328125f, 0f, 0.1328125f);
            ModelVerts[37] = new Vector3(0.125f, 0.2851563f, 0.125f);
            ModelVerts[38] = new Vector3(0.1328125f, 0f, 0.1484375f);
            ModelVerts[39] = new Vector3(0.125f, 0.2851563f, 0.15625f);
            ModelVerts[40] = new Vector3(0.1484375f, 0f, 0.15625f);
            ModelVerts[41] = new Vector3(0.140625f, 0.2851563f, 0.1601563f);
            ModelVerts[42] = new Vector3(0.15625f, 0f, 0.15625f);
            ModelVerts[43] = new Vector3(0.1601563f, 0.2851563f, 0.1601563f);
            ModelVerts[44] = new Vector3(0.1679688f, 0f, 0.1484375f);
            ModelVerts[45] = new Vector3(0.1757813f, 0.2851563f, 0.15625f);
            ModelVerts[46] = new Vector3(0.1679688f, 0f, 0.1328125f);
            ModelVerts[47] = new Vector3(0.1757813f, 0.2851563f, 0.125f);
            ModelVerts[48] = new Vector3(0.15625f, 0f, 0.125f);
            ModelVerts[49] = new Vector3(0.1601563f, 0.2851563f, 0.1210938f);
            ModelVerts[50] = new Vector3(-0.28125f, 0.2851563f, 0.140625f);
            ModelVerts[51] = new Vector3(-0.28125f, 0.328125f, 0.140625f);
            ModelVerts[52] = new Vector3(-0.265625f, 0.3398438f, 0.1328125f);
            ModelVerts[53] = new Vector3(-0.28125f, 0.2851563f, -0.140625f);
            ModelVerts[54] = new Vector3(-0.28125f, 0.328125f, -0.140625f);
            ModelVerts[55] = new Vector3(-0.265625f, 0.3398438f, -0.1328125f);
            ModelVerts[56] = new Vector3(-0.1601563f, 0.2851563f, -0.1601563f);
            ModelVerts[57] = new Vector3(-0.15625f, 0f, -0.15625f);
            ModelVerts[58] = new Vector3(-0.1679688f, 0f, -0.1484375f);
            ModelVerts[59] = new Vector3(-0.1757813f, 0.2851563f, -0.15625f);
            ModelVerts[60] = new Vector3(-0.1679688f, 0f, -0.1328125f);
            ModelVerts[61] = new Vector3(-0.1757813f, 0.2851563f, -0.125f);
            ModelVerts[62] = new Vector3(-0.15625f, 0f, -0.125f);
            ModelVerts[63] = new Vector3(-0.1601563f, 0.2851563f, -0.1210938f);
            ModelVerts[64] = new Vector3(-0.1484375f, 0f, -0.125f);
            ModelVerts[65] = new Vector3(-0.140625f, 0.2851563f, -0.1210938f);
            ModelVerts[66] = new Vector3(-0.1328125f, 0f, -0.1328125f);
            ModelVerts[67] = new Vector3(-0.125f, 0.2851563f, -0.125f);
            ModelVerts[68] = new Vector3(-0.1328125f, 0f, -0.1484375f);
            ModelVerts[69] = new Vector3(-0.125f, 0.2851563f, -0.15625f);
            ModelVerts[70] = new Vector3(-0.1484375f, 0f, -0.15625f);
            ModelVerts[71] = new Vector3(-0.140625f, 0.2851563f, -0.1601563f);
            ModelVerts[72] = new Vector3(-0.1601563f, 0.2851563f, 0.1210938f);
            ModelVerts[73] = new Vector3(-0.15625f, 0f, 0.125f);
            ModelVerts[74] = new Vector3(-0.1679688f, 0f, 0.1328125f);
            ModelVerts[75] = new Vector3(-0.1757813f, 0.2851563f, 0.125f);
            ModelVerts[76] = new Vector3(-0.1679688f, 0f, 0.1484375f);
            ModelVerts[77] = new Vector3(-0.1757813f, 0.2851563f, 0.15625f);
            ModelVerts[78] = new Vector3(-0.15625f, 0f, 0.15625f);
            ModelVerts[79] = new Vector3(-0.1601563f, 0.2851563f, 0.1601563f);
            ModelVerts[80] = new Vector3(-0.1484375f, 0f, 0.15625f);
            ModelVerts[81] = new Vector3(-0.140625f, 0.2851563f, 0.1601563f);
            ModelVerts[82] = new Vector3(-0.1328125f, 0f, 0.1484375f);
            ModelVerts[83] = new Vector3(-0.125f, 0.2851563f, 0.15625f);
            ModelVerts[84] = new Vector3(-0.1328125f, 0f, 0.1328125f);
            ModelVerts[85] = new Vector3(-0.125f, 0.2851563f, 0.125f);
            ModelVerts[86] = new Vector3(-0.1484375f, 0f, 0.125f);
            ModelVerts[87] = new Vector3(-0.140625f, 0.2851563f, 0.1210938f);
            // for (int i = 0; i < 88; i++)
            // {
            //     ModelVerts[i] = ModelVerts[i] * new Vector3(8, 8, 8);
            // }
            return ModelVerts;
        }

        public override int[] ModelTriangles(int meshNo)
        {
            switch (meshNo)
            {
                case 0://table top
                    {
                        int[] tris = new int[6];
                        tris[0] = 0;
                        tris[1] = 4;
                        tris[2] = 14;
                        tris[3] = 14;
                        tris[4] = 16;
                        tris[5] = 0;
                        return tris;
                    }
                case 1: // table trim and underneath
                    {
                        int[] tris = new int[30];
                        tris[0] = 4;
                        tris[1] = 0;
                        tris[2] = 6;
                        tris[3] = 6;
                        tris[4] = 5;
                        tris[5] = 4;

                        tris[6] = 14;
                        tris[7] = 4;
                        tris[8] = 5;
                        tris[9] = 5;
                        tris[10] = 15;
                        tris[11] = 14;

                        tris[12] = 16;
                        tris[13] = 14;
                        tris[14] = 15;
                        tris[15] = 15;
                        tris[16] = 17;
                        tris[17] = 16;

                        tris[18] = 0;
                        tris[19] = 16;
                        tris[20] = 17;
                        tris[21] = 17;
                        tris[22] = 6;
                        tris[23] = 0;

                        tris[24] = 17;//underneat
                        tris[25] = 15;
                        tris[26] = 5;
                        tris[27] = 5;
                        tris[28] = 6;
                        tris[29] = 17;

                        return tris;
                    }
                case 2: // Leg #1
                    {
                        int[] tris = new int[48];

                        tris[0] = 70;
                        tris[1] = 68;
                        tris[2] = 69;
                        tris[3] = 69;
                        tris[4] = 71;
                        tris[5] = 70;

                        tris[6] = 57;
                        tris[7] = 70;
                        tris[8] = 71;
                        tris[9] = 71;
                        tris[10] = 56;
                        tris[11] = 57;

                        tris[12] = 60;
                        tris[13] = 58;
                        tris[14] = 59;
                        tris[15] = 59;
                        tris[16] = 61;
                        tris[17] = 60;


                        tris[18] = 58;
                        tris[19] = 57;
                        tris[20] = 56;
                        tris[21] = 56;
                        tris[22] = 59;
                        tris[23] = 58;

                        tris[24] = 62;
                        tris[25] = 60;
                        tris[26] = 61;
                        tris[27] = 61;
                        tris[28] = 63;
                        tris[29] = 62;


                        tris[30] = 64;
                        tris[31] = 62;
                        tris[32] = 63;
                        tris[33] = 63;
                        tris[34] = 65;
                        tris[35] = 64;

                        tris[36] = 66;
                        tris[37] = 64;
                        tris[38] = 65;
                        tris[39] = 65;
                        tris[40] = 67;
                        tris[41] = 66;

                        tris[42] = 68;
                        tris[43] = 66;
                        tris[44] = 67;
                        tris[45] = 67;
                        tris[46] = 69;
                        tris[47] = 68;

                        return tris;
                    } //end leg 1
                case 3: // Leg #2
                    {
                        int[] tris = new int[48];

                        tris[0] = 31;
                        tris[1] = 33;
                        tris[2] = 32;
                        tris[3] = 32;
                        tris[4] = 30;
                        tris[5] = 31;

                        tris[6] = 29;
                        tris[7] = 31;
                        tris[8] = 30;
                        tris[9] = 30;
                        tris[10] = 28;
                        tris[11] = 29;

                        tris[12] = 27;
                        tris[13] = 29;
                        tris[14] = 28;
                        tris[15] = 28;
                        tris[16] = 26;
                        tris[17] = 27;


                        tris[18] = 25;
                        tris[19] = 27;
                        tris[20] = 26;
                        tris[21] = 26;
                        tris[22] = 24;
                        tris[23] = 25;

                        tris[24] = 23;
                        tris[25] = 25;
                        tris[26] = 24;
                        tris[27] = 24;
                        tris[28] = 22;
                        tris[29] = 23;


                        tris[30] = 2;
                        tris[31] = 23;
                        tris[32] = 22;
                        tris[33] = 22;
                        tris[34] = 19;
                        tris[35] = 2;

                        tris[36] = 20;
                        tris[37] = 2;
                        tris[38] = 19;
                        tris[39] = 19;
                        tris[40] = 21;
                        tris[41] = 20;

                        tris[42] = 33;
                        tris[43] = 20;
                        tris[44] = 21;
                        tris[45] = 21;
                        tris[46] = 32;
                        tris[47] = 33;

                        return tris;
                    } //end leg 2
                case 4: // leg #3
                    {
                        int[] tris = new int[48];

                        tris[0] = 42;
                        tris[1] = 40;
                        tris[2] = 41;
                        tris[3] = 41;
                        tris[4] = 43;
                        tris[5] = 42;

                        tris[6] = 44;
                        tris[7] = 42;
                        tris[8] = 43;
                        tris[9] = 43;
                        tris[10] = 45;
                        tris[11] = 44;

                        tris[12] = 46;
                        tris[13] = 44;
                        tris[14] = 45;
                        tris[15] = 45;
                        tris[16] = 47;
                        tris[17] = 46;


                        tris[18] = 48;
                        tris[19] = 46;
                        tris[20] = 47;
                        tris[21] = 47;
                        tris[22] = 49;
                        tris[23] = 48;

                        tris[24] = 35;
                        tris[25] = 48;
                        tris[26] = 49;
                        tris[27] = 49;
                        tris[28] = 34;
                        tris[29] = 35;


                        tris[30] = 36;
                        tris[31] = 35;
                        tris[32] = 34;
                        tris[33] = 34;
                        tris[34] = 37;
                        tris[35] = 36;

                        tris[36] = 38;
                        tris[37] = 36;
                        tris[38] = 37;
                        tris[39] = 37;
                        tris[40] = 39;
                        tris[41] = 38;

                        tris[42] = 40;
                        tris[43] = 38;
                        tris[44] = 39;
                        tris[45] = 39;
                        tris[46] = 41;
                        tris[47] = 40;

                        return tris;
                    } //end leg 3
                case 5:// leg #4
                    {
                        int[] tris = new int[48];

                        tris[0] = 76;
                        tris[1] = 74;
                        tris[2] = 75;
                        tris[3] = 75;
                        tris[4] = 77;
                        tris[5] = 76;

                        tris[6] = 78;
                        tris[7] = 76;
                        tris[8] = 77;
                        tris[9] = 77;
                        tris[10] = 79;
                        tris[11] = 78;

                        tris[12] = 80;
                        tris[13] = 78;
                        tris[14] = 79;
                        tris[15] = 79;
                        tris[16] = 81;
                        tris[17] = 80;

                        tris[18] = 82;
                        tris[19] = 80;
                        tris[20] = 81;
                        tris[21] = 81;
                        tris[22] = 83;
                        tris[23] = 82;

                        tris[24] = 84;
                        tris[25] = 82;
                        tris[26] = 83;
                        tris[27] = 83;
                        tris[28] = 85;
                        tris[29] = 84;


                        tris[30] = 86;
                        tris[31] = 84;
                        tris[32] = 85;
                        tris[33] = 85;
                        tris[34] = 87;
                        tris[35] = 86;

                        tris[36] = 73;
                        tris[37] = 86;
                        tris[38] = 87;
                        tris[39] = 87;
                        tris[40] = 72;
                        tris[41] = 73;

                        tris[42] = 74;
                        tris[43] = 73;
                        tris[44] = 72;
                        tris[45] = 72;
                        tris[46] = 75;
                        tris[47] = 74;

                        return tris;
                    } //end leg 4
            }
            return base.ModelTriangles(meshNo);
        }


        public override Vector2[] ModelUVs(Vector3[] verts)
        {
            var uvs = base.ModelUVs(verts);
            //top of table
            uvs[0] = new Vector2(0f, 0f);
            uvs[4] = new Vector2(0f, 1f);
            uvs[16] = new Vector2(1f, 0f);
            uvs[14] = new Vector2(1f, 1f);

            uvs[5] = new Vector2(0f, 0.9f);
            uvs[15] = new Vector2(1f, 0.9f);

            uvs[6] = new Vector2(0f, 0.1f);
            uvs[17] = new Vector2(1f, 0.1f);

            return uvs;
        }



        public override int ModelColour(int meshNo)
        {
            if (_RES == GAME_UW2)
            {
                return 34;
            }
            else
            {
                return 30;
            }
        }

        public override ShaderMaterial GetMaterial(int textureno, int surface)
        {//Get the material texture from tmobj   
            LoadTmObj();
            return tmObj.GetMaterial((byte)ModelColour(surface));
        }
    } //end class

} //end namespace