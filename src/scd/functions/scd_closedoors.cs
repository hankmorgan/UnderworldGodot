using System.Diagnostics;

namespace Underworld
{
    public partial class scd : UWClass
    {
        public static void FindAndCloseDoors(byte[] currentblock, int eventOffset)
        {
            FindAndClose(currentblock[eventOffset+6]);
        }

        static void FindAndClose(int arg0)
        {
            door.SkipDoorSound = true;
            int x=0; int y=0;

            while ((x < 64) && (y < 64))
            {
                var d = objectsearch.FindObjectOnMapByPosition(
                    startIndex: (x*64)+y,
                    majorclass: 5,
                    minorclass: 0,
                    classindex: -1,
                    objList: UWTileMap.current_tilemap.LevelObjects,
                    FoundX: out int foundX, FoundY: out int foundY);
                
                if (d != null)
                {
                    //Debug.Print($"Door found at {foundX},{foundY}");
                    //the next door has been found
                    if (UWTileMap.ValidTile(foundX, foundY))
                    {
                        var tile = UWTileMap.current_tilemap.Tiles[foundX, foundY];
                        if (tile.doorBit == 0)
                        {
                            if (d.classindex >= 8)
                            {   //door is open
                                if (Rng.r.Next(2) == 0)
                                {//50:50 chance
                                    if (TileInfo.CheckIfOutsideRange(foundX, foundY, arg0, 8))
                                    {
                                    Debug.Print($"SCD Closing Door at {foundX} {foundY}");
                                        door.CloseDoor(d);
                                    }
                                }
                            }
                        }
                    }
                    x = foundX; y=foundY+1;//start next search from next tile
                }
                else
                {
                    //no door found
                    if (arg0 == 0)
                    {
                        for (int si = 0; si < 8; si++)
                        {
                            AnimationOverlay.UpdateAnimationOverlays();//update animation overlays 8 times to complete all door animations running.
                        }
                    }
                    x=65;y=65;//end loop
                }
            }


            door.SkipDoorSound = false;
        }
    }//end class
}//end namespace