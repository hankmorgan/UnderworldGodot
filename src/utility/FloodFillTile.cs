namespace Underworld
{
    public class FloodFillTile
    {
        public int Tiletype;
        public bool Tested;
        public bool Accessible;
        public int ActualX;
        public int ActualY;
        public bool ClosedDoor;//mark that the tile has a closed door.



        public FloodFillTile(int _actualX, int _actualY, bool _accessible, bool _tested, int _tiletype, bool _closedDoor)
        {
            Tiletype = _tiletype;
            ActualX = _actualX;
            ActualY = _actualY;
            Accessible = _accessible;
            Tested = _tested;
            ClosedDoor = _closedDoor;
        }


        /// <summary>
        /// builds a local grid containing tile info to allow a floodfill to be ran on an area for automap updates.
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static FloodFillTile[,] FloodFillSubArea(int startX, int startY, int range)
        {
            var FillArea = new FloodFillTile[1+range*2, 1+range*2];
            var CentreX = 0; var CentreY =0;
            //copy data into the array.
            var x = 0;
            var y = 0;
            for (var aX = startX - range; aX <= startX + range; aX++)
            {
                y = 0;
                for (var aY = startY - range; aY <= startY + range; aY++)
                {
                    if ((aX==startX) && (aY==startY))
                        {//get the start position.
                            CentreX =x; CentreY = y;
                        }
                    if (UWTileMap.ValidTile(aX, aY))
                    {
                        bool _closedDoor = false;
                        if (UWTileMap.current_tilemap.Tiles[aX, aY].HasDoor)
                        {
                            var doorObj = UWTileMap.current_tilemap.LevelObjects[UWTileMap.current_tilemap.Tiles[aX, aY].DoorIndex];
                            if (doorObj != null)
                            {
                                //var d = (door)(doorObj.instance);
                                if (!door.isOpen(doorObj))
                                {
                                    _closedDoor = true;
                                }
                            }
                        }
                        FillArea[x, y] = new FloodFillTile(
                            _actualX: aX,
                            _actualY: aY,
                            _accessible: false,
                            _tested: false,
                            _tiletype: UWTileMap.current_tilemap.Tiles[aX, aY].tileType,
                            _closedDoor: _closedDoor);
                    }
                    else
                    {
                        //Flag as tested, inaccessible.
                        FillArea[x, y] = new FloodFillTile(
                            _actualX: aX,
                            _actualY: aY,
                            _accessible: false,
                            _tested: true,
                            _tiletype: 0,
                            _closedDoor: false);
                    }
                    y++;
                }
                x++;
            }           

            FloodFill(CentreX, CentreY, ref FillArea);

            return FillArea;
        }


        static void FloodFill(int x, int y, ref FloodFillTile[,] fill)
        {
            //Stop on out of bounds.

            if ((x < 0) || (x > fill.GetUpperBound(0))) { return; }

            if ((y < 0) || (y > fill.GetUpperBound(1))) { return; }

            //Stop on already tested.   
            if (fill[x, y].Tested) { return; }            

            fill[x, y].Tested = true;
            fill[x, y].Accessible = true;

            if (fill[x, y].ClosedDoor) { return; }
            switch (fill[x, y].Tiletype)

            {

                case UWTileMap.TILE_SOLID:  //solid stop. go no further
                    return;

                case UWTileMap.TILE_OPEN: //open tile. check in all directions
                case UWTileMap.TILE_SLOPE_N:
                case UWTileMap.TILE_SLOPE_E:
                case UWTileMap.TILE_SLOPE_W:
                case UWTileMap.TILE_SLOPE_S:
                    FloodFill(x + 1, y, ref fill);
                    FloodFill(x - 1, y, ref fill);
                    FloodFill(x, y + 1, ref fill);
                    FloodFill(x, y - 1, ref fill);
                    return;

                case UWTileMap.TILE_DIAG_NE://Open to the north and east
                    FloodFill(x + 1, y, ref fill);
                    FloodFill(x, y + 1, ref fill);
                    return;

                case UWTileMap.TILE_DIAG_NW://Open to the north and west
                    FloodFill(x + 1, y, ref fill);
                    FloodFill(x, y - 1, ref fill);
                    return;

                case UWTileMap.TILE_DIAG_SE://Open to the south and east
                    FloodFill(x - 1, y, ref fill);
                    FloodFill(x, y + 1, ref fill);
                    return;

                case UWTileMap.TILE_DIAG_SW://Open to the south and west
                    FloodFill(x - 1, y, ref fill);
                    FloodFill(x, y - 1, ref fill);
                    return;

            }

        }

    }//end class

}//end namespace