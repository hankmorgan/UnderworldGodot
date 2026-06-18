using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Linq;

namespace Underworld
{
    /// <summary>
    /// For implementations of 1992-1993 16 bit math. Handles some edge cases where modern math returns different results then what Dos returns.
    /// </summary>


    public class UnderworldMath
    {
        //Registers..
        public static ushort ax;
        public static ushort bx;
        public static ushort cx;
        public static ushort dx;

        public static ushort ah
        {
            get
            {
                return (ushort)(ax >> 8);
            }
            set
            {
                ax = (ushort)((ax & 0x00FF) | (value << 8));
            }
        }

        public static ushort bh
        {
            get
            {
                return (ushort)(bx >> 8);
            }
            set
            {
                bx = (ushort)((bx & 0x00FF) | (value << 8));
            }
        }

        public static ushort ch
        {
            get
            {
                return (ushort)(cx >> 8);
            }
            set
            {
                cx = (ushort)((cx & 0x00FF) | (value << 8));
            }
        }

        public static ushort dh
        {
            get
            {
                return (ushort)(dx >> 8);
            }
            set
            {
                dx = (ushort)((dx & 0x00FF) | (value << 8));
            }
        }

        public static ushort al
        {
            get
            {
                return (ushort)(ax & 0xFF);
            }
            set
            {
                ax = (ushort)((ax & 0xFF00) | (value));
            }
        }

        public static ushort bl
        {
            get
            {
                return (ushort)(bx & 0xFF);
            }
            set
            {
                bx = (ushort)((bx & 0xFF00) | (value));
            }
        }

        public static ushort cl
        {
            get
            {
                return (ushort)(cx & 0xFF);
            }
            set
            {
                cx = (ushort)((cx & 0xFF00) | (value));
            }
        }

        public static ushort dl
        {
            get
            {
                return (ushort)(dx & 0xFF);
            }
            set
            {
                dx = (ushort)((dx & 0xFF00) | (value));
            }
        }

        public static uint axdx
        {
            get
            {
                return (uint)((dx << 16) | (ax));
            }
        }

        public static ushort di;
        public static ushort si;
        public static ushort cf;

        //https://stackoverflow.com/questions/73961951/bit-right-rotate-through-carry-implementation-c
        public static ushort RotateRightThroughCarry(ushort value, ushort count, ref ushort carry)
        {
            for (ushort i = 0; i < count; i++)
            {
                value = RotateRightOneBitThroughCarry(value, ref carry);
            }
            return value;
        }

        public static ushort RotateRightOneBitThroughCarry(ushort value, ref ushort carry)
        {
            ushort newCarry = (ushort)(value & 1);
            ushort newValue = (ushort)((value >> 1) | (carry << 31));
            carry = newCarry;
            return newValue;
        }


        /// <summary>
        /// X86 Unsigned Divide.
        /// </summary>
        /// <param name="divisor"></param>
        public static void div(ushort divisor)
        {
            var tmpaxdx = axdx;
            var temp = (uint)(ax / divisor);  //might need to be axdx
            if (temp > 0xFFFF)
            {

            }
            else
            {
                ax = (ushort)temp;
                dx = (ushort)(tmpaxdx % divisor);
            }
        }


    }

    /// <summary>
    /// Groan. an implementation of vanilla square root code to handle edge cases in shade calcs...
    /// </summary>
    public class UnderWorldSqrt : UnderworldMath
    {
        public static ushort sqrt_vanilla(uint value)
        {
            cx = (ushort)(value >> 16);
            bx = (ushort)(value & 0xFFFF);
            //seg021_A7A
            if (ch == 0)
            {
                //seg021_AB9
                if (cl == 0)
                {
                    //Seg021_AF3
                    if (bh == 0)
                    {
                        //seg021_B2F
                        if (bl == 0)
                        {
                            return 0;
                        }
                        else
                        {
                            cx = 4;
                            for (byte i = 0; i < 5; i++)
                            {
                                ax = bx;
                                div(cl);
                                cl += al;
                                cl = (ushort)(cl >> 1); //RotateRightThroughCarry(cl, 1, ref cf);
                            }
                            di = cx;
                            return di;
                        }
                    }
                    else
                    {
                        //Seg021_AF7
                        cx = 0x40;
                        for (byte i = 0; i < 5; i++)
                        {
                            ax = bx;
                            dx = 0;
                            div(cx);
                            cx += ax;
                            RotateRightThroughCarry(cx, 1, ref cf);
                        }
                        di = cx;
                        return di;
                    }
                }
                else
                {
                    //Seg021_ABD
                    di = 0x400;
                    for (byte i = 0; i < 5; i++)
                    {
                        dx = cx;
                        ax = bx;
                        div(di);
                        di += ax;
                        RotateRightThroughCarry(di, 1, ref cf);
                    }
                    return di;
                }
            }
            else
            {
                //seg021_A7C
                di = 0x4000;
                if (cx >= di)
                {
                    di = 0xFFFF;
                }
                //seg021_A86 (this repeats 5 times)
                for (byte i = 0; i < 5; i++)
                {
                    dx = cx;
                    ax = bx;
                    ax = (ushort)(ax / di);
                    div(di);
                    di += ax;
                    RotateRightThroughCarry(di, 1, ref cf);
                }
                return di;
            }
        }
    }
}

