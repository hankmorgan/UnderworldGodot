using System;
using Godot;
namespace Underworld
{
    public class lotus : model3D
    {
        public static lotus CreateInstance(Node3D parent, uwObject obj, string name)
        {
            var l = new lotus(obj);
            var modelNode = l.Generate3DModel(parent, name);
           // modelNode.Rotate(Vector3.Up, (float)Math.PI);
            SetModelRotation(parent, l);
            DisplayModelPoints(l, parent,200);           
            return l;
        }

        public lotus(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }


        public override Vector3[] ModelVertices()
        {
            var v = new Vector3[194];
            v[0] = new Vector3(-0.34375f, 0.1640625f, 0.3671875f);
            v[1] = new Vector3(-0.24609375f, 0.37109375f, -0.00390625f);
            v[2] = new Vector3(0.3359375f, 0.26953125f, 0.1796875f);
            v[3] = new Vector3(0.26953125f, 0.34375f, -0.2734375f);
            v[4] = new Vector3(0.31640625f, 0.2890625f, -0.30078125f);
            v[5] = new Vector3(-0.34375f, 0.16015625f, 0.33984375f);
            v[6] = new Vector3(-0.34375f, 0.14453125f, 0.3203125f);
            v[7] = new Vector3(-0.34375f, 0.12109375f, 0.3046875f);
            v[8] = new Vector3(-0.34375f, 0.09765625f, 0.296875f);
            v[9] = new Vector3(-0.34375f, 0.0703125f, 0.3046875f);
            v[10] = new Vector3(-0.34375f, 0.046875f, 0.3203125f);
            v[11] = new Vector3(-0.34375f, 0.03515625f, 0.33984375f);
            v[12] = new Vector3(-0.34375f, 0.02734375f, 0.3671875f);
            v[13] = new Vector3(-0.34375f, 0.03515625f, 0.390625f);
            v[14] = new Vector3(-0.34375f, 0.046875f, 0.4140625f);
            v[15] = new Vector3(-0.34375f, 0.0703125f, 0.4296875f);
            v[16] = new Vector3(-0.34375f, 0.09765625f, 0.4375f);
            v[17] = new Vector3(-0.34375f, 0.12109375f, 0.4296875f);
            v[18] = new Vector3(-0.34375f, 0.14453125f, 0.4140625f);
            v[19] = new Vector3(-0.34375f, 0.16015625f, 0.390625f);
            v[20] = new Vector3(-0.34375f, 0.1875f, 0.3671875f);
            v[21] = new Vector3(-0.34375f, 0.1796875f, 0.33203125f);
            v[22] = new Vector3(-0.34375f, 0.16015625f, 0.30078125f);
            v[23] = new Vector3(-0.34375f, 0.12890625f, 0.28125f);
            v[24] = new Vector3(-0.34375f, 0.09375f, 0.2734375f);
            v[25] = new Vector3(-0.34375f, 0.05859375f, 0.28125f);
            v[26] = new Vector3(-0.34375f, 0.02734375f, 0.30078125f);
            v[27] = new Vector3(-0.34375f, 0.0078125f, 0.33203125f);
            v[28] = new Vector3(-0.34375f, 0f, 0.3671875f);
            v[29] = new Vector3(-0.34375f, 0.0078125f, 0.40234375f);
            v[30] = new Vector3(-0.34375f, 0.02734375f, 0.43359375f);
            v[31] = new Vector3(-0.34375f, 0.05859375f, 0.453125f);
            v[32] = new Vector3(-0.34375f, 0.09375f, 0.4609375f);
            v[33] = new Vector3(-0.34375f, 0.12890625f, 0.453125f);
            v[34] = new Vector3(-0.34375f, 0.16015625f, 0.43359375f);
            v[35] = new Vector3(-0.34375f, 0.1796875f, 0.40234375f);
            v[36] = new Vector3(-0.34375f, 0.19140625f, -0.40234375f);
            v[37] = new Vector3(-0.34375f, 0.18359375f, -0.4375f);
            v[38] = new Vector3(-0.34375f, 0.1640625f, -0.46875f);
            v[39] = new Vector3(-0.34375f, 0.1328125f, -0.48828125f);
            v[40] = new Vector3(-0.34375f, 0.09765625f, -0.49609375f);
            v[41] = new Vector3(-0.34375f, 0.0625f, -0.48828125f);
            v[42] = new Vector3(-0.34375f, 0.03125f, -0.46875f);
            v[43] = new Vector3(-0.34375f, 0.01171875f, -0.4375f);
            v[44] = new Vector3(-0.34375f, 0.00390625f, -0.40234375f);
            v[45] = new Vector3(-0.34375f, 0.01171875f, -0.3671875f);
            v[46] = new Vector3(-0.34375f, 0.03125f, -0.3359375f);
            v[47] = new Vector3(-0.34375f, 0.0625f, -0.31640625f);
            v[48] = new Vector3(-0.34375f, 0.09765625f, -0.30859375f);
            v[49] = new Vector3(-0.34375f, 0.1328125f, -0.31640625f);
            v[50] = new Vector3(-0.34375f, 0.1640625f, -0.3359375f);
            v[51] = new Vector3(-0.34375f, 0.18359375f, -0.3671875f);
            v[52] = new Vector3(-0.34375f, 0.16796875f, -0.40234375f);
            v[53] = new Vector3(-0.34375f, 0.1640625f, -0.4296875f);
            v[54] = new Vector3(-0.34375f, 0.1484375f, -0.44921875f);
            v[55] = new Vector3(-0.34375f, 0.125f, -0.46484375f);
            v[56] = new Vector3(-0.34375f, 0.1015625f, -0.47265625f);
            v[57] = new Vector3(-0.34375f, 0.07421875f, -0.46484375f);
            v[58] = new Vector3(-0.34375f, 0.05078125f, -0.44921875f);
            v[59] = new Vector3(-0.34375f, 0.0390625f, -0.4296875f);
            v[60] = new Vector3(-0.34375f, 0.03125f, -0.40234375f);
            v[61] = new Vector3(-0.34375f, 0.0390625f, -0.37890625f);
            v[62] = new Vector3(-0.34375f, 0.05078125f, -0.35546875f);
            v[63] = new Vector3(-0.34375f, 0.07421875f, -0.33984375f);
            v[64] = new Vector3(-0.34375f, 0.1015625f, -0.33203125f);
            v[65] = new Vector3(-0.34375f, 0.125f, -0.33984375f);
            v[66] = new Vector3(-0.34375f, 0.1484375f, -0.35546875f);
            v[67] = new Vector3(-0.34375f, 0.1640625f, -0.37890625f);
            v[68] = new Vector3(0.34375f, 0.16796875f, -0.40234375f);
            v[69] = new Vector3(0.34375f, 0.1640625f, -0.4296875f);
            v[70] = new Vector3(0.34375f, 0.1484375f, -0.44921875f);
            v[71] = new Vector3(0.34375f, 0.125f, -0.46484375f);
            v[72] = new Vector3(0.34375f, 0.1015625f, -0.47265625f);
            v[73] = new Vector3(0.34375f, 0.07421875f, -0.46484375f);
            v[74] = new Vector3(0.34375f, 0.05078125f, -0.44921875f);
            v[75] = new Vector3(0.34375f, 0.0390625f, -0.4296875f);
            v[76] = new Vector3(0.34375f, 0.03125f, -0.40234375f);
            v[77] = new Vector3(0.34375f, 0.0390625f, -0.37890625f);
            v[78] = new Vector3(0.34375f, 0.05078125f, -0.35546875f);
            v[79] = new Vector3(0.34375f, 0.07421875f, -0.33984375f);
            v[80] = new Vector3(0.34375f, 0.1015625f, -0.33203125f);
            v[81] = new Vector3(0.34375f, 0.125f, -0.33984375f);
            v[82] = new Vector3(0.34375f, 0.1484375f, -0.35546875f);
            v[83] = new Vector3(0.34375f, 0.1640625f, -0.37890625f);
            v[84] = new Vector3(0.34375f, 0.1875f, 0.3671875f);
            v[85] = new Vector3(0.34375f, 0.1796875f, 0.33203125f);
            v[86] = new Vector3(0.34375f, 0.16015625f, 0.30078125f);
            v[87] = new Vector3(0.34375f, 0.12890625f, 0.28125f);
            v[88] = new Vector3(0.34375f, 0.09375f, 0.2734375f);
            v[89] = new Vector3(0.34375f, 0.05859375f, 0.28125f);
            v[90] = new Vector3(0.34375f, 0.02734375f, 0.30078125f);
            v[91] = new Vector3(0.34375f, 0.0078125f, 0.33203125f);
            v[92] = new Vector3(0.34375f, 0f, 0.3671875f);
            v[93] = new Vector3(0.34375f, 0.0078125f, 0.40234375f);
            v[94] = new Vector3(0.34375f, 0.02734375f, 0.43359375f);
            v[95] = new Vector3(0.34375f, 0.05859375f, 0.453125f);
            v[96] = new Vector3(0.34375f, 0.09375f, 0.4609375f);
            v[97] = new Vector3(0.34375f, 0.12890625f, 0.453125f);
            v[98] = new Vector3(0.34375f, 0.16015625f, 0.43359375f);
            v[99] = new Vector3(0.34375f, 0.1796875f, 0.40234375f);
            v[100] = new Vector3(0.34375f, 0.1640625f, 0.3671875f);
            v[101] = new Vector3(0.34375f, 0.16015625f, 0.33984375f);
            v[102] = new Vector3(0.34375f, 0.14453125f, 0.3203125f);
            v[103] = new Vector3(0.34375f, 0.12109375f, 0.3046875f);
            v[104] = new Vector3(0.34375f, 0.09765625f, 0.296875f);
            v[105] = new Vector3(0.34375f, 0.0703125f, 0.3046875f);
            v[106] = new Vector3(0.34375f, 0.046875f, 0.3203125f);
            v[107] = new Vector3(0.34375f, 0.03515625f, 0.33984375f);
            v[108] = new Vector3(0.34375f, 0.02734375f, 0.3671875f);
            v[109] = new Vector3(0.34375f, 0.03515625f, 0.390625f);
            v[110] = new Vector3(0.34375f, 0.046875f, 0.4140625f);
            v[111] = new Vector3(0.34375f, 0.0703125f, 0.4296875f);
            v[112] = new Vector3(0.34375f, 0.09765625f, 0.4375f);
            v[113] = new Vector3(0.34375f, 0.12109375f, 0.4296875f);
            v[114] = new Vector3(0.34375f, 0.14453125f, 0.4140625f);
            v[115] = new Vector3(0.34375f, 0.16015625f, 0.390625f);
            v[116] = new Vector3(0.34375f, 0.19140625f, -0.40234375f);
            v[117] = new Vector3(0.34375f, 0.18359375f, -0.4375f);
            v[118] = new Vector3(0.34375f, 0.1640625f, -0.46875f);
            v[119] = new Vector3(0.34375f, 0.1328125f, -0.48828125f);
            v[120] = new Vector3(0.34375f, 0.09765625f, -0.49609375f);
            v[121] = new Vector3(0.34375f, 0.0625f, -0.48828125f);
            v[122] = new Vector3(0.34375f, 0.03125f, -0.46875f);
            v[123] = new Vector3(0.34375f, 0.01171875f, -0.4375f);
            v[124] = new Vector3(0.34375f, 0.00390625f, -0.40234375f);
            v[125] = new Vector3(0.34375f, 0.01171875f, -0.3671875f);
            v[126] = new Vector3(0.34375f, 0.03125f, -0.3359375f);
            v[127] = new Vector3(0.34375f, 0.0625f, -0.31640625f);
            v[128] = new Vector3(0.34375f, 0.09765625f, -0.30859375f);
            v[129] = new Vector3(0.34375f, 0.1328125f, -0.31640625f);
            v[130] = new Vector3(0.34375f, 0.1640625f, -0.3359375f);
            v[131] = new Vector3(0.34375f, 0.18359375f, -0.3671875f);
            v[132] = new Vector3(-0.34375f, 0.046875f, 0.48828125f);
            v[133] = new Vector3(-0.34375f, 0.234375f, 0.484375f);
            v[134] = new Vector3(-0.34375f, 0.26171875f, 0.3125f);
            v[135] = new Vector3(-0.34375f, 0.26171875f, -0.65625f);
            v[136] = new Vector3(-0.34375f, 0.17578125f, -0.66015625f);
            v[137] = new Vector3(-0.34375f, 0.0859375f, -0.65625f);
            v[138] = new Vector3(-0.34375f, 0.046875f, -0.6484375f);
            v[139] = new Vector3(-0.34375f, 0.15625f, -0.70703125f);
            v[140] = new Vector3(-0.34375f, 0.09765625f, -0.703125f);
            v[141] = new Vector3(-0.34375f, 0.1796875f, 0.44921875f);
            v[142] = new Vector3(-0.34375f, 0.21484375f, 0.3671875f);
            v[143] = new Vector3(-0.34375f, 0.17578125f, 0.28125f);
            v[144] = new Vector3(-0.34375f, 0.046875f, 0.25f);
            v[145] = new Vector3(-0.34375f, 0.046875f, 0.64453125f);
            v[146] = new Vector3(-0.34375f, 0.1484375f, 0.70703125f);
            v[147] = new Vector3(-0.34375f, 0.046875f, -0.28125f);
            v[148] = new Vector3(-0.34375f, 0.1796875f, -0.3203125f);
            v[149] = new Vector3(-0.34375f, 0.21484375f, -0.40234375f);
            v[150] = new Vector3(-0.34375f, 0.17578125f, -0.48828125f);
            v[151] = new Vector3(-0.34375f, 0.046875f, -0.51953125f);
            v[152] = new Vector3(0.34375f, 0.17578125f, -0.66015625f);
            v[153] = new Vector3(0.34375f, 0.15625f, -0.70703125f);
            v[154] = new Vector3(0.34375f, 0.09765625f, -0.703125f);
            v[155] = new Vector3(0.34375f, 0.0859375f, -0.65625f);
            v[156] = new Vector3(0.34375f, 0.046875f, 0.48828125f);
            v[157] = new Vector3(0.34375f, 0.046875f, 0.64453125f);
            v[158] = new Vector3(0.34375f, 0.1484375f, 0.70703125f);
            v[159] = new Vector3(0.34375f, 0.234375f, 0.484375f);
            v[160] = new Vector3(0.34375f, 0.26171875f, 0.3125f);
            v[161] = new Vector3(0.34375f, 0.26171875f, -0.65625f);
            v[162] = new Vector3(0.34375f, 0.046875f, -0.6484375f);
            v[163] = new Vector3(-0.24609375f, 0.37109375f, -0.25f);
            v[164] = new Vector3(0.24609375f, 0.37109375f, -0.00390625f);
            v[165] = new Vector3(0.24609375f, 0.37109375f, -0.25f);
            v[166] = new Vector3(0.30859375f, 0.296875f, 0.1796875f);
            v[167] = new Vector3(0.25390625f, 0.359375f, -0.00390625f);
            v[168] = new Vector3(0.25390625f, 0.359375f, -0.23828125f);
            v[169] = new Vector3(0.3359375f, 0.26953125f, -0.28125f);
            v[170] = new Vector3(0.24609375f, 0.27734375f, -0.5859375f);
            v[171] = new Vector3(-0.24609375f, 0.27734375f, -0.5859375f);
            v[172] = new Vector3(0.34375f, 0.046875f, -0.28125f);
            v[173] = new Vector3(0.34375f, 0.1796875f, -0.3203125f);
            v[174] = new Vector3(0.34375f, 0.21484375f, -0.40234375f);
            v[175] = new Vector3(0.34375f, 0.17578125f, -0.48828125f);
            v[176] = new Vector3(0.34375f, 0.046875f, -0.51953125f);
            v[177] = new Vector3(0.34375f, 0.1796875f, 0.44921875f);
            v[178] = new Vector3(0.34375f, 0.21484375f, 0.3671875f);
            v[179] = new Vector3(0.34375f, 0.17578125f, 0.28125f);
            v[180] = new Vector3(0.34375f, 0.046875f, 0.25f);
            v[181] = new Vector3(-0.3359375f, 0.26953125f, -0.28125f);
            v[182] = new Vector3(-0.31640625f, 0.2890625f, -0.30078125f);
            v[183] = new Vector3(-0.26953125f, 0.34375f, -0.2734375f);
            v[184] = new Vector3(-0.25390625f, 0.359375f, -0.23828125f);
            v[185] = new Vector3(-0.25390625f, 0.359375f, -0.00390625f);
            v[186] = new Vector3(-0.30859375f, 0.296875f, 0.1796875f);
            v[187] = new Vector3(-0.3359375f, 0.26953125f, 0.1796875f);
            v[188] = new Vector3(0.16015625f, 0.26171875f, -0.65625f);
            v[189] = new Vector3(0.16015625f, 0.20703125f, -0.65625f);
            v[190] = new Vector3(0.34375f, 0.20703125f, -0.65625f);
            v[191] = new Vector3(-0.34375f, 0.20703125f, -0.65625f);
            v[192] = new Vector3(-0.16015625f, 0.20703125f, -0.65625f);
            v[193] = new Vector3(-0.16015625f, 0.26171875f, -0.65625f);

            for (int i=0; i<= v.GetUpperBound(0); i++)
            {
                v[i] = v[i]  * new Vector3(8f,8f,8f);
            }

            return v;
        }

        public override int ModelColour(int meshNo)
        {
            switch (meshNo)
            {
                case 0://wheel hubs
                case 1:
                    return 81;//blue
            }
            return base.ModelColour(meshNo);
        }

        public override int NoOfMeshes()
        {
            return 2;
        }
        public override int[] ModelTriangles(int meshNo)
        {
            switch (meshNo)
            {
                case 0:
                    return Wheel1Hub();
                case 1:
                    return Wheel2Hub();
            }
            return base.ModelTriangles(meshNo);
        }

        public  int[] Wheel1Hub()
        {
            var tris = new int[42];
            tris[0] = 68;
            tris[1] = 82;
            tris[2] = 83;
            tris[3] = 68;
            tris[4] = 81;
            tris[5] = 82;
            tris[6] = 68;
            tris[7] = 80;
            tris[8] = 81;
            tris[9] = 68;
            tris[10] = 79;
            tris[11] = 80;
            tris[12] = 68;
            tris[13] = 78;
            tris[14] = 79;
            tris[15] = 68;
            tris[16] = 77;
            tris[17] = 78;
            tris[18] = 68;
            tris[19] = 76;
            tris[20] = 77;
            tris[21] = 68;
            tris[22] = 75;
            tris[23] = 76;
            tris[24] = 68;
            tris[25] = 74;
            tris[26] = 75;
            tris[27] = 68;
            tris[28] = 73;
            tris[29] = 74;
            tris[30] = 68;
            tris[31] = 72;
            tris[32] = 73;
            tris[33] = 68;
            tris[34] = 71;
            tris[35] = 72;
            tris[36] = 68;
            tris[37] = 70;
            tris[38] = 71;
            tris[39] = 68;
            tris[40] = 69;
            tris[41] = 70;


            return tris;

        }


        public  int[] Wheel2Hub()
        {
            var tris = new int[42];
            tris[0] = 100;
            tris[1] = 114;
            tris[2] = 115;
            tris[3] = 100;
            tris[4] = 113;
            tris[5] = 114;
            tris[6] = 100;
            tris[7] = 112;
            tris[8] = 114;
            tris[9] = 100;
            tris[10] = 111;
            tris[11] = 112;
            tris[12] = 100;
            tris[13] = 110;
            tris[14] = 111;
            tris[15] = 100;
            tris[16] = 109;
            tris[17] = 110;
            tris[18] = 100;
            tris[19] = 108;
            tris[20] = 109;
            tris[21] = 100;
            tris[22] = 107;
            tris[23] = 108;
            tris[24] = 100;
            tris[25] = 106;
            tris[26] = 107;
            tris[27] = 100;
            tris[28] = 105;
            tris[29] = 106;
            tris[30] = 100;
            tris[31] = 104;
            tris[32] = 105;
            tris[33] = 100;
            tris[34] = 103;
            tris[35] = 104;
            tris[36] = 100;
            tris[37] = 102;
            tris[38] = 103;
            tris[39] = 100;
            tris[40] = 102;
            tris[41] = 101;


            return tris;

        }


    }
}//end namespace