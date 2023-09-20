using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Underworld;

public partial class imageloader : Sprite2D
{
	// Called when the node enters the scene tree for the first time.

[Export] public TextureRect testTextureNode;
[Export] public MeshInstance3D mesh;

[Export] public Sprite3D sprite_3d;
[Export] public Sprite3D critter_3d;
[Export] public Sprite2D sprite_2d;

[Export] public Sprite2D weapon_2d;



	public override void _Ready()
	{
		

		UWClass._RES=UWClass.GAME_UW1;
		switch(UWClass._RES)
		{
			case UWClass.GAME_UW1:
				UWClass.BasePath = "C:\\Games\\UW1\\game\\UW"; break;
			case UWClass.GAME_UW2:
				UWClass.BasePath = "C:\\Games\\UW2\\game\\UW2"; break;
			default:
				throw new InvalidOperationException("Invalid Game Selected");				
		}
		
		Random rnd = new Random();
		var index = rnd.Next(8);
		Debug.Print (index.ToString());

		//Load palettes. run first
		PaletteLoader.LoadPalettes(Path.Combine(UWClass.BasePath,"data","pals.dat"));// "C:\\Games\\UW1\\game\\UW\\data\\pals.dat");

		var textureloader = new TextureLoader();
		var a_texture = textureloader.LoadImageAt(index);
		
		var bytloader = new Underworld.BytLoader();
		var a_bitmap =  bytloader.LoadImageAt(index);
	
		// create the texture for the mesh
		//ImageTexture textureForMesh=new();
		//textureForMesh.SetImage(a_texture);
		//textureForMesh.SetImage(a_bitmap);

		CreateMesh(a_texture);

		//update the mesh with a new material.
		var material = mesh.GetActiveMaterial(0) as StandardMaterial3D; // or your shader...
		material!.AlbedoTexture = a_bitmap; // shader parameter, etc.
		material.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;

		//Load a sprte and apply it to the 2d and 3d sprties
		GRLoader gr = new GRLoader(GRLoader.OBJECTS_GR);
		var a_sprite = gr.LoadImageAt(index);
		sprite_3d.TextureFilter=BaseMaterial3D.TextureFilterEnum.Nearest;
		sprite_2d.TextureFilter=CanvasItem.TextureFilterEnum.Nearest;

 		sprite_2d.Texture = a_sprite;
		sprite_3d.Texture = a_sprite;

		//Load strings
		GetTree().Root.GetNode("Node3D").GetNode<Button>("Button").Text= StringLoader.GetString(1,index);

		var weaponloader = new WeaponsLoader(0);
		var a_weapon = weaponloader.LoadImageAt(index);
		weapon_2d.TextureFilter=CanvasItem.TextureFilterEnum.Nearest;
		weapon_2d.Texture = a_weapon;

		var critloader = new CritLoader(0);
		var a_critter = critloader.critter.AnimInfo.animSprites[index];
		critter_3d.Texture=a_critter;
		critter_3d.TextureFilter=BaseMaterial3D.TextureFilterEnum.Nearest;

		var main_windowgr =  new GRLoader(GRLoader.ThreeDWIN_GR);
		var uielem = GetNode<TextureRect>("/root/Node3D/UI/3DWin");
		var ThreeDWinImg = bytloader.LoadImageAt(BytLoader.MAIN_BYT,true);

		uielem.Texture=ThreeDWinImg;
		uielem.TextureFilter=CanvasItem.TextureFilterEnum.Nearest;

		// var cuts = new CutsLoader(Path.Combine(UWClass.BasePath,"CUTS","CS000.N02"));
		// var cutimg = cuts.ImageCache[index];
		// // var cutstex = new ImageTexture();
		// // cutstex.SetImage(cutimg);
		// uielem.Texture=cutimg;
		// uielem.TextureFilter=CanvasItem.TextureFilterEnum.Nearest;
	}

	public void CreateMesh(Texture2D textureForMesh)
	{


		//var nd = new Node3D();
		//nd.Name = "yay";
		//GetTree().Root.AddChild;
		var surfaceArray = new Godot.Collections.Array();
		surfaceArray.Resize((int)Mesh.ArrayType.Max);

		var verts = new List<Vector3>();
		var uvs = new List<Vector2>();
		var normals = new List<Vector3>();
		var indices = new List<int>();

		var vert = new Vector3 (10f,10f,0f) ;
		verts.Add (vert);
		normals.Add(vert.Normalized());

		vert = new Vector3 (0f,0f,0f) ;
		verts.Add (vert);	
		normals.Add(vert.Normalized());
		
		vert = new Vector3 (0f,10f,5f) ;
		verts.Add (vert);
		normals.Add(vert.Normalized());

		uvs.Add(new Vector2(0,0));
		uvs.Add(new Vector2(1,0));
		uvs.Add(new Vector2(1,1));
		//uvs.Add(new Vector2(0,1));

		indices.Add(0);
		indices.Add(1);
		indices.Add(2);

		surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
		surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
		surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
		surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

		var arrMesh = new ArrayMesh(); //= Mesh as ArrayMesh;
		if (arrMesh != null)
		{
			// No blendshapes, lods, or compression used.
			arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

			var material = new StandardMaterial3D(); // or your shader...
			material!.AlbedoTexture = textureForMesh; // shader parameter, etc.
			material.TextureFilter=BaseMaterial3D.TextureFilterEnum.Nearest;
			arrMesh.SurfaceSetMaterial(0, material);

			var m = new MeshInstance3D();
			
    		m.Mesh=arrMesh;
			GetTree().Root.CallDeferred("add_child",m);
			//GetTree().Root.AddChild(m);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
}
