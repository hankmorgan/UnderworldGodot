using System.IO;
using Godot;

namespace Underworld
{
    /// <summary>
    /// Crit loader.
    /// </summary>
    /// Loads the basic animation frames for npcs
    public class CritLoader : ArtLoader
    {

        public CritterInfo critter;// =new CritterInfo[64];


        public CritLoader(int CritterToLoad)
        {
            //if (xfer==null)
            //{
            //    xfer = new XFerLoader();
            //}
            //Load the assoc file
            switch (_RES)
            {
                case GAME_UW2:
                    ReadUW2AssocFile(CritterToLoad);
                    return;
                case GAME_UWDEMO:
                    ReadUw1AssocFile(CritterToLoad, Path.Combine(BasePath, "CRIT", "DASSOC.ANM"));
                    return;
                default:
                    ReadUw1AssocFile(CritterToLoad, Path.Combine(BasePath, "CRIT", "ASSOC.ANM"));
                    return;
            }
        }

        private void ReadUw1AssocFile(int CritterToLoad, string assocpath)
        {
            long AssocAddressPtr = 256;
            if (ReadStreamFile(assocpath, out byte[] assoc))
            {
                for (int ass = 0; ass <= 63; ass++)
                {
                    int FileID = (int)getValAtAddress(assoc, AssocAddressPtr++, 8);
                    int auxPal = (int)getValAtAddress(assoc, AssocAddressPtr++, 8);
                    if (ass == CritterToLoad)
                    {
                        critter = new CritterInfo(FileID, PaletteLoader.Palettes[0], auxPal);
                    }
                }
            }
        }


        //  public Sprite RetrieveSpriteByName(string AnimToFind, int currentAnimNo)
        //{
        //		int index=-1;
        //		//I will know my animation mode from the npcs so i just need to iterate through the animation

        //		for (int j=0; j<=critter.AnimInfo.animSequence.GetUpperBound(0);j++)
        //		{
        //				if (critter.AnimInfo.animSequence[j,0]!=null)
        //				{
        //						for (int i=0; i<=critter.AnimInfo.animSequence.GetUpperBound(1);i++)
        //						{
        //								if (critter.AnimInfo.animSequence[j,i]==AnimToFind )
        //								{
        //										index= critter.AnimInfo.animIndices[j,i];
        //										break;
        //								}
        //						}		
        //				}
        //		}


        //		if(index!=-1)
        //		{
        //				return critter.AnimInfo.animSprites[index];
        //		}
        //		else
        //		{
        //				return null;
        //		}

        //}


        void ReadUW2AssocFile(int CritterToLoad)
        {
            //Load the assoc file
            long AssocAddressPtr = 0;
            if (
                            (ReadStreamFile(Path.Combine(BasePath, "CRIT", "AS.AN"), out byte[] assoc))
                            && (ReadStreamFile(Path.Combine(BasePath, "CRIT", "PG.MP"), out byte[] pgmp))
                            && (ReadStreamFile(Path.Combine(BasePath, "CRIT", "CR.AN"), out byte[] cran))
                    )
            {
                for (int ass = 0; ass <= 63; ass++)
                {
                    int FileID = (int)getValAtAddress(assoc, AssocAddressPtr++, 8);
                    int auxPal = (int)getValAtAddress(assoc, AssocAddressPtr++, 8);
                    if (FileID != 255)
                    {
                        if (ass == CritterToLoad)
                        {
                            critter = new CritterInfo(FileID, PaletteLoader.Palettes[0], auxPal, assoc, pgmp, cran);
                        }
                    }
                }
            }
        }
    }



/// <summary>
/// Class for critter animation info
/// </summary>
    public class CritterInfo : Loader
    {

        public const int idle_combat = 0x0;
        public const int attack_bash = 0x1;
        public const int attack_slash = 0x2;
        public const int attack_thrust = 0x3;
        public const int attack_unk4 = 0x4;
        public const int attack_secondary = 0x5;
        public const int attack_unk6 = 0x6;
        public const int walking_towards = 0x7;
        public const int death = 0xc;
        public const int begin_combat = 0xd;
        public const int idle_rear = 0x20;
        public const int idle_rear_right = 0x21;
        public const int idle_right = 0x22;
        public const int idle_front_right = 0x23;
        public const int idle_front = 0x24;
        public const int idle_front_left = 0x25;
        public const int idle_left = 0x26;
        public const int idle_rear_left = 0x27;
        public const int walking_rear = 0x80;
        public const int walking_rear_right = 0x81;
        public const int walking_right = 0x82;
        public const int walking_front_right = 0x83;
        public const int walking_front = 0x84;
        public const int walking_front_left = 0x85;
        public const int walking_left = 0x86;
        public const int walking_rear_left = 0x87;



