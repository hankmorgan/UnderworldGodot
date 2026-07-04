shader_type canvas_item;

uniform sampler2D viewport_1: source_color,filter_nearest; //: filter_linear_mipmap;//source_color,filter_nearest;
uniform sampler2D viewport_2: source_color,filter_nearest; //: filter_linear_mipmap;// source_color,filter_nearest;   //filter_linear_mipmap;
global uniform sampler2D xfer: source_color,filter_nearest; //xfer.dat transparency
global uniform sampler2D smoothpalette: source_color,filter_nearest; // pals.dat mapped into shades
uniform int uwgame;

void fragment() {
    // Sample the color from both viewports
    vec4 texWorld = texture(viewport_1, UV);//world
    vec4 texXfer = texture(viewport_2, UV);//xfers
    vec4 finalcolor;

    ivec3 indexfer = ivec3(round(texXfer.rgb * 255.0));
    ivec3 indexworld = ivec3(round(texWorld.rgb * 255.0));

    //Get the uv distance offset within the shades.dat image
    float maptouse = texWorld.g; //0.0;// dist/cutoffdistance;
    
    float row = 0.0; //the row inthe 5x256 xfer image
    //rows map as follows
    //uw1 0 = shadows(251), 1= ?, 2 = white(252), 3 =? , 4 = green(?), 5 = blue(?)
    //uw2 0 = red, 1 =red(249), 2 = green, 3 = blue(251), 4 = white(252) 5 = shadows(253).

    if ((indexfer.r>=249) && (indexfer.r<=253))
    {
        switch(uwgame)
        {
            case 2:
                {
                    switch (indexfer.r)
                    {
                        case 248:
                            {
                                break;//not observed yet. Not included in conditions yet...
                            }
                        case 249:
                            {
                                row = 1.0;//done
                                break;
                            }
                        case 250:
                            {
                                row = 2.0;//not observed yet but assuming this does apply.
                                break;
                            }
                        case 251:
                            {
                                row = 3.0;//done
                                break;
                            }
                        case 252:
                            {
                                row = 4.0;//done
                                break;
                            }                            
                        case 253:
                            {
                                row = 5.0;//done
                                break;                                
                            }
                    }
                    break;
                }
            case 1: //uw1
            default:
                {
                    switch (indexfer.r)
                    {
                        //other cases are so far not seen in the wild..
                        case 251:
                            {
                                row = 1.0;//confirmed
                                break;
                            }
                        case 252:
                            {
                                row = 2.0;//confirmed.
                                break;
                            }
                            
                    }
                    break;
                }
        }
    }



    if ( ((indexfer.r>=249) && (indexfer.r<=253) && (uwgame == 2)) || ((indexfer.r>=251) && (indexfer.r<=252) && (uwgame == 1)))
    {
        vec4 shade = texture(xfer, vec2(float(indexworld.r) / 255.0, row/5.0));
        finalcolor.r = shade.r;     
    }
    else
    {
        finalcolor = texWorld;
    }	

    
    //lookup shades.dat data which is stored in smoothpalette using the uv	
    vec4 outputcolor = texture(smoothpalette, vec2(finalcolor.r, maptouse));

    // Combine the views: this simply averages the two textures
    COLOR.rgba = outputcolor.rgba; 
}