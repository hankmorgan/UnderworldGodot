namespace Underworld
{
     /// <summary>
     /// Player dat variables that refer to the player status. eg motion states, status effects
     /// </summary>
     public partial class playerdat : Loader
     {
        
        /// <summary>
        /// Ptr to next available spell effect slot.
        /// </summary>
          public static int ActiveSpellEffectCount
          {
               get
               {
                    if (_RES==GAME_UW2)
                    {
                         return (GetAt16(0x61)>>5) & 0xF;
                    }
                    else
                    {
                         return (GetAt16(0x60)>>6) & 0xF;
                    }                    
               }

               set
               {
                    if (_RES==GAME_UW2)
                    {
                        var temp = GetAt16(0x61);
                        temp &= 0xFE1F; //clear bits
                        temp |= (value <<5); //set new value
                        SetAt16(0x61, temp);
                    }
                    else
                    {
                         var temp = GetAt16(0x60);
                        temp &= 0xFC3F; //clear bits
                        temp |= (value <<6); //set new value
                        SetAt16(0x61, temp);
                    }  
               }
          }

          public static int SilverTreeLevel
          {
               get
               {
                    return GetAt(0x5F)>>4;
               }
               set
               {
                    value = value & 0xF;
                    var tmp =  GetAt(0x5F);
                    tmp &=0xF;
                    tmp |= (byte)(value<<4);
                    SetAt(0x5F, tmp);
               }
          }

        
          /// <summary>
          /// True when the player has fallen asleep under the effect of dream plants
          /// </summary>
          public static bool DreamingInVoid
          {
               get
               {
                    if(_RES==GAME_UW2)
                    {
                         return ((GetAt(0x63) >> 1) & 1) == 1;
                    }
                    return false;//not applicable to UW2
               }
          }
     }
}