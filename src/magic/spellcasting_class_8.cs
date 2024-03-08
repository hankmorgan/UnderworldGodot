using System.Diagnostics;

namespace Underworld
{
    public partial class SpellCasting : UWClass
    {
        public static void CastClass8_Summoning(int minorclass)
        {
            //Preamble of getting positon to spawn in.  based on facing direction of the player
            switch (minorclass)
            {
                case 1:
                    {
                        Debug.Print("Create Food"); break;
                    }
                case 2:
                    {
                        if (_RES==GAME_UW2)
                        {
                            Debug.Print ("FLAM RUNE");
                        }
                        break;
                    }
                case 3:
                    {
                        if (_RES==GAME_UW2)
                        {
                            Debug.Print ("TYM RUNE");
                        }
                        else
                        {
                            Debug.Print ("Rune of warding"); //creates a move trigger linked to a trap (#9 ward)
                        }
                        break;
                    }
                case 4:
                    {
                        Debug.Print ("Summon Monster");
                        break;
                    }
                case 5:
                    {
                        if (_RES==GAME_UW2)
                        {
                            Debug.Print ("Summon Demon");
                        }
                        break;
                    }
                case 6:
                    {
                        if (_RES==GAME_UW2)
                        {
                            Debug.Print ("Satellite");
                        }
                        break;
                    }
            }


            //Spawn the object in the tile           

        }
    }   //end class
}//end namespace