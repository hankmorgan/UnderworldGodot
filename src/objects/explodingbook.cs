namespace Underworld
{
    public class explodingbook : objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (WorldObject)
            {
                return false;                
            }
            else
            {
                uimanager.AddToMessageScroll("The book explodes in your face!");
                SpellCasting.CastClass9_Curse(3, false);
                playerdat.SetQuest(8,1);
                playerdat.RemoveFromInventory(obj.index, true);
                return true;
            }
        }
    }//end class
}//end namespace