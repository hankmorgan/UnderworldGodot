using System.Collections.Generic;
using Godot;

namespace Underworld;

// <summary>
// This script node backs-up the initial launch scene for the game, allowing
// the user to configure paths and pick between UW 1 & 2.
// </summary>
public partial class LaunchScreen : Control
{

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

	// Allows easy storage and retrieval of mouse mode overrides.
	readonly Stack<Input.MouseModeEnum> mouseModeHistory = new();

	public override void _Ready()
	{
		var settings = uwsettings.instance;

		// Load initial paths from settings.
		PathUW1.Text = settings.pathuw1;
		PathUW2.Text = settings.pathuw2;

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
		mouseModeHistory.Push(Input.MouseMode);
		Input.MouseMode = Input.MouseModeEnum.Visible;

		// Finally display the
		GameFilesSelector.Show();

	}

	public void OnGameFilesSelectorSubmitted(string path)
	{
		var settings = uwsettings.instance;

		// Save the selected directory back, and clear the selection for
		// no good reason in particular.
		var selectedDir = System.IO.Path.GetDirectoryName(path);
		switch (UWClass._RES)
		{
			case UWClass.GAME_UWDEMO:
			case UWClass.GAME_UW1:
				PathUW1.Text = selectedDir;
				settings.pathuw1 = selectedDir;
				break;
			case UWClass.GAME_UW2:
				PathUW2.Text = selectedDir;
				settings.pathuw2 = selectedDir;
				break;
			default:
				// Non-blocking at this point. Just notify and do no more.
				GD.PushError("Invalid game selection: ", UWClass._RES);
				break;
		}

		// Revert the cursor. Yes I'm not checking that it's there first.
		Input.MouseMode = mouseModeHistory.Pop();

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

		var settings = uwsettings.instance;

		// Update settings and the current state.
		UWClass._RES = (byte)selection;
		switch (UWClass._RES)
		{
			case UWClass.GAME_UWDEMO:
			case UWClass.GAME_UW1:
				settings.gametoload = "UW1";
				UWClass.BasePath = settings.pathuw1;
				break;
			case UWClass.GAME_UW2:
				settings.gametoload = "UW2";
				UWClass.BasePath = settings.pathuw2;
				break;
			default:
				// Non-blocking at this point. Just notify and do no more.
				GD.PushError("Invalid game path selection: ", selection);
				return;
		}

		// Save any changes to our settings.
		settings.Save();

		// Switch scenes to start the game.
		GetTree().ChangeSceneToFile("res://scenes/Underworld.tscn");

	}

}
