using System.Diagnostics;
using Godot;

public partial class movetrigger:  Area3D
{
    
    public int uwObjectIndex;

    [Signal]
    public delegate void MoveTriggerEnteredEventHandler();

    public static void body_entered()
    {
        Debug.Print("YAY!");
    }

}