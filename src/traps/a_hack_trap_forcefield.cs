namespace Underworld
{
    /// <summary>
    /// Trap which controls the behaviour of force fields that can be bypassed when fraznium gear is worn
    /// </summary>
    public class a_hack_trap_forcefield : hack_trap
    {
        public static void Activate(uwObject trapObj, int triggerX, int triggerY, uwObject[] objList)
        {
            //param owner
            bool foundFraznium=false;
            if (trapObj.owner==0)
            {
                if (playerdat.GlovesObject!=null)
                {
                    if (playerdat.GlovesObject.item_id == 0x33)//gloves
                    {
                        foundFraznium = true;
                    }
                }
                if (playerdat.HelmObject!=null)
                {
                    if (playerdat.HelmObject.item_id==0x34) // circlet
                    {
                        foundFraznium = true;
                    }
                }
            }

            var tile = UWTileMap.current_tilemap.Tiles[triggerX, triggerY];
            if (tile.indexObjectList!=0)
            {
                var ff = objectsearch.FindMatchInObjectListChainNextObjectsOnly(
                    ListHeadIndex: tile.indexObjectList, 
                    majorclass: 5, minorclass: 2, classindex: 0xD, 
                    objList: UWTileMap.current_tilemap.LevelObjects);
                if (ff!=null)
                {
                    if (foundFraznium)
                    {
                        ff.zpos = 127;
                    }
                    else
                    {
                        ff.zpos = 0;
                    }
                    objectInstance.Reposition(ff);//do the object move
                }
            }

        }
    }//end class
}//end namespace