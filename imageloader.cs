using Godot;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
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
		//Load palettes. run first
		PaletteLoader.LoadPalettes("C:\\Games\\UW1\\game\\UW\\data\\pals.dat");


		var textureloader = new TextureLoader();
		var a_texture = textureloader.LoadImageAt(1);
		//var byt = new Underworld.BytLoader();
		//var bytimg =  byt.LoadImageAt(1);
		//Debug.Print("Format of the bitmap is " + spr.Texture.GetImage().GetFormat());
		
		// create the texture for the mesh
		ImageTexture tex=new();
		tex.SetImage(a_texture);
		
		//update the mesh with a new material.
		var material = mesh.GetActiveMaterial(0) as StandardMaterial3D; // or your shader...
		material!.AlbedoTexture = tex; // shader parameter, etc.
		material.TextureFilter=BaseMaterial3D.TextureFilterEnum.Nearest;


		GRLoader gr = new GRLoader(20);
		var spriteimg = gr.LoadImageAt(0);
		spr.TextureFilter=BaseMaterial3D.TextureFilterEnum.Nearest;
		sprX.TextureFilter=CanvasItem.TextureFilterEnum.Nearest;
		Debug.Print ("H" + spriteimg.GetHeight());
		ImageTexture texgr=new();	

		texgr.SetImage(spriteimg);
	
		Debug.Print ("A" + texgr.HasAlpha());
		sprX.Texture = texgr;
		spr.Texture=texgr;


		//Load strings
		var strs = new StringLoader();
		strs.LoadStringsPak("C:\\Games\\UW1\\game\\UW\\data\\strings.pak");
		Debug.Print(strs.GetString(1,1));
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
}
