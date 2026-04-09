using System.Diagnostics;
using Godot;
namespace Underworld
{
    /// <summary>
    /// Placeholder for handling the victory screen.
    /// </summary>
    public partial class uimanager : Node2D
    {
        public static void VictoryScreen()
        {
            Debug.Print("You have won the game!");
        }
    }
}