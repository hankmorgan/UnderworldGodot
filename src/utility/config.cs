using System.IO;
using System.Text.Json;
using System;
using Godot;

namespace Underworld
{
    public class uwsettings
    {
        public string pathuw1 { get; set; }
        public string pathuw2 { get; set; }
        public string gametoload { get; set; }
        public int level { get; set; }
        public int lightlevel { get; set; }
        public string levarkfolder { get; set; }
        public string shader { get; set; }
        public float FOV {get; set;}
        public static uwsettings instance;


   public static void LoadSettings()
    {
        var appfolder = OS.GetExecutablePath();
        appfolder = Path.GetDirectoryName(appfolder);
        var settingsfile = Path.Combine(appfolder, "uwsettings.json");

        if (!File.Exists(settingsfile))
        {
            OS.Alert("missing file uwsettings.json at " + settingsfile);
            return;
        }
        var gamesettings = JsonSerializer.Deserialize<uwsettings>(File.ReadAllText(settingsfile));
        uwsettings.instance = gamesettings;
        main.gamecam.Fov = Math.Max(50, uwsettings.instance.FOV);

        UWClass._RES = gamesettings.gametoload;
        switch (UWClass._RES)
        {
            case UWClass.GAME_UW1:
                UWClass.BasePath = gamesettings.pathuw1; break;
            case UWClass.GAME_UW2:
                UWClass.BasePath = gamesettings.pathuw2; break;
            default:
                throw new InvalidOperationException("Invalid Game Selected");
        }
    }


    } //end class
}//end namespace