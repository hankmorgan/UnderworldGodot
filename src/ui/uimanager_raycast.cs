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
                        if (playerdat.TelekenesisEnchantment)
                            {
                                return 10f;
                            }
                            else
                            {
                                return 3f;
                            }    
                    case InteractionModes.ModeUse:
                        if (playerdat.usingpole)
                        {
                            return 6f;
                        }   
                        else
                        {
                            if (playerdat.TelekenesisEnchantment)
                            {
                                return 10f;
                            }
                            else
                            {
                                return 3f;
                            }                            
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
        public static Dictionary DoRayCast(Vector2 eventMousePosition, float RayLength, out Vector3 RayOrigin)   //InputEventMouseButton eventMouseButton, float RayLength)
        {            
            var MainPos =  eventMousePosition; //eventMouseButton.Position;
            var mousepos  = instance.uwsubviewport.GetMousePosition();

            var mouselook = (bool)main.gamecam.Get("MOUSELOOK");
            if (mouselook)
            {//get centred mouse cursor positions when in mouselook mode to account for off-centre vanilla ui
                MainPos = mouseCursor.CursorPosition;
                mousepos = mouseCursor.CursorPositionSub;
            }

            var from = main.gamecam.ProjectRayOrigin(MainPos);
            RayOrigin = from;
            var to = from + main.gamecam.ProjectRayNormal(mousepos) * RayLength;
            var query = PhysicsRayQueryParameters3D.Create(from, to);
            var spaceState = main.instance.GetWorld3D().DirectSpaceState;
            var result = spaceState.IntersectRay(query);
            return result;
        }
    }   //end class
}//end namespace