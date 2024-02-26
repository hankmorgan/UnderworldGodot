using System.Diagnostics;
using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("Runes")]
        [Export] public TextureRect[] RuneTiles = new TextureRect[24];
        [Export] public TextureRect[] SelectedRunes = new TextureRect[3];

        /// <summary>
        /// Sets the display on or off of the rune at the specified slot index.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="state"></param>
        public static void SetRuneInBag(int slot, bool state)
        {
            if (state)
            {
                instance.RuneTiles[slot].Texture = grObjects.LoadImageAt(232 + slot);
                instance.RuneTiles[slot].Material = grObjects.GetMaterial(232 + slot);
            }
            else
            {
                instance.RuneTiles[slot].Texture = null;
            }
        }

        /// <summary>
        /// Handles the rune click event
        /// </summary>
        /// <param name="event"></param>
        /// <param name="extra_arg_0"></param>
        private void RuneClick(InputEvent @event, long extra_arg_0)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                if (extra_arg_0 >= 0)
                {
                    if (playerdat.GetRune((int)extra_arg_0))
                    {
                        if (InteractionMode == InteractionModes.ModeLook)
                        {
                            look.GenericLookDescription((int)(232 + extra_arg_0));
                        }
                        else
                        {
                            //use action
                            Debug.Print($"Rune {extra_arg_0} can be selected");
                            SelectRune((int)extra_arg_0);
                        }
                    }
                    else
                    {
                        //Debug.Print($"Rune {extra_arg_0} is not available");
                    }
                }
                else
                {
                    //clear runes  
                    for (int i = 0; i < 3; i++)
                    {
                        playerdat.SetSelectedRune(i, 24);
                    }
                    RedrawSelectedRuneSlots();
                }
            }
        }


        /// <summary>
		/// Selects a rune from the rune bag, triggers update of the selected ui and updates player dat
		/// </summary>
		/// <param name="NewRuneToSelect"></param>
		static void SelectRune(int NewRuneToSelect)
        {
            //Adds rune to the selected shelf
            if (playerdat.IsSelectedRune(0))
            {
                if (playerdat.IsSelectedRune(1))
                {
                    if (playerdat.IsSelectedRune(2))
                    {
                        //All three slots are filled. Shift values down and fill slot 3
                        playerdat.SetSelectedRune(0, playerdat.GetSelectedRune(1));
                        playerdat.SetSelectedRune(1, playerdat.GetSelectedRune(2));
                        playerdat.SetSelectedRune(2, NewRuneToSelect);
                    }
                    else
                    {
                        //Slot 2 is available.
                        playerdat.SetSelectedRune(2, NewRuneToSelect);
                    }
                }
                else
                {   //slot 1 is available
                    playerdat.SetSelectedRune(1, NewRuneToSelect);
                }
            }
            else
            {//Slot 0 is available.
                playerdat.SetSelectedRune(0, NewRuneToSelect);
            }
            RedrawSelectedRuneSlots();
        }

        /// <summary>
		/// Draws the selected rune slots after a change is made to them.
		/// </summary>
		public static void RedrawSelectedRuneSlots()
        {
            for (int slot = 0; slot < 3; slot++)
            {
                if (playerdat.IsSelectedRune(slot))
                {
                    //display
                    instance.SelectedRunes[slot].Texture = grObjects.LoadImageAt(232 + playerdat.GetSelectedRune(slot));
                    instance.SelectedRunes[slot].Material = grObjects.GetMaterial(232 + playerdat.GetSelectedRune(slot));
                }
                else
                {
                    //clear
                    instance.SelectedRunes[slot].Texture = null;
                }
            }
        }

        /// <summary>
        /// Begin casting the current runic spell
        /// </summary>
        /// <param name="event"></param>
        private void SelectedRunesClick(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                Debug.Print("Casting");
                for (int i = 0; i < 3; i++)
                {
                    if (playerdat.IsSelectedRune(i))
                    {
                        Debug.Print($"{GameStrings.GetObjectNounUW(232 + playerdat.GetSelectedRune(i))}");
                    }
                }
            }
        }

    } //end class
}//end namespace