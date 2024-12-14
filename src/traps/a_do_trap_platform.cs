using System;
using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Trap which moves a platform up and down
    /// </summary>
    public class a_do_trap_platform : hack_trap
    {

        public static void Activate(uwObject trapObj, int triggerX, int triggerY, uwObject[] objList)
        {

            if (ObjectThatStartedChain != 0)
            {                    
                var obj = UWTileMap.current_tilemap.LevelObjects[ObjectThatStartedChain];       
                var si = trapObj.zpos + (obj.flags<<3);

                if (trapObj.quality != 3)
                {
                    if (trapObj.link!=0)
                    {//Not sure what scenario this code will occur?
                        var linked = UWTileMap.current_tilemap.LevelObjects[trapObj.link];
                        linked.zpos = (short)si;//?
                        objectInstance.Reposition(linked);//?
                    }                    
                }
                else
                {                        
                    if (si<0x68)
                    {
                        TileInfo.ChangeTile(
                            StartTileX: triggerX, StartTileY: triggerY,
                            newHeight: (si>>3));
                    }
                }
            }
        }
    } //end class
}//end namespace