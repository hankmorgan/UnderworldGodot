using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Area effecting spells.
    /// </summary>
    public partial class SpellCasting : UWClass
    {
        /// <summary>
        /// Used to track what tiles and no of NCPS have been hit by flamewind in UW2.
        /// </summary>
        static int FlameWindGlobal = 0;

        /// <summary>
        /// Accumulated damage caused by Repel undead.
        /// </summary>
        static int RepelUndeadGlobal = 0;
        public static void CastClass6_SpellsAroundPlayer(int minorclass)
        {
            CallBacks.AreaEffectCallBack methodtorun = null;
            //These values come from a table in the exe
            int tileRadius = 0;
            int distanceFromCaster = 0;
            int rngProbablity = 0;
            if (_RES == GAME_UW2)
            {
                switch (minorclass & 0x3F)
                {
                    case 1: //reveal
                        methodtorun = Reveal;
                        rngProbablity = 0x64; distanceFromCaster = 1; tileRadius = 2;
                        break;
                    case 2: //sheetlightning
                        methodtorun = SheetLightning;
                        rngProbablity = 0x6; distanceFromCaster = 4; tileRadius = 2;
                        break;
                    case 3://maybe mass confuse                       
                        methodtorun = Confusion;
                        rngProbablity = 0xC; distanceFromCaster = 4; tileRadius = 2;
                        break;
                    case 4: //flame wind
                        Debug.Print("Flamewind");
                        FlameWindGlobal = 0;
                        methodtorun = FlameWindUW2;
                        rngProbablity = 0xA; distanceFromCaster = 4; tileRadius = 2;
                        break;
                    case 5://repel undead
                        RepelUndeadGlobal = 0;
                        methodtorun = RepelUndead;
                        rngProbablity = 0x32; distanceFromCaster = 4; tileRadius = 2;
                        break;
                    case 6://shockwave
                        methodtorun = Shockwave;
                        rngProbablity = 0x32; distanceFromCaster = 0; tileRadius = 1;
                        break;
                    case 7://frost
                        methodtorun = Frost;
                        rngProbablity = 0x5; distanceFromCaster = 3; tileRadius = 1;
                        break;
                }
            }
            else
            {
                distanceFromCaster = 4;
                rngProbablity = 2;//these values are always the same in uw1.
                switch (minorclass & 0x3F) //the other bits in minorclass define the target type for the area effect spell
                {
                    case 1: //reveal
                        methodtorun = Reveal;
                        break;
                    case 2: //sheetlightning
                        methodtorun = SheetLightning;
                        break;
                    case 3://Mass Confusion
                        methodtorun = Confusion;
                        break;
                    case 4: //flame wind
                        methodtorun = FlameWindUW1;
                        break;
                }
            }
            if (methodtorun != null)
            {
                CallBacks.RunCodeOnTargetsAroundObject(methodtorun, 1, rngProbablity, minorclass & 0xC0, distanceFromCaster, tileRadius);
            }
        }

        /// <summary>
        /// Casts flamewind in a cross shape around each target tile.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="critter"></param>
        /// <param name="tile"></param>
        /// <param name="srcIndex"></param>
        /// <returns></returns>
        public static bool FlameWindUW1(int x, int y, uwObject critter, TileInfo tile, int srcIndex)
        {
            //centre tile
            DamageEffectinTile(tile.tileX, tile.tileY, 2, 1);

            //cross shape
            DamageEffectinTile(tile.tileX + 1, tile.tileY, 2, 1);
            DamageEffectinTile(tile.tileX - 1, tile.tileY, 2, 1);
            DamageEffectinTile(tile.tileX, tile.tileY + 1, 2, 1);
            DamageEffectinTile(tile.tileX, tile.tileY - 1, 2, 1);
            return true;
        }


        /// <summary>
        /// Just spawns a damaging effect in a tile
        /// </summary>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        /// <param name="animoNo">The class index of the animation to play</param>
        /// <param name="damageClass">Lookup into a table of damage parameters for DamageObjectsInTile</param>
        private static void DamageEffectinTile(int tileX, int tileY, int animoNo, int damageClass)
        {
            if (UWTileMap.ValidTile(tileX, tileY))
            {
                var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                if (tile.tileType != 0)
                {
                    animo.SpawnAnimoInTile(animoNo, 3, 3, (short)(tile.floorHeight << 3), tile.tileX, tile.tileY);
                    damage.DamageObjectsInTile(tile.tileX, tile.tileY, 1, damageClass);
                }
            }
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
            Debug.Print($"Flamewind on tile {x},{y}");
            var di = (x << 6) + y;
            var var6 = FlameWindGlobal >> 4;
            var si = FlameWindGlobal & 0xF;
            bool isNPC = false;

            if (targetObject != null)
            {
                Debug.Print($"Flamewind on {targetObject.a_name}");
                isNPC = (targetObject.majorclass == 1);
            }
            if ((var6 != di) && (si < 5))
            {//likely a check that the spell is cast once per tile and hits a max of 5 targets
                if (!isNPC)
                {
                    if (Rng.r.Next(3) != 0)
                    {
                        return false;
                    }
                }
                DamageEffectinTile(tile.tileX, tile.tileY, 2, 1);
                if (isNPC)
                {
                    si++;
                }
                FlameWindGlobal = (di << 4) | (si & 0xF);
            }

            return true;
        }

        /// <summary>
        /// Temporarily raises the search skill to allow for triggering of any look triggers found in the area.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="targetObject"></param>
        /// <param name="tile"></param>
        /// <param name="srcIndex"></param>
        /// <returns></returns>
        public static bool Reveal(int x, int y, uwObject targetObject, TileInfo tile, int srcIndex)
        {
            if (targetObject != null)
            {
                if ((targetObject.is_quant == 0) && (targetObject.link != 0) && (targetObject.majorclass != 6))
                {
                    var looktrigger = objectsearch.FindMatchInObjectChain(targetObject.link, 6, 2, 3, UWTileMap.current_tilemap.LevelObjects);
                    if (looktrigger != null)
                    {
                        var oldSearchSkill = playerdat.Search;
                        playerdat.Search = 0x2D;
                        trigger.TriggerObjectLink(
                            character: 1,
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



        /// <summary>
        /// Makes the target confused and wander around
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="targetObject"></param>
        /// <param name="tile"></param>
        /// <param name="srcIndex"></param>
        /// <returns></returns>
        public static bool Confusion(int x, int y, uwObject targetObject, TileInfo tile, int srcIndex)
        {
            if (targetObject == null)
            {
                if (Rng.r.Next(3) == 0)
                {
                    if (srcIndex == 0)
                    {
                        //spawn twinkies at the player position
                        animo.SpawnAnimoAtPoint(7, main.instance.cam.Position);
                    }
                }
            }
            else
            {
                if (targetObject.majorclass == 1)
                {
                    animo.SpawnAnimoAtPoint(7, targetObject.instance.uwnode.Position);
                    return ApplyAIChangingSpell(targetObject, newgoal: 2, newattitude: 1, newgtarg: 1);
                }
            }
            return false;
        }

        /// <summary>
        /// Spawns lightning bolts in the area.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="targetObject"></param>
        /// <param name="tile"></param>
        /// <param name="srcIndex"></param>
        /// <returns></returns>
        public static bool SheetLightning(int x, int y, uwObject targetObject, TileInfo tile, int srcIndex)
        {
            DamageEffectinTile(tile.tileX, tile.tileY, 5, 2);
            return true;
        }


        /// <summary>
        /// Scares away undead. If undead already scared then applies damage.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="targetObject"></param>
        /// <param name="tile"></param>
        /// <param name="srcIndex"></param>
        /// <returns></returns>
        public static bool RepelUndead(int x, int y, uwObject targetObject, TileInfo tile, int srcIndex)
        {
            if (targetObject == null)
            {
                return false;
            }
            else
            {
                if (targetObject.item_id == 0x13)
                {
                    Debug.Print("A skull in Repel undead?");
                    RepelUndeadGlobal += targetObject.quality;
                    return SmiteUndead(targetObject.index, UWTileMap.current_tilemap.LevelObjects, targetObject.instance.uwnode.Position, playerdat.playerObject);
                }
                else
                {
                    if (targetObject.majorclass == 1)
                    {
                        var testdamage = 1;
                        if (damage.ScaleDamage(targetObject.item_id, ref testdamage, 0x80) == 0)// check if target is undead
                        {
                            if (playerdat.Casting * 0xA >= RepelUndeadGlobal)
                            {
                                bool returnValue;
                                if (targetObject.npc_goal == 6)
                                {//already fleeing
                                    returnValue = damage.DamageObject(targetObject, playerdat.Casting, 3, UWTileMap.current_tilemap.LevelObjects, true, Godot.Vector3.Zero, 1, ignoreVector: true) != 0;
                                }
                                else
                                {
                                    returnValue = ApplyAIChangingSpell(targetObject, newgoal: 6, newgtarg: 1);
                                }
                                animo.SpawnAnimoInTile(0xB, 3, 3, targetObject.zpos, targetObject.tileX, targetObject.tileY);
                                RepelUndeadGlobal += targetObject.npc_hp;
                                return returnValue;
                            }
                        }
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// Applies damage to targets based on player casting skill.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="targetObject"></param>
        /// <param name="tile"></param>
        /// <param name="srcIndex"></param>
        /// <returns></returns>
        public static bool Shockwave(int x, int y, uwObject targetObject, TileInfo tile, int srcIndex)
        {
            if (targetObject == null)
            {
                return false;
            }
            var damagetoapply = 15 + (playerdat.Casting / 2);
            if (targetObject.index != srcIndex)
            {
                if (targetObject.majorclass == 1)
                {
                    animo.SpawnAnimoAtPoint(0xB, targetObject.instance.uwnode.Position);
                    return damage.DamageObject(
                        objToDamage: targetObject,
                        basedamage: damagetoapply,
                        damagetype: 3,
                        objList: UWTileMap.current_tilemap.LevelObjects,
                        WorldObject: true,
                        hitCoordinate: Godot.Vector3.Zero,
                        damagesource: 1,
                        ignoreVector: true) != 0;
                }
            }
            return false;
        }

        /// <summary>
        /// Applies cold damage to a target.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="targetObject"></param>
        /// <param name="tile"></param>
        /// <param name="srcIndex"></param>
        /// <returns></returns>
        public static bool Frost(int x, int y, uwObject targetObject, TileInfo tile, int srcIndex)
        {
            if (targetObject == null)
            {
                animo.SpawnAnimoInTile(0xA, 3,3, (short)(tile.floorHeight<<3),  tile.tileX, tile.tileY);
                return false;
            }
            else
            {
                var damagetoapply = 0xA;
                if (targetObject.index != srcIndex)
                {
                    if (targetObject.majorclass == 1)
                    {
                        animo.SpawnAnimoAtPoint(0xB, targetObject.instance.uwnode.Position);                        
                    }
                    return damage.DamageObject(
                        objToDamage: targetObject,
                        basedamage: damagetoapply,
                        damagetype: 0x23,
                        objList: UWTileMap.current_tilemap.LevelObjects,
                        WorldObject: true,
                        hitCoordinate: targetObject.instance.uwnode.Position,
                        damagesource: 1
                        ) != 0;
                }
            }

            return false;
        }
    }//end class
}//end namespace