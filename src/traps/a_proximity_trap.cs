using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Trap that checks object is within the bounds of a defined area. Returns the next trap/trigger that runs depending on result.
    /// </summary>
    public class a_proximity_trap : trap
    {
        public static short Activate(uwObject trapObj, int triggerX, int triggerY, int character)
        {    
            bool result = false;
            int zpos;//of the character that activated the trap. Can in theory be a NPC/object.
            int xhome; int yhome;
            if (character==0)
            {
                zpos = playerdat.playerObject.zpos;
                xhome = playerdat.playerObject.tileX;
                yhome = playerdat.playerObject.tileY;

            }
            else
            {
                var obj = UWTileMap.current_tilemap.LevelObjects[character];
                zpos = obj.zpos;
                xhome = obj.tileX;
                yhome = obj.tileY;
            }

            Debug.Print($"Checking if character {character} at {xhome},{yhome},{zpos} is within bounds of {triggerX},{triggerY} to {triggerX+trapObj.quality},{triggerY+trapObj.owner}");
            
            if (xhome>=triggerX)
            {
                if (yhome>=triggerY)
                {
                    if (xhome <= triggerX + trapObj.quality)
                    {
                        if (yhome <= triggerY+ trapObj .owner)
                        {
                            if (trapObj.xpos!=0)
                            {
                                if (zpos>= trapObj.zpos)
                                {
                                    goto testresult;//object is above the zone
                                }
                            }
                            if (trapObj.ypos==0)
                            {
                                if (zpos<trapObj.zpos)
                                {
                                    goto testresult;// object is below the zone.
                                }
                            }
                            result = true;//within bounds
                        }
                    }
                }
            }

        testresult:
            if (result)
            {
                Debug.Print($"Proximity trap within bounds. Running link {trapObj.link}");
                return trapObj.link;
            }
            else
            {
                //test the next of the linked object.
                if (trapObj.link !=0)
                {                   
                    var linkobj = UWTileMap.current_tilemap.LevelObjects[trapObj.link];
                    Debug.Print($"Proximity trap outside bounds. Running next {linkobj.next}");
                    return linkobj.next;//the next is the action to do when not in bounds.
                }
            }
            return 0; // failed case.
        }
    }
}