        //public int Item_Id;
        //public int FileNo;
        //public int AuxPalNo;
        readonly byte[] FilePage0;
        readonly byte[] FilePage1;
        public Palette pal; //the game pal.
                            //private Palette auxpal;

        public CritterAnimInfo AnimInfo;


        /// <summary>
        /// Critter animations for UW1
        /// </summary>
        /// <param name="critter_id"></param>
        /// <param name="paletteToUse"></param>
        /// <param name="AuxPalNo"></param>
        public CritterInfo(int critter_id, Palette paletteToUse, int AuxPalNo)
        {
            //string fileO = DecimalToOct(file.ToString());
            string critterIDO = DecimalToOct(critter_id.ToString());
            //FileNo=file;
            //AuxPalNo=palno;
            pal = paletteToUse;
            AnimInfo = new CritterAnimInfo();
            int spriteIndex = 0;
            for (int pass = 0; pass < 2; pass++)
            {
                //load in both page files.
                if (pass == 0)
                {//CR{CRITTER file ID in octal}PAGE.N{Page}
                    var toLoad = Path.Combine(BasePath, "CRIT", "CR" + critterIDO + "PAGE.N0" + pass);
                    ReadStreamFile(toLoad, out FilePage0);
                    bool LoadMod = Directory.Exists(toLoad);  //TODO: Does this load mods here.
                    spriteIndex = ReadPageFileUW1(FilePage0, critter_id, pass, spriteIndex, AuxPalNo, LoadMod, toLoad);
                }
                else
                {
                    var toLoad = Path.Combine(BasePath, "CRIT", "CR" + critterIDO + "PAGE.N0" + pass);
                    ReadStreamFile(toLoad, out FilePage1);
                    bool LoadMod = Directory.Exists(toLoad);
                    ReadPageFileUW1(FilePage1, critter_id, pass, spriteIndex, AuxPalNo, LoadMod, toLoad);
                }
            }
        }

        /// <summary>
        /// Criter animations for UW2
        /// </summary>
        /// <param name="critter_id"></param>
        /// <param name="paletteToUse"></param>
        /// <param name="palno"></param>
        /// <param name="assocData"></param>
        /// <param name="PGMP"></param>
        /// <param name="cran"></param>
        public CritterInfo(int critter_id, Palette paletteToUse, int palno, byte[] assocData, byte[] PGMP, byte[] cran)
        {
            int ExtractPageNo = 0;
            string critterIDO = DecimalToOct(critter_id.ToString());
            AnimInfo = new CritterAnimInfo();
            int spriteIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                if ((int)getValAtAddress(PGMP, critter_id * 8 + i, 8) != 255)//Checks if a critter exists at this index in the page file.
                {
                    string ExtractPageNoO = DecimalToOct(ExtractPageNo.ToString());
                    string fileCrit = Path.Combine(BasePath, "CRIT", "CR" + critterIDO + "." + ExtractPageNoO);  // BasePath + sep + "CRIT" + sep + "CR" + critterIDO + "." + ExtractPageNoO;
                    spriteIndex = ReadUW2PageFileData(assocData, palno, fileCrit, AnimInfo, spriteIndex, paletteToUse);
                    ExtractPageNo++;
                }
            }
            int cranAdd = (critter_id * 512);
            for (int Animation = 0; Animation < 8; Animation++)//The the animation slot
            {
                bool NoAngle = isAnimUnAngled(Animation);
                //int NoOfValid=0;

                for (int Angle = 0; Angle < 8; Angle++)//Each animation has every possible angle.
                {
                    if ((NoAngle == false) || (Angle == 4))
                    {
                        int UW2animIndex = GetUW2Anim(Animation, Angle);
                        int animIndex = TranslateAnimToIndex(UW2animIndex);//Maybe remove this?
                        AnimInfo.animName[animIndex] = PrintAnimName(UW2animIndex);
                        int ValidEntries = (int)getValAtAddress(cran, cranAdd + (Animation * 64) + (Angle * 8) + (7), 8);//Get how many valid frames are in the animation
                        for (int FrameNo = 0; FrameNo < 8; FrameNo++)
                        {
                            int currFrame = (int)getValAtAddress(cran, cranAdd + (Animation * 64) + (Angle * 8) + (FrameNo), 8);
                            if (FrameNo < ValidEntries)
                            {
                                AnimInfo.animIndices[animIndex, FrameNo] = currFrame;
                            }
                            else
                            {
                                AnimInfo.animIndices[animIndex, FrameNo] = -1;
                            }
                        }
                    }
                }
            }
        }



