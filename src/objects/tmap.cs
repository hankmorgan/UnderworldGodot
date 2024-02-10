using System.Diagnostics;
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

        public static tmap CreateInstance(Node3D parent, uwObject obj, UWTileMap a_tilemap, string name)
        {
            int tileX = obj.tileX;
            int tileY = obj.tileY;
            var t = new tmap(obj);
            t.texture = a_tilemap.texture_map[obj.owner];
    
            t.tmapnode = t.Generate3DModel(parent, name);
            
            SetModelRotation(parent,t);
            centreAlongAxis(parent, t);
            
            //DisplayModelPoints(t, parent);
            return t;
        }    

        public static bool LookAt(uwObject obj)
        {
            int textureindex = UWTileMap.current_tilemap.texture_map[obj.owner];
            uimanager.AddToMessageScroll(GameStrings.TextureDescription(textureindex));
            if ((textureindex == 142) && ((_RES == GAME_UW1) || (_RES == GAME_UWDEMO)))
            {//This is a window into the abyss.
                Debug.Print("Look into the volcano");
                // UWHUD.instance.CutScenesSmall.anim.SetAnimation = "VolcanoWindow_" + GameWorldController.instance.dungeon_level;
                // UWHUD.instance.CutScenesSmall.anim.looping = true;
            }
            return true; //prevents the default you cannot use message
        }


        public override Vector3[] ModelVertices()
        {
            Vector3[] v = new Vector3[4];
            v[0] = new Vector3(-0.6f, 0f, 0f);//0.0625f);
            v[1] = new Vector3(0.6f, 0f, 0f);//0.0625f);
            v[2] = new Vector3(0.6f, 1.2f, 0f);//0.0625f);
            v[3] = new Vector3(-0.6f, 1.2f, 0f);//..0.0625f);
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