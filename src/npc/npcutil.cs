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


        static void SetRaceAttitude(int targetrace, int newAttitute)
        {
            for (var tileX =0; tileX <=63 ; tileX++)
            {
                for (var tileY = 0; tileY <= 63; tileY++)
                {
                    if (UWTileMap.ValidTile(tileX,tileY))
                    {
                        var next = UWTileMap.current_tilemap.Tiles[tileX,tileY].indexObjectList;
                        while (next!=0)
                        {
                            var obj = UWTileMap.current_tilemap.LevelObjects[next];
                            if(obj.majorclass == 1)
                            {//npc
                                if (critterObjectDat.race(obj.item_id) == targetrace)
                                {                                   
                                    obj.npc_attitude = (short)newAttitute;
                                    if (newAttitute==0)
                                    {
                                        obj.ProjectileSourceID=0; //clear last hit index
                                    }
                                }
                            }
                            next = obj.next;                            
                        }
                    }
                }
            }
        }
    }//end class
}//end namespace