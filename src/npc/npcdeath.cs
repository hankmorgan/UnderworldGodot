using System;
using System.Diagnostics;
using System.Linq;
namespace Underworld
{
    /// <summary>
    /// Class for managing the ai death special cases
    /// </summary>
    public partial class npc : objectInstance
    {

        /// <summary>
        /// Handles special death cases for npcs.
        /// </summary>
        /// <param name="critter"></param>
        /// <param name="mode">0 when initial killing blow, 1 when at end of death animation</param>
        /// <returns>true if NPC should die, otherwise false to stay alive</returns>
        public static bool SpecialDeathCases(uwObject critter, int mode = 0)
        {
            switch (_RES)
            {
                case GAME_UW2:
                    return SpecialDeathCasesUW2(critter, mode);
                default:
                    return SpecialDeathCasesUW1(critter, mode);
            }
        }

        public static bool SpecialDeathCasesUW1(uwObject critter, int mode = 0)
        {
            switch (critter.npc_whoami)
            {
                case 0xB://Thorlson (cut content npc)
                    {
                        if (mode == 0)
                        {
                            talk.Talk(critter.index, UWTileMap.current_tilemap.LevelObjects, true);
                            critter.npc_hp = 0x3C;//thorlson is unstoppable, once enraged nothing can kill him!
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                case 0x16://golem
                    {
                        if (mode == 0)
                        {
                            if (critter.UnkBit_0XA_Bit456 == 0)
                            {
                                critter.UnkBit_0XA_Bit456 = 1;
                                playerdat.ChangeExperience(500);
                            }
                            combat.EndCombatLoop();
                            critter.ProjectileSourceID = 0; //forget that the player has hit them.
                            critter.npc_animation = 32;//?
                            critter.AnimationFrame = 0;
                            talk.Talk(critter.index, UWTileMap.current_tilemap.LevelObjects, true);
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                case 0x18://prisoner (murgo)
                    {
                        if (mode == 1)
                        {
                            playerdat.SetQuest(6, 1);
                        }
                        return true;
                    }
                case 0x1B://garamon
                    {
                        if (mode == 1)
                        {
                            var tmp = playerdat.GetQuest(37);
                            tmp &= 0xFB;
                            playerdat.SetQuest(37, tmp);//can talismans be thrown == false
                        }
                        return true;
                    }
                case 0x6E://gazer
                    {
                        if (mode == 1)
                        {
                            playerdat.SetQuest(4, 1);
                        }
                        return true;
                    }

                case 0x8E://Rodrick
                    {
                        if (mode == 1)
                        {
                            playerdat.SetQuest(11, 1);
                        }
                        return true;
                    }
                case 0xE7://tybal
                    {
                        if (mode == 1)
                        {
                            TybalDeath(critter);
                        }
                        return true;
                    }


                case 0://generic npcs
                default:
                    return true;//to die
            }
        }



        public static bool SpecialDeathCasesUW2(uwObject critter, int mode = 0)
        {
            bool WasPitFighter = false;
            if (mode != 0)
            {//handle some initial events
                if (CheckIfMatchingRaceUW2(critter, 0xB))//a_trilkhun&trilkhai
                {
                    //set attitude to 0 on all members of race 0xB
                    SetRaceAttitude(0, 0xB);
                }
                if ((playerdat.dungeon_level == 4) && (critter.item_id == 0x4E))
                {//killing worms in the sewers
                    playerdat.SetQuest(135, Math.Min(playerdat.GetQuest(135) + 1, 200));
                }
                if (playerdat.IsFightingInPit)
                {
                    if (playerdat.IsDuelingAgainstCritter(critter.index))
                    {
                        if (playerdat.RemovePitFighter(critter.index))
                        {
                            WasPitFighter = true;
                            playerdat.SetQuest(129, playerdat.GetQuest(129) + 1); //if enough duels are fought the pit counter will overflow!
                            if (critter.npc_whoami != 0x64)
                            {//no honour in defeating krillner..., note krillners special case will decrease quest 129 by 1.
                                playerdat.SetQuest(24, 1);
                            }
                        }
                    }
                }
            }




            if
                (
                    (mode == 0) && (critter.UnkBit_0XA_Bit7 == 0)
                    &&
                    (
                        (critter.npc_whoami >= 81) && (critter.npc_whoami <= 0x8F)
                        ||
                        (critter.npc_whoami == 0xA8)
                        ||
                        (critter.npc_whoami == 0x95)
                    )
                )
            {
                //friendly britannian npcs
                if ((critter.npc_whoami == 0x8D) && (playerdat.GetXClock(1) >= 8))//lady tori
                {
                    return true; //allow her to die only after the xclock reaches 8
                }
                //revive
                var newhp = (critterObjectDat.avghit(critter.item_id) / 3) - 1;
                critter.npc_hp = (byte)newhp;
                trigger.RunScheduledTriggerInTile_15_29(critter.npc_xhome, critter.npc_yhome);
                return false;//do not kill
            }
            else
            {               
                //the others
                switch (critter.npc_whoami)
                {
                    case 0x4b: //guard, transforms into hordling
                        {
                            if (mode == 0)
                            {
                                if (critter.item_id != 0x5E)//check if turned yet
                                {
                                    critter.item_id = 0x5E;
                                    critter.npc_hp = 92;
                                    critter.npc_goal = 5;
                                    return false;//do not kill at this point
                                }
                            }
                            return true;
                        }
                    case 0x8B://nelson
                        {
                            if (mode != 0)
                            {
                                if (playerdat.GetXClock(1) >= 0xB)
                                {
                                    if (
                                        (critter.npc_xhome >= 0x14)
                                        &&
                                        (critter.npc_yhome >= 0x25)
                                        &&
                                        (critter.npc_xhome <= 0x16)
                                        &&
                                        (critter.npc_yhome <= 0x27)
                                    )
                                    {
                                        trigger.RunScheduledTriggerInTile_15_29(0x12, 0x28); //spawns guards??
                                    }
                                }
                                return true;
                            }
                            break;
                        }
                    case 0x64://krillner
                        {
                            if (WasPitFighter)
                            {
                                playerdat.SetQuest(129, playerdat.GetQuest(129) - 1);
                            }
                            if (playerdat.GetQuest(28) == 0) //Is krillner your slave
                            {
                                //talk to krillner
                                if (mode == 0)
                                {
                                    if (critter.UnkBit_0XA_Bit456 == 0)
                                    {
                                        critter.UnkBit_0XA_Bit456 = 1;
                                        playerdat.ChangeExperience(0x32);
                                        //Talk to NPC
                                        TalkToDyingNPC(critter);
                                        return false;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                }
                            }
                            else
                            {
                                return true;
                            }
                            break;
                        }
                    case 0x80://fissif
                        {
                            if (playerdat.GetQuest(119) == 0)
                            {//talk to fissif
                                if (mode == 0)
                                {
                                    if (critter.UnkBit_0XA_Bit456 == 0)
                                    {
                                        critter.UnkBit_0XA_Bit456 = 1;
                                        playerdat.ChangeExperience(0x32);
                                        //Talk to NPC
                                        TalkToDyingNPC(critter);
                                        return false;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                }
                            }
                            else
                            {
                                return true;
                            }
                            break;
                        }
                    case 0x91://the listener
                    {
                        if (mode==0)
                        {
                            if (combat.JeweledDagger)
                            {
                                if (combat.currentweapon!=null)
                                {
                                    combat.currentweapon.item_id=199;//broken dagger
                                    uimanager.UpdateInventoryDisplay();
                                    playerdat.SetQuest(11,1);
                                    playerdat.IncrementXClock(1);
                                }
                            }
                            else
                            {
                                critter.npc_hp=1;
                                critter.npc_goal=5;
                                return false;//do not kill
                            }
                        }
                        break;                        
                    }
                }

                //update winloss record.
                if ( playerdat.GetQuest(129) < playerdat.GetXClock(14))
                {
                    playerdat.SetQuest(129, playerdat.GetXClock(14));
                }               
                return true;
            }
        }



        /// <summary>
        /// Spawns the blood fluids and corpse object for this crittter type
        /// </summary>
        /// <param name="critter"></param>
        private static void DropRemains(uwObject critter)
        {
            var tile = UWTileMap.current_tilemap.Tiles[critter.tileX, critter.tileY];
            //Drop npc loot (spawn if missing)
            if (critter.LootSpawnedFlag == 0)
            {
                spawnloot(critter);
            }
            container.SpillWorldContainer(critter);

            var fluids = critterObjectDat.fluids(critter.item_id);
            if (fluids != 0)
            {
                if (_RES == GAME_UW2)
                {
                    fluids += 0xD9;
                }
                else
                {
                    fluids += 0xD8;
                }
                //UWTileMap.GetRandomXYZForTile(tile, out int newxpos, out int newypos, out int newzpos);
                ObjectCreator.spawnObjectInTile(
                    itemid: fluids,
                    tileX: critter.tileX,
                    tileY: critter.tileY,
                    xpos: (short)critter.xpos,
                    ypos: (short)critter.zpos,
                    zpos: (short)(tile.floorHeight << 3),
                    WhichList: ObjectCreator.ObjectListType.StaticList);
            }

            //Drop corpse
            var corpse = critterObjectDat.corpse(critter.item_id);
            if ((_RES == GAME_UW2) && (worlds.GetWorldNo(playerdat.dungeon_level) == 7))
            {
                corpse = 0;//no spawn in the pits of carnage
            }
            if (corpse != 0)
            {
                if (Rng.r.Next(0, 16) < 7)
                {
                    corpse += 0xC0;
                    //UWTileMap.GetRandomXYZForTile(tile, out int newxpos, out int newypos, out int newzpos);
                    ObjectCreator.spawnObjectInTile(
                        itemid: corpse,
                        tileX: critter.tileX,
                        tileY: critter.tileY,
                        xpos: (short)critter.xpos,
                        ypos: (short)critter.zpos,
                        zpos: (short)(tile.floorHeight << 3),
                        WhichList: ObjectCreator.ObjectListType.StaticList);
                }
            }
        }

        /// <summary>
        /// Handles tybals death.
        /// </summary>
        /// <param name="tybal"></param>
        public static void TybalDeath(uwObject tybal)
        {
            Debug.Print("Play cutscene 2");
            //set quest variables
            var tmp = playerdat.GetQuest(37);
            tmp |= 4;
            playerdat.SetQuest(37, tmp);
            //remove a move trigger from the prison
            var tile = UWTileMap.current_tilemap.Tiles[0x17, 0x38];//trap in the nw prison area
            var next = tile.indexObjectList;
            var previous = 0;
            while (next != 0)
            {
                var nextObj = UWTileMap.current_tilemap.LevelObjects[next];
                if (nextObj.item_id == 0x1A0)
                {
                    next = nextObj.next;
                    if (previous == 0)
                    {//head of tile
                        tile.indexObjectList = nextObj.next;
                        nextObj.next = 0;
                    }
                    else
                    {
                        var previousObject = UWTileMap.current_tilemap.LevelObjects[previous];
                        previousObject.next = nextObj.next;
                        nextObj.next = 0;
                    }
                    ObjectCreator.RemoveObject(nextObj);
                }
                else
                {//not a match. try next
                    previous = nextObj.index;
                    next = nextObj.next;
                }
            }

            int[] WhotoRemove = new int[] { 0xDE, 0xD1, 0xD8, 0xD2, 0xDC, 0xD5, 0xD8, 0xD4, 0xD3, 0xDD };

            for (int i = 1; i <= 255; i++)
            {
                var n = UWTileMap.current_tilemap.LevelObjects[i];
                if (n.majorclass == 1)
                {
                    if (UWTileMap.ValidTile(n.tileX, n.tileY))
                    {
                        if (WhotoRemove.Contains(n.npc_whoami))
                        {
                            ObjectCreator.DeleteObjectFromTile(n.tileX, n.tileY, n.index, true);
                        }
                    }
                }
            }
        }

        public static void TalkToDyingNPC(uwObject critter)
        {
            combat.EndCombatLoop();
            critter.ProjectileSourceID=0;
            critter.npc_animation=0;
            critter.AnimationFrame=0;
            critter.npc_attitude=2;
            critter.npc_goal=8;
            playerdat.FreezeTimeEnchantment=false;//?
            talk.Talk(critter.index, UWTileMap.current_tilemap.LevelObjects, true);
            //todo: in uw2 npc_talkedto gets cleared here. does this matter and if so how would implement it seeing as the conversation runs in a co-routine
        }
    }//end class
}//end namespace