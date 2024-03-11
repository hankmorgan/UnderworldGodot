using Underworld;
using System.Diagnostics;
using Godot;

public partial class a_movetriggercollision:  Area3D
{
    
    public int uwObjectIndex;

    [Signal]
    public delegate void MoveTriggerEnteredEventHandler();

    public void movetrigger_entered(Node3D body)
    {
        
        Debug.Print($"{body.Name} collides with {uwObjectIndex}");
        if (body.Name == "Gronk")
        {
            use.SpellHasBeenCast = false;
            trap.ObjectThatStartedChain = uwObjectIndex;
            trigger.Move(null, uwObjectIndex, UWTileMap.current_tilemap.LevelObjects);
        }
        
    }
}