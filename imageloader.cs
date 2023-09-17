using Godot;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Underworld;

public partial class imageloader : Sprite2D
{
	// Called when the node enters the scene tree for the first time.

[Export] public TextureRect testTextureNode;
[Export] public MeshInstance3D mesh;

[Export] public Sprite3D sprite_3d;
[Export] public Sprite2D sprite_2d;



	public override void _Ready()
	{
		
		//Load palettes. run first
		PaletteLoader.LoadPalettes(Path.Combine(UWClass.BasePath,"data","pals.dat"));// "C:\\Games\\UW1\\game\\UW\\data\\pals.dat");

		var textureloader = new TextureLoader();
		var a_texture = textureloader.LoadImageAt(1);
		
		var bytloader = new Underworld.BytLoader();
		var a_bitmap =  bytloader.LoadImageAt(1);
	
		// create the texture for the mesh
		ImageTexture textureForMesh=new();
		textureForMesh.SetImage(a_texture);
		//textureForMesh.SetImage(a_bitmap);

		//update the mesh with a new material.
		var material = mesh.GetActiveMaterial(0) as StandardMaterial3D; // or your shader...
		material!.AlbedoTexture = textureForMesh; // shader parameter, etc.
		material.TextureFilter=BaseMaterial3D.TextureFilterEnum.Nearest;

		//Load a sprte and apply it to the 2d and 3d sprties
		GRLoader gr = new GRLoader(20);
		var a_sprite = gr.LoadImageAt(0);
		sprite_3d.TextureFilter=BaseMaterial3D.TextureFilterEnum.Nearest;
		sprite_2d.TextureFilter=CanvasItem.TextureFilterEnum.Nearest;

		ImageTexture sprite_texture=new();
		sprite_texture.SetImage(a_sprite);
	
		sprite_2d.Texture = sprite_texture;
		sprite_3d.Texture=sprite_texture;

		//Load strings
		//var strs = new StringLoader();
		StringLoader.LoadStringsPak(Path.Combine(UWClass.BasePath,"data","strings.pak"));
		Debug.Print(StringLoader.GetString(1,1));

		//Update a UI Element with a message
		GetTree().Root.GetNode("Node3D").GetNode<Button>("Button").Text= StringLoader.GetString(1,0);
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
}
