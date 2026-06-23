using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Godot;

namespace Underworld;

// <summary>
// This script node backs-up the initial launch scene for the game, allowing
// the user to configure paths and pick between UW 1 & 2.
// </summary>
public partial class LaunchMenu : Control
{

	private const string NextScene = "res://scenes/Underworld.tscn";

	[Export]
	public TextureRect SelectUW1;

	[Export]
	public TextEdit PathUW1;

	[Export]
	public TextureRect SelectUW2;

	[Export]
	public TextEdit PathUW2;

	[Export]
	public FileDialog GameFilesSelector;

	[Export]
	public OptionButton SoundOptionSelector;

	[Export]
	public TextEdit SynthPath;

	static bool LoadingSynthPath = false;

	// Allows easy storage and retrieval of mouse mode overrides.
	private readonly Stack<Input.MouseModeEnum> _mouseModeHistory = new();

	// Only need one reference to the current settings.
	private readonly uwsettings _uwSettings = uwsettings.instance;

	public override void _Ready()
	{
		//Debug.Print($"_Ready fired");

		// Load initial paths from settings.
		PathUW1.Text = _uwSettings.pathuw1;
		PathUW2.Text = _uwSettings.pathuw2;

		SynthPath.Text = _uwSettings.synthpath;
		for (int i = 0; i < SoundOptionSelector.ItemCount; i++)
		{
			if (SoundOptionSelector.GetItemText(i) == _uwSettings.synth)
			{
				SoundOptionSelector.Selected = i;
				break;
			}
		}

		// Set the initial focus selection.
		switch (UWClass._RES)
		{
			case UWClass.GAME_UWDEMO:
			case UWClass.GAME_UW1:
				SelectUW1.GrabFocus();
				break;
			case UWClass.GAME_UW2:
				SelectUW2.GrabFocus();
				break;
			default:
				// Non-blocking at this point. Just notify and do no more.
				GD.PushError("Invalid game path selection: ", UWClass._RES);
				return;
		}

		// Start loading the main scene as soon as practicable.
		var error = ResourceLoader.LoadThreadedRequest(NextScene, "PackedScene", true);
		GD.PushError($"{Enum.GetName(error)} while preloading main scene.");

	}

	public void _on_synth_path_gui_input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventMouseButton mouseButtonEvent:
				if (mouseButtonEvent.Pressed)
					break;
				return;
			default:
				return;
		}
		LoadingSynthPath = true;
		GameFilesSelector.CurrentPath = SynthPath.Text;
		GameFilesSelector.CurrentDir = SynthPath.Text;
		GameFilesSelector.Filters = [
			"*.*;Rom Files"
		];

		// Switch to the regular cursor, saving the previous state so we
		// can revert to it later.
		_mouseModeHistory.Push(Input.MouseMode);
		Input.MouseMode = Input.MouseModeEnum.Visible;

		// Finally display the selector
		GameFilesSelector.Show();
	}

	public void OnPathInput(InputEvent @event, int selection)
	{
		// Seems dumb at the moment, but this lets us process multiple
		// event types, such as InputEventKey. Feels like this should
		// be more abstract, though. Like focus and selection events
		// instead of binding to specific device inputs.
		switch (@event)
		{
			case InputEventMouseButton mouseButtonEvent:
				if (mouseButtonEvent.Pressed)
					break;
				return;
			default:
				return;
		}

		//Debug.Print($"OnPathInput: {@event}");
		LoadingSynthPath = false;
		// Select which path we're going to edit.
		UWClass._RES = (byte)selection;
		switch (UWClass._RES)
		{
			case UWClass.GAME_UWDEMO:
			case UWClass.GAME_UW1:
				GameFilesSelector.CurrentPath = PathUW1.Text;
				GameFilesSelector.CurrentDir = PathUW1.Text;
				GameFilesSelector.Filters = [
					"uw.exe;Stygian Abyss",
					"uwdemo.exe;Demo"
				];
				break;
			case UWClass.GAME_UW2:
				GameFilesSelector.CurrentPath = PathUW2.Text;
				GameFilesSelector.CurrentDir = PathUW2.Text;
				GameFilesSelector.Filters = [
					"uw2.exe;Labyrinth of Worlds",
				];
				break;
			default:
				// Non-blocking at this point. Just notify and do no more.
				GD.PushError("Invalid game path selection: ", selection);
				return;
		}

		// Switch to the regular cursor, saving the previous state so we
		// can revert to it later.
		_mouseModeHistory.Push(Input.MouseMode);
		Input.MouseMode = Input.MouseModeEnum.Visible;

		// Finally display the selector
		GameFilesSelector.Show();

	}

	public void OnGameFilesSelectorSubmitted(string path)
	{
		//Debug.Print($"GameFilesSelector submitted {path}");
		if (LoadingSynthPath)
		{
			if (System.IO.Path.Exists(path))
			{
				_uwSettings.synthpath = System.IO.Path.GetDirectoryName(path);
				_uwSettings.Save();
			}			
		}
		else
		{
			// Save the selected directory back, and clear the selection for
			// no good reason in particular.
			var selectedDir = System.IO.Path.GetDirectoryName(path);
			switch (UWClass._RES)
			{
				case UWClass.GAME_UWDEMO:
				case UWClass.GAME_UW1:
					PathUW1.Text = selectedDir;
					_uwSettings.pathuw1 = selectedDir;
					_uwSettings.Save();
					break;
				case UWClass.GAME_UW2:
					PathUW2.Text = selectedDir;
					_uwSettings.pathuw2 = selectedDir;
					_uwSettings.Save();
					break;
				default:
					// Non-blocking at this point. Just notify and do no more.
					GD.PushError("Invalid game selection: ", UWClass._RES);
					break;
			}
		}


		// Revert the cursor. Yes I'm not checking that it's there first.
		Input.MouseMode = _mouseModeHistory.Pop();

	}

	public void OnLaunchInput(InputEvent @event, int selection)
	{

		// Filter appropriate event triggers.
		switch (@event)
		{
			case InputEventMouseButton mouseButtonEvent:
				if (mouseButtonEvent.Pressed)
					break;
				return;
			default:
				return;
		}

		//Debug.Print($"OnPathInput: {@event}");

		// Update settings and the current state.
		UWClass._RES = (byte)selection;
		switch (UWClass._RES)
		{
			case UWClass.GAME_UWDEMO:
			case UWClass.GAME_UW1:
				if (File.Exists(System.IO.Path.Combine(_uwSettings.pathuw1, "UWDEMO.EXE")))
				{
					Debug.Print("UWDemo has been selected.");
					_uwSettings.gametoload = "UWDEMO";
					UWClass._RES = UWClass.GAME_UWDEMO;
				}
				else
				{
					_uwSettings.gametoload = "UW1";
				}
				UWClass.BasePath = _uwSettings.pathuw1;
				break;
			case UWClass.GAME_UW2:
				UWClass.BasePath = _uwSettings.pathuw2;
				_uwSettings.gametoload = "UW2";
				break;
			default:
				// Non-blocking at this point. Just notify and do no more.
				GD.PushError("Invalid game path selection: ", selection);
				return;
		}

		// Save any changes to our settings.
		_uwSettings.Save();

		// Switch scenes to start the game.
		var scene = (PackedScene)ResourceLoader.LoadThreadedGet(NextScene);
		GetTree().ChangeSceneToPacked(scene);

	}

	public void _on_sound_options_item_selected(int index)
	{
		_uwSettings.synth = SoundOptionSelector.GetItemText(index);
		_uwSettings.Save();
	}

}
