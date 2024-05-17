using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Functions related to thievery and illegal actions.
    /// </summary>
    public class thief : UWClass
    {   
        /// <summary>
        /// Currently owner of the stolen item
        /// </summary>
        public static int StolenItemOwner;

        public static void FlagTheftToObjectOwner(uwObject illegallyusedobject, int objectowner)
        {
            StolenItemOwner = 0;
            if (objectowner<=0)
            {
                if (commonObjDat.canhaveowner(illegallyusedobject.item_id))
                {
                    StolenItemOwner = illegallyusedobject.owner;
                }
            }
            else
            {
                StolenItemOwner = objectowner;
            }
            if (StolenItemOwner != 0)
            {
                CallBacks.RunCodeOnTargetsInArea
                (
                    methodToCall: AngerNPCByIllegalAction, 
                    Rng_arg0: 0x14, 
                    srcItemIndex: 0, 
                    typeOfTargetArg8: 0, 
                    tileX0: illegallyusedobject.tileX-7, 
                    tileY0: illegallyusedobject.tileY-7, 
                    xWidth: 15, 
                    yHeight: 15
                );

                if ((objectowner>=0) && (illegallyusedobject.owner<=29))
                {
                    illegallyusedobject.owner=0;
                    //TODO if this object is a container clear owner of it's contents
                }
            }
        }

        
        public static bool AngerNPCByIllegalAction(int x, int y, uwObject obj, TileInfo tile, int srcIndex)
        {
            Debug.Print($"test angering of {obj.a_name}");
            return true;
        }

    }//end class
}//end namespace