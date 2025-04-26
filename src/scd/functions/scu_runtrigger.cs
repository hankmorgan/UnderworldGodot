
namespace Underworld
{
    /// <summary>
    /// What the hell is SCD.ARK? It's an event system. Data is read from scd.ark at the end of conversations, level transitions, by a hack trap, when a block of time is added to the clock and when Killorn is crashing.
    /// </summary>
    public partial class scd:UWClass
    {

        /// <summary>
        /// Runs the scheduled triggers in the specified tile
        /// </summary>
        /// <returns></returns>
        public static int RunTrigger(byte[] currentblock, int eventOffset)
        {
            var tileX = currentblock[eventOffset + 5];
            var tileY = currentblock[eventOffset + 6];

            if (UWTileMap.ValidTile(tileX,tileY))
            {
                var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                if (tile.indexObjectList!=0)
                {
                    var next = tile.indexObjectList;
                    while (next!=0)
                    {
                        var obj = UWTileMap.current_tilemap.LevelObjects[next];
                        next = obj.next;//get this now in case the obj gets deleted.
                        if (obj.IsTrigger)
                        {
                            trigger.RunTrigger(
                                character: 1, 
                                ObjectUsed: obj, 
                                TriggerObject: obj, 
                                triggerType: (int)triggerObjectDat.triggertypes.SCHEDULED, 
                                objList: UWTileMap.current_tilemap.LevelObjects );
                        }
                    }
                }
            }
            return 0;
        }
    }//end class
}//end namespace