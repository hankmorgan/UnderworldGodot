using System;
using System.Diagnostics;

namespace Underworld
{
    public partial class SpellCasting : UWClass
    {
        //targeted spells
        public static void CastClass7_SpellsOnCallBack(int minorclass, int index, uwObject[] objList, Godot.Vector3 hitCoordinate, bool WorldObject = true, int caster = 1)
        {
            if (_RES == GAME_UW2)
            {
                switch (minorclass)
                {
                    case 0:
                        //cause bleeding  
                        CauseBleeding(index, objList, hitCoordinate);
                        break;
                    case 1:
                        //Causefear
                        Causefear(index, objList);
                        break;
                    case 2:
                        //SmiteUndead
                        SmiteUndead(index, objList, hitCoordinate);
                        break;
                    case 3:
                        //Charm
                        Charm(index, objList);
                        break;
                    case 4:
                        //smite foe
                        break;
                    case 5:
                        //paralyse  
                        Paralyse(index, objList, caster);
                        break;
                    case 6:
                        ///bleed (identical)
                        CauseBleeding(index, objList, hitCoordinate);
                        break;
                    case 7:
                        StudyMonster(index, objList);
                        break;
                    case 8:
                        //Dispel rune
                        break;
                    case 9:
                        //Mending
                        repair.MendingSpell(index, objList);
                        break;
                    case 10:
                        //disarm trap
                        break;
                    case 11:
                        //name enchantment
                        NameEnchantment(index, objList);
                        break;
                    case 12:
                        //unlock spell
                        Unlock(index, objList);
                        break;
                    case 13:
                        //detect trap
                        switch (trapdisarming.DetectTrapTrigger(index,objList,45))
                        {
                            case playerdat.SkillCheckResult.Success:
                            case playerdat.SkillCheckResult.CritSucess:
                                uimanager.AddToMessageScroll(GameStrings.GetString(1,0x130)); break;//you have detected
                            case playerdat.SkillCheckResult.Fail:
                            case playerdat.SkillCheckResult.CritFail:
                                uimanager.AddToMessageScroll(GameStrings.GetString(1,0x131)); break;//you hae not detected.
                        }
                        break;
                    case 14:
                        //enchantment spell
                        enchanting.EnchantObject(index, objList, WorldObject);
                        break;
                    case 15:
                        //gate travel
                        GateTravelUW2(
                            index: index,
                            objList: objList,
                            WorldObject: WorldObject);
                        break;
                }
            }
            else
            {//the uw1 list of class 7 spells is much shorter
                switch (minorclass)
                {
                    case 0:
                        //some sort of explosion?   
                        break;
                    case 1:
                        //Cause fear
                        Causefear(index, objList);
                        break;
                    case 2:
                        //smite undeead
                        break;
                    case 3:
                        //ally
                        break;
                    case 4:
                        //poison
                        break;
                    case 5:
                        //paralyse
                        Paralyse(index, objList, caster);
                        break;
                }
            }
        }

