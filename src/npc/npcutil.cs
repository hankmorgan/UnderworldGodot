using System.Diagnostics;

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
                    return (critterObjectDat.faction(critter.item_id) == targetRace);
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
                                if (critterObjectDat.faction(obj.item_id) == targetrace)
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

        /// <summary>
        /// For use with delegates
        /// {target,goal}
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="paramsarray"></param>
        public static void set_goal_and_target_by_array(uwObject critter, int[] paramsarray)
        {
            SetGoalAndGtarg(
                critter: critter, 
                goal: paramsarray[1], 
                target: paramsarray[0]);
        }

        /// <summary>
        /// For use with delegates
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="paramsarray"></param>
         public static void set_attitude_by_array(uwObject critter, int[] paramsarray)
         {
            set_attitude(critter, paramsarray[0]);
         }


        /// <summary>
        /// Used by event based attitude changes. Eg avatar going to jail
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="newAttitute"></param>
        public static void set_attitude(uwObject critter, int newAttitute)
        {
            critter.npc_attitude = (short)newAttitute;
            if (newAttitute!=0)
            {
                critter.ProjectileSourceID = 0;
            }
        }

        public static bool moveNPCToTile(uwObject critter, int destTileX, int destTileY)
        {
            Debug.Print($"Moving {critter.a_name} to {destTileX},{destTileY}");
            if (UWTileMap.ValidTile(destTileX,destTileY) && UWTileMap.ValidTile(critter.tileX,critter.tileY))
            {
                var sourceTile = UWTileMap.current_tilemap.Tiles[critter.tileX,critter.tileY];
                var destTile = UWTileMap.current_tilemap.Tiles[destTileX,destTileY];
                if (destTile.tileType != UWTileMap.TILE_SOLID)
                {
                    ObjectRemover_OLD.RemoveObjectFromLinkedList(sourceTile.indexObjectList,critter.index,UWTileMap.current_tilemap.LevelObjects, sourceTile.Ptr+2);              
                    critter.next = destTile.indexObjectList ;
                    destTile.indexObjectList = critter.index;
                    critter.tileX = destTileX; critter.tileY= destTileY;
                    critter.zpos = (short)(destTile.floorHeight<<3);
                    critter.xpos = 3; critter.ypos = 3;
                    critter.npc_xhome = (short)destTileX;
                    critter.npc_yhome = (short)destTileY;
                 
                    //Clear some bits relating to AI
                    critter.UnkBit_0x19_4 = 0;
                    critter.UnkBit_0x19_5 = 0;
                    critter.UnkBit_0x19_0_likelyincombat = 0;
                    critter.UnkBit_0x19_1 = 0;

                    Reposition(critter);
                    return true;
                }
                else
                {
                    Debug.Print($"Attempt to move {critter.a_name} to solid tile {destTileX},{destTileY}");
                    return false;
                }

            }
            return false;
        }

    }//end class
}//end namespace