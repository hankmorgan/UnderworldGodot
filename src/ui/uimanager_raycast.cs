using Godot.Collections;
using Godot;
namespace Underworld
{
    public partial class uimanager : Node2D
    {
        public static float RayDistance
        {
            get
            {
                if (SpellCasting.currentSpell != null)
                {
                    if (SpellCasting.currentSpell.SpellMajorClass == 5)
                    {
                        return 1f;
                    }
                    else
                    {
                        return 3f;
                    }

                }
                switch (InteractionMode)
                {
                    case InteractionModes.ModeLook:
                        return 16f;
                    case InteractionModes.ModeTalk:
                        return 8f;
                    case InteractionModes.ModePickup:
                        return 3f;
                    case InteractionModes.ModeUse:
                        if (playerdat.usingpole)
                        {
                            return 6f;
                        }   
                        else
                        {
                            return 3f;
                        } 
                    case InteractionModes.ModeAttack:
                        return 1f;
                }
                return 0;
            }
        }

        /// <summary>
        /// Does a raycast of specified length from the mouse event position.
        /// </summary>
        /// <param name="eventMouseButton"></param>
        /// <param name="RayLength"></param>
        /// <returns></returns>
        public static Dictionary DoRayCast(InputEventMouseButton eventMouseButton, float RayLength)
        {
            var from = main.gamecam.ProjectRayOrigin(eventMouseButton.Position);
            var mousepos = uimanager.instance.uwsubviewport.GetMousePosition(); //eventMouseButton.Position
            var to = from + main.gamecam.ProjectRayNormal(mousepos) * RayLength;
            var query = PhysicsRayQueryParameters3D.Create(from, to);
            var spaceState = main.instance.GetWorld3D().DirectSpaceState;
            var result = spaceState.IntersectRay(query);
            return result;
        }
    }   //end class
}//end namespace