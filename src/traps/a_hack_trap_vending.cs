namespace Underworld
{

    /// <summary>
    /// Trap(s) that control the behaviour of the scintilus academy vending machines
    /// </summary>
    public class a_hack_trap_vending : trap
    {
        static int[] VendingMachineChoices = new int[] { 0xb6, 0xb0, 0xbb, 0x125, 0xbc, 0x3, 0x101, 0x91 };// fish, meat, ale, leech, water, dagger, lockpick, torch
        public static void Activate(uwObject trapObj, int triggerX, int triggerY)
        {
            switch (trapObj.quality)
            {
                case 40: // updates the gamevariable(s) that track the current selection
                    UpdateVendingSelection(trapObj); return;
                case 41: // try and purchase the selected items
                    return;
                case 42: // handles the for sale sign.
                    VendingForSaleSign(trapObj);
                    break;
            }
        }

        static void UpdateVendingSelection(uwObject trapObj)
        {
            if (ObjectThatStartedChain != 0)
            {
                var obj = UWTileMap.current_tilemap.LevelObjects[ObjectThatStartedChain];
                playerdat.SetGameVariable(trapObj.owner, obj.flags);
            }
        }

        static void VendingForSaleSign(uwObject trapObj)
        {
            var selection = playerdat.GetGameVariable(trapObj.owner);
            var selection_itemid = VendingMachineChoices[selection];
            var monetaryvalue = ((commonObjDat.monetaryvalue(selection_itemid) + 1) >> 1) + 1;

            //The plaque reads: A fish is the current selection (3 gp)
            string objectname = GameStrings.GetObjectNounUW(selection_itemid, 1);
            var article = look.GetArticle(objectname, true);
            var msg = $"{article}{GameStrings.GetObjectNounUW(selection_itemid)}{GameStrings.GetString(1, 0x15D)}({monetaryvalue} gp)";
            uimanager.AddToMessageScroll(msg);
        }

    }//end class
}//end namespace