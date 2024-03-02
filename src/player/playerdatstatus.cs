namespace Underworld
{
     /// <summary>
     /// Player dat variables that refer to the player status. eg motion states, status effects
     /// </summary>
     public partial class playerdat : Loader
     {
          /// <summary>
          /// The current light level the player has from torches and magical lights
          /// </summary>
          public static int lightlevel
          {
               get
               {
                    if (pdat==null){return uwsettings.instance.lightlevel;}
                    if(_RES==GAME_UW2)
                    {
                         return GetAt(0x65)>>4;
                    }
                    else
                    {
                         return GetAt(0x64)>>4;
                    }
               }

               set
               {
                    if (pdat==null){ uwsettings.instance.lightlevel = value;}
                    if(_RES==GAME_UW2)
                    {
                         var tmp = GetAt(0x65);
                         tmp = (byte)(tmp & 0x0F);
                         tmp = (byte)(tmp | (value<<4));
                         SetAt(0x65,tmp);
                    }
                    else
                    {
                         var tmp = GetAt(0x64);
                         tmp = (byte)(tmp & 0x0F);
                         tmp = (byte)(tmp | (value<<4));
                         SetAt(0x64,tmp);
                    }
               }
          }

          /// <summary>
          /// Ptr to next available spell effect slot.
          /// </summary>
          public static int ActiveSpellEffectCount
          {
               get
               {
                    if (_RES == GAME_UW2)
                    {
                         return (GetAt16(0x61) >> 5) & 0xF;
                    }
                    else
                    {
                         return (GetAt16(0x60) >> 6) & 0xF;
                    }
               }

               set
               {
                    if (_RES == GAME_UW2)
                    {
                         var temp = GetAt16(0x61);
                         temp &= 0xFE1F; //clear bits
                         temp |= (value << 5); //set new value
                         SetAt16(0x61, temp);
                    }
                    else
                    {
                         var temp = GetAt16(0x60);
                         temp &= 0xFC3F; //clear bits
                         temp |= (value << 6); //set new value
                         SetAt16(0x60, temp);
                    }
               }
          }


          /// <summary>
          /// Applies a spell effect to the player.dat
          /// </summary>
          /// <param name="index"></param>
          /// <param name="effectid"></param>
          /// <param name="stability"></param>
          public static void SetSpellEffect(int index, int effectid, int stability)
          {
               var temp = GetAt16(0x3F + index * 2);
               temp &= 0x00FF; //clear stability
               temp |= (stability<<8); //set stability

               temp &= 0xFF00; //clear effect
               temp |= effectid;

               SetAt16(0x3F  + index * 2 , temp);               
          }


          public static int GetEffectStability(int index)
          {               
               return GetAt16(0x3F + index * 2) >> 8;              
          }

          public static int GetEffectClass(int index)
          {
               return GetAt16(0x3F + index * 2) & 0xFF;                    
          }

          public static int SilverTreeLevel
          {
               get
               {
                    return GetAt(0x5F) >> 4;
               }
               set
               {
                    value = value & 0xF;
                    var tmp = GetAt(0x5F);
                    tmp &= 0xF;
                    tmp |= (byte)(value << 4);
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
                    if (_RES == GAME_UW2)
                    {
                         return ((GetAt(0x63) >> 1) & 1) == 1;
                    }
                    return false;//not applicable to UW2
               }
          }
     }
}