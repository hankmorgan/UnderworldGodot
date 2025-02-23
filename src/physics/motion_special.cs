using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Handle quest and special events that are related to motion. Eg throwing basilisk oil into mud, talismans into lava, projectile detonation
    /// </summary>

    public partial class motion : Loader
    {

        /// <summary>
        /// Handles throwing basilisk oil on muddy water in UW2 as part of the Djinn Capture quest.
        /// </summary>
        /// <param name="obj"></param>
        static void OilOnMud(uwObject obj)
        {
            var tile = UWTileMap.current_tilemap.Tiles[obj.tileX, obj.tileY];
            if (UWTileMap.current_tilemap.texture_map[tile.floorTexture] == 0xC1)
            {
                var enchant = MagicEnchantment.GetSpellEnchantment(obj, UWTileMap.current_tilemap.LevelObjects);
                if (enchant != null)
                {
                    if (enchant.IsFlagBit2Set && (enchant.SpellMajorClass == 0xD) && (enchant.SpellMinorClass == 3))
                    {
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x14C));//the thick oil permeates the mud
                        if (playerdat.GetXClock(3) < 2)
                        {
                            playerdat.SetXClock(3, 2);//advance djinn capture xclock to 2.
                        }
                    }
                }
            }
        }

        static bool DetonateProjectile(uwObject projectile, int effectId)
        {
            Debug.Print("Detonate projectile");
            return true;
        }
    }//end class
}//end namespace