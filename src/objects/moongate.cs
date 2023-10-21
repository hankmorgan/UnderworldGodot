using Godot;
namespace Underworld
{
    public class moongate : model3D
    {
        public static moongate CreateInstance(Node3D parent, uwObject obj)
        {
            var n = new moongate(obj);
            var modelNode = n.Generate3DModel(parent);
            SetModelRotation(parent, n);
            return n;
        }

        public moongate(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public override Vector3[] ModelVertices()
        {
            Vector3[] ModelVerts = new Vector3[8];
            ModelVerts[0] = new Vector3(-0.375f, 0f, 0f);
            ModelVerts[1] = new Vector3(0.375f, 0f, 0f);
            ModelVerts[2] = new Vector3(0.375f, 1.25f, 0f);
            ModelVerts[3] = new Vector3(-0.375f, 1.25f, 0f);
            ModelVerts[4] = new Vector3(-0.375f, 0f, 0.025f);
            ModelVerts[5] = new Vector3(0.375f, 0f, 0.025f);
            ModelVerts[6] = new Vector3(0.375f, 1.25f, 0.025f);
            ModelVerts[7] = new Vector3(-0.375f, 1.25f, 0.025f);
            return ModelVerts;
        }


        public override int[] ModelTriangles(int meshNo)
        {
            return new int[] { 1, 2, 3, 1, 3, 0, 4, 7, 6, 4, 6, 5 };
        }

        public override int ModelColour(int meshNo)
        {
            return uwobject.link - 512;
        }

        public override Vector2[] ModelUVs(Vector3[] verts)
        {
            Vector2[] ModelUVs = new Vector2[8];
            ModelUVs[0] = new Vector2(0f, 0f);
            ModelUVs[1] = new Vector2(0f, 1f);
            ModelUVs[2] = new Vector2(1f, 1f);
            ModelUVs[3] = new Vector2(1f, 0f);
            ModelUVs[4] = new Vector2(0f, 0f);
            ModelUVs[5] = new Vector2(0f, 1f);
            ModelUVs[6] = new Vector2(1f, 1f);
            ModelUVs[7] = new Vector2(1f, 0f);
            return ModelUVs;
        }
    }//end class
}//end namespace