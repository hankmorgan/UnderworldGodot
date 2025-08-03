using System.IO;
using System.Text.Json;
using System;
using Godot;
using System.Diagnostics;

namespace Underworld;

public class uwsettings
{

    static readonly JsonSerializerOptions jsonOpts = new()
    {
        WriteIndented = true,
        IgnoreReadOnlyProperties = true,
        PropertyNameCaseInsensitive = true,
    };
    static readonly string filePath = Path.Join(
        Path.GetDirectoryName(
            OS.GetExecutablePath()),
        "settings.json");

    public static uwsettings instance;

    // This initialises our instance as soon as the class is loaded.
    static uwsettings() => LoadSettings();

    public static void LoadSettings()
    {

        if (File.Exists(filePath))
        {
            Debug.Print($"Loading settings from {filePath}");
            using var stream = File.OpenRead(filePath);
            instance = JsonSerializer.Deserialize<uwsettings>(stream, jsonOpts);
        }
        else
        {
            Debug.Print($"No existing settings at {filePath}. Loading defaults.");
            instance = new();
        }

        if (main.gamecam != null)
        {
            main.gamecam.Fov = Math.Max(50, instance.FOV);
        }

        switch (instance.gametoload.ToUpper())
        {
            case "UW2":
            case "2":
                UWClass._RES = UWClass.GAME_UW2;
                UWClass.BasePath = instance.pathuw1;
                break;
            case "UW1":
            case "1":
                UWClass._RES = UWClass.GAME_UW1;
                UWClass.BasePath = instance.pathuw2;
                break;
            case "UWDEMO":
            case "0":
                UWClass._RES = UWClass.GAME_UWDEMO;
                break;
            default:
                throw new InvalidOperationException("Invalid Game Selected");
        }

    }

    public string pathuw1 { get; set; } = "c:\\games";
    public string pathuw2 { get; set; } = "c:\\games";
    public string gametoload { get; set; } = "UW1";
    public int level { get; set; } = 0;
    public float FOV { get; set; } = 75;
    public bool showcolliders { get; set; }
    public int shaderbandsize { get; set; } = 8;

    public void Save()
    {
        Debug.Print($"Saving settings to {filePath}");
        using var stream = File.OpenWrite(filePath);
        JsonSerializer.Serialize(stream, this, jsonOpts);
    }

}
