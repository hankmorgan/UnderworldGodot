using System.Collections.Generic;
using System.ComponentModel;
using Godot;

namespace Underworld
{
    /// <summary>
    /// Lists of texture materials/shaders for rendering
    /// </summary>
    // public class surfacematerial : UWClass
    // {
    //     public static TextureLoader textures;
    //    // public static Shader textureshader;
    //     public static ImageTexture[] texturePalettes;
    //     public static surfacematerial[] surfaceMaterials = new surfacematerial[512];
    //     public int textureNo;

    //     private ShaderMaterial material;


    //     static surfacematerial()
    //     {
    //         if (textureshader == null)
    //         {//first time init
    //             textureshader = (Shader)ResourceLoader.Load("res://resources/shaders/uwshader.gdshader");
    //         }

    //         if (texturePalettes == null)
    //         {//init the shader palette
    //             CreateTexturePaletteCycles(0);
    //             RenderingServer.GlobalShaderParameterAdd("uwpalette", RenderingServer.GlobalShaderParameterType.Sampler2D, (Texture)texturePalettes[0]);
    //             RenderingServer.GlobalShaderParameterAdd("uwlightmapnear", RenderingServer.GlobalShaderParameterType.Sampler2D, (Texture)PaletteLoader.light[0].toImage());
    //             RenderingServer.GlobalShaderParameterAdd("uwlightmapfar", RenderingServer.GlobalShaderParameterType.Sampler2D, (Texture)PaletteLoader.light[15].toImage());
    //             RenderingServer.GlobalShaderParameterAdd("uwlightmapdark", RenderingServer.GlobalShaderParameterType.Sampler2D, (Texture)PaletteLoader.light[15].toImage());
    //             //RenderingServer.GlobalShaderParameterAdd("fardistance", RenderingServer.GlobalShaderParameterType.Float, uwsettings.instance.drawdistance);
    //             RenderingServer.GlobalShaderParameterAdd("neardistance", RenderingServer.GlobalShaderParameterType.Float, 0);
            
    //             RenderingServer.GlobalShaderParameterAdd("cutoffdistance", RenderingServer.GlobalShaderParameterType.Float, 1.2f * shade.getShadeCutoff(uwsettings.instance.lightlevel));
    //             RenderingServer.GlobalShaderParameterAdd("uwlightmap", RenderingServer.GlobalShaderParameterType.Sampler2D, PaletteLoader.AllLightMaps(PaletteLoader.light));
    //             RenderingServer.GlobalShaderParameterAdd("shades", RenderingServer.GlobalShaderParameterType.Sampler2D, shade.shadesdata[uwsettings.instance.lightlevel].ToImage());
               
    //         }//shade.shadesdata[4].ToImage();
    //         textures=new TextureLoader();
    //     }

    //     // public static Material Get(int textureno)
    //     // {
    //     //     if (surfaceMaterials[textureno] == null)
    //     //     {
    //     //         surfaceMaterials[textureno] = new surfacematerial(textureno);
    //     //     }
    //     //     return surfaceMaterials[textureno].material;
    //     // }

    //     public surfacematerial(int textureno)
    //     {
    //         textureNo = textureno;
    //         //create this material and add it to the list
    //         material = new ShaderMaterial();
    //         material.Shader = textureshader;
    //         material.SetShaderParameter("texture_albedo", (Texture)textures.LoadImageAt(textureno));
    //         material.SetShaderParameter("albedo", new Color(1, 1, 1, 1));
    //         material.SetShaderParameter("uv1_scale", new Vector3(1, 1, 1));
    //         material.SetShaderParameter("uv2_scale", new Vector3(1, 1, 1));
    //     }

        


    // }//end class

}//end namespace