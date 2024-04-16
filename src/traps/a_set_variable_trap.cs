using System.Diagnostics;

namespace Underworld
{
    public class a_set_variable_trap : trap
    {
        public static void Activate(uwObject trapObj)
        {
            switch (_RES)
            {
                case GAME_UW2:
                    {
                        var Operator = trapObj.heading;  //arg4
                        var RightVariable = ((trapObj.quality & 0x3F) << 6) + (trapObj.owner & 0x3F); //arg2
                        var LeftVariable = (trapObj.xpos << 7) + trapObj.zpos; //arg0                       
                        VariableOperationUW2(LeftVariable, Operator, RightVariable);
                        if (trapObj.ypos != 0)
                        {
                            //increment xclock 15
                            var tmp = playerdat.GetXClock(15);
                            tmp++;
                            playerdat.SetXClock(15, tmp);
                        }
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

        /// <summary>
        /// Performs a change to the specified game variable
        /// </summary>
        /// <param name="LeftOperator"></param>
        /// <param name="Operation"></param>
        /// <param name="RightOperator"></param>
        private static void VariableOperationUW1(short LeftOperator, short Operation, int RightOperator)
        {
            if (LeftOperator == 0)
            {// bitwise operations on quest variables. no known instances of this happening?
                Debug.Print("Quest var bitwise operation");
                var questToChange = (1 << RightOperator);
                switch (Operation)
                {
                    case 1://Unset
                        {
                            playerdat.SetQuest(questToChange, 0);
                            break;
                        }
                    case 5://XOR
                        {
                            var tmp = playerdat.GetQuest(questToChange);
                            tmp ^= 1;
                            playerdat.SetQuest(questToChange, tmp);
                            break;
                        }
                    default://set bit
                        {
                            playerdat.SetQuest(questToChange, 1);
                            break;
                        }
                }
            }
            else
            {
                switch (Operation)
                {
                    case 0: //add
                        {
                            var tmp = playerdat.GetGameVariable(LeftOperator);
                            tmp += RightOperator;
                            playerdat.SetGameVariable(LeftOperator, tmp);
                            break;
                        }
                    case 1:
                        {   //SUB
                            var tmp = playerdat.GetGameVariable(LeftOperator);
                            tmp -= RightOperator;
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
        /// <param name="Operation"></param>
        /// <param name="RightVariable"></param>
        static void VariableOperationUW2(int LeftVariable, int Operation, int RightVariable)
        {
            Debug.Print($"L:{LeftVariable} O:{Operation} R:{RightVariable}");
            switch (LeftVariable)
            {
                case < 0x100:
                    { //game variables

                        var gamevar = playerdat.GetGameVariable(LeftVariable);
                        var newValue = VariableTransformUW2(gamevar, Operation, RightVariable);
                        playerdat.SetGameVariable(LeftVariable, newValue & 0x3FF);

                        // var gamevaroffset = (LeftVariable<<1) + 0x0FA;                        
                        // var gamevar = playerdat.pdat[(LeftVariable<<1) + 0x0FA];
                        // //var gamevar = playerdat.GetGameVariable(LeftVariable);
                        // var newValue = VariableTransformUW2(gamevar, Operation, RightVariable);
                        // playerdat.pdat[(LeftVariable<<1) + 0x0FA] = (byte)newValue;
                        // Debug.Print($"Setting gamevar at offset {gamevaroffset} from {gamevar} to {newValue}");
                        // //playerdat.SetGameVariable(LeftVariable, newValue & 0x3FF);
                        break;
                    }
                case >= 0x100 and < 0x180:
                    {//Quest vars
                        var questno = LeftVariable - 0x100;// >> 2;
                        switch (Operation)
                        {   //assuming these cases are the same as UW1. the dissasembly is complex so I could be wrong here
                            case 1://maybe unset
                                {
                                    playerdat.SetQuest(questno,0);
                                    break;
                                }
                            case 5://maybe xor
                                {
                                    var tmp = playerdat.GetQuest(questno);
                                    tmp ^= 1;
                                    playerdat.SetQuest(questno, tmp);
                                    break;
                                }
                            default://maybe set
                                {
                                    playerdat.SetQuest(questno,1);
                                    break;
                                }
                        }
                        break;
                    }
                case >=0x180 and <0x190:
                    {
                        //quest 128 and up
                        var questvar = playerdat.GetQuest(128 + LeftVariable-0x180);
                        var newValue = VariableTransformUW2(questvar, Operation,RightVariable);
                        playerdat.SetQuest(128 + LeftVariable-0x180, newValue);
                        break;
                    }
                case >=0x190 and <0x200:
                    {
                        //xclock
                        var xclockvar = playerdat.GetXClock(LeftVariable-0x190);
                        var newValue = VariableTransformUW2(xclockvar, Operation,RightVariable); 
                        playerdat.SetXClock(LeftVariable-0x190, newValue);
                        break;
                    }
                default:
                    {
                        Debug.Print ("Invalid Operation {LeftOperator} {Operation} {RightOperator}");
                        break;
                    }
            }
        }


        /// <summary>
        /// UW2 variable operation calculations. Has some slight differences to UW1
        /// </summary>
        /// <param name="LeftOperator"></param>
        /// <param name="Operation"></param>
        /// <param name="RightOperator"></param>
        /// <returns></returns>
        public static int VariableTransformUW2(int LeftOperator, int Operation, int RightOperator)
        {
            switch (Operation)
            {
                case 0: //add
                    {
                        LeftOperator += RightOperator;
                        return LeftOperator;
                    }
                case 1:
                    {   //SUB                           
                        LeftOperator -= RightOperator;
                        return LeftOperator;
                    }
                case 2:
                    { //SET    
                        return RightOperator;
                    }
                case 3:
                    {
                        //AND
                        LeftOperator &= RightOperator;
                        return LeftOperator;
                    }
                case 4:
                    {
                        //OR                            
                        LeftOperator |= RightOperator;
                        return LeftOperator;
                    }
                case 5:
                    {//XOR
                        LeftOperator ^= RightOperator;
                        return LeftOperator;
                    }
                case 6:
                    {  //LSHIFT
                        LeftOperator <<= RightOperator;
                        return LeftOperator;
                    }
                case 7:
                    {
                        if (LeftOperator == RightOperator)
                        {//XOR leftoperator, leftoperator
                            return 0;
                        }
                        else
                        {
                            return LeftOperator + 1;//increment
                        }
                    }
                default:
                    Debug.Print($"UNKNOWN OPERATOR {LeftOperator} {Operation} {RightOperator}");
                    return LeftOperator; //make no change
            }
        }
    }//end class
}//end namespace