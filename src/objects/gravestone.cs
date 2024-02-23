using Godot;

namespace Underworld
{
    public class gravestone : model3D
    {

        public static bool Use(uwObject obj)
        {
            DisplayGrave(obj);
            return true;
        }

        /// <summary>
        /// Displays the grave cutscene
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static void DisplayGrave(uwObject obj)
        {
            var graveid = GetGraveID(obj);
            uimanager.AddToMessageScroll(GameStrings.GetString(8, obj.link - 512));   
            uimanager.DisplayCutsImage("cs401.n01", graveid,  uimanager.CutsSmall);
            //"cs401.n01"            
        }

        /// <summary>
        /// Opens graves.dat to get the correct grave no to display.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int GetGraveID(uwObject obj)
        {
            //Load in the grave information
            if (_RES != GAME_UW2)
            {
                Loader.ReadStreamFile(System.IO.Path.Combine(BasePath, "DATA", "GRAVE.DAT"), out byte[] graves);
                if (obj.link >= 512)
                {
                    return (short)Loader.getAt(graves, obj.link - 512, 8) - 1;
                }
            }
            return 0;
        }

        public static gravestone CreateInstance(Node3D parent, uwObject obj, string name)
        {
            var g = new gravestone(obj);
            var modelNode = g.Generate3DModel(parent, name);
            SetModelRotation(parent, g);
            //DisplayModelPoints(g, parent);
            return g;
        }

        public gravestone(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public override int NoOfMeshes()
        {
            return 2;
        }

        public override Vector3[] ModelVertices()
        {
            Vector3[] ModelVerts = new Vector3[8];
            ModelVerts[0] = new Vector3(-0.125f, 0f, -0.015625f);
            ModelVerts[1] = new Vector3(-0.125f, 0.375f, -0.015625f);
            ModelVerts[2] = new Vector3(0.125f, 0.375f, -0.015625f);
            ModelVerts[3] = new Vector3(0.125f, 0f, -0.015625f);
            ModelVerts[4] = new Vector3(-0.125f, 0f, 0.015625f);
            ModelVerts[5] = new Vector3(-0.125f, 0.375f, 0.015625f);
            ModelVerts[6] = new Vector3(0.125f, 0.375f, 0.015625f);
            ModelVerts[7] = new Vector3(0.125f, 0f, 0.015625f);
            return ModelVerts;
        }

        public override int[] ModelTriangles(int meshNo)
        {
            switch (meshNo)
            {
                case 0://headstone
                    return new int[] { 4, 5, 6, 6, 7, 4, 3, 2, 1, 1, 0, 3 };
                case 1://trim
                    return new int[] { 0, 1, 5, 5, 4, 0
                                        , 1, 2, 6, 6, 5, 1, 
                                        7, 6, 2, 2, 3, 7, 
                                        0, 4, 7, 7, 3, 0 };
            }
            return base.ModelTriangles(meshNo);
        }

        public override Vector2[] ModelUVs(Vector3[] verts)
        {
            Vector2[] ModelUvs = new Vector2[8];
            ModelUvs[0] = new Vector2(1f, 1f);
            ModelUvs[1] = new Vector2(1f, 0.3f);
            ModelUvs[2] = new Vector2(0f, 0.3f);
            ModelUvs[3] = new Vector2(0f, 1f);
            ModelUvs[4] = new Vector2(0f, 1f);
            ModelUvs[5] = new Vector2(0f, 0.3f);
            ModelUvs[6] = new Vector2(1f, 0.3f);
            ModelUvs[7] = new Vector2(1f, 1f);
            return ModelUvs;
        }

        public override ShaderMaterial GetMaterial(int textureno, int surface)
        {
            switch (surface)
            {
                case 0://headstone
                    return GetTmObj.GetMaterial(uwobject.flags + 28);
                case 1://Trim
                    return GetTmObj.GetMaterial(0);
            }
            return base.GetMaterial(textureno,surface);
        }
    }//end class
}//end namespace