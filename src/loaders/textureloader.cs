using System.IO;
using Godot;

namespace Underworld
{
/// <summary>
/// Loads textures.
/// </summary>
public class TextureLoader : ArtLoader
{
    static bool RenderGrey=true;

    private readonly string pathTexW_UW0 = "DW64.TR";
    private readonly string pathTexF_UW0 = "DF32.TR";
    private string pathTexW_UW1 = "W64.TR";
    private string pathTexF_UW1 = "F32.TR";
    private readonly string pathTex_UW2 = "T64.TR";
    //private readonly string pathTex_SS1 = "Texture.res";

    byte[] texturebufferW;
    byte[] texturebufferF;
    byte[] texturebufferT;

    public bool texturesWLoaded;
    public bool texturesFLoaded;
    private int TextureSplit = 210;//at what point does a texture index refer to the floor instead of a wall in uw1/demo
    private int FloorDim = 32;
    private readonly string ModPathW;
    private readonly string ModPathF;

    public const float BumpMapStrength = 1f;

    public TextureLoader()
    {
        switch (_RES)
        {
            // case GAME_SHOCK:
            //     break;
            case GAME_UW2:
                ModPathW = Path.Combine(BasePath, "DATA", pathTex_UW2.Replace(".", "_"));  // BasePath + pathTex_UW2.Replace(".", "_").Replace("--", sep.ToString());
                if (Directory.Exists(ModPathW))
                {
                    LoadMod = true;
                }
                break;
            case GAME_UWDEMO:
                ModPathW = Path.Combine(BasePath, "DATA", pathTexW_UW0.Replace(".", "_")); //BasePath + pathTexW_UW0.Replace(".", "_").Replace("--", sep.ToString());
                if (Directory.Exists(ModPathW))
                {
                    LoadMod = true;
                }
                ModPathF = Path.Combine(BasePath, "DATA", pathTexF_UW0.Replace(".", "_"));
                if (Directory.Exists(ModPathF))
                {
                    LoadMod = true;
                }
                break;
            case GAME_UW1:
                ModPathW = Path.Combine(BasePath, "DATA", pathTexW_UW1.Replace(".", "_"));
                if (Directory.Exists(ModPathW))
                {
                    LoadMod = true;
                }
                ModPathF = Path.Combine(BasePath, "DATA", pathTexF_UW1.Replace(".", "_"));
                if (Directory.Exists(ModPathF))
                {
                    LoadMod = true;
                }
                break;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextureLoader"/> class.
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="TextureType">Texture type. 0 = normal, 1 = palette cycled</param>
    public ImageTexture LoadImageAt(int index, short TextureType)
    {
        TextureType=0;
        switch (TextureType)
        {
            case 1: // Palette cycled
                return LoadImageAt(index, PaletteLoader.GreyScale);
            //case 2://modded normal map                
            //    return TGALoader.LoadTGA(Path.Combine(ModPathW, index.ToString("d3") + "_normal.tga"));
            default:
                return LoadImageAt(index, PaletteLoader.Palettes[0]);
        }
    }


    public override ImageTexture LoadImageAt(int index)
    {
        return LoadImageAt(index, PaletteLoader.Palettes[0]);
    }

    /// <summary>
    /// Loads the image at index.
    /// </summary>
    /// <returns>The <see cref="UnityEngine.Image"/>.</returns>
    /// <param name="index">Index.</param>
    /// <param name="palToUse">Pal to use.</param>
    /// If the index is greater than 209 I return a floor texture.
    public ImageTexture LoadImageAt(int index, Palette palToUse)
    {
        if (_RES == GAME_UWDEMO)
        {//Point the UW1 texture files to the demo files
            TextureSplit = 48;
            pathTexW_UW1 = pathTexW_UW0;
            pathTexF_UW1 = pathTexF_UW0;
        }
        if (_RES == GAME_UW2)
        {
            FloorDim = 64;
        }

        switch (_RES)
        {
               case GAME_UW2:
                {
                    // if (LoadMod)
                    // {
                    //     var toLoadMod = Path.Combine(ModPathW, index.ToString("d3") + ".tga");
                    //     if (File.Exists(toLoadMod))
                    //     {
                    //         return TGALoader.LoadTGA(toLoadMod);
                    //     }
                    // }
                    if (texturesFLoaded == false)
                    {
                        if (!ReadStreamFile(Path.Combine(BasePath, "DATA", pathTex_UW2), out texturebufferT))
                        {
                            return base.LoadImageAt(index);
                        }
                        else
                        {
                            texturesFLoaded = true;
                        }
                    }
                    long textureOffset = getValAtAddress(texturebufferT, ((index) * 4) + 4, 32);
                    return Image(texturebufferT, textureOffset, FloorDim, FloorDim, "name_goes_here", palToUse, false, RenderGrey);
                }


            case GAME_UWDEMO:
            case GAME_UW1:
            default:
                {
                    if (index < TextureSplit)
                    {//Wall textures
                        if (texturesWLoaded == false)
                        {
                            if (!ReadStreamFile(Path.Combine(BasePath, "DATA", pathTexW_UW1), out texturebufferW))
                            {
                                return base.LoadImageAt(index);
                            }
                            else
                            {
                                texturesWLoaded = true;
                            }
                        }
                        // if (LoadMod)
                        // {
                        //     var toLoadMod = Path.Combine(ModPathW, index.ToString("d3") + ".tga");
                        //     if (File.Exists(toLoadMod))
                        //     {
                        //         return TGALoader.LoadTGA(toLoadMod);
                        //     }
                        // }
                        long textureOffset = getValAtAddress(texturebufferW, (index * 4) + 4, 32);
                        return Image(texturebufferW, textureOffset, 64, 64, "name_goes_here", palToUse, false, RenderGrey);
                    }
                    else
                    {//Floor textures (to match my list of textures)
                        if (texturesFLoaded == false)
                        {
                            if (!ReadStreamFile(Path.Combine(BasePath, "DATA", pathTexF_UW1), out texturebufferF))
                            {
                                return base.LoadImageAt(index);
                            }
                            else
                            {
                                texturesFLoaded = true;
                            }
                        }
                        // if (LoadMod)
                        // {
                        //     var toLoadMod = Path.Combine(ModPathF, index.ToString("d3") + ".tga");
                        //     if (File.Exists(toLoadMod))
                        //     {
                        //         return TGALoader.LoadTGA(toLoadMod);
                        //     }
                        // }
                        long textureOffset = getValAtAddress(texturebufferF, ((index - TextureSplit) * 4) + 4, 32);
                        return Image(texturebufferF, textureOffset, FloorDim, FloorDim, "name_goes_here", palToUse, false, RenderGrey);
                    }
                }//end switch	
        }
    }



    // /// <summary>
    // /// Converts a Image into a normal map
    // /// </summary>
    // /// <returns>The map.</returns>
    // /// <param name="source">Source.</param>
    // /// <param name="strength">Strength.</param>
    // /// Sourced from http://jon-martin.com/?p=123
    // public static Image NormalMap(Image source, float strength)
    // {
    //     strength = Mathf.Clamp(strength, 0.0F, 10.0F);
    //     Image result;
    //     float xLeft;
    //     float xRight;
    //     float yUp;
    //     float yDown;
    //     float yDelta;
    //     float xDelta;
    //     result = new Image(source.width, source.height, TextureFormat.ARGB32, true);
    //     for (int by = 0; by < result.height; by++)
    //     {
    //         for (int bx = 0; bx < result.width; bx++)
    //         {
    //             xLeft = source.GetPixel(bx - 1, by).grayscale * strength;
    //             xRight = source.GetPixel(bx + 1, by).grayscale * strength;
    //             yUp = source.GetPixel(bx, by - 1).grayscale * strength;
    //             yDown = source.GetPixel(bx, by + 1).grayscale * strength;
    //             xDelta = ((xLeft - xRight) + 1) * 0.5f;
    //             yDelta = ((yUp - yDown) + 1) * 0.5f;
    //             result.SetPixel(bx, by, new Color(xDelta, yDelta, 1.0f, yDelta));
    //         }
    //     }
    //     result.Apply();
    //     return result;
    // }

    /// <summary>
    /// Returns the Mod path at the specified index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public string ModPath(int index)
    {
        switch (_RES)
        {
            // case GAME_SHOCK:
            //     {
            //         return "";
            //     }
            case GAME_UW2:
                {
                    return Path.Combine(ModPathW, index.ToString("d3") + ".tga");//  //ModPathW + sep + index.ToString("d3") + ".tga";
                }
            case GAME_UWDEMO:
            case GAME_UW1:
            default:
                {
                    if (index < TextureSplit)
                    {//Wall textures
                        return Path.Combine(ModPathW, index.ToString("d3") + ".tga"); // ModPathW + sep + index.ToString("d3") + ".tga";
                    }
                    else
                    {//Floor textures (to match my list of textures)
                        return Path.Combine(ModPathF, index.ToString("d3")); // ModPathF + sep + index.ToString("d3") + ".tga";
                    }
                }
        }//end switch	
    }
}

}