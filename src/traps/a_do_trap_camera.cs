using Godot;
using Peaky.Coroutines;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Underworld
{

    public class a_do_trap_camera : objectInstance
    {
        //public static List<Camera3D> secondarycameras = new();
        //public Camera3D do_camera;

        // public a_do_trap_camera(Camera3D do_camera, uwObject _uwobject)
        // {
        //     //this.do_camera = do_camera;
        //     uwobject = _uwobject;
        // }

        // public static bool CreateDoTrapCamera(Node3D parent, uwObject trapObj, string name)
        // {//maybe consider creating this camera only when needed or just using it's value to override the player camera position set in PositionPlayerCamera()
        //     Camera3D newcam = new();
        //     newcam.Fov = main.cameraPitchGimbal.Fov;      
        //     main.instance.secondarycameras.AddChild(newcam);
        //     newcam.Current=false;
        //     newcam.Position = trapObj.GetCoordinate();
        //     model3D.SetObjectRotation(newcam, trapObj);
        //     trapObj.instance = new a_do_trap_camera(newcam, trapObj);
        //     secondarycameras.Add(newcam);// to track cameras so that when loading is implemented I can destroy old cameras
        //     return false;//implemented
        // }            


        /// <summary>
        /// Waits until further input before clearing the cam.
        /// </summary>
        /// <returns></returns>
        static IEnumerator CameraWaitForInput()
        {
            playerdat.CameraReference = null;//switch to the do trap
            playerdat.PositionPlayerCamera();
            bool automap = playerdat.AutomapEnabled;
            playerdat.AutomapEnabled = false;
            MessageDisplay.WaitingForMore = true; //quick hack to block input
            while (MessageDisplay.WaitingForMore)
            {
                yield return new WaitOneFrame();//TODO when using waiting for more the gameworld pauses. I need to update this so that npcs still move while viewing the camera.                
            }
            playerdat.AutomapEnabled = automap;
            playerdat.CameraReference = playerdat.playerObject;
            playerdat.PositionPlayerCamera();
            //main.cameraPitchGimbal.MakeCurrent();
        }


        /// <summary>
        /// Activates the camera linked to the trap
        /// </summary>
        /// <param name="trapObj"></param>
        /// <param name="triggerObj"></param>
        /// <param name="objList"></param>
        public static void Activate(uwObject trapObj, uwObject[] objList)
        {
            //switch camera            
            //var d = (a_do_trap_camera)(trapObj.instance);
            playerdat.DoCameraX = (short)((trapObj.tileX << 8) + (trapObj.xpos << 5));
            playerdat.DoCameraY = (short)((trapObj.tileY << 8) + (trapObj.ypos << 5));
            playerdat.DoCameraZ = (short)(trapObj.zpos << 3);
            playerdat.DoCameraH = (short)(trapObj.heading<<0xD);
            playerdat.DoCameraPitch = 0;
            playerdat.DoCameraRoll = 0;

            _ = Coroutine.Run(
                   CameraWaitForInput(),
                    main.instance
               );
        }

    }//end class
}//end namespace