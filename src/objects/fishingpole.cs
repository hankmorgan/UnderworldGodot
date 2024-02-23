using System;

namespace Underworld
{
    public class fishingpole : objectInstance
    {

        public static bool use(uwObject obj, bool WorldObject)
        {
            if (WorldObject)
            {                    
                return false;
            }
            else
            {
                //go fish

                //check for water tile.
                
                //do a tracking skill check vs ?
                var result = playerdat.SkillCheck(playerdat.Track, 15);
                //check for inventory/weight capacity.

                //spawn fish in hand
                return true;
            }
        }

    }//end class
}//end namespace