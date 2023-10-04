shader_type canvas_item;
uniform sampler2D palette: repeat_enable, filter_nearest;
uniform sampler2D uwtexture: repeat_enable, filter_nearest;

void fragment() {
	// The original greyscaled texture.
	vec4 rgba = texture(uwtexture, UV);
	vec4 finalcolour = texture(palette, vec2(rgba.r,0));
	finalcolour.a = rgba.a;
	COLOR = finalcolour;
	}
