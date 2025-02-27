using System;
using System.Diagnostics;
namespace Underworld
{
    /// <summary>
    /// Class for managing damage to objects.
    /// </summary>
    public class damage : UWClass
    {
        public static void DamagePlayer(int basedamage, int damagetype, int damagesource)
        {
            Debug.Print("TODO further implement this");
            ScaleDamage(playerdat.playerObject.item_id, ref basedamage, damagetype);
            playerdat.playerObject.ProjectileSourceID = (short)damagesource;

            if (basedamage < playerdat.play_hp)
            {
                playerdat.play_hp -= basedamage;
            }
            else
            {
                playerdat.play_hp = 0;
            }
        }

        /// <summary>
        /// Applies damage to objects
        /// </summary>
        /// <param name="objToDamage"></param>
        /// <param name="basedamage"></param>
        /// <param name="damagetype"></param>
        /// <param name="damagesource"></param>
        public static int DamageObject(uwObject objToDamage, int basedamage, int damagetype, uwObject[] objList, bool WorldObject, Godot.Vector3 hitCoordinate, int damagesource, bool ignoreVector = false)
        {
            ScaleDamage(objToDamage.item_id, ref basedamage, damagetype);
            Debug.Print($"Try and Damage {objToDamage.a_name} by {basedamage}");
            if (objToDamage.majorclass == 1)
            {
                return DamageNPC(
                    critter: objToDamage,
                    basedamage: basedamage,
                    damagetype: damagetype,
                    damagesource: damagesource);
            }
            else
            {
                if (DamageOtherObjectTypes(objToDamage, basedamage, 0))
                {
                    //object should be destroyed
                    return ObjectDestruction(
                        objToDestroy: objToDamage,
                        damagetype: damagetype,
                        objList: objList,
                        WorldObject: WorldObject,
                        hitcoordinate: hitCoordinate,
                        ignoreVector: ignoreVector);
                }
            }
            return 0;
        }


        /// <summary>
        /// Applies damage to the npc with the specified damage type
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="damage"></param>
        /// <param name="damagetype"></param>
        static int DamageNPC(uwObject critter, int basedamage, int damagetype, int damagesource)
        {
            ScaleDamage(critter.item_id, ref basedamage, damagetype);

            Debug.Print($"Damage {critter.a_name} by {basedamage}");

            //Note to be strictly compatable with UW behaviour the damage should be accumulated for the npc and test
            //once per tick This is used to control the angering behaviour of the npc in checking against passiveness.   
            critter.AccumulatedDamage += (short)basedamage;//how much total damage has been applied in this tick.        
            critter.npc_hp = (byte)Math.Max(0, critter.npc_hp - basedamage);
            
            //make the npc react to the damage source. player if 0
            //record the damage source as the player
            Debug.Print($"Record damage source as {damagesource}");

            critter.ProjectileSourceID = (short)damagesource;
            if (damagesource == 1)
            {//player applied damage
                playerdat.LastDamagedNPCIndex = critter.index;
                playerdat.LastDamagedNPCType = critterObjectDat.generaltype(critter.item_id);
                playerdat.LastDamagedNPCTime = playerdat.ClockValue;
                playerdat.LastDamagedNPCTileX = critter.tileX;
                playerdat.LastDamagedNPCTileY = critter.tileY;
                playerdat.LastDamagedNPCZpos = critter.zpos;
            }

            if (critter.npc_hp == 0)
            {
                if (
                    (_RES == GAME_UW2) && (critter.npc_animation != 7)
                    ||
                    (_RES != GAME_UW2) && (critter.npc_animation != 0xC)
                )
                { //if not already in the death animation
                    if (npc.SpecialDeathCases(critter))
                    {
                        if (_RES == GAME_UW2)
                        {
                            critter.npc_animation = 7;//are these right??                            
                        }
                        else
                        {
                            critter.npc_animation = 0xC;//
                        }
                        critter.AnimationFrame = 0;
                        npc.RedrawAnimation(critter);
                        return 1;
                    }
                }
            }

            return 0;
        }


