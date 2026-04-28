using System.Collections;
using Peaky.Coroutines;
using Godot;

namespace Underworld
{
    /// <summary>
    /// For managing the two dragons on the UW1 Hud.
    /// </summary>
    public partial class uimanager : Node2D
    {
        
        static ImageTexture[] DragonFrames;

        const int LeftTailShakeIndex = 0;
        const int LeftHeadScrollIndex = 1;
        const int LeftHeadCowerIndex = 2;
        const int LeftHeadIdleIndex = 3;
        const int RightTailShakeIndex = 4;
        const int RightHeadScrollIndex = 5;
        const int RightHeadCowerIndex = 6;
        const int RightHeadIdleIndex = 7;

        static bool LeftDragonHeadBusy = false;
        static bool RightDragonHeadBusy = false;
        static bool LeftDragonTailBusy = false;
        static bool RightDragonTailBusy = false;

        static readonly int[,] AnimFrames =
            {
                {14,15,16,17,16,15},	//left tail shake
				{2, 3, 4, 5, 4, 3 },	//left head scroll	
				{10,11,12,13,12,11},	//left head cower
				{1, 1, 1, 1, 1, 1 },	//left head idle
				{32,33,34,35,34,33},	//right tail shake
				{20,21,22,23,22,21},	//right head scroll
				{28,29,30,31,30,29},	//right head cower
				{19,19,19,19,19,19}		//right head idle
		};

        [ExportGroup("Dragons")]
        [Export] public TextureRect[] DragonHeads = new TextureRect[2];
        [Export] public TextureRect[] DragonTails = new TextureRect[2];
        [Export] public TextureRect[] DragonTorsos = new TextureRect[2];

        public static void StartDragonAnimation(int animationNo)
        {
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                return;
            }
            if (!InGame)
            {
                return;
            }
            //0=shake tail.
            //1=scroll
            //2=cower
            //3=idle
            //if right side offset index by +4
            bool IsRightSide = false;
            bool IsTail = false;
            if (animationNo == 0)
            {
                IsTail = true;
            }
            //pick a random dragon to animate.
            if (Rng.r.Next(2) >= 1)
            {
                IsRightSide = true;
                animationNo = animationNo + 4;
            }

            if ((IsTail == false))
            {
                if (IsRightSide && RightDragonHeadBusy)
                {
                    return;//do nothing
                }
                if (!IsRightSide && LeftDragonHeadBusy)
                {
                    return;
                }
            }
            else
            {
                if (IsRightSide && RightDragonTailBusy)
                {
                    return;//do nothing
                }
                if (!IsRightSide && LeftDragonTailBusy)
                {
                    return;
                }
            }


            _ = Peaky.Coroutines.Coroutine.Run(
                        DragonAnimation(animationNo: animationNo, IsRightSide: IsRightSide, IsTail: IsTail),
                        main.instance);
        }

        static IEnumerator DragonAnimation(int animationNo, bool IsRightSide, bool IsTail)
        {
            TextureRect targetcontrol;
            int frame = 0;
            if (IsTail)
            {
                if (IsRightSide)
                {
                    targetcontrol = instance.DragonTails[1];
                }
                else
                {
                    targetcontrol = instance.DragonTails[0];
                }
            }
            else
            {
                if (IsRightSide)
                {
                    targetcontrol = instance.DragonHeads[1];
                }
                else
                {
                    targetcontrol = instance.DragonHeads[0];
                }
            }
            
            for (int i = 0; i <= AnimFrames.GetUpperBound(1); i++)
            {
                var toDisplay = AnimFrames[animationNo, frame++];
                targetcontrol.Texture = DragonFrames[toDisplay];

                yield return new WaitForSeconds(0.1f);
            }

            //clear busy flags
            if (IsRightSide && !IsTail)
            {
                RightDragonHeadBusy = false;
            }
            if (IsRightSide && IsTail)
            {
                RightDragonTailBusy = false;
            }

            if (!IsRightSide && !IsTail)
            {
                LeftDragonHeadBusy = false;
            }
            if (!IsRightSide && IsTail)
            {
                LeftDragonTailBusy = false;
            }

            yield return null;
        }

