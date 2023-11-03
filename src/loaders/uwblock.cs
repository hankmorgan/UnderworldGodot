namespace Underworld
{
        /// <summary>
        /// UW block structure for .ark files.
        /// </summary>
        public class UWBlock :UWClass
        {
            public byte[] Data;
            public int Address; //The file address of the block

            public int DataLen;
            //UW2 specific
            public int CompressionFlag;
            public int ReservedSpace;
        };
}