namespace Underworld
{
    /// <summary>
    /// Trap which moves a starts a conversation with a taking door in UW1
    /// </summary>
    public class a_do_trap_conversation : hack_trap
    {
        public static void Activate()
        {
            uwObject talkingDoor = talk.SpawnTemporaryTalker(whoami:25);
            talk.Talk(talkingDoor.index, UWTileMap.current_tilemap.LevelObjects, true);
        }


    }//end class
}//end namespace
