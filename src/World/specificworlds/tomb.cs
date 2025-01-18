
using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Specific code for the tomb
    /// </summary>
    public class tomb : UWClass
    {
        /// <summary>
        /// Handles killing of all undead in Loths tomb. TODO: this needs to be called when changing levels when quest 7 is set
        /// </summary>
        /// <param name="raceparam"></param>
        public static void KillLothsLiches(int raceparam)
        {
            Debug.Print("kill all the liches");
            for (int i = 0; i < UWTileMap.current_tilemap.NoOfActiveMobiles; i++)
            {
                var critterindex = UWTileMap.current_tilemap.GetActiveMobileAtIndex(i);
                var obj = UWTileMap.current_tilemap.LevelObjects[critterindex];
                if (obj.majorclass == 1)
                {//npc
                    if (npc.CheckIfMatchingRaceUW2(obj, raceparam))
                    {
                        npc.KillCritter(obj);
                        i--;//reduce index as list count has changed.
                    }
                }
                else
                {
                    if (raceparam == -1)
                    {
                        if (obj.item_id == 0x13)
                        {
                            Debug.Print($"Smite {obj.a_name}");
                        }
                    }
                }
            }
        }
    }//end class
}//end namespace