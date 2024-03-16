using Godot;

namespace Underworld
{
    public class spike : objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (!WorldObject)
            {
                //flag we are using the spike        
                useon.CurrentItemBeingUsed = new useon(obj, WorldObject);
                //print use message
                uimanager.AddToMessageScroll(GameStrings.GetString(1,0x82));//select door to spike
            }
            return true;
        }

        public static bool UseOn(uwObject SpikeObject, uwObject targetObject, bool WorldObject)
        {
            if (!WorldObject){return false;}
            if ((targetObject.majorclass==5) && (targetObject.minorclass == 0))
            {//doors
                if (targetObject.classindex>=8)
                    {//open doors
                         uimanager.AddToMessageScroll(GameStrings.GetString(1,0x80));
                    }
                else
                {
                   targetObject.owner = 63;
                   uimanager.AddToMessageScroll(GameStrings.GetString(1,0x81));//door is now spiked
                   ObjectCreator.Consume(SpikeObject, true); //assumes using from inventory
                }
                return true;
            }
            return false;
        }
    }//end class
}//end namepace