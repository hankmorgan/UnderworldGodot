shader_type canvas_item;

uniform sampler2D viewport_1: source_color,filter_nearest; //: filter_linear_mipmap;//source_color,filter_nearest;
uniform sampler2D viewport_2: source_color,filter_nearest; //: filter_linear_mipmap;// source_color,filter_nearest;   //filter_linear_mipmap;
global uniform sampler2D xfer: source_color,filter_nearest; //xfer.dat transparency


void fragment() {
    // Sample the color from both viewports
    vec4 texWorld = texture(viewport_1, UV);//world
    vec4 texXfer = texture(viewport_2, UV);//xfers
    vec4 finalcolor;

    ivec3 indexfer = ivec3(round(texXfer.rgb * 255.0));
    ivec3 indexgeo = ivec3(round(texWorld.rgb * 255.0));

    if (indexfer.r == 252)
    {
        vec4 shade = texture(xfer, vec2(texXfer.r, 0));
        finalcolor.r = shade.r;
    }
    else
    {
        finalcolor = texWorld;
    }

    // Combine the views: this simply averages the two textures
    COLOR = finalcolor; 
}