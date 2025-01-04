using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Trap which handles the puzzle in UW1 that spawns a VAS rune when 4 emeralds are placed on tiles.
    /// </summary>
    public class a_do_trap_emeraldpuzzle : hack_trap
    {
        public static void Activate(uwObject trapObj, int triggerX, int triggerY)
        {
            var counter = 0;
            var offsetX = -4;
            int[] foundEmeralds = new int[4];
            int[] foundEmeraldsX = new int[4];
            int[] foundEmeraldsY = new int[4];
            
            
            while (offsetX<5)
            {
                var offsetY = -4;
                while (offsetY<5)
                {
                    if (UWTileMap.ValidTile(triggerX+offsetX, triggerY+offsetY))
                    {
                        Debug.Print($"Testing tile {triggerX+offsetX},{triggerY+offsetY}");
                        var tile = UWTileMap.current_tilemap.Tiles[triggerX+offsetX, triggerY+offsetY];
                        var emerald = objectsearch.FindMatchInObjectListChainNextObjectsOnly(
                            ListHeadIndex: tile.indexObjectList, 
                            majorclass: 2, minorclass: 2, classindex: 7, 
                            objList: UWTileMap.current_tilemap.LevelObjects);
                        if (emerald!=null)
                        {
                            Debug.Print($"Found Emerald in {triggerX+offsetX},{triggerY+offsetY}");
                            foundEmeralds[counter] = emerald.index;
                            foundEmeraldsX[counter] = tile.tileX;
                            foundEmeraldsY[counter] = tile.tileY;
                            counter++;
                        }
                    }                    
                    offsetY += 8;
                }
                offsetX += 8;  
            }
            if (counter == 4)//four emeralds have been found
            {
                for (int i=0; i<4;i++)
                {
                    if (UWTileMap.ValidTile(foundEmeraldsX[i], foundEmeraldsY[i]))
                    {
                        ObjectRemover.DeleteObjectFromTile(
                            tileX: foundEmeraldsX[i], tileY: 
                            foundEmeraldsY[i], 
                            indexToDelete: (short)foundEmeralds[i]);
                    }
                    else
                    {
                        return;//not a valid tile. Should not happen.
                    }
                }
                //spawn the Vas stone
                ObjectCreator.spawnObjectInTile(
                    itemid:0xFD, 
                    tileX: triggerX, tileY: triggerY + 1, 
                    xpos:6,
                    ypos:6,
                    zpos: 0x40);
            }
        }
    }//end class
}//end namesace