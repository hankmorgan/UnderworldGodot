namespace Underworld
{
    public partial class scd : UWClass
    {
        /// <summary>
        /// Removes object(s) specified by the filter from the map.
        /// </summary>
        /// <param name="currentblock"></param>
        /// <param name="eventOffset"></param>
        /// <returns></returns>
        static int RemoveObject(byte[] currentblock, int eventOffset)
        {
            RunCodeOnObjects_SCD(
                methodToCall: RemoveObject,
                mode: currentblock[eventOffset + 5],
                filter: currentblock[eventOffset + 6],
                loopAll: true,
                currentblock: currentblock,
                eventOffset: eventOffset);
            return 0;
        }


        static void RemoveObject(uwObject obj, int[] paramsarray)
        {
            //TODO: there is more logic here including stuff with animation overlays.
            if (UWTileMap.ValidTile(obj.tileX, obj.tileY))
            {
                ObjectRemover.DeleteObjectFromTile(
                    tileX: obj.tileX, tileY: obj.tileY, 
                    indexToDelete: obj.index);
            }
        }
    }//end class
}//end namesace