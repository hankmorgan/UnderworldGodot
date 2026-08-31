using System.IO;
using System.Text.Json;
using System;
using Godot;
using System.Diagnostics;

namespace Underworld;

public class uwsettings
{

	private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        IgnoreReadOnlyProperties = true,
        PropertyNameCaseInsensitive = true,
    };

	private static readonly string FilePath
		= ProjectSettings.GlobalizePath("user://settings.json");

    public static uwsettings instance;

    // This initialises our instance as soon as the class is loaded.
    static uwsettings() => LoadSettings();

    public static void LoadSettings()
    {

        switch (JsonFile.TryRead<uwsettings>(FilePath, JsonOpts, out var loaded, out string error))
        {
            case JsonReadOutcome.Ok:
                Debug.Print($"Loading settings from {FilePath}");
                instance = loaded;
                break;

            case JsonReadOutcome.NotFound:
                Debug.Print($"No existing settings at {FilePath}. Loading defaults.");
                instance = new();
                break;

            default:
                // This runs from the static constructor, so letting the failure out faults the
                // type for the whole process: every later read of uwsettings.instance throws,
                // BasePath is never set, and the game cannot start with nothing on screen to
                // say why. Keep the file so it can be looked at, and carry on with defaults.
                //
                // GD.PushWarning rather than Debug.Print: Debug.Print is [Conditional("DEBUG")]
                // and compiles out of a release build, and this is the one message that explains
                // why the settings just reverted.
                string kept = JsonFile.PreserveUnreadable(FilePath);
                GD.PushWarning($"Could not read {FilePath} ({error}). Loading defaults."
                    + (kept != null
                        ? $" The file was kept as {kept}."
                        : " The file could not be moved aside, so the next save will replace it."));
                instance = new();
                break;
        }

        if (main.cameraPitchGimbal_world != null)
        {
            main.cameraPitchGimbal_world.Fov = Math.Max(50, instance.FOV);
            main.cameraPitchGimbal_sprites.Fov = main.cameraPitchGimbal_world.Fov;
        }

        switch (instance.gametoload.ToUpper())
        {
            case "UW2":
            case "2":
                UWClass._RES = UWClass.GAME_UW2;
                UWClass.BasePath = instance.pathuw2;
                break;
            case "UW1":
            case "1":
                UWClass._RES = UWClass.GAME_UW1;
                UWClass.BasePath = instance.pathuw1;
                break;
            case "UWDEMO":
            case "0":
                UWClass._RES = UWClass.GAME_UWDEMO;
                UWClass.BasePath = instance.pathuw1;
                break;
            default:
                throw new InvalidOperationException("Invalid Game Selected");
        }

        // Backward compat: if legacy 'rompath' is set but new 'synthpath' isn't,
        // promote rompath to synthpath.
        if (string.IsNullOrEmpty(instance.synthpath) && !string.IsNullOrEmpty(instance.rompath))
        {
            instance.synthpath = instance.rompath;
            Debug.Print("Warning: 'rompath' setting is deprecated, use 'synthpath' instead.");
        }

    }

    public string pathuw1 { get; set; } = @"C:\Games\UW";
    public string pathuw2 { get; set; } = @"C:\Games\UW2";
    public string gametoload { get; set; } = "UW1";
    public int level { get; set; } = 0;
    public float FOV { get; set; } = 75;
    public bool showcolliders { get; set; }
    public int shaderbandsize { get; set; } = 8;
    public string synth { get; set; } = "soundfont";
    public string synthpath { get; set; } = "";
    // Legacy field, still read for backward compatibility. If set and synthpath is empty,
    // synthpath is populated from this in LoadSettings.
    public string rompath { get; set; } = "";

    public void Save()
    {
        Debug.Print($"Saving settings to {FilePath}");
        using var stream = File.Create(FilePath);
        JsonSerializer.Serialize(stream, this, JsonOpts);
    }

}
