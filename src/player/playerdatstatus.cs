namespace Underworld
{
     /// <summary>
     /// Player dat variables that refer to the player status. eg motion states, status effects
     /// </summary>
     public partial class playerdat : Loader
     {
        
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