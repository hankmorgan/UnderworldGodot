using System;
using System.Diagnostics;
using System.Linq;
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
        public uwMeshInstance3D sprite;

        /// <summary>
        /// The material for rendering this unique npc
        /// </summary>
        public ShaderMaterial material;



        /// <summary>
        /// Possible AI goals
        /// </summary>
        public enum npc_goals : byte
        {
            npc_goal_stand_still_0 = 0,
            npc_goal_goto_1 = 1,
            npc_goal_wander_2 = 2,
            npc_goal_follow = 3,
            npc_goal_wander_4 = 4, //possibly this should be another standstill goal
            npc_goal_attack_5 = 5,
            npc_goal_fear_6 = 6,  //goal appears to be attack at a distance using ranged weapons, but also fear??
            npc_goal_stand_still_7 = 7, //same hehaviour as 0
            npc_goal_charmed_8 = 8, //8 is the goal the npc gets when charmed, castle npcs have this too.
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
            var a_sprite = new uwMeshInstance3D(); //new Sprite3D();
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
            if (uwobject.AnimationFrame >= 8)
            {
                uwobject.AnimationFrame = 0;
            }
            string animname = CritterArt.GetAnimName(animationNo, relativeHeading); // "idle_front";
            //var crit = CritLoader.GetCritter(this.uwobject.item_id & 0x3F);
            var crit = CritterArt.GetCritter(this.uwobject.item_id & 0x3F);
            if (crit.Animations.ContainsKey(animname))
            {
                uwobject.AnimationFrame = (byte)ApplyCritterAnimation(animationNo, frameNo, animname, crit);

            }
            else
            {
                uwobject.npc_animation = 0; //default animation to zero;
                Debug.Print($"{animname} ({animationNo}) was not found for {this.uwobject.item_id & 0x3F}");
                uwobject.AnimationFrame = (byte)ApplyCritterAnimation(animationNo, frameNo, CritterArt.GetAnimName(0, 0), crit);
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
                    ArtLoader.NPCSpriteScale * texture.GetWidth(),

                    ArtLoader.NPCSpriteScale * texture.GetHeight()
                    );
                material.SetShaderParameter("texture_albedo", (Texture)texture);
                if (sprite!=null)
                {
                    sprite.Mesh.Set("size", FrameSize * 1.5f);
                }                
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
                    n.uwobject.AccumulatedDamage = 0;
                    if (n.uwobject.tileY != 99)
                    {
                        if (n.uwnode!=null)
                        {
                            short CalcedFacing = CalculateFacingAngleToNPC(n);
                            n.uwobject.AnimationFrame++;
                            n.SetAnimSprite(n.uwobject.npc_animation, n.uwobject.AnimationFrame, CalcedFacing); //n.uwobject.heading);
                        }                        
                    }
                }
            }
        }

        private static short CalculateFacingAngleToNPC(npc n)
        {
            var direction = -main.gamecam.Position + n.uwnode.Position;
            var angle = Mathf.RadToDeg(Mathf.Atan2(direction.X, direction.Z));
            var facingIndex = facing(angle);
            var CalcedFacing = (short)(facingIndex + n.uwobject.heading);
            if (CalcedFacing >= 8)//Make sure it wraps around correcly between 0 and 7 ->The compass headings.
            {
                CalcedFacing = (short)(CalcedFacing - 8);
            }
            if (CalcedFacing <= -8)
            {
                CalcedFacing = (short)(CalcedFacing + 8);
            }
            if (CalcedFacing < 0)
            {
                CalcedFacing = (short)(8 + CalcedFacing);
            }
            else if (CalcedFacing > 7)
            {
                CalcedFacing = (short)(8 - CalcedFacing);
            }

            return CalcedFacing;
        }



        /// <summary>
        /// Changes the goal and gtarg for the npc
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="goal"></param>
        /// <param name="target"></param>
        public static void SetGoalAndGtarg(uwObject critter, int goal, int target)
        {
            if (critter.npc_goal == 4)
            {//back up goal for some unknown reason
                critter.npc_level = critter.npc_goal;
            }
            critter.npc_goal = (byte)goal;
            critter.npc_gtarg = (byte)target;
        }

        public static bool LookAt(uwObject critter)
        {
            if ((critter.npc_whoami >= 240) && (critter.npc_whoami != 248))//Ethereal void creatures 
            {
                EtheralVoidNPCDescription(critter);
            }
            else
            {
                if ((_RES != GAME_UW2) && (critter.npc_whoami == 248))
                {//slasher of veils
                    var name = critter.a_name;
                    uimanager.AddToMessageScroll($"You see {name}");
                }
                else
                {
                    RegularNPCDescription(critter);
                }
            }
            return true;
        }

        /// <summary>
        /// Describes Mr Jaws and his friends
        /// </summary>
        /// <param name="critter"></param>
        private static void EtheralVoidNPCDescription(uwObject critter)
        {
            if (_RES == GAME_UW2)
            {
                var id = critter.npc_animation + 277;
                var name = GameStrings.GetString(1, id);
                uimanager.AddToMessageScroll($"You see {name}");
            }
            else
            {
                uimanager.AddToMessageScroll($"You see {critter.a_name.Replace("_", " ")}");
            }
        }

        /// <summary>
        /// Describes an NPC in terms of their mood, race and name
        /// </summary>
        /// <param name="critter"></param>
        private static void RegularNPCDescription(uwObject critter)
        {
            //TODO: A worried spectre named Warren.
            var name = critter.a_name;
            var lowercasename = char.IsLower(name.First<char>());//check if name is lower case. if so do not print it
            string npcrace = GameStrings.GetObjectNounUW(critter.item_id);
            var mood = GameStrings.GetString(5, 96 + critter.npc_attitude);

            var article = "a";
            switch (mood.ToUpper().Substring(0, 1))
            {
                case "A":
                case "E":
                case "I":
                case "O":
                case "U":
                    article = "an"; break;

            }
            if ((critter.npc_whoami != 0))
            {
                if (lowercasename)
                {//print the whoami as the race only
                    uimanager.AddToMessageScroll($"You see {article} {mood} {name}");
                }
                else
                {//print race
                    uimanager.AddToMessageScroll($"You see {article} {mood} {npcrace} named {name}");
                }
            }
            else
            {
                uimanager.AddToMessageScroll($"You see {article} {mood} {npcrace}");
            }
        }  

        /// <summary>
        /// Gets a list of spells and properties the npc has.
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static bool ListNPCProperties(uwObject critter, out string propertystring)
        {
            var propertycount = 0;
            byte[] prop = new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
            propertystring = "";
            if (critterObjectDat.UNK0x2DBits1To7(critter.item_id) != 0)
            {
                int si = 0;

                for (si = 0; si < 3; si++)
                {//read in the spells
                    prop[si] = (byte)critterObjectDat.spell(critter.item_id, si);
                }
                if (critterObjectDat.generaltype(critter.item_id) == 0x17)
                {//liches?
                    if (critterObjectDat.isFlier(critter.item_id))
                    {
                        prop[si++] = 0x39;//can cast fly
                    }
                    if (critterObjectDat.dooropenskill(critter.item_id) >= 0x2D)
                    {
                        prop[si++] = 0x23;//can cast open
                    }
                    var testdam = 1;
                    var scale = uwObject.ScaleDamage(critter.item_id, ref testdam, 8);

                    if (scale == 0)
                    {
                        prop[si++] = 0x1C; //can cast flameproof
                    }

                    if (critterObjectDat.UNK0x2DBits1To7(critter.item_id) >= 0x19)
                    {
                        if (critterObjectDat.level(critter.item_id) >= 9)
                        {
                            prop[si++] = 0x3b; //can cast ironflesh
                            propertycount++;
                        }
                    }
                }

                //Make sure properties are unique
                if (prop[0] == prop[1])
                {
                    prop[1] = 0xff;
                }
                else
                {
                    if (prop[1] == prop[2])
                    {
                        prop[1] = 0xff;
                    }
                }
                if (prop[0] == prop[2])
                {
                    prop[2] = 0xff;
                }

                for (si = 0; si < 8; si++)
                {
                    if (prop[si] != 0xff)
                    {
                        propertycount++;
                    }
                }
                if (propertycount == 0)
                {
                    return false;
                }
                else
                {
                    var propertiesremaining = propertycount - 1;
                    for (si = 0; si < 8; si++)
                    {
                        //loop the properties and turn them into spellnames
                        if (prop[si] != 0xff)
                        {
                            int major = (prop[si] & 0xC0) >> 6;
                            int minor = prop[si] & 0x3F;
                            if (major == 0)
                            {
                                var spell = RunicMagic.SpellList[prop[si]];
                                if (spell.SpellMajorClass == 4)
                                {//if healing? and a golem??
                                    if (critterObjectDat.generaltype(critter.item_id) == 0xF)
                                    {
                                        major = 1; minor = 6;//bouncing???
                                    }
                                }
                            }
                            if (major <= 0)
                            {
                                minor += 0x100;
                            }
                            else
                            {
                                minor += ((major + 0xC0) << 4);
                            }

                            propertystring += GameStrings.GetString(6, minor);
                            if (propertiesremaining >= 2)
                            {
                                propertystring += ", ";
                            }
                            else
                            {
                                if ((propertiesremaining == 1) && (propertycount > 1))
                                {
                                    propertystring += GameStrings.GetString(0x274); //AND
                                }
                            }
                            propertiesremaining--;
                        }
                    }
                }
            }
            return (propertycount > 0);
        }


        /// <summary>
        /// Breaks down the angle in the the facing sector. Clockwise from 0)
        /// </summary>
        /// <param name="angle">Angle.</param>
        static short facing(float angle)
        {
            if ((angle >= -22.5) && (angle <= 22.5))
            {
                return 0;//*AnimRange;//Facing forward
            }
            else
            {
                if ((angle > 22.5) && (angle <= 67.5))
                {//Facing forward left
                    return 1;//*AnimRange;
                }
                else
                {
                    if ((angle > 67.5) && (angle <= 112.5))
                    {//facing (right)
                        return 2;//*AnimRange;
                    }
                    else
                    {
                        if ((angle > 112.5) && (angle <= 157.5))
                        {//Facing away left
                            return 3;//*AnimRange;
                        }
                        else
                        {
                            if (((angle > 157.5) && (angle <= 180.0)) || ((angle >= -180) && (angle <= -157.5)))
                            {//Facing away
                                return 4;//*AnimRange;
                            }
                            else
                            {
                                if ((angle >= -157.5) && (angle < -112.5))
                                {//Facing away right
                                    return 5;//*AnimRange;
                                }
                                else
                                {
                                    if ((angle > -112.5) && (angle < -67.5))
                                    {//Facing (left)
                                        return 6;//*AnimRange;
                                    }
                                    else
                                    {
                                        if ((angle > -67.5) && (angle < -22.5))
                                        {//Facing forward right
                                            return 7;//*AnimRange;
                                        }
                                        else
                                        {
                                            return 0;//*AnimRange;//default
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

    }//end class
}//end namespace