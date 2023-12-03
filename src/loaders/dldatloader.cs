namespace Underworld
{
    public class DlDat:Loader
    {
        private static int[]ambientlights= new int[80];

        public static int GetAmbientLight(int levelno)
        {
            if (_RES==GAME_UW2)
            {
                return ambientlights[levelno];
            }
            return 0;
        }
        static  DlDat()
        {
            var path = System.IO.Path.Combine(BasePath,"DATA","DL.DAT");
            if (System.IO.File.Exists(path))
            {
                var dl = System.IO.File.ReadAllBytes(path);
                for (int i=0;i<80;i++)
                {
                    ambientlights[i]=(int)dl[i];
                }
            }
        }
         
    }//end class
}//end namespace