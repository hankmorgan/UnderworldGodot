namespace Underworld
{
    public class AnimationOverlay:UWClass
    {
        public int PTR; //PTR to where the data is located in the file data.
        
        public AnimationOverlay(int index)
        {
            switch(_RES)
            {
                case GAME_UW2:
                    PTR = 0x7c06 + 2 + (index * 6); break; // Located after end of tilemap data;
                default:
                    PTR = index * 6; break;
            }        
        }

        /// <summary>
        /// Link to the object being animated in the  object list
        /// </summary>
        public int link
        {
            get
            {
                switch (_RES)
                {                    
                    case GAME_UW2: // data is at the end of the tilemap
                        return (int)((Loader.getAt(TileMap.current_tilemap.lev_ark_block.Data,PTR,16) >> 6) & 0x3ff);
                    default://but UW1 stores the data in it's own block
                        return (int)((Loader.getAt(TileMap.current_tilemap.ovl_ark_block.Data,PTR,16) >> 6) & 0x3ff);       
                }
            }
        }



        /// <summary>
        /// Calling this owner to be consistant with game object naming.
        /// </summary>
        public int owner
        {
            get
            {
                switch (_RES)
                {                    
                    case GAME_UW2: // data is at the end of the tilemap
                        return (int)(Loader.getAt(TileMap.current_tilemap.lev_ark_block.Data,PTR,16) & 0x3f);
                    default://but UW1 stores the data in it's own block
                        return (int)(Loader.getAt(TileMap.current_tilemap.ovl_ark_block.Data,PTR,16) & 0x3f);                  
                }
            }
        }

        public int maybeDuration //starts at -1.
        {
            get
            {
                switch (_RES)
                {                    
                    case GAME_UW2: // data is at the end of the tilemap
                        return (int)Loader.getAt(TileMap.current_tilemap.lev_ark_block.Data,PTR+2,16);
                    default://but UW1 stores the data in it's own block
                        return (int)Loader.getAt(TileMap.current_tilemap.ovl_ark_block.Data,PTR+2,16);                  
                }
            }
        }

        /// <summary>
        /// Tile X where the linked object is located.
        /// </summary>
        public int tileX
        {
            get
            {
                switch (_RES)
                {                    
                    case GAME_UW2: // data is at the end of the tilemap
                        return TileMap.current_tilemap.lev_ark_block.Data[PTR+4]  & 0x3f;
                    default://but UW1 stores the data in it's own block
                        return TileMap.current_tilemap.ovl_ark_block.Data[PTR+4] & 0x3f;                  
                }
            }
        }

        /// <summary>
        /// Tile Y where the linked object is located.
        /// </summary>
        public int tileY
        {
            get
            {
                switch (_RES)
                {                    
                    case GAME_UW2: // data is at the end of the tilemap
                        return TileMap.current_tilemap.lev_ark_block.Data[PTR+5]  & 0x3f;
                    default://but UW1 stores the data in it's own block
                        return TileMap.current_tilemap.ovl_ark_block.Data[PTR+5] & 0x3f;                  
                }
            }
        }
    
    }//end class
}//end namespace