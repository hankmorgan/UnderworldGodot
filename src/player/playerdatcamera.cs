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


        /// <summary>
        /// Camera positions for do trap camera and roaming sight.
        /// </summary>
        public static short DoCameraX = 0;
        public static short DoCameraY = 0;
        public static short DoCameraZ = 0;
        public static short DoCameraH = 0;
        public static int DoCameraPitch = 0;
        public static int DoCameraRoll = 0;

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
                        z: motion.playerMotionParams.z_4 + 0xA4, 
                        yaw: (float)motion.PlayerCameraYaw_dseg_8294, 
                        roll: (float)motion.PlayerCameraRoll_dseg_67d6_33D8, 
                        pitch: (float)motion.PlayerCameraPitch_dseg_67d6_33D6, 
                        applyBob: motion.CameraIsBobbing_dseg_67d6_33c6);

                }
                else
                {
                    Debug.Print("TODO attach a camera to a non-player mobile object.");
                }
            }
            else
            {
                //no camera object is define. Used the values in doTrapCameraX/Y/Z/heading to render the camera.
                    PositionCamera(
                        x: DoCameraX, y: DoCameraY, z: DoCameraZ, 
                        yaw: DoCameraH, roll: DoCameraRoll, pitch: DoCameraPitch, 
                        applyBob: false);
            }
        }

        private static void PositionCamera(short x, short y, int z, float yaw, float roll, float pitch, bool applyBob)
        {
            if (applyBob)
            {
                z += motion.CameraBobZAdjust_dseg_67d6_33CE; //note that over time when in water the camera will go beneath the water level due to the swimmingcounter getting larger
            }

            //Get a vector for the camera that is relative to the bounds of an underworld level map.
            Vector3 underworldVector = new(x: -(float)x / 16384f, y: (float)z / 1024f, z: (float)y / 16384f);

            //then transform it into godot positioning using a vector based on the size we are rendering the gameworld in.
            main.cameraYawGimbal.Position = underworldVector * UWTileMap.godotscale;

            if (motion.CameraIsBobbing_dseg_67d6_33c6)
            {
                yaw += (float)motion.CameraYawModifier_dseg_67d6_33D0;
                roll += (float)motion.CameraRollModifier_dseg_67d6_33D4;
                pitch += (float)motion.CameraPitchModifier_dseg_67d6_33D2;
            }

            //Set up the Yaw gimbal             
            main.cameraYawGimbal.Rotation = Vector3.Zero;
            main.cameraYawGimbal.Rotate(Vector3.Up, (float)(Math.PI));//align to the north.
            main.cameraYawGimbal.Rotate(Vector3.Up, (float)(-(yaw / 32767f) * Math.PI));

            //Set up the Roll Gimbal
            main.cameraRollGimbal.Rotation = Vector3.Zero;
            main.cameraRollGimbal.Rotate(Vector3.Forward, (float)(-(roll / 32767f) * Math.PI));

            //Set up the pitch gimbal.
            main.cameraPitchGimbal.Rotation = Vector3.Zero;
            main.cameraPitchGimbal.Rotate(Vector3.Right, (float)(+(pitch / 32767f) * Math.PI));

            //Set this value to calculate npc angles
            motion.CameraYawHeadingRelated_2B52 = (short)(((1 + (motion.PlayerCameraYaw_dseg_8294 >> 0xD)) & 0x7) >> 1);
            motion.CameraPointer2C = (short)(motion.PlayerCameraYaw_dseg_8294 - motion.PlayerCardinalHeadingLookupTable[motion.CameraYawHeadingRelated_2B52]);
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
                }
            }
            else
            {
                Debug.Print("set camera view values for other objects.");
            }
        }
    }
}