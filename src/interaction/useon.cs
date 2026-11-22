namespace Underworld
{
    /// <summary>
    /// Class for interactions involving the use verb when holding another object
    /// </summary>
    public class useon : UWClass
    {
        public static useon CurrentItemBeingUsed;
        public int index = 0;

        public bool WorldObject;

        public uwObject itemBeingUsed;

        public useon(uwObject obj, bool _worldobject)
        {
            index = obj.index;
            WorldObject = _worldobject;
            itemBeingUsed = obj;
            //change the mouse cursor
            uimanager.instance.mousecursor.SetCursorToObject(obj.item_id);
        }

        public string GeneralUseOnString
        {
            get
            {
                return $"Use {GameStrings.GetObjectNounUW(itemBeingUsed.item_id, 0)} on?";
            }
        }

        public static bool UseOn(uwObject ObjectUsed, useon srcObject, bool WorldObject)
        {

            var ObjInHand = srcObject.itemBeingUsed;
            if (ObjectUsed == null) { return false; }
            trap.ObjectThatStartedChain = ObjectUsed.index;
            bool result = false;
            // if (index <= targetobjList.GetUpperBound(0))
            // {
            //     var targetObj = targetobjList[index];
            switch (ObjInHand.majorclass)
            {
                case 2:
                    {
                        //result = UseOnMajorClass2(obj, objList, WorldObject);
                        break;
                    }
                case 3:
                    {
                        result = UseOnMajorClass3(ObjInHand, ObjectUsed, WorldObject);
                        break;
                    }
                case 4:
                    {
                        result = UseOnMajorClass4(ObjInHand, ObjectUsed, WorldObject);
                        break;
                    }
                case 5:
                    {
                        //result = UseOnMajorClass5(obj, objList, WorldObject);
                        break;
                    }
            }
            uwObject[] targetobjList;
            if (WorldObject)
            {
                targetobjList = UWTileMap.current_tilemap.LevelObjects;
            }
            else
            {
                targetobjList = playerdat.InventoryObjects;
            }
            //Check for use trigger on this action and try activate if so.
            trigger.TriggerObjectLink
            (
                character: 1,
                ObjectUsed: ObjectUsed,
                triggerType: (int)triggerObjectDat.triggertypes.USE,
                triggerX: ObjectUsed.tileX,
                triggerY: ObjectUsed.tileY,
                objList: targetobjList
            );

            //Clear the cursor icons
            CurrentItemBeingUsed = null;
            // playerdat.ObjectInHand = -1;
            uimanager.instance.mousecursor.SetCursorToCursor();

            return false;
        }

        public static bool UseOnMajorClass3(uwObject objInHand, uwObject targetObject, bool WorldObject)
        {
            switch (objInHand.minorclass)
            {
                case 0:
                    {
                        if (_RES != GAME_UW2)
                        {
                            switch (objInHand.classindex)
                            {
                                case >= 2 and <= 6://skulls and bones
                                    return UseBonesOn(objInHand, targetObject);
                            }
                        }
                        break;
                    }
                case 1:
                    {//some misc items                    
                        switch (objInHand.classindex)
                        {
                            case 7://anvil
                                return anvil.UseOn(objInHand, targetObject, WorldObject);
                            case 8://pole
                                return pole.UseOn(objInHand, targetObject, WorldObject);
                        }
                        break;
                    }
                case 2:
                    {
                        if (_RES != GAME_UW2)
                        {
                            switch (objInHand.classindex)
                            {
                                case 7://key of infinity
                                    return key_of_infinity.UseOn(objInHand, targetObject, WorldObject);
                            }
                        }
                        break;
                    }
            }
            return false;
        }

        public static bool UseOnMajorClass4(uwObject objInHand, uwObject targetObject, bool WorldObject)
        {
            switch (objInHand.minorclass)
            {
                case 0: //keys up to 0xE
                    {
                        if (_RES == GAME_UW2)
                        {
                            switch (objInHand.classindex)
                            {
                                case 0:
                                case 1:
                                    //LOCKPICK and curious implement.
                                    return lockpick.UseOn(objInHand, targetObject);
                                case > 1 and < 0xE://keys
                                    return doorkey.UseOn(objInHand, targetObject);
                            }
                        }
                        else
                        {
                            switch (objInHand.classindex)
                            {
                                case 0:
                                case > 1 and < 0xE://keys
                                    return doorkey.UseOn(objInHand, targetObject);
                                case 1://lockpick
                                    return lockpick.UseOn(objInHand, targetObject);
                            }
                        }
                        break;
                    }
                case 1:
                    {
                        if (_RES == GAME_UW2)
                        {
                            switch (objInHand.classindex)
                            {
                                case >= 8 and <= 0xF:
                                    return smallblackrockgem.UseOn(objInHand, targetObject, WorldObject);
                            }
                        }
                        else
                        {
                            switch(objInHand.classindex)
                            {
                                case 2: //orbrock
                                    return OrbRock.UseOn(OrbObject: objInHand, targetObject: targetObject, WorldObject: WorldObject, playerUsing: true);
                            }
                        }
                        break;
                    }
                case 2:// misc usables
                    {
                        switch (objInHand.classindex)
                        {
                            case 0x7:   //spike in uw1
                                {
                                    if (_RES != GAME_UW2)
                                    {
                                        return spike.UseOn(objInHand, targetObject, WorldObject);
                                    }
                                    break;
                                }
                            case 0x8://rock hammer
                                {
                                    return rockhammer.UseOn(objInHand, targetObject, WorldObject);
                                }
                            case 0xD://oil flask
                                {
                                    return oilflask.UseOn(objInHand, targetObject);
                                }
                        }
                        break;
                    }
            }
            return false;
        }

        /// <summary>
        /// UW1 bones used on something. Garamon's bones on his grave bury him. (UseBonesOn, uw1 USEITEMS.C:416-454)
        /// </summary>
        public static bool UseBonesOn(uwObject bones, uwObject targetObject)
        {
            bool used = false;
            if (targetObject.item_id == 0x165)//gravestone
            {
                if (bones.owner == 0x3E)
                {
                    if ((targetObject.is_quant == 1) && ((targetObject.link & 0x200) != 0) && ((targetObject.link & 0x1FF) == 0x21))
                    {
                        playerdat.CanTalismansBeDestroyed = true;
                        playerdat.IsGaramonBuried = true;
                        targetObject.link = 0x222;
                        //Garamon talks
                        var garamon = talk.SpawnTemporaryTalker(
                            whoami: 0x1B, itemid: 0x7E,
                            tileX: playerdat.playerObject.tileX, tileY: playerdat.playerObject.tileY,
                            attitude: 3, goal: 7);
                        talk.Talk(garamon, true);
                        //set off the trigger at tile (0x36, 0x34)
                        var tile = UWTileMap.current_tilemap.Tiles[0x36, 0x34];
                        if (tile.indexObjectList != 0)
                        {
                            trigger.RunTrigger(
                                character: 1,
                                ObjectUsed: null,
                                TriggerObject: UWTileMap.current_tilemap.LevelObjects[tile.indexObjectList],
                                triggerType: (int)triggerObjectDat.triggertypes.MOVE,
                                objList: UWTileMap.current_tilemap.LevelObjects);
                        }
                        used = true;
                    }
                    else
                    {
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x103));
                        return true;
                    }
                }
                else
                {
                    used = true;
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x86));
                }
            }
            if (!used)
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1, 0x84));
            }
            else
            {
                ObjectCreator.Consume(bones, true);
            }
            return true;
        }

    } //end class
} //end namespace