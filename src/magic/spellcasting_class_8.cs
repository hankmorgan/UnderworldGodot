using System.Diagnostics;
using Godot;

namespace Underworld
{
    public partial class SpellCasting : UWClass
    {
        public static void CastClass8_Summoning(int minorclass, int caster = 0)
        {
            //Preamble of getting positon to spawn in.  based on facing direction of the player
            var itemid = -1;
            var whichList = ObjectCreator.ObjectListType.StaticList;
            bool isNPCSpawn = false;
            var tile = UWTileMap.GetTileInDirectionFromCamera(1.2f);
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
                        itemid = Rng.r.Next(0,7) + 0xB0;                       
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
                            itemid = 393;
                        }
                        break;
                    }
                case 4:
                    {          //Summon Monster (UW1 only? )  
                        isNPCSpawn= true;            
                        var monsterlevel = 2;
                        if (caster == 0)
                        {
                            monsterlevel = System.Math.Max(monsterlevel, playerdat.Casting);
                        }
                        else
                        {
                            monsterlevel = System.Math.Max(monsterlevel, playerdat.dungeon_level << 2); //this is very high, carried over code from UW1 as summon monster not available in uw2
                        }
                        bool ValidMonster = false;
                        int retries = 0;//just in case probability runs against us
                        while ((!ValidMonster) && (retries<=16))
                        {
                            itemid = 0x40 + (Rng.r.Next(0, monsterlevel) & 0x3F);
                            whichList = ObjectCreator.ObjectListType.MobileList;
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
                            if (retries>=16)
                            {
                                itemid=-1;//cancel
                            }
                        }
                        break;
                    }
                case 5:
                    {//SUmmon demon. summons a hostile demon from a list of demons item ids
                        if (_RES == GAME_UW2)
                        {
                            isNPCSpawn= true; 
                            var demons = new int[]{0x4B, 0x4B, 0x5E, 0x64, 0x68};
                            var demonIndex = (Rng.r.Next(0,0x1E) + playerdat.Casting)/0xC;                            
                            itemid = demons[demonIndex];
                            whichList = ObjectCreator.ObjectListType.MobileList;
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
                            itemid =0x1E;// spawn a satellite. it does nothing
                            break;
                        }
                        else
                        {
                            return;
                        }
                    }
            }

            //Do the item creation.
            if (itemid != -1)
            {
                UWTileMap.GetRandomXYZForTile(tile, out int newxpos, out int newypos, out int newzpos);
                if ((whichList == ObjectCreator.ObjectListType.MobileList) && (isNPCSpawn))
                {
                    if (critterObjectDat.isFlier(itemid))
                    {
                        newzpos = (newzpos + 0x80) / 2;
                    }
                }
                var newObject = ObjectCreator.spawnObjectInTile(
                    itemid: itemid,
                    tileX: tile.tileX,
                    tileY: tile.tileY,
                    xpos: (short)newxpos, ypos: (short)newypos, zpos: (short)newzpos,
                    WhichList: whichList);

                switch (minorclass)
                {//post spawn handling.
                    case 4://summon monster
                    case 5://summon demon
                        newObject.npc_xhome = (short)tile.tileX;
                        newObject.npc_yhome = (short)tile.tileY;
                        if ((caster == 0) && (minorclass == 4))
                        {
                            newObject.UnkBit_0x19_6 = 1;
                        }
                        else
                        {
                            newObject.npc_attitude = 0;
                            newObject.UnkBit_0x19_0 = 1;
                            newObject.TargetTileX = (short)playerdat.tileX;
                            newObject.TargetTileY = (short)playerdat.tileY;

                        }
                        break;

                }
            }
        }
    }   //end class
}//end namespace