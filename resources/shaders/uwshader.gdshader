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


// Camera
//uniform vec3 centerPos;// = vec3(0.0, 0.0, 0.0);  // anything further from this pos from the camera will be discarded, needs to be updated by script each frame
//global uniform vec3 cameraPos;// = vec3(34.160301, 136.675003, 127.48799);  // position of the camera, needs to be updated by script each frame, this is the default start pos

varying float distanceToCam; // distance from cam to center
varying vec4 world_pos;  // of current vertex
varying float vertexToCam; // distance from current vertex to the camera



void vertex() {
	UV=UV*uv1_scale.xy+uv1_offset.xy;

	//world_pos = MODEL_MATRIX * vec4(VERTEX, 1.0);  // get coords as world coords
    //vertexToCam = distance(world_pos.xyz, cameraPos);
    //distanceToCam = distance(centerPos, cameraPos);  // could be computed outside shader but easier to debug this way
}


void fragment() {

    //if(vertexToCam > 25.0f){        
    //    discard;
    //}

	//vec3 world_camera = (INV_VIEW_MATRIX * vec4(vec3(0.0), 1.0)).xyz;
	//vec4 a = INV_VIEW_MATRIX * vec4(VERTEX, 1.0);
    //float dist = (distance(a.xyz, world_camera));
	//if(dist > 25.0f)
	//{
	//	discard;
	//}

	vec4 rgba = texture(texture_albedo, UV);
	vec4 finalcolour = texture(uwpalette, vec2(rgba.r ,0));
	
	
	//mix(vec3(0.0), c.rgb, brightness);
	//finalcolour.rgb = mix(vec3(0.0f), finalcolour.rgb, 1.0f/dist); //example brightness change
	
	
	ALBEDO = finalcolour.rgb;
	
	//float metallic_tex = dot(texture(texture_metallic,base_uv),metallic_texture_channel);
	//METALLIC = metallic_tex * metallic;
	//vec4 roughness_texture_channel = vec4(1.0,0.0,0.0,0.0);
	//float roughness_tex = dot(texture(texture_roughness,base_uv),roughness_texture_channel);
	//ROUGHNESS = roughness_tex * roughness;
	//SPECULAR = specular;
}