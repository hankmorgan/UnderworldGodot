namespace Underworld
{
    /// <summary>
    /// Trap which flicks switches randomly flicks switches in a range of tiles. Used in Talorus Bly Skup chamber
    /// </summary>
    public class a_hack_trap_switchflicker : hack_trap
    {
        public static void Activate(int triggerX, int triggerY)
        {
            var di_x = 0;
            while (di_x < 5)
            {
                var si_y = 0;
                while (si_y < 2)
                {
                    if (UWTileMap.ValidTile(triggerX + di_x, triggerY + si_y))
                    {
                        var tile = UWTileMap.current_tilemap.Tiles[triggerX + di_x, triggerY + si_y];
                        if (tile.indexObjectList != 0)
                        {
                            var obj = UWTileMap.current_tilemap.LevelObjects[tile.indexObjectList];
                            CallBacks.CallFunctionOnObjectsInChain(
                                methodToCall: Flicker,
                                obj: obj,
                                objList: UWTileMap.current_tilemap.LevelObjects);
                        }
                    }
                    si_y++;
                }
                di_x++;
            }
        }

        public static bool Flicker(uwObject obj)
        {
            if (obj.OneF0Class == 0x17)
            {
                if (Rng.r.Next(2) == 0)//50:50 chance
                {
                    button.TryAndUse(
                        obj: obj,
                        toggleMode: 3);
                }
            }
            return false;
        }
    }//end class
}//end namespace