using System.Diagnostics;

namespace Underworld
{
    public class smallblackrockgem:objectInstance
    {

        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (!WorldObject)
            {
                //flag we are using the key
                useon.CurrentItemBeingUsed = new useon(obj, WorldObject);
                //print use message
                uimanager.AddToMessageScroll(useon.CurrentItemBeingUsed.GeneralUseOnString);
            }
            return true;
        }

        public static bool UseOn(uwObject smallGem, uwObject itemUsedOn, bool WorldObject)
        {
            //assume small gem in inventory, largegem in world
            if (itemUsedOn.item_id==0x168)
            {//used on large gem
                if (smallGem.owner==1)
                {//small gem has been treated
                    var gemNo = smallGem.classindex-7;
                    if (gemNo==4)
                        {
                            playerdat.IncrementXClock(1);
                        }
                    var shakeintensity = playerdat.GetXClock(2)<<2;
                    special_effects.SpecialEffect(4, shakeintensity);
                    uimanager.AddToMessageScroll(GameStrings.GetString(1,0x152));
                    playerdat.IncrementXClock(2);
                    var shakemessage = 0x152 + playerdat.GetXClock(2);
                    if ((gemNo & 0x6) == 6)
                        { //swap gems 6 and 7 around
                            gemNo = 0xD - gemNo;
                        }

                    var quest130 = playerdat.GetQuest(130);
                    quest130 = quest130 | (1<<(gemNo-1));
                    playerdat.SetQuest(130, quest130);

                   ObjectCreator.Consume(smallGem, true);
                    //Debug.Print("Play sound effect 0x12h");

                    quest130 = playerdat.GetQuest(130);
                    if (quest130==0xFF)
                    {
                        //all gems used
                         //Debug.Print("Play Sound Effect 0x14h at 0x40,0x2c");
                       
                    }
                    else
                    {
                         //Debug.Print("Play Sound Effect 0x14h  at 0x40,0x2a");
                    }
                    return true;
                }
            }
            uimanager.AddToMessageScroll(GameStrings.GetString(1,0x15B));//The key gem remains inert in your hand
            return true;
        }

        public static bool LookAt(uwObject obj, uwObject[] objList)
        {
            var warmth = "";
            if (obj.owner==1)
                {
                    warmth = GameStrings.GetString(1,357); //warm
                }
            else
                {
                    warmth = GameStrings.GetString(1,356); //cool
                }

            uimanager.AddToMessageScroll($"{GameStrings.GetString(1,GameStrings.str_you_see_)}a {warmth}{GameStrings.GetSimpleObjectNameUW(obj.item_id)}");
            return true;
        }
    }//end calss
}//end namespace