namespace Underworld
{
    /// <summary>
    /// Some utility code for NPCS
    /// </summary>
    public partial class npc : objectInstance
    {
        /// <summary>
        /// Does some checks that the npc is part of a target race.
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="targetRace"></param>
        /// <returns></returns>
        public static bool CheckIfMatchingRaceUW2(uwObject critter, int targetRace)
        {
            if (critter.majorclass==1)
            {
                if (targetRace==-1)
                {//special case for undead.
                    var testdam = 1;
                    if (damage.ScaleDamage(critter.item_id, ref testdam, 0x80)==0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return (critterObjectDat.race(critter.item_id) == targetRace);
                }
            }
            else
            {
                if (targetRace==-1)
                {
                    if (critter.item_id==0xB)//a skull. possibly a special case for UW2 loths tomb
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
    }//end class
}//end namespace