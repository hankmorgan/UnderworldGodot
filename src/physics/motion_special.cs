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

        static void TalismanOnLava(uwObject obj)
        {
            
        }


        /// <summary>
        /// Detonates projectiles like fireballs and lightning bolts.
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="Source"></param>
        /// <returns></returns>
        static bool DetonateProjectile(uwObject projectile, int Source)
        {
            Debug.Print("Detonate projectile");
            var maxEntries = 2;
            if (_RES==GAME_UW2)
            {
                maxEntries = 3;
            }
            var newItemID = -1;
            var EffectIndex = 0;
            for (int i = 0; i<maxEntries; i++)
            {
                if (ProjectileOldItemIDTable[i] == projectile.item_id)
                {
                    newItemID = ProjectileNewItemIDTable[i];
                    EffectIndex = i;
                    break;
                }
            }
            if (newItemID == -1)
            {
                return false;
            }

            projectile.item_id = newItemID;
            if(animo.CreateAnimoLink(projectile, animationObjectDat.noOfFrames(newItemID)))
            {
                objectInstance.RedrawFull(projectile);
                if (EffectIndex != 1)
                {
                    //fireballs
                    animo.SpawnAnimoCopies(projectile, projectile.tileX, projectile.tileY);
                }
                damage.DamageObjectsInTile(projectile.tileX, projectile.tileY, Source, EffectIndex);

                return true;
            }
            else
            {
                //vanilla behaviour is the projectile will not damage anything if the animo cannot be created. The animo will be deleted by the calling function.
                return false;
            }
            
        }
    }//end class
}//end namespace