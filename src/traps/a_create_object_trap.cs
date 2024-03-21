using System.Diagnostics;
using Godot;

namespace Underworld
{
    public class a_create_object_trap : trap
    {

        /// <summary>
        /// Spawns a copy of the object at trap link. UW1 and UW2 have some differing logic here.
        /// </summary>
        /// <param name="triggerObj"></param>
        /// <param name="trapObj"></param>
        /// <param name="objList"></param>
        public static void activate(uwObject triggerObj, uwObject trapObj, uwObject[] objList)
        {
            if ((trapObj.link!=0) && (trapObj.is_quant==0))
            {
                
                var template = objList[trapObj.link];
                Debug.Print($"Cloning {template.index} {template.a_name}");
                if (template !=null)
                {
                    if (_RES==GAME_UW2)
                    {
                        CreateObjectUW2(triggerObj,trapObj,template,objList);
                    }
                    else
                    {
                        CreateObjectUW1(triggerObj,trapObj,template,objList);
                    }                
                }                            
            } 
        }

        static void CreateObjectUW1(uwObject  triggerObj, uwObject trapObj, uwObject template, uwObject[] objList)
        {
            if (Rng.r.Next(0,63)>= trapObj.quality)
            {
                //there appears to be a logic check here first that runs in the area around the template. 
                //not currently implemented as I suspect it is a check that the template is on the map and/or maybe the player 
                //do spawn
                int slot;
                if (template.IsStatic)
                {
                    //static object spawn
                    slot = ObjectCreator.GetAvailableObjectSlot(ObjectCreator.ObjectListType.StaticList);
                }
                else
                {
                    //mobile object spawn
                    slot = ObjectCreator.GetAvailableObjectSlot(ObjectCreator.ObjectListType.MobileList);
                }
                var newobj = UWTileMap.current_tilemap.LevelObjects[slot];
                //copy from template to new obj
                if (template.IsStatic)
                {
                    for (int i=0;i<8;i++)
                    {
                        newobj.DataBuffer[newobj.PTR + i] = template.DataBuffer[template.PTR + i];
                    }
                }
                else
                {
                    for (int i=0;i<27;i++)
                    {
                        newobj.DataBuffer[newobj.PTR + i] = template.DataBuffer[template.PTR + i];
                    }
                }
                
                //set new position and spawn
                newobj.tileX = triggerObj.quality;
                newobj.tileY = triggerObj.owner;
                if (UWTileMap.ValidTile(newobj.tileX, newobj.tileY))
                {
                    var tile = UWTileMap.current_tilemap.Tiles[newobj.tileX, newobj.tileY];
                    UWTileMap.GetRandomXYZForTile(tile, out int newxpos, out int newypos, out int newzpos);
                    newobj.xpos = (short)newxpos;//obj.xpos;
                    newobj.ypos = (short)newypos;///obj.ypos;
                    newobj.zpos = (short)newzpos; //obj.zpos;
                }
                ObjectCreator.RenderObject(newobj, UWTileMap.current_tilemap);
            }
        }

        /// <summary>
        /// UW2 create object has additional logic relating to loths tomb after loth is freed 
        /// and can spawn leveled creatures if the template object is an adventurer
        /// </summary>
        /// <param name="triggerObj"></param>
        /// <param name="trapObj"></param>
        /// <param name="template"></param>
        /// <param name="objList"></param>
        static void CreateObjectUW2(uwObject  triggerObj, uwObject trapObj, uwObject template, uwObject[] objList)
        {

        }


    }//end class
}//end namespace