        private int ReadPageFileUW1(byte[] PageFile, int XX, int YY, int spriteIndex, int AuxPalNo, bool LoadMod, string ModPath)
        {
            int addptr = 0;
            int slotbase = (int)getValAtAddress(PageFile, addptr++, 8);
            int NoOfSlots = (int)getValAtAddress(PageFile, addptr++, 8);
            int[] SlotIndices = new int[NoOfSlots];
            int spriteCounter = 0;
            int k = 0;
            string XXo = DecimalToOct(XX.ToString());
            string YYo = DecimalToOct(YY.ToString());
            for (int i = 0; i < NoOfSlots; i++)
            {
                int val = (int)getValAtAddress(PageFile, addptr++, 8);
                if (val != 255)
                {
                    SlotIndices[k++] = i;
                }
            }
            int NoOfSegs = (int)getValAtAddress(PageFile, addptr++, 8);
            for (int i = 0; i < NoOfSegs; i++)
            {
                //string[] AnimFiles = new string[8];
                string AnimName = PrintAnimName(slotbase + SlotIndices[i]);

                int index = TranslateAnimToIndex(slotbase + SlotIndices[i]);
                AnimInfo.animName[index] = AnimName;
                int ValidCount = 0;
                for (int j = 0; j < 8; j++)
                {
                    int val = (int)getValAtAddress(PageFile, addptr++, 8);
                    if (val != 255)
                    {                   //AnimFiles[j] = "CR" + XX.ToString("d2") + "PAGE_N" + YY.ToString("d2") + "_" + AuxPalNo + "_" + val;

                        AnimInfo.animSequence[index, j] = "CR" + XXo + "PAGE_N" + YYo + "_" + AuxPalNo + "_" + (val).ToString("d4");
                        AnimInfo.animIndices[index, j] = (val + spriteIndex);
                        ValidCount++;
                    }
                    else
                    {
                        AnimInfo.animIndices[index, j] = -1;
                    }
                }
            }

            //Read in the palette
            int NoOfPals = (int)getValAtAddress(PageFile, addptr, 8);//Will skip ahead this far.
            addptr++;
            byte[] auxPalVal = new byte[32];
            for (int i = 0; i < 32; i++)
            {
                auxPalVal[i] = (byte)getValAtAddress(PageFile, (addptr) + (AuxPalNo * 32) + i, 8);
            }

            //Skip past the palettes
            addptr += NoOfPals * 32;
            int NoOfFrames = (int)getValAtAddress(PageFile, addptr, 8);
            //AnimInfo.animSprites=new Sprite[NoOfFrames];
            addptr += 2;
            int addptr_start = addptr;//Bookmark my positiohn
            int MaxWidth = 0;
            int MaxHeight = 0;
            int MaxHotSpotX = 0;
            int MaxHotSpotY = 0;
            for (int pass = 0; pass <= 1; pass++)
            {
                addptr = addptr_start;
                if (pass == 0)
                {//get the max width and height
                    for (int i = 0; i < NoOfFrames; i++)
                    {
                        int frameOffset = (int)getValAtAddress(PageFile, addptr + (i * 2), 16);
                        int BitMapWidth = (int)getValAtAddress(PageFile, frameOffset + 0, 8);
                        int BitMapHeight = (int)getValAtAddress(PageFile, frameOffset + 1, 8);
                        int hotspotx = (int)getValAtAddress(PageFile, frameOffset + 2, 8);
                        int hotspoty = (int)getValAtAddress(PageFile, frameOffset + 3, 8);
                        if (hotspotx > BitMapWidth)
                        {
                            hotspotx = BitMapWidth;
                        }
                        if (hotspoty > BitMapHeight)
                        {
                            hotspoty = BitMapHeight;
                        }

                        if (BitMapWidth > MaxWidth)
                        {
                            MaxWidth = BitMapWidth;
                        }
                        if (BitMapHeight > MaxHeight)
                        {
                            MaxHeight = BitMapHeight;
                        }

                        if (hotspotx > MaxHotSpotX)
                        {
                            MaxHotSpotX = hotspotx;
                        }
                        if (hotspoty > MaxHotSpotY)
                        {
                            MaxHotSpotY = hotspoty;
                        }
                    }
                }
                else
                {//Extract
                    if (MaxHotSpotX * 2 > MaxWidth)
                    {//Try and center the hot spot in the image.
                        MaxWidth = MaxHotSpotX * 2;
                    }
                    byte[] outputImg;
                    outputImg = new byte[MaxWidth * MaxHeight * 2];
                    for (int i = 0; i < NoOfFrames; i++)
                    {
                        int frameOffset = (int)getValAtAddress(PageFile, addptr + (i * 2), 16);
                        int BitMapWidth = (int)getValAtAddress(PageFile, frameOffset + 0, 8);
                        int BitMapHeight = (int)getValAtAddress(PageFile, frameOffset + 1, 8);
                        int hotspotx = (int)getValAtAddress(PageFile, frameOffset + 2, 8);
                        int hotspoty = (int)getValAtAddress(PageFile, frameOffset + 3, 8);
                        int compression = (int)getValAtAddress(PageFile, frameOffset + 4, 8);
                        int datalen = (int)getValAtAddress(PageFile, frameOffset + 5, 16);

                        //Adjust the hotspots from the biggest point back to the image corners
                        int cornerX; int cornerY;
                        cornerX = MaxHotSpotX - hotspotx;
                        cornerY = MaxHotSpotY - hotspoty;
                        if (cornerX <= 0)
                        {
                            cornerX = 0;
                        }
                        else
                        {
                            cornerX--;
                        }
                        if (cornerY <= 0)
                        {
                            cornerY = 0;
                        }
                        bool ModFileIsLoaded = false;
                        // if (LoadMod)
                        // {
                        //     var toLoadMod = Path.Combine(ModPath, AuxPalNo.ToString(), i.ToString("d3") + ".tga");
                        //     if (File.Exists(toLoadMod))
                        //     {
                        //         Godot.Image tex = TGALoader.LoadTGA(toLoadMod);
                        //         ModFileIsLoaded = true;
                        //         AnimInfo.animSprites[spriteIndex + i] = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.0f));
                        //     }
                        // }
                        if (!ModFileIsLoaded)
                        {
                            //Extract the image
                            byte[] srcImg;
                            srcImg = new byte[BitMapWidth * BitMapHeight * 2];
                            outputImg = new byte[MaxWidth * MaxHeight * 2];
                            ArtLoader.Ua_image_decode_rle(PageFile, srcImg, compression == 6 ? 5 : 4, datalen, BitMapWidth * BitMapHeight, frameOffset + 7, auxPalVal);


                            //*Put the sprite in the a frame of size max width & height
                            cornerY = MaxHeight - cornerY;//y is from the top left corner
                            int ColCounter = 0; int RowCounter = 0;
                            bool ImgStarted = false;
                            for (int y = 0; y < MaxHeight; y++)
                            {
                                for (int x = 0; x < MaxWidth; x++)
                                {
                                    if ((cornerX + ColCounter == x) && (MaxHeight - cornerY + RowCounter == y) && (ColCounter < BitMapWidth) && (RowCounter < BitMapHeight))
                                    {//the pixel from the source image is here 
                                        ImgStarted = true;
                                        outputImg[x + (y * MaxWidth)] = srcImg[ColCounter + (RowCounter * BitMapWidth)];
                                        ColCounter++;
                                    }
                                    else
                                    {
                                        outputImg[x + (y * MaxWidth)] = 0;//alpha
                                    }
                                }
                                if (ImgStarted == true)
                                {//New Row on the src image
                                    RowCounter++;
                                    ColCounter = 0;
                                }
                            }
                            //Set the heights for output
                            BitMapWidth = MaxWidth;
                            BitMapHeight = MaxHeight;

                            //****************************

                            Godot.Image imgData = ArtLoader.Image(outputImg, 0, BitMapWidth, BitMapHeight, "namehere", pal, true, true);
                            CropImageData(ref imgData, pal);
                            //Image.CreateFromData(imgData.GetWidth(),imgData.GetHeight(),false,Image.Format.Rgba8,)
                            AnimInfo.animSprites[spriteIndex + i] = imgData;  ;//Sprite.Create(imgData, new Rect(0f, 0f, imgData.width, imgData.height), new Vector2(0.5f, 0.0f));
                            //AnimInfo.animSprites[spriteIndex + i].texture.filterMode = FilterMode.Point;
                            spriteCounter++;
                        }

                    }

                }//endextract
            }
            return spriteCounter;
        }




