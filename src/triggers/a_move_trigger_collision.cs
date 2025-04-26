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
        if (body.Name == "Gronk")
        {
            Debug.Print($"{body.Name} collides with {uwObjectIndex}");
           
            trigger.RunTrigger(
                character: 1, 
                ObjectUsed: null, 
                triggerType: (int)triggerObjectDat.triggertypes.MOVE, 
                TriggerObject: UWTileMap.current_tilemap.LevelObjects[uwObjectIndex],                
                objList: UWTileMap.current_tilemap.LevelObjects);
        
        }
        
    }
}