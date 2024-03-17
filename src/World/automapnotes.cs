using System.Collections.Generic;
using System.Diagnostics;

namespace Underworld
{
    public class automapnotes : Loader
    {
        public static automapnotes currentautomapnotes;

        /// <summary>
        /// Array of all cached automaps
        /// </summary>
        public static automapnotes[] automapsnote;


        //The raw data for this set of automap notes.
        public byte[] buffer;

        public List<mapnote> notes=new();

        static int GetBlockAddress(int blockno, byte[] buffer)
        {
            if (_RES==GAME_UW2)
            {
                return (int)getAt(buffer, 6 + (blockno * 4), 32);
            }
            else
            {
                return (int)getAt(buffer, (blockno * 4) + 2, 32);
            }
        }
        public automapnotes(int LevelNo, int gameNo)
        {
            int blockno;  
            int noOfPossibleBlocks; 
            int thisAddress;        
            int startblock;
            if (gameNo == UWClass.GAME_UW2)
            {
                blockno = 240 + LevelNo;
                noOfPossibleBlocks = 80;  
                startblock =240;              
            }
            else
            {
                blockno = LevelNo + 36;
                noOfPossibleBlocks = 9; 
                startblock = 36;              
            }
            thisAddress = GetBlockAddress(blockno, LevArkLoader.lev_ark_file_data);
            if (thisAddress==0)
            {
                //no data
                return;
            }
            var EOF = LevArkLoader.lev_ark_file_data.GetUpperBound(0)+1;//end of file is the max length.
            for (int i = 0; i<noOfPossibleBlocks; i++)
            {
                if (i != LevelNo)
                {
                    var addressToCheck = GetBlockAddress(startblock+i, LevArkLoader.lev_ark_file_data);
                    if (addressToCheck>thisAddress)
                    {//block is after the current one.
                        if (addressToCheck < EOF)
                        {
                            EOF = addressToCheck; //try and get the nearest next address to the current block
                        }
                    }
                }
            }
            
            
            
            if (DataLoader.LoadUWBlock(LevArkLoader.lev_ark_file_data, blockno, EOF-thisAddress, out UWBlock block))
            {
                var addptr = 0;
                int counter =0;
                var NoOfNotes = block.Data.GetUpperBound(0) / 54;
                while ((addptr<= block.Data.GetUpperBound(0)) && (counter<NoOfNotes))  
                {
                    //construct note
                    if (block.Data[addptr]!='\0')//if not null
                    {
                        var charptr =0;
                        var nextchar = (char)block.Data[addptr];
                        var fullstring = "";
                        while ((charptr<0x31) && (nextchar!='\0'))
                        {                            
                            fullstring+=(char)block.Data[addptr+charptr];
                            charptr++;
                            nextchar= (char)block.Data[addptr+charptr];
                        }
                        var _posX = (int)Loader.getAt(block.Data,addptr+0x32,16);
                        var _posY = (int)Loader.getAt(block.Data,addptr+0x34,16);
                        notes.Add (new mapnote(fullstring,_posX,_posY));
                    }
                    addptr+=54;
                    counter++;
                }                        
            }
        }    

        public class mapnote:UWClass
        {
            public string notetext;
            public int posX; 
            public int posY;

            public mapnote(string _notetext, int _posX, int _posY)
            {
                notetext = _notetext;
                posX = _posX;
                posY = _posY;
                Debug.Print($"{posX},{posY} {notetext}");
            }
        }
    }//end class
}//end namespace