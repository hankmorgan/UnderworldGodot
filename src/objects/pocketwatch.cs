namespace Underworld
{
    public class pocketwatch:objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (!WorldObject)
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, 39) + playerdat.TwelveHourClock + ":" + playerdat.Minute.ToString("d2"));
            }            
            return true;
        }
    }//end class
}//end namespace