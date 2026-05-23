using System;

namespace Underworld
{

    /// <summary>
    /// Handlers (for want of a better word) seem to handle what happens when an npc/player/object collides
    /// </summary>
    public class MotionHandler
    {

        // //These likey need to be references to delegate functions... In dosvalue at array +8 is a function call that triggers during collisions
        // //public static byte[] DSEG_27B2_SpecialMotionHandling = new byte[8];
        // public static byte[] DSEG_26BA_LandNPCMotionHandler = new byte[] { 0x0, 0x0, 0x30, 0x1F, 0x10, 0x10, 0x20, 0x0 };
        // public static byte[] DSEG_26DE_SwimmingNPCMotionHandler = new byte[] { 0x10, 0, 0x28, 0x17, 0x10, 0x10, 0x20, 0 };
        // public static byte[] DSEG_26C6_FlyingNPCMotionHandler = new byte[] { 0x0, 0x10, 0x0, 0x7, 0x80, 0x0, 0x0, 0x0 };
        // public static byte[] PlayerMotionHandler_dseg_67d6_26AA = new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

        public static MotionHandler PlayerMotionHandler;
        public static MotionHandler LandNPCMotionHandler;
        public static MotionHandler FlierNPCMotionHandler;
        public static MotionHandler SwimmerNPCMotionHandler;
        public static MotionHandler ObjectMotionHandler;

        public byte[] handlerdata;

        public delegate bool MotionHandlerCallback(uwObject obj, ref int arg0, UWMotionParamArray motionparams);

        public MotionHandlerCallback HandlerFunction;

        static MotionHandler()
        {
            PlayerMotionHandler = new(_handlerdata: new byte[] { 0x0, 0x0, 0x0, 0x11, 0x0, 0x0, 0x0, 0x0 }, _motionhandlerfunction: PlayerCallBackFunction);
            LandNPCMotionHandler = new(_handlerdata: new byte[] { 0x0, 0x0, 0x30, 0x1F, 0x10, 0x10, 0x20, 0x0 }, _motionhandlerfunction: LandBasedCallBackFunction);
            FlierNPCMotionHandler = new(_handlerdata: new byte[] { 0x0, 0x10, 0x0, 0x7, 0x80, 0x0, 0x0, 0x0 }, _motionhandlerfunction: FlierCallBackFunction);
            SwimmerNPCMotionHandler = new(_handlerdata: new byte[] { 0x10, 0, 0x28, 0x17, 0x10, 0x10, 0x20, 0 }, _motionhandlerfunction: SwimmerCallBackFunction);
            ObjectMotionHandler = new(new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 }, _motionhandlerfunction: ObjectCallBackFunction);
        }

        public MotionHandler(byte[] _handlerdata, MotionHandlerCallback _motionhandlerfunction)
        {
            handlerdata = _handlerdata;
            HandlerFunction = _motionhandlerfunction;
        }

