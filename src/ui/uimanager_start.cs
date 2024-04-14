using Godot;
namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("Startmenu")]
        [Export]
        public CanvasLayer StartMenuPanel;

        [Export]
        public TextEdit pathuw1;

        [Export]
        public TextEdit pathuw2;

        [Export]
        FileDialog filed;

        static int currentpathselectmode;

        private void _on_path_input(InputEvent @event, long extra_arg_0)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
                currentpathselectmode = (int)extra_arg_0;
                switch (extra_arg_0)
                {
                    case 1:
                        filed.CurrentPath = pathuw1.Text;
                        break;
                }
                //filed.CurrentPath = pathuw1.Text;
                filed.Show();
                // var filed = new FileDialog();
                // filed.FileMode = FileDialog.FileModeEnum.OpenFile;
                // filed.Filters = new string[]{"*.exe"};
                // filed.CurrentPath = pathuw1.Text;
                // filed.Show();            
            }
        }

        private void _on_game_select_file_dialog_file_selected(string path)
        {

            var pathdir = System.IO.Path.GetDirectoryName(path);
            switch (currentpathselectmode)
            {
                case 1:
                    pathuw1.Text = pathdir; break;
                case 2:
                    pathuw2.Text = pathdir; break;
            }
        }

        private void _on_launch_input(InputEvent @event, long extra_arg_0)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
            {
                switch (extra_arg_0)
                {
                    case 1:
                        uwsettings.instance.gametoload = "UW1";  
                        UWClass._RES = UWClass.GAME_UW1;
                        UWClass.BasePath = uwsettings.instance.pathuw1;  
                        uwsettings.Save();                    
                        main.StartGame();
                        break;
                    case 2:
                        uwsettings.instance.gametoload = "UW2";
                        UWClass._RES = UWClass.GAME_UW2;
                        UWClass.BasePath = uwsettings.instance.pathuw2;
                        uwsettings.Save();
                        main.StartGame();
                        break;
                }
            }
        }
    }//end class
}//end namespace