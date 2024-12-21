using System.Diagnostics;


namespace Underworld
{
    public partial class trigger : UWClass
    {
        /// <summary>
        /// test and debug index for testing scheduled triggers
        /// </summary>
        static int scheduledtriggerindex=0;

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
                                        //var tile = UWTileMap.current_tilemap.Tiles[triggerX, triggerY];
                                        //Debug.Print($"Test me. remove trap {trapObj.index} {trapObj.a_name} from object list here");
                                        trap.RemoveSingleUseTrap(
                                            trapObj: trapObj, 
                                            triggerX: triggerX,
                                            triggerY: triggerY);                                        
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
                    if (ObjectToTrigger.IsTrigger)
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

        /// <summary>
        /// Finds the first trap in the specified tile and runs it
        /// </summary>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
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

        /// <summary>
        /// Runs all found timer triggers for debugging
        /// </summary>
        public static void RunTimerTriggers()
        {
            if (_RES!=GAME_UW2){return;}
            for (int i = 256; i<=UWTileMap.current_tilemap.LevelObjects.GetUpperBound(0);i++)
            {
                //if (i==966)
               // {
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
        /// Debug/Test function that will pick the next scheduled trigger on the map and run it.
        /// </summary>
        public static void RunNextScheduledTrigger()
        {
            if (_RES!=GAME_UW2){return;}
            if ((scheduledtriggerindex<256) ||(scheduledtriggerindex>=1024))
            {
                scheduledtriggerindex=256;                
            }
            while(scheduledtriggerindex<1024)
            {
                if ((UWTileMap.current_tilemap.LevelObjects[scheduledtriggerindex].item_id==428) || (UWTileMap.current_tilemap.LevelObjects[scheduledtriggerindex].item_id==444))
                {
                    Debug.Print($"Testing scheduled trigger {scheduledtriggerindex}");
                    RunTrigger(
                        character: 0, 
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
    }//end class
}//end namespace