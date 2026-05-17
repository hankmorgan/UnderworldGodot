using System;
using Godot;

namespace Underworld
{
    //For management of the camera view of the player
    public partial class playerdat : Loader
    {
        
        /// <summary>
        /// Positions the player game camera based on x/y/z pos and current tileX/Y. 
        /// 
        /// For future expansion to match the original game this will need to be able support attaching the camera to a game object. 
        /// This is most often used in do_trap_camera but theoreticial the camera can be attached to any object by changing a global pointer 
        /// to point away from the player object and aim at an object instead.
        /// </summary>
        public static void PositionPlayerCamera()
        {
            var x = motion.playerMotionParams.x_0;
            var y = motion.playerMotionParams.y_2;
            var z = motion.playerMotionParams.z_4 + 0xA4;//offset for player head. This is lower than player height.
            if (motion.CameraIsBobbing_dseg_67d6_33c6)
            {
                z += motion.CameraBobZAdjust_dseg_67d6_33CE;
            }


            //Get a vector for the camera that is relative to the bounds of an underworld level map.
            Vector3 underworldVector = new(x: -(float)x / 16384f, y: (float)z / 1024f, z: (float)y / 16384f);
           
           //then transform it into godot positioning using a vector based on the size we are rendering the gameworld in.
            main.cameraYawGimbal.Position = underworldVector * UWTileMap.godotscale;
        

            //set up yaw, pitch and other motion values.
            var yaw = (float)motion.PlayerCameraYaw_dseg_8294;
            var roll = (float)motion.PlayerCameraRoll_dseg_67d6_33D8;
            var pitch = (float)motion.PlayerCameraPitch_dseg_67d6_33D6;

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

    }
}