using System;
using System.Diagnostics;

namespace Underworld
{

    /// <summary>
    /// Trap that creates an item in Britannia that suits the player skillset
    /// </summary>
    public class a_hack_trap_classitem : trap
    {
        public static void Activate(uwObject trapObj, uwObject triggerObj, uwObject[] objList)
        {
            var BestCombatSkill = -1;
            var BestCombatSkillScore = -1;
            int BestMagicSkillScore;
            var newQTY = 0;
            int itemID;
            var newQuality = -1;

            for (int si = 2; si <= 6; si++)
            {
                if (playerdat.GetSkillValue(si) > BestCombatSkillScore)
                {
                    BestCombatSkill = si;
                    BestCombatSkillScore = playerdat.GetSkillValue(si);
                }
            }
            BestMagicSkillScore = Math.Max(playerdat.Casting, playerdat.ManaSkill);
            if (BestMagicSkillScore >= BestCombatSkill)
            {
                if (BestMagicSkillScore == 0)
                {
                    itemID = 0x3C;//a wooden shield. The player is going to need it without skills...
                }
                else
                {
                    itemID = 0xF4; //a mani stone. 
                }
                newQuality = 0x3F;
            }
            else
            {
                switch (BestCombatSkill)
                {
                    case 2: //unarmed, gloves
                        itemID = 0x26; newQuality = 0x3F; break;
                    case 3: //swords
                        itemID = 4; break;
                    case 4: //axe
                        itemID = 0; break;
                    case 5://maces
                        itemID = 7; break;
                    case 6://missile
                        itemID = 0x10;
                        newQTY = 30; break;//there is a complicated set of rng calls that need to be investigated further here.
                    default:// not a skill.
                        return;
                }
            }
            if (itemID != -1)
            {
                var tile = UWTileMap.current_tilemap.Tiles[triggerObj.quality, triggerObj.owner];
                var ObjToChange = objectsearch.FindMatchInObjectChain(tile.indexObjectList, 0, 0, trapObj.owner, objList);
                if (ObjToChange != null)
                {
                    ObjToChange.item_id = itemID;
                    if (newQTY > 0)
                    {
                        ObjToChange.is_quant = 1;
                        ObjToChange.link = (short)newQTY;
                    }
                    if (newQuality != -1)
                    {
                        ObjToChange.quality = (short)newQuality;
                    }

                    if (ObjToChange.instance != null)
                    {
                        ObjToChange.instance.uwnode.QueueFree();
                    }
                    ObjectCreator.RenderObject(ObjToChange, UWTileMap.current_tilemap);
                }
            }
        }

    }//end class
}//end namespace
