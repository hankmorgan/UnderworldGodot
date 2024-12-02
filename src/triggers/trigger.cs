using System.Diagnostics;


namespace Underworld
{
    public partial class trigger : UWClass
    {



        /// <summary>
        /// Tests if object is a trigger(true) or a not(false)
        /// </summary>
        /// <param name="toTest"></param>
        /// <returns></returns>
        static bool IsTrigger(uwObject toTest)
        {
            if (_RES == GAME_UW2)
            {
                return (toTest.majorclass == 6) && ((toTest.minorclass == 2) || (toTest.minorclass == 3));
            }
            else
            {
                return (toTest.majorclass == 6) && (toTest.minorclass == 2);
            }
        }


        public static int RunTrigger(int character, uwObject ObjectUsed, uwObject TriggerObject, int triggerType, uwObject[] objList)
        {            
            if (IsTrigger(TriggerObject))
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
                    if (character == 0)
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
                                    trigger.RunTrap(
                                        character: character,
                                        ObjectUsed: ObjectUsed,
                                        trapObject: trapObj,
                                        triggerX: triggerX,
                                        triggerY: triggerY,
                                        objList: objList);

                                    //Test for trap repeat.
                                    if (TriggerObject.flags1 == 0)
                                    {
                                        Debug.Print("Test me. remove trap from object list here");
                                    }
                                    //if uw2 test for pressure triggers
                                    if (_RES == GAME_UW2)
                                    {
                                        if ((triggerType & 0xF07) == 7)
                                        {
                                            Debug.Print("Do something with pressure triggers and texures");
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
                var ObjectToTrigger = objectsearch.FindMatchInObjectListChain(
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
                            var buttonTriggerNext = objectsearch.FindMatchInObjectListChain(
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
                    if (IsTrigger(ObjectToTrigger))
                    {
                        //Get the next of the trigger object first
                        var toTriggerNext = objectsearch.FindMatchInObjectListChain(
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

        public static void TriggerTrapInTile(int tileX, int tileY)
        {
            var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
            var traptrigger = objectsearch.FindMatchInObjectListChain(tile.indexObjectList,6,-1,-1,UWTileMap.current_tilemap.LevelObjects);
            if (traptrigger!=null)
            {
                if ((traptrigger.minorclass & 2) == 0)
                {
                    trap.ActivateTrap(
                        character: 0, 
                        trapObj: traptrigger, 
                        ObjectUsed: null, 
                        triggerX: tileX, 
                        triggerY: tileY, 
                        objList: UWTileMap.current_tilemap.LevelObjects);
                }
                else
                {
                    trigger.RunTrigger(
                        character: 0, 
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

        public static void RunTimerTriggers()
        {
            if (_RES!=GAME_UW2){return;}
            for (int i = 256; i<=UWTileMap.current_tilemap.LevelObjects.GetUpperBound(0);i++)
            {
                if ((UWTileMap.current_tilemap.LevelObjects[i].item_id==425) || (UWTileMap.current_tilemap.LevelObjects[i].item_id==441))
                {
                    RunTrigger(
                        character: 0, 
                        ObjectUsed: null, 
                        TriggerObject: UWTileMap.current_tilemap.LevelObjects[i], 
                        triggerType: (int)triggerObjectDat.triggertypes.TIMER, 
                        objList: UWTileMap.current_tilemap.LevelObjects);
                }
            }
        }

        /// <summary>
        /// General trigger function for the execution of triggers generically (call this from the traps only when continuing a chain
        /// Specialised triggers like look and use triggers should be called directly by the interaction modes.
        /// IE this is use to continue chains, not start them so it should not call the start and end trigger chain events
        /// </summary>
        /// <param name="srcObject"></param>
        /// <param name="triggerIndex"></param>
        /// <param name="objList"></param>
        // public static void Trigger_OBSOLETE(uwObject srcObject, int triggerIndex, uwObject[] objList)
        // {
        //     if (triggerIndex != 0)
        //     {
        //         var Trig = objList[triggerIndex];
        //         if (Trig != null)
        //         {
        //             if (Trig.link != 0)
        //             {
        //                 trap.ActivateTrap(Trig, Trig.link, objList);
        //             }
        //         }
        //     }
        // }

        /// <summary>
        /// Handles common start events at start of chain.
        // /// </summary>
        // public static void StartTriggerChainEvents()
        // {
        //     TeleportLevel = -1;
        //     TeleportTileX = -1;
        //     TeleportTileY = -1;
        //     DoRedraw = false;
        // }      


    }//end class
}//end namespace