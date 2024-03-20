using System.Diagnostics;

namespace Underworld
{
    public class a_set_variable_trap : trap
    {
        public static void activate(uwObject triggerObj, uwObject trapObj)
        {
            switch (_RES)
            {
                case GAME_UW2:
                    {
                        var RightVariable = trapObj.heading;  //arg4
                        var Operator = trapObj.quality << 6 + trapObj.owner; //arg2
                        var LeftVariable = trapObj.xpos << 7 + trapObj.zpos; //arg0
                        VariableOperationUW2(LeftVariable, Operator, RightVariable);
                        break;
                    }
                default:
                    {
                        var LeftOperator = trapObj.zpos;
                        var RightOperator = (trapObj.quality << 5) | (int)trapObj.owner;
                        RightOperator <<= 3;
                        RightOperator |= (int)trapObj.ypos;
                        var Operation = trapObj.heading;
                        VariableOperationUW1(LeftOperator, Operation, RightOperator);

                        break;
                    }
            }

        }

        private static void VariableOperationUW1(short LeftOperator, short Operation, int RightOperator)
        {
            if (LeftOperator == 0)
            {// bitwise operations on quest variables. no known instances of this happening?
                Debug.Print ("Quest var bitwise operation");
            }
            else
            {
                switch (Operation)
                {
                    case 0: //add
                        {
                            var tmp = playerdat.GetGameVariable(LeftOperator);
                            tmp+= RightOperator;
                            playerdat.SetGameVariable(LeftOperator, tmp);
                            break;
                        }
                    case 1:
                        {   //SUB
                            var tmp = playerdat.GetGameVariable(LeftOperator);
                            tmp-= RightOperator;
                            playerdat.SetGameVariable(LeftOperator, tmp);
                            break;
                        }
                    case 2:
                        { //SET                            
                            playerdat.SetGameVariable(LeftOperator, RightOperator);
                            break;
                        }
                    case 3:
                        {
                            //AND
                            var tmp = playerdat.GetGameVariable(LeftOperator);
                            tmp &= RightOperator;
                            playerdat.SetGameVariable(LeftOperator, tmp);
                            break;
                        }
                    case 4:
                        {
                            //OR
                            var tmp = playerdat.GetGameVariable(LeftOperator);
                            tmp |= RightOperator;
                            playerdat.SetGameVariable(LeftOperator, tmp);
                            break;
                        }
                    case 5:
                        {//XOR
                            var tmp = playerdat.GetGameVariable(LeftOperator);
                            tmp ^= RightOperator;
                            playerdat.SetGameVariable(LeftOperator, tmp);
                            break;
                        }
                    case 6:
                        {  //LSHIFT
                            var tmp = playerdat.GetGameVariable(LeftOperator);
                            tmp <<= RightOperator;
                            playerdat.SetGameVariable(LeftOperator, tmp);
                            break;
                        }
                    default:
                        Debug.Print($"UNKNOWN OPERATOR {LeftOperator} {Operation} {RightOperator}");
                        break;
                }
            }
        }



        /// <summary>
        /// performs variable operations that change gamevars, xclock and quest vars.
        /// </summary>
        /// <param name="LeftVariable"></param>
        /// <param name="Operator"></param>
        /// <param name="RightVariable"></param>
        static void VariableOperationUW2(int LeftVariable, int Operator, int RightVariable)
        {
            Debug.Print($"{LeftVariable} {Operator} {RightVariable}");
        }
    }//end class
}//end namespace