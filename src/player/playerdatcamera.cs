using System;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    //For management of the camera view of the player
    public partial class playerdat : Loader
    {

        /// <summary>
        /// This object is a reference to the game object that the game camera will use. 
        /// In the original game engine it is theoretically possible to attach the game camera to another mobile object apart from the player.
        /// Unfortunately there is no direct way to do it in game without memory editing
        /// Normally this value will be a reference to the playerobject. When a do_trap_camera or roaming sight is active this reference will change to null.
        /// </summary>
        public static uwObject CameraReference;

        public static bool MoongateSucking = false;
        public static ushort MoonGateCameraYaw = 0;
        public static byte StepsTakenToMoongate = 0;
        public static byte MoongateSuckX = 0x20;
        public static byte MoongateSuckY = 0x20;
        public static short MoongateDist = 0;

        /// <summary>
        /// Camera positions for do trap camera and roaming sight.
        /// </summary>
        public static short DoCameraX = 0;
        public static short DoCameraY = 0;
        public static short DoCameraZ = 0;
        public static short DoCameraH = 0;
        public static short DoCameraPitch = 0;
        public static short DoCameraRoll = 0;

        public static short CameraTileX; public static short CameraTileY;

        /// <summary>
        /// Positions the player game camera based on x/y/z pos and current tileX/Y. 
        /// 
        /// For future expansion to match the original game this will need to be able support attaching the camera to a game object. 
        /// This is most often used in do_trap_camera but theoreticial the camera can be attached to any object by changing a global pointer 
        /// to point away from the player object and aim at an object instead.
        /// </summary>
        public static void PositionPlayerCamera()
        {
            if (CameraReference != null)
            {
                if (!MoongateSucking)
                {
                    if (CameraReference == playerObject)
                    {
                        // var x = motion.playerMotionParams.x_0;
                        // var y = motion.playerMotionParams.y_2;
                        // var z = motion.playerMotionParams.z_4 + 0xA4;//offset for player head. This is lower than player height.
                        //set up yaw, pitch and other motion values.
                        // var yaw = (float)motion.PlayerCameraYaw_dseg_8294;
                        // var roll = (float)motion.PlayerCameraRoll_dseg_67d6_33D8;
                        // var pitch = (float)motion.PlayerCameraPitch_dseg_67d6_33D6;
                        //bool applyBob = motion.CameraIsBobbing_dseg_67d6_33c6;
                        PositionCamera(
                            x: motion.playerMotionParams.x_0,
                            y: motion.playerMotionParams.y_2,
                            z: (short)(motion.playerMotionParams.z_4 + 0xA4),
                            yaw: motion.PlayerCameraYaw_dseg_8294,
                            roll: motion.PlayerCameraRoll_dseg_67d6_33D8,
                            pitch: motion.PlayerCameraPitch_dseg_67d6_33D6,
                            applyBob: motion.CameraIsBobbing_dseg_67d6_33c6);

                    }
                    else
                    {
                        //Debug.Print("attach a camera to a non-player mobile object.");
                        var x = (CameraReference.npc_xhome << 8) + (CameraReference.xpos << 5);
                        var y = (CameraReference.npc_yhome << 8) + (CameraReference.ypos << 5);
                        var z = (CameraReference.zpos << 3) + 0xB0;
                        var yaw = (CameraReference.npc_heading << 8) + (CameraReference.heading << 0xD);
                        PositionCamera(
                            x: (short)x,
                            y: (short)y,
                            z: (short)z,
                            yaw: (short)yaw,
                            roll: 0,
                            pitch: 0,
                            applyBob: false);
                    }
                }
                else
                {
                    //Rotate and move the camera towards the XY location until player is sucked in to the moongate at UW1 endgame.
                    short x = 0; short y = 0;
                    motion.SomethingProjectileHeading_seg021_22FD_EAE(MoonGateCameraYaw, ref x, ref y);
                    x = (short)(x / 0x100);
                    y = (short)(y / 0x100);
                    var stepsremaining = 0x40 - StepsTakenToMoongate;

                    x = (short)((x * stepsremaining) / 0x40);
                    y = (short)((y * stepsremaining) / 0x40);

                    var moonX = (short)(((x * MoongateDist) / 2) + (MoongateSuckX << 8) + 0x80);
                    var moonY = (short)(((y * MoongateDist) / 2) + (MoongateSuckY << 8) + 0x80);
                    var moonZ = (short)((motion.playerMotionParams.z_4 + 0xA4) - (StepsTakenToMoongate << 1));
                    var yaw = (short)(MoonGateCameraYaw + 0x7FFF);
                    var roll = (short)(StepsTakenToMoongate << 0xB);

                    PositionCamera(
                        x: moonX, y: moonY, z: moonZ,
                        yaw: yaw, roll: roll, pitch: 0,
                        applyBob: false);
                }
            }
            else
            {
                //no camera object is defined. Used the values in doTrapCameraX/Y/Z/heading to render the camera.
                PositionCamera(
                    x: DoCameraX, y: DoCameraY, z: DoCameraZ,
                    yaw: DoCameraH, roll: DoCameraRoll, pitch: DoCameraPitch,
                    applyBob: false);
            }
        }

        private static void PositionCamera(short x, short y, short z, short yaw, short roll, short pitch, bool applyBob)
        {
            if (applyBob)
            {
                z += motion.CameraBobZAdjust_dseg_67d6_33CE; //note that over time when in water the camera will go beneath the water level due to the swimmingcounter getting larger
            }

            //Get a vector for the camera that is relative to the bounds of an underworld level map.
            Vector3 underworldVector = new(x: -(float)x / 16384f, y: (float)z / 1024f, z: (float)y / 16384f);

            //then transform it into godot positioning using a vector based on the size we are rendering the gameworld in.
            main.cameraYawGimbal.Position = underworldVector * tileMapRender.godotscale;

            if ((motion.CameraIsBobbing_dseg_67d6_33c6) && (applyBob))
            {
                yaw += motion.CameraYawModifier_dseg_67d6_33D0;
                roll += motion.CameraRollModifier_dseg_67d6_33D4;
                pitch += motion.CameraPitchModifier_dseg_67d6_33D2;
            }

            CameraTileX = (short)(x >> 8);
            CameraTileY = (short)(y >> 8);

            //Set up the Yaw gimbal             
            main.cameraYawGimbal.Rotation = Vector3.Zero;
            main.cameraYawGimbal.Rotate(Vector3.Up, (float)(Math.PI));//align to the north.
            main.cameraYawGimbal.Rotate(Vector3.Up, (float)(-((float)yaw / 32767f) * Math.PI));

            //Set up the Roll Gimbal
            main.cameraRollGimbal.Rotation = Vector3.Zero;
            main.cameraRollGimbal.Rotate(Vector3.Forward, (float)(-((float)roll / 32767f) * Math.PI));

            //Set up the pitch gimbal.
            main.cameraPitchGimbal.Rotation = Vector3.Zero;
            main.cameraPitchGimbal.Rotate(Vector3.Right, (float)(+((float)pitch / 32767f) * Math.PI));

            //Set this value to calculate npc angles
            motion.CameraYawHeadingRelated_2B52 = (short)(((1 + (yaw >> 0xD)) & 0x7) >> 1);
            motion.CameraPointer2C = (short)(yaw - motion.PlayerCardinalHeadingLookupTable[motion.CameraYawHeadingRelated_2B52]);
            if (false)
            {
                //dont't run yet due to infinite looping
                //Following functions are used to determine what is in sight and to update automap accordingly.
                VisionParams.SetRangeOfVisionParams(
                    camerax: x,
                    cameray: y,
                    camerayaw: yaw);

                VisionParams.LikelyGetViewDistance();
            }

        }


        /// <summary>
        /// Sets the camera view object and default positions for use in roaming,sight, dotrapcamera
        /// 0 = roaming sight.
        /// 1 = player camera
        /// 2 = other object.
        /// </summary>
        /// <param name="index"></param>
        public static void SetCameraViewValues(int index)
        {
            if (index <= 1)
            {
                DoCameraX = motion.playerMotionParams.x_0;
                DoCameraY = motion.playerMotionParams.y_2;
                DoCameraH = motion.PlayerCameraYaw_dseg_8294;
                if (index == 1)
                {
                    DoCameraZ = (short)(motion.playerMotionParams.z_4 + 0xA4);
                }
                else
                {
                    //roaming sight
                    DoCameraZ = 0x458;
                    DoCameraPitch = -1024;
                }
            }
            else
            {
                Debug.Print("set camera view values for other objects.");
            }
        }

    }//end class

    /// <summary>
    /// Range of Vision Variables
    /// </summary>
    public class VisionParams
    {
        static short RelatedToFov_2C60 = 0;
        static short LikelyDistanceToWallOrDarkness = -1;

        //array of data starting at dseg:2b5e
        public static short dseg_2B5E;
        public static short FovYawXLEFT = 0; //2B5F
        public static short FovYawYLEFT = 0; //2B61
        public static byte dseg_2B63;
        public static byte CameraX_2b64;//2B64

        public static short dseg_2B65;
        public static byte CameraY_2b66;//2B66

        public static TileInfo playerTileCopy_2B67; //2B67

        public static short dseg_2B6B;
        public static short dseg_2B6D;
        public static short dseg_2B6F;
        public static short FovYawXRIGHT = 0;//2b70        
        public static short FovYawYRIGHT = 0;//2b72

        public static short dseg_2B74;
        public static byte CameraX_2B75;
        public static byte dseg_2B76;
        public static byte CameraY_2B77;

        public static TileInfo playerTileCopy_2B78;
        public static byte dseg_2B7C;


        /// <summary>
        /// Port of a vanilla function which is used to calcuate what objects are inview. Implemented here to help support vanilla implementation of automapping
        /// </summary>
        public static void SetRangeOfVisionParams(short camerax, short cameray, short camerayaw)
        {
            var tileX = camerax >> 8; var tileY = cameray >> 8;
            if (!UWTileMap.ValidTile(tileX, tileY))
            {
                return;
            }
            var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
            if (tile.tileType == 0)
            {
                RelatedToFov_2C60 = 0xF;
            }
            else
            {
                RelatedToFov_2C60 = 0;

                dseg_2B5E = 0x81;
                dseg_2B63 = 0;
                dseg_2B65 = 0;

                CameraX_2b64 = (byte)camerax;
                CameraY_2b66 = (byte)cameray;

                playerTileCopy_2B67 = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                //dseg_2B6B = reference to an array;
                dseg_2B6F = 0;
                dseg_2B74 = 0;
                dseg_2B76 = 0;

                CameraX_2B75 = (byte)camerax;
                CameraY_2B77 = (byte)cameray;
                playerTileCopy_2B78 = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                //dseg_2B7C =reference to an array that is set up in Seg032_6CF

                motion.SomethingProjectileHeading_seg021_22FD_EAE(heading: (ushort)(camerayaw + 0x2040), Result_arg2: ref FovYawXRIGHT, Result_arg4: ref FovYawYRIGHT);
                motion.SomethingProjectileHeading_seg021_22FD_EAE(heading: (ushort)(camerayaw - 0x2040), Result_arg2: ref FovYawXLEFT, Result_arg4: ref FovYawYLEFT);

                FovYawXRIGHT = (short)(FovYawXRIGHT >> 4);
                FovYawXLEFT = (short)(FovYawXLEFT >> 4);
                FovYawYRIGHT = (short)(FovYawYRIGHT >> 4);
                FovYawYLEFT = (short)(FovYawYLEFT >> 4);
            }
        }

        public static void LikelyGetViewDistance()
        {
            var di = 0;
            LikelyDistanceToWallOrDarkness = -1;
            var currentshade = shade.shadesdata[playerdat.lightlevel].ShadingArray_26EF;
            var var2currentshadePtr = 0;

        seg032_1175:
            LikelyDistanceToWallOrDarkness++;
            //var var4 = RelatedToFov_2C60; //var4 appears to be a pointer to this value
        seg032_119D:

            if ((RelatedToFov_2C60 & 0xF) == 0xF)
            {
                //seg032_11B0
                di = currentshade[66 + var2currentshadePtr];

                //loop seg32_11ED
                if ((RelatedToFov_2C60 & 0xF) == 0xF)
                {
                seg032_120B:
                    if (currentshade[var2currentshadePtr] < di)
                    {
                        //seg032_11FD
                        currentshade[var2currentshadePtr] = 0;
                        var2currentshadePtr += 2;

                        //Loop back to 120B
                        goto seg032_120B;
                    }
                    else
                    {
                        //seg032_1210
                        if (RelatedToFov_2C60 == 0xF)
                        {
                            return;
                        }
                        else
                        {
                            goto seg032_1175;
                        }
                    }
                }
                else
                {
                    //SEG32_11C8
                }
            }
            else
            {
                //seg032_1180
                //lookup a dseg and call 
                //seg032_C9D(visionparams)

                goto seg032_119D;
            }

        }


    }
}//end namespace