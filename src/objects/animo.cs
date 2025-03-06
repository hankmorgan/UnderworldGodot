using System;
using System.Diagnostics;
using Godot;
namespace Underworld
{

    public class animo : objectInstance
    {

        /// <summary>
        /// global shader for npcs.
        /// </summary>
        public static Shader textureshader;

        public static GRLoader grAnimo;

        /// <summary>
        /// Mesh this sprite is drawn on
        /// </summary>
        public uwMeshInstance3D sprite;

        /// <summary>
        /// The material for rendering this unique npc
        /// </summary>
        public ShaderMaterial material;


        public int startFrame
        {
            get
            {
                return animationObjectDat.startFrame(uwobject.item_id);
            }
        }

        public int endFrame
        {
            get
            {
                return animationObjectDat.endFrame(uwobject.item_id);
            }
        }

        public static animo CreateInstance(Node3D parent, uwObject obj, string name)
        {
            if (obj.invis == 1)
            {
                return null;
            }
            var a = new animo(obj);
            //a.ApplyAnimoSprite();
            var a_sprite = new uwMeshInstance3D(); //new Sprite3D();
            a_sprite.Name = name;
            a_sprite.Mesh = new QuadMesh();
            a_sprite.Mesh.SurfaceSetMaterial(0, grAnimo.GetMaterial(obj.owner));

            var img = grAnimo.LoadImageAt(obj.owner);
            var NewSize = new Vector2(
                ArtLoader.SpriteScale * img.GetWidth(),
                ArtLoader.SpriteScale * img.GetHeight()
            );
            a_sprite.Mesh.Set("size", NewSize);
            a.sprite = a_sprite;
            parent.AddChild(a_sprite);
            a_sprite.Position = new Vector3(0, NewSize.Y / 2 + 0f, 0);
            a_sprite.CreateConvexCollision();
            return a;
        }

        static animo()
        {
            textureshader = (Shader)ResourceLoader.Load("res://resources/shaders/uwsprite.gdshader");
            grAnimo = new GRLoader(GRLoader.ANIMO_GR, GRLoader.GRShaderMode.BillboardSpriteShader);
            grAnimo.UseRedChannel = true;
        }

        public animo(uwObject _uwobject)
        {
            uwobject = _uwobject;
            uwobject.instance = this;
        }

        public void ApplyAnimoSprite()
        {
            sprite.Mesh.SurfaceSetMaterial(0, grAnimo.GetMaterial(uwobject.owner));
            // if (material == null)
            // {//create the initial material
            //     var newmaterial = new ShaderMaterial();
            //     newmaterial.Shader = textureshader;
            //     newmaterial.SetShaderParameter("albedo", new Color(1, 1, 1, 1));
            //     newmaterial.SetShaderParameter("uv1_scale", new Vector3(1, 1, 1));
            //     newmaterial.SetShaderParameter("uv2_scale", new Vector3(1, 1, 1));
            //     newmaterial.SetShaderParameter("UseAlpha", true);
            //     if (uwobject.item_id==457)
            //         {//make fountain water appear closer than the fountain base
            //         newmaterial.SetShaderParameter("camera_offset", 0.01);
            //         }   
            //     else
            //     {
            //         newmaterial.SetShaderParameter("camera_offset", 0);
            //     }
            //     material = newmaterial;
            //     sprite.Mesh.SurfaceSetMaterial(0, material);
            // }
            // //sprite.Mesh.SurfaceSetMaterial(0, grAnimo.GetMaterial(uwobject.owner));
            // material.SetShaderParameter("texture_albedo", (Texture)grAnimo.LoadImageAt(uwobject.owner,true));
        }

        public static void AdvanceAnimo(animo obj)
        {
            if (obj != null)
            {
                obj.uwobject.owner++;
                RefreshAnimo(obj);
            }
        }


        public static void ResetAnimo(animo obj)
        {
            if (obj != null)
            {
                obj.uwobject.owner = (short)obj.startFrame;
                RefreshAnimo(obj);
            }
        }

        public static void RefreshAnimo(animo obj)
        {
            if (obj != null)
            {
                obj.ApplyAnimoSprite();
            }
        }


        /// <summary>
        /// Finds the first available animo slot in the overlays list
        /// </summary>
        /// <returns>the index of the animo if found. Othewise -1</returns>
        public static int GetFreeAnimoSlot()
        {
            foreach (var a in UWTileMap.current_tilemap.Overlays)
            {
                if (a != null)
                {
                    if (a.Duration == 0)
                    {
                        Debug.Print($"Free animo slot {a.index}");
                        return a.index;
                    }
                }
            }
            Debug.Print($"No Free Animo slot");
            return -1; //no animo found
        }



        /// <summary>
        /// Creates a linked animo.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="Duration"></param>
        /// <returns></returns>
        public static bool CreateAnimoLink(uwObject obj, int Duration)
        {
            //Add animation overlay entry
            var animoindex = GetFreeAnimoSlot();
            obj.owner = (short)animationObjectDat.startFrame(obj.item_id);
            if (animoindex != -1)
            {
                var anim = UWTileMap.current_tilemap.Overlays[animoindex];
                anim.link = obj.index;
                anim.tileX = obj.tileX;
                anim.tileY = obj.tileY;
                anim.Duration = Duration;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void SpawnAnimoAtPoint(int subclassindex, Godot.Vector3 point, bool randomZpos = true)
        {
            var xpos = uwObject.FloatXYToXYPos(-point.X);
            var ypos = uwObject.FloatXYToXYPos(point.Z);
            var zpos = uwObject.FloatZToZPos(point.Y);
            var tileX = -(int)(point.X / 1.2f);
            var tileY = (int)(point.Z / 1.2f);
            SpawnAnimoInTile(subclassindex, xpos, ypos, zpos, tileX, tileY);
        }

        /// <summary>
        /// Creates an animo object and links it to the running animation overlays list
        /// </summary>
        /// <param name="subclassindex"></param>
        /// <param name="xpos"></param>
        /// <param name="ypos"></param>
        /// <param name="zpos"></param>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        public static void SpawnAnimoInTile(int subclassindex, short xpos, short ypos, short zpos, int tileX, int tileY)
        {
            if (animo.GetFreeAnimoSlot() != -1)
            {
                var itemid = 448 + subclassindex;
                var newObject = ObjectCreator.spawnObjectInTile(
                                itemid: itemid,
                                tileX: tileX,
                                tileY: tileY,
                                xpos: xpos,
                                ypos: ypos,
                                zpos: zpos,
                                WhichList: ObjectFreeLists.ObjectListType.StaticList);
                if (newObject != null)
                {
                    var duration = animationObjectDat.endFrame(itemid) - animationObjectDat.startFrame(itemid);
                    CreateAnimoLink(newObject, duration);
                }
            }
        }

        /// <summary>
        /// Spawns copies of the animo at random locations in the tile. Used primarily to simulate explosion effects.
        /// </summary>
        /// <param name="BaseObject"></param>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        public static void SpawnAnimoCopies(uwObject BaseObject, int tileX, int tileY)
        {
            var di_rng = 2 + Rng.r.Next(3);

            for (int i = 0; i<di_rng; i++)
            {
                var classIndex = BaseObject.classindex + Rng.r.Next(2);
                var rndX = Math.Min(Math.Max(0, BaseObject.xpos + Rng.r.Next(5) - 2), 7);
                var rndY = Math.Min(Math.Max(0, BaseObject.ypos + Rng.r.Next(5) - 2), 7);
                var rndZ = Math.Min(Math.Max(0,BaseObject.zpos - 8 + Rng.r.Next(15)), 127);
                animo.SpawnAnimoInTile(classIndex, (short)rndX, (short)rndY, (short)rndZ, tileX, tileY );
            }
        }

    }//end class
}//end namespace