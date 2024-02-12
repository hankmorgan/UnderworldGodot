using System;
using System.Diagnostics;
using Godot;

namespace Underworld
{

    public partial class uimanager : Node2D
    {

        public enum InteractionModes
        {
            ModeOptions = 0,
            ModeTalk = 1,
            ModePickup = 2,
            ModeLook = 3,
            ModeAttack = 4,
            ModeUse = 5
        };

        public static InteractionModes InteractionMode = InteractionModes.ModeUse;
        [ExportGroup("InteractionModes")]
        //Array to store the interaction mode mo
        [Export] public Godot.TextureButton[] InteractionButtonsUW1 = new Godot.TextureButton[6];
        [Export] public Godot.TextureButton[] InteractionButtonsUW2 = new Godot.TextureButton[6];


        private void InitInteraction()
        {
            switch (UWClass._RES)
            {
                case UWClass.GAME_UW2:
                    for (int i = 0; i <= InteractionButtonsUW2.GetUpperBound(0); i++)
                    {
                        InteractionButtonsUW2[i].TexturePressed = UW2OptBtnsOn[i]; // grLfti.LoadImageAt(i*2 + 1,false);
                        InteractionButtonsUW2[i].TextureNormal = UW2OptBtnsOff[i]; //grLfti.LoadImageAt(i*2,false);  
                        InteractionButtonsUW2[i].SetPressedNoSignal((i == (int)InteractionMode));
                    }
                    break;
                default:
                    for (int i = 0; i <= InteractionButtonsUW1.GetUpperBound(0); i++)
                    {
                        InteractionButtonsUW1[i].TexturePressed = grLfti.LoadImageAt(i * 2 + 1, false);
                        InteractionButtonsUW1[i].TextureNormal = grLfti.LoadImageAt(i * 2, false);
                        InteractionButtonsUW1[i].SetPressedNoSignal((i == (int)InteractionMode));
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles interacting with objects in the world
        /// </summary>
        /// <param name="index"></param>
        public static void InteractWithObjectCollider(int index)
        {
            switch (InteractionMode)
            {
                case InteractionModes.ModeTalk:
                    talk.Talk(
                        index: index, 
                        objList: Underworld.UWTileMap.current_tilemap.LevelObjects, 
                        WorldObject: true);
                    break;
                case InteractionModes.ModeLook:
                    //Do a look interaction with the object
                    look.LookAt(
                        index: index, 
                        objList: Underworld.UWTileMap.current_tilemap.LevelObjects, 
                        WorldObject: true);
                    break;
                case InteractionModes.ModeUse:
                    //do a use interaction with the object.
                    use.Use(
                        index: index, 
                        objList: Underworld.UWTileMap.current_tilemap.LevelObjects, 
                        WorldObject: true);
                    break;
                case InteractionModes.ModePickup:
                    pickup.PickUp(
                        index: index, 
                        objList: Underworld.UWTileMap.current_tilemap.LevelObjects, 
                        WorldObject: true);
                    break;
            }
        }




        public void InteractionModeToggle(InteractionModes index)
        {
            Debug.Print($"Press {index}");

            if (UWClass._RES == UWClass.GAME_UW2)
            {

                for (int i = 0; i <= instance.InteractionButtonsUW2.GetUpperBound(0); i++)
                {
                    InteractionButtonsUW2[i].SetPressedNoSignal(i == (int)(index));
                    if (i == (int)(index))
                    {
                        InteractionMode = index;
                    }
                }
            }
            else
            {
                for (int i = 0; i <= instance.InteractionButtonsUW1.GetUpperBound(0); i++)
                {
                    InteractionButtonsUW1[i].SetPressedNoSignal(i == (int)(index));
                    if (i == (int)(index))
                    {
                        InteractionMode = index;
                    }
                }
            }
        }
    }//end class    
}//end namespace