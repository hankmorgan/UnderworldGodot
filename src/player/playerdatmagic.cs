using Godot;

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
        public static bool GetRune(int index)
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
        public static void SetRune(int index, bool newValue)
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

        /// <summary>
        /// Gets the selected rune at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static int GetSelectedRune(int index)
        {
            //48,49 and 4A
            return GetAt(0x48+index);
        }

        /// <summary>
        /// Sets the value at this slot. Use 24 if slot is to be empty.
        /// </summary>
        /// <param name="index"></param>
        public static void SetSelectedRune(int index, int value)
        {
            //48,49 and 4A
            SetAt(0x48+index, (byte)value);
        }

        /// <summary>
        /// Returns true if the rune is selected at this slot index 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool IsSelectedRune(int index)
        {
            //48,49 and 4A
            return GetAt(0x48+index) != 24;
        }

        /// <summary>
        /// Count of runes selected to cast a spell with
        /// </summary>
        public static int NoOfSelectedRunes
        {
            get
            {
                if(_RES==GAME_UW2)
                {
                    return (GetAt(0x62)>>2) & 3;
                }
                else
                {
                    return (GetAt(0x61)>>2) & 3;
                }
            }
            set
            {  
                if (_RES==GAME_UW2)
                {
                    var tmp = GetAt(0x62);
                    tmp = (byte)(tmp & 0xF3);
                    value = value & 0x3;
                    tmp |= (byte)(value<<2);
                    SetAt(0x62,tmp);
                }
                else
                {
                    var tmp = GetAt(0x61);
                    tmp = (byte)(tmp & 0xF3);
                    value = value & 0x3;
                    tmp |= (byte)(value<<2);
                    SetAt(0x61,tmp);
                }
            }
        }


    } //end class
} //end namespace