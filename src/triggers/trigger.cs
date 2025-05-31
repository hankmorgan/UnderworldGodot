using System.Diagnostics;
using System.Runtime.Serialization;


namespace Underworld
{
    public partial class trigger : UWClass
    {
        /// <summary>
        /// test and debug index for testing scheduled triggers
        /// </summary>
        static int scheduledtriggerindex = 0;

        /// <summary>
        /// Runs the the trigger class object (if it matches the required triggerType)
        /// </summary>
        /// <param name="character"></param>
        /// <param name="ObjectUsed"></param>
        /// <param name="TriggerObject"></param>
        /// <param name="triggerType"></param>
        /// <param name="objList"></param>
        /// <returns></returns>
        public static int RunTrigger(int character, uwObject ObjectUsed, uwObject TriggerObject, int triggerType, uwObject[] objList)
        {
            if (TriggerObject.IsTrigger)
            {
                if (triggerType >= 0)
                {
                    if (ObjectUsed != null)
                    {
                        if (ObjectUsed.OneF0Class == 0x17)
                        {
                            if (ObjectUsed.classindex > 7)
                            {
                                //on button
                                if (TriggerObject.next != 0)
                                {
                                    var nextObj = objList[TriggerObject.next];
                                    Debug.Print($"Run Trigger {TriggerObject.index} {TriggerObject.a_name}");
                                    return RunTrigger(
                                        character: character,
                                        ObjectUsed: ObjectUsed,
                                        TriggerObject: nextObj,
                                        triggerType: triggerType,
                                        objList: objList);
                                }
                            }
                        }
                    }
                }
                if (triggerType == triggerObjectDat.triggertype(TriggerObject.item_id) || (triggerType == (int)triggerObjectDat.triggertypes.ALL))
                {
                    if (character == 1)
                    {//player
                        if (TriggerObject.flags2 != 0)
                        {
                            if (triggerType == (int)triggerObjectDat.triggertypes.LOOK)
                            {
                                if (TriggerObject.zpos > 0)
                                {
                                    var skillcheck = playerdat.SkillCheck(playerdat.Search, TriggerObject.zpos);
                                    if (skillcheck <= 0)
                                    {//failed search
                                        return 2;
                                    }
                                }
                            }
                            //run trap.
                            if (TriggerObject.link != 0)
                            {
                                var trapObj = objList[TriggerObject.link];
                                if (trapObj != null)
                                {
                                    int triggerX = TriggerObject.quality;
                                    int triggerY = TriggerObject.owner;
                                    RunTrap(
                                        character: character,
                                        ObjectUsed: ObjectUsed,
                                        trapObject: trapObj,
                                        triggerX: triggerX,
                                        triggerY: triggerY,
                                        objList: objList);

                                    
                                    //Test for trap repeat.
                                    if (TriggerObject.flags1 == 0)
                                    {           
                                        var tile = UWTileMap.current_tilemap.Tiles[triggerX, triggerY];                             
                                        //Debug.Print($"Test me. remove trap {trapObj.index} {trapObj.a_name} from object list here");
                                        ObjectRemover.RemoveTrapChain(
                                            trapObj: trapObj,
                                            ptrListHead: tile.Ptr + 2);
                                    }
                                    //if uw2 test for pressure triggers
                                    if (_RES == GAME_UW2)
                                    {
                                        if ((triggerType & 0xF07) == 7)
                                        {
                                            var tile = UWTileMap.current_tilemap.Tiles[TriggerObject.tileX, TriggerObject.tileY];
                                            ChangePressureTriggerTexture(tile, TriggerObject);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.Print("To implement NPC activation of triggers");
                    }
                }
            }
            return 2;
        }

        public static void RunTrap(int character, uwObject ObjectUsed, uwObject trapObject, int triggerX, int triggerY, uwObject[] objList)
        {//directly triggers a trap
            trap.ActivateTrap(
                character: character,
                trapObj: trapObject,
                ObjectUsed: ObjectUsed,
                triggerX: triggerX,
                triggerY: triggerY,
                objList: objList);
        }

        /// <summary>
        /// Handles the triggering of object links
        /// </summary>
        /// <param name="character"></param>
        /// <param name="ObjectUsed"></param>
        /// <param name="triggerType"></param>
        /// <param name="triggerX"></param>
        /// <param name="triggerY"></param>
        public static void TriggerObjectLink(int character, uwObject ObjectUsed, int triggerType, int triggerX, int triggerY, uwObject[] objList)
        {
            bool RunNext = true;
            if (ObjectUsed.is_quant == 0 && ObjectUsed.link > 0)
            {
                var ObjectToTrigger = objectsearch.FindMatchInObjectListChainNextObjectsOnly(
                    ListHeadIndex: ObjectUsed.link,
                    majorclass: 6,
                    minorclass: -1,
                    classindex: -1,
                    objList: objList);

                if (ObjectUsed.OneF0Class == 0x17)
                {//a button/switch
                    RunNext = false; //flag so that no next triggers are ran
                    if (ObjectUsed.classindex > 7)
                    {//button is in on postion
                        if (ObjectToTrigger != null)
                        {
                            var buttonTriggerNext = objectsearch.FindMatchInObjectListChainNextObjectsOnly(
                                ListHeadIndex: ObjectUsed.link,
                                majorclass: 6,
                                minorclass: -1,
                                classindex: -1,
                                objList: objList);
                            if (buttonTriggerNext != null)
                            {
                                ObjectToTrigger = buttonTriggerNext;
                            }
                        }
                    }
                }

                while (ObjectToTrigger != null)
                {
                    if (ObjectToTrigger.IsTrigger)
                    {
                        //Get the next of the trigger object first
                        var toTriggerNext = objectsearch.FindMatchInObjectListChainNextObjectsOnly(
                                ListHeadIndex: ObjectToTrigger.next,
                                majorclass: 6,
                                minorclass: -1,
                                classindex: -1,
                                objList: objList);

                        RunTrigger(
                            character: character,
                            ObjectUsed: ObjectUsed,
                            TriggerObject: ObjectToTrigger,
                            triggerType: triggerType,
                            objList: objList);

                        if (RunNext)
                        {
                            ObjectToTrigger = toTriggerNext;
                        }
                        else
                        {
                            ObjectToTrigger = null; //end process
                        }
                    }
                    else
                    { //linked to a trap. if not a door trap linked to a use trigger with flags=0, run it once and then delete
                        if (
                            (ObjectToTrigger.flags_full == 0)
                            &&
                            (triggerType == 4)
                            &&
                            ((ObjectToTrigger.item_id & 0x3F) != 8)
                            )
                        {

                            RunTrap(
                                character: character,
                                ObjectUsed: ObjectUsed,
                                trapObject: ObjectToTrigger,
                                triggerX: triggerX,
                                triggerY: triggerY,
                                objList: objList);

                            Debug.Print("TODO RemoveThisTriggerChain");
                        }
                        else
                        {
                            ObjectToTrigger = null;//end process
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds the first trap in the specified tile and runs it
        /// </summary>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        public static void TriggerTrapInTile(int tileX, int tileY)
        {
            var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
            var traptrigger = objectsearch.FindMatchInObjectListChainNextObjectsOnly(tile.indexObjectList, 6, -1, -1, UWTileMap.current_tilemap.LevelObjects);
            if (traptrigger != null)
            {
                if ((traptrigger.minorclass & 2) == 0)
                {
                    trap.ActivateTrap(
                        character: 1,
                        trapObj: traptrigger,
                        ObjectUsed: null,
                        triggerX: tileX,
                        triggerY: tileY,
                        objList: UWTileMap.current_tilemap.LevelObjects);
                }
                else
                {
                    RunTrigger(
                        character: 1,
                        ObjectUsed: null,
                        TriggerObject: traptrigger,
                        triggerType: -1,
                        objList: UWTileMap.current_tilemap.LevelObjects);
                }
            }

        }

        public static void RunScheduledTriggerInTile_15_29(int xhome, int yhome)
        {
            Debug.Print("Find schedule triggers in this hard coded tile and run it to create npcs");
        }

        /// <summary>
        /// Runs all found timer triggers for debugging
        /// </summary>
        public static void RunTimerTriggers()
        {
            if (_RES != GAME_UW2) { return; }
            for (int i = 256; i <= UWTileMap.current_tilemap.LevelObjects.GetUpperBound(0); i++)
            {
                //if (i==884)
                //{
                if ((UWTileMap.current_tilemap.LevelObjects[i].item_id == 425) || (UWTileMap.current_tilemap.LevelObjects[i].item_id == 441))
                {
                    RunTrigger(
                        character: 1,
                        ObjectUsed: null,
                        TriggerObject: UWTileMap.current_tilemap.LevelObjects[i],
                        triggerType: (int)triggerObjectDat.triggertypes.TIMER,
                        objList: UWTileMap.current_tilemap.LevelObjects);
                }
                //}
            }
        }

        /// <summary>
        /// Debug/Test function that will pick the next scheduled trigger on the map and run it.
        /// </summary>
        public static void RunNextScheduledTrigger()
        {
            if (_RES != GAME_UW2) { return; }
            if ((scheduledtriggerindex < 256) || (scheduledtriggerindex >= 1024))
            {
                scheduledtriggerindex = 256;
            }

            while (scheduledtriggerindex < 1024)
            {
                if ((UWTileMap.current_tilemap.LevelObjects[scheduledtriggerindex].item_id == 428) || (UWTileMap.current_tilemap.LevelObjects[scheduledtriggerindex].item_id == 444))
                {
                    Debug.Print($"Testing scheduled trigger {scheduledtriggerindex}");
                    RunTrigger(
                        character: 1,
                        ObjectUsed: null,
                        TriggerObject: UWTileMap.current_tilemap.LevelObjects[scheduledtriggerindex],
                        triggerType: (int)triggerObjectDat.triggertypes.SCHEDULED,
                        objList: UWTileMap.current_tilemap.LevelObjects);
                    scheduledtriggerindex++;
                    return;
                }
                else
                {
                    scheduledtriggerindex++;
                }
            }
        }


        /// <summary>
        /// Checks the tile for the matching trigger of type EXIT, ENTER, PRESSURE or PRESSURE_RELEASE and handles triggering logic for each type.
        /// </summary>
        /// <param name="triggeringObject"></param>
        /// <param name="tile"></param>
        /// <param name="ZParam"></param>
        /// <param name="triggerType"></param>
        /// <returns></returns>
        public static bool RunPressureEnterExitTriggersInTile(uwObject triggeringObject, TileInfo tile, int ZParam, int triggerType)
        {
            var Result = false;
            bool PressureTriggerHasBeenRan = false;
            uwObject PressureTriggerObject = null;

            if (tile.indexObjectList != 0)
            {
                var next = tile.indexObjectList;

                while (next != 0)
                {
                    var NextObject = UWTileMap.current_tilemap.LevelObjects[next];
                    if ((NextObject.OneF0Class & 0x1E) == 0x1A)
                    {
                        Debug.Print($"{NextObject.index} {NextObject.a_name} is a trigger to test");
                        //NextObject is a trigger.
                        if (triggerObjectDat.triggertype(NextObject.item_id) == triggerType)
                        {
                            if (((triggerType & 0x7) == (int)triggerObjectDat.triggertypes.ENTER) || ((triggerType & 0x7) == (int)triggerObjectDat.triggertypes.EXIT))
                            {
                                Debug.Print($"Running {NextObject.index} {NextObject.a_name}");
                                RunTrigger(triggeringObject.index, null, NextObject, triggerType, UWTileMap.current_tilemap.LevelObjects);
                            }
                        }
                        if ((triggerType & 7) == 6)
                        {
                            //pressure trigger
                            triggerType++;
                        }
                        if (((triggerType & 7) == 7) && (triggerType == triggerObjectDat.triggertype(NextObject.item_id)))
                        {
                            //ovr166_29A2:
                            Result = true;
                            if (NextObject.zpos == ZParam)
                            {
                                if
                                (
                                    (((NextObject.ypos & 0x1) == 1) && (triggerType == 0xF))
                                    ||
                                    (((NextObject.ypos & 0x1) == 0) && (triggerType == 0x7))
                                )
                                {
                                    Debug.Print($"Weighing {NextObject.index} {NextObject.a_name}");
                                    var WeightCheck = CheckWeightOnPressureTrigger(
                                        ListHead: tile.indexObjectList,
                                        MinWeight: (short)(4 + (((NextObject.ypos & 0x4) >> 2) * 0x50)),
                                        TileHeight: NextObject.zpos,
                                        PlayerInventoryWeightAdjustment: playerdat.PlayerWeightPlusObjectInHand);

                                    if (
                                        ((WeightCheck == 0) && (triggerType == 0xF))
                                        ||
                                        ((WeightCheck == 1) && (triggerType == 7))
                                        )
                                    {
                                        Debug.Print($"Running {NextObject.index} {NextObject.a_name} after weigh check");
                                        RunTrigger(playerdat.playerObject.index, null, NextObject, triggerType, UWTileMap.current_tilemap.LevelObjects);
                                        PressureTriggerHasBeenRan = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            //ovr166_2A5B:
                            if ((triggerType & 7) == 7) //was looking for a pressure/pressure release trigger
                            {
                                if ((triggerObjectDat.triggertype(NextObject.item_id) & 0x7) == 7)
                                {
                                    PressureTriggerObject = NextObject;
                                }
                            }
                        }


                    }
                    next = NextObject.next;
                }

                //after loop
                //ovr166_2AAC
                if (PressureTriggerObject != null)
                {
                    if (PressureTriggerHasBeenRan == false)
                    {
                        Result = true;
                        if
                            (
                                (((PressureTriggerObject.ypos & 0x1) == 1) && (triggerType == 0xF))
                                ||
                                (((PressureTriggerObject.ypos & 0x1) == 0) && (triggerType == 0x7))
                            )
                        {
                            var WeightCheck = CheckWeightOnPressureTrigger(
                                    ListHead: tile.indexObjectList,
                                    MinWeight: (short)(4 + (((PressureTriggerObject.ypos & 0x4) >> 2) * 0x50)),
                                    TileHeight: PressureTriggerObject.zpos,
                                    PlayerInventoryWeightAdjustment: playerdat.PlayerWeightPlusObjectInHand);
                            if (
                                ((WeightCheck == 0) && (triggerType == 0xF))
                                ||
                                ((WeightCheck == 1) && (triggerType == 7))
                                )
                            {
                                ChangePressureTriggerTexture(tile, PressureTriggerObject);
                            }
                        }
                    }
                }
            }

            return Result;
        }
    }//end class
}//end namespace