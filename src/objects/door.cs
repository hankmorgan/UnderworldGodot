using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
namespace Underworld
{
    public class door : model3D
    {
        const float doorwidth = 0.8f;
        const float doorframewidth = 1.2f;
        const float doorSideWidth = (doorframewidth - doorwidth) / 2f;
        const float doorheight = 7f * 0.15f;

        public int texture;
        public int floorheight;
        public Vector3 position;

        public static door CreateInstance(Node3D parent, uwObject obj, TileMap a_tilemap)
        {
            int tileX = obj.tileX;
            int tileY = obj.tileY;
            var n = new door(obj);
            n.texture = a_tilemap.texture_map[a_tilemap.Tiles[tileX, tileY].wallTexture];
            n.floorheight = a_tilemap.Tiles[tileX, tileY].floorHeight;
            n.position = parent.Position;
            
            if (n.uwobject.heading*45 != tileMapRender.EAST)
            {
                return n;
            }

            var modelNode = n.Generate3DModel(parent);
            //modelNode.Rotate(Vector3.Up,(float)Math.PI);


            switch (n.uwobject.heading*45)
            {
                case tileMapRender.EAST:
                    modelNode.Rotate(Vector3.Up,(float)Math.PI * 1.5f);
                    //align model node in centre of tile along it's axis
                    parent.Position = new Vector3(parent.Position.X, parent.Position.Y, (tileY * 1.2f) + 0.6f);
                    break;
                case tileMapRender.WEST:
                    break;
                case tileMapRender.NORTH:
                    break;
                case tileMapRender.SOUTH:
                    break;
            }

            //render the points for debugging
            var vs = n.ModelVertices();
            int vindex = 0;
            Label3D obj_orign = new();
            obj_orign.Text = $"@";
            obj_orign.Position = Vector3.Zero;
            obj_orign.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
            modelNode.AddChild(obj_orign);

            foreach (var v in vs)
            {
                if (vindex < 20)
                {
                    Label3D obj_lbl = new();
                    obj_lbl.Text = $"{vindex}";
                    obj_lbl.Position = new Vector3(v.X, v.Y, v.Z);
                    obj_lbl.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
                    modelNode.AddChild(obj_lbl);
                }
                vindex++;
            }

            return n;
        }
        public door(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public override int NoOfMeshes()
        {
            return 7;
        }

        public override Vector3[] ModelVertices()
        {
            float ceilingAdjustment = (float)(32 - floorheight) * 0.15f;//- position.Y; 
            //float frameadjustment = (float)(floorheight+4) * 0.15f ;   //0.8125f
            float framethickness = 0.125f;  //0.3125f;
            Vector3[] v = new Vector3[20];
            v[0] = new Vector3(-0.1875f * 1.2f, 0f, 0f);
            v[1] = new Vector3(-0.1875f * 1.2f, 0.8125f * 1.2f, 0f); //frame
            v[2] = new Vector3(0.3125f * 1.2f, 0.8125f * 1.2f, 0f);  //frame
            v[3] = new Vector3(0.3125f * 1.2f, 0f, 0f);
            v[4] = new Vector3(-0.1875f * 1.2f, 0f, framethickness);  //rear
            v[5] = new Vector3(-0.1875f * 1.2f, 0.8125f * 1.2f, framethickness); //frame //rear
            v[6] = new Vector3(0.3125f * 1.2f, 0.8125f * 1.2f, framethickness);  //frame //rear
            v[7] = new Vector3(0.3125f * 1.2f, 0f, framethickness);  //rear
            v[8] = new Vector3(-0.4375f * 1.2f, 0f, 0f);
            v[9] = new Vector3(-0.4375f * 1.2f, 0f, framethickness);   //rear
            v[10] = new Vector3(0.5625f * 1.2f, 0f, 0f);
            v[11] = new Vector3(0.5625f * 1.2f, 0f, framethickness);   //rear
            v[12] = new Vector3(-0.4375f * 1.2f, 0.8125f * 1.2f, 0f);  //level with frame //right
            v[13] = new Vector3(-0.4375f * 1.2f, 0.8125f * 1.2f, framethickness); //level with frame //rear //right
            v[14] = new Vector3(0.5625f * 1.2f, 0.8125f * 1.2f, 0f);  //level with frame  //left
            v[15] = new Vector3(0.5625f * 1.2f, 0.8125f * 1.2f, framethickness);  //level with frame //rear //left
            v[16] = new Vector3(-0.4375f * 1.2f, ceilingAdjustment, 0f);  //ceiling
            v[17] = new Vector3(-0.4375f * 1.2f, ceilingAdjustment, framethickness); //ceiling //rear
            v[18] = new Vector3(0.5625f * 1.2f, ceilingAdjustment, 0f);       //ceiling
            v[19] = new Vector3(0.5625f * 1.2f, ceilingAdjustment, framethickness);  //ceiling //rear
            return v;
        }

        public override int[] ModelTriangles(int meshNo)
        {
            int[] tris = new int[6];

            switch (meshNo)
            {
                case 0:  //over the frame front
                    tris[0] = 16;
                    tris[1] = 12;
                    tris[2] = 14;
                    tris[3] = 14;
                    tris[4] = 18;
                    tris[5] = 16;
                    return tris;
                case 1: // left of frame front.
                    tris[0] = 2;
                    tris[1] = 3;
                    tris[2] = 10;
                    tris[3] = 10;
                    tris[4] = 14;
                    tris[5] = 2;
                    return tris;
                case 2:// right of frame front
                    tris[0] = 12;
                    tris[1] = 8;
                    tris[2] = 0;
                    tris[3] = 0;
                    tris[4] = 1;
                    tris[5] = 12;
                    return tris;

                case 3: // over the frame rear
                    tris[0] = 17;
                    tris[1] = 19;
                    tris[2] = 15;
                    tris[3] = 15;
                    tris[4] = 13;
                    tris[5] = 17;
                    return tris;

                case 4: // left of frame rear
                    tris[0] = 13;
                    tris[1] = 5;
                    tris[2] = 4;
                    tris[3] = 4;
                    tris[4] = 9;
                    tris[5] = 13;
                    return tris;

                case 5: // right of frame rear
                    tris[0] = 6;
                    tris[1] = 15;
                    tris[2] = 11;
                    tris[3] = 11;
                    tris[4] = 7;
                    tris[5] = 6;
                    return tris;

                case 6: //inner frame
                    {
                        tris = new int[18];

                        //right
                        tris[0] = 4;
                        tris[1] = 5;
                        tris[2] = 1;
                        tris[3] = 1;
                        tris[4] = 0;
                        tris[5] = 4;

                        //top
                        tris[6] = 5;
                        tris[7] = 6;
                        tris[8] = 2;
                        tris[9] = 2;
                        tris[10] = 1;
                        tris[11] = 5;

                        //left
                        tris[12] = 2;
                        tris[13] = 6;
                        tris[14] = 7;
                        tris[15] = 7;
                        tris[16] = 3;
                        tris[17] = 2;
                        return tris;
                    }
            }
            return base.ModelTriangles(meshNo);
        }

        public override Vector2[] ModelUVs(Vector3[] verts)
        {
            return base.ModelUVs(verts);
        }

        public override ShaderMaterial GetMaterial(int textureno)
        {//Get the material texture from tmobj
            return tileMapRender.mapTextures.GetMaterial(texture);
        }


    }//end class
} // end namespace