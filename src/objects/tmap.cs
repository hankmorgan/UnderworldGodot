using Godot;

namespace Underworld
{
    public class tmap:model3D
    {
        int texture;
        Node3D tmapnode;

        public tmap(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public static tmap CreateInstance(Node3D parent, uwObject obj, TileMap a_tilemap)
        {
            int tileX = obj.tileX;
            int tileY = obj.tileY;
            var t = new tmap(obj);
            t.texture = a_tilemap.texture_map[obj.owner];
    
            t.tmapnode = t.Generate3DModel(parent);
            
            SetModelRotation(parent,t);
            centreInTile(parent, t);
            
            //DisplayModelPoints(t, parent);
            return t;
        }

        //Centeres the tmap in the tile it is in.
        static void centreInTile(Node3D node, tmap t)
        {
            int x = t.uwobject.tileX;
            int y = t.uwobject.tileY;
            switch (t.uwobject.heading * 45)
            {
                case tileMapRender.heading0: 
                    node.Position = new Vector3(-(x * 1.2f + 0.6f), node.Position.Y, node.Position.Z);              
                    break;
                case tileMapRender.heading2: 
                     node.Position = new Vector3(node.Position.X, node.Position.Y, y * 1.2f + 0.6f); 
                    break;
                case tileMapRender.heading4: 
                    node.Position = new Vector3(-(x * 1.2f + 0.6f), node.Position.Y, node.Position.Z); 
                    break;
                case tileMapRender.Heading6: 
                    node.Position = new Vector3(node.Position.X, node.Position.Y, y * 1.2f + 0.6f); 
                    break;
                default:
                    System.Diagnostics.Debug.Print($"Unhandled tmap centering heading. {t.uwobject.item_id} h:{t.uwobject.heading}");
                    break;
            }
        }


        public override Vector3[] ModelVertices()
        {
            Vector3[] v = new Vector3[4];
            v[0] = new Vector3(-0.6f, 0f, 0.0625f);
            v[1] = new Vector3(0.6f, 0f, 0.0625f);
            v[2] = new Vector3(0.6f, 1.2f, 0.0625f);
            v[3] = new Vector3(-0.6f, 1.2f, 0.0625f);
            return v;
        }

        public override int[] ModelTriangles(int meshNo)
        {
            //face
            int[] tris = new int[6];
            tris[0] = 1;
            tris[1] = 0;
            tris[2] = 3;
            tris[3] = 3;
            tris[4] = 2;
            tris[5] = 1;
            return tris;
        }

        public override Vector2[] ModelUVs(Vector3[] verts)
        {
            Vector2[] v = new Vector2[4];
            v[0] = new Vector2(0,1); 
            v[1] = new Vector2(1,1); 
            v[2] = new Vector2(1,0); 
            v[3]  = new Vector2(0,0); 
            return v;
        }


        public override ShaderMaterial GetMaterial(int textureno, int surface)
        {//Get the material texture from tmobj   
            if (surface != 6)
            {
                return tileMapRender.mapTextures.GetMaterial(texture);
            }
            else
            {
                return base.GetMaterial(0, 6);
            }
        }
    } //end class
}//end namespace