namespace Underworld
{
    public class incense:uwObject
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (WorldObject)
            {
                return false;
            }    
            var visiontoplay = Rng.r.Next(3);
            if (playerdat.IncenseCounter < 3)
            {
                //ensure the first three visions are played. Afterwards it's entirely random 0-2.
                playerdat.IncenseCounter++;
                visiontoplay = 3-playerdat.IncenseCounter;
            }

            cutsplayer.PlayCutscene(CutsceneNo: 0xb + visiontoplay, callBackMethod: null, useSingleRedChannel: false);
            obj.item_id = 213;//turn to debris
            uimanager.UpdateInventoryDisplay();
            return true;            
        }
    }
}