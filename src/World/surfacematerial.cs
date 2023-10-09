using System.Collections.Generic;
using System.ComponentModel;
using Godot;

namespace Underworld
{
    /// <summary>
    /// Lists of texture materials/shaders for rendering
    /// </summary>
    public class surfacematerial : UWClass
    {
        public static TextureLoader textures;
        public static Shader textureshader;
        public static ImageTexture[] texturePalettes;
        public static surfacematerial[] surfaceMaterials = new surfacematerial[512];
        public int textureNo;

        private ShaderMaterial material;


        static surfacematerial()
        {
            if (textureshader == null)
            {//first time init
                textureshader = (Shader)ResourceLoader.Load("res://resources/shaders/uwshader.gdshader");
            }

            if (texturePalettes == null)
            {//init the shader palette
                CreateTexturePaletteCycles();
                RenderingServer.GlobalShaderParameterAdd("uwpalette", RenderingServer.GlobalShaderParameterType.Sampler2D, (Texture)texturePalettes[0]);
                RenderingServer.GlobalShaderParameterAdd("uwlightmapnear", RenderingServer.GlobalShaderParameterType.Sampler2D, (Texture)PaletteLoader.light[0].toImage());
                RenderingServer.GlobalShaderParameterAdd("uwlightmapfar", RenderingServer.GlobalShaderParameterType.Sampler2D, (Texture)PaletteLoader.light[15].toImage());
                RenderingServer.GlobalShaderParameterAdd("uwlightmapdark", RenderingServer.GlobalShaderParameterType.Sampler2D, (Texture)PaletteLoader.light[15].toImage());
                RenderingServer.GlobalShaderParameterAdd("fardistance", RenderingServer.GlobalShaderParameterType.Float, uwsettings.instance.drawdistance);
                RenderingServer.GlobalShaderParameterAdd("neardistance", RenderingServer.GlobalShaderParameterType.Float, 0);
            }
            textures=new TextureLoader();
        }

        public static Material Get(int textureno)
        {
            if (surfaceMaterials[textureno] == null)
            {
                surfaceMaterials[textureno] = new surfacematerial(textureno);
            }
            return surfaceMaterials[textureno].material;
        }

        public surfacematerial(int textureno)
        {
            textureNo = textureno;
            //create this material and add it to the list
            material = new ShaderMaterial();
            material.Shader = textureshader;
            material.SetShaderParameter("texture_albedo", (Texture)textures.LoadImageAt(textureno));
            material.SetShaderParameter("albedo", new Color(1, 1, 1, 1));
            material.SetShaderParameter("uv1_scale", new Vector3(1, 1, 1));
            material.SetShaderParameter("uv2_scale", new Vector3(1, 1, 1));
        }

        static void CreateTexturePaletteCycles(int paletteno = 0)
        {
            //copy initial palette

            var palCycler = new Palette();
            for (int i = 0; i < 256; i++)
            {
                palCycler.red = PaletteLoader.Palettes[paletteno].red;
                palCycler.green = PaletteLoader.Palettes[paletteno].green;
                palCycler.blue = PaletteLoader.Palettes[paletteno].blue;
            }

            texturePalettes = new ImageTexture[28];
            for (int c = 0; c <= 27; c++)
            {//Create palette cycles
                switch (_RES)
                {
                    case GAME_UW2:
                        Palette.cyclePalette(palCycler, 224, 16);
                        Palette.cyclePaletteReverse(palCycler, 3, 6);
                        break;
                    default:
                        Palette.cyclePalette(palCycler, 48, 16);//Forward
                        Palette.cyclePaletteReverse(palCycler, 16, 7);//Reverse direction.
                        break;
                }
                texturePalettes[c] = palCycler.toImage();
            }
        }


    }//end class

}//end namespace