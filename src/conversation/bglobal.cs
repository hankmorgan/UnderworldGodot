using System.IO;
namespace Underworld
{
    /// <summary>
    /// For loading,saving and accessing the bglobal.dat and BABGLOBS.DAT files used in conversations 
    /// </summary>
    public class bglobal : UWClass
    {
        public struct BablGlobal
        {
            public short ConversationNo;
            public short Size;
            public short[] Globals;
        };

        /// <summary>
        /// Conversation Global data
        /// </summary>
        public static BablGlobal[] bGlobals;

        public static void LoadGlobals(string datafolder)
        {
            byte[] bglob_data;
            if (datafolder.ToUpper() == "DATA") //loading from DATA
            {//Init from BABGLOBS.DAT. Initialise the data.
                if (Loader.ReadStreamFile(Path.Combine(Loader.BasePath, "DATA", "BABGLOBS.DAT"), out bglob_data))
                {
                    int NoOfSlots = bglob_data.GetUpperBound(0) / 4;
                    int add_ptr = 0;
                    bGlobals = new BablGlobal[NoOfSlots + 1];
                    for (int i = 0; i <= NoOfSlots; i++)
                    {
                        bGlobals[i].ConversationNo = (short)Loader.getAt(bglob_data, add_ptr, 16);
                        bGlobals[i].Size = (short)Loader.getAt(bglob_data, add_ptr + 2, 16);
                        bGlobals[i].Globals = new short[bGlobals[i].Size];
                        add_ptr += 4;
                    }
                }
            }
            else
            {
                int NoOfSlots = 0;//Assumes the same no of slots that is in the babglobs is in bglobals.
                if (Loader.ReadStreamFile(Path.Combine(Loader.BasePath, "DATA", "BABGLOBS.DAT"), out bglob_data))
                {
                    NoOfSlots = bglob_data.GetUpperBound(0) / 4;
                    NoOfSlots++;
                }
                if (Loader.ReadStreamFile(Path.Combine(Loader.BasePath, datafolder, "BGLOBALS.DAT"), out bglob_data))
                {
                    int add_ptr = 0;
                    bGlobals = new BablGlobal[NoOfSlots];
                    for (int i = 0; i < NoOfSlots; i++)
                    {

                        bGlobals[i].ConversationNo = (short)Loader.getAt(bglob_data, add_ptr, 16);
                        bGlobals[i].Size = (short)Loader.getAt(bglob_data, add_ptr + 2, 16);
                        bGlobals[i].Globals = new short[bGlobals[i].Size];
                        add_ptr += 4;
                        for (int g = 0; g < bGlobals[i].Size; g++)
                        {
                            bGlobals[i].Globals[g] = (short)Loader.getAt(bglob_data, add_ptr, 16);
                            // if (bGlobals[i].Globals[g] == 65535)
                            // {
                            //     bGlobals[i].Globals[g] = 0;
                            // }
                            add_ptr += 2;
                        }
                    }
                }
            }
        }

    } //end class
}//end namespace