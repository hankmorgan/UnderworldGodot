namespace Underworld
{
    public class light: objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (WorldObject) { return false; }

            if (uimanager.CurrentSlot >= 5 && uimanager.CurrentSlot <= 8)
            {
                ToggleLight(obj);
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
                    uimanager.PickupObjectFromSlot(obj.index);
                    var newObj = UWTileMap.current_tilemap.LevelObjects[playerdat.ObjectInHand];
                    LightOn(newObj); //assuming light starts in an off state
                    uimanager.CurrentSlot = freeslot;
                    uimanager.PickupToEmptySlot(playerdat.ObjectInHand);
                }
            }

            uimanager.UpdateInventoryDisplay();
            playerdat.PlayerStatusUpdate();
            return true;
        }

        public static void ToggleLight(uwObject obj)
        {
            //turn light on or off.
            if (obj.classindex <= 3)
            {
                obj.item_id += 4;
            }
            else
            {
                obj.item_id -= 4;
            }
        }

        public static void LightOff(uwObject obj)
        {
            if (obj.classindex <= 3)
            {
                //obj.item_id += 4;
            }
            else
            {
                obj.item_id -= 4;
            }
        }

        public static void LightOn(uwObject obj)
        {
            if (obj.classindex <= 3)
            {
                obj.item_id += 4;
            }
            else
            {
                //obj.item_id -= 4;
            }
        }


        
    }//end class
}//end namespace