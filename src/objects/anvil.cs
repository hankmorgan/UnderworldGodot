using GodotPlugins.Game;
using Peaky.Coroutines;
using System.Collections;
using System.Diagnostics;

namespace Underworld
{
    public class anvil : objectInstance
    {
        public static bool Use(uwObject obj, bool WorldObject)
        {
            //flag we are using the anvil
            useon.CurrentItemBeingUsed = new useon(obj, WorldObject);
            //print use message
            uimanager.AddToMessageScroll(useon.CurrentItemBeingUsed.GeneralUseOnString);
            return true;
        }

        public static bool UseOn(uwObject itemToRepair, uwObject targetObject, bool WorldObject)
        {
            Debug.Print("useon repair");
            //Check if object can be repaired.

            //do repair
            _ = Peaky.Coroutines.Coroutine.Run(
                RepairLogic(),
                main.instance
                );
            return true;
        }

        public static IEnumerator RepairLogic()
        {
            //do estimate of difficulty

            //Ask y/n to repair
            main.gamecam.Set("MOVE", false);
            MessageDisplay.WaitingForYesOrNo = true;
            uimanager.instance.scroll.Clear();
            uimanager.AddToMessageScroll("You think it might be ? make the repair? {TYPEDINPUT}", mode: MessageDisplay.MessageDisplayMode.TypedInput);

            while (MessageDisplay.WaitingForYesOrNo)
            {
                yield return new WaitOneFrame();
            }
             var response = uimanager.instance.TypedInput.Text;
             if (response.ToUpper()=="YES")
             {
                //Attempt repair
                Debug.Print("attempt");
                yield return 0;
             }
             else
             {
                Debug.Print("Cancel");
                yield return 0;
             }
        }
    }//end class
}//end namespace