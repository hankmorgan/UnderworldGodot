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
//Shader Params
//global uniform sampler2D uwnpc: source_color,filter_nearest;
uniform sampler2D texture_albedo : source_color,filter_nearest,repeat_enable;

//global uniform sampler2D uwlightmap : source_color,filter_nearest,repeat_enable;
//global uniform sampler2D shades : source_color,filter_nearest,repeat_enable;
uniform bool UseAlpha = false;
global uniform float cutoffdistance;
global uniform sampler2D smoothpalette: source_color,filter_nearest;

// Camera
//uniform vec3 centerPos;// = vec3(0.0, 0.0, 0.0);  // anything further from this pos from the camera will be discarded, needs to be updated by script each frame
//global uniform vec3 cameraPos;// = vec3(34.160301, 136.675003, 127.48799);  // position of the camera, needs to be updated by script each frame, this is the default start pos

varying float distanceToCam; // distance from cam to center
varying vec4 world_pos;  // of current vertex
varying float vertexToCam; // distance from current vertex to the camera



void vertex() {
	UV=UV*uv1_scale.xy+uv1_offset.xy;

	//https://ask.godotengine.org/152606/how-to-do-i-make-a-shader-a-billboard-face-the-player
	mat4 modified_model_view = VIEW_MATRIX * mat4(
        INV_VIEW_MATRIX[0],
        INV_VIEW_MATRIX[1],
        INV_VIEW_MATRIX[2],
        MODEL_MATRIX[3]
    );
    MODELVIEW_MATRIX = modified_model_view;
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
		//Get the uv offset within the shades.dat image
		float maptouse = dist/cutoffdistance;
		//lookup shades.dat using the uv
		//vec4 shade = texture(shades, vec2(maptouse,0));
		//Apply the shade returned from shades.dat for the lightmap
		//vec4 lightval = texture(uwlightmap, vec2(rgba.r, shade.r));	
		//lookup the palette colour from the lightmap
		//vec4 finalcolour = texture(uwnpc, vec2(lightval.r ,0));
		//apply
		vec4 finalcolour = texture(smoothpalette, vec2(rgba.r, maptouse));
		ALBEDO = finalcolour.rgb;
		//if (UseAlpha)
		//{
			ALPHA = finalcolour.a;
		//}		
	}
}