using System.ComponentModel;
using Godot;
using Peaky.Coroutines;
using System.Collections;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("Cutscenes")]
        [Export] TextureRect CutsSmallUW1;
        [Export] TextureRect CutsSmallUW2;

        public static void InitCuts()
        {
            EnableDisable(CutsSmall,false);
        }

        /// <summary>
        /// Gets the smaller cuts window
        /// </summary>
        public static TextureRect CutsSmall
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return instance.CutsSmallUW2;
                }
                else
                {
                    return instance.CutsSmallUW1;
                }
            }
        }


        /// <summary>
        /// Loads a cuts file and display image at specified index on a target control
        /// </summary>
        /// <param name="cutsfile"></param>
        /// <param name="imageNo"></param>
        /// <param name="targetControl"></param>
        public static void DisplayCutsImage(string cutsfile, int imageNo, TextureRect targetControl, bool DisableCamera = true)
        {            
            var cuts = new CutsLoader(cutsfile);
            if (!csCuts.ContainsKey(cutsfile))
            {
                csCuts.Add(cutsfile, new CutsLoader(cutsfile));
            }
            _ = Peaky.Coroutines.Coroutine.Run(
                    DisplayCutsImageWithWait(cuts: csCuts[cutsfile], imageNo: imageNo, targetControl: targetControl, DisableCamera: DisableCamera),
                    main.instance
               );            
        }

        /// <summary>
        /// Displays the specified image number from a cuts file and waits for input
        /// </summary>
        /// <param name="cuts"></param>
        /// <param name="imageNo"></param>
        /// <param name="targetControl"></param>
        /// <returns></returns>
        private static IEnumerator DisplayCutsImageWithWait(CutsLoader cuts, int imageNo, TextureRect targetControl, bool DisableCamera = true)
        {
            targetControl.Texture = cuts.LoadImageAt(imageNo);
            targetControl.Material = cuts.GetMaterial(imageNo); 
            EnableDisable(targetControl, true);
            if (DisableCamera)
            {
                instance.uwsubviewport.Disable3D=true;
            }
            MessageDisplay.WaitingForMore=true;
            while(MessageDisplay.WaitingForMore)
            {//wait until key input before clearing the image
                yield return new WaitOneFrame();
            }
            EnableDisable(targetControl, false);
            instance.uwsubviewport.Disable3D=false;
            yield return 0;
        }


    }//end class
}//end namespace