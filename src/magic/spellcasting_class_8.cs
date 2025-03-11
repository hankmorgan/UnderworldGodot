using System.Diagnostics;

namespace Underworld
{
    public partial class SpellCasting : UWClass
    {
        public static void CastClass8_Summoning(int minorclass, uwObject caster)
        {
            //Preamble of getting positon to spawn in.  based on facing direction of the player
            var distance = 9;
            if ((_RES != GAME_UW2) && (minorclass == 4))
            {//EXTRA DISTANCE FOR SUMMON MONSTER IN UW1
                distance = 0xC;
            }
            var itemid = -1;
            //var whichList = ObjectFreeLists.ObjectListType.StaticList;
            //bool isNPCSpawn = false;

            int x0 = (caster.npc_xhome << 3) + caster.xpos;
            int y0 = (caster.npc_yhome << 3) + caster.ypos;
            var rngrange = Rng.r.Next(26) - 13;
            var heading = (((caster.heading << 5) + caster.npc_heading + rngrange) & 0xFFFF) % 0xFF;

            motion.GetCoordinateInDirection(heading, distance, ref x0, ref y0);

            var tile = UWTileMap.current_tilemap.Tiles[x0 >> 3, y0 >> 3];
            var z0 = tile.floorHeight << 3;
            if (tile != null)
            {
                if (tile.tileType == UWTileMap.TILE_SOLID)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(GameStrings.str_there_is_no_room_to_create_that_));
                    return;
                }
            }
            switch (minorclass)
            {
                case 1:
                    {
                        // Debug.Print("Create Food");
                        itemid = Rng.r.Next(0, 7) + 0xB0;
                        break;
                    }
                case 2:
                    {
                        if (_RES == GAME_UW2)
                        {
                            //Debug.Print("FLAM RUNE");
                            itemid = 414;
                        }
                        break;
                    }
                case 3:
                    {
                        if (_RES == GAME_UW2)
                        {
                            //Debug.Print("TYM RUNE");
                            itemid = 415;
                        }
                        else
                        {
                            Debug.Print("Rune of warding"); //creates a move trigger linked to a trap (#9 ward)
                            CastWardTrap(tile);
                            return;
                        }
                        break;
                    }
                case 4:
                    {          //Summon Monster (UW1 only? )  
                        //isNPCSpawn = true;
                        var monsterlevel = 2;
                        if (caster == playerdat.playerObject)
                        {
                            monsterlevel = System.Math.Max(monsterlevel, playerdat.Casting);
                        }
                        else
                        {
                            monsterlevel = System.Math.Max(monsterlevel, playerdat.dungeon_level << 2); //this is very high, carried over code from UW1 as summon monster not available in uw2
                        }
                        bool ValidMonster = false;
                        int retries = 0;//just in case probability runs against us
                        while ((!ValidMonster) && (retries <= 16))
                        {
                            itemid = 0x40 + (Rng.r.Next(0, monsterlevel) & 0x3F);
                            //whichList = ObjectFreeLists.ObjectListType.MobileList;
                            if (
                                (critterObjectDat.avghit(itemid) == 0)
                                || critterObjectDat.isSwimmer(itemid)
                                || critterObjectDat.unkPassivenessProperty(itemid)
                                || (itemid == 0x7b)
                                || (itemid == 0x7c)
                                )
                            {//monster types not allowed
                                ValidMonster = false;
                            }
                            else
                            {
                                ValidMonster = true;
                            }
                            retries++;
                            if (retries >= 16)
                            {
                                itemid = -1;//cancel
                                return;
                            }
                        }
                        break;
                    }
                case 5:
                    {//Summon demon. summons a hostile demon from a list of demons item ids
                        if (_RES == GAME_UW2)
                        {
                            //isNPCSpawn = true;
                            var demons = new int[] { 0x4B, 0x4B, 0x5E, 0x64, 0x68 };
                            var demonIndex = (Rng.r.Next(0, 0x1E) + playerdat.Casting) / 0xC;
                            itemid = demons[demonIndex];
                            //whichList = ObjectFreeLists.ObjectListType.MobileList;
                            break;
                        }
                        else
                        {
                            return;
                        }
                    }
                case 6:
                    {
                        if (_RES == GAME_UW2)
                        {
                            //Debug.Print("Satellite");
                            itemid = 0x1E;// spawn a satellite.
                            break;
                        }
                        else
                        {
                            return;
                        }
                    }
            }

