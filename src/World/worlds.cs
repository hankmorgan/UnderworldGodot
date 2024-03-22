namespace Underworld
{
    /// <summary>
    /// Levels in Underworld are classified into "worlds". In UW1 simply each level is a world. In UW2 each block of 8 levels is it's own world
    /// This is relevant for how skill checks work
    /// </summary>
    public class worlds:UWClass
    {
        public enum UW1_LevelNos
        {
            EntranceLevel = 0,
            MountainMen = 1,
            Swamp = 2,
            Knights = 3,
            Catacombs = 4,
            Seers = 5,
            Tybal = 6,
            Volcano = 7,
            Ethereal = 8
        };

        public static string[] UW1_LevelNames = new string[]
        {
            "Outcast",
            "Dwarf",
            "Swamp",
            "Knight",
            "Tombs",
            "Seers",
            "Tybal",
            "Abyss",
            "Void"
        };

        /// <summary>
        /// First index of the level no for a world
        /// </summary>
        public enum Worlds
        {
            Britannia = 0,
            PrisonTower = 8,
            Killorn = 16,
            Ice = 24,
            Talorus = 32,
            Academy = 40,
            Tomb = 48,
            Pits = 56,
            Ethereal = 64
        };

        public enum UW2_LevelNos
        {
            Britannia0 = 0,
            Britannia1 = 1,
            Britannia2 = 2,
            Britannia3 = 3,
            Britannia4 = 4,
            Prison0 = 8,
            Prison1 = 9,
            Prison2 = 10,
            Prison3 = 11,
            Prison4 = 12,
            Prison5 = 13,
            Prison6 = 14,
            Prison7 = 15,
            Killorn0 = 16,
            Killorn1 = 17,
            Ice0 = 24,
            Ice1 = 25,
            Talorus0 = 32,
            Talorus1 = 33,
            Academy0 = 40,
            Academy1 = 41,
            Academy2 = 42,
            Academy3 = 43,
            Academy4 = 44,
            Academy5 = 45,
            Academy6 = 46,
            Academy7 = 47,
            Tomb0 = 48,
            Tomb1 = 49,
            Tomb2 = 50,
            Tomb3 = 51,
            Pits0 = 56,
            Pits1 = 57,
            Pits2 = 58,
            Ethereal0 = 64,
            Ethereal1 = 65,
            Ethereal2 = 66,
            Ethereal3 = 67,
            Ethereal4 = 68,
            Ethereal5 = 69,
            Ethereal6 = 70,
            Ethereal7 = 71,
            Ethereal8 = 72
        }; //uw2 floppy level 69 is a secret level containing the lotus.

        /// <summary>
        /// Converts a dungeon level into a world no. Note dungeon level is not zero-based
        /// </summary>
        /// <param name="dungeonlevel"></param>
        /// <returns></returns>
        public static int GetWorldNo(int dungeonlevel)
        {            
            switch(_RES)
            {
                case GAME_UW2:
                    return (dungeonlevel - 1)/8;
                default:
                    return dungeonlevel;
            }
        }

    }//end class
}//end namespace