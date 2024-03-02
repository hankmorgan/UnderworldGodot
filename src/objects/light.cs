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
            RefreshLighting();
            return true;
        }

        public static void RefreshLighting()
        {

            playerdat.lightlevel = BrightestLight();
            Godot.RenderingServer.GlobalShaderParameterSet("cutoffdistance", shade.GetViewingDistance(playerdat.lightlevel));
            Godot.RenderingServer.GlobalShaderParameterSet("shades", shade.shadesdata[playerdat.lightlevel].ToImage());
        }


        private static void ToggleLight(uwObject obj)
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


        /// <summary>
        /// Calculates the brightest light source that surrounds the player
        /// </summary>
        /// <returns></returns>
        public static int BrightestLight()
        {
            int lightlevel = 0; //darkness
            //5,6,7,8
            for (int i =5; i<=8; i++)
            {
                var obj = playerdat.GetInventorySlotObject(i);
                if (obj!=null)
                {
                    if ( (obj.majorclass ==2) && (obj.minorclass==1) && (obj.classindex >=4) && (obj.classindex<=7))
                    {   //object is a lit light
                        var level = lightsourceObjectDat.brightness(obj.item_id);
                        if (level>lightlevel)
                        {
                            lightlevel = level;
                        }
                    }
                }
            }
            //If uw2 check for dungeon light level
            if (_RES==GAME_UW2)
            {
                var dungeon_ambientlight = DlDat.GetAmbientLight(playerdat.dungeon_level-1);
                var remainder = dungeon_ambientlight % 10;
                var dlFlag =0;
                if (dungeon_ambientlight >=10)
                {
                    dlFlag=1;
                }
                int tileLightFlag = UWTileMap.current_tilemap.Tiles[playerdat.tileX,playerdat.tileY].lightFlag;
                if ((tileLightFlag ^ dlFlag) == 1)
                {
                    dungeon_ambientlight = remainder;
                }
                else
                {
                    dungeon_ambientlight = -1;
                }                
                if (dungeon_ambientlight>lightlevel)
                {
                    lightlevel = dungeon_ambientlight;
                }
            }

            //TODO check for magic lights
            return lightlevel;
        }
    }
}//end namespace