        public static string PrintAnimName(int animNo)
        {
            switch (animNo)
            {
                case 0x0:
                    return "idle_combat";
                case 0x1:
                    return "attack_bash";
                case 0x2:
                    return "attack_slash";
                case 0x3:
                    return "attack_thrust";
                case 0x4:
                    return "attack_unk4";
                case 0x5:
                    return "attack_secondary";
                case 0x6:
                    return "attack_unk6";
                case 0x7:
                    return "walking_towards";
                case 0xc:
                    return "death";
                case 0xd:
                    return "begin_combat";
                case 0x20:
                    return "idle_rear";
                case 0x21:
                    return "idle_rear_right";
                case 0x22:
                    return "idle_right";
                case 0x23:
                    return "idle_front_right";
                case 0x24:
                    return "idle_front";
                case 0x25:
                    return "idle_front_left";
                case 0x26:
                    return "idle_left";
                case 0x27:
                    return "idle_rear_left";
                case 0x28:
                    return "unknown_anim_40";
                case 0x29:
                    return "unknown_anim_41";
                case 0x2a:
                    return "unknown_anim_42";
                case 0x2b:
                    return "unknown_anim_43";
                case 0x2c:
                    return "unknown_anim_44";
                case 0x2d:
                    return "unknown_anim_45";
                case 0x2e:
                    return "unknown_anim_46";
                case 0x2f:
                    return "unknown_anim_47";
                case 0x50:
                    return "unknown_anim_80";
                case 0x51:
                    return "unknown_anim_81";
                case 0x52:
                    return "unknown_anim_82";
                case 0x53:
                    return "unknown_anim_83";
                case 0x54:
                    return "unknown_anim_84";
                case 0x55:
                    return "unknown_anim_85";
                case 0x56:
                    return "unknown_anim_86";
                case 0x57:
                    return "unknown_anim_87";
                case 0x80:
                    return "walking_rear";
                case 0x81:
                    return "walking_rear_right";
                case 0x82:
                    return "walking_right";
                case 0x83:
                    return "walking_front_right";
                case 0x84:
                    return "walking_front";
                case 0x85:
                    return "walking_front_left";
                case 0x86:
                    return "walking_left";
                case 0x87:
                    return "walking_rear_left";
                default:
                    System.Diagnostics.Debug.Print("unknown animation" + animNo);
                    return "unknown_anim";
            }
        }

        public static int TranslateIndexToAnim(int animIndex)
        {
            switch (animIndex)
            {
                case 0:
                    return idle_combat;
                case 1:
                    return attack_bash;
                case 2:
                    return attack_slash;
                case 3:
                    return attack_thrust;
                case 4:
                    return attack_unk4;
                case 5:
                    return attack_secondary;
                case 6:
                    return attack_unk6;
                case 7:
                    return walking_towards;
                case 8:
                    return death;
                case 9:
                    return begin_combat;
                case 10:
                    return idle_rear;
                case 11:
                    return idle_rear_right;
                case 12:
                    return idle_right;
                case 13:
                    return idle_front_right;
                case 14:
                    return idle_front;
                case 15:
                    return idle_front_left;
                case 16:
                    return idle_left;
                case 17:
                    return idle_rear_left;
                case 18:
                    return walking_rear;
                case 19:
                    return walking_rear_right;
                case 20:
                    return walking_right;
                case 21:
                    return walking_front_right;
                case 22:
                    return walking_front;
                case 23:
                    return walking_front_left;
                case 24:
                    return walking_left;
                case 25:
                    return walking_rear_left;
                case 26:
                    return 80;
                case 27:
                    return 81;
                case 28:
                    return 82;
                case 29:
                    return 83;
                case 30:
                    return 84;
                case 31:
                    return 85;
                case 32:
                    return 86;
                case 33:
                    return 87;
                case 34:
                    return 40;
                case 35:
                    return 41;
                case 36:
                    return 42;
                case 37:
                    return 43;
                case 38:
                    return 44;
                case 39:
                    return 45;
                case 40:
                    return 46;
                case 41:
                    return 47;
                default:
                    return 0;
            }
        }


        public static int TranslateAnimToIndex(int animNo)
        {
            switch (animNo)
            {

                case idle_combat:
                    return 0;
                case attack_bash:
                    return 1;
                case attack_slash:
                    return 2;
                case attack_thrust:
                    return 3;
                case attack_unk4:
                    return 4;
                case attack_secondary:
                    return 5;
                case attack_unk6:
                    return 6;
                case walking_towards:
                    return 7;
                case death:
                    return 8;
                case begin_combat:
                    return 9;
                case idle_rear:
                    return 10;
                case idle_rear_right:
                    return 11;
                case idle_right:
                    return 12;
                case idle_front_right:
                    return 13;
                case idle_front:
                    return 14;
                case idle_front_left:
                    return 15;
                case idle_left:
                    return 16;
                case idle_rear_left:
                    return 17;
                case walking_rear:
                    return 18;
                case walking_rear_right:
                    return 19;
                case walking_right:
                    return 20;
                case walking_front_right:
                    return 21;
                case walking_front:
                    return 22;
                case walking_front_left:
                    return 23;
                case walking_left:
                    return 24;
                case walking_rear_left:
                    return 25;
                case 80://unknown anim 1
                    return 26;
                case 81://unknown anim 2
                    return 27;
                case 82://unknown anim 3
                    return 28;
                case 83://unknown anim 4
                    return 29;
                case 84://unknown anim 5
                    return 30;
                case 85://unknown anim 6
                    return 31;
                case 86://unknown anim 7
                    return 32;
                case 87://unknown anim 8
                    return 33;
                case 40://unknown anim 9
                    return 34;
                case 41://unknown anim 10
                    return 35;
                case 42://unknown anim 11
                    return 36;
                case 43://unknown anim 12
                    return 37;
                case 44://unknown anim 13
                    return 38;
                case 45://unknown anim 14
                    return 39;
                case 46://unknown anim 15
                    return 40;
                case 47://unknown anim 16
                    return 41;
                default:
                    return 0;
            }
        }

        public string DecimalToOct(string data)
        {
            if (data == "0")
            { return "00"; }
            string result = string.Empty;
            int num = int.Parse(data);
            while (num > 0)
            {
                int rem = num % 8;
                num /= 8;
                result = rem.ToString() + result;
            }
            if (result.Length == 1)
            {
                result = "0" + result;
            }
            return result;
        }


        /*	public static int TranslateAnimRangeToAnim(int animToTranslate)
            {
                    //Animations are clasified by number
                    switch (animToTranslate)
                    {
                    case NPC.AI_ANIM_IDLE_FRONT:
                            return idle_front;
                    case NPC.AI_ANIM_IDLE_FRONT_RIGHT:
                            return idle_front_right;
                    case NPC.AI_ANIM_IDLE_RIGHT:
                            return idle_right;
                    case NPC.AI_ANIM_IDLE_REAR_RIGHT:
                            return idle_rear_right;
                    case NPC.AI_ANIM_IDLE_REAR:
                            return idle_rear;
                    case NPC.AI_ANIM_IDLE_REAR_LEFT:
                            return idle_rear_left;
                    case NPC.AI_ANIM_IDLE_LEFT :
                            return idle_left;
                    case NPC.AI_ANIM_IDLE_FRONT_LEFT:
                            return idle_front_left;
                    case NPC.AI_ANIM_WALKING_FRONT:
                            return walking_front;
                    case NPC.AI_ANIM_WALKING_FRONT_RIGHT:
                            return walking_front_right;
                    case NPC.AI_ANIM_WALKING_RIGHT:
                            return walking_right;
                    case NPC.AI_ANIM_WALKING_REAR_RIGHT:
                            return walking_rear_right;
                    case NPC.AI_ANIM_WALKING_REAR :
                            return walking_rear;
                    case NPC.AI_ANIM_WALKING_REAR_LEFT :
                            return walking_rear_left;
                    case NPC.AI_ANIM_WALKING_LEFT :
                            return walking_left;
                    case NPC.AI_ANIM_WALKING_FRONT_LEFT:
                            return walking_front_left;
                    case NPC.AI_ANIM_DEATH:
                            return death;
                    case NPC.AI_ANIM_ATTACK_BASH:
                            return attack_bash;
                    case NPC.AI_ANIM_ATTACK_SLASH :
                            return attack_slash;
                    case NPC.AI_ANIM_ATTACK_THRUST:
                            return attack_thrust;
                    case NPC.AI_ANIM_COMBAT_IDLE:
                            return idle_combat;//is this right??
                    case NPC.AI_ANIM_ATTACK_SECONDARY:
                            return attack_secondary;
                    default:
                            return idle_front;
                    }	
            } */


        /// <summary>
        /// Read critter animation page files for UW2
        /// </summary>
        /// <param name="assocFile"></param>
        /// <param name="AuxPalNo"></param>
        /// <param name="fileCrit"></param>
        /// <param name="critanim"></param>
        /// <param name="spriteIndex"></param>
        /// <param name="paletteToUse"></param>
        /// <returns></returns>
        int ReadUW2PageFileData(byte[] assocFile, int AuxPalNo, string fileCrit, CritterAnimInfo critanim, int spriteIndex, Palette paletteToUse)
        {
            //Debug.Log(fileCrit + " starting at  "  + spriteIndex);
            Palette pal = paletteToUse;
            //byte[] auxpalval=new byte[32];
            //Palette[] auxpal = new Palette[32];
            //int auxPalNo = PaletteNo;
            int AddressPointer;


            //pal = new palette[256];
            //getPalette(PaletteFile, pal, 0);//always palette 0?

            ReadStreamFile(fileCrit, out byte[] critterFile);


            //UW2 uses a different method
            //Starting at offset 0x80
            //fprintf(LOGFILE, "\n\t//%s - palette = %d", fileCrit, auxPalNo);
            //auxPalNo=2;
            AddressPointer = 0;//auxPalNo * 32;

            byte[] auxPalVal = new byte[32];
            for (int j = 0; j < 32; j++)
            {
                auxPalVal[j] = (byte)getValAtAddress(critterFile, (AddressPointer) + (AuxPalNo * 32) + j, 8);
            }

            //int i = 0;
            int MaxWidth = 0;
            int MaxHeight = 0;
            int MaxHotSpotX = 0;
            int MaxHotSpotY = 0;

            for (int pass = 0; pass <= 1; pass++)
            {
                if (pass == 0)
                {//First pass is getting max image sizes
                    for (int index = 128; index < 640; index += 2)
                    {
                        int frameOffset = (int)getValAtAddress(critterFile, index, 16);
                        if (frameOffset != 0)
                        {
                            int BitMapWidth = (int)getValAtAddress(critterFile, frameOffset + 0, 8);
                            int BitMapHeight = (int)getValAtAddress(critterFile, frameOffset + 1, 8);
                            int hotspotx = (int)getValAtAddress(critterFile, frameOffset + 2, 8);
                            int hotspoty = (int)getValAtAddress(critterFile, frameOffset + 3, 8);
                            if (hotspotx > BitMapWidth) { hotspotx = BitMapWidth; }
                            if (hotspoty > BitMapHeight) { hotspoty = BitMapHeight; }
                            if (BitMapWidth > MaxWidth) { MaxWidth = BitMapWidth; }
                            if (BitMapHeight > MaxHeight) { MaxHeight = BitMapHeight; }
                            if (hotspotx > MaxHotSpotX) { MaxHotSpotX = hotspotx; }
                            if (hotspoty > MaxHotSpotY) { MaxHotSpotY = hotspoty; }
                        }//End frameoffsetr first pass
                    }//End for loop first pass

                    switch (fileCrit.Substring(fileCrit.Length - 7, 4).ToUpper())
                    {
                        case "CR02"://Rat. max height is calculated incorrectly
                            MaxHeight = 100;
                            break;
                    }
                }//End first pass
                else
                {//Extract images
                    if (MaxHotSpotX * 2 > MaxWidth)
                    {//Try and center the hot spot in the image.
                        MaxWidth = MaxHotSpotX * 2;
                    }
                    byte[] outputImg;
                    outputImg = new byte[MaxWidth * MaxHeight * 2];
                    for (int index = 128; index < 640; index += 2)
                    {
                        int frameOffset = (int)getValAtAddress(critterFile, index, 16);
                        if (frameOffset != 0)
                        {
                            int BitMapWidth = (int)getValAtAddress(critterFile, frameOffset + 0, 8);
                            int BitMapHeight = (int)getValAtAddress(critterFile, frameOffset + 1, 8);
                            int hotspotx = (int)getValAtAddress(critterFile, frameOffset + 2, 8);
                            int hotspoty = (int)getValAtAddress(critterFile, frameOffset + 3, 8);
                            int compression = (int)getValAtAddress(critterFile, frameOffset + 4, 8);
                            int datalen = (int)getValAtAddress(critterFile, frameOffset + 5, 16);
                            //Adjust the hotspots from the biggest point back to the image corners
                            int cornerX; int cornerY;
                            cornerX = MaxHotSpotX - hotspotx;
                            cornerY = MaxHotSpotY - hotspoty;
                            if (cornerX <= 0) { cornerX = 0; }
                            else { cornerX--; }
                            if (cornerY <= 0) { cornerY = 0; }

                            if (true)
                            {
                                //Merge the image into a new big image at the hotspot coordinates.;
                                byte[] srcImg;

                                srcImg = new byte[BitMapWidth * BitMapHeight * 2];
                                ArtLoader.Ua_image_decode_rle(critterFile, srcImg, compression == 6 ? 5 : 4, datalen, BitMapWidth * BitMapHeight, frameOffset + 7, auxPalVal);
                                cornerY = MaxHeight - cornerY;//y is from the top left corner

                                int ColCounter = 0; int RowCounter = 0;
                                bool ImgStarted = false;
                                for (int y = 0; y < MaxHeight; y++)
                                {
                                    for (int x = 0; x < MaxWidth; x++)
                                    {
                                        if ((cornerX + ColCounter == x) && (MaxHeight - cornerY + RowCounter == y) && (ColCounter < BitMapWidth) && (RowCounter < BitMapHeight))
                                        {//the pixel from the source image is here 
                                            ImgStarted = true;
                                            outputImg[x + (y * MaxWidth)] = srcImg[ColCounter + (RowCounter * BitMapWidth)];
                                            ColCounter++;
                                        }
                                        else
                                        {
                                            outputImg[x + (y * MaxWidth)] = 0;//alpha
                                        }
                                    }
                                    if (ImgStarted == true)
                                    {//New Row on the src image
                                        RowCounter++;
                                        ColCounter = 0;
                                    }
                                }
                                //Set the heights for output
                                BitMapWidth = MaxWidth;
                                BitMapHeight = MaxHeight;

                                Godot.Image imgData = ArtLoader.Image(outputImg, 0, BitMapWidth, BitMapHeight, "namehere", pal, true, true);
                                CropImageData(ref imgData, pal);
                                AnimInfo.animSprites[spriteIndex++] = imgData; //Sprite.Create(imgData, new Rect(0f, 0f, imgData.width, imgData.height), new Vector2(0.5f, 0.0f));
                            }
                        }//end extrac frameoffset
                    }//End for loop extract
                }//End extract images

            }
            //Debug.Log(fileCrit + " returning  "  + spriteIndex);
            return spriteIndex;


        }


        /// <summary>
        /// Is the animation unangled.
        /// </summary>
        /// <returns><c>true</c>, if animation unangled was ised, <c>false</c> otherwise.</returns>
        /// <param name="animationNo">Animation no.</param>
        bool isAnimUnAngled(int animationNo)
        {
            switch (animationNo)
            {
                case 0x2:
                case 0x3:
                case 0x4:
                case 0x5:
                case 0x6:
                case 0x7:
                case 0xd:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Translates a uw2 animation index into a uw1 version
        /// </summary>
        /// <returns>The U w2 animation.</returns>
        /// <param name="animation">Animation.</param>
        /// <param name="angle">Angle.</param>
        int GetUW2Anim(int animation, int angle)
        {
            {
                switch (animation)
                {
                    case 0x0://idlecombat
                        {
                            return 0x20 + angle;
                        }
                    case 0x1://walking
                        {
                            return 0x80 + angle;
                        }
                    case 0x2://Attacks
                    case 0x3:
                    case 0x4:
                        return animation - 1;
                    case 0x5://secondary attack
                    case 0x6://unknown attack
                        return animation;
                    case 0x7://death
                        return 0xc;
                    default:
                        return animation;
                }
            }
        }


        /// <summary>
        /// Crops the image data to remove all alpha space from beneath the npcs feet
        /// </summary>
        /// <param name="imgData">Image data.</param>
        /// <param name="PalUsed">Pal used.</param>
        static void CropImageData(ref Godot.Image imgData, Palette PalUsed)
        {
            return;// turn this off for the moment.
        //     Color alphacolor = PalUsed.ColorAtPixel(0, true);
        //     int InvalidRows = 0;//imgData.height;
        //     for (int x = 0; x < imgData.GetHeight(); x++)
        //     {
        //         bool rowIsAllAlpha = true;
        //         for (int y = 0; y <= imgData.GetWidth(); y++)
        //         {
        //             if (imgData.GetPixel(y, x) != alphacolor)
        //             {
        //                 rowIsAllAlpha = false;
        //                 break;
        //             }
        //         }
        //         if (rowIsAllAlpha)
        //         {
        //             InvalidRows++;
        //             for (int z = 0; z <= imgData.GetWidth(); z++)
        //             {
        //                 imgData.SetPixel(z, x, Color.Color8(0,0,0,0)) ;// .white);
        //             }
        //         }
        //         else
        //         {
        //             break;
        //         }
        //     }
        //     if ((InvalidRows < imgData.GetHeight()))
        //     {
        //         //TODO rebuild this section
        //         // Godot.Image newImg = new Godot.Image(imgData.GetWidth(), imgData.GetHeight() - InvalidRows, false);
        //         // newImg.SetPixels(imgData.GetPixels(0, InvalidRows, imgData.width, imgData.height - InvalidRows));
        //         // newImg.filterMode = FilterMode.Point;
        //         // newImg.Apply();
        //         // imgData = newImg;

        //         //imgData.Apply();
        //     }
         }
    }



public class CritterAnimInfo
{
    public const int NoOfAnims = 44;
    public string[,] animSequence;
    public int[,] animIndices;
    //public ArtLoader.RawImageData[,] animSprites;
    public Image[] animSprites;
    public string[] animName;

    public CritterAnimInfo()
    {
        animSequence = new string[NoOfAnims, 8];
        animIndices = new int[NoOfAnims, 8];
        switch (UWClass._RES)
        {
            case UWClass.GAME_UW2:
                animSprites = new Image[180];
                break;
            default:
                animSprites = new Image[128];
                break;
        }
        animName = new string[NoOfAnims];
    }
}



} //end namespace