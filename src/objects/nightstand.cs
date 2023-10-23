using Godot;
namespace Underworld
{
    public class nightstand : model3D
    {
        public static nightstand CreateInstance(Node3D parent, uwObject obj, string name)
        {
            var n = new nightstand(obj);
            var modelNode = n.Generate3DModel(parent, name);
            SetModelRotation(parent, n);
       
            return n;
        }

        public nightstand(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public override Vector3[] ModelVertices()
        {
            Vector3[] ModelVerts = new Vector3[40];
            ModelVerts[0] = new Vector3(0.1171875f, 0.1835938f, -0.1171875f);
            ModelVerts[1] = new Vector3(-0.09375f, -0.00390625f, -0.07421875f);
            ModelVerts[2] = new Vector3(-0.1171875f, 0.1835938f, -0.1171875f);
            ModelVerts[3] = new Vector3(-0.1171875f, 0.1835938f, 0.1679688f);
            ModelVerts[4] = new Vector3(0.1171875f, 0.1835938f, 0.1679688f);
            ModelVerts[5] = new Vector3(0.1171875f, 0.1679688f, -0.1171875f);
            ModelVerts[6] = new Vector3(0.1171875f, 0.1679688f, 0.1679688f);
            ModelVerts[7] = new Vector3(-0.1171875f, 0.1679688f, 0.1679688f);
            ModelVerts[8] = new Vector3(-0.1171875f, 0.1679688f, -0.1171875f);
            ModelVerts[9] = new Vector3(-0.09375f, 0.1679688f, -0.0625f);
            ModelVerts[10] = new Vector3(-0.09375f, 0.1679688f, -0.0859375f);
            ModelVerts[11] = new Vector3(-0.09375f, -0.00390625f, -0.0859375f);
            ModelVerts[12] = new Vector3(-0.0703125f, 0.1679688f, -0.0859375f);
            ModelVerts[13] = new Vector3(-0.08203125f, -0.00390625f, -0.0859375f);
            ModelVerts[14] = new Vector3(-0.08203125f, -0.00390625f, -0.07421875f);
            ModelVerts[15] = new Vector3(-0.0703125f, 0.1679688f, -0.0625f);
            ModelVerts[16] = new Vector3(0.0703125f, 0.1679688f, -0.0625f);
            ModelVerts[17] = new Vector3(0.08203125f, -0.00390625f, -0.07421875f);
            ModelVerts[18] = new Vector3(0.09375f, -0.00390625f, -0.07421875f);
            ModelVerts[19] = new Vector3(0.09375f, 0.1679688f, -0.0625f);
            ModelVerts[20] = new Vector3(0.08203125f, -0.00390625f, -0.0859375f);
            ModelVerts[21] = new Vector3(0.0703125f, 0.1679688f, -0.0859375f);
            ModelVerts[22] = new Vector3(0.09375f, -0.00390625f, -0.0859375f);
            ModelVerts[23] = new Vector3(0.09375f, 0.1679688f, -0.0859375f);
            ModelVerts[24] = new Vector3(0.09375f, -0.00390625f, 0.125f);
            ModelVerts[25] = new Vector3(0.09375f, 0.1679688f, 0.1132813f);
            ModelVerts[26] = new Vector3(0.09375f, 0.1679688f, 0.1367188f);
            ModelVerts[27] = new Vector3(0.09375f, -0.00390625f, 0.1367188f);
            ModelVerts[28] = new Vector3(0.0703125f, 0.1679688f, 0.1367188f);
            ModelVerts[29] = new Vector3(0.08203125f, -0.00390625f, 0.1367188f);
            ModelVerts[30] = new Vector3(0.08203125f, -0.00390625f, 0.125f);
            ModelVerts[31] = new Vector3(0.0703125f, 0.1679688f, 0.1132813f);
            ModelVerts[32] = new Vector3(-0.0703125f, 0.1679688f, 0.1132813f);
            ModelVerts[33] = new Vector3(-0.08203125f, -0.00390625f, 0.125f);
            ModelVerts[34] = new Vector3(-0.09375f, -0.00390625f, 0.125f);
            ModelVerts[35] = new Vector3(-0.09375f, 0.1679688f, 0.1132813f);
            ModelVerts[36] = new Vector3(-0.08203125f, -0.00390625f, 0.1367188f);
            ModelVerts[37] = new Vector3(-0.0703125f, 0.1679688f, 0.1367188f);
            ModelVerts[38] = new Vector3(-0.09375f, -0.00390625f, 0.1367188f);
            ModelVerts[39] = new Vector3(-0.09375f, 0.1679688f, 0.1367188f);
            return ModelVerts;
        }

        public override Vector2[] ModelUVs(Vector3[] verts)
        {
            var uvs = base.ModelUVs(verts);
             uvs[4] = new Vector2(0, 0);
             uvs[3] = new Vector2(1, 0);
            uvs[0] = new Vector2(0,1);
           
            uvs[2] = new Vector2(1, 1);
            
            
            return uvs;

        }

        public override int[] ModelTriangles(int meshNo)
        {
            return new int[] { 0, 2, 3, 4, 0, 3, 35, 39, 38, 34, 35, 38, 39, 37, 36, 38, 39, 36, 28, 26, 27, 29, 28, 27, 26, 25, 24, 27, 26, 24, 19, 23, 22, 18, 19, 22, 23, 21, 20, 22, 23, 20, 12, 10, 11, 13, 12, 11, 10, 9, 1, 11, 10, 1, 3, 2, 0, 4, 3, 0, 4, 0, 5, 6, 4, 5, 3, 4, 6, 7, 3, 6, 2, 3, 7, 8, 2, 7, 0, 2, 8, 5, 0, 8, 32, 33, 36, 37, 32, 36, 14, 15, 12, 13, 14, 12, 30, 24, 25, 31, 30, 25, 16, 17, 20, 21, 16, 20, 18, 17, 16, 19, 18, 16, 30, 31, 28, 29, 30, 28, 35, 39, 38, 34, 35, 38, 39, 37, 36, 38, 39, 36, 28, 26, 27, 29, 28, 27, 26, 25, 24, 27, 26, 24, 19, 23, 22, 18, 19, 22, 23, 21, 20, 22, 23, 20, 12, 10, 11, 13, 12, 11, 10, 9, 1, 11, 10, 1, 3, 2, 0, 4, 3, 0, 4, 0, 5, 6, 4, 5, 3, 4, 6, 7, 3, 6, 2, 3, 7, 8, 2, 7, 0, 2, 8, 5, 0, 8 };
        }

        public override int ModelColour(int meshNo)
        {
            return 1;
        }

        public override ShaderMaterial GetMaterial(int textureno, int surface)
        {//Get the material texture from tmobj    
            switch (surface)
            {
                default:
                    return GetTmObj.GetMaterial((byte)ModelColour(surface));
            }
        }

    }//end class
}//end namespace