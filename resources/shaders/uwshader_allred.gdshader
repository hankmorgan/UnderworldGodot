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
global uniform sampler2D simpleshade: source_color,filter_nearest;//shading data for objectinfo layer. used to find darkness

global uniform float cutoffdistance; // how far away the scene will render
global uniform bool renderwalls;
global uniform bool renderfloors;
global uniform bool renderceilings;


// tileFlags - indicates the tile surface that is being rendered on the infolayer
//        public const int fCEIL = 128;
//        public const int fSELF = 64;
//        public const int fNORTH = 32;
//        public const int fSOUTH = 16;
//        public const int fEAST = 8;
//        public const int fWEST = 4;
//        public const int fFLOOR = 2;        
//        public const int fBOTTOM = 1;
uniform int tileflags;

//The lower bits of the objects index. Used to identify the object in the info layer
uniform int objectindex_lowerbytes;

// the upper bits of the objects index. Used to identify the object in the info layer
uniform int objectindex_upperbytes;

void vertex() {
	UV=UV*uv1_scale.xy+uv1_offset.xy;
}


void fragment() {

	vec3 world_camera = (INV_VIEW_MATRIX * vec4(vec3(0.0), 1.0)).xyz;
	vec4 a = INV_VIEW_MATRIX * vec4(VERTEX, 1.0);
    float dist = (distance(a.xyz, world_camera));
	uint camera_layers = CAMERA_VISIBLE_LAYERS;

	if (dist > cutoffdistance)
	{		
		discard;
	}
	else
	{

		switch (camera_layers)
		{
			case 8u: //object info layer. 
			{				
				//contains tile information. (eg x,y and surface) or object index in the case of 3d models.
				//Used for pixel perfect raycasts.
				
				vec4 simple = texture(simpleshade, vec2(dist/cutoffdistance, 0.0));
				int shadelevel = ivec3(round(simple.rgb * 255.0)).r;
				if (shadelevel >= 112 )
				{
					discard;
				}
				else
				{
					ALBEDO.r = float(tileflags)/255.0;// tileflags indicating if this is a floor, NSEW facing wall, diagonal or ceiling. In game when this value >0 then I know I'm looking at this wall surface.
					ALBEDO.g = float(objectindex_lowerbytes)/255.0;   // this is either the objectindex bytes or tileX value. If tileflags is set it's tile, otherwise objectindex.
					ALBEDO.b = float(objectindex_upperbytes)/255.0;   // this is either the objectindex bytes or tileY value. If tileflags is set it's tile, otherwise objectindex.
				}
				break;
			}

			case 2u: // Layer GEO
			{
				vec4 lowdetail = texture(texture_albedo, vec2(0.0, 0.0)); //get pixel at 0,0 which is the lowdetail colour to use
				//Get the red channel original data.
				vec4 rgba;// = texture(texture_albedo, UV);

				//Check for low detail modes.
				if ((!renderwalls) && (((tileflags >= 4) && (tileflags<=64)) || (tileflags == 0)))
				{
					rgba = lowdetail;
				}
				else
				{
					if ((!renderceilings) && (tileflags == 128))
					{
						rgba = lowdetail;
					}
					else
					{
						if ((!renderfloors) && ((tileflags >= 1) && (tileflags<=2)))
						{
							rgba = lowdetail;
						}
					else
						{
							rgba = texture(texture_albedo, UV);
						}
					}
					
				}
				
				//Get the uv distance offset within the shades.dat image
				float maptouse = dist/cutoffdistance;

				rgba.g = maptouse; // green will indicate for the next shader what shading value in the lighting palettes to use for this pixel.
			
				ALBEDO = rgba.rgb; // This shader output becomes an input into viewport_combination.gdshader

				break;
			}
		}
	}
}