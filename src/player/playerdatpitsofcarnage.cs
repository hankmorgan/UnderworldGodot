namespace Underworld
{
    //The pits of carnage arena fights are fairly complex.
    public partial class playerdat : Loader
    {
        
        public static bool IsFightingInPit
        {
            get
            {
                return ((GetAt(0x64) >> 2) & 0x1) == 1;
            }
            set
            {
                var currval = GetAt(0x64);
                if (value)
                {//set
                    currval |= 4;
                }
                else
                {//clear
                    currval &= 0xFB;
                }
                SetAt(0x64, currval);
            }
        }       

        public static void SetPitFighter(int i, byte indexToSet)
        {
            SetAt(0x361 + i, indexToSet);
        }


        public static byte GetPitFighter(int i)
        {
            return GetAt(0x361 + i);
        }        


    }//end class
}//end namespace