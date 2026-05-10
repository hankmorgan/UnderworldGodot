namespace Underworld
{

    /// <summary>
    /// Handles the map pieces in UW2's tomb of praecor loth.
    /// </summary>
    public class a_bit_of_a_map : objectInstance
    {

        //Map pieces
        //index     link
        //925       520
        //931       528
        //964       516  
        //975       513
        //997       514
        //999       544
        public static bool Use(uwObject obj, bool WorldObject)
        {
            if (WorldObject)
            {
                return false;
            }
            var match = objectsearch.FindMatchInFullObjectList(4, 3, 0xA, playerdat.InventoryObjects); //search for player map
            {
                if (match == null)
                {
                    uimanager.AddToMessageScroll(GameStrings.GetString(1, 0xA));//you don't have your map
                }
                else
                {
                    var dungeon = 0x47 + obj.quality; //hidden maps after the end of normally accessible levels.
                    var mapPieceLink = obj.link & 0xFF;

                    if (mapPieceLink != 0)
                    {
                        obj.link = 512;
                        uimanager.AddToMessageScroll(GameStrings.GetString(1, 9));
                        CopyMapPiece(SourceMapNo: dungeon, TargetMapNo: obj.owner, SegmentToCopy: mapPieceLink);
                    }
                    else
                    {
                        //display the automap of the target level
                        var worldno = worlds.GetWorldNo(obj.owner);
                        uimanager.DrawAutoMap(obj.owner - 1, worldno);
                    }
                }
            }



            return true;
        }


        static void CopyMapPiece(int SourceMapNo, int TargetMapNo, int SegmentToCopy)
        {
            int expgain = 0;
            //if (automap.automaps[SourceMapNo - 1] == null)
            //{
            var sourcemap = new automap(SourceMapNo - 1);//this map is only temporarily loaded. the data is has to be removed as subsequent uses of map pieces will not work if the actual map data is changed.
            //}
            //var sourcemap = automap.automaps[SourceMapNo - 1];

            if (automap.automaps[TargetMapNo - 1] == null)
            {
                automap.automaps[TargetMapNo - 1] = new automap(TargetMapNo - 1);
            }
            var targetmap = automap.automaps[TargetMapNo - 1];

            var currentmap = sourcemap;
            var tempmap = targetmap;

            if ((targetmap != null) && (sourcemap != null))
            {
                var di = 0x3F;
                while (di >= 1)
                {
                    //ovr94_1666
                    var si = 1;

                    while (si <= 0x3F)
                    {
                        var TestSegmentNo = GetMapPieceSegmentNo(si - 32, di - 32); //find which eight the x/y value is in.

                        if (((SegmentToCopy >> TestSegmentNo) & 1) == 0)
                        {
                            //ovr094_168F
                            var index = (di << 6) + si;
                            var nibblevar9 = (byte)(currentmap.buffer[index] & 0xF);
                            if ((nibblevar9 >= 6) && (nibblevar9 <= 9))
                            {
                                nibblevar9 = 0xB;
                            }
                            else
                            {
                                if (nibblevar9 <= 5)
                                {
                                    nibblevar9 += 0xA;
                                }
                            }
                            currentmap.buffer[index] |= nibblevar9;
                        }

                        si++;
                    }
                    di--;
                }

                //ovr094_16D5
                tempmap = currentmap;
                currentmap = targetmap;

                di = 0x3F;
                while (di >= 1)
                {
                    var si = 1;
                    while (si < 0x3F)
                    {
                        var index = (di << 6) + si;
                        var src_nibblevar8 = tempmap.buffer[index] & 0xF;
                        var dst_nibblevar9 = currentmap.buffer[index] & 0xF;
                        var var7 = 0;
                        if (dst_nibblevar9 != 0)
                        {
                            //ovr94_172C
                            if (src_nibblevar8 < 0xA)
                            {
                                if (dst_nibblevar9 >= 0xA)
                                {
                                    var7 = 1;
                                }
                            }
                        }
                        else
                        {
                            var7 = 1;
                        }
                        //ovr094_173C
                        if (var7 != 0)
                        {
                            //ovr94_1742
                            expgain += 1 + (TargetMapNo / 8);

                            currentmap.buffer[index] = tempmap.buffer[index];
                        }
                        si++;
                    }

                    di--;
                }
            }

            //Apply exp gain
            playerdat.ChangeExperience(expgain / 0x14);

            var sourcenotes = new automapnote(SourceMapNo - 1);
            if (automapnote.automapsnotes[TargetMapNo - 1] == null)
            {
                automapnote.automapsnotes[TargetMapNo - 1] = new(TargetMapNo - 1);
            }
            var targetnotes = automapnote.automapsnotes[TargetMapNo - 1];

            //var si = 0;
            foreach (var srcNote in sourcenotes.notes)
            {                
                var NoteX_var14 = (srcNote.posX - 10) / 3;
                var NoteY_var16 = (srcNote.posY - 9) / 3;
                var var6 = GetMapPieceSegmentNo(NoteX_var14 - 32, NoteY_var16-32);
                if (((SegmentToCopy >> var6) & 0x1) != 0)
                {
                    if (targetnotes.NoOfNotes < 0x64)
                    {
                        // there is space to store the note.
                        targetnotes.notes.Add(
                            new(
                                _notetext: srcNote.notetext, 
                                _posX: srcNote.posX, 
                                _posY: srcNote.posY)
                            );
                    }
                }

            }





        }



        /// <summary>
        /// Calculates a bit field that indicates which map segment the specified x/y values (adjusted) are part off. 
        /// Note the x/y values are offset before function call.
        /// </summary>
        /// <param name="di"></param>
        /// <param name="cx"></param>
        /// <returns></returns>
        static int GetMapPieceSegmentNo(int arg0, int arg2)
        {
            var si = 0;
            var cx = arg0;
            var di = arg2;

            if ((di * di) + (cx * cx) < 0x51)
            {
                return 0;
            }
            else
            {
                if (di < 0)
                {
                    di = -di;
                    cx = -cx;
                    si = 0x1C;
                }

                if (cx < 0)
                {
                    var var2 = di;
                    di = -cx;
                    cx = var2;
                    si += 0xE;
                }

                if (di >= cx)
                {
                    si += 0xE - ((cx * 7) / di);
                }
                else
                {
                    si += (di * 7) / cx;
                }

                si = 1 + si / 8;

                if (si > 7)
                {
                    si = 7;
                }
                return si;
            }
        }
    }
}