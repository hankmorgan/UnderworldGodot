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

//UW Shader Params

uniform sampler2D texture_albedo : source_color,filter_nearest,repeat_enable;// the source image
global uniform sampler2D smoothpalette: source_color,filter_nearest; // pals.dat mapped into shades
global uniform sampler2D xfer: source_color,filter_nearest; //xfer.dat transparency
global uniform float cutoffdistance; // how far away the scene will render

uniform int objectindex_lowerbytes;
uniform int objectindex_upperbytes;

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
	
	if (dist > cutoffdistance)
	{		
		discard;
	}
	else
	{
		uint camera_layers = CAMERA_VISIBLE_LAYERS;
		//read in greyscale texture to get color index		
		vec4 rgba = texture(texture_albedo, UV);
		vec4 alphacolour = texture(smoothpalette, vec2(rgba.r, 0));
		if ((camera_layers & 6u) != 0u)   //render on cameras 2 and 3
		{
			//render normally

			//Get the uv offset within the shades.dat image
			float maptouse = dist/cutoffdistance;
			rgba.g = maptouse;		
			ALBEDO = rgba.rgb; 
		}
		else
		{
			//Render object info on camera 4
			if ((camera_layers & 8u) != 0u)  
			{
				ALBEDO.r = 0.0;
				ALBEDO.g = float(objectindex_lowerbytes)/255.0; 
				ALBEDO.b = float(objectindex_upperbytes)/255.0;
			}
			else
			{
				ALBEDO.r = 0.5;
				ALBEDO.g = 0.5;
				ALBEDO.b = 0.5;
			}
		}

		ALPHA = alphacolour.a;//finalcolour.a;	
	}
}