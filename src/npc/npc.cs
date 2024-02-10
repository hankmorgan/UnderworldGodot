using System;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    public partial class npc : objectInstance
    {

        /// <summary>
        /// global shader for npcs.
        /// </summary>
        public static Shader textureshader;

        /// <summary>
        /// Mesh this sprite is drawn on
        /// </summary>
        public MeshInstance3D sprite;

        /// <summary>
        /// The material for rendering this unique npc
        /// </summary>
        public ShaderMaterial material;



        /// <summary>
        /// Mesh this sprite is drawn on
        /// </summary>
        //public MeshInstance3D sprite;

        /// <summary>
        /// The material for rendering this unique npc
        /// </summary>
        //public ShaderMaterial material;

        public enum npc_goals
        {
            npc_goal_stand_still_0 = 0,
            npc_goal_goto_1 = 1,
            npc_goal_wander_2 = 2,
            npc_goal_follow = 3,
            npc_goal_wander_4 = 4, //possibly this should be another standstill goal
            npc_goal_attack_5 = 5,
            npc_goal_attack_6 = 6,  //goal appears to be attack at a distance using ranged weapons, but also fear??
            npc_goal_stand_still_7 = 7, //same hehaviour as 0
            npc_goal_wander_8 = 8, //8 is the goal the npc gets when charmed, castle npcs have this too.
            npc_goal_attack_9 = 9, //goal appears to also be attack at a distance, possibly using magic attacks
            npc_goal_want_to_talk = 10,
            npc_goal_stand_still_11 = 11, //This goal is only seen in ethereal void creatures. 0xB
            npc_goal_stand_still_12 = 12,
            npc_goal_unk13 = 13,
            npc_goal_unk14 = 14,
            npc_goal_petrified = 15
        };


        public npc(uwObject _uwobject)
        {
            uwobject = _uwobject;
            try
            {
                SetAnimSprite(uwobject.npc_animation, uwobject.AnimationFrame, uwobject.heading);//TODO this value has to be relative to the player heading
            }
            catch (Exception ex)
            {
                Debug.Print($"{ex.ToString()}");
            }
        }

        /// <summary>
        /// Creates a rendered version of this object in the gameworld
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static npc CreateInstance(Node3D parent, uwObject obj, string name)
        {
            var n = new npc(obj);
            var a_sprite = new MeshInstance3D(); //new Sprite3D();
            a_sprite.Name = name;
            a_sprite.Mesh = new QuadMesh();
            a_sprite.Mesh.SurfaceSetMaterial(0, n.material);
            a_sprite.Mesh.Set("size", n.FrameSize * 1.5f);
            n.sprite = a_sprite;
            parent.AddChild(a_sprite);
            a_sprite.Position = new Vector3(0, n.FrameSize.Y / 2 + 0.12f, 0);
            a_sprite.CreateConvexCollision();
            string animname;
            // if(_RES==GAME_UW2)
            // {
            //     animname = CritterArt.GetUW2AnimName(obj.npc_animation, obj.AnimationFrame);
            // }
            // else
            // {
            //     animname = CritterArt.GetUW1AnimName(obj.npc_animation);
            // }
            animname = CritterArt.GetAnimName(obj.npc_animation, obj.heading);
            if (ObjectCreator.printlabels)
            {
                Label3D obj_lbl = new();
                obj_lbl.Text = $"{name} {obj.item_id & 0x3F} \nAnim={obj.npc_animation} Frame={obj.AnimationFrame} {animname}\n Goal {obj.npc_goal}";
                obj_lbl.Font = uimanager.instance.Font4X5P;
                obj_lbl.FontSize = 16;
                obj_lbl.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
                obj_lbl.Position = new Vector3(0f, 0.4f, 0f);
                parent.AddChild(obj_lbl);
            }
            obj.instance = n;
            return n;
        }

        static npc()
        {
            textureshader = (Shader)ResourceLoader.Load("res://resources/shaders/uwnpc.gdshader");
        }

        public void SetAnimSprite(int animationNo, short frameNo, int relativeHeading)
        {
            //if (this.uwobject.item_id >= 127) { return; }
            if (uwobject.npc_animation >= 8)
            {
                uwobject.npc_animation = 0;
            }
            string animname = CritterArt.GetAnimName(animationNo, relativeHeading); // "idle_front";
            //var crit = CritLoader.GetCritter(this.uwobject.item_id & 0x3F);
            var crit = CritterArt.GetCritter(this.uwobject.item_id & 0x3F);
            if (crit.Animations.ContainsKey(animname))
            {
                uwobject.npc_animation = ApplyCritterAnimation(animationNo, frameNo, animname, crit);
            }
            else
            {
                uwobject.npc_animation = 0; //default animation to zero;
                Debug.Print($"{animname} ({animationNo}) was not found for {this.uwobject.item_id & 0x3F}");
                uwobject.npc_animation = ApplyCritterAnimation(animationNo, frameNo, CritterArt.GetAnimName(0, 0), crit);
            }
        }

        private short ApplyCritterAnimation(int animationNo, short frameNo, string animname, CritterArt crit)
        {
            var anim = crit.Animations[animname];
            if (material == null)
            {//create the initial material
                var newmaterial = new ShaderMaterial();
                newmaterial.Shader = textureshader;
                //newmaterial.SetShaderParameter("texture_albedo", (Texture)LoadImageAt(textureno,true));
                newmaterial.SetShaderParameter("albedo", new Color(1, 1, 1, 1));
                newmaterial.SetShaderParameter("uv1_scale", new Vector3(1, 1, 1));
                newmaterial.SetShaderParameter("uv2_scale", new Vector3(1, 1, 1));
                newmaterial.SetShaderParameter("UseAlpha", true);
                material = newmaterial;
            }
            //assign the params to the shader
            //critAnim.animSprites[critAnim.animIndices[AnimationIndex, AnimationPos++]] 
            if (frameNo >= 8) { frameNo = 0; }
            if (anim.animIndices[frameNo] == -1)
            {
                frameNo = 0;
            }

            if (anim.animIndices[frameNo] != -1)
            {
                var texture = crit.animSprites[anim.animIndices[frameNo]];
                FrameSize = new Vector2(
                    ArtLoader.SpriteScale * texture.GetWidth(),

                    ArtLoader.SpriteScale * texture.GetHeight()
                    );
                material.SetShaderParameter("texture_albedo", (Texture)texture);
                //sprite.Mesh.Set("size",FrameSize*1.5f);//TODO fix so this does not call a null crash and sprite mesh keeps size
                return frameNo;
            }
            else
            {
                Debug.Print($"invalid animation {animationNo} {frameNo} for {this.uwobject.item_id}");
            }
            return 0;
        }


        /// <summary>
        /// Interate through the npcs and up their their animations
        /// </summary>
        public static void UpdateNPCs()
        {
            if (ObjectCreator.npcs != null)
            {
                foreach (var n in ObjectCreator.npcs)
                {
                    if (n.uwobject.tileY != 99)
                    {
                        n.uwobject.AnimationFrame++;
                        n.SetAnimSprite(n.uwobject.npc_animation, n.uwobject.AnimationFrame, n.uwobject.heading);
                    }
                }
            }
        }

    }//end class

}//end namespace