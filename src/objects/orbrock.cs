namespace Underworld
{
    public class OrbRock : objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject, uwObject UsingObjectOrCharacter)
        {
            if (!WorldObject)
            {
                //flag we are using the orbrock
                useon.CurrentItemBeingUsed = new useon(obj, WorldObject);
                //print use message
                uimanager.AddToMessageScroll($"Use {GameStrings.GetSimpleObjectNameUW(obj.item_id)} on?");
            }
            else
            {
                if (UsingObjectOrCharacter != null)
                {
                    if (UsingObjectOrCharacter.item_id == 0x117)
                    {
                        UseOn(OrbObject: obj, targetObject: UsingObjectOrCharacter, WorldObject: true, playerUsing: false);
                    }
                }
            }
            return true;
        }


        public static bool UseOn(uwObject OrbObject, uwObject targetObject, bool WorldObject, bool playerUsing)
        {
            if (targetObject.item_id == 0x117)
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x85));//the orb is destroyed.
                animo.SpawnAnimoAtTarget(targetObject, 0x4, 0x5, targetObject.tileX, targetObject.tileY);
                ObjectRemover_OLD.DeleteObjectFromTile_DEPRECIATED(targetObject.tileX, targetObject.tileY, targetObject.index);
                playerdat.isOrbDestroyed = true;
                playerdat.max_mana = playerdat.backup_mana;
                playerdat.play_mana = playerdat.max_mana;

                //damage tybal
                CallBacks.RunCodeOnNPCS_WhoAmI(methodToCall: ChangeTybalHP, whoami: 0xE7, paramsArray: new int[]{2}, loopAll: true);
            }
            else
            {
                if (playerUsing)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x84));// it seems to have no effect.
                }
            }
            return false;
        }

        static void ChangeTybalHP(uwObject critter, int[] paramsarray)
        {
            critter.npc_hp = (byte)(1 + (critter.npc_hp / paramsarray[0]));
            critter.UnkBit_0XD_Bit9_StopHPRegen = 1; //This will prevent Tybal from regenerating health if player leaves the level.
        }
    } //end class
}//end namespace