using System;

namespace Underworld
{
    public class a_damage_trap : trap
    {

        public static void Activate(uwObject trapObj, uwObject triggeringCharacter)
        {
            ApplyDamageTrap(triggeringCharacter, trapObj.owner != 0, trapObj.quality);
        }


        public static void ApplyDamageTrap(uwObject triggeringCharacter, bool Poison, int basedamage)
        {
            if (triggeringCharacter == playerdat.playerObject)
            {
                if (Poison)
                {
                    //apply poisoning
                    //int test = basedamage;
                    //var scale = damage.ScaleDamage(127, ref test, 0x10); //player;
                    if (basedamage > playerdat.play_poison)
                    {
                        playerdat.play_poison = (byte)Math.Min(basedamage, 0xF);
                    }
                }
                else
                {
                    //appy damage
                    if (basedamage>0)
                    {
                        playerdat.play_hp = Math.Max(0, playerdat.play_hp - basedamage);
                    }                    
                }
            }
            else
            {
                //npc
                if (Poison)
                {
                    basedamage = -basedamage;
                }
                if (basedamage>0)
                {
                    damage.DamageObject(objToDamage: triggeringCharacter, basedamage: basedamage, damagetype: 4, objList: UWTileMap.current_tilemap.LevelObjects, WorldObject: true, damagesource: 0);
                }                
            }
        }
    }
}
