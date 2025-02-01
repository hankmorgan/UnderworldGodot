using System.Diagnostics;

namespace Underworld
{
    public partial class SpellCasting : UWClass
    {
        static int FlameWindGlobal = 0;
        public static void CastClass6_SpellsAroundPlayer(int minorclass)
        {        
            CallBacks.AreaEffectCallBack methodtorun = null;    
            //These values come from a table in the exe
            int tileRadius = 0;
            int distanceFromCaster = 0;
            int rngProbablity=0;
            if (_RES == GAME_UW2)
            {
                switch (minorclass & 0x3F)
                {
                    case 1: //reveal
                        methodtorun = Reveal;
                        rngProbablity = 0x64; distanceFromCaster = 1; tileRadius = 2;
                        break;
                    case 2: //sheetlightning
                        Debug.Print("Sheetlighning");
                        break;
                    case 3://maybe mass confuse
                        Debug.Print("mass confuse?");
                        break;
                    case 4: //flame wind
                        Debug.Print("Flamewind");
                        FlameWindGlobal = 0;
                        methodtorun = FlameWindUW2;
                        rngProbablity = 0xA; distanceFromCaster = 4; tileRadius = 2;
                        break;
                    case 5://repel undead
                        Debug.Print ("Repel undead");
                        break;
                    case 6://shockwave
                        Debug.Print("Shockwave");
                        break;
                    case 7://frost
                        Debug.Print ("Frost");
                        break;
                }
            }
            else
            {
                Debug.Print($"{minorclass & 0x3F}");
                distanceFromCaster = 4;
                rngProbablity=2;//these values are always the same in uw1.
                switch (minorclass & 0x3F)
                {
                    case 1: //reveal
                        methodtorun = Reveal;
                        break;
                    case 2: //sheetlightning
                        Debug.Print("Sheetlighning");
                        break;
                    case 3://maybe mass confuse
                       Debug.Print("mass confuse?");
                       break;
                    case 4: //flame wind
                        methodtorun = FlameWindUW1;
                        Debug.Print("Flamewind");
                        break;
                }
            }
            if (methodtorun!=null)
            {
                CallBacks.RunCodeOnTargetsAroundObject(methodtorun,0, rngProbablity, minorclass&0xC0, distanceFromCaster, tileRadius);
            }            
        }

        public static bool FlameWindUW1(int x, int y, uwObject critter, TileInfo tile, int srcIndex)
        {//uw1 casts flamewind in a cross shape around each target tile.
            return true;
        }
        

        /// <summary>
        /// Spawns fiery explosions on the target location
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="targetObject"></param>
        /// <param name="tile"></param>
        /// <param name="srcIndex"></param>
        /// <returns></returns>
        public static bool FlameWindUW2(int x, int y, uwObject targetObject, TileInfo tile, int srcIndex)
        {
            Debug.Print ($"Flamewind on tile {x},{y}");
            var di = (x<<6) + y;
            var var6 = FlameWindGlobal >>4;
            var si = FlameWindGlobal & 0xF;
            bool isNPC=false;

            if (targetObject!=null)
            {
                Debug.Print($"Flamewind on {targetObject.a_name}");
                isNPC = (targetObject.majorclass ==1);
            }
            if ((var6 != di) && (si<5))
            {//likely a check that the spell is cast once per tile and hits a max of 5 targets
                if (!isNPC)
                {
                    if (Rng.r.Next(3) !=0)
                    {
                        return false;
                    }
                }

                animo.SpawnAnimoInTile(2, 3,3, (short)(tile.floorHeight<<3), tile.tileX, tile.tileY);
                damage.DamageObjectsInTile(x, y, 1, 1);
                if (isNPC)
                {
                    si++;
                }
                FlameWindGlobal = (di<<4) | (si & 0xF);
            }
            
            return true;
        }

        public static bool Reveal(int x, int y, uwObject targetObject, TileInfo tile, int srcIndex)
        {
            if (targetObject !=null)
            {
                if ((targetObject.is_quant == 0) && (targetObject.link!=0) && (targetObject.majorclass!=6))
                {
                    var looktrigger = objectsearch.FindMatchInObjectChain(targetObject.link, 6, 2, 3, UWTileMap.current_tilemap.LevelObjects);
                    if (looktrigger!=null)
                    {
                        var oldSearchSkill = playerdat.Search;
                        playerdat.Search = 0x2D;
                        trigger.TriggerObjectLink(
                            character: 0,
                            ObjectUsed: targetObject,
                            triggerType: (int)triggerObjectDat.triggertypes.LOOK,
                            triggerX: targetObject.tileX,
                            triggerY: targetObject.tileY,
                            objList: UWTileMap.current_tilemap.LevelObjects);
                        playerdat.Search = oldSearchSkill;
                        return true;
                    }
                }
            }            
            return false;            
        }
    }//end class
}//end namespace