using System.Diagnostics.Tracing;
using Godot;
using Microsoft.VisualBasic;

namespace Underworld
{
    public class shelf : model3D
    {
        public static shelf CreateInstance(Node3D parent, uwObject obj, string name)
        {
            var s = new shelf(obj);
            var modelNode = s.Generate3DModel(parent, name);
            SetModelRotation(parent, s);
            DisplayModelPoints(s, modelNode);
            modelNode.Scale = modelNode.Scale * 2;
            return s;
        }

        public shelf(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public override int NoOfMeshes()
        {
            return 3;
        }

        public override Vector3[] ModelVertices()
        {
            Vector3[] ModelVerts = new Vector3[14];
            ModelVerts[0] = new Vector3(-0.25f, 0.1289063f, -0.125f);
            ModelVerts[1] = new Vector3(-0.25f, 0.1601563f, -0.125f);
            ModelVerts[2] = new Vector3(0.25f, 0.1601563f, -0.125f);
            ModelVerts[3] = new Vector3(0.25f, 0.1289063f, -0.125f);
            ModelVerts[4] = new Vector3(-0.25f, 0.1289063f, 0.125f);
            ModelVerts[5] = new Vector3(-0.25f, 0.1601563f, 0.125f);
            ModelVerts[6] = new Vector3(0.25f, 0.1601563f, 0.125f);
            ModelVerts[7] = new Vector3(0.25f, 0.1289063f, 0.125f);
            ModelVerts[8] = new Vector3(0.1640625f, 0.1289063f, 0.125f);
            ModelVerts[9] = new Vector3(0.1640625f, 0f, 0.125f);
            ModelVerts[10] = new Vector3(0.1640625f, 0.1289063f, -0.00390625f);
            ModelVerts[11] = new Vector3(-0.1640625f, 0.1289063f, 0.125f);
            ModelVerts[12] = new Vector3(-0.1640625f, 0.1289063f, -0.00390625f);
            ModelVerts[13] = new Vector3(-0.1640625f, 0f, 0.125f);
            return ModelVerts;
        }

        public override int[] ModelTriangles(int meshNo)
        {

            switch (meshNo)
            {
                case 0:
                    {   // shelf top and bottom
                        var tris = new int[12];
                        tris[0] = 1;
                        tris[1] = 2;
                        tris[2] = 6;
                        tris[3] = 6;
                        tris[4] = 5;
                        tris[5] = 1;

                        tris[6] = 0;
                        tris[7] = 4;
                        tris[8] = 7;
                        tris[9] = 7;
                        tris[10] = 3;
                        tris[11] = 0;
                        return tris;
                    }
                case 1: // shelf trim
                    {
                        var tris = new int[24];
                        tris[0] = 0;
                        tris[1] = 1;
                        tris[2] = 5;
                        tris[3] = 5;
                        tris[4] = 4;
                        tris[5] = 0;

                        tris[6] = 2;
                        tris[7] = 1;
                        tris[8] = 0;
                        tris[9] = 0;
                        tris[10] = 3;
                        tris[11] = 2;


                        tris[12] = 6;
                        tris[13] = 2;
                        tris[14] = 3;
                        tris[15] = 3;
                        tris[16] = 7;
                        tris[17] = 6;

                        tris[18] = 5;
                        tris[19] = 6;
                        tris[20] = 7;
                        tris[21] = 7;
                        tris[22] = 4;
                        tris[23] = 5;

                        return tris;
                    }
                case 2:
                    {//supports
                        var tris = new int[12];
                        tris[0] = 12;
                        tris[1] = 11;
                        tris[2] = 13;
                        tris[3] = 13;
                        tris[4] = 11;
                        tris[5] = 12;

                        tris[6] = 9;
                        tris[7] = 8;
                        tris[8] = 10;
                        tris[9] = 10;
                        tris[10] = 8;
                        tris[11] = 9;


                        return tris;
                    }
            }

            return base.ModelTriangles(meshNo);
            // return new int[] { 4, 7, 3, 0, 4, 3, 10, 9, 8, 9, 10, 8, 13, 12, 11, 12, 13, 11, 2, 1, 0, 3, 2, 0, 5, 6, 7, 4, 5, 7, 5, 4, 0, 1, 5, 0, 6, 5, 1, 2, 6, 1, 7, 6, 2, 3, 7, 2 };
        }

        public override int ModelColour(int meshNo)
        {
            return 32;
        }

        public override Vector2[] ModelUVs(Vector3[] verts)
        {
            Vector2[] uvs = new Vector2[14];
            uvs[1] = new Vector2(0, 1);  // top of shel
            uvs[2] = new Vector2(0, 0);
            uvs[5] = new Vector2(1, 1);
            uvs[6] = new Vector2(1, 0);

            uvs[0] = new Vector2(0, 1); // underneath shelf
            uvs[3] = new Vector2(0, 0);
            uvs[4] = new Vector2(1, 1);
            uvs[7] = new Vector2(1, 0);

            //8,9,10
            //11,12,13
            uvs[10] = new Vector2(0, 0);
            uvs[8] = new Vector2(1, 0);          
            
            uvs[9] = new Vector2(1, 1);

            uvs[10] = new Vector2(0, 0);
            uvs[12] = new Vector2(1, 0);
            uvs[13] = new Vector2(0, 1);

            return uvs;
        }

        public override ShaderMaterial GetMaterial(int textureno, int surface)
        {//Get the material texture from tmobj    
            switch (surface)
            {
                case 1: //shelf trim
                    return base.GetMaterial(0, 6);
                case 0: //shelf
                default:
                    return GetTmObj.GetMaterial((byte)ModelColour(surface));
            }
        }
    } //end class
}//end namespace