        private void InitDragons()
        {
            if (UWClass._RES != UWClass.GAME_UW2)
            {                                
                //prepare the images by taking the images from dragons.gr and inserting them into a set of blank frames.
                var dragonsGr = new GRLoader(GRLoader.DRAGONS_GR, GRLoader.GRShaderMode.UIShader);
                //first resize all dragon heads to their max size
                Image[] _tempDragonFrames = new Image[dragonsGr.ImageCache.GetUpperBound(0) + 1];
                DragonFrames = new ImageTexture[dragonsGr.ImageCache.GetUpperBound(0) + 1];
                for (int i = 0; i <= _tempDragonFrames.GetUpperBound(0); i++)
                {
                    switch (i)
                    {
                        case 1:// max size image, no changes needed.
                             _tempDragonFrames[i] = dragonsGr.LoadImageAt(i).GetImage(); break;//ArtLoader.InsertImage(dragonsGr.LoadImageAt(i).GetImage(), ArtLoader.BlankCanvas(37, 23), 0, 0); break;
                        case 2:// body and hands moving
                        case 3:
                        case 4:
                        case 5:
                            {
                                var tmp = dragonsGr.LoadImageAt(1).GetImage();//load key frame
                                 _tempDragonFrames[i] = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(i).GetImage(), dstImg: tmp, cornerX: 4, cornerY: 10); break;
                            }
                        case 6://left haders
                        case 7:
                        case 8:
                        case 9:
                            _tempDragonFrames[i] = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(i).GetImage(), dstImg: _tempDragonFrames[1], cornerX: 12, cornerY: 0); break;
                        case 10:
                        case 11:
                        case 12:
                        case 13://max image for right, needs to be increased due to lack of uniformity in images sizes on the right side.
                             _tempDragonFrames[i] = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(i).GetImage(), dstImg: ArtLoader.BlankCanvas(37, 23), cornerX: 0, cornerY: 0); break;
                        case 19:
                            _tempDragonFrames[i] = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(i).GetImage(), dstImg: ArtLoader.BlankCanvas(40, 25), cornerX: 6, cornerY: 0); break;
                        case 20:
                            {
                                var tmp = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(19).GetImage(), dstImg: ArtLoader.BlankCanvas(40, 25), cornerX: 6, cornerY: 0);
                                 _tempDragonFrames[i] = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(i).GetImage(), dstImg: tmp, cornerX: 5, cornerY: 12); break;
                            }
                        case 21:
                        case 22:
                        case 23:
                            {
                                var tmp = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(19).GetImage(), dstImg: ArtLoader.BlankCanvas(40, 25), cornerX: 6, cornerY: 0);
                                 _tempDragonFrames[i] = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(i).GetImage(), dstImg: tmp, cornerX: 6, cornerY: 10); break;
                            }
                        case 24:
                        case 25:
                        case 26: //right heads
                        case 27:
                            {
                                var tmp = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(19).GetImage(), dstImg: ArtLoader.BlankCanvas(40, 25), cornerX: 6, cornerY: 0);
                                 _tempDragonFrames[i] = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(i).GetImage(), dstImg: tmp, cornerX: 6, cornerY: 0); break;
                            }
                        default:
                             _tempDragonFrames[i] = dragonsGr.LoadImageAt(i).GetImage(); break;//  ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(i).GetImage(), dstImg: ArtLoader.BlankCanvas(40, 23), cornerX: 0, cornerY: 0); break;
                    }
                    var tex = new ImageTexture();
                    tex.SetImage( _tempDragonFrames[i]);
                    DragonFrames[i] = tex;   
                }

                //load ui
                instance.DragonHeads[0].Texture = dragonsGr.LoadImageAt(1);
                instance.DragonHeads[1].Texture = dragonsGr.LoadImageAt(19);
                instance.DragonTorsos[0].Texture = dragonsGr.LoadImageAt(0);
                instance.DragonTorsos[1].Texture = dragonsGr.LoadImageAt(18);
                instance.DragonTails[0].Texture =  dragonsGr.LoadImageAt(14);
                instance.DragonTails[1].Texture =  dragonsGr.LoadImageAt(32);
            }
        }


    }//end class
}//end namespace