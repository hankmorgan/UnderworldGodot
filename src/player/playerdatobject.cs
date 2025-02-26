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
        {//todo move this object into the tilemap objects per vanilla behaviour.            
            playerObject = new uwObject
            {
                    //isInventory = false,
                    IsStatic = false,
                    index = 1,
                    PTR = PlayerObjectPTR,
                    DataBuffer = pdat
            };
        }
    }//end class
}//end namespace