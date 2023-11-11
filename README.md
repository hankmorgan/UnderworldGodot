# UnderworldGodot

A port of the old unity project to Godot. Lets see how this goes...

## Before you begin.
This is an experiment. No support is provided for it and usage is at your own risk


## Getting Started

No exe builds yet. This is just a glorified map viewer at this point.

1. Clone the repository
2. Install Godot https://godotengine.org/ and run it.
3. Save a file called ``uwsettings.json`` in the Godot Folder. See below for format of the file
4. Open the Project folder in Godot.
5. Run. It might work


This project is developed using VSCode using the C# Tools for Godot extensions.


## UWsettings

Enter optional paths for each game. Select the folder with the .exe file.
If using the gog versions extract the file ``game.gog`` using a tool like 7-zip and point to that folder.

To select maps to load.
1. Choose the game mode by editing the gametoload param
   1. UW1 or UW2 are the valid values.
2. Enter the level
3. Other values
   1. Light level- Shading in a range 0-7. (dark to not so dark)
   2. levarkfolder - Change from DATA to SAVE1 to SAVE4 to load savegames.
   3. Shader - Default shader used. Do not change.

```json
{
    "pathuw1": "C:\\Games\\UW1\\game\\UW",
    "pathuw2": "C:\\Games\\UW2IDA\\UW2",
    "gametoload": "UW2",
    "level": 0,
    "lightlevel" : 7,
    "levarkfolder" : "DATA",
    "shader" : "UWSHADER"
}
```