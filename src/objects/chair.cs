using Godot;
namespace Underworld
{
    public class chair : model3D
    {
        public static chair CreateInstance(Node3D parent, uwObject obj, string name)
        {
            var c = new chair(obj);
            var modelNode = c.Generate3DModel(parent, name);
            SetModelRotation(parent, c);
            return c;
        }

        public chair(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public override Vector3[] ModelVertices()
        {
            Vector3[] ModelVerts = new Vector3[36];
            ModelVerts[0] = new Vector3(0.15625f, 0.296875f, 0.1367188f);
            ModelVerts[1] = new Vector3(0.1679688f, 0f, 0.1367188f);
            ModelVerts[2] = new Vector3(0.1679688f, 0f, 0.1054688f);
            ModelVerts[3] = new Vector3(0.15625f, 0.2421875f, 0.0703125f);
            ModelVerts[4] = new Vector3(0.15625f, 0.2421875f, -0.0703125f);
            ModelVerts[5] = new Vector3(0.1679688f, 0f, -0.1054688f);
            ModelVerts[6] = new Vector3(0.1679688f, 0f, -0.1367188f);
            ModelVerts[7] = new Vector3(0.15625f, 0.296875f, -0.1367188f);
            ModelVerts[8] = new Vector3(-0.04296875f, 0.296875f, -0.1367188f);
            ModelVerts[9] = new Vector3(-0.0703125f, 0.296875f, 0f);
            ModelVerts[10] = new Vector3(-0.04296875f, 0.296875f, 0.1367188f);
            ModelVerts[11] = new Vector3(-0.1171875f, 0.671875f, -0.1367188f);
            ModelVerts[12] = new Vector3(-0.1445313f, 0.671875f, 0f);
            ModelVerts[13] = new Vector3(-0.1171875f, 0.671875f, 0.1367188f);
            ModelVerts[14] = new Vector3(-0.09375f, 0.296875f, 0.1367188f);
            ModelVerts[15] = new Vector3(-0.1445313f, 0.671875f, 0.1367188f);
            ModelVerts[16] = new Vector3(-0.1679688f, 0.671875f, 0f);
            ModelVerts[17] = new Vector3(-0.1171875f, 0.296875f, 0f);
            ModelVerts[18] = new Vector3(-0.1445313f, 0.671875f, -0.1367188f);
            ModelVerts[19] = new Vector3(-0.09375f, 0.296875f, -0.1367188f);
            ModelVerts[20] = new Vector3(-0.15625f, 0f, 0.1367188f);
            ModelVerts[21] = new Vector3(-0.1054688f, 0f, 0.1367188f);
            ModelVerts[22] = new Vector3(-0.04296875f, 0.2421875f, 0.1367188f);
            ModelVerts[23] = new Vector3(0.1015625f, 0.2421875f, 0.1367188f);
            ModelVerts[24] = new Vector3(0.1328125f, 0f, 0.1367188f);
            ModelVerts[25] = new Vector3(-0.15625f, 0f, 0.1054688f);
            ModelVerts[26] = new Vector3(-0.15625f, 0f, -0.1367188f);
            ModelVerts[27] = new Vector3(-0.15625f, 0f, -0.1054688f);
            ModelVerts[28] = new Vector3(-0.1054688f, 0.234375f, -0.0703125f);
            ModelVerts[29] = new Vector3(-0.1054688f, 0.234375f, 0.0703125f);
            ModelVerts[30] = new Vector3(-0.1054688f, 0f, -0.1367188f);
            ModelVerts[31] = new Vector3(-0.04296875f, 0.2421875f, -0.1367188f);
            ModelVerts[32] = new Vector3(0.1015625f, 0.2421875f, -0.1367188f);
            ModelVerts[33] = new Vector3(0.1328125f, 0f, -0.1367188f);
            ModelVerts[34] = new Vector3(-0.1054688f, 0.2421875f, 0.0703125f);
            ModelVerts[35] = new Vector3(-0.1054688f, 0.2421875f, -0.0703125f);
            return ModelVerts;
        }


        public override int NoOfMeshes()
        {
            return 2;
        }

        public override int[] ModelTriangles(int meshNo)
        {
            switch (meshNo)
            {
                case 0: //frame
                    return new int[] { 1, 0, 2, 0, 3, 2, 1, 24, 23, 1, 23, 0, 5, 7, 6, 5, 4, 7, 6, 7, 32, 33, 6, 32, 20, 22, 21, 20, 20, 25, 20, 25, 29, 20, 14, 22, 20, 29, 14, 26, 28, 27, 26, 19, 28, 26, 30, 19, 30, 31, 19, 3, 7, 4, 3, 0, 7, 23, 22, 0, 0, 22, 14, 7, 31, 32, 7, 19, 31, 28, 19, 29, 29, 19, 14, 14, 13, 10, 14, 15, 13, 13, 13, 13, 19, 8, 18, 8, 11, 18, 14, 19, 15, 15, 19, 18, 18, 11, 13, 18, 13, 15 };
                case 1: //cushion
                    return new int[] { 0, 10, 8, 0, 8, 7, 10, 13, 11, 10, 11, 8 };
            }
            return base.ModelTriangles(meshNo);
        }

        public override int ModelColour(int meshNo)
        {
            switch (meshNo)
            {
                case 0://Frame
                    if (_RES == GAME_UW2)
                    {
                        return 32;
                    }
                    else
                    {
                        return 30;
                    }
                default://cushion
                    if (_RES == GAME_UW2)
                    {
                        return 38;
                    }
                    else
                    {
                        return 32;
                    }
            }
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
    }//end class   
}//end namespace