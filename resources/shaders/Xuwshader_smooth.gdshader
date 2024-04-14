// NOTE: Shader automatically converted from Godot Engine 4.1.1.stable.mono's StandardMaterial3D.

shader_type spatial;
render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_disabled,unshaded;
uniform vec4 albedo : source_color;
uniform float point_size : hint_range(0,128);
uniform float roughness : hint_range(0,1);
uniform sampler2D texture_metallic : hint_default_white,filter_linear_mipmap,repeat_enable;
uniform vec4 metallic_texture_channel;
uniform sampler2D texture_roughness : hint_roughness_r,filter_linear_mipmap,repeat_enable;
uniform float specular;
uniform float metallic;
uniform vec3 uv1_scale;
uniform vec3 uv1_offset;
uniform vec3 uv2_scale;
uniform vec3 uv2_offset;

//Shader Params
global uniform sampler2D uwpalette: source_color,filter_nearest;
uniform sampler2D texture_albedo : source_color,filter_nearest,repeat_enable;

global uniform sampler2D uwlightmap : source_color,filter_nearest,repeat_enable;
global uniform sampler2D shades : source_color,filter_nearest,repeat_enable;


global uniform float cutoffdistance;

void vertex() {
	UV=UV*uv1_scale.xy+uv1_offset.xy;

	//world_pos = MODEL_MATRIX * vec4(VERTEX, 1.0);  // get coords as world coords
    //vertexToCam = distance(world_pos.xyz, cameraPos);
    //distanceToCam = distance(centerPos, cameraPos);  // could be computed outside shader but easier to debug this way
}


void fragment() {

	vec2 world_camera = (INV_VIEW_MATRIX * vec4(vec3(0.0), 1.0)).xz;
	vec4 a = INV_VIEW_MATRIX * vec4(VERTEX, 1.0);
    float dist = (distance(a.xz, world_camera));

	//read in greyscale texture to get color index
	vec4 rgba = texture(texture_albedo, UV);	

	if (dist > cutoffdistance)
	{		
		discard;
	}
	else
	{
		vec4 brightshade = texture(shades, vec2(0,0));
		vec4 brightval = texture(uwlightmap, vec2(rgba.r, brightshade.r));	
		vec4 brightcolour = texture(uwpalette, vec2(brightval.r ,0));

		//Get the uv offset within the shades.dat image
		float maptouse = dist/cutoffdistance;

		//lookup shades.dat using the uv
		vec4 shade = texture(shades, vec2(maptouse,0));

		//Apply the shade returned from shades.dat for the lightmap
		vec4 lightval = texture(uwlightmap, vec2(rgba.r, shade.r));	

		//lookup the palette colour from the lightmap
		vec4 finalcolour = texture(uwpalette, vec2(lightval.r ,0));
		
		ALBEDO = mix (brightcolour,finalcolour, smoothstep(0, cutoffdistance, dist) ).rgb;
		//apply
		//ALBEDO = finalcolour.rgb;
	}
}