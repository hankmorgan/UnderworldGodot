namespace Underworld
{
    /// <summary>
    /// Storing and retreval of info about automap tiles
    /// </summary>
    public class automaptileinfo
    {
        /// <summary>
        /// Location of data in the buffer
        /// </summary>
        public int PTR;

        /// <summary>
        /// Reference to the data buffer where the tile data is held
        /// </summary>
        public byte[] buffer;
        /// <summary>
        /// How the tile is rendered
        /// </summary>
        public short DisplayType 
        {
            get
            {
                return (short)((buffer[PTR] >> 4) & 0xf);
            }
        }

        /// <summary>
        /// The type of tile. When set means tile has been visited
        /// </summary>
        public short tileType 
        {
            get
            {
                return (short)(buffer[PTR] & 0xF);
            }
        }

        public automaptileinfo(int _ptr, ref byte[] _buffer)
        {
            PTR = _ptr;
            buffer = _buffer;
        }
    }//end class
}//end namespace