        /// <summary>
        /// UW2 only spell. Causes damage scaled by vulnerability
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        static void CauseBleeding(int index, uwObject[] objList, Godot.Vector3 hitCoordinate)
        {
            var obj = objList[index];
            if (obj != null)
            {
                if (obj.majorclass == 1)
                {
                    //npc

                    var bleed = critterObjectDat.bleed(obj.item_id);
                    if (bleed == 0)
                    {
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x12B));
                    }
                    else
                    {
                        var basedamage = 0xA + playerdat.Casting / 2;
                        damage.DamageObject(
                            objToDamage: obj,
                            basedamage: basedamage,
                            damagetype: 4, objList: objList,
                            WorldObject: true,
                            hitCoordinate: hitCoordinate,
                            damagesource: 1);
                        animo.SpawnAnimoAtPoint(0, hitCoordinate);
                        // damage.ScaledDamageOnNPCWithAnimo(
                        //     critter: obj,
                        //     basedamage: basedamage,
                        //     damagetype: 4,
                        //     animoclassindex: 0, 
                        //     hitCoordinate: obj.GetCoordinate(obj.tileX, obj.tileY)+ Godot.Vector3.Up);
                    }
                }
            }
        }


        /// <summary>
        /// Identifies the attributes and stats of a selected monster.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        static void StudyMonster(int index, uwObject[] objList)
        {
            var critter = objList[index];
            if (critter == null) { return; }
            bool isNPC = (critter.majorclass == 1);
            var critterdat = critter.item_id & 0x3f; //for looking up critter dat when there are non-npc critter types here
            int var6 = 0;
            if (isNPC)
            {
                if (critter.IsPowerfull == 1)
                {
                    var6 |= 1;
                }
                //test damage type 7 in scale damage.
                int test = 1;
                var scale = damage.ScaleDamage(critter.item_id, ref test, 128);
                if (scale == 0)
                {
                    var6 |= 2; //undead?
                }
            }
            else
            {
                if (critter.item_id == 0x13) // a_skull???
                {
                    var6 |= 2;//undead??
                }
                else
                {
                    if (critter.item_id != 0x1CD)
                    {//not a wisp
                        return;
                    }
                }
            }

            string npcrace = GameStrings.GetObjectNounUW(critter.item_id);
            var mood = GameStrings.GetString(5, 96 + critter.npc_attitude);
            switch (mood.ToUpper().Substring(0, 1))
            {
                case "A":
                case "E":
                case "I":
                case "O":
                case "U":
                    mood = "an " + mood; break;
                default:
                    mood = "a " + mood; break;
            }

            var output = $"{GameStrings.GetString(1, var6 + 0x135)}{mood} {npcrace}";//this creature/powerful/undead/undead powerful

            int hp = 0;
            if (isNPC)
            {
                if (critter.npc_whoami == 0x100)
                {
                    var unk = critter.UnkBit_0XA_Bit456; //(critter.MobileUnk_0xA & 0x70) >> 4;
                    if (unk == 0)
                    {
                        hp = 30;
                    }
                    else
                    {
                        hp = critter.npc_hp;
                    }
                }
                else
                {
                    hp = critter.npc_hp;
                }
            }
            else
            {
                hp = critter.quality;
            }
            output += $"\n{GameStrings.GetString(1, 0x139)}{hp}\n";
            uimanager.AddToMessageScroll(output);
            output = "";
            if (!isNPC)
            {
                output += $"{GameStrings.GetString(1, 0x13D)}";//it is resistant to 
                output += $"{GameStrings.GetString(0x346)},"; //magic
                output += $"{GameStrings.GetString(0x347)}"; //physical
                uimanager.AddToMessageScroll(output);
                output = "";
            }
            else
            {
                var6 = 0;
                if (critterObjectDat.poisondamage(critterdat) != 0)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x13C));//it can use poison attacks
                }
                if (npc.ListNPCProperties(critter, out string prop))
                {//list spells
                    uimanager.AddToMessageScroll($"{GameStrings.GetString(1, 0x144)}{prop}");// it can use
                }

                output = "";
                //do resistances
                var HasResistances = false;
                var damagetypes = new int[] { 3, 4, 8, 0x10, 0x20, 0x40 };
                var resistancesstring = "";
                for (int si = 0; si < 6; si++)
                {
                    var testdam = 1;
                    var scale = damage.ScaleDamage(critter.item_id, ref testdam, damagetypes[si]);
                    if (scale == 0)
                    {
                        if (
                            (damagetypes[si] == 8)
                            &&
                            (critterObjectDat.generaltype(critter.item_id) == 0x17)
                        )
                        {
                            //skip. this class of npc, skip displaying this resistance
                        }
                        else
                        {
                            if (HasResistances)
                            {
                                resistancesstring += ",";
                            }
                            resistancesstring += GameStrings.GetString(1, 0x146 + si);
                            HasResistances = true;
                        }
                    }
                }
                if (critterObjectDat.generaltype(critter.item_id) == 0x17)
                {
                    if (HasResistances)
                    {
                        resistancesstring += ", ";
                    }
                    resistancesstring += GameStrings.GetString(0xD25);//rune of statis?
                }
                if (HasResistances)
                {
                    uimanager.AddToMessageScroll($"{GameStrings.GetString(1, 0x13D)}{resistancesstring}");
                }
            }
            uimanager.AddToMessageScroll(output);
        }


        /// <summary>
        /// Changes the AI attitudes and goals if npc is not resistant to raw/magic damage
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="newgoal"></param>
        /// <param name="newattitude"></param>
        /// <param name="newgtarg"></param>
        static void ApplyAIChangingSpell(uwObject critter, byte newgoal = 0xFF, byte newattitude = 0xFF, byte newgtarg = 0xFF)
        {
            int test = 1;
            if (damage.ScaleDamage(critter.item_id, ref test, 3) != 0)
            {
                animo.SpawnAnimoAtPoint(7, critter.GetCoordinate(critter.tileX, critter.tileY) + Godot.Vector3.Up);
                if (newgoal != 0xFF)
                {
                    critter.npc_goal = newgoal;
                }

                if (newattitude != 0xFF)
                {
                    critter.npc_attitude = newattitude;
                }

                if (newgtarg != 0xFF)
                {
                    critter.npc_gtarg = newgtarg;
                }
            }
            else
            {
                Debug.Print("NPC has resisted spell");
            }
        }

        /// <summary>
        /// Makes an NPC afraid of the player
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        static void Causefear(int index, uwObject[] objList)
        {
            var critter = objList[index];
            if (critter != null)
            {
                if (critter.majorclass == 1)
                {//npc class
                    critter.npc_attitude = 1;

                    ApplyAIChangingSpell(
                        critter: critter,
                        newgoal: (byte)npc.npc_goals.npc_goal_fear_6,
                        newgtarg: 1);

                }
            }
        }

        /// <summary>
        /// Smites undead and damages liches by half their hp
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        /// <param name="caster"></param>
        /// <returns></returns>
        public static bool SmiteUndead(int index, uwObject[] objList, Godot.Vector3 hitCoordinate, int caster = 1)
        {
            var obj = objList[index];

            if (obj.item_id != 0x13)
            {
                if (obj.majorclass == 1)
                {
                    var damageToApply = 0xFF;
                    if (critterObjectDat.race(obj.item_id) == 0x17)
                    {//a liche
                        damageToApply = obj.npc_hp / 2;
                    }
                    var test = 1;
                    damage.ScaleDamage(obj.item_id, ref test, 0x80);
                    if (test > 0)
                    {
                        damage.DamageObject(
                            objToDamage: obj,
                            basedamage: damageToApply,
                            damagetype: 3,
                            objList: objList,
                            WorldObject: true,
                            hitCoordinate: hitCoordinate,
                            damagesource: 1);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //a skull?
                return SmiteUndeadObject(obj);
            }
        }

        public static bool SmiteUndeadObject(uwObject obj)
        {
            Debug.Print("Smite undead object. unimplemented!");
            return true;
        }

        /// <summary>
        /// Paralyses or applies stone strike to an NNPC
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        /// <param name="caster"></param>
        public static void Paralyse(int index, uwObject[] objList, int caster = 1)
        {
            var critter = objList[index];
            if (critter != null)
            {
                if (_RES == GAME_UW2)
                {
                    int castscore;
                    if (caster == 1)//player has cast
                    {
                        castscore = playerdat.Casting / 3;
                    }
                    else
                    {
                        castscore = 8;
                    }

                    var duration = 0x10 + Rng.r.Next(0, 16) * castscore;

                    if (critter.majorclass == 1)
                    {
                        int test = 1;
                        if (damage.ScaleDamage(critter.item_id, ref test, 0x80) != 0)
                        {//check for undead, immune
                            ApplyAIChangingSpell(
                                critter: critter,
                                newgoal: (byte)npc.npc_goals.npc_goal_petrified,
                                newattitude: 1,
                                newgtarg: (byte)duration);
                        }
                    }
                }
                else
                {   //UW1 is much simpler, does no skill checking and sets a different goal than UW2.
                    if (critter.majorclass == 1)
                    {
                        ApplyAIChangingSpell(
                            critter: critter,
                            newgoal: (byte)npc.npc_goals.npc_goal_stand_still_7,
                            newattitude: 1);
                    }
                }
            }
        }

        /// <summary>
        /// Makes an NPC friendly. If the NPC is a valid target for charming.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        public static void Charm(int index, uwObject[] objList)
        {//UW2 spell.
            var critter = objList[index];
            if (critter.majorclass == 1)
            {
                var test = 1;
                if (damage.ScaleDamage(critter.item_id, ref test, 3) != 0)
                {
                    animo.SpawnAnimoAtPoint(7, critter.GetCoordinate(critter.tileX, critter.tileY) + Godot.Vector3.Up);
                    var whoami = critter.npc_whoami;
                    int stringoffset = 0;
                    if (whoami >= 0x8C)
                    {
                        whoami -= 0x8C;
                        stringoffset++;
                    }
                    var maskstring = GameStrings.GetString(1, 0x15E + stringoffset); //get a long string that flags what whoami npcs can be charmed
                    var flagchar = maskstring.ToCharArray()[whoami];
                    if (flagchar == '+')
                    {
                        critter.npc_gtarg = 0;
                        critter.npc_goal = (byte)npc.npc_goals.npc_goal_charmed_8;
                        critter.npc_attitude = 3;
                        if (playerdat.IsFightingInPit)
                        {
                            pitsofcarnage.RemovePitFighter(critter.index);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Identifies an object
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        public static void NameEnchantment(int index, uwObject[] objList)
        {
            var obj = objList[index];
            if (obj != null)
            {
                if (
                    (obj.majorclass != 5)
                    &&
                    (obj.majorclass != 6)
                    &&
                    (commonObjDat.rendertype(obj.item_id) != 2)
                    )
                {//can be identifed
                    obj.heading = 7;
                    look.PrintLookDescription(obj, objList, 3);
                }
            }
        }

        /// <summary>
        /// Magically unlocks a door
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        /// <param name="character"></param>
        public static void Unlock(int index, uwObject[] objList, int character = 0)
        {
            var target = objList[index];
            if (a_lock.GetIsLocked(target))
            {
                a_lock.SetIsLocked(
                    parentObject: target,
                    value: false,
                    character: character);
                if (character == 0)
                {
                    //this has been unlocked
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_the_spell_unlocks_the_lock_));
                }
            }
            else
            {
                if (character == 0)
                {
                    //this has no lock.
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_that_is_not_locked_));
                }
            }
            // if ((target.majorclass==5) && (target.minorclass==0))
            // {
            //     var doorobj = (door)target.instance;
            //     if (doorobj!=null)
            //     {
            //         doorobj.Locked = false;
            //     }
            // }
        }



        /// <summary>
        /// If the player casts on a moonstone this will teleport the player to that location.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        /// <param name="WorldObject"></param>
        private static void GateTravelUW2(int index, uwObject[] objList, bool WorldObject)
        {
            for (int i = 0; i < 2; i++)
            {
                Debug.Print($"Moonstone at {playerdat.GetMoonstone(i)}");
            }

            var MoonStoneActivated = objList[index];
            if (MoonStoneActivated.item_id != 0x126)
            {//not a moonstone
                uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x143));// that is not a moonstone.                
            }
            else
            {
                var di = 0;
                var var9 = 1;
                var si = 0;
                while (si < 2)
                {
                    if (playerdat.GetMoonstone(si) != playerdat.dungeon_level)
                    {
                        if (playerdat.GetMoonstone(si) != 0)
                        {
                            var9 = 0;
                            break;
                        }
                        else
                        {
                            di++;
                        }
                    }
                    si++;
                }

                if ((si >= 2) && (var9 == 0))
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x121));// the moonstone is not available.
                    return;
                }
                else
                {
                    if (di == 2)
                    {
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x121));// the moonstone is not available.
                        return;
                    }
                    else
                    {
                        if (var9 != 0)
                        {//Moonstone on this level                            
                            if (WorldObject)
                            {//Used a moonstone from the ground.
                                int targetX = MoonStoneActivated.tileX; int targetY = MoonStoneActivated.tileY;
                                //check if this moonstone is the not as the one that has been used.
                                for (int x = 0; x < 64; x++)
                                {
                                    for (int y = 0; y < 64; y++)
                                    {
                                        if ((x != targetX) || (y != targetY))
                                        {
                                            if (UWTileMap.current_tilemap.Tiles[x, y].indexObjectList != 0)
                                            {
                                                var foundMoonstone = objectsearch.FindMatchInTile(x, y, 4, 2, 6);
                                                if (foundMoonstone != null)
                                                {
                                                    //Found a different moonstone. go to it.
                                                    Teleportation.Teleport(character: 0, tileX: x, tileY: y, newLevel: 0, heading: 0);
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                                //No other moonstone has been found. Likely the two moonstones are on the same tile.
                                Teleportation.Teleport(character: 0, tileX: targetX, tileY: targetY, newLevel: 0, heading: 0);
                            }
                            else
                            {
                                //Clicked on inventory to activate. The only moonstone on the map is a different one so use the callback.
                                Teleportation.JumpToMoonStoneOnLevel();
                            }
                        }
                        else
                        {
                            //moonstone on another map.
                            Teleportation.CodeToRunOnTeleport = Teleportation.JumpToMoonStoneOnLevel;
                            Teleportation.Teleport(
                                character: 0,
                                tileX: 32, tileY: 32,
                                newLevel: playerdat.GetMoonstone(si),
                                heading: 0);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Checks for a trap on an object.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        static void DetectTrap(int index, uwObject[] objList)
        {

        }
    }//end class
}//end namespace