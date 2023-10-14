using System.Collections.Generic;
using Godot;
namespace Underworld
{
    public class door : model3D
    {
        const float doorwidth = 0.8f;
        const float doorframewidth = 1.2f;
        const float doorSideWidth = (doorframewidth - doorwidth) / 2f;
        const float doorheight = 7f * 0.15f;

        public static door CreateInstance(Node3D parent, uwObject obj, TileMap a_tilemap)
        {
            var n = new door(obj);            
            var modelNode = n.Generate3DModel(parent);	
            //modelNode.Rotate(Vector3.Up,(float)Math.PI);	
            return n;
        }
        public door(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public override int NoOfMeshes()
        {
            return 1;
        }

        public override Vector3[] ModelVertices()
        {
            Vector3[] v = new Vector3[10]; //10 front and back
            v[0] = new Vector3(0, 0, 0);
           // v[1] = new Vector3
            return v;
        }


    }//end class
} // end namespace