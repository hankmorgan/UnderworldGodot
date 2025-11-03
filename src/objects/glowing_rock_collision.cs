using Underworld;
using System.Diagnostics;
using Godot;

public partial class glowing_rock_collision : Area3D
{

    public int uwObjectIndex;

    [Signal]
    public delegate void MoveTriggerEnteredEventHandler();

    public void movetrigger_entered(Node3D body)
    {
        Debug.Print($"{body.Name} collides with {uwObjectIndex}");
        if (body.Name == "Gronk")
        {
            Debug.Print("Player has collided with Zanium");
            bool DestroyZanium = false;
            var zanium = objectsearch.FindMatchInFullObjectList(4, 2, 9, playerdat.InventoryObjects);
            if (zanium != null)
            {    //player has zanium. add to the pile.
                zanium.link++;
                uimanager.UpdateInventoryDisplay();
                DestroyZanium = true;
            }
            else
            {
                //try object in hand
                if (playerdat.ObjectInHand != -1)
                {
                    var objInHand = UWTileMap.current_tilemap.LevelObjects[playerdat.ObjectInHand];
                    if (objInHand!=null)
                    {//increase counter on object.
                        objInHand.link++;
                        DestroyZanium = true;
                    }
                }
            }
            if (DestroyZanium)
            {
                var toRemove = UWTileMap.current_tilemap.LevelObjects[uwObjectIndex];
                if (toRemove!=null)
                {
                    ObjectRemover_OLD.DeleteObjectFromTile_DEPRECIATED(toRemove.tileX,toRemove.tileY,toRemove.index);
                }               
            }
        }
    }
}