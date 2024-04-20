namespace Underworld
{
    /// <summary>
    /// Trap which moves a platform up and down
    /// </summary>
    public class a_do_trap_conversation : hack_trap
    {
        public static void Activate()
        {
            var talkingDoor = ObjectCreator.spawnObjectInTile(
                itemid: 64, 
                tileX: 32, tileY: 32, 
                xpos: 0, ypos: 0, zpos: 0, 
                WhichList: ObjectCreator.ObjectListType.MobileList);
            talkingDoor.npc_whoami = 25;
            talkingDoor.npc_attitude = 3;
            talkingDoor.npc_goal = 10;
            ConversationVM.TemporaryTalker = true;
         
            talk.Talk(talkingDoor.index, UWTileMap.current_tilemap.LevelObjects, true);
        }
    }//end class
}//end namespace
