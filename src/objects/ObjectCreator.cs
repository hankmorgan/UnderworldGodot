using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;

namespace Underworld
{
    /// <summary>
    /// Class for creating instances of objects and calling updates
    /// </summary>
    public class ObjectCreator : UWClass
    {
        //List of active NPCs
        public static List<npc> npcs;
        static bool printlabels = true;

        public static void GenerateObjects(Node3D worldparent, List<uwObject> objects, GRLoader grObjects, TileMap a_tilemap)
        {
            npcs = new();
            foreach (var obj in objects)
            {
                if (obj.item_id <= 463)
                {
                    RenderObject(worldparent, grObjects, obj, a_tilemap);
                }
            }
        }

        public static void RenderObject(Node3D worldparent, GRLoader grObjects, uwObject obj, TileMap a_tilemap)
        {
            bool unimplemented = true;

            var newparent = new Node3D();
            newparent.Name = StringLoader.GetObjectNounUW(obj.item_id) + "_" + obj.index.ToString();
            newparent.Position = obj.GetCoordinate(obj.tileX, obj.tileY);
            worldparent.AddChild(newparent);

            switch (obj.majorclass)
            {
                case 1://npcs
                    {
                        if (obj.item_id<124)
                        {
                        npcs.Add(npc.CreateInstance(newparent, obj));
                        unimplemented = false;
                        }
                        break;
                    }
                case 3: // misc objects
                {
                    unimplemented = MajorClass3(obj, newparent, grObjects);
                    break;
                }
                case 5: //doors, 3d models, buttons/switches
                    {
                        unimplemented = MajorClass5(obj, newparent, a_tilemap);
                        break;
                    }

                case 0://Weapons
                case 2://misc items incl containers, food, and lights.                
                case 4://keys, usables and readables
                case 6://Traps and Triggers
                case 7://Animos
                default:
                    unimplemented = true; break;

            }

            if (unimplemented)
            {
                //just render a sprite.
                CreateSpriteInstance(grObjects, obj, newparent);
                if (printlabels)
                {
                    Label3D obj_lbl = new();
                    obj_lbl.Text = $"{StringLoader.GetObjectNounUW(obj.item_id)} {obj.index}";
                    obj_lbl.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
                    //obj_lbl.Font = font;
                    newparent.AddChild(obj_lbl);
                }
            }
        }


        /// <summary>
        /// Runestones and some misc objects
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="parent"></param>
        /// <param name="grObjects"></param>
        /// <returns></returns>
        private static bool MajorClass3(uwObject obj, Node3D parent, GRLoader grObjects)
        {
            if ((obj.minorclass == 2) && (obj.classindex == 0))
            {
                runestone.CreateInstance(parent, obj, grObjects);
                return false;
            }
            if (((obj.minorclass == 2) && (obj.classindex >= 8))
                || (obj.minorclass == 3)
                || ((obj.minorclass == 2) && (obj.classindex == 0)))
            {//runestones
                runestone.CreateInstance(parent, obj, grObjects);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Doors, 3d models, buttons/switches
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="unimplemented"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private static bool MajorClass5(uwObject obj, Node3D parent, TileMap a_tilemap)
        {
            if (obj.tileX >= 65) { return true; }//don't render offmap models.
            switch (obj.minorclass)
            {
                case 0: //doors
                    {
                        door.CreateInstance(parent, obj, a_tilemap);
                        doorway.CreateInstance(parent, obj, a_tilemap);
                        return false;
                    }
                case 1: //3D Models
                    {
                        if ((obj.classindex>=3) && (obj.classindex<=6))
                        {//boulders
                            boulder.CreateInstance(parent,obj);
                            return false;
                        }
                        if (obj.classindex == 7)
                        {//shrine
                            shrine.CreateInstance(parent, obj);
                            return false;
                        }
                        if (obj.classindex == 8)
                        {
                            table.CreateInstance(parent, obj);
                            return false;
                        }   
                        if (obj.classindex == 0xA)
                        {
                            moongate.CreateInstance(parent, obj);
                            return false;
                        }                        
                        break;
                    }
                case 2: //3D models
                    {
                        if (obj.classindex == 0)
                        {//pillar 352
                            pillar.CreateInstance(parent, obj, parent.Position);
                            return false; /// unimplemented = false;
                        }
                        if ((_RES == GAME_UW2) && (obj.classindex == 3))
                        {  //or item id 163
                            painting.CreateInstance(parent, obj);
                            return false; //unimplemented = false;
                        }
                        if ((_RES == GAME_UW2) && (obj.classindex == 7))
                        {  //or item id 359
                            bed.CreateInstance(parent, obj);
                            return false; //unimplemented = false;
                        }
                        if ((_RES == GAME_UW2) && (obj.classindex == 8))
                        {  //or item id 360
                            largeblackrockgem.CreateInstance(parent, obj);
                            return false; //unimplemented = false;
                        }
                        if ((obj.classindex == 0xE) || (obj.classindex == 0xF))
                        {//tmaps
                            tmap.CreateInstance(parent, obj, a_tilemap);
                            return false;
                        }
                        break;
                    }
            }

            return true;
        }

        public static void CreateSpriteInstance(GRLoader grObjects, uwObject obj, Node3D parent)
        {
            CreateSprite(grObjects, obj.item_id, parent);
        }

        public static void CreateSprite(GRLoader grObjects, int spriteNo, Node3D parent)
        {
            var a_sprite = new MeshInstance3D(); //new Sprite3D();
            a_sprite.Mesh = new QuadMesh();
            Vector2 NewSize;
            var img = grObjects.LoadImageAt(spriteNo);
            if (img != null)
            {
                a_sprite.Mesh.SurfaceSetMaterial(0, grObjects.GetMaterial(spriteNo));
                NewSize = new Vector2(
                        ArtLoader.SpriteScale * img.GetWidth(),
                        ArtLoader.SpriteScale * img.GetHeight()
                        );
                a_sprite.Mesh.Set("size", NewSize);
                parent.AddChild(a_sprite);
                a_sprite.Position = new Vector3(0, NewSize.Y / 2, 0);
            }
        }
    }

} //end namesace