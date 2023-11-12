// NOTE: Shader automatically converted from Godot Engine 4.1.1.stable.mono's StandardMaterial3D.

shader_type canvas_item;

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
global uniform sampler2D uwpalette: source_color,filter_nearest;
uniform sampler2D texture_albedo : source_color,filter_nearest,repeat_enable;
uniform bool UseAlpha = false;

void vertex() {

}


void fragment(){
  vec4 rgba = texture(TEXTURE, UV); //read from texture  
  vec4 finalcolour = texture(uwpalette, vec2(rgba.r ,0));
  COLOR.rgba = finalcolour.rgba;
}