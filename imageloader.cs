using Godot;
using System;
using System.Diagnostics;
using Underworld;

public partial class imageloader : Sprite2D
{
	// Called when the node enters the scene tree for the first time.

[Export] public TextureRect testTextureNode;
[Export] public MeshInstance3D mesh;

[Export] public Sprite3D spr;
[Export] public Sprite2D sprX;



	public override void _Ready()
	{
		PaletteLoader.LoadPalettes("C:/Games/UW1/game/UW/data/pals.dat");
		var byt = new Underworld.BytLoader();
		var img =  byt.LoadImageAt(1);

		// create the texture for the mesh
		ImageTexture tex=new();
		tex.SetImage(img); 
		//update the mesh with a new material.
		var material = mesh.GetActiveMaterial(0) as StandardMaterial3D; // or your shader...
		material!.AlbedoTexture = tex; // shader parameter, etc.
		material.TextureFilter=BaseMaterial3D.TextureFilterEnum.Linear;
		sprX.Texture=tex;
		spr.Texture=tex;
		
	
		}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
}
