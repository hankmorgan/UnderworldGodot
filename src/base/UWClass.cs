namespace Underworld
{
    /// <summary>
    /// Base class
    /// </summary>
    public class UWClass
    {
        public const byte GAME_UWDEMO = 0;//"UW0";
        public const byte GAME_UW1 = 1; // "UW1";
        public const byte GAME_UW2 = 2; // "UW2";

        /// <summary>
        /// Use to track what game is currently active.
        /// </summary>
        public static byte _RES = 1;  //"UW1";

        public static string BasePath  ;    // = "C:\\Games\\UW2\\game\\UW2";

    }//end class
}//end namespace