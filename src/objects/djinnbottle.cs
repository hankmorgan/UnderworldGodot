using System.Diagnostics;

namespace Underworld
{
    public class djinnbottle : objectInstance
    {
        public static void DestroyDjinnBottle(uwObject obj, bool WorldObject)
        {
            if (!WorldObject)
            {
                return;
            }            
            if (playerdat.GetXClock(3)<5)
            {//player has not coated in treated filanium, baked in lava and cast ironflesh
                ObjectRemover.DeleteObjectFromTile_DEPRECIATED(obj.tileX,obj.tileY, obj.index,true);
                uimanager.AddToMessageScroll(GameStrings.GetString(1,0x171));
                Debug.Print("Kill the player with 255 points of raw damage");
                playerdat.play_hp = 0;//probably need to wait for next frame before processing player death
            }
            else
            {//pre-reqs met, test if at the sigil
                if (
                    (playerdat.dungeon_level == 0x45)
                    &&
                    (obj.tileX>=0x15) && (obj.tileX<=0x16)
                    &&
                     (obj.tileY>=0x34) && (obj.tileY<=0x35)
                )
                {
                   //at the sigil
                   ObjectRemover.DeleteObjectFromTile_DEPRECIATED(obj.tileX,obj.tileY, obj.index,true);
                   uimanager.FlashColour(2, uimanager.CutsSmall);
                   playerdat.SetXClock(3, 6);//djinn has been captured
                   uimanager.AddToMessageScroll(GameStrings.GetString(1,0x150));
                   if (playerdat.GetXClock(1)==0xD)
                   {
                    playerdat.SetXClock(1,0xE);
                   }
                   playerdat.SetQuest(105,1);                   
                }
                else
                { // not a sigil
                    ObjectRemover.DeleteObjectFromTile_DEPRECIATED(obj.tileX,obj.tileY, obj.index,true);
                    uimanager.FlashColour(2, uimanager.CutsSmall);
                }                
            }
            return;
        }
    }//end class
}//end namespace