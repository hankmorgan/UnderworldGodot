namespace Underworld
{
    public class storage_crystal : objectInstance
    {

        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (WorldObject)
            {
                return false;
            }
            else
            {
                var output = GameStrings.GetString(1, 352); //you read the crystal signature

                var di = (73 - obj.quality) << 1;
                for (int si = 0; si < 4; si++)
                {
                    if (si % 2 == 1)
                    {//1,3
                        var c = (char)(48 + di % 10);
                        output += c;
                    }
                    else
                    {//0,2
                        var c = (char)(65 + di % 26);
                        output += c;
                    }
                    di += (30 + obj.quality);
                }

                uimanager.AddToMessageScroll(output);
                return true;
            }
        }

    }//end class
}//end namespace