namespace Underworld
{
    public class silvertree:objectInstance
    {

        public static void PickupTree(uwObject obj)
        {
            //remove the silver trees animo
            foreach (var ovl in UWTileMap.current_tilemap.Overlays)
		    {
                if (ovl!=null)
                {
                    if (ovl.link == obj.index)
                    {
                        ovl.Duration=0;
                        ovl.link=0;
                    }
                }
            }
            //now change into a seed with doordir set to 1
            obj.item_id = 290;
            obj.doordir = 1;

            playerdat.SilverTreeLevel = 0;
            
            if (playerdat.ObjectInHand == obj.index)
            {
                uimanager.instance.mousecursor.SetCursorToObject(obj.item_id);
            }
            uimanager.AddToMessageScroll(GameStrings.GetString(1,9));

        }
    }//end class
}//end namespace
