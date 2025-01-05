using System.Diagnostics;

namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        /// <summary>
        /// Performs operations on an object position
        /// </summary>
        public static void x_obj_pos()
        {
            Debug.Print("Untested x_obj_pos");
            var X_var4 = at(stackptr - 3);
            var Y_var8 = at(stackptr - 2);
            var Z_var10 = at(stackptr - 1);
            var objindex = at(at(stackptr - 5));
            var mode = at(at(stackptr - 4));

            var obj = UWTileMap.current_tilemap.LevelObjects[objindex];

            switch (mode)
            {
                case 1:
                    {//Change x,y,z
                        if (X_var4 != -1)
                        {
                            obj.xpos = X_var4;
                        }
                        if (Y_var8 != -1)
                        {
                            obj.ypos = Y_var8;
                        }
                        if (Z_var10 != -1)
                        {
                            if (Z_var10 <= 0x7F)
                            {//use the supplied height

                            }
                            else
                            {//use the height of the tile given by X and Y
                                if (UWTileMap.ValidTile(X_var4, Y_var8))
                                {
                                    var tile = UWTileMap.current_tilemap.Tiles[X_var4, Y_var8];
                                    obj.zpos = (short)(tile.floorHeight << 3);
                                }
                            }
                        }
                        break;
                    }
                case 2:
                    {//Set xhome and yhome for mobile objects, stores value of zpos on stack
                        if (X_var4 != -1)
                        {
                            obj.npc_xhome = X_var4;
                        }
                        if (Y_var8 != -1)
                        {
                            obj.npc_yhome = Y_var8;
                        }
                        if (Z_var10 != -1)
                        {
                            Debug.Print("Store zos, untested");
                            Set(Z_var10, obj.zpos);
                        }
                        break;
                    }
                case 3:
                    {//store values.
                        if (X_var4 != -1)
                        {
                            Debug.Print("Store xpos, untested");
                            Set(X_var4, obj.xpos);
                        }
                        if (Y_var8 != -1)
                        {
                            Debug.Print("Store xpos, untested");
                            Set(Y_var8, obj.ypos);
                        }
                        if (Z_var10 != -1)
                        {
                            Debug.Print("Store zpos, untested");
                            Set(Z_var10, obj.zpos);
                        }
                        break;
                    }
            }
        }
    } //end class
}//end namespace