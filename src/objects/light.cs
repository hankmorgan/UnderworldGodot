namespace Underworld
{
    public class light: objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (WorldObject) { return false; }

            if (uimanager.CurrentSlot >= 5 && uimanager.CurrentSlot <= 8)
            {
                if (ToggleLight(obj))
                {
                    UWsoundeffects.PlaySoundEffectAtAvatar(UWsoundeffects.SoundEffectLight,0x40,0);
                }
            }
            else
            {
                var freeslot = -1;
                //try and find a free slot to move the lightsource to.
                for (int i = 5; i <= 8; i++)
                {
                    if (playerdat.GetInventorySlotListHead(i) == 0)
                    {
                        freeslot = i;
                        break;
                    }
                }
                if (freeslot != -1)
                {
                    //move from slot by picking it up and dropping it.
                    if (obj.ObjectQuantity==1)
                    {
                        uimanager.PickupObjectFromSlot(obj);      
                    }
                    else
                    {
                        //pick up a single instance
                        obj.link--;
                        var newObjIndex = ObjectCreator.SpawnObjectInHand(obj.item_id); //spawning in hand is very handy here
                        var cloneObj = UWTileMap.current_tilemap.LevelObjects[newObjIndex];
                        cloneObj.link = 1;
                        cloneObj.quality = obj.quality;
                        cloneObj.owner = obj.owner;
                        playerdat.ObjectInHand = cloneObj.index;
                    }
                                  
                    var newObj = UWTileMap.current_tilemap.LevelObjects[playerdat.ObjectInHand];
                    LightOn(newObj); //assuming light starts in an off state    
                    UWsoundeffects.PlaySoundEffectAtAvatar(UWsoundeffects.SoundEffectLight,0x40,0);                
                    uimanager.CurrentSlot = freeslot;
                    uimanager.PickupToEmptySlot(playerdat.ObjectInHand);
                }
            }

            uimanager.UpdateInventoryDisplay();
            playerdat.PlayerStatusUpdate();
            return true;
        }

        /// <summary>
        /// Return true if light is now on.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool ToggleLight(uwObject obj)
        {
            //turn light on or off.
            if (obj.classindex <= 3)
            {
                LightOn(obj); 
                return true;               
            }
            else
            {
                LightOff(obj);
                return false;
            }
        }

        public static void LightOff(uwObject obj)
        {
            if (obj.classindex >= 4)
            {
                obj.item_id -= 4;
            }
        }

        public static void LightOn(uwObject obj)
        {
            if (obj.classindex <= 3)
            {
                if (obj.quality > 1 )
                {
                    obj.item_id += 4;
                }
                else
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1,GameStrings.str_that_light_is_already_used_up_));
                }                
            }
        }


        
    }//end class
}//end namespace