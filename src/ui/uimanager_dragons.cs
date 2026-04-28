using System.Numerics;
using Godot;

namespace Underworld
{
    /// <summary>
    /// For managing the two dragons on the UW1 Hud.
    /// </summary>
    public partial class uimanager : Node2D
    {
        static Image[] DragonFrames = new Image[32];


        const int LeftTailShakeIndex = 0;
        const int LeftHeadScrollIndex = 1;
        const int LeftHeadCowerIndex = 2;
        const int LeftHeadIdleIndex = 3;
        const int RightTailShakeIndex = 4;
        const int RightHeadScrollIndex = 5;
        const int RightHeadCowerIndex = 6;
        const int RightHeadIdleIndex = 7;

        readonly int[,] DragonAnimFrames =
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


        private void InitDragons()
        {
            if (UWClass._RES != UWClass.GAME_UW2)
            {
                //prepare the images by taking the images from dragons.gr and inserting them into a set of blank frames.
                var dragonsGr = new GRLoader(GRLoader.DRAGONS_GR, GRLoader.GRShaderMode.UIShader);
                //first resize all dragon heads to their max size
                DragonFrames = new Image[dragonsGr.ImageCache.GetUpperBound(0) + 1];
                for (int i = 0; i <= DragonFrames.GetUpperBound(0); i++)
                {
                    switch (i)
                    {
                        case 1:// max size image, no changes needed.
                            DragonFrames[i] = dragonsGr.LoadImageAt(i).GetImage(); break;//ArtLoader.InsertImage(dragonsGr.LoadImageAt(i).GetImage(), ArtLoader.BlankCanvas(37, 23), 0, 0); break;
                        case 2:// body and hands moving
                        case 3:
                        case 4:
                        case 5:
                            {
                                var tmp = dragonsGr.LoadImageAt(1).GetImage();//load key frame
                                DragonFrames[i] = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(i).GetImage(), dstImg: tmp, cornerX: 4, cornerY: 10); break;
                            }
                        case 6://left haders
                        case 7:
                        case 8:
                        case 9:
                            DragonFrames[i] = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(i).GetImage(), dstImg: DragonFrames[1], cornerX: 12, cornerY: 0); break;
                        case 10:
                        case 11:
                        case 12:
                        case 13://max image for right, needs to be increased due to lack of uniformity in images sizes on the right side.
                            DragonFrames[i] = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(i).GetImage(), dstImg: ArtLoader.BlankCanvas(37, 23), cornerX: 0, cornerY: 0); break;
                        case 19:
                            DragonFrames[i] = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(i).GetImage(), dstImg: ArtLoader.BlankCanvas(40, 25), cornerX: 6, cornerY: 0); break;
                        case 20:
                            {
                                var tmp = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(19).GetImage(), dstImg: ArtLoader.BlankCanvas(40, 25), cornerX: 6, cornerY: 0);
                                DragonFrames[i] = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(i).GetImage(), dstImg: tmp, cornerX: 5, cornerY: 12); break;
                            }
                        case 21:
                        case 22:
                        case 23:
                            {
                                var tmp = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(19).GetImage(), dstImg: ArtLoader.BlankCanvas(40, 25), cornerX: 6, cornerY: 0);
                                DragonFrames[i] = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(i).GetImage(), dstImg: tmp, cornerX: 6, cornerY: 10); break;
                            }
                        case 24:
                        case 25:
                        case 26: //right heads
                        case 27:
                            {
                                var tmp = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(19).GetImage(), dstImg: ArtLoader.BlankCanvas(40, 25), cornerX: 6, cornerY: 0);
                                DragonFrames[i] = ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(i).GetImage(), dstImg: tmp, cornerX: 6, cornerY: 0); break;
                            }
                        default:
                            DragonFrames[i] = dragonsGr.LoadImageAt(i).GetImage(); break;//  ArtLoader.InsertImage(srcImg: dragonsGr.LoadImageAt(i).GetImage(), dstImg: ArtLoader.BlankCanvas(40, 23), cornerX: 0, cornerY: 0); break;
                    }
                    //DragonFrames[i].SavePng($"c:\\temp\\resize\\dragonframe{i.ToString("d2")}(resized).png");

                }                
            }
        }


    }//end class
}//end namespace