        /// <summary>
        /// Flier call back function.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="arg0"></param>
        /// <param name="motionparams"></param>
        /// <returns></returns>
        static bool FlierCallBackFunction(uwObject obj, ref int arg0, UWMotionParamArray motionparams)
        {
            npc.IsNPCActive_dseg_67d6_2234 = true; //?
            if ((arg0 & 0x200) == 0)
            {
                if ((arg0 & 0x100)!=0)
                {
                    npc.dseg_2636 = 0x80;
                }
                if ((arg0 & 0x400) != 0)
                {
                    npc.RelatedToMotionCollision_dseg_67d6_224E = true;
                    npc.dseg_67d6_2269 = true;
                    npc.collisionObject = motion.FindCollisionObject();
                }
                if (npc.RelatedToMotionCollision_dseg_67d6_224E)
                {
                    if (npc.IsNPCActive_dseg_67d6_2234)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                npc.RelatedToMotionCollision_dseg_67d6_224E = true;
                return false;
            }
        }

        static bool SwimmerCallBackFunction(uwObject obj, ref int arg0, UWMotionParamArray motionparams)
        {
            if ((arg0 & 0x300) == 0)
            {
                if ((arg0 & 0x400) !=0)
                {
                    npc.dseg_2682 = false;
                    npc.dseg_2684 = false;
                    npc.RelatedToMotionCollision_dseg_67d6_224E = true;
                    npc.dseg_67d6_2269 = true;
                    npc.collisionObject = motion.FindCollisionObject();
                }
                if ((arg0 & 0x8) != 0)
                {
                    npc.dseg_2682 = false;
                    npc.dseg_2684 = false;
                    npc.RelatedToMotionCollision_dseg_67d6_224E = true;
                }
                if (npc.RelatedToMotionCollision_dseg_67d6_224E)
                {
                    if (npc.IsNPCActive_dseg_67d6_2234)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                npc.dseg_2682 = false;
                npc.dseg_2684 = false;
                npc.RelatedToMotionCollision_dseg_67d6_224E = true;
                return false;
            }
        }

        /// <summary>
        /// Players collision callback. Stop some motion.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="arg0"></param>
        /// <param name="motionparams"></param>
        /// <returns></returns>
        static bool PlayerCallBackFunction(uwObject obj, ref int arg0, UWMotionParamArray motionparams)
        {
            if ((arg0 & 0x1000)== 0)
            {
                return false;
            }
            if (motion.playerMotionParams.unk_a_pitch != 0)
            {
                return false;
            } 
            if ((motion.playerMotionParams.momentum_14 * 0xA) >= (motion.PlayerActualForwardSpeed_1_dseg_67d6_22A6 * 3))
            {
                return false;
            }
            if ((motion.PreviousTileState_dseg_67d6_22B4 & 0xA) !=0)
            {
                return false;
            }
            motion.playerMotionParams.unk_8_y = 0;
            motion.playerMotionParams.unk_6_x = 0;
            return true;
        }

        /// <summary>
        /// This callback handler for regular objects will just return false.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="arg0"></param>
        /// <param name="motionparams"></param>
        /// <returns></returns>
        static bool ObjectCallBackFunction(uwObject obj, ref int arg0, UWMotionParamArray motionparams)
        {
            return false;
        }


        /// <summary>
        /// This will run when land npcs collide.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="arg0"></param>
        /// <param name="motionparams"></param>
        /// <returns></returns>
        static bool LandBasedCallBackFunction(uwObject obj, ref int arg0, UWMotionParamArray motionparams)
        {
            if ((arg0 & 0x1000) == 0)
            {
                //seg006_1413_AFB:  
                if ((arg0 & 0x10) != 0)
                {
                    if ((arg0 & 0xF8) != 0x10)
                    {
                        //seg006_1413_B86:
                        if (obj.UnkBit_0X15_Bit7 == 0)
                        {
                            npc.RelatedToMotionCollision_dseg_67d6_224E = true;
                            UWMotionParamArray.dseg_67d6_260C = false;
                            UWMotionParamArray.dseg_67d6_260A = false;
                            return true;
                        }
                    }
                    else
                    {
                        //seg006_1413_B0E: 
                        //likely land based NPC has landed in water.
                        npc.RelatedToMotionCollision_dseg_67d6_224E = true;
                        npc.IsNPCActive_dseg_67d6_2234 = false;
                        var tile = UWTileMap.current_tilemap.Tiles[motionparams.x_0 >> 8, motionparams.y_2 >> 8];
                        //spawn a splash
                        animo.SpawnAnimoInTile(subclassindex: 6, xpos: 3, ypos: 3, zpos: (short)(tile.floorHeight << 3), tileX: tile.tileX, tileY: tile.tileY);
                        obj.npc_animation = npc.ANIMATION_DEATH;
                        npc.GetCritterAnimationGlobalsForCurrObj(obj);
                        obj.AnimationFrame = (byte)npc.MaxAnimFrame;
                        obj.Projectile_Speed = 1;
                        return true;
                    }
                }
                //seg006_1413_BAC
                if (
                    ((arg0 & 0x800) != 0)
                    &&
                    ((UWMotionParamArray.LikelyNPCTileStates_222C & 0x800) == 0)
                )
                {
                    //seg006_1413:0BBA
                    if (obj.UnkBit_0X15_Bit7 == 0)
                    {
                        //seg006_1413_BD3
                        npc.RelatedToMotionCollision_dseg_67d6_224E = true;
                        UWMotionParamArray.dseg_67d6_260C = false;
                        UWMotionParamArray.dseg_67d6_260A = false;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (
                     ((arg0 & 0x20) != 0)
                     &&
                      ((UWMotionParamArray.LikelyNPCTileStates_222C & 0x20) == 0)
                    )
                    {
                        if (obj.UnkBit_0X15_Bit7 == 0)
                        {
                            npc.RelatedToMotionCollision_dseg_67d6_224E = true;
                            UWMotionParamArray.dseg_67d6_260C = false;
                            UWMotionParamArray.dseg_67d6_260A = false;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if ((arg0 & 0x300) == 0)
                        {
                            if ((arg0 & 0x400) != 0)
                            {
                                var DoorCollision = motion.FindClosedDoorCollision(ref UWMotionParamArray.DoorX_222E, ref UWMotionParamArray.DoorY_222F);
                                if (DoorCollision == null)
                                {
                                    npc.RelatedToMotionCollision_dseg_67d6_224E = true;
                                    npc.dseg_67d6_2269 = true;
                                    npc.collisionObject = motion.FindCollisionObject();
                                }
                                else
                                {
                                    npc.collisionObject = DoorCollision;
                                    npc.RelatedToColliding_dseg_67d6_226F = true;
                                    npc.RelatedToMotionCollision_dseg_67d6_224E = true;
                                    npc.dseg_67d6_2269 = true;
                                }
                            }
                            //seg006_1413_C7E
                            return (npc.IsNPCActive_dseg_67d6_2234 && npc.RelatedToMotionCollision_dseg_67d6_224E);
                        }
                        else
                        {
                            npc.RelatedToMotionCollision_dseg_67d6_224E = true;
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (UWMotionParamArray.dseg_67d6_2614 == 0)
                {
                    UWMotionParamArray.dseg_67d6_2614 = -4;
                }
                obj.Projectile_Speed = 1;
                npc.RelatedToMotionCollision_dseg_67d6_224E = true;
                npc.IsNPCActive_dseg_67d6_2234 = false;
                return false;
            }
        }

        //helpers
        public short table01
        {
            get
            {
                return (short)DataLoader.getAt(handlerdata, 0, 16);
            }
            set
            {
                DataLoader.setAt(handlerdata, 0, 16, value);
            }
        }

        public short table23
        {
            get
            {
                return (short)DataLoader.getAt(handlerdata, 2, 16);
            }
        }

        /// <summary>
        /// Magic projectile + 4
        /// </summary>
        public short table45
        {
            get
            {
                return (short)DataLoader.getAt(handlerdata, 4, 16);
            }
        }

    }//end class
}//end namespace