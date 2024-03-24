namespace Underworld
{
    public class a_delete_object_trap : trap
    {
        public static void Activate(uwObject trapObj, uwObject[] objList)
        {
            var tileX = trapObj.quality; var tileY = trapObj.owner;           
            var indexToDelete = trapObj.link;
            ObjectCreator.DeleteObjectFromTile(
                tileX: tileX, 
                tileY: tileY,
                indexToDelete: indexToDelete);
        }        
    }//end class
}//end namespace