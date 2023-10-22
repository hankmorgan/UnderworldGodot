using System;
using Godot;
namespace Underworld
{
    public class painting : model3D
    {
        public painting(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public static painting CreateInstance(Node3D parent, uwObject obj, string name)
        {
            var n = new painting(obj);
            var modelNode = n.Generate3DModel(parent, name);
            modelNode.Rotate(Vector3.Up, (float)Math.PI); 
            SetModelRotation(parent, n);  
            modelNode.Position += new Vector3(0f,0f,0.08f); //extrude the model out of the wall           
            //DisplayModelPoints(n, parent,11); 
            return n;
        }



        public override int NoOfMeshes()
        {
            return 2;
        }

        public override int[] ModelTriangles(int meshNo)
        {
            switch (meshNo)
            {
                case 0://Canvas
                    return new int[] { 4, 3, 2, 0, 4, 2 };
                case 1://Frame
                    return new int[] { 5, 1, 0, 2, 5, 0, 6, 5, 2, 3, 6, 2, 7, 6, 3, 4, 7, 3, 1, 7, 4, 0, 1, 4, 6, 7, 8, 9, 6, 8, 1, 5, 10, 11, 1, 10, 6, 9, 10, 5, 6, 10, 7, 1, 11, 8, 7, 11 };
            }
            return base.ModelTriangles(meshNo);
        }


        public override Vector3[] ModelVertices()
        {
            Vector3[] ModelVerts = new Vector3[12];
            ModelVerts[0] = new Vector3(-0.25f, 0.03515625f, 0.05078125f);
            ModelVerts[1] = new Vector3(-0.3046875f, 0f, 0.03125f);
            ModelVerts[2] = new Vector3(-0.25f, 0.2851563f, 0.05078125f);
            ModelVerts[3] = new Vector3(0.25f, 0.2851563f, 0.05078125f);
            ModelVerts[4] = new Vector3(0.25f, 0.03515625f, 0.05078125f);
            ModelVerts[5] = new Vector3(-0.3046875f, 0.3203125f, 0.03125f);
            ModelVerts[6] = new Vector3(0.3046875f, 0.3203125f, 0.03125f);
            ModelVerts[7] = new Vector3(0.3046875f, 0f, 0.03125f);
            ModelVerts[8] = new Vector3(0.3046875f, 0f, 0.05859375f);
            ModelVerts[9] = new Vector3(0.3046875f, 0.3203125f, 0.05859375f);
            ModelVerts[10] = new Vector3(-0.3046875f, 0.3203125f, 0.05859375f);
            ModelVerts[11] = new Vector3(-0.3046875f, 0f, 0.05859375f);
            // for (int i=0;i<12;i++)
            // {
            //     ModelVerts[i] += new Vector3(0.0f,0f,0.5f);
            // }
            return ModelVerts;
        }

        public override int ModelColour(int meshNo)
        {
            switch (meshNo)
            {
                case 0://canvas
                    return 42 + (uwobject.flags & 0x7);
                case 1: //frame
                    return 38;
            }
            return base.ModelColour(meshNo);
        }



        public override Vector2[] ModelUVs(Vector3[] verts)
        {
            Vector2[] uvs = base.ModelUVs(verts);
            //Uvs to align the painting canvas correctly
            uvs[0] = new Vector2(0f, 0f);
            uvs[2] = new Vector2(0f, 1f);
            uvs[3] = new Vector2(1f, 1f);
            uvs[4] = new Vector2(1f, 0f);
            return uvs;
        }

        public override ShaderMaterial GetMaterial(int textureno, int surface)
        {//Get the material texture from tmobj  
            return GetTmObj.GetMaterial((byte)ModelColour(surface));
        }

    } // end class
} //end namespace