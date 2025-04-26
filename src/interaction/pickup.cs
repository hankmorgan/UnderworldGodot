using Godot;
using Peaky.Coroutines;
using System.Collections;
using System.Diagnostics;

namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the pickup verb
    /// </summary>
    public class pickup : UWClass
    {

        public static bool DropOrThrowByPlayer(uwObject srcObject, bool printMessage)
        {
            uwObject ThrownObject = null;
            motion.projectileXHome = playerdat.playerObject.npc_xhome;
            motion.projectileYHome = playerdat.playerObject.npc_yhome;

            if (motion.InitPlayerProjectileValues())
            {
                //throw
                motion.MissileLauncherHeadingBase = 1;
                motion.RangedAmmoItemID = srcObject.item_id;
                motion.RangedAmmoType = 0xF;
                ThrownObject = motion.PrepareProjectileObject(playerdat.playerObject);
                if (ThrownObject != null)
                {
                    //copy props to new object
                    ThrownObject.is_quant = srcObject.is_quant;
                    ThrownObject.link = srcObject.link;
                    ThrownObject.flags_full = srcObject.flags_full;
                    ThrownObject.quality = srcObject.quality;
                    ThrownObject.owner = srcObject.owner;
                    ThrownObject.doordir = srcObject.doordir;
                    if (ThrownObject.majorclass != 5)
                    {
                        if (commonObjDat.rendertype(srcObject.item_id) != 2)
                        {
                            ThrownObject.npc_whoami = srcObject.heading;//preserve identification status.
                        }
                    }
                    objectInstance.RedrawFull(ThrownObject);
                    //seg027_2856_599:
                    ObjectFreeLists.ReleaseFreeObject(srcObject);
                    srcObject = null;
                }
            }

            //seg027_2856_5B1:
            if (srcObject != null)
            {//tileY = MotionCalcArray.y2 >> 3;
                var CannotFit = false;
                int x_ToSpawnIn = (motion.projectileXHome << 3) + playerdat.playerObject.xpos;
                int y_toSpawnIn = (motion.projectileYHome << 3) + playerdat.playerObject.ypos;
                var distance = commonObjDat.radius(playerdat.playerObject.item_id) + commonObjDat.radius(srcObject.item_id) + 1;
                srcObject.zpos =playerdat.playerObject.zpos;
                motion.GetCoordinateInDirection(
                    heading: playerdat.playerObject.npc_heading + (playerdat.playerObject.heading << 5),
                    distance: distance,
                    X0: ref x_ToSpawnIn,
                    Y0: ref y_toSpawnIn);

                CannotFit = !motion.TestIfObjectFitsInTile(srcObject.item_id, 0, x_ToSpawnIn, y_toSpawnIn, playerdat.playerObject.zpos, 1, distance);
                if (CannotFit == false)
                {
                    //object cannot fit at first tried position. Try and do it 3 units from player.
                    motion.GetCoordinateInDirection(
                        heading: playerdat.playerObject.npc_heading + (playerdat.playerObject.heading << 5),
                        distance: 3,
                        X0: ref x_ToSpawnIn,
                        Y0: ref y_toSpawnIn);
                    CannotFit = !motion.TestIfObjectFitsInTile(srcObject.item_id, 0, x_ToSpawnIn, y_toSpawnIn, playerdat.playerObject.zpos, 1, distance);
                }

                var tileX = x_ToSpawnIn >> 3; var tileY = y_toSpawnIn >> 3;
                var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                if (CannotFit == false)
                {
                    srcObject.xpos = (short)(x_ToSpawnIn & 7);
                    srcObject.ypos = (short)(y_toSpawnIn & 7);
                    //add to tile
                    srcObject.next = tile.indexObjectList;
                    tile.indexObjectList = srcObject.index;
                    srcObject.tileX = tileX; srcObject.tileY = tileY;
                    if (srcObject.OneF0Class == 9)
                    {
                        if (srcObject.classindex >= 4 && srcObject.classindex <= 6)
                        {
                            srcObject.item_id -= 4; //turn off lit lights.
                        }
                    }

                    objectInstance.RedrawFull(srcObject);

                    var ObjectAfterCollison = motion.PlacedObjectCollision_seg030_2BB7_10BC(
                        projectile: srcObject, 
                        tileX: tileX, tileY: tileY, 
                        arg8: 1);
                    if (ObjectAfterCollison != null)
                    {
                        objectInstance.RedrawFull(ObjectAfterCollison);
                        if (ObjectAfterCollison.IsStatic)
                        {
                            Debug.Print("dropped object. Do check for pressure triggers here");
                        }
                    }
                    srcObject = null;
                }
                else
                {
                    if (printMessage)
                    {
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_there_is_no_space_to_drop_that_));
                    }
                    //Play sound effect?
                }
            }

            //seg027_2856_845:
            return (srcObject == null);
        }


        public static bool Drop_old(int index, uwObject[] objList, Vector3 dropPosition, int tileX, int tileY, bool DoSpecialCases = true)
        {
            Debug.Print("To Remove Drop_old()");
            var t = UWTileMap.current_tilemap.Tiles[tileX, tileY];
            if (t.tileType == UWTileMap.TILE_SOLID)
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_there_is_no_place_to_put_that_));
                return false;
            }
            else
            {
                //translate drop position into xpos, ypos and zpos on the tile.
                //for the moment just use 4,4 and floorheight.
                var obj = objList[index];
                obj.xpos = uwObject.FloatXYToXYPos(-dropPosition.X);
                obj.ypos = uwObject.FloatXYToXYPos(dropPosition.Z);
                obj.zpos = uwObject.FloatZToZPos(dropPosition.Y); //(short)(t.floorHeight<<2);

                obj.tileX = tileX; obj.tileY = tileY;
                obj.next = t.indexObjectList;
                t.indexObjectList = (short)index;

                //create the new   
                ObjectCreator.RenderObject(obj, UWTileMap.current_tilemap);

                if (DoSpecialCases)
                {
                    //Handle some special cases
                    DropSpecialCases(obj.item_id);
                }
                return true;
            }
        }

        public static void DropSpecialCases(int item_id)
        {
            if (_RES != GAME_UW2)
            {
                switch (item_id)
                {
                    case 294://moonstone
                        playerdat.SetMoonstone(0, playerdat.dungeon_level);
                        break;
                }
            }
            else
            {
                switch (item_id)
                {
                    case 294://moonstone
                        {
                            for (int m = 0; m < 2; m++)
                            {//Store the current level in the first free moonstone variable.
                                if (playerdat.GetMoonstone(m) == 0)
                                {
                                    playerdat.SetMoonstone(m, playerdat.dungeon_level);
                                    break;
                                }
                            }
                            break;
                        }
                }
            }
        }


        static bool CanBePickedUpOverrides(int item_id)
        {
            if (_RES != GAME_UW2)
            {
                if (item_id == 458)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool PickUp(int index, uwObject[] objList, bool WorldObject = true)
        {
            var objPicked = objList[index];
            if (useon.CurrentItemBeingUsed != null)
            {
                return useon.UseOn(objPicked, useon.CurrentItemBeingUsed, WorldObject);
            }
            else
            {
                if (!commonObjDat.CanBePickedUp(objPicked.item_id) && !CanBePickedUpOverrides(objPicked.item_id))
                {//object cannot be picked up
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, GameStrings.str_you_cannot_pick_that_up_));
                    return false;
                }
                if (objPicked.ObjectQuantity > 1)
                {
                    //prompt for quantity in coroutine.
                    _ = Coroutine.Run(
                            DoPickupQty(index, objList, objPicked),
                            main.instance
                        );
                    return true;
                }
                else
                {
                    //single instance object
                    DoPickup(index, objList, objPicked);
                }
            }
            return true;
        }

        /// <summary>
        /// Handles pickup up stacks of objects which need to be split
        /// </summary>
        /// <param name="index"></param>
        /// <param name="objList"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerator DoPickupQty(int index, uwObject[] objList, uwObject obj)
        {
            MessageDisplay.WaitingForTypedInput = true;

            uimanager.instance.TypedInput.Text = obj.ObjectQuantity.ToString();
            uimanager.instance.scroll.Clear();
            uimanager.AddToMessageScroll("Move how many? {TYPEDINPUT}|");

            while (MessageDisplay.WaitingForTypedInput)
            {
                yield return new WaitOneFrame();
            }

            var response = uimanager.instance.TypedInput.Text;
            if (int.TryParse(response, out int result))
            {
                if (result > 0)
                {
                    if (obj.ObjectQuantity <= result)
                    {//at least all of the stack is seleced
                        DoPickup(index, objList, obj);
                    }
                    else
                    {
                        //if <quantity selected, split objects, pickup object of that quantity.
                        var newObjIndex = ObjectCreator.SpawnObjectInHand(obj.item_id); //spawning in hand is very handy here
                        var newObj = UWTileMap.current_tilemap.LevelObjects[newObjIndex];
                        newObj.link = (short)result;
                        newObj.quality = obj.quality;
                        newObj.owner = obj.owner;
                        //TODO. see if other object properties need copying.                    
                        obj.link = (short)(obj.link - result);//reduce the other object.
                    }
                    yield return true;
                }
                else
                {
                    //<0
                    yield return false;
                }
            }
            else
            {
                //invalid input. cancel                
                yield return false;
            }
            yield return false;
        }

        private static void DoPickup(int index, uwObject[] objList, uwObject obj)
        {

            //first handle some special cases
            PickupSpecialCases(obj);

            //check for pickup triggers linked to this object
            trigger.TriggerObjectLink(
                    character: 1,

                    ObjectUsed: obj,

                    triggerType: (int)triggerObjectDat.triggertypes.PICKUP,

                    triggerX: obj.tileX,

                    triggerY: obj.tileY,

                    objList: UWTileMap.current_tilemap.LevelObjects);

            if (obj.owner != 0)
            {
                Debug.Print($"Object Owner is {obj.owner}");
                thief.FlagTheftToObjectOwner(obj, 0);
            }

            //player is trying to pick something up
            playerdat.ObjectInHand = index;
            uimanager.instance.mousecursor.SetCursorToObject(obj.item_id);

            //remove from it's tile
            var tile = UWTileMap.current_tilemap.Tiles[obj.tileX, obj.tileY];
            int nextObjectIndex = tile.indexObjectList;
            if (nextObjectIndex == index)
            {//object is first in list, easy swap
                tile.indexObjectList = obj.next;
            }
            else
            {
                while (nextObjectIndex != 0)
                {
                    var nextObj = objList[nextObjectIndex];
                    if (nextObj.next == index)
                    {
                        nextObj.next = obj.next;
                        nextObjectIndex = 0;
                    }
                    else
                    {
                        nextObjectIndex = nextObj.next;
                    }
                }
            }
            obj.next = 0; //ensure end of chain.               
            obj.tileX = 99; obj.tileY = 99;
            if (obj.instance != null)
            {
                obj.instance.uwnode.Position = obj.GetCoordinate(obj.tileX, obj.tileY);
            }
            else
            {
                Debug.Print($"Trying to pick up {obj.a_name} without an instance!");
            }
        }

        /// <summary>
        /// Special events when objects are picked up and data needs to be updated.
        /// </summary>
        /// <param name="obj"></param>
        private static void PickupSpecialCases(uwObject obj)
        {
            if (_RES != GAME_UW2)
            {
                switch (obj.item_id)
                {
                    case 294://Moonstone
                        playerdat.SetMoonstone(0, 0);
                        break;
                    case 458://silver tree
                        silvertree.PickupTree(obj);
                        break;
                }
            }
            else
            {
                switch (obj.item_id)
                {
                    case 294://Moonstone
                        {
                            for (int m = 0; m < 2; m++)
                            {//Clear the moonstone if it matches the current level.
                                if (playerdat.GetMoonstone(m) == playerdat.dungeon_level)
                                {
                                    playerdat.SetMoonstone(m, 0);
                                    break;//only do it for the first match.
                                }
                            }
                            break;
                        }
                    case 312://mors gotha's spellbook
                        {
                            if (obj.doordir == 1)
                            {
                                playerdat.SetQuest(106, 1);
                                //a_do_trap_trespass.HackTrapTrespass(28);//flag trespass to human faction #28 guardian forces, note vanilla behaviour is that dropping and picking the book up again will retrigger this behaviour.
                                thief.FlagTheftToObjectOwner(obj, 28);
                            }
                            break;
                        }
                }
            }
        }

    } //end class
}//end namespace