        static bool DamageOtherObjectTypes(uwObject objToDamage, int basedamage, int damagesource)
        {
            if(objToDamage.majorclass == 6)
            {
                Debug.Print("Do not damage traps/trigger!");
                return false;
            }
            bool IsBroken = false;
            var qualityclass = commonObjDat.qualityclass(objToDamage.item_id);
            if (objToDamage.doordir == 1)
            {
                if (objToDamage.OneF0Class != 0x14)
                {//door dir set and not a door.
                    return false;
                }
            }
            if (qualityclass == 3)
            {
                Debug.Print($"{objToDamage.a_name} cannot be damaged");
                return false;//invulnerable object
            }

            basedamage >>= qualityclass;//reduce damage based on quality class
            if (objToDamage.IsStatic)
            {//damage to a static object

                if (
                    (objToDamage.item_id >= 0x140) && (objToDamage.item_id <= 0x147)
                    &&
                    ((objToDamage.owner & 0x1) == 1)
                    &&
                    ((objToDamage.owner >> 1) > 0)
                )
                {
                    //damage to spiked closed doors?
                    var finalquality = Math.Max(0, (objToDamage.owner >> 1) - basedamage);
                    finalquality <<= 1;
                    finalquality = (objToDamage.owner & 0x1) | finalquality;
                    finalquality &= 0x3F;
                    objToDamage.owner = (short)finalquality; //apply damage to owner instead.
                    Debug.Print($"Final owner(spiked door) is {objToDamage.owner}");
                }
                else
                {
                    var finalquality = Math.Max(0, objToDamage.quality - basedamage);
                    objToDamage.quality = (short)finalquality;
                    Debug.Print($"Final quality is {objToDamage.quality}");
                    IsBroken = (finalquality == 0);
                    if (commonObjDat.canhaveowner(objToDamage.item_id))
                    {
                        Debug.Print($"Flag trespass for owner {objToDamage.owner}");
                    }
                }
            }
            else
            {//damage to a mobile object (excl NPCS)
                var finalquality = Math.Max(0, objToDamage.npc_hp - basedamage);
                objToDamage.npc_hp = (byte)finalquality;
                Debug.Print("Final hp is {objToDamage.npc_hp}");
                IsBroken = (finalquality == 0);
            }

            if ((IsBroken) && (objToDamage.IsStatic))
            {
                if (UWTileMap.ValidTile(objToDamage.tileX, objToDamage.tileY))
                {
                    trigger.TriggerObjectLink(
                            character: damagesource,
                            ObjectUsed: objToDamage,
                            triggerType: (int)triggerObjectDat.triggertypes.USE,
                            triggerX: objToDamage.tileX,
                            triggerY: objToDamage.tileY,
                            objList: UWTileMap.current_tilemap.LevelObjects);
                }
            }
            return IsBroken;
        }


        /// <summary>
        /// Scales damage on the NPC based on it's vulnerabilities defined in critter object data
        /// Spawns an animo of the specified type to represent blood etc
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="basedamage"></param>
        /// <param name="damagetype"></param>
        /// <param name="UpdateUI"></param>
        // public static void ScaledDamageOnNPCWithAnimo(uwObject critter, int basedamage, int damagetype, int animoclassindex, Godot.Vector3 hitCoordinate, bool UpdateUI = true)
        // {
        //     var noOfSplatters = basedamage;

        //     noOfSplatters = noOfSplatters / 4;
        //     if (noOfSplatters > 3)
        //     {
        //         noOfSplatters = 3;
        //     }
        //     if (hitCoordinate!=Vector3.Zero)
        //     {
        //         animo.SpawnAnimoAtPoint(animoclassindex, hitCoordinate);
        //     }            
        //     Debug.Print($"Spawn animo {animoclassindex} {noOfSplatters} times");

        //     DamageNPC(critter, basedamage, damagetype);
        // }

        /// <summary>
        /// Handles destruction of a damage object and replaces it with appropiate debris objects
        /// </summary>
        /// <param name="objToDestroy"></param>
        /// <param name="damagetype"></param>
        /// <param name="objList"></param>
        /// <param name="WorldObject"></param>
        /// <returns>1 if debris is set</returns>
        public static int ObjectDestruction(uwObject objToDestroy, int damagetype, uwObject[] objList, bool WorldObject, Godot.Vector3 hitcoordinate, bool ignoreVector = false)
        {
            int Debris = -2;
            if ((!UWTileMap.ValidTile(objToDestroy.tileX, objToDestroy.tileY)) && (WorldObject))
            {
                return 0;
            }
            if (objToDestroy.OneF0Class == 0x14)
            {
                //doors
                if (objToDestroy.classindex <= 7)
                {
                    a_lock.SetIsLocked(objToDestroy, false, 0);
                    door.OpenDoor(objToDestroy);
                    return 1;
                }
            }
            switch (objToDestroy.item_id)
            {
                case 0x15B://barrel
                case 0x15D://chest
                    {
                        if (objToDestroy.doordir == 0)
                        {
                            a_lock.SetIsLocked(objToDestroy, false, 0);
                            use.Use(objToDestroy, null, objList, WorldObject);
                        }
                        break;
                    }
                default:
                    {
                        if ((objToDestroy.OneF0Class == 0) && (damagetype != 8))
                        {//weapons
                            if (
                                (objToDestroy.item_id == 3) && (_RES == GAME_UW2)
                                ||
                                (objToDestroy.item_id == 0x10) && (_RES == GAME_UW2)
                                )
                            {
                                Debris = 0xC7;
                            }
                            else
                            {
                                if (_RES == GAME_UW2)
                                {
                                    Debris = 0xC5 + weaponObjectDat.skill(objToDestroy.item_id);
                                }
                                else
                                {
                                    Debris = 0xC8 + weaponObjectDat.skill(objToDestroy.item_id);
                                }
                            }
                        }
                        else
                        {
                            if (objToDestroy.OneF0Class == 8)
                            {//containers
                                Debug.Print("TODO Cull Container behaviour to be implemented");
                                //Spill container.
                                if (WorldObject)
                                {
                                    container.SpillWorldContainer(objToDestroy);
                                }
                            }
                            else
                            {
                                if ((_RES == GAME_UW2) && (objToDestroy.item_id == 0x116))
                                {
                                    djinnbottle.DestroyDjinnBottle(objToDestroy, WorldObject);
                                }
                                else
                                {
                                    //default destruction logic
                                    if (objToDestroy.item_id == 0xD6)
                                    {//already debris, remove it
                                        if (WorldObject)
                                        {
                                            ObjectRemover.DeleteObjectFromTile(
                                                tileX: objToDestroy.tileX,
                                                tileY: objToDestroy.tileY,
                                                indexToDelete: objToDestroy.index,
                                                RemoveFromWorld: true);
                                        }
                                        Debris = -1;
                                        return 1;
                                    }
                                    else
                                    {
                                        if (((Rng.r.Next(0, 0x7fff) & 0x3) != 0) || (true))
                                        {
                                            if (!ignoreVector)
                                            {
                                                if (hitcoordinate != Godot.Vector3.Zero)
                                                {
                                                    animo.SpawnAnimoAtPoint(8, hitcoordinate);
                                                }
                                            }
                                            Debris = 0xD6;
                                        }

                                        if ((objToDestroy.is_quant == 0) && (objToDestroy.link > 0))
                                        {
                                            Debug.Print("TODO Clear object chain");
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
            }

            if (Debris <= -1)
            {
                if (_RES == GAME_UW2)
                {
                    Debris = GetObjectTypeDebris(objToDestroy, damagetype);
                }
                else
                {
                    Debris = 0xD5 + Rng.r.Next(0, 2);
                }
            }

            if (Debris >= 0)
            {
                Debug.Print("Turn into debris");
                objToDestroy.item_id = Debris;
                if (!objToDestroy.IsStatic)
                {
                    objToDestroy.npc_hp = 40;
                }
                objToDestroy.quality = 40;
                if (WorldObject)
                {
                    objectInstance.RedrawFull(objToDestroy);
                }
                else
                {
                    uimanager.UpdateInventoryDisplay();
                }
                //spawn this debris item id at the position of the old object
            }
            if (Debris >= -1)
            {
                return 0;
            }
            return 1;
        }



        /// <summary>
        /// Gets the type of debris to be left behind by an object.
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns>item id of debris to spawn.</returns>
        public static int GetObjectTypeDebris(uwObject objToDestroy, int damagetype)
        {//only used in uw2
            if ((objToDestroy.majorclass != 0) || (damagetype == 8))
            {
                if (objToDestroy.OneF0Class == 0x15)
                {//wood chips
                    if (_RES == GAME_UW2)
                    {
                        return 0xDC;
                    }
                    else
                    {
                        return 0xDB;
                    }

                }
                else
                {
                    return 0xD6;
                }
            }
            else
            {
                if ((objToDestroy.item_id == 3) && (_RES == GAME_UW2))
                {
                    return 0xC7;//broken dagger in UW2
                }
                else
                {
                    if (_RES == GAME_UW2)
                    {
                        return 0xC5 + weaponObjectDat.skill(objToDestroy.item_id);
                    }
                    else
                    {
                        return 0xC8 + weaponObjectDat.skill(objToDestroy.item_id);
                    }
                }
            }
        }


        public static int ScaleDamage(int item_id, ref int basedamage, int damagetype)
        {
            if (_RES == GAME_UW2)
            {
                return ScaleDamageUW2(item_id: item_id, basedamage: ref basedamage, damagetype: damagetype);
            }
            else
            {
                return ScaleDamageUW1(item_id: item_id, basedamage: ref basedamage, damagetype: damagetype);
            }
        }


        static int ScaleDamageUW1(int item_id, ref int basedamage, int damagetype)
        {

            var scales = commonObjDat.scaleresistances(item_id);

            if ((scales & damagetype) != 0)
            { //has some sort of resistance
                if ((damagetype & 3) != 0) //magic resistances
                {
                    //damage & 3 != 0
                    //seg025_26A1_4BA:
                    var r = Rng.r.Next(0, 3);
                    if (r >= (scales & 0x3))
                    {
                        //seg025_26A1_4D5
                        damagetype &= 0xFC; //mask out magic damage. possibly means magic can be used in combo with other damage type??
                    }
                    else
                    {
                        return 0;
                    }
                }

                //seg025_26A1_4DD
                if ((scales & damagetype) != 0)
                { //the damage type is resisted after possibly masking out magic damage.
                    return 0;
                }
            }
            return basedamage;
        }
        /// <summary>
        /// Scales damage up or down based on the NPCs damage resistances
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="basedamage"></param>
        /// <param name="damagetype"></param>
        /// <returns></returns>
        static int ScaleDamageUW2(int item_id, ref int basedamage, int damagetype)
        {
            var scales = commonObjDat.scaleresistances(item_id);

            if ((scales & damagetype) != 0)
            { //has some sort of resistance
                if ((damagetype & 3) != 0) //magic resistances
                {
                    //damage & 3 != 0
                    //seg025_26A1_4BA:
                    var r = Rng.r.Next(0, 3);
                    if (r >= (scales & 0x3))
                    {
                        //seg025_26A1_4D5
                        damagetype &= 0xFC; //mask out magic damage. possibly means magic can be used in combo with other damage type??
                    }
                    else
                    {
                        return 0;
                    }
                }

                //seg025_26A1_4DD
                if ((scales & damagetype) != 0)
                { //the damage type is resisted after possibly masking out magic damage.
                    return 0;
                }
            }

            //if at this point magic has possibly been masked out. but checks need to be made to see how fire/ice damage interact with fire/ice resistances
            //Ice is unique to UW2 so this does not apply to UW1.
            //seg 25:429
            if ((damagetype & 8) != 0) //test for fire damage
            {
                if ((scales & 0x20) != 0) //test for ice resistance
                {
                    return VulnerableDamage(ref basedamage); //fire does double damage to ice resistance creatures?
                }
            }

            //seg025_26A1_4F5
            if ((damagetype & 0x20) == 0) //test for ice damage
            {
                return basedamage;
            }
            else
            {//not ice damage
                if ((scales & 8) == 0)//test for no fire resistance
                {
                    return basedamage;
                }
                else
                {
                    //if has fire resistance
                    if ((scales & 0x28) == 0x28) //test for fire and ice together.
                    {
                        return basedamage; //when fire and ice together ice only does regular damage
                    }
                    else
                    {
                        return VulnerableDamage(ref basedamage); //if has no ice resistance, ice damage will do double damage
                    }
                }
            }

        }

        /// <summary>
        /// Applies damage increase when object is vulnerable to a damage type.
        /// </summary>
        /// <param name="basedamage"></param>
        /// <returns></returns>
        static int VulnerableDamage(ref int basedamage)
        {
            basedamage = Math.Min(127, basedamage << 1);
            return basedamage;
        }


        public static void DamageObjectsInTile(int tileX, int tileY, int source, int spellclass)
        {
            if (spellclass != 0)
            {
                var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];

                if (tile.indexObjectList != 0)
                {
                    var next = tile.indexObjectList;
                    int[] damagetypes = new int[] { 0xB, 3 };
                    int[] damagerange = new int[] { 6, 5 };
                    int[] damagerolls = new int[] { 0xA, 6 };
                    int index = (spellclass - 1) & 1;
                    while (next != 0)
                    {
                        var nextObj = UWTileMap.current_tilemap.LevelObjects[next];
                        next = nextObj.next;
                        var damagetoapply = Rng.DiceRoll(damagerolls[index], damagerange[index]);
                        DamageObject(
                            objToDamage: nextObj,
                            basedamage: damagetoapply,
                            damagetype: damagetypes[index],
                            objList: UWTileMap.current_tilemap.LevelObjects,
                            WorldObject: true,
                            hitCoordinate: Godot.Vector3.Zero,
                            damagesource: source);

                    }
                }
            }
        }


        /// <summary>
        /// Damages a piece of equipment in the inventory at the specified slot
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>1 if object was destroyed, 0 if damage, negative if no change</returns>
        public static int DamageEquipment(int slot, int damage, int damagetype, int arg6, int arg8)
        {
            uwObject obj = null;
           
            var si = 0;
            string distring="";
            if (slot <= 11)
            {
                obj = playerdat.GetInventorySlotObject(slot);
            }
            else
            {
                var index = uimanager.BackPackIndices[slot - 11];
                if (index != -1)
                {
                    obj = playerdat.InventoryObjects[index];
                }
            }

            if (obj != null)
            {
                var originalquality = obj.quality;
                switch (arg6)
                {
                    case 2:
                        break;
                    case 0:
                        if (obj.majorclass != 0)
                        {
                            return -2;

                        }
                        break;
                    default:
                        {
                            if (!uwObject.CheckIfObjectInValidSlot(obj, uimanager.CurrentSlot))
                            {
                                return -2;
                            }
                        }
                        break;
                }

                //apply damage
                var result = DamageObject(obj, damage, damagetype, playerdat.InventoryObjects, false, Godot.Vector3.Zero, 0, true);
                if (result == 1)
                {//taken damage and can be destroyed
                    if (arg8 != 0)
                    {
                        //Turn into debris? (could this have already happened inside DamageObject)
                        var debris = GetObjectTypeDebris(obj, 0);
                        var tile = UWTileMap.current_tilemap.Tiles[playerdat.tileX, playerdat.tileY];
                        ObjectCreator.spawnObjectInTile(
                            itemid: debris, 
                            tileX: playerdat.tileX, tileY: playerdat.tileY, 
                            xpos: 3, ypos: 3, zpos: (short)(tile.floorHeight<<3), 
                            WhichList: ObjectFreeLists.ObjectListType.StaticList);
                    }
                    ObjectCreator.Consume(obj, true);//removes from inventory     
                    playerdat.PlayerStatusUpdate();     
                    distring = " destroyed";  
                    si = 1;     
                }
                else
                {//taken damage and not destroyed
                    if (obj.quality!=originalquality)
                    {
                        distring = " damaged";
                        si = 0;
                    }
                    else
                    {
                        return -1;
                    }
                }
                uimanager.AddToMessageScroll($"Your {GameStrings.GetObjectNounUW(obj.item_id)} was{distring}");
                return si;
            }
            else
            {
                return -2;
            }
        }


    }//end class
}//end namespace