using System;
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
        public static uwObject IllegalObject;

        public static void FlagTheftToObjectOwner(uwObject illegallyusedobject, int objectowner)
        {
            IllegalObject = null;
            IllegalObject = illegallyusedobject;
            StolenItemOwner = 0;
            if (objectowner<=0)
            {
                if (commonObjDat.canhaveowner(illegallyusedobject.item_id))
                {
                    StolenItemOwner = illegallyusedobject.owner & 0x3F;
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

        
        public static bool AngerNPCByIllegalAction(int x, int y, uwObject critter, TileInfo tile, int srcIndex)
        {
            
            Debug.Print($"test angering of {critter.a_name}");

            if (critterObjectDat.generaltype(critter.item_id) == (StolenItemOwner & 0x1F))
            {
                if (critter.UnkBit_0XA_Bit7 != 0)
                {
                    if ((StolenItemOwner & 0x20)==0)
                    {
                        return false;
                    }
                }
                if (StolenItemOwner == 0x20)
                {//is this a uw2 special case??
                    if (critter.UnkBit_0XA_Bit7==0)
                    {
                        return false;
                    }
                }
                if (_RES!=GAME_UW2)
                {
                    if (StolenItemOwner==0xD)
                    {
                        if (playerdat.GetQuest(32)>=3)
                        {//once player becomes a knight taking knight owned objects is not considered theft
                            return false;
                        }
                    }
                }
                //ovr107_F1F
                var npc_xcoord = (critter.tileX<<3) + critter.xpos;
                var npc_ycoord = (critter.tileY<<3) + critter.ypos;
                var npc_zcoord = commonObjDat.height(critter.item_id) + critter.zpos;
            
                
                var stolenitem_xcoord = (IllegalObject.tileX<<3) + IllegalObject.xpos;
                var stolenitem_ycoord = (IllegalObject.tileY<<3) + IllegalObject.ypos;
                var stolenitem_zcoord = commonObjDat.height(IllegalObject.item_id) + IllegalObject.zpos + 12;
            
                var xdiff = (npc_xcoord-stolenitem_xcoord) /8;
                var ydiff = (npc_ycoord-stolenitem_ycoord) /8;

                var dist = xdiff*xdiff + ydiff*ydiff;
                if (dist <= critterObjectDat.theftdetectionrange(critter.item_id))
                {
                    if (pathfind.TestBetweenPoints(npc_xcoord,npc_ycoord,npc_zcoord, stolenitem_xcoord, stolenitem_ycoord,stolenitem_zcoord))
                    {
                        critter.npc_attitude = (short)Math.Max(critter.npc_attitude-1,0);
                        var msg = $"{critter.a_name}{GameStrings.GetString(1,GameStrings.str__is_angered_by_your_action_)}";
                        uimanager.AddToMessageScroll(msg);
                    }
                }
            }            
            return false;
        }

    }//end class
}//end namespace