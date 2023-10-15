using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
        bool SecretDoor = false;//327 and 325

        static door()
        {
            tmDoor = new GRLoader(GRLoader.DOORS_GR, GRLoader.GRShaderMode.TextureShader);
            tmDoor.RenderGrey = true;
        }

        public static door CreateInstance(Node3D parent, uwObject obj, TileMap a_tilemap)
        {
            int tileX = obj.tileX;
            int tileY = obj.tileY;
            var d = new door(obj);
            if (d.SecretDoor)
            {
                d.texture = a_tilemap.texture_map[a_tilemap.Tiles[tileX, tileY].wallTexture];
            }
            else
            {
                d.texture = d.uwobject.item_id & 0x7;
            }

            d.floorheight = a_tilemap.Tiles[tileX, tileY].floorHeight;
            d.position = parent.Position;
            d.doorNode = d.Generate3DModel(parent);

              DisplayModelPoints(d,parent);
            return d;
        }

        public door(uwObject _uwobject)
        {
            uwobject = _uwobject;
            if ((uwobject.item_id == 327) || (uwobject.item_id == 325))
            {
                SecretDoor = true;
            }
        }

        public override Vector3[] ModelVertices()
        {
            ///Same vertices as the doorframe.
            float ceilingAdjustment = (float)(32 - floorheight) * 0.15f;//- position.Y; 
            //float frameadjustment = (float)(floorheight+4) * 0.15f ;   //0.8125f
            float framethickness = 0.1f; 
            Vector3[] v = new Vector3[8];
            v[0] = new Vector3(-0.3125f * 1.2f, 0f, 0f); //frame //bottom //right //front
            v[1] = new Vector3(-0.3125f * 1.2f, 0.8125f * 1.2f, 0f); //frame //right //top //front
            v[2] = new Vector3(0.3125f * 1.2f, 0.8125f * 1.2f, 0f);  //frame // left //top //front
            v[3] = new Vector3(0.3125f * 1.2f, 0f, 0f);//frame // left //bottom //front
            v[4] = new Vector3(-0.3125f * 1.2f, 0f, framethickness);  //rear
            v[5] = new Vector3(-0.3125f * 1.2f, 0.8125f * 1.2f, framethickness); //frame //rear
            v[6] = new Vector3(0.3125f * 1.2f, 0.8125f * 1.2f, framethickness);  //frame //rear
            v[7] = new Vector3(0.3125f * 1.2f, 0f, framethickness);  //rear            
            return v;
        }

        public override Vector2[] ModelUVs(Vector3[] verts)
        {
            if (!SecretDoor)
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
                var uv = new Vector2[8];
                uv[0] = new Vector2(1f, 1f);
                uv[1] = new Vector2(1f, 0.0f);
                uv[2] = new Vector2(0f, 0.0f);
                uv[3] = new Vector2(0f, 1f);

                uv[4] = new Vector2(1f, 1f);
                uv[5] = new Vector2(1f, 0.0f);
                uv[6] = new Vector2(0f, 0.0f);
                uv[7] = new Vector2(0f, 1f);

                return uv;
            }

        }

        public override int NoOfMeshes()
        {
            return 2;
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
                        //TODO
                        break;
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
                    if (!SecretDoor)
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
        public Vector3 position;

        public Node3D doorFrameNode;

        public static doorway CreateInstance(Node3D parent, uwObject obj, TileMap a_tilemap)
        {
            int tileX = obj.tileX;
            int tileY = obj.tileY;
            var n = new doorway(obj);
            n.texture = a_tilemap.texture_map[a_tilemap.Tiles[tileX, tileY].wallTexture];
            n.texture = 27;
            n.floorheight = (float)(obj.zpos) / 4f; //a_tilemap.Tiles[tileX, tileY].floorHeight;
            n.position = parent.Position;
            n.doorFrameNode = n.Generate3DModel(parent);

            switch (n.uwobject.heading * 45)
            {//align model node in centre of tile along it's axis
                case tileMapRender.EAST:
                    parent.Rotate(Vector3.Up, (float)Math.PI * 1.5f);
                    parent.Position = new Vector3(parent.Position.X, parent.Position.Y, (tileY * 1.2f) + 0.6f);
                    break;
                case tileMapRender.WEST:
                    parent.Rotate(Vector3.Up, (float)Math.PI / 2f);
                    parent.Position = new Vector3(parent.Position.X, parent.Position.Y, (tileY * 1.2f) + 0.6f);
                    break;
                case tileMapRender.NORTH:
                    parent.Position = new Vector3((tileX * -1.2f) - 0.6f, parent.Position.Y, parent.Position.Z);
                    break;
                case tileMapRender.SOUTH:
                    //modelNode.Rotate(Vector3.Up,(float)Math.PI /2f);                    
                    parent.Position = new Vector3((tileX * -1.2f) - 0.6f, parent.Position.Y, parent.Position.Z);
                    break;
            }       

            return n;
        }        

        public doorway(uwObject _uwobject)
        {
            uwobject = _uwobject;
        }

        public override int NoOfMeshes()
        {
            return 7;
        }

        public override Vector3[] ModelVertices()
        {
            float ceilingAdjustment = (float)(32 - floorheight) * 0.15f;//- position.Y;   //distance from object to ceiling
            //float frameadjustment = (float)(floorheight+4) * 0.15f ;   //0.8125f
            float framethickness = 0.1f;
            Vector3[] v = new Vector3[20];
            v[0] = new Vector3(-0.3125f * 1.2f, 0f, 0f);
            v[1] = new Vector3(-0.3125f * 1.2f, 0.8125f * 1.2f, 0f); //frame
            v[2] = new Vector3(0.3125f * 1.2f, 0.8125f * 1.2f, 0f);  //frame
            v[3] = new Vector3(0.3125f * 1.2f, 0f, 0f);
            v[4] = new Vector3(-0.3125f * 1.2f, 0f, framethickness);  //rear
            v[5] = new Vector3(-0.3125f * 1.2f, 0.8125f * 1.2f, framethickness); //frame //rear
            v[6] = new Vector3(0.3125f * 1.2f, 0.8125f * 1.2f, framethickness);  //frame //rear
            v[7] = new Vector3(0.3125f * 1.2f, 0f, framethickness);  //rear
            v[8] = new Vector3(-0.6f, 0f, 0f);
            v[9] = new Vector3(-0.6f, 0f, framethickness);   //rear
            v[10] = new Vector3(0.6f, 0f, 0f);
            v[11] = new Vector3(0.6f, 0f, framethickness);   //rear
            v[12] = new Vector3(-0.6f, 0.8125f * 1.2f, 0f);  //level with frame //right
            v[13] = new Vector3(-0.6f, 0.8125f * 1.2f, framethickness); //level with frame //rear //right
            v[14] = new Vector3(0.6f, 0.8125f * 1.2f, 0f);  //level with frame  //left
            v[15] = new Vector3(0.6f, 0.8125f * 1.2f, framethickness);  //level with frame //rear //left
            v[16] = new Vector3(-0.6f, ceilingAdjustment, 0f);  //ceiling
            v[17] = new Vector3(-0.6f, ceilingAdjustment, framethickness); //ceiling //rear
            v[18] = new Vector3(0.6f, ceilingAdjustment, 0f);       //ceiling
            v[19] = new Vector3(0.6f, ceilingAdjustment, framethickness);  //ceiling //rear
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
            Vector2[] v = new Vector2[20];
            float distanceToFloor = (float)(32 - floorheight) * 0.15f; //distance to floor
            float distanceToFrameHead = distanceToFloor - (0.8125f * 1.2f);   //0.975

            float ceilingHeight = 32 * 0.15f;  //4.8f
            float floor = floorheight * 0.15f;
            distanceToFloor = ceilingHeight-floor;
            var vectorToFloor = distanceToFloor/1.2f;
            var vectorToFrameHead  = (vectorToFloor/distanceToFloor) * distanceToFrameHead;

            var frameV0 = 0.185f;
            var frameV1 = 1f-frameV0;//0.760416667f;

            v[0] = new Vector2(frameV1, vectorToFloor); 
            v[3] = new Vector2(frameV0, vectorToFloor); 
            v[4] = new Vector2(frameV0,vectorToFloor);
            v[7] = new Vector2(frameV1,vectorToFloor);
            v[8] = new Vector2(1f, vectorToFloor); 
            v[9] = new Vector2(0f, vectorToFloor); 
            v[10] = new Vector2(0f, vectorToFloor); 
            v[11] = new Vector2(1f, vectorToFloor); 


            //midpoint
            //1,2,5,6,12,13,14,15
            v[1] = new Vector2(frameV1, vectorToFrameHead);
            v[2] = new Vector2(frameV0, vectorToFrameHead); 
            v[5] = new Vector2(frameV0,vectorToFrameHead);
            v[6] = new Vector2(frameV1,vectorToFrameHead);
            v[12] = new Vector2(1f,vectorToFrameHead); 
            v[13] = new Vector2(0f,vectorToFrameHead);
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
            if (surface!=6)
            {
                return tileMapRender.mapTextures.GetMaterial(texture);
            }   
            else
            {
                return base.GetMaterial(0,6);            
            }              
        }


    }//end class
} // end namespace