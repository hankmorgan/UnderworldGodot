using System.IO;
using System.Text.Json;
using System;
using Godot;
using System.Diagnostics;

namespace Underworld
{
    public class uwsettings
    {
        public string pathuw1 { get; set; }
        public string pathuw2 { get; set; }
        public string gametoload { get; set; }
        public int level { get; set; }

        public float FOV { get; set; }

        public bool showcolliders { get; set; }
        public static uwsettings instance;

        public static void Save()
        {
            var appfolder = OS.GetExecutablePath();
            appfolder = Path.GetDirectoryName(appfolder);
            var json = JsonSerializer.Serialize(instance);
            Debug.Print($"If I was to save settings now the value would be\n{json}");
        }

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

            setGame(gamesettings.gametoload);
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

        public static void setGame(string gamemode)
        {
            switch (gamemode.ToUpper())
            {
                case "UW2":
                case "2":
                    UWClass._RES = UWClass.GAME_UW2; break;
                case "UW1":
                case "1":
                    UWClass._RES = UWClass.GAME_UW1; break;                
                case "UWDEMO":
                case "0":
                    UWClass._RES = UWClass.GAME_UWDEMO; break;
            }
        }


    } //end class
}//end namespace