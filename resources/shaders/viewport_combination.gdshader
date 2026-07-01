shader_type canvas_item;

uniform sampler2D viewport_1: source_color,filter_nearest; //: filter_linear_mipmap;//source_color,filter_nearest;
uniform sampler2D viewport_2: source_color,filter_nearest; //: filter_linear_mipmap;// source_color,filter_nearest;   //filter_linear_mipmap;
global uniform sampler2D xfer: source_color,filter_nearest; //xfer.dat transparency
global uniform sampler2D smoothpalette: source_color,filter_nearest; // pals.dat mapped into shades

void fragment() {
    // Sample the color from both viewports
    vec4 texWorld = texture(viewport_1, UV);//world
    vec4 texXfer = texture(viewport_2, UV);//xfers
    vec4 finalcolor;

    ivec3 indexfer = ivec3(round(texXfer.rgb * 255.0));
    ivec3 indexworld = ivec3(round(texWorld.rgb * 255.0));

    if (indexfer.r == 252)
    {
        //vec2(x, y) * TEXTURE_PIXEL_SIZE
        vec4 shade = texture(xfer, vec2(float(indexworld.r) / 255.0, 2.0/5.0));
        finalcolor.r = shade.r;     
    }
    else
    {
        finalcolor = texWorld;
    }
		
    //Get the uv distance offset within the shades.dat image
    float maptouse = 0.0;// dist/cutoffdistance;
    
    //lookup shades.dat data which is stored in smoothpalette using the uv	
    vec4 outputcolor = texture(smoothpalette, vec2(finalcolor.r, maptouse));

    // Combine the views: this simply averages the two textures
    COLOR.rgba = outputcolor.rgba; 
}