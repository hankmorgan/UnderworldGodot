shader_type canvas_item;

uniform sampler2D viewport_1: source_color,filter_nearest; //: filter_linear_mipmap;//source_color,filter_nearest;
uniform sampler2D viewport_2: source_color,filter_nearest; //: filter_linear_mipmap;// source_color,filter_nearest;   //filter_linear_mipmap;

void fragment() {
    // Sample the color from both viewports
    vec4 tex1 = texture(viewport_1, UV);//world
    vec4 tex2 = texture(viewport_2, UV);//sprites
    vec4 finalcolor;
    if (int(tex2.r * 255.0) >= int(252))
    {
        //finalcolor.r = tex1.r;
        discard;
    }
    else
    {
        finalcolor = mix(tex1, tex2, 0.5); 
    }

    // Combine the views: this simply averages the two textures
    COLOR = finalcolor; ///mix(tex1, tex2, 0.5); 
}