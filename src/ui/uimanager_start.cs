using System.Collections.Generic;
using Godot;

namespace Underworld;

public partial class uimanager : Node2D
{
    [ExportGroup("Startmenu")]
    [Export]
    public CanvasLayer StartMenuPanel;

    [Export]
    public TextEdit pathuw1;

    [Export]
    public TextEdit pathuw2;

    [Export]
    public FileDialog pathSelector;

    // Allows easy storage and retrieval of mouse mode overrides.
    Stack<Input.MouseModeEnum> mouseModeHistory = new();

    private void _Ready_Start()
    {
        var settings = uwsettings.instance;

        // Load initial paths from settings.
        pathuw1.Text = settings.pathuw1;
        pathuw2.Text = settings.pathuw2;

        // Customise our file selector.
        pathSelector.FileMode = FileDialog.FileModeEnum.OpenFile;

    }

    private void _on_path_input(InputEvent @event, int selection)
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
            case UWClass.GAME_UW1:
                pathSelector.CurrentPath = pathuw1.Text;
                pathSelector.CurrentDir = pathuw1.Text;
                pathSelector.Filters = [
                    "uw.exe;Stygian Abyss",
                ];
                break;
            case UWClass.GAME_UW2:
                pathSelector.CurrentPath = pathuw2.Text;
                pathSelector.CurrentDir = pathuw2.Text;
                pathSelector.Filters = [
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
        pathSelector.Show();

    }

    private void _on_game_select_file_dialog_file_selected(string path)
    {
        var settings = uwsettings.instance;

        // Save the selected directory back, and clear the selection for
        // no good reason in particular.
        var selectedDir = System.IO.Path.GetDirectoryName(path);
        switch (UWClass._RES)
        {
            case UWClass.GAME_UW1:
                pathuw1.Text = selectedDir;
                settings.pathuw1 = selectedDir;
                break;
            case UWClass.GAME_UW2:
                pathuw2.Text = selectedDir;
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

    private void _on_launch_input(InputEvent @event, int selection)
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

        // Save any changes to our settings, start the game.
        settings.Save();
        main.StartGame();

    }
    
}