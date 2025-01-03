using System.Diagnostics;
using System.Collections;
using Peaky.Coroutines;
using System.Collections.Generic;
using Godot;

namespace Underworld
{
    /// <summary>
    /// The main virtual machine for running the cutscenes
    /// </summary>
    public partial class cutsplayer : UWClass
	{       

        static int FrameNo = 0;
        static bool FullScreen;

        static int StringBlock;

        public static List<CutSceneCommand> commands;
        
        
        /// <summary>
        /// Loads and begins a cutscene
        /// </summary>
        /// <param name="CutsceneNo">The index number of the cutscene to play</param>
        /// <param name="callBackMethod">Function to call after the cutscene has played</param>

        public static void PlayCutscene(int CutsceneNo, CallBacks.CutsceneCallBack callBackMethod)
        {           
            if (CutsceneNo>=256)
            {
                FullScreen = false;
            }
            else
            {
                FullScreen = true;
            }
            if (_RES==GAME_UW2)
            {
                if (CutsceneNo == 2)
                {
                    Debug.Print("Do something with XMI");
                }
            }
            else
            {
                if ((CutsceneNo == 1) || (CutsceneNo == 2) || (CutsceneNo == 3))
                {
                    Debug.Print("Do something with XMI");
                }
            }

            StringBlock = 0xC00 + CutsceneNo;

            if (CutsceneNo == 0x103)
            {
                Debug.Print("Uses a different draw height");
            }

            //Debug.Print(GetsCutsceneFileName(0,0));
            

            //Read the .N00 control file
            if (Loader.ReadStreamFile(
                System.IO.Path.Combine(
                    Loader.BasePath, "CUTS", GetsCutsceneFileName(CutsceneNo,0)
                    ), out byte[] CutsData)
                )
            {
                commands = LoadCutsceneData(CutsData);
            }

            uimanager.EnableDisable(uimanager.instance.PanelMainMenu, false);
            
            //start the cutscene
            _ = Peaky.Coroutines.Coroutine.Run(
                RunCutscene(CutsceneNo,callBackMethod),
                main.instance);
        }

        public static IEnumerator RunCutscene(int CutsceneNo, CallBacks.CutsceneCallBack callBackMethod = null)
        {       
            TextureRect cutscontrol;   
            
            if (FullScreen)
            {
                cutscontrol = uimanager.CutsFullscreen;
            }
            else
            {
                cutscontrol = uimanager.CutsSmall;
            }
            uimanager.EnableDisable(cutscontrol,true);
            uimanager.EnableDisable(uimanager.instance.CutsSubtitle,true);
            uimanager.instance.CutsSubtitle.Text = "";

            FrameNo = 0;
            var FrameWait = 1;
            //load the art file

            //Art file.
            CutsLoader cuts = null;
            // var cuts = new CutsLoader(System.IO.Path.Combine(
            //         Loader.BasePath, "CUTS", GetsCutsceneFileName(CutsceneNo,1)));

            //Set initial frame to black
            uimanager.FlashColour(
                colour: 0, 
                targetControl: cutscontrol, 
                duration: 1, 
                IgnoreDelay: true);
            
            int cmdCount = 0;

            foreach (var cmd in commands)
            {
                string paramlist="";
                for (int p = 0; p<cmd.NoOfParams;p++)
                {
                    paramlist = paramlist+$"({cmd.functionParams[p]})";
                }
                Debug.Print($"{cmdCount++} {cmd.FunctionName}: {paramlist}");
                switch (cmd.functionNo)
                {
                    case 0: //show text with colour
                        {
                            Debug.Print($"Display subtitle: {GameStrings.GetString(StringBlock, cmd.functionParams[1])}");
                            uimanager.instance.CutsSubtitle.Text = GameStrings.GetString(StringBlock, cmd.functionParams[1]);
                            break;
                        }
                    case 3://pause
                    {
                        FrameWait = 0;
                        yield return new WaitForSeconds(cmd.functionParams[0]);
                        break;
                    }
                    case 4://to frame
                        {                            
                            //FrameWait = cmd.functionParams[0];
                            for (int i = 0; i<cmd.functionParams[0];i++)
                            {
                                if (cuts!=null)
                                {
                                    if (FrameNo> cuts.ImageCache.GetUpperBound(0))
                                    {
                                        //if (cmd.functionParams[1]==0) //loop?
                                        //{
                                            FrameNo = 0;
                                        //}
                                        //else
                                        //{
                                        //    break;//end loop.
                                        //}
                                    }
                                    uimanager.DisplayCutsImage(
                                        cuts: cuts, 
                                        imageNo: FrameNo++, 
                                        targetControl: cutscontrol);
                                }
                                else
                                {
                                    Debug.Print ("Cuts file is null!");
                                }

                                yield return new WaitForSeconds(0.2f);
                            }
                            break;
                        }  
                    case 6://End cutscene
                        {
                            uimanager.EnableDisable(cutscontrol,false);
                            uimanager.EnableDisable(uimanager.instance.CutsSubtitle,false);
                            yield return null;
                            break;
                        }
                    case 8://open file
                        {
                            Debug.Print($"Open {GetsCutsceneFileName(cmd.functionParams[0],cmd.functionParams[1])}");
                            cuts = new CutsLoader(System.IO.Path.Combine(
                                Loader.BasePath, "CUTS", GetsCutsceneFileName(cmd.functionParams[0],cmd.functionParams[1])));
                            FrameNo = 0;
                            FrameWait = 0;
                            break;
                        }              
                    case 13://text-play with audio.       
                        {
                            Debug.Print($"Display subtitle: {GameStrings.GetString(StringBlock, cmd.functionParams[1])}");
                            uimanager.instance.CutsSubtitle.Text = GameStrings.GetString(StringBlock, cmd.functionParams[1]);
                            if (cmd.functionParams[2] != 999 )
                            {
                                Debug.Print($"Play .voc audio {cmd.functionParams[2]}");
                                var sound = vocLoader.Load(
                                    System.IO.Path.Combine(
                                        Loader.BasePath,
                                        "SOUND",
                                        $"{cmd.functionParams[2]:0#}.VOC"));
                                if (sound!=null)
                                {//TODO: make sure a wait for as long as the audio needs to play occurs.
                                    main.instance.audioplayer.Stream = sound.toWav();
                                    main.instance.audioplayer.Play();                                    
                                }
                            }
                            break; 
                        }     
                    case 14: //wait seconds
                        {
                            FrameWait = 0;
                            yield return new WaitForSeconds(cmd.functionParams[0]);
                            break;
                        } 
                    default:
                        {
                            Debug.Print($"Unimplemented cutscene command {cmd.functionNo} {cmd.FunctionName}");
                            break;
                        }

                }
                for (int f = 0; f<FrameWait;f++)
                {
                    yield return new WaitForSeconds(0.2f);
                }               
 
            }

            if (callBackMethod!=null)
            {
                callBackMethod();
            }
            
            yield return null;
        }


    }//end class
}//end namespace