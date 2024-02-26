using System.Collections;
using System.Threading.Tasks;
using Godot;
using Peaky.Coroutines;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("MainMenu")]
        [Export]
        public Panel PanelMainMenu;
        [Export]
        public TextureRect MainMenuBG;

        [Export]
        public TextureRect[] MainMenuButtons = new TextureRect[4];

        [Export] public Label LoadingLabel;

        private void InitMainMenu()
        {

            MainMenuBG.Texture = bitmaps.LoadImageAt(BytLoader.OPSCR_BYT);
            //MainMenuBG.Material = bitmaps.GetMaterial(BytLoader.OPSCR_BYT);
            LoadingLabel.Text = "";
            TurnButtonsOff();

        }

        private void TurnButtonsOff()
        {
            for (int i = 0; i < 4; i++)
            {
                MainMenuButtons[i].Texture = grOptbtn.LoadImageAt(i * 2);
            }
        }

        private void HideButtons()
        {
            for (int i = 0; i < 4; i++)
            {
                EnableDisable(MainMenuButtons[i], false);
            }
        }

        private void _on_introduction_mouse_entered()
        {
            TurnButtonsOff();
            MainMenuButtons[0].Texture = grOptbtn.LoadImageAt(1);
        }


        private void _on_introduction_mouse_exited()
        {
            TurnButtonsOff();
            MainMenuButtons[0].Texture = grOptbtn.LoadImageAt(0);
        }


        private void _on_create_character_mouse_entered()
        {
            TurnButtonsOff();
            MainMenuButtons[1].Texture = grOptbtn.LoadImageAt(3);
        }


        private void _on_create_character_mouse_exited()
        {
            TurnButtonsOff();
            MainMenuButtons[1].Texture = grOptbtn.LoadImageAt(2);
        }


        private void _on_acknowledgements_mouse_entered()
        {
            TurnButtonsOff();
            MainMenuButtons[2].Texture = grOptbtn.LoadImageAt(5);
        }


        private void _on_acknowledgements_mouse_exited()
        {
            TurnButtonsOff();
            MainMenuButtons[2].Texture = grOptbtn.LoadImageAt(4);
        }


        private void _on_journey_onwards_mouse_entered()
        {
            TurnButtonsOff();
            MainMenuButtons[3].Texture = grOptbtn.LoadImageAt(7);
        }


        private void _on_journey_onwards_mouse_exited()
        {
            TurnButtonsOff();
            MainMenuButtons[3].Texture = grOptbtn.LoadImageAt(6);
        }


        /// <summary>
        /// Load the save game specified in the config file
        /// </summary>
        /// <param name="event"></param>
        private void _on_journey_onwards_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
            {
                _ = Coroutine.Run(
                   ClearMainMenu()
                   , main.instance);

                _ = Coroutine.Run(
                    JourneyOnwards()
                    , main.instance);
            }
        }

        private IEnumerator ClearMainMenu()
        {
            HideButtons();
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                LoadingLabel.Text = GameStrings.GetString(1, 273);
            }
            else
            {
                LoadingLabel.Text = GameStrings.GetString(1, 257);
            }
            yield return 0;
        }

        private IEnumerator JourneyOnwards()
        {

            playerdat.LoadPlayerDat(datafolder: uwsettings.instance.levarkfolder);

            // //Common launch actions            
            yield return UWTileMap.LoadTileMap(
                    newLevelNo: playerdat.dungeon_level - 1,
                    datafolder: uwsettings.instance.levarkfolder,
                    fromMainMenu: true);

            // _ = Coroutine.Run(
            //     UWTileMap.LoadTileMap(
            //         newLevelNo: playerdat.dungeon_level - 1,
            //         datafolder: uwsettings.instance.levarkfolder,
            //         fromMainMenu: true)
            //         , main.instance);

            yield return null;

        }
    }//end class
}//end namespace