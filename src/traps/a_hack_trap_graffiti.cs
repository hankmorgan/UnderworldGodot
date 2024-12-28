using System.Diagnostics;

namespace Underworld
{

    /// <summary>
    /// Trap that changes the texture used by TMAPS based on probability and preset values
    /// </summary>
    public class a_hack_trap_graffiti : trap
    {
       
        public static void Activate(uwObject trapObj, int triggerX, int triggerY)
        {
            var newTexture = 0x25;
            var ToChange = 0x3E;
            var changeProbability = 0;

            switch (trapObj.owner-1)
            {
                case 0:
                    ToChange = 0x3E;
                    newTexture = 0x25;
                    changeProbability = 0x10;
                    break;
                case 1:
                    ToChange = 0x3E;
                    newTexture = 0x2A;
                    changeProbability = 0xA;
                    break;
                case 2:
                    ToChange = 0x3D;
                    newTexture =0x3C;
                    changeProbability = 6;
                    break;
                case 3:
                    ToChange = 0x3B;
                    newTexture = 0x3A;
                    changeProbability = 6;
                    break;
                case 4:
                    ToChange = 0x29;
                    newTexture = 0x2A;
                    changeProbability = 0x10;
                    break;
            }

            for (int x = triggerX; x<triggerX+8;x++)
            {
                for (int y = triggerY; y<triggerY+8;y++)
                {
                    if (UWTileMap.ValidTile(x,y))
                    {
                        var tile = UWTileMap.current_tilemap.Tiles[x,y];
                        if (tile.indexObjectList!=0)
                        {
                            var next = tile.indexObjectList;
                            while (next!=0)
                            {
                                var obj = UWTileMap.current_tilemap.LevelObjects[next];
                                
                                if (obj.item_id == 366)
                                {//tmap
                                    if (obj.owner == ToChange)
                                    {
                                        if ((Rng.r.Next(0x10) < changeProbability) || (true))
                                        {
                                            Debug.Print($"Changing Tmap {obj.index} at {x} {y} to use texture {newTexture}");
                                            obj.owner = (short)newTexture;
                                            objectInstance.RedrawFull(obj);
                                        }
                                    }
                                }

                                next = obj.next;
                            }
                        }
                    }
                }
            }
        }
    }//end class
}//end namespace