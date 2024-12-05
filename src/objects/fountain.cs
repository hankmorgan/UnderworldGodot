namespace Underworld
{
    public class fountain : objectInstance
    {
        public static bool Use(uwObject obj)
        {
            var spell = MagicEnchantment.GetSpellEnchantment(obj, UWTileMap.current_tilemap.LevelObjects);
            if (spell != null)
            {
                SpellCasting.CastSpell(
                    majorclass: spell.SpellMajorClass,
                    minorclass: spell.SpellMinorClass,
                    caster: null,
                    target: null,
                    tileX: playerdat.tileX,
                    tileY: playerdat.tileY,
                    CastOnEquip: false,
                    PlayerCast: true);
                use.SpellHasBeenCast = true;
                if (spell.SpellMajorClass == 4)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_the_waters_of_the_fountain_renew_your_strength_));
                }
                else
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_the_water_refreshes_you_));
                }

                return true;
            }
            //Check if water on
            var tile = UWTileMap.current_tilemap.Tiles[obj.tileX, obj.tileY];
            var water = objectsearch.FindMatchInObjectChainIncLinks(
                ListHeadIndex: tile.indexObjectList,
                majorclass: 4,
                minorclass: 2,
                classindex: 0xE,
                objList: UWTileMap.current_tilemap.LevelObjects);
            if (water == null)
            {
                if (_RES == GAME_UW2)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, 147));//The fountain is dry. \n
                }
            }
            else
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_the_water_refreshes_you_));
            }

            return true;
        }

        public static bool FountainAnimoUse(uwObject obj)
        {
            //Checks for a linked fountain to this animated water
            var tile = UWTileMap.current_tilemap.Tiles[obj.tileX, obj.tileY];
            var parentfountain = objectsearch.FindMatchInObjectChainIncLinks(
                ListHeadIndex: tile.indexObjectList,
                majorclass: 4,
                minorclass: 2,
                classindex: 0xE,
                objList: UWTileMap.current_tilemap.LevelObjects);
            if (parentfountain != null)
            {
                return Use(parentfountain);
            }
            return false;
        }

    }//end class
}//end namespace