using System.Diagnostics;
using Godot;
namespace Underworld
{
    public class hack_trap : trap
    {
        public static bool CreateDoTrap(Node3D parent, uwObject trapObj, string name)
        {
            switch (trapObj.quality)
            {
                case 2://camera
                    {
                        return a_do_trap_camera.CreateDoTrapCamera(parent, trapObj, name);
                    }
            }
            return true;//unimplemented
        }

        public static bool ActivateHackTrap(uwObject trapObj, uwObject ObjectUsed, int triggerX, int triggerY, uwObject[] objList, int character, ref short triggerNextIndex)
        {
            switch (trapObj.quality)
            {
                case 2: //do trap camera
                    {
                        a_do_trap_camera.Activate(
                            trapObj: trapObj,
                            objList: objList
                        );
                        return true;
                    }
                case 3: //do trap platform                
                    {
                        a_do_trap_platform.Activate(
                            trapObj: trapObj,
                            triggerX: triggerX,
                            triggerY: triggerY
                        );
                        return true;
                    }
                case 4: //uw1 alternate do_trap_platform behaviour
                    {
                        if (_RES != GAME_UW2)
                        {
                            a_do_trap_platform.Activate(
                                trapObj: trapObj,
                                triggerX: triggerX,
                                triggerY: triggerY
                                );
                            return true;
                        }
                        break;
                    }
                case 5:// trespass trap
                    {
                        a_do_trap_trespass.Activate(
                            trapObj: trapObj,
                            objList: objList
                        );
                        return false;
                    }
                case 10://change class item
                    {
                        if (_RES == GAME_UW2)
                        {
                            a_hack_trap_classitem.Activate(
                                trapObj: trapObj,
                                triggerX: triggerX,
                                triggerY: triggerY,
                                objList: objList
                            );
                            return true;
                        }
                        break;
                    }
                case 11://fraznium forcefields
                    {
                        if (_RES == GAME_UW2)
                        {
                            a_hack_trap_forcefield.Activate(
                                trapObj: trapObj,
                                triggerX: triggerX,
                                triggerY: triggerY,
                                objList: objList
                            );
                            return true;
                        }
                        break;
                    }
                case 12://tile oscillator
                    {
                        if (_RES==GAME_UW2)
                        {
                            a_hack_trap_oscillator.Activate(
                                trapObj: trapObj,
                                triggerX: triggerX,
                                triggerY: triggerY,
                                objList: objList
                            );
                            return true;
                        }
                        break;
                    }
                case 14://texture cycle
                    {
                        if (_RES==GAME_UW2)
                        {
                            a_hack_trap_texturecycle.Activate(
                                trapObj: trapObj,
                                triggerX: triggerX,
                                triggerY: triggerY);
                            return true;
                        }
                        break;
                    }
                case 17: //Floor collapse
                    {
                        if (_RES==GAME_UW2)
                        {
                            a_hack_trap_floorcollapse.Activate(
                                trapObj: trapObj,
                                triggerX: triggerX,
                                triggerY: triggerY,
                                objList: objList
                            );
                            return true;
                        }
                        break;
                    }
                case 18://button triggering talorus
                {
                    if (_RES==GAME_UW2)
                    {
                        a_hack_trap_usebutton.Activate(
                            trapObj: trapObj, 
                            triggerX: triggerX, 
                            triggerY: triggerY);
                        return true;
                    }
                    break;
                }
                case 19://platforms reset in scintillus academy 7
                {
                    if (_RES==GAME_UW2)
                    {
                        a_hack_trap_platformreset.Activate(
                            trapObj: trapObj, 
                            triggerX: triggerX, 
                            triggerY: triggerY);
                        return true;
                    }
                    break;
                }
                case 20://rising platforms in scintillus academy 3
                {
                    if(_RES==GAME_UW2)
                    {
                        a_hack_trap_terraformplatforms.Activate(
                            trapObj: trapObj, 
                            triggerX: triggerX, 
                            triggerY: triggerY); 
                        return true;
                    }

                    break;
                }
                case 21://Change object zpos
                        {//used in the tombs?
                        if (_RES==GAME_UW2)
                        {
                            //TODO
                            a_hack_trap_resetzpos.Activate(trapObj: trapObj, 1);
                            return true;
                        }
                        break;
                    }
                case 22://Change object zpos
                    {
                        if (_RES==GAME_UW2)
                        {
                            a_hack_trap_resetzpos.Activate(trapObj: trapObj, mode: 0);
                            return true;
                        }
                        break;
                    }
                case 23://alternate index for change owner trap
                    {
                        if (_RES==GAME_UW2)
                        {
                            a_hack_trap_owner.Activate(trapObj: trapObj);
                            return true;
                        }  
                        break;
                    }
                case 24://bullfrog in UW1 and graffiti change in UW2
                    {
                        if (_RES != GAME_UW2)
                        {
                            a_do_trap_bullfrog.Activate(
                                trapObj: trapObj,
                                triggerX: triggerX,
                                triggerY: triggerY);
                            return true;
                        }
                        else
                        {
                            a_hack_trap_graffiti.Activate(
                                trapObj: trapObj, 
                                triggerX: triggerX, 
                                triggerY: triggerY);
                            return true;
                        }
                    }
                case 25://Bly Skup Chamber
                    {
                        if (_RES==GAME_UW2)
                        {
                            a_hack_trap_blyskup.Activate();
                            return true;
                        }
                        break;   
                    }
                case 26://Toggle forcefield
                    {
                        if (_RES==GAME_UW2)
                        {
                            a_hack_trap_toggleforcefield.Activate(
                                triggerX: triggerX, triggerY: triggerY);
                            return true;
                        }   
                        break;
                    }
                case 27://change quality trap
                    {
                        if (_RES==GAME_UW2)
                        {
                            a_hack_trap_quality.Activate(trapObj);
                            return true;
                        }
                        break;
                    }
                case 28://change owner trap
                    {
                        if (_RES==GAME_UW2)
                        {
                            a_hack_trap_owner.Activate(trapObj);
                            return true;
                        }
                        break;
                    }
                case 29://button flickering
                    {
                        if (_RES==GAME_UW2)
                        {
                            a_hack_trap_switchflicker.Activate(
                                triggerX: triggerX, 
                                triggerY: triggerY);
                            return true;
                        }
                        break;
                    }
                case 32://qbert in UW2
                    {
                        if (_RES == GAME_UW2)
                        {
                            a_hack_trap_qbert.Activate(
                                trapObj: trapObj,
                                objList: objList);
                            return true;
                        }
                        break;
                    }
                case 33: // bottle recycler in UW2
                    {
                        if (_RES==GAME_UW2)
                        {
                            a_hack_trap_recycler.Activate(
                                    trapObj: trapObj,
                                    triggerX: triggerX,
                                    triggerY: triggerY,
                                    objList: objList
                                );
                            return true;
                        }
                        break;
                    }
                case 35://recharges light sphere in UW2
                    {
                        if (_RES==GAME_UW2)
                        {
                            a_hack_trap_rechargelightsphere.Activate(triggerX, triggerY);
                            return true;
                        }
                        break; 
                    }
                case 38:
                    {//transform red potions to poison
                        if (_RES == GAME_UW2)
                        {
                            a_hack_trap_transformpotion.Activate(
                                triggerX: triggerX,
                                triggerY: triggerY);
                            return true;
                        }
                        break;
                    }
                case 39:
                    {//changes visibility of object
                        if (_RES == GAME_UW2)
                        {
                            a_hack_trap_visibility.Activate(
                                trapObj: trapObj
                                );
                            triggerNextIndex = 0; //stop chain
                            return true;
                        }
                        break;
                    }
                case 40: //vending machine uw2 (variant), emerald puzzle in UW1
                    {
                        if (_RES == GAME_UW2)
                        {
                            a_hack_trap_vending.Activate(
                                trapObj: trapObj,
                                triggerX: triggerX, triggerY: triggerY);                            
                        }
                        else
                        {
                            a_do_trap_emeraldpuzzle.Activate(
                                trapObj: trapObj,
                                triggerX: triggerX, triggerY: triggerY);

                        }
                        return true;
                    }
                case 41://vending machine uw2 (variant)
                    {
                        if (_RES == GAME_UW2)
                        {
                            a_hack_trap_vending.Activate(
                                trapObj: trapObj,
                                triggerX: triggerX, triggerY: triggerY);
                            return true;
                        }
                        break;
                    }
                case 42://vending machine uw2 (variant), UW1 talking door
                    {
                        if (_RES != GAME_UW2)
                        {
                            a_do_trap_conversation.Activate();//a talking door!
                            return true;
                        }
                        else
                        {
                            a_hack_trap_vending.Activate(
                                trapObj: trapObj,
                                triggerX: triggerX, triggerY: triggerY);
                            return true;
                        }
                    }
                case 43: // change goal and target
                    {
                        if (_RES==GAME_UW2)
                        {
                            a_hack_trap_changegoaltarget.Activate(trapObj: trapObj, character: character);
                        }
                        break;
                    }

                case 54://world gem rotation
                    {
                        if (_RES == GAME_UW2)
                        {
                            a_hack_trap_gemrotate.Activate();
                            return true;
                        }
                        break;
                    }
                case 55:// world gem transportation
                    {
                        if (_RES == GAME_UW2)
                        {
                            a_hack_trap_gemteleport.Activate();
                            return true;
                        }
                        break;
                    }
            }//end switch

            Debug.Print($"Unimplemented hack trap {trapObj.quality}");
            return false;
        }
    }//end class
}//end namespace