            if (motion.TestIfObjectFitsInTile(itemid, 0, x0, y0, tile.floorHeight << 3, 1, 8))
            {
                int newIndex;
                if (_RES == GAME_UW2)
                {
                    if (minorclass >= 4)
                    {
                        newIndex = ObjectCreator.PrepareNewObject(itemid, ObjectFreeLists.ObjectListType.MobileList);
                    }
                    else
                    {
                        newIndex = ObjectCreator.PrepareNewObject(itemid, ObjectFreeLists.ObjectListType.StaticList);
                    }
                }
                else
                {
                    if (minorclass == 4)
                    {
                        newIndex = ObjectCreator.PrepareNewObject(itemid, ObjectFreeLists.ObjectListType.MobileList);
                    }
                    else
                    {
                        newIndex = ObjectCreator.PrepareNewObject(itemid, ObjectFreeLists.ObjectListType.StaticList);
                    }
                }
                var newObject = UWTileMap.current_tilemap.LevelObjects[newIndex];
                newObject.tileX = x0 >> 3;
                newObject.tileY = y0 >> 3;
                newObject.xpos = (short)(x0 & 7);
                newObject.ypos = (short)(x0 & 7);
                if (newObject.IsStatic)
                {
                    newObject.quality = 0x3F;
                }
                switch (minorclass)
                {
                    case 4:
                    case 5://summon monster or summon demon
                        {
                            //vanilla behaviour calls initcritter values. This has already ran as part of PrepareNewObject so I skip.
                            newObject.npc_xhome = (short)(x0 >> 3);
                            newObject.npc_yhome = (short)(y0 >> 3);
                            if (critterObjectDat.isFlier(itemid))
                            {
                                z0 = (z0 + 0x80) / 2;
                            }
                            if ((caster == playerdat.playerObject) && (minorclass == 4))
                            {
                                newObject.IsAlly = 1;
                            }
                            else
                            {
                                newObject.npc_attitude = 0;
                                newObject.UnkBit_0x19_0_likelyincombat = 1;
                                newObject.TargetTileX = playerdat.playerObject.npc_xhome;
                                newObject.TargetTileY = playerdat.playerObject.npc_yhome;
                            }
                            break;
                        }
                    case 6:
                        {
                            if (_RES==GAME_UW2)
                            {
                                //satellite
                                ObjectCreator.InitMobileObject(newObject, x0 >> 3, y0 >> 3);

                                newObject.ProjectileHeading = (ushort)(0x40 + heading + Rng.r.Next(2) * 0x7F);
                                if ((caster.majorclass == 1) && (caster.IsStatic == false))
                                {
                                    newObject.ProjectileSourceID = caster.index;
                                }
                                else
                                {
                                    newObject.ProjectileSourceID = 0;
                                }

                                newObject.UnkBit_0X15_Bit7 = 0;
                                z0 += 0x12;
                                if (z0 > 0x78)
                                {
                                    z0++; //?
                                }
                                newObject.UnkBit_0X13_Bit0to6 = (short)(15 + Rng.r.Next(15));
                            }
                            break;
                        }
                    default:
                        {
                            newObject.quality = 0x3F;
                            break;
                        }
                }

                newObject.zpos = (short)z0;
                newObject.next = tile.indexObjectList;
                tile.indexObjectList = newObject.index;                
                if (minorclass != 4)
                {
                    newObject = motion.PlacedObjectCollision_seg030_2BB7_10BC(newObject, x0 >> 3, y0 >> 3, 1);
                }
                if (newObject!=null)
                {
                    ObjectCreator.RenderObject(newObject, UWTileMap.current_tilemap);
                }   
            }
            else
            {
                if (caster == playerdat.playerObject)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_there_is_no_room_to_create_that_));
                }
            }
        }


        /// <summary>
        /// Creates a move triggerlinked to a ward trap
        /// </summary>
        /// <param name="tile"></param>
        private static void CastWardTrap(TileInfo tile)
        {
            //create a move trigger
            var newMoveTrigger = ObjectCreator.spawnObjectInTile(
                itemid: 416,
                tileX: tile.tileX, tileY: tile.tileY,
                xpos: 3, ypos: 3, zpos: (short)(tile.floorHeight << 3),
                WhichList: ObjectFreeLists.ObjectListType.StaticList, RenderImmediately: false);
            if (newMoveTrigger != null)
            {
                newMoveTrigger.flags = 0;//with this value set the player will not activate this move trigger.
                newMoveTrigger.enchantment = 1;
                newMoveTrigger.doordir = 1;
                newMoveTrigger.is_quant = 1;
                newMoveTrigger.invis = 1;
                newMoveTrigger.quality = tile.tileX;
                newMoveTrigger.owner = tile.tileY;
                //create the ward trap
                var newWardTrap = ObjectCreator.spawnObjectInTile(
                    itemid: 393,
                    tileX: tile.tileX, tileY: tile.tileY,
                    xpos: 3, ypos: 3, zpos: (short)(tile.floorHeight << 3),
                    WhichList: ObjectFreeLists.ObjectListType.StaticList, RenderImmediately: false);
                if (newWardTrap != null)
                {
                    newMoveTrigger.link = newWardTrap.index;
                    newWardTrap.enchantment = 0;
                    newWardTrap.quality = 63;
                    newWardTrap.flags = 1;
                    newWardTrap.is_quant = 1;
                    newWardTrap.doordir = 1;
                    newWardTrap.invis = 1;
                    //newWardTrap.owner = 3; // possibly this is a random value based on what was in memory already.
                    ObjectCreator.RenderObject(newMoveTrigger, UWTileMap.current_tilemap);
                }
            }
        }

    }   //end class
}//end namespace