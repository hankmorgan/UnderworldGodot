using System.IO;
using Godot;
using System.Collections.Generic;
using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Class for critter animation info
    /// </summary>
    public class CritterArt : ArtLoader
    {
        public static CritterArt[] critterArt = new CritterArt[64];

        public Palette pal; //the game pal.
                            //private Palette auxpal;

        public Dictionary<string, CritterAnimation> Animations = new Dictionary<string, CritterAnimation>();
        public ImageTexture[] animSprites = new ImageTexture[256];

        /// <summary>
        /// Critter animations for UW1
        /// </summary>
        /// <param name="critter_id"></param>
        /// <param name="paletteToUse"></param>
        /// <param name="AuxPalNo"></param>
        public CritterArt(int critter_id, Palette paletteToUse, int AuxPalNo)
        {
            byte[] FilePage0;
            byte[] FilePage1;
            string critterIDO = DecimalToOct(critter_id.ToString());

            pal = paletteToUse;
            int spriteIndex = 0;
            for (int pass = 0; pass < 2; pass++)
            {
                //load in both page files.
                if (pass == 0)
                {//CR{CRITTER file ID in octal}PAGE.N{Page}
                    var toLoad = Path.Combine(BasePath, "CRIT", "CR" + critterIDO + "PAGE.N0" + pass);
                    ReadStreamFile(toLoad, out FilePage0);
                    spriteIndex = ReadPageFileUW1(FilePage0, critter_id, pass, spriteIndex, AuxPalNo);
                }
                else
                {
                    var toLoad = Path.Combine(BasePath, "CRIT", "CR" + critterIDO + "PAGE.N0" + pass);
                    ReadStreamFile(toLoad, out FilePage1);
                    bool LoadMod = Directory.Exists(toLoad);
                    ReadPageFileUW1(FilePage1, critter_id, pass, spriteIndex, AuxPalNo);
                }
            }
        }

        /// <summary>
        /// Critter animations for UW2
        /// </summary>
        /// <param name="critter_id"></param>
        /// <param name="paletteToUse"></param>
        /// <param name="palno"></param>
        /// <param name="assocData"></param>
        /// <param name="PGMP"></param>
        /// <param name="cran"></param>
        public CritterArt(int critter_id, Palette paletteToUse, int palno, byte[] PGMP, byte[] cran)
        {
            int ExtractPageNo = 0;
            string critterIDO = DecimalToOct(critter_id.ToString());
            int spriteIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                if ((int)getAt(PGMP, critter_id * 8 + i, 8) != 255)//Checks if a critter exists at this index in the page file.
                {
                    string ExtractPageNoO = DecimalToOct(ExtractPageNo.ToString());
                    string fileCrit = Path.Combine(BasePath, "CRIT", "CR" + critterIDO + "." + ExtractPageNoO);  // BasePath + sep + "CRIT" + sep + "CR" + critterIDO + "." + ExtractPageNoO;
                    spriteIndex = ReadUW2PageFileData(palno, fileCrit, spriteIndex, paletteToUse);
                    ExtractPageNo++;
                }
            }
            int cranAdd = (critter_id * 512);
            for (int Animation = 0; Animation < 8; Animation++)//The the animation slot
            {
                for (int Angle = 0; Angle < 8; Angle++)//Each animation has every possible angle.
                {
                    string newAnimName = GetUW2AnimName(Animation, Angle);
                    int[] newIndices = new int[8];
                    int ValidEntries = (int)getAt(cran, cranAdd + (Animation * 64) + (Angle * 8) + 7, 8);//Get how many valid frames are in the animation
                    for (int FrameNo = 0; FrameNo < 8; FrameNo++)
                    {
                        int currFrame = (int)getAt(cran, cranAdd + (Animation * 64) + (Angle * 8) + FrameNo, 8);
                        if (FrameNo < ValidEntries)
                        {
                            newIndices[FrameNo] = currFrame;
                        }
                        else
                        {
                            newIndices[FrameNo] = -1;
                        }
                    }

                    var newAnim = new CritterAnimation(newAnimName, newIndices);
                    Animations.Add(newAnimName, newAnim);
                }
            }
        }

        public static CritterArt GetCritter(int CritterToLoad)
        {
            if (critterArt[CritterToLoad] == null)
            {
                LoadCritter(CritterToLoad);
            }
            return critterArt[CritterToLoad];
        }


        public static void LoadCritter(int CritterToLoad)
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

        static void ReadUw1AssocFile(int CritterToLoad, string assocpath)
        {
            long AssocAddressPtr = 256;
            if (ReadStreamFile(assocpath, out byte[] assoc))
            {
                for (int ass = 0; ass <= 63; ass++)
                {
                    int FileID = (int)getAt(assoc, AssocAddressPtr++, 8);
                    int auxPal = (int)getAt(assoc, AssocAddressPtr++, 8);
                    if (ass == CritterToLoad)
                    {
                        critterArt[CritterToLoad] = new CritterArt(FileID, PaletteLoader.Palettes[0], auxPal);
                    }
                }
            }
        }

        static void ReadUW2AssocFile(int CritterToLoad)
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
                    int FileID = (int)getAt(assoc, AssocAddressPtr++, 8);
                    int auxPal = (int)getAt(assoc, AssocAddressPtr++, 8);
                    if (FileID != 255)
                    {
                        if (ass == CritterToLoad)
                        {
                            critterArt[CritterToLoad] = new CritterArt(FileID, PaletteLoader.Palettes[0], auxPal, pgmp, cran);
                        }
                    }
                }
            }
        }


        private int ReadPageFileUW1(byte[] PageFile, int XX, int YY, int spriteIndex, int AuxPalNo)
        {
            int addptr = 0;
            int slotbase = (int)getAt(PageFile, addptr++, 8);            
            int NoOfSlots = (int)getAt(PageFile, addptr++, 8);
            int[] SlotIndices = new int[NoOfSlots];
            int spriteCounter = 0;
            int slotCounter = 0;
             string XXo = DecimalToOct(XX.ToString());
             string YYo = DecimalToOct(YY.ToString());
            //Debug.Print($"{XXo} {YYo} has {slotbase} + {NoOfSlots}");
            for (int i = 0; i < NoOfSlots; i++)
            {//check if the slot is enabled
                int val = (int)getAt(PageFile, addptr++, 8);
                if (val != 255)
                {
                    SlotIndices[slotCounter++] = i;
                }
            }
            int NoOfSegs = (int)getAt(PageFile, addptr++, 8);
            for (int i = 0; i < NoOfSegs; i++)
            {
                string AnimName = GetUW1AnimName(slotbase + SlotIndices[i]);
                int[] newIndices = new int[8];

                for (int j = 0; j < 8; j++)
                {
                    int val = (int)getAt(PageFile, addptr++, 8);
                    if (val != 255)
                    {
                        newIndices[j] = val + spriteIndex;
                    }
                    else
                    {
                        newIndices[j] = -1;
                    }
                }
                var newanim = new CritterAnimation(AnimName, newIndices);
                Animations.Add(AnimName, newanim);
            }

            //Read in the palette
            int NoOfPals = (int)getAt(PageFile, addptr, 8);//Will skip ahead this far.
            addptr++;
            byte[] auxPalVal = new byte[32];
            for (int i = 0; i < 32; i++)
            {
                auxPalVal[i] = (byte)getAt(PageFile, (addptr) + (AuxPalNo * 32) + i, 8);
            }

            //Skip past the palettes
            addptr += NoOfPals * 32;
            int NoOfFrames = (int)getAt(PageFile, addptr, 8);
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
                        int frameOffset = (int)getAt(PageFile, addptr + (i * 2), 16);
                        int BitMapWidth = (int)getAt(PageFile, frameOffset + 0, 8);
                        int BitMapHeight = (int)getAt(PageFile, frameOffset + 1, 8);
                        int hotspotx = (int)getAt(PageFile, frameOffset + 2, 8);
                        int hotspoty = (int)getAt(PageFile, frameOffset + 3, 8);
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
                        int frameOffset = (int)getAt(PageFile, addptr + (i * 2), 16);
                        int BitMapWidth = (int)getAt(PageFile, frameOffset + 0, 8);
                        int BitMapHeight = (int)getAt(PageFile, frameOffset + 1, 8);
                        int hotspotx = (int)getAt(PageFile, frameOffset + 2, 8);
                        int hotspoty = (int)getAt(PageFile, frameOffset + 3, 8);
                        int compression = (int)getAt(PageFile, frameOffset + 4, 8);
                        int datalen = (int)getAt(PageFile, frameOffset + 5, 16);

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

                        //Extract the image
                        byte[] srcImg;
                        srcImg = new byte[BitMapWidth * BitMapHeight * 2];
                        outputImg = new byte[MaxWidth * MaxHeight * 2];
                        Ua_image_decode_rle(PageFile, srcImg, compression == 6 ? 5 : 4, datalen, BitMapWidth * BitMapHeight, frameOffset + 7, auxPalVal);


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

                        ImageTexture imgData = Image(
                            databuffer: outputImg, 
                            dataOffSet: 0, 
                            width: BitMapWidth, height: BitMapHeight,
                            palette: pal, 
                            useAlphaChannel: true, 
                            useSingleRedChannel: true);
                        CropImageData(ref imgData, pal);
                        this.animSprites[spriteIndex + i] = imgData;
                        spriteCounter++;
                    }

                }//endextract
            }
            return spriteCounter;
        }

        public static string GetAnimName (int animation, int angle)
        {
            if(_RES==GAME_UW2)
            {
                return GetUW2AnimName(animation,angle);
            }
            else
            {
                if ((animation>=0x20) && (animation<=0x27))
                {
                    return $"idle_{angleToString(angle)}";
                }
                if ((animation>=0x80) && (animation<=0x87))
                {
                    return $"walking_{angleToString(angle)}";
                }

                if ((animation>=0x80) && (animation<=0x87))
                {
                    return $"walking_{angleToString(angle)}";
                }
                return GetUW1AnimName(animation);
            }
        }

        /// <summary>
        /// Converts a hearing into the heading portion of the animation name
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static string angleToString(int angle)
        {
            //TODO: these angles may be incorrect.
            switch (angle)
            {
                case 0: return "left";
                case 1: return "rear_left";
                case 2: return "rear";
                case 3: return "rear_right";
                case 4: return "right";
                case 5: return "front_right";
                case 6: return "front";
                case 7: return "front_left";               
            }
            return "front";
        }

        public static string GetUW2AnimName(int animation, int angle)
        {
            /*
                  x*512 : start character x animation definition [C]
                Each chunk has 8 subchunks of 64 bytes. A subchunk [SC] describes the
                animation frames to take for a certain action. The actions are
                  [C]+0000 : [SC] standing 0
                  [C]+0040 : [SC] walking  1
                  [C]+0080 : [SC] in combat 2
                  [C]+00c0 : [SC] attack 3
                  [C]+0100 : [SC] attack 4
                  [C]+0140 : [SC] attack 5
                  [C]+0180 : [SC] attack 6
                  [C]+01c0 : [SC] dying 7
                */
            string output = $"UNKNOWNANIM_{animation}";
            switch (animation)
            {
                case 0:
                    output = "idle_"; break;
                case 1:
                    output = "walking_"; break;
                case 2:
                    output = "idle_combat_"; break;
                case 3:
                    output = "attack_bash_"; break;
                case 4:
                    output = "attack_slash_"; break;
                case 5:
                    output = "attack_stab_"; break;
                case 6:
                    output = "attack_secondary_"; break;
                case 7:
                    output = "death_"; break;

            }

            //[SC] +0000 : [AS] rear  0
            //[SC] + 0008 : [AS] rear right 1
            //[SC]+0010 : [AS] right  2
            //[SC] + 0018 : [AS] front right 3
            //[SC]+0020 : [AS] front  4
            //[SC] + 0028 : [AS] front left 5
            //[SC]+0030 : [AS] left  6
            //[SC] + 0038 : [AS] rear left 7

            switch (angle)
            {
                case 0:
                    output += "rear"; break;
                case 1:
                    output += "rear_right"; break;
                case 2:
                    output += "right"; break;
                case 3:
                    output += "front_right"; break;
                case 4:
                    output += "front"; break;
                case 5:
                    output += "front_left"; break;
                case 6:
                    output += "left"; break;
                case 7:
                    output += "rear_left"; break;
            }

            return output;
        }

        public static string GetUW1AnimName(int animNo)
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
                    return "attack_stab";
                //case 0x4:
                //    return "attack_unk4";
                case 0x5:
                    return "attack_secondary";
                //case 0x6:
                //    return "attack_unk6"; //does not exist
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
                // the following anims appear at different slots
                case 0x27:
                    return "idle_rear_left";
                case 0x28:
                    return "walking_rear";
                case 0x29:
                    return "walking_rear_right";
                case 0x2a:
                    return "walking_right";
                case 0x2b:
                    return "walking_front_right";
                case 0x2c:
                    return "walking_front";
                case 0x2d:
                    return "walking_front_left";
                case 0x2e:
                    return "walking_left";
                case 0x2f:
                    return "unknown_anim_47";
                case 0x50:
                    return "ethereal_anim_80";
                case 0x51:
                    return "ethereal_anim_81";
                case 0x52:
                    return "ethereal_anim_82";
                case 0x53:
                    return "ethereal_anim_83";
                case 0x54:
                    return "ethereal_anim_84";
                case 0x55:
                    return "ethereal_anim_85";
                case 0x56:
                    return "ethereal_anim_86";
                case 0x57:
                    return "ethereal_anim_87";
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
                    return "unknown_anim";
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


        /// <summary>
        /// Read critter animation page files for UW2
        /// </summary>
        /// <param name="AuxPalNo"></param>
        /// <param name="critterFilePath"></param>
        /// <param name="spriteIndex"></param>
        /// <param name="gamePalette"></param>
        /// <returns></returns>
        int ReadUW2PageFileData(int AuxPalNo, string critterFilePath, int spriteIndex, Palette gamePalette)
        {
            Palette pal = gamePalette;
            int AddressPointer;
            ReadStreamFile(critterFilePath, out byte[] critterFile);

            AddressPointer = 0;//auxPalNo * 32;

            byte[] auxPalVal = new byte[32];
            for (int j = 0; j < 32; j++)
            {
                auxPalVal[j] = (byte)getAt(critterFile, (AddressPointer) + (AuxPalNo * 32) + j, 8);
            }

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
                        int frameOffset = (int)getAt(critterFile, index, 16);
                        if (frameOffset != 0)
                        {
                            int BitMapWidth = (int)getAt(critterFile, frameOffset + 0, 8);
                            int BitMapHeight = (int)getAt(critterFile, frameOffset + 1, 8);
                            int hotspotx = (int)getAt(critterFile, frameOffset + 2, 8);
                            int hotspoty = (int)getAt(critterFile, frameOffset + 3, 8);
                            if (hotspotx > BitMapWidth) { hotspotx = BitMapWidth; }
                            if (hotspoty > BitMapHeight) { hotspoty = BitMapHeight; }
                            if (BitMapWidth > MaxWidth) { MaxWidth = BitMapWidth; }
                            if (BitMapHeight > MaxHeight) { MaxHeight = BitMapHeight; }
                            if (hotspotx > MaxHotSpotX) { MaxHotSpotX = hotspotx; }
                            if (hotspoty > MaxHotSpotY) { MaxHotSpotY = hotspoty; }
                        }//End frameoffsetr first pass
                    }//End for loop first pass

                    switch (critterFilePath.Substring(critterFilePath.Length - 7, 4).ToUpper())
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
                        int frameOffset = (int)getAt(critterFile, index, 16);
                        if (frameOffset != 0)
                        {
                            int BitMapWidth = (int)getAt(critterFile, frameOffset + 0, 8);
                            int BitMapHeight = (int)getAt(critterFile, frameOffset + 1, 8);
                            int hotspotx = (int)getAt(critterFile, frameOffset + 2, 8);
                            int hotspoty = (int)getAt(critterFile, frameOffset + 3, 8);
                            int compression = (int)getAt(critterFile, frameOffset + 4, 8);
                            int datalen = (int)getAt(critterFile, frameOffset + 5, 16);
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
                                Ua_image_decode_rle(critterFile, srcImg, compression == 6 ? 5 : 4, datalen, BitMapWidth * BitMapHeight, frameOffset + 7, auxPalVal);
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

                                ImageTexture imgData = Image(
                                    databuffer: outputImg, 
                                    dataOffSet: 0, 
                                    width: BitMapWidth, height: BitMapHeight, 
                                    palette: pal, 
                                    useAlphaChannel: true, 
                                    useSingleRedChannel: true);
                                CropImageData(ref imgData, pal);
                                this.animSprites[spriteIndex++] = imgData; //Sprite.Create(imgData, new Rect(0f, 0f, imgData.width, imgData.height), new Vector2(0.5f, 0.0f));
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
        /// Crops the image data to remove all alpha space from beneath the npcs feet
        /// </summary>
        /// <param name="imgData">Image data.</param>
        /// <param name="PalUsed">Pal used.</param>
        static void CropImageData(ref ImageTexture imgData, Palette PalUsed)
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


    /// <summary>
    /// Class for storing info about the animation sequence.
    /// </summary>
    public class CritterAnimation
    {
        public string animName;
        //public int[] animSequence = { -1, -1, -1, -1, -1, -1, -1, -1 };
        public int[] animIndices = { -1, -1, -1, -1, -1, -1, -1, -1 };
        public CritterAnimation(string _animName, int[] _indices)
        {
            animName = _animName;
            animIndices = _indices;
        }
    }//end class critteranimation


} //end namespace