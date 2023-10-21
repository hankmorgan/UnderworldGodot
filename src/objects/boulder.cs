using Godot;

namespace Underworld
{
    public class boulder : model3D
    {
        const int LargeBoulder1 = 339;
        const int LargeBoulder2 = 340;
        const int MediumBoulder = 341;
        const int SmallBoulder = 342;

        public static boulder CreateInstance(Node3D parent, uwObject obj, string name)
        {
            var b = new boulder(obj);
            var modelNode = b.Generate3DModel(parent, name);            
            return b;
        }

        public boulder(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public override int[] ModelTriangles(int meshNo)
        {
            switch (uwobject.item_id)
            {
                case LargeBoulder1:
                case LargeBoulder2:
                    return new int[] { 24, 25, 2, 33, 5, 4, 26, 33, 4, 4, 5, 3, 2, 4, 3, 3, 18, 6, 7, 9, 19, 19, 11, 10, 21, 12, 23, 11, 19, 9, 27, 26, 14, 28, 27, 14, 29, 28, 14, 30, 29, 14, 31, 30, 14, 28, 7, 27, 9, 7, 28, 28, 29, 9, 30, 22, 21, 31, 22, 30, 25, 24, 22, 22, 31, 25, 26, 25, 14, 4, 25, 26, 5, 34, 27, 15, 5, 27, 7, 15, 27, 1, 2, 0, 34, 5, 33, 6, 8, 17, 0, 6, 17, 18, 5, 15, 2, 3, 0, 0, 3, 6, 3, 5, 18, 34, 5, 33, 24, 25, 2, 33, 5, 4, 26, 33, 4, 4, 5, 3, 2, 4, 3, 3, 18, 6, 7, 9, 19, 19, 11, 10, 21, 12, 23, 11, 19, 9, 27, 26, 14, 28, 27, 14, 29, 28, 14, 30, 29, 14, 31, 30, 14, 28, 7, 27, 9, 7, 28, 28, 29, 9, 30, 22, 21, 31, 22, 30, 25, 24, 22, 22, 31, 25, 26, 25, 14, 4, 25, 26, 5, 34, 27, 15, 5, 27, 7, 15, 27, 34, 33, 26, 34, 26, 27, 14, 25, 31, 25, 4, 2, 2, 1, 24, 1, 0, 17, 1, 17, 8, 15, 7, 18, 18, 7, 19, 18, 19, 10, 18, 10, 6, 8, 6, 10, 1, 8, 24, 8, 10, 24, 24, 10, 11, 12, 21, 22, 12, 22, 24, 24, 23, 12, 24, 11, 23, 23, 11, 21, 21, 11, 30, 11, 9, 30, 30, 9, 29 };

                case MediumBoulder://medium boulder model 6
                    return new int[] { 1, 22, 2, 13, 5, 4, 23, 13, 4, 4, 5, 3, 4, 3, 29, 2, 4, 29, 3, 5, 16, 6, 8, 15, 15, 10, 9, 18, 11, 20, 16, 6, 15, 7, 15, 9, 10, 15, 8, 24, 23, 12, 25, 24, 12, 26, 25, 12, 27, 26, 12, 28, 27, 12, 25, 6, 24, 8, 6, 25, 25, 26, 8, 27, 19, 18, 28, 19, 27, 22, 1, 19, 19, 28, 22, 23, 22, 12, 4, 22, 23, 5, 30, 24, 6, 5, 24, 15, 7, 14, 0, 15, 14, 30, 5, 13, 5, 6, 16, 29, 3, 0, 3, 16, 15, 0, 3, 15, 1, 22, 2, 13, 5, 4, 23, 13, 4, 4, 5, 3, 4, 3, 29, 2, 4, 29, 3, 5, 16, 6, 8, 15, 15, 10, 9, 18, 11, 20, 16, 6, 15, 7, 15, 9, 10, 15, 8, 24, 23, 12, 25, 24, 12, 26, 25, 12, 27, 26, 12, 28, 27, 12, 25, 6, 24, 8, 6, 25, 25, 26, 8, 27, 19, 18, 28, 19, 27, 22, 1, 19, 19, 28, 22, 23, 22, 12, 4, 22, 23, 5, 30, 24, 6, 5, 24, 13, 23, 24, 30, 13, 24, 8, 26, 27, 8, 27, 10, 12, 22, 28, 10, 27, 18, 4, 2, 22, 2, 29, 0, 18, 19, 11, 9, 10, 18, 9, 18, 20, 7, 9, 20, 0, 14, 2, 2, 14, 7, 2, 7, 20, 2, 20, 11, 2, 11, 1, 1, 11, 19 };

                case SmallBoulder://A small boulder model #5
                default:
                    return new int[] { 6, 9, 8, 11, 12, 8, 1, 2, 0, 2, 4, 0, 9, 12, 11, 0, 8, 1, 1, 3, 2, 15, 14, 7, 16, 15, 7, 17, 16, 7, 18, 17, 7, 19, 18, 7, 16, 4, 15, 5, 4, 16, 16, 17, 5, 18, 10, 9, 19, 10, 18, 13, 1, 10, 10, 19, 13, 7, 13, 19, 14, 13, 7, 3, 13, 14, 14, 2, 3, 4, 2, 15, 12, 1, 8, 12, 10, 1, 9, 10, 12, 8, 9, 11, 6, 9, 8, 11, 12, 8, 1, 2, 0, 2, 4, 0, 9, 12, 11, 0, 8, 1, 1, 13, 3, 15, 14, 7, 16, 15, 7, 17, 16, 7, 18, 17, 7, 19, 18, 7, 16, 4, 15, 5, 4, 16, 16, 17, 5, 18, 10, 9, 19, 10, 18, 13, 1, 10, 10, 19, 13, 7, 13, 19, 14, 13, 7, 3, 13, 14, 14, 2, 3, 4, 2, 15, 2, 14, 15, 9, 6, 18, 6, 5, 17, 6, 17, 18, 5, 0, 4, 8, 0, 5, 5, 6, 8 };
            }
        }

        public override Vector3[] ModelVertices()
        {
            switch (uwobject.item_id)
            {
                case LargeBoulder1:
                case LargeBoulder2:
                    {
                        Vector3[] ModelVerts = new Vector3[36];
                        ModelVerts[0] = new Vector3(-0.06640625f, 0.3046875f, -0.08203125f);
                        ModelVerts[1] = new Vector3(-0.140625f, 0.3203125f, 0f);
                        ModelVerts[2] = new Vector3(-0.2070313f, 0.2773438f, 0.015625f);
                        ModelVerts[3] = new Vector3(-0.1054688f, 0.328125f, -0.140625f);
                        ModelVerts[4] = new Vector3(-0.2617188f, 0.2070313f, -0.015625f);
                        ModelVerts[5] = new Vector3(-0.1484375f, 0.2539063f, -0.171875f);
                        ModelVerts[6] = new Vector3(0.015625f, 0.359375f, -0.1132813f);
                        ModelVerts[7] = new Vector3(0.07421875f, 0.2382813f, -0.2773438f);
                        ModelVerts[8] = new Vector3(-0.015625f, 0.4101563f, -0.0234375f);
                        ModelVerts[9] = new Vector3(0.2070313f, 0.1484375f, -0.1484375f);
                        ModelVerts[10] = new Vector3(0.125f, 0.3710938f, 0.03125f);
                        ModelVerts[11] = new Vector3(0.2460938f, 0.2617188f, 0.125f);
                        ModelVerts[12] = new Vector3(0.0234375f, 0.2460938f, 0.328125f);
                        ModelVerts[13] = new Vector3(-0.125f, 0.3359375f, 0.2070313f);
                        ModelVerts[14] = new Vector3(-0.09765625f, 0.05859375f, 0.1484375f);
                        ModelVerts[15] = new Vector3(0.02734375f, 0.2421875f, -0.2539063f);
                        ModelVerts[16] = new Vector3(0.1445313f, 0.28125f, -0.1210938f);
                        ModelVerts[17] = new Vector3(-0.0625f, 0.3125f, -0.078125f);
                        ModelVerts[18] = new Vector3(0.03125f, 0.3046875f, -0.1875f);
                        ModelVerts[19] = new Vector3(0.15625f, 0.2773438f, -0.1132813f);
                        ModelVerts[20] = new Vector3(0.06640625f, 0.2695313f, 0.125f);
                        ModelVerts[21] = new Vector3(0.2460938f, 0.1796875f, 0.2460938f);
                        ModelVerts[22] = new Vector3(0.125f, 0.1796875f, 0.3710938f);
                        ModelVerts[23] = new Vector3(0f, 0.3046875f, 0.2890625f);
                        ModelVerts[24] = new Vector3(-0.1328125f, 0.3359375f, 0.3203125f);
                        ModelVerts[25] = new Vector3(-0.2460938f, 0.140625f, 0.125f);
                        ModelVerts[26] = new Vector3(-0.1132813f, 0.05859375f, 0.06640625f);
                        ModelVerts[27] = new Vector3(-0.05078125f, 0.05859375f, -0.015625f);
                        ModelVerts[28] = new Vector3(0.05859375f, 0.05859375f, -0.0234375f);
                        ModelVerts[29] = new Vector3(0.140625f, 0.05859375f, 0.05078125f);
                        ModelVerts[30] = new Vector3(0.125f, 0.05859375f, 0.1640625f);
                        ModelVerts[31] = new Vector3(0.04296875f, 0.05859375f, 0.2226563f);
                        ModelVerts[32] = new Vector3(0.04296875f, 0.3007813f, -0.1796875f);
                        ModelVerts[33] = new Vector3(-0.140625f, 0.2070313f, -0.1132813f);
                        ModelVerts[34] = new Vector3(-0.125f, 0.203125f, -0.1328125f);
                        ModelVerts[35] = new Vector3(0.06640625f, 0.2539063f, -0.2539063f);
                        return ModelVerts;
                    }

                case MediumBoulder://large boulder
                    {
                        Vector3[] ModelVerts = new Vector3[31];
                        ModelVerts[0] = new Vector3(-0.04296875f, 0.203125f, -0.0546875f);
                        ModelVerts[1] = new Vector3(-0.0859375f, 0.2226563f, 0.2148438f);
                        ModelVerts[2] = new Vector3(-0.1367188f, 0.1875f, 0.01171875f);
                        ModelVerts[3] = new Vector3(-0.0703125f, 0.21875f, -0.09375f);
                        ModelVerts[4] = new Vector3(-0.1757813f, 0.1367188f, -0.01171875f);
                        ModelVerts[5] = new Vector3(-0.09765625f, 0.1679688f, -0.1132813f);
                        ModelVerts[6] = new Vector3(0.05078125f, 0.1601563f, -0.1875f);
                        ModelVerts[7] = new Vector3(-0.01171875f, 0.2734375f, -0.015625f);
                        ModelVerts[8] = new Vector3(0.1367188f, 0.09765625f, -0.09765625f);
                        ModelVerts[9] = new Vector3(0.08203125f, 0.2460938f, 0.0234375f);
                        ModelVerts[10] = new Vector3(0.1640625f, 0.1757813f, 0.08203125f);
                        ModelVerts[11] = new Vector3(0.015625f, 0.1640625f, 0.21875f);
                        ModelVerts[12] = new Vector3(-0.06640625f, 0.0390625f, 0.09765625f);
                        ModelVerts[13] = new Vector3(-0.09375f, 0.1445313f, -0.0859375f);
                        ModelVerts[14] = new Vector3(-0.0234375f, 0.25f, -0.02734375f);
                        ModelVerts[15] = new Vector3(0.1054688f, 0.1875f, -0.078125f);
                        ModelVerts[16] = new Vector3(0.0234375f, 0.203125f, -0.125f);
                        ModelVerts[17] = new Vector3(0.04296875f, 0.1796875f, 0.08203125f);
                        ModelVerts[18] = new Vector3(0.1640625f, 0.1210938f, 0.1640625f);
                        ModelVerts[19] = new Vector3(0.08203125f, 0.1210938f, 0.2460938f);
                        ModelVerts[20] = new Vector3(0f, 0.203125f, 0.1914063f);
                        ModelVerts[21] = new Vector3(-0.05078125f, 0.2226563f, 0.140625f);
                        ModelVerts[22] = new Vector3(-0.1640625f, 0.09375f, 0.08203125f);
                        ModelVerts[23] = new Vector3(-0.078125f, 0.0390625f, 0.04296875f);
                        ModelVerts[24] = new Vector3(-0.03125f, 0.0390625f, -0.01171875f);
                        ModelVerts[25] = new Vector3(0.0390625f, 0.0390625f, -0.015625f);
                        ModelVerts[26] = new Vector3(0.09375f, 0.0390625f, 0.03125f);
                        ModelVerts[27] = new Vector3(0.08203125f, 0.0390625f, 0.109375f);
                        ModelVerts[28] = new Vector3(0.02734375f, 0.0390625f, 0.1484375f);
                        ModelVerts[29] = new Vector3(-0.09765625f, 0.2070313f, -0.046875f);
                        ModelVerts[30] = new Vector3(-0.08203125f, 0.1367188f, -0.08984375f);
                        return ModelVerts;
                    }


                case SmallBoulder://A small boulder
                default:
                    {
                        Vector3[] ModelVerts = new Vector3[20];
                        ModelVerts[0] = new Vector3(0.0078125f, 0.078125f, -0.046875f);
                        ModelVerts[1] = new Vector3(-0.03125f, 0.0859375f, 0.08203125f);
                        ModelVerts[2] = new Vector3(-0.03515625f, 0.0625f, -0.04296875f);
                        ModelVerts[3] = new Vector3(-0.06640625f, 0.05078125f, -0.00390625f);
                        ModelVerts[4] = new Vector3(0.015625f, 0.05859375f, -0.0703125f);
                        ModelVerts[5] = new Vector3(0.05078125f, 0.03515625f, -0.03515625f);
                        ModelVerts[6] = new Vector3(0.0625f, 0.06640625f, 0.03125f);
                        ModelVerts[7] = new Vector3(-0.0234375f, 0.01171875f, 0.03515625f);
                        ModelVerts[8] = new Vector3(-0.015625f, 0.0859375f, 0.0546875f);
                        ModelVerts[9] = new Vector3(0.0625f, 0.046875f, 0.0625f);
                        ModelVerts[10] = new Vector3(0.03125f, 0.046875f, 0.09375f);
                        ModelVerts[11] = new Vector3(0f, 0.078125f, 0.07421875f);
                        ModelVerts[12] = new Vector3(0.00390625f, 0.0625f, 0.08203125f);
                        ModelVerts[13] = new Vector3(-0.0625f, 0.03515625f, 0.03125f);
                        ModelVerts[14] = new Vector3(-0.02734375f, 0.01171875f, 0.015625f);
                        ModelVerts[15] = new Vector3(-0.01171875f, 0.01171875f, -0.00390625f);
                        ModelVerts[16] = new Vector3(0.01171875f, 0.01171875f, -0.00390625f);
                        ModelVerts[17] = new Vector3(0.03515625f, 0.01171875f, 0.01171875f);
                        ModelVerts[18] = new Vector3(0.03125f, 0.01171875f, 0.0390625f);
                        ModelVerts[19] = new Vector3(0.0078125f, 0.01171875f, 0.0546875f);
                        return ModelVerts;
                    }
            }
        }


        public override int ModelColour(int meshNo)
        {
            switch (_RES)
                {
                    case GAME_UW2:
                        return 150;
                    default:
                        return 64;
                }
        }
    }
}//end namespace