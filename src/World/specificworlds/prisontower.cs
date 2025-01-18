namespace Underworld
{
    /// <summary>
    /// Specific code for the prison tower
    /// </summary>
    public class prisontower:UWClass
    {

        public static void PrisonTowerQuest60()
        {
            if (playerdat.GetQuest(60)==0)
            {
                CallBacks.RunCodeOnAllNPCS(TestForQuest60, null);
            }
        }

        static void TestForQuest60(uwObject obj, int[] paramsarray)
        {
            if (obj.majorclass==1)
            {
                if (obj.npc_attitude==0)
                {
                    if (npc.CheckIfMatchingRaceUW2(obj, 6))
                    {
                        if (obj.UnkBit_0XA_Bit7 == 0)
                        {
                            playerdat.SetQuest(60,1);
                            playerdat.IncrementXClock(15);
                        }
                    }
                }
            }
        }
    }
}