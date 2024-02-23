using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {

        private void _on_compass_click(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
            {
                scroll.Clear();
                AddToMessageScroll("\n");
                if (UWClass._RES == UWClass.GAME_UW2)
                {                    
                    StatusMessageUW2();
                }
                else
                {
                    StatusMessageUW1();
                }
            }
        }

        private static void StatusMessageUW2()
        {
            var hunger = 117 +  playerdat.play_hunger / 30;
            var output =
                GameStrings.GetString(1, GameStrings.str_you_are_currently_)
                +
                GameStrings.GetString(1, hunger) 
                +
                GameStrings.GetString(1, GameStrings.str__and_);        
            
            if (playerdat.DreamingInVoid)
            {
                output += GameStrings.GetString(1, 0x128);//sleeping
            }
            else
            {
                //general fatigue string
                var fatigue = playerdat.play_fatigue / 23;
                if (fatigue > 5) { fatigue = 5; }
                fatigue = 131 - fatigue;
                output += GameStrings.GetString(1, fatigue);
            }
            AddToMessageScroll(output);

            var worldsKnown = playerdat.GetQuest(131);
            if (worldsKnown == 0)
                {//other world names are unknown
                    //do nothing
                }
            else
                {//other world names are known
                    var world = worlds.GetWorldNo(playerdat.dungeon_level);
                    worldsKnown = 1 + (worldsKnown << 1);
                    var testval = 1 << world;
                    int di;
                    if ((testval & worldsKnown) !=0 ) //check if we know the name of this world.
                    {
                        di = world + 2;
                    }
                    else
                    {
                        di = 1;                            
                    }

                    output  = 
                        GameStrings.GetString(1, 0x49)
                        +
                        GameStrings.GetString(1, 0x49 + di)
                        + ".";

                    AddToMessageScroll(output);
                }

            var timeofday = (playerdat.ClockValue / 0x1c2000);
            timeofday = timeofday % 0x12;
            
            output =
                GameStrings.GetString(1, GameStrings.str_you_guess_that_it_is_currently_)
                +
                GameStrings.GetString(1, timeofday + 84);

            AddToMessageScroll(output);
            
        }

        private static void StatusMessageUW1()
        {
            var hunger = 104 + playerdat.play_hunger / 30;
            var fatigue = playerdat.play_fatigue / 23;
            if (fatigue > 5) { fatigue = 5; }
            fatigue = 118 - fatigue;

            //hunger and fatigue line
            var output =
                GameStrings.GetString(1, GameStrings.str_you_are_currently_)
                +
                GameStrings.GetString(1, hunger)
                +
                GameStrings.GetString(1, GameStrings.str__and_)
                +
                GameStrings.GetString(1, fatigue)
                +
                ".";

            AddToMessageScroll(output);

            var level = playerdat.dungeon_level + 410;
            output =
                GameStrings.GetString(1, GameStrings.str_you_are_on_the_)
                +
                GameStrings.GetString(1, level)
                +
                GameStrings.GetString(1, GameStrings.str__level_of_the_abyss_);

            AddToMessageScroll(output);

            var timeofday = (playerdat.ClockValue / 0x1c2000);
            var noOfDays = timeofday / 12;
            timeofday = timeofday % 12;

            if (noOfDays > 100)
            {
                output = GameStrings.GetString(1, GameStrings.str_it_has_been_an_uncountable_number_of_days_since_you_entered_the_abyss_);
            }
            else
            {
                output =
                    GameStrings.GetString(1, GameStrings.str_it_is_the_)
                    +
                    GameStrings.GetString(1, noOfDays + 411)
                    +
                    GameStrings.GetString(1, GameStrings.str__day_of_your_imprisonment_);

            }
            AddToMessageScroll(output);

            output =
                GameStrings.GetString(1, GameStrings.str_you_guess_that_it_is_currently_)
                +
                GameStrings.GetString(1, timeofday + 71);

            AddToMessageScroll(output);
        }
    }//end class
}//end namespace