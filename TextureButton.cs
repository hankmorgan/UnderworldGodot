using Godot;
using System;

public partial class TextureButton : Godot.TextureButton
{
	public static int xyz = 1;
		private float _speed = 400;
	private float _angularSpeed = Mathf.Pi;

	public override void _Process(double delta)
	{
		return;
		Rotation += _angularSpeed * (float)delta;
		var velocity = Vector2.Up.Rotated(Rotation) * _speed;
		Position += velocity * (float)delta;	
			
	}

private void _on_button_pressed()
{
	SetProcess(!IsProcessing());
}
	
}



