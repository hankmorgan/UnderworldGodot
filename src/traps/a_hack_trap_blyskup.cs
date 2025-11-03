namespace Underworld
{

    /// <summary>
    /// Trap that spawns the bly skup ductosnore when objects are placed in tile
    /// </summary>
    public class a_hack_trap_blyskup : trap
    {
        public static void Activate()
        {
            if (playerdat.GetQuest(122)==1)//has the original ductosnore been killed
            {
                var delgnigzator = objectsearch.FindMatchInTile(
                    tileX:58, 
                    tileY:4,
                    majorclass:2,
                    minorclass:2,
                    classindex:0xE);//in repulsor circle
                if (delgnigzator!=null)
                {
                    var storagecrystal_1  = objectsearch.FindMatchInTile(
                        tileX:57, 
                        tileY:4,
                        majorclass:2,
                        minorclass:2,
                        classindex:1);//in yellow tile
                    if (storagecrystal_1!=null)
                    {
                        if ((storagecrystal_1.quality==2)||(storagecrystal_1.quality==6))
                        {
                            var storagecrystal_2 = objectsearch.FindMatchInTile(
                                tileX:59, 
                                tileY:4,
                                majorclass:2,
                                minorclass:2,
                                classindex:1);//in purple tile
                            
                                if (storagecrystal_2!=null)
                                {
                                    if (storagecrystal_2.quality == 2 || storagecrystal_2.quality == 6 )
                                    {
                                        //remove crystal1 from 57,4
                                        ObjectRemover_OLD.DeleteObjectFromTile_DEPRECIATED(
                                            tileX: storagecrystal_1.tileX, 
                                            tileY: storagecrystal_1.tileY, 
                                            indexToDelete: storagecrystal_1.index);
                                                                                //remove crystal1 from 59,4
                                        ObjectRemover_OLD.DeleteObjectFromTile_DEPRECIATED(
                                            tileX: storagecrystal_2.tileX, 
                                            tileY: storagecrystal_2.tileY, 
                                            indexToDelete: storagecrystal_2.index);
                                    }//note in vanilla if object cannot be removed then move it to 58,3

                                    trigger.TriggerTrapInTile(58,5);
                                    playerdat.SetQuest(122,0);
                                }
                        }//storage1 quality
                    }//storage1
                }//delg
            }//quest
        }
    }//end class
}//end namespace