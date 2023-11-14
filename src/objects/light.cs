namespace Underworld
{
    public class light: objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (WorldObject){return false;}
            //turn light on or off.
            if (obj.classindex<=3)
            {
                obj.item_id+=4;
            }
            else
            {
                obj.item_id-=4;
            }
            uimanager.RefreshSlot(uimanager.CurrentSlot, playerdat.isFemale);
            uwsettings.instance.lightlevel = BrightestLight();

            Godot.RenderingServer.GlobalShaderParameterSet("cutoffdistance", shade.getShadeCutoff(uwsettings.instance.lightlevel));
            Godot.RenderingServer.GlobalShaderParameterSet("shades", shade.shadesdata[uwsettings.instance.lightlevel].ToImage());

            return true;
        }

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
                            lightlevel= level;
                        }
                    }
                }
            }
            //TODO check for magic lights
            return lightlevel;
        }
    }
}//end namespace