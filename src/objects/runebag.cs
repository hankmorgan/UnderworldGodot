namespace Underworld
{
    public class runebag:objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {     
            if (WorldObject)
            {
                return false;
            }
            else
            {
                uimanager.SetPanelMode(1);
                return true;
            }
        }
    }//end class
}//end namespace