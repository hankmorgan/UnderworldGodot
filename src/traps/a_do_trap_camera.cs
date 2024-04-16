using Godot;
using Peaky.Coroutines;
using System.Collections;
using System.Collections.Generic;

namespace Underworld
{
   
    public class a_do_trap_camera : objectInstance
    {
        public static List<Camera3D> secondarycameras = new();
        public Camera3D do_camera;

        public a_do_trap_camera(Camera3D do_camera, uwObject _uwobject)
        {
            this.do_camera = do_camera;
            uwobject = _uwobject;
        }

        public static bool CreateDoTrapCamera(Node3D parent, uwObject trapObj, string name)
        {//maybe consider creating this camera only when needed
            Camera3D newcam = new();
            newcam.Fov = main.gamecam.Fov;      
            main.instance.secondarycameras.AddChild(newcam);
            newcam.Current=false;
            newcam.Position = trapObj.GetCoordinate(trapObj.tileX, trapObj.tileY);
            //todo:rotation
            model3D.SetObjectRotation(newcam, trapObj);
            trapObj.instance = new a_do_trap_camera(newcam, trapObj);
            secondarycameras.Add(newcam);// to track cameras so that when loading is implemented I can destroy old cameras
            return false;//implemented
        }            


        /// <summary>
        /// Waits until further input before clearing the cam.
        /// </summary>
        /// <returns></returns>
        static IEnumerator CameraWaitForInput()
        {
            MessageDisplay.WaitingForMore = true; //quick hack to block input
            while (MessageDisplay.WaitingForMore)
            {
                yield return new WaitOneFrame();
            }
            main.gamecam.MakeCurrent();
            main.gamecam.Set("MOVE", true);
        }
        

        /// <summary>
        /// Activates the camera linked to the trap
        /// </summary>
        /// <param name="trapObj"></param>
        /// <param name="triggerObj"></param>
        /// <param name="objList"></param>
        public static void Activate(uwObject trapObj,uwObject[] objList)
        {
            //switch camera
            var d = (a_do_trap_camera)(trapObj.instance);
            d.do_camera.MakeCurrent();
            main.gamecam.Set("MOVE", false);
            _ = Peaky.Coroutines.Coroutine.Run(
                   CameraWaitForInput(),
                    main.instance
               );
        }

    }//end class
}//end namespace