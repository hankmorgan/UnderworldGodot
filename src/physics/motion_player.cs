using System;
using System.Data;
using System.Net.Sockets;

namespace Underworld
{
    /// <summary>
    /// Player Motion.
    /// </summary>
    public partial class motion : Loader
    {
       public static UWMotionParamArray playerMotionParams = new UWMotionParamArray();

        public static short PlayerHeadingRelated_dseg_67d6_33D6 = 0;

        //These 2 globals are used to determine the heading/velocity for the player.
        public static short PlayerMotionWalk_77C = 0;
        public static short PlayerMotionHeading_77E = 0;
        public static short MotionInputPressed = 0;


        //Player Vectors (to be identified)
        static short dseg_67d6_D0 = 0;
        public static short Examine_dseg_D3 = 0;

        //globals used in player motion calcs
        //Possibly some of these are actually part of the player motion params. to do map them out.
        static short dseg_67d6_33D4;
        static short dseg_67d6_33D2;
        static short dseg_67d6_33D0;

        static short dseg_67d6_229A;
        static short dseg_67d6_229C;
        
        static short dseg_67d6_22AA;
        static short dseg_67d6_22A6;
        static short dseg_67d6_22A8;

        static short MotionWeightRelated_dseg_67d6_C8;

        static short MotionRelated_dseg_67d6_775 = 0xF;
       
        public static void PlayerMotion(short ClockIncrement)
        {
            dseg_67d6_33D4 = 0;
            dseg_67d6_33D2 = 0;
            dseg_67d6_33D0 = 0;
            playerMotionParams.radius_22 = (byte)commonObjDat.radius(playerdat.playerObject.item_id);
            playerMotionParams.height_23 = (byte)commonObjDat.height(playerdat.playerObject.item_id);

            PlayerMotionInitialCalculation_seg008_1B09_7B2(ClockIncrement);

            CalculateMotion(
                projectile: playerdat.playerObject,
                MotionParams: playerMotionParams,
                SpecialMotionHandler: UWMotionParamArray.PlayerMotionHandler_dseg_67d6_26AA);

            ApplyPlayerMotion(playerMotionParams);


        }


        static void PlayerMotionInitialCalculation_seg008_1B09_7B2(short ClockIncrement)
        {
            //todo
            short di = 0;
            short var2 = 0;
            playerMotionParams.unk_16_relatedtoPitch = 5;
            playerMotionParams.unk_17 = 0;

            dseg_67d6_229A = playerMotionParams.heading_1E;
            dseg_67d6_229C = playerMotionParams.unk_14;


            if (playerMotionParams.unk_10 == 0)
            {
                CalculateMotionFromCommand_seg008_1B09_108E(MotionInputPressed, ClockIncrement, out var2);
                if (playerMotionParams.unk_10 == 0)
                {
                    //seg008_1B09_7F6
                    di = (short)(var2 - playerMotionParams.unk_14);

                    if (Math.Abs(di) > MotionWeightRelated_dseg_67d6_C8)
                    {
                        if(di > 0)
                        {
                            di = (short)(1 * MotionWeightRelated_dseg_67d6_C8);
                        }
                        else
                        {
                            di = (short)(-1 * MotionWeightRelated_dseg_67d6_C8);
                        }

                    }
                    //seg008_1B09_81C
                    playerMotionParams.unk_14 += di;
                    if (playerMotionParams.unk_14 <= dseg_67d6_22A6)
                    {
                        if (playerMotionParams.unk_14<0)
                        {
                            playerMotionParams.unk_14 = 0;
                        }
                    }
                    else
                    {
                        playerMotionParams.unk_14 = dseg_67d6_22A6;
                    }
                }
            }


            //seg008_1B09_83E:




        }

        static void ApplyPlayerMotion(UWMotionParamArray MotionParams)
        {
            //todo

        }

        /// <summary>
        /// Translates an input command into motion inputs.
        /// </summary>
        /// <param name="inputcmd"></param>
        /// <param name="ClockIncrement"></param>
        /// <param name="arg4"></param>
        static void CalculateMotionFromCommand_seg008_1B09_108E(short inputcmd, int ClockIncrement, out short arg4)
        {
            arg4=0;
            //todo
            if (playerdat.ParalyseTimer != 0)
            {
                arg4 = 0;
            }
            else
            {
                var di = playerdat.heading_minor;
                switch (inputcmd)
                {
                    case 0:
                        arg4 = 0;break;
                    case 1://walk,run,turn
                        {
                            playerdat.heading_minor += (MotionRelated_dseg_67d6_775 * ClockIncrement) * (PlayerMotionHeading_77E / 4);
                            di = playerdat.heading_minor;
                            playerdat.heading_major = di;
                            arg4 = (short)(((PlayerMotionWalk_77C >> 2) * dseg_67d6_22A6) / 0x20);
                            break;
                        }  
                    case 6:
                    case 7://jumps
                        {
                            if (inputcmd == 6)
                            {
                                if (playerMotionParams.unk_a_pitch == 0)
                                    {
                                        if (playerMotionParams.unk_10 == 0)
                                        {
                                            if (playerMotionParams.unk_14 == 0)
                                            {
                                                //seg008_1B09_1158:
                                                di = playerdat.heading_minor;
                                                playerdat.heading_major = di;
                                                arg4 = (short)(dseg_67d6_22A6 / 2);
                                                playerMotionParams.unk_14 = arg4;
                                                dseg_67d6_D0 = 0;
                                            }
                                        }
                                    }
                            }
                            //case 7
                            playerMotionParams.unk_a_pitch = 0x263;
                            if (playerMotionParams.z_4 > 0x280)
                            {
                                playerMotionParams.unk_a_pitch = (short)((playerMotionParams.unk_a_pitch * 5)/6);   
                                if (playerMotionParams.z_4 > 0x2C0)
                                {
                                    playerMotionParams.unk_a_pitch = (short)((playerMotionParams.unk_a_pitch<<1) / 3);
                                }                             
                            }
                            if ((playerdat.MagicalMotionAbilities & 0x1) == 0) //leaping
                            {
                                playerMotionParams.unk_10 = -4;
                            }
                            else
                            {
                                playerMotionParams.unk_10 = -2;
                            }                           
                            
                            break;
                        }
                    case 8://walk backwards
                        {
                            di = di - 0x8000;
                            arg4 = dseg_67d6_22AA;
                            dseg_67d6_D0 = -2;
                            playerdat.heading_major = di;
                            break;
                        }
                    case 9://slide
                    case 0xA:
                        {
                            di = di - 0x4000;
                            arg4 = dseg_67d6_22A8;
                            dseg_67d6_D0 = -1;
                            playerdat.heading_major = di;
                            break;
                        }
                    case 0xC://flying up?
                        {
                            dseg_67d6_D0 = 0;
                            playerMotionParams.unk_a_pitch = 0x8D;
                            playerMotionParams.unk_10 = 0;
                            playerdat.heading_major = di;
                            break;
                        }
                    case 0xD://flying down?
                        {
                            dseg_67d6_D0 = 0;
                            playerMotionParams.unk_a_pitch = -141;
                            playerMotionParams.unk_10 = 0;
                            playerdat.heading_major = di;
                            break;
                        }
                    default:
                        {
                            playerdat.heading_major = di;
                            break;
                        }
                }
            }
        }

    }//end class
}//end namespace