namespace Underworld
{
    public class AnimationOverlay:UWClass
    {
        public int PTR; //PTR to where the data is located in the file data.
        
        public int index;

        public AnimationOverlay(int _index)
        {
            index = _index;
            switch(_RES)
            {
                case GAME_UW2:
                    PTR = 0x7c06 + 2 + (_index * 6); break; // Located after end of tilemap data;
                default:
                    PTR = _index * 6; break;
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
            set
            {      
                var newvalue = (value & 0x3FF) << 6; 
                switch (_RES)
                {                    
                    case GAME_UW2: // data is at the end of the tilemap
                        {
                        var currentVal = (int)Loader.getAt(TileMap.current_tilemap.lev_ark_block.Data,PTR,16);
                        currentVal &= 0x3F; //mask out the link to 0.
                        newvalue = currentVal | newvalue;
                        Loader.setAt(TileMap.current_tilemap.lev_ark_block.Data,PTR,16, newvalue);
                        break;
                        }

                    default://but UW1 stores the data in it's own block
                    {
                        var currentVal = (int)Loader.getAt(TileMap.current_tilemap.ovl_ark_block.Data,PTR,16);
                        currentVal &= 0x3F; //mask out the link to 0.
                        newvalue = currentVal | newvalue;
                        Loader.setAt(TileMap.current_tilemap.ovl_ark_block.Data,PTR,16, newvalue);
                        break;    
                    }
                }
            }
        }



        // /// <summary>
        // /// Calling this owner to be consistant with game object naming.
        // /// </summary>
        // public int owner
        // {
        //     get
        //     {
        //         switch (_RES)
        //         {                    
        //             case GAME_UW2: // data is at the end of the tilemap
        //                 return (int)(Loader.getAt(TileMap.current_tilemap.lev_ark_block.Data,PTR,16) & 0x3f);
        //             default://but UW1 stores the data in it's own block
        //                 return (int)(Loader.getAt(TileMap.current_tilemap.ovl_ark_block.Data,PTR,16) & 0x3f);                  
        //         }
        //     }
        // }

        /// <summary>
        /// No of frames left to play. -1 means it goes forever. TODO: This info is enough to play the animo?
        /// </summary>
        public int Duration //starts at -1.
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
            set
            {
                switch (_RES)
                {                    
                    case GAME_UW2: // data is at the end of the tilemap
                        Loader.setAt(TileMap.current_tilemap.lev_ark_block.Data,PTR+2, 16, value); break;
                    default://but UW1 stores the data in it's own block
                       Loader.setAt(TileMap.current_tilemap.ovl_ark_block.Data,PTR+2, 16 , value); break;                 
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
            set
            {
                switch (_RES)
                {                    
                    case GAME_UW2: // data is at the end of the tilemap
                        TileMap.current_tilemap.lev_ark_block.Data[PTR+4] = (byte)(value & 0x3f);
                        break;
                    default://but UW1 stores the data in it's own block
                        TileMap.current_tilemap.ovl_ark_block.Data[PTR+4] = (byte)(value & 0x3f);  
                        break;                
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
            set
            {
                switch (_RES)
                {                    
                    case GAME_UW2: // data is at the end of the tilemap
                        TileMap.current_tilemap.lev_ark_block.Data[PTR+5] = (byte)(value & 0x3f);
                        break;
                    default://but UW1 stores the data in it's own block
                        TileMap.current_tilemap.ovl_ark_block.Data[PTR+5] = (byte)(value & 0x3f);  
                        break;                
                }
            }
        }



    /// <summary>
    /// Process the current animation overlays and advance them
    /// </summary>
    public static void UpdateAnimationOverlays()
	{
		foreach (var ovl in Underworld.TileMap.current_tilemap.Overlays)
		{
			if (ovl != null)
			{
				if (ovl.link != 0)
				{
					if (ovl.Duration != 0)
					{
						var obj = Underworld.TileMap.current_tilemap.LevelObjects[ovl.link];
						if (obj != null)
						{
							if (obj.majorclass == 7) //animo
							{
								if (obj.classindex != 0xF)
								{//animated sprite
									if (obj.owner < animationObjectDat.endFrame(obj.item_id))
									{ //animation in progress
										animo.AdvanceAnimo((animo)obj.instance);
									}
									else
									{
										if (ovl.Duration == 0xFFFF)
										{//infinitely loop
											animo.ResetAnimo((animo)obj.instance);
										}
										else
										{
											ovl.Duration = 0;
											//TODO Destroy the animo
										}
									}
								}
								else
								{//a moving door
									ovl.Duration--;
									door.MoveDoor((door)obj.instance, 1);
								}
							}
						}
					}
				}
			}
		}
	}
    
    }//end class
}//end namespace