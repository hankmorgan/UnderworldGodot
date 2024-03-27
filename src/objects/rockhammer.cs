namespace Underworld
{
    public class rockhammer : objectInstance
    {

        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (!WorldObject)
            {
                //flag we are using the pole
                useon.CurrentItemBeingUsed = new useon(obj, WorldObject);
                //print use message
                uimanager.AddToMessageScroll(useon.CurrentItemBeingUsed.GeneralUseOnString);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool UseOn(uwObject hammerobject, uwObject itemTargeted, bool WorldObject)
        {
            if (!WorldObject){return false;}
            if ((itemTargeted.item_id>=0x153) && (itemTargeted.item_id<=0x156))
            {//breaks a large boulder into spawner pieces
                uimanager.AddToMessageScroll(GameStrings.GetString(1,GameStrings.str_the_rock_breaks_into_smaller_pieces_));
                var newItemID = itemTargeted.item_id + Rng.r.Next(1,2);
                if (newItemID>0x156)
                {
                    newItemID = 0x10;
                }

                var newobj = ObjectCreator.spawnObjectInTile(
                    itemid: newItemID, 
                    tileX: itemTargeted.tileX, 
                    tileY: itemTargeted.tileY, 
                    xpos: itemTargeted.xpos, 
                    ypos: itemTargeted.ypos, 
                    zpos: itemTargeted.zpos);

                if (newobj.item_id==0x10)
                {//when a sling stone. 
                    newobj.is_quant = 1;
                    newobj.link = (short)(3 + Rng.r.Next(0,6));
                }

                ObjectCreator.DeleteObjectFromTile(itemTargeted.tileX, itemTargeted.tileY, itemTargeted.index);
                return true;
            }
            else
            {
                uimanager.AddToMessageScroll(GameStrings.GetString(1,GameStrings.str_it_seems_to_have_no_effect_));
                return true;
            }
        }
    }//end class
}//end namespace