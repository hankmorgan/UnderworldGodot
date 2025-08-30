using System.Diagnostics;

namespace Underworld
{
    public class a_delete_object_trap : trap
    {
        public static void Activate(uwObject trapObj, uwObject[] objList)
        {
            var tileX = trapObj.quality; var tileY = trapObj.owner;           
            var indexToDelete = trapObj.link;
            Debug.Print($"Delete object trap: Deleting {indexToDelete}");
            ObjectRemover.DeleteObjectFromTile_DEPRECIATED(
                tileX: tileX, 
                tileY: tileY,
                indexToDelete: indexToDelete);
        }        
    }//end class
}//end namespace