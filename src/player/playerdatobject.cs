namespace Underworld
{
    public partial class playerdat : Loader
    {
        public static uwObject playerObject;

        static int PlayerObjectPTR
        {
            get
            {
                if (_RES==GAME_UW2)
                {
                    return 0x380;
                }
                else
                {
                    return 0xD5;
                }
            }
        }
        public static void InitPlayerObject()
        {
            
            playerObject = new uwObject
            {
                    //isInventory = false,
                    IsStatic = false,
                    index = 0,
                    PTR = PlayerObjectPTR,
                    DataBuffer = pdat
            };
        }
    }//end class
}//end namespace