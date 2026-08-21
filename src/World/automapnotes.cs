using System.Collections.Generic;
using System.Diagnostics;

namespace Underworld
{
    public class automapnote : Loader
    {
        // public static automapnotes currentautomapnotes;

        /// <summary>
        /// Array of all cached automaps
        /// </summary>
        public static automapnote[] automapsnotes;

        public int NoOfNotes
        {
            get
            {
                if (notes == null)
                {
                    return 0;
                }
                else
                {
                    return notes.Count;
                }
            }
        }

        //The raw data for this set of automap notes.
        public byte[] buffer;

        public List<mapnotetext> notes = new();

        static int GetBlockAddress(int blockno, byte[] buffer)
        {
            if (_RES == GAME_UW2)
            {
                return (int)getAt(buffer, 6 + (blockno * 4), 32);
            }
            else
            {
                return (int)getAt(buffer, (blockno * 4) + 2, 32);
            }
        }
        public automapnote(int LevelNo)
        {
            if (_RES == GAME_UWDEMO)
            {
                buffer = new byte[0];
            }
            else
            {
                int blockno;
                int noOfPossibleBlocks;
                int thisAddress;
                int startblock;
                if (_RES == GAME_UW2)
                {
                    blockno = 240 + LevelNo;
                    noOfPossibleBlocks = 80;
                    startblock = 240;
                }
                else
                {
                    blockno = LevelNo + 36;
                    noOfPossibleBlocks = 9;
                    startblock = 36;
                }
                thisAddress = GetBlockAddress(blockno, LevArkLoader.lev_ark_file_data);
                if (thisAddress == 0)
                {
                    //no data
                    return;
                }
                var EOF = LevArkLoader.lev_ark_file_data.GetUpperBound(0) + 1;//end of file is the max length.
                for (int i = 0; i < noOfPossibleBlocks; i++)
                {
                    if (i != LevelNo)
                    {
                        var addressToCheck = GetBlockAddress(startblock + i, LevArkLoader.lev_ark_file_data);
                        if (addressToCheck > thisAddress)
                        {//block is after the current one.
                            if (addressToCheck < EOF)
                            {
                                EOF = addressToCheck; //try and get the nearest next address to the current block
                            }
                        }
                    }
                }



                if (DataLoader.LoadUWBlock(LevArkLoader.lev_ark_file_data, blockno, EOF - thisAddress, out UWBlock block))
                {
                    var addptr = 0;
                    int counter = 0;
                    var NoOfNotes = block.Data.Length / 54;
                    while ((addptr <= block.Data.GetUpperBound(0)) && (counter < NoOfNotes))
                    {
                        //construct note
                        if (block.Data[addptr] != '\0')//if not null
                        {
                            var charptr = 0;
                            var nextchar = (char)block.Data[addptr];
                            var fullstring = "";
                            while ((charptr < 0x31) && (nextchar != '\0'))
                            {
                                fullstring += (char)block.Data[addptr + charptr];
                                charptr++;
                                nextchar = (char)block.Data[addptr + charptr];
                            }
                            if (nextchar == '\0')
                            {
                                var _posX = (int)getAt(block.Data, addptr + 0x32, 16);
                                var _posY = (int)getAt(block.Data, addptr + 0x34, 16);
                                notes.Add(new mapnotetext(fullstring, _posX, _posY));
                            }
                            //Debug.Print(fullstring);
                        }
                        addptr += 54;
                        counter++;
                    }
                }
            }
        }

        /// <summary>
        /// Parameterless constructor for tests and for writer flows that populate
        /// the notes list independently of lev.ark load.
        /// </summary>
        public automapnote() { }

        /// <summary>
        /// Serialises the notes list back to the 54-byte-per-record block layout
        /// used in lev.ark map-notes blocks (UW1 blocks 36..44, UW2 blocks 240..319).
        /// Each record: zero-terminated string from offset 0 (max 0x31 bytes),
        /// Int16 posX at offset 0x32, Int16 posY at offset 0x34.
        /// Returns an empty byte array when there are no notes.
        /// </summary>
        /// <summary>
        /// The longest note DOS will accept. Its entry loop stops inserting once the
        /// index passes 0x2D, so 46 characters is the maximum.
        /// </summary>
        public const int MaxNoteLength = 46;

        /// <summary>
        /// Reduces a note to what DOS can store and draw. Both games accept only
        /// 0x20 to 0x7A on entry and upper-case as they go, so a note holding anything
        /// else was never reachable in the original. A lower-case note hangs UW1 when
        /// the automap is opened.
        /// </summary>
        public static string NormaliseNoteText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            var sb = new System.Text.StringBuilder(text.Length);
            foreach (char c in text)
            {
                if (c < 0x20 || c > 0x7A) continue;
                sb.Append(c >= 'a' && c <= 'z' ? (char)(c - 0x20) : c);
                if (sb.Length == MaxNoteLength) break;
            }
            return sb.ToString();
        }

        public byte[] Serialize()
        {
            if (notes == null || notes.Count == 0) return System.Array.Empty<byte>();

            byte[] output = new byte[notes.Count * 54];
            for (int i = 0; i < notes.Count; i++)
            {
                int recordStart = i * 54;
                var n = notes[i];
                string text = NormaliseNoteText(n.notetext);

                int copyLen = System.Math.Min(text.Length, 0x31);
                for (int c = 0; c < copyLen; c++)
                {
                    output[recordStart + c] = (byte)text[c];
                }

                output[recordStart + 0x32] = (byte)(n.posX & 0xFF);
                output[recordStart + 0x33] = (byte)((n.posX >> 8) & 0xFF);
                output[recordStart + 0x34] = (byte)(n.posY & 0xFF);
                output[recordStart + 0x35] = (byte)((n.posY >> 8) & 0xFF);
            }
            return output;
        }

        public class mapnotetext : UWClass
        {
            public string notetext;
            public int posX;
            public int posY;
            public RichTextLabelMapNote textlabel;//reference to the label created by this note.

            public mapnotetext(string _notetext, int _posX, int _posY)
            {
                notetext = _notetext;
                posX = _posX;
                posY = _posY;
                //Debug.Print($"{posX},{posY} {notetext}");
            }
        }
    }//end class
}//end namespace