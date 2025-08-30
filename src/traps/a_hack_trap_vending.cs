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
                    VendItem(trapObj);return;
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

        static void VendItem(uwObject trapObj)
        {
            var tile = UWTileMap.current_tilemap.Tiles[trapObj.tileX, trapObj.tileY];
            var selection = playerdat.GetGameVariable(trapObj.owner);
            var selection_itemid = VendingMachineChoices[selection];
            var monetaryvalue = ((commonObjDat.monetaryvalue(selection_itemid) + 1) >> 1) + 1;
            if (CollectMoneyInTile(
                tileX: trapObj.tileX,
                tileY: trapObj.tileY,
                CountOnly: true,
                AmountToCollect: monetaryvalue))
            {//there is enough money to vend, take money for real and spawn the object
                if (CollectMoneyInTile(
                    tileX: trapObj.tileX,
                    tileY: trapObj.tileY,
                    CountOnly: false,
                    AmountToCollect: monetaryvalue))
                {
                    var newobj = ObjectCreator.spawnObjectInTile(
                        itemid: selection_itemid,
                        tileX: trapObj.tileX, tileY: trapObj.tileY,
                        xpos: 4, ypos: 4,
                        zpos: (short)(tile.floorHeight << 3),
                        WhichList: ObjectFreeLists.ObjectListType.StaticList);
                }
            }
        }

        static bool CollectMoneyInTile(int tileX, int tileY, bool CountOnly = true, int AmountToCollect = 0)
        {
            var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
            uwObject objectToCheck = null;
            uwObject nextobject = null;
            if (tile.indexObjectList!=0)
            {
                objectToCheck = UWTileMap.current_tilemap.LevelObjects[tile.indexObjectList];
            }
            while (true)
            {                
                if ((objectToCheck == null) || (objectToCheck != null && AmountToCollect <=0))
                {
                    if (AmountToCollect > 0)
                    {
                        return false;//short money
                    }
                    else
                    {
                        return true; //found all money
                    }
                }
                if (objectToCheck.next!=0)
                {
                    nextobject = UWTileMap.current_tilemap.LevelObjects[objectToCheck.next];//get the reference to the next object now since the coin object may be destroyed by this process
                }
                else
                {
                    nextobject = null;
                }
                
                if (objectToCheck.item_id == 0xA0) // a coin
                {
                    var coinsdeposited = 0;
                    if ((objectToCheck.is_quant == 1) && ((objectToCheck.link & 0x200) == 0))//isquant == 1 and bit 9 is not set
                    {
                        coinsdeposited = objectToCheck.link;
                    }
                    else
                    {
                        coinsdeposited = 1;
                    }
                    if (AmountToCollect < coinsdeposited)
                    {//target value is less than coins in the stack. reduce the stack size if not counting
                        if (!CountOnly)
                        {
                            objectToCheck.link -= (short)AmountToCollect;
                        }                        
                        AmountToCollect  = 0;//set remaining count to 0
                    }
                    else
                    {//target value is greater or equal to the stack size. the stack must be consumed.
                        if (!CountOnly)
                        {//consume the stack
                            ObjectRemover.DeleteObjectFromTile_DEPRECIATED(tileX, tileY, objectToCheck.index);
                        }
                        AmountToCollect -=coinsdeposited;//update the remainin count.
                    }
                }
                
                //try next object
                objectToCheck = nextobject;
            }
        }

    }//end class
}//end namespace