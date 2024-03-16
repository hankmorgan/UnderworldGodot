using System;
using System.Runtime.Serialization;

namespace Underworld
{
    public class a_damage_trap:trap
    {
        public static void ApplyDamageTrap(bool Poison, int basedamage)
        {
            if (Poison)
            {
                //apply poisoning
                //int test = basedamage;
                //var scale = uwObject.ScaleDamage(127, ref test, 0x10); //player;
                if (basedamage> playerdat.play_poison)
                {
                    playerdat.play_poison = (byte)Math.Min(basedamage,0xF);
                }
            }
            else
            {
                //appy damage
                playerdat.play_hp  = Math.Max(0, playerdat.play_hp-basedamage);
            }
        }
    }
}
