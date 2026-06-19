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

        public static short CAM_x;
        public static short CAM_y;

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

            //Set values needed for visibility checks
            CAM_x = (short)(x & 0xFF);
            CAM_y = (short)(y & 0xFF);

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

            //The following code is used to draw the automap.
            //first the camera values must be updated depending on the player direction.
            x = (short)(x & 0xFF);
            y = (short)(y & 0xFF);
            switch (motion.CameraYawHeadingRelated_2B52)
            {
                case 1:
                    {
                        var si = x;
                        x = (short)(0xFF - y);
                        y = si;
                        break;
                    }
                case 2:
                    {
                        x = (short)(0xFF - x);
                        y = (short)(0xFF - y);
                        break;
                    }
                case 3:
                    {
                        var si = x;
                        x = y;
                        y = (short)(0xFF - si);
                        break;
                    }
            }

            yaw = (short)(yaw - VisionParams.cardinallookup_44A[motion.CameraYawHeadingRelated_2B52]);

            VisionParams.SetRangeOfVisionParams(
                camerax: x,
                cameray: y,
                camerayaw: yaw);

            VisionParams.GetViewDistance();
            VisionParams.FakeRender();

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


}//end namespace