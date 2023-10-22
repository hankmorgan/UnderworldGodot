using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot;
namespace Underworld
{

    /// <summary>
    /// Class to render the moveable door
    /// </summary>
    public class door : model3D
    {
        static GRLoader tmDoor;
        public Node3D doorNode;
        public int texture;
        public int floorheight;
        public Vector3 position;
        bool isSecretDoor 
        {
            get
            {
                return ((uwobject.item_id == 327) || (uwobject.item_id == 335));
            }
        }

        Vector3 pivot =Vector3.Zero;

        public const float framethickness = 0.08f;

        public bool isOpen
        {
            get
            {
                return uwobject.classindex>=8;
            }
        }

        public bool isPortcullis
        {
            get
            {
                return ((uwobject.classindex == 6) || (uwobject.classindex==0xE));
            }
        }

        static door()
        {
            tmDoor = new GRLoader(GRLoader.DOORS_GR, GRLoader.GRShaderMode.TextureShader);
            tmDoor.RenderGrey = true;
        }

        public static door CreateInstance(Node3D parent, uwObject obj, TileMap a_tilemap, string name)
        {
            int tileX = obj.tileX;
            int tileY = obj.tileY;
            var d = new door(obj);
            if (d.isSecretDoor)
            {
                d.texture = a_tilemap.texture_map[a_tilemap.Tiles[tileX, tileY].wallTexture];
            }
            else
            {
                d.texture = d.uwobject.item_id & 0x7;
            }
            
            d.floorheight = a_tilemap.Tiles[tileX, tileY].floorHeight;
            d.doorNode = d.Generate3DModel(parent, name);
            d.doorNode.Position= d.pivot;
            if (d.isOpen)
            {
                if (d.isPortcullis)
                {//translate model up 1 unit
                    d.doorNode.Position 
                        = new Vector3
                            (d.doorNode.Position.X,
                            d.doorNode.Position.Z + 0.8f,
                            d.doorNode.Position.Y                            
                            );
                 
                }
                else
                {// rotate model 90 
                    d.doorNode.Rotate(Vector3.Up, (float)(Math.PI/2));
                }
            }
            else
            {
                d.position = parent.Position;
            }

            //DisplayModelPoints(d, parent);
            return d;
        }

        public door(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public override Vector3[] ModelVertices()
        {
            ///Same vertices as the doorframe.
            float ceilingAdjustment = (float)(32 - floorheight) * 0.15f;//- position.Y; 
            //float frameadjustment = (float)(floorheight+4) * 0.15f ;   //0.8125f
            
            Vector3[] v = new Vector3[8];
            v[0] = new Vector3(-0.3125f * 1.2f, 0f, 0f); //frame //bottom //right //front           
            v[1] = new Vector3(-0.3125f * 1.2f, 0.8125f * 1.2f, 0f); //frame //right //top //front
            v[2] = new Vector3(0.3125f * 1.2f, 0.8125f * 1.2f, 0f);  //frame // left //top //front
            v[3] = new Vector3(0.3125f * 1.2f, 0f, 0f);//frame // left //bottom //front
            v[4] = new Vector3(-0.3125f * 1.2f, 0f, framethickness);  //rear
            v[5] = new Vector3(-0.3125f * 1.2f, 0.8125f * 1.2f, framethickness); //frame //rear
            v[6] = new Vector3(0.3125f * 1.2f, 0.8125f * 1.2f, framethickness);  //frame //rear
            v[7] = new Vector3(0.3125f * 1.2f, 0f, framethickness);  //rear  
           
            pivot=v[0];
            for (int i =1; i<8;i++)
            {   //translate away so that v[0] is at the model origin.
                v[i] -= v[0];
            }     
            v[0] = Vector3.Zero;     
            return v;
        }

        public override Vector2[] ModelUVs(Vector3[] verts)
        {

            if (!isSecretDoor)
            { //normal door textures have a black border at the top that needs to excluded 
                var uv = new Vector2[8];
                uv[0] = new Vector2(1f, 1f);
                uv[1] = new Vector2(1f, 0.2f);
                uv[2] = new Vector2(0f, 0.2f);
                uv[3] = new Vector2(0f, 1f);

                uv[4] = new Vector2(1f, 1f);
                uv[5] = new Vector2(1f, 0.2f);
                uv[6] = new Vector2(0f, 0.2f);
                uv[7] = new Vector2(0f, 1f);
                return uv;
            }
            else
            {
                //3,2
                //0,1

                float distanceToFloor = (float)(32 - floorheight) * 0.15f; //distance to floor
                float distanceToFrameHead = distanceToFloor - (0.8125f * 1.2f);   //0.975

                float ceilingHeight = 32 * 0.15f;  //4.8f
                float floor = floorheight * 0.15f;
                distanceToFloor = ceilingHeight - floor;
                var vectorToFloor = distanceToFloor / 1.2f;
                var vectorToFrameHead = (vectorToFloor / distanceToFloor) * distanceToFrameHead;

                var frameV0 = 0.185f;
                var frameV1 = 1f - frameV0;//0.760416667f;
                var uv = new Vector2[8];
                uv[0] = new Vector2(frameV1,vectorToFloor);
                uv[1] = new Vector2(frameV1,vectorToFrameHead);
                uv[2] = new Vector2(frameV0, vectorToFrameHead);
                uv[3] = new Vector2(frameV0, vectorToFloor);

                uv[4] = new Vector2(frameV0, vectorToFloor);
                uv[5] = new Vector2(frameV0, vectorToFrameHead);
                uv[6] = new Vector2(frameV1, vectorToFrameHead);
                uv[7] = new Vector2(frameV1, vectorToFloor);

                return uv;
            }

        }

        public override int NoOfMeshes()
        {
            return 3;
        }

        public override int[] ModelTriangles(int meshNo)
        {
            int[] tris = new int[6];
            switch (meshNo)
            {
                case 0: //Door Front
                    tris[0] = 0;
                    tris[1] = 3;
                    tris[2] = 2;
                    tris[3] = 2;
                    tris[4] = 1;
                    tris[5] = 0;
                    return tris;
                case 1: //Door Rear
                    tris[0] = 7;
                    tris[1] = 4;
                    tris[2] = 5;
                    tris[3] = 5;
                    tris[4] = 6;
                    tris[5] = 7;
                    return tris;
                case 2: // Door trim
                    {
                        tris = new int[6 * 4];
                        tris[0] = 4;
                        tris[1] = 0;
                        tris[2] = 1;
                        tris[3] = 1;
                        tris[4] = 5;
                        tris[5] = 4;

                        tris[6] = 5;
                        tris[7] = 1;
                        tris[8] = 2;
                        tris[9] = 2;
                        tris[10] = 6;
                        tris[11] = 5;

                        tris[12] = 3;
                        tris[13] = 7;
                        tris[14] = 6;
                        tris[15] = 6;
                        tris[16] = 2;
                        tris[17] = 3;

                        tris[18] = 0;
                        tris[19] = 4;
                        tris[20] = 7;
                        tris[21] = 7;
                        tris[22] = 3;
                        tris[23] = 0;
                        return tris;
                    }

            }
            return base.ModelTriangles(meshNo);
        }

        public override int ModelColour(int meshNo)
        {
            switch (meshNo)
            {
                case 0:
                case 1:
                    return texture;
                case 2:
                    return 0;
            }
            return base.ModelColour(meshNo);
        }

        public override ShaderMaterial GetMaterial(int textureno, int surface)
        {
            switch (surface)
            {
                case 0: //door texture
                case 1:
                    if (!isSecretDoor)
                    {
                        return tmDoor.GetMaterial(textureno);
                    }
                    else
                    {
                        return tileMapRender.mapTextures.GetMaterial(textureno);
                    }
            }
            return base.GetMaterial(textureno, surface);
        }
    } //end class door


    /// <summary>
    /// Class to render the doorway frames
    /// </summary>
    public class doorway : model3D
    {
        //const float doorwidth = 0.8f;
        //const float doorframewidth = 1.2f;
        // const float doorSideWidth = (doorframewidth - doorwidth) / 2f;
        // const float doorheight = 7f * 0.15f;

        public int texture;
        public float floorheight;
       // public Vector3 position;
        public Node3D doorFrameNode;

        public bool isOpen
        {
            get
            {
                return uwobject.classindex>=8;
            }
        }
        public static doorway CreateInstance(Node3D parent, uwObject obj, TileMap a_tilemap, string name)
        {
            int tileX = obj.tileX;
            int tileY = obj.tileY;
            var dw = new doorway(obj);
            dw.texture = a_tilemap.texture_map[a_tilemap.Tiles[tileX, tileY].wallTexture];
            dw.floorheight = a_tilemap.Tiles[tileX, tileY].floorHeight;//uses floorheight since portculli use zpos when opened // (float)(obj.zpos) / 4f; //a_tilemap.Tiles[tileX, tileY].floorHeight;
            //n.position = parent.Position;
            //a portcullis. 

            dw.doorFrameNode = dw.Generate3DModel(parent, name);
            if ( dw.isOpen )
            {//fix for map bug where some open doors extend out of the map. Force them onto a lower zpos without changing data
                parent.Position = new Vector3(parent.Position.X, uwObject.GetZCoordinate(dw.uwobject.zpos-24), parent.Position.Z);
            }

            SetModelRotation(parent, dw);

            switch (dw.uwobject.heading * 45)
            {//align model node in centre of tile along it's axis
                case tileMapRender.Heading6:
                    parent.Position = new Vector3(parent.Position.X, parent.Position.Y, (tileY * 1.2f) + 0.6f);
                    break;
                case tileMapRender.heading2:
                    parent.Position = new Vector3(parent.Position.X, parent.Position.Y, (tileY * 1.2f) + 0.6f);
                    break;
                case tileMapRender.heading4:
                    parent.Position = new Vector3((tileX * -1.2f) - 0.6f, parent.Position.Y, parent.Position.Z);
                    break;
                case tileMapRender.heading0:
                    parent.Position = new Vector3((tileX * -1.2f) - 0.6f, parent.Position.Y, parent.Position.Z);
                    break;
                default:
                    System.Diagnostics.Debug.Print("Unhandled model centre");
                    break;
            }

            //DisplayModelPoints(dw,parent);
            return dw;
        }

        public doorway(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public override int NoOfMeshes()
        {
            return 8;
        }

        public override Vector3[] ModelVertices()
        {
            float ceilingAdjustment = (float)(32 - floorheight) * 0.15f;//- position.Y;   //distance from object to ceiling
            //float frameadjustment = (float)(floorheight+4) * 0.15f ;   //0.8125f
            
            Vector3[] v = new Vector3[20];
            v[0] = new Vector3(-0.3125f * 1.2f, 0f, 0f);
            v[1] = new Vector3(-0.3125f * 1.2f, 0.8125f * 1.2f, 0f); //frame
            v[2] = new Vector3(0.3125f * 1.2f, 0.8125f * 1.2f, 0f);  //frame
            v[3] = new Vector3(0.3125f * 1.2f, 0f, 0f);
            v[4] = new Vector3(-0.3125f * 1.2f, 0f, door.framethickness);  //rear
            v[5] = new Vector3(-0.3125f * 1.2f, 0.8125f * 1.2f, door.framethickness); //frame //rear
            v[6] = new Vector3(0.3125f * 1.2f, 0.8125f * 1.2f, door.framethickness);  //frame //rear
            v[7] = new Vector3(0.3125f * 1.2f, 0f, door.framethickness);  //rear
            v[8] = new Vector3(-0.6f, 0f, 0f);
            v[9] = new Vector3(-0.6f, 0f, door.framethickness);   //rear
            v[10] = new Vector3(0.6f, 0f, 0f);
            v[11] = new Vector3(0.6f, 0f, door.framethickness);   //rear
            v[12] = new Vector3(-0.6f, 0.8125f * 1.2f, 0f);  //level with frame //right
            v[13] = new Vector3(-0.6f, 0.8125f * 1.2f, door.framethickness); //level with frame //rear //right
            v[14] = new Vector3(0.6f, 0.8125f * 1.2f, 0f);  //level with frame  //left
            v[15] = new Vector3(0.6f, 0.8125f * 1.2f, door.framethickness);  //level with frame //rear //left
            v[16] = new Vector3(-0.6f, ceilingAdjustment, 0f);  //ceiling
            v[17] = new Vector3(-0.6f, ceilingAdjustment, door.framethickness); //ceiling //rear
            v[18] = new Vector3(0.6f, ceilingAdjustment, 0f);       //ceiling
            v[19] = new Vector3(0.6f, ceilingAdjustment, door.framethickness);  //ceiling //rear
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
                case 7:
                {
                    tris = new int[18];
                    tris[0] = 9;
                    tris[1] = 8;
                    tris[2] = 16;
                    tris[3] = 16;
                    tris[4] = 17;
                    tris[5] = 9;

                    tris[6] = 17;
                    tris[7] = 16;
                    tris[8] = 18;
                    tris[9] = 18;
                    tris[10] = 19;
                    tris[11] = 17;

                    tris[12] = 10;
                    tris[13] = 11;
                    tris[14] = 19;
                    tris[15] = 19;
                    tris[16] = 18;
                    tris[17] = 10;
                    return tris;
                }
            }
            return base.ModelTriangles(meshNo);
        }

        public override Vector2[] ModelUVs(Vector3[] verts)
        {
            Vector2[] v = new Vector2[20];
            float distanceToFloor = (float)(32 - floorheight) * 0.15f; //distance to floor
            float distanceToFrameHead = distanceToFloor - (0.8125f * 1.2f);   //0.975

            float ceilingHeight = 32 * 0.15f;  //4.8f
            float floor = floorheight * 0.15f;
            distanceToFloor = ceilingHeight - floor;
            var vectorToFloor = distanceToFloor / 1.2f;
            var vectorToFrameHead = (vectorToFloor / distanceToFloor) * distanceToFrameHead;

            var frameV0 = 0.185f;
            var frameV1 = 1f - frameV0;//0.760416667f;

            v[0] = new Vector2(frameV1, vectorToFloor);
            v[3] = new Vector2(frameV0, vectorToFloor);
            v[4] = new Vector2(frameV0, vectorToFloor);
            v[7] = new Vector2(frameV1, vectorToFloor);
            v[8] = new Vector2(1f, vectorToFloor);
            v[9] = new Vector2(0f, vectorToFloor);
            v[10] = new Vector2(0f, vectorToFloor);
            v[11] = new Vector2(1f, vectorToFloor);

            //midpoint
            //1,2,5,6,12,13,14,15
            v[1] = new Vector2(frameV1, vectorToFrameHead);
            v[2] = new Vector2(frameV0, vectorToFrameHead);
            v[5] = new Vector2(frameV0, vectorToFrameHead);
            v[6] = new Vector2(frameV1, vectorToFrameHead);
            v[12] = new Vector2(1f, vectorToFrameHead);
            v[13] = new Vector2(0f, vectorToFrameHead);
            v[14] = new Vector2(0f, vectorToFrameHead);
            v[15] = new Vector2(1f, vectorToFrameHead);

            // //Top Vertices            
            v[16] = new Vector2(1, 0f);
            v[17] = new Vector2(0, 0f);
            v[18] = new Vector2(0, 0f);
            v[19] = new Vector2(1, 0f);

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


    }//end class
} // end namespace