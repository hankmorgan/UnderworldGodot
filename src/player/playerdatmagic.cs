using System;

namespace Underworld
{
    /// <summary>
    /// Partial player.dat for managing runestones, magic effects etc
    /// </summary>
    public partial class playerdat : Loader
    {
        /// <summary>
        /// Returns true if specified rune is in the rune bag
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool GetRune(int index)
        {//0x45,0x46,0x47
            int RuneGroup = index / 8;
            int bit = 7 - (index % 8);
            return ((GetAt(0x45 + RuneGroup) >> bit) & 0x1) == 1;
        }


        /// <summary>
        /// Set the state of the rune at index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newValue"></param>
        public void SetRune(int index, bool newValue)
        {
            int RuneSet = index / 8;
            int bit = 7 - (index % 8);
            byte mask;
            byte existingValue = GetAt(0x45 + RuneSet);
            mask = (byte)(1 << bit);
            if (newValue == true)
            {//Set rune
                existingValue |= mask;
            }
            else
            {//clear runes. Should only happen at chargen. Possibly redundant due to initsavegame function
                existingValue = (byte)(existingValue & (~mask));
            }
            SetAt(0x45 + RuneSet, existingValue);
        }

    } //end class
} //end namespace