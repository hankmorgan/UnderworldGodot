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
global uniform sampler2D uwpalette: filter_nearest;
uniform sampler2D texture_albedo : source_color,filter_nearest,repeat_enable;

global uniform sampler2D uwlightmap : source_color,filter_nearest,repeat_enable;
global uniform sampler2D shades : source_color,filter_nearest,repeat_enable;
global uniform sampler2D shadeshift : source_color,filter_nearest,repeat_enable;

global uniform float cutoffdistance;


void vertex() {
	UV=UV*uv1_scale.xy+uv1_offset.xy;
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
		float step = cutoffdistance / 16.0;
		float bandstart = mod(dist, step) * step;	


		//Get the uv offset within the shades.dat image
		float maptouse = dist/cutoffdistance;
		//lookup shades.dat using the uv
		vec4 shade = texture(shades, vec2(maptouse,0));
		vec4 shadestart = texture(shades, vec2(bandstart/cutoffdistance,0));

		//Apply the shade returned from shades.dat for the lightmap
		vec4 lightval = texture(uwlightmap, vec2(rgba.r, shade.r));	
		vec4 lightvalstart = texture(uwlightmap, vec2(rgba.r, shadestart.r));


		//lookup the palette colour from the lightmap
		vec4 finalcolour = texture(uwpalette, vec2(lightval.r ,0));
		vec4 finalcolourstart = texture(uwpalette, vec2(lightvalstart.r ,0));
		

		ALBEDO = mix (finalcolourstart, finalcolour, smoothstep(0, step, dist-bandstart)).rgb;


		//apply
		//ALBEDO = finalcolour.rgb;
	}
}