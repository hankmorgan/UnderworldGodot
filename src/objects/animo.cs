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
        // public static GRLoader grAnimoxfer;
        // public static GRLoader grAnimoinfo;

        /// <summary>
        /// Mesh this sprite is drawn on
        /// </summary>
        public uwMeshInstance3D sprite;
        // public uwMeshInstance3D spritexfer;
        // public uwMeshInstance3D spriteinfo;

        /// <summary>
        /// The material for rendering this unique npc
        /// </summary>
        public ShaderMaterial material;
        // public ShaderMaterial materialxfer;
        // public ShaderMaterial materialinfo;

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
               
            var _animo = new animo(obj);

            uwMeshInstance3D a_sprite;
            Vector2 NewSize;
            _animo.material = grAnimo.GetMaterial(obj.owner);
            a_sprite = CreateAnimoSprite(obj: obj, name: name, NewSize: out NewSize, gr: grAnimo, material: _animo.material);            
            _animo.sprite = a_sprite;
            parent.AddChild(a_sprite);
            a_sprite.Position = new Vector3(0, NewSize.Y / 2 + 0f, 0);
            _animo.material.SetShaderParameter("objectindex_lowerbytes", obj.index & 0xFF);
            _animo.material.SetShaderParameter("objectindex_upperbytes", (obj.index>>8) & 0xFF);
            // //a_sprite.CreateConvexCollision();

            // _animo.materialxfer = grAnimoxfer.GetMaterial(obj.owner);
            // a_sprite = CreateAnimoSprite(obj: obj, name: name, NewSize: out NewSize, gr: grAnimoxfer, Layer: main.LayerXFER, material: _animo.materialxfer);
            // _animo.spritexfer = a_sprite;
            // parent.AddChild(a_sprite);
            // a_sprite.Position = new Vector3(0, NewSize.Y / 2 + 0f, 0);
            // //a_sprite.CreateConvexCollision(); no collision on xfer la

            // _animo.materialinfo = grAnimoinfo.GetMaterial(obj.owner);
            // a_sprite = CreateAnimoSprite(obj: obj, name: name, NewSize: out NewSize, gr: grAnimoxfer, Layer: main.LayerObjectInfo, material: _animo.materialinfo);
            // _animo.spriteinfo = a_sprite;
            // parent.AddChild(a_sprite);
            // a_sprite.Position = new Vector3(0, NewSize.Y / 2 + 0f, 0);
            // _animo.materialinfo.SetShaderParameter("objectindex_lowerbytes", obj.index & 0xFF);
            // _animo.materialinfo.SetShaderParameter("objectindex_upperbytes", (obj.index>>8) & 0xFF);

            return _animo;
        }

        private static uwMeshInstance3D CreateAnimoSprite(uwObject obj, string name, out Vector2 NewSize, GRLoader gr, ShaderMaterial material)
        {
            var img = gr.LoadImageAt(obj.owner);
            var a_sprite = new uwMeshInstance3D();
            a_sprite.Name = name;
            a_sprite.Mesh = new QuadMesh();            
            a_sprite.Mesh.SurfaceSetMaterial(0, material);
            NewSize = new Vector2(
                ArtLoader.SpriteScale * img.GetWidth(),
                ArtLoader.SpriteScale * img.GetHeight()
            );
            a_sprite.Mesh.Set("size", NewSize);
            a_sprite.Layers = main.LayerGeo | main.LayerObjectInfo | main.LayerXFER;
            return a_sprite;
        }


        static animo()
        {
            textureshader = (Shader)ResourceLoader.Load("res://resources/shaders/uwsprite_allred.gdshader");
            grAnimo = new GRLoader(GRLoader.ANIMO_GR, GRLoader.GRShaderMode.BillboardSpriteShader);
            grAnimo.UseRedChannel = true;
            grAnimo.XFER = ArtLoader.XferChannnelMode.AllColours;
            
            // grAnimoxfer = new GRLoader(GRLoader.ANIMO_GR, GRLoader.GRShaderMode.BillboardSpriteShader);
            // grAnimoxfer.UseRedChannel = true;
            // grAnimoxfer.XFER = ArtLoader.XferChannnelMode.XFEROnly;

            // grAnimoinfo = new GRLoader(GRLoader.ANIMO_GR, GRLoader.GRShaderMode.BillboardSpriteShader);
            // grAnimoinfo.UseRedChannel = true;
            // grAnimoinfo.XFER = ArtLoader.XferChannnelMode.AllColours;
        }

        public animo(uwObject _uwobject)
        {
            uwobject = _uwobject;
            uwobject.instance = this;
        }

        public void ApplyAnimoSprite()
        {
            material.SetShaderParameter("texture_albedo", (Texture)grAnimo.LoadImageAt(uwobject.owner));
            // materialxfer.SetShaderParameter("texture_albedo", (Texture)grAnimoxfer.LoadImageAt(uwobject.owner));
            // materialinfo.SetShaderParameter("texture_albedo", (Texture)grAnimoinfo.LoadImageAt(uwobject.owner));
            //sprite.Mesh.SurfaceSetMaterial(surfIdx: 0, material: grAnimo.GetMaterial(uwobject.owner));
            //spritexfer.Mesh.SurfaceSetMaterial(surfIdx: 0, material: grAnimoxfer.GetMaterial(uwobject.owner));
            //spritexfer.Mesh.SurfaceSetMaterial(surfIdx: 0, material: grAnimoinfo.GetMaterial(uwobject.owner));
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
                        //Debug.Print($"Free animo slot {a.index}");
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
        public static bool CreateAnimoLink(uwObject obj, int Duration, bool DrawSprite = true)
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
                //AnimationOverlay.NoOfAnimationOverlays++;
                if (DrawSprite)
                {
                    RefreshAnimo((animo)obj.instance);
                }                
                return true;
            }
            else
            {
                return false;
            }
        }

        // public static void SpawnAnimoAtPoint(int subclassindex, Godot.Vector3 point, bool randomZpos = true)
        // {
        //     Debug.Print("DEPRECIATED SpawnAnimoAtPoint");
        //     short xpos, ypos, zpos;
        //     int tileX, tileY;
        //     PointToXYZ(point, out xpos, out ypos, out zpos, out tileX, out tileY);
        //     SpawnAnimoInTile(subclassindex, xpos, ypos, zpos, tileX, tileY);
        // }

        /// <summary>
        /// Converts a Godot vector3 to tileX/Y and xyzpos values
        /// </summary>
        /// <param name="point"></param>
        /// <param name="xpos"></param>
        /// <param name="ypos"></param>
        /// <param name="zpos"></param>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        public static void PointToXYZ(Vector3 point, out short xpos, out short ypos, out short zpos, out int tileX, out int tileY)
        {
            xpos = uwObject.FloatXYToXYPos(-point.X);
            ypos = uwObject.FloatXYToXYPos(point.Z);
            zpos = uwObject.FloatZToZPos(point.Y);
            tileX = -(int)(point.X / 1.2f);
            tileY = (int)(point.Z / 1.2f);
        }


        /// <summary>
        /// Creates an animo at the target object (eg blood spatters)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="subclassindex"></param>
        /// <param name="si_zpos"></param>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        /// <returns></returns>
        public static bool SpawnAnimoAtTarget(uwObject target, int subclassindex, int si_zpos, int tileX, int tileY)
        {
            var animoObject = SpawnAnimoInTile(subclassindex: subclassindex, xpos: 3, ypos: 3, zpos: 0, tileX: tileX, tileY: tileY);
            if (animoObject != null)
            {
                if (target != null)
                {
                    animoObject.xpos = target.xpos;
                    animoObject.ypos = target.ypos;
                    if (si_zpos >= 0)
                    {
                        var Height = commonObjDat.height(target.item_id) >> 3;
                        if (Height == 0)
                        {
                            Height = 1;
                        }
                        animoObject.zpos = (short)(target.zpos + (Height * si_zpos));
                    }
                    else
                    {
                        animoObject.zpos = (short)-si_zpos;
                    }  
                    objectInstance.Reposition(animoObject);
                    return true;
                }
                else
                {
                    return false;//target is null
                }
            }
            else
            {
                return false;
            }
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
        public static uwObject SpawnAnimoInTile(int subclassindex, short xpos, short ypos, short zpos, int tileX, int tileY)
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
                return newObject;
            }
            return null;
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

            for (int i = 0; i < di_rng; i++)
            {
                var classIndex = BaseObject.classindex + Rng.r.Next(2);
                var rndX = Math.Min(Math.Max(0, BaseObject.xpos + Rng.r.Next(5) - 2), 7);
                var rndY = Math.Min(Math.Max(0, BaseObject.ypos + Rng.r.Next(5) - 2), 7);
                var rndZ = Math.Min(Math.Max(0, BaseObject.zpos - 8 + Rng.r.Next(15)), 127);
                animo.SpawnAnimoInTile(classIndex, (short)rndX, (short)rndY, (short)rndZ, tileX, tileY);
            }
        }

    }//end class
}//end namespace