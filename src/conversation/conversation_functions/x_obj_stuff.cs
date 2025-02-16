namespace Underworld
{
    public partial class ConversationVM:UWClass
    {
        /// <summary>
        /// Performs arbitary operations on an object
        /// </summary>
        public static void x_obj_stuff()
        {
            var identifyObject = at(at(stackptr-7));
            var owner = at(at(stackptr-6));
            var flags = at(at(stackptr-5));
            var link = at(at(stackptr-4));
            var flag1 = at(at(stackptr-3));
            var flag0 = at(at(stackptr-2));
            var quality = at(at(stackptr-1));
            var objindex = at(at(stackptr-9));
            var mode = at(at(stackptr-8));

            var obj = UWTileMap.current_tilemap.LevelObjects[objindex];
            if (obj!=null)
            {
                if (mode==0)
                {//retrieve values            
                    //Get identification status
                    if (identifyObject != -1)
                    {
                        if (look.CanBeIdentified(obj))
                        {
                            Set(at(stackptr-7), obj.heading); //update variable.
                        }
                    }
                    if (owner != -1)
                    {
                        Set(at(stackptr-6), obj.owner);
                    }
                    if (flags != -1)
                    {
                        Set(at(stackptr-5), obj.flags_full);
                    }
                    if (link!=-1)
                    {
                        if (_RES==GAME_UW2)
                        {
                            Set(at(stackptr-4),obj.link);
                        }
                        else
                        {
                            Set(at(stackptr-4),obj.link & 0x1FF);
                        }                       
                    }
                    if (flag1!=-1)
                    {
                        Set(at(stackptr-3), obj.flags1);
                    }
                    if (flag0!=-1)
                    {
                        Set(at(stackptr-2), obj.flags0);
                    }
                    if (quality!=-1)
                    {
                        Set(at(stackptr-1), obj.quality);
                    }
                }
                else
                { //mode <> 0. set values
                    if (identifyObject != -1)
                    {
                        if (look.CanBeIdentified(obj))
                        {
                            obj.heading = (ushort)identifyObject;
                        }
                    }
                    if (owner!=-1)
                    {
                        obj.owner = owner;
                    }
                    if (flags!=-1)
                    {
                        obj.flags_full = flags;
                    }
                    if (link!=-1)
                    {
                        if (_RES==GAME_UW2)
                        {
                            obj.link = link;
                        }
                        else
                        {
                            obj.link = (short)(link | 0x200);
                        }                        
                    }
                    if (flag1!=-1)
                    {
                        obj.flags1 = flag1;
                    }
                    if (flag0!=-1)
                    {
                        obj.flags0 = flag0;
                    }
                    if (quality!=-1)
                    {
                        obj.quality = quality;
                    }
                }
            }
            else
            {
                result_register = 0;
            }
        }
    } //end class
}//end namespace