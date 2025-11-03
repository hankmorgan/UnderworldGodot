using System;
using Godot;

namespace Underworld
{
    public class AnimationOverlay : UWClass
    {

        public static int NoOfAnimationOverlays;
        public int PTR; //PTR to where the data is located in the file data.

        public int index;

        public AnimationOverlay(int _index)
        {
            index = _index;
            switch (_RES)
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
                        return (int)((Loader.getAt(UWTileMap.current_tilemap.lev_ark_block.Data, PTR, 16) >> 6) & 0x3ff);
                    default://but UW1 stores the data in it's own block
                        return (int)((Loader.getAt(UWTileMap.current_tilemap.ovl_ark_block.Data, PTR, 16) >> 6) & 0x3ff);
                }
            }
            set
            {
                var newvalue = (value & 0x3FF) << 6;
                switch (_RES)
                {
                    case GAME_UW2: // data is at the end of the tilemap
                        {
                            var currentVal = (int)Loader.getAt(UWTileMap.current_tilemap.lev_ark_block.Data, PTR, 16);
                            currentVal &= 0x3F; //mask out the link to 0.
                            newvalue = currentVal | newvalue;
                            Loader.setAt(UWTileMap.current_tilemap.lev_ark_block.Data, PTR, 16, newvalue);
                            break;
                        }

                    default://but UW1 stores the data in it's own block
                        {
                            var currentVal = (int)Loader.getAt(UWTileMap.current_tilemap.ovl_ark_block.Data, PTR, 16);
                            currentVal &= 0x3F; //mask out the link to 0.
                            newvalue = currentVal | newvalue;
                            Loader.setAt(UWTileMap.current_tilemap.ovl_ark_block.Data, PTR, 16, newvalue);
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
                        return (int)Loader.getAt(UWTileMap.current_tilemap.lev_ark_block.Data, PTR + 2, 16);
                    default://but UW1 stores the data in it's own block
                        return (int)Loader.getAt(UWTileMap.current_tilemap.ovl_ark_block.Data, PTR + 2, 16);
                }
            }
            set
            {
                switch (_RES)
                {
                    case GAME_UW2: // data is at the end of the tilemap
                        Loader.setAt(UWTileMap.current_tilemap.lev_ark_block.Data, PTR + 2, 16, value); break;
                    default://but UW1 stores the data in it's own block
                        Loader.setAt(UWTileMap.current_tilemap.ovl_ark_block.Data, PTR + 2, 16, value); break;
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
                        return UWTileMap.current_tilemap.lev_ark_block.Data[PTR + 4] & 0x3f;
                    default://but UW1 stores the data in it's own block
                        return UWTileMap.current_tilemap.ovl_ark_block.Data[PTR + 4] & 0x3f;
                }
            }
            set
            {
                switch (_RES)
                {
                    case GAME_UW2: // data is at the end of the tilemap
                        UWTileMap.current_tilemap.lev_ark_block.Data[PTR + 4] = (byte)(value & 0x3f);
                        break;
                    default://but UW1 stores the data in it's own block
                        UWTileMap.current_tilemap.ovl_ark_block.Data[PTR + 4] = (byte)(value & 0x3f);
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
                        return UWTileMap.current_tilemap.lev_ark_block.Data[PTR + 5] & 0x3f;
                    default://but UW1 stores the data in it's own block
                        return UWTileMap.current_tilemap.ovl_ark_block.Data[PTR + 5] & 0x3f;
                }
            }
            set
            {
                switch (_RES)
                {
                    case GAME_UW2: // data is at the end of the tilemap
                        UWTileMap.current_tilemap.lev_ark_block.Data[PTR + 5] = (byte)(value & 0x3f);
                        break;
                    default://but UW1 stores the data in it's own block
                        UWTileMap.current_tilemap.ovl_ark_block.Data[PTR + 5] = (byte)(value & 0x3f);
                        break;
                }
            }
        }



        /// <summary>
        /// Process the current animation overlays and advance them
        /// </summary>
        public static void UpdateAnimationOverlays()
        {
            for (int i = 0; i < NoOfAnimationOverlays; i++)
            //{
            //foreach (var ovl in UWTileMap.current_tilemap.Overlays)
            {
                var ovl = UWTileMap.current_tilemap.Overlays[i];
                if (ovl != null)
                {
                    if (ovl.link != 0)
                    {
                        if (ovl.Duration != 0)
                        {
                            var obj = UWTileMap.current_tilemap.LevelObjects[ovl.link];
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
                                                var linktoremove = ovl.link;
                                                RemoveAnimationOverlay(ovl.link);
                                                ObjectRemover_OLD.DeleteObjectFromTile_DEPRECIATED(
                                                    tileX: ovl.tileX,
                                                    tileY: ovl.tileY,
                                                    indexToDelete: (short)linktoremove);
                                                //EndOverlay_DEPRECIATED(ovl);
                                            }
                                        }
                                    }
                                    else
                                    {//a moving door
                                        ovl.Duration--;
                                        door.MoveDoor(obj, 1);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds the overlay that links to the specified world object.
        /// </summary>
        /// <param name="linkToFind"></param>
        /// <returns></returns>
        public static AnimationOverlay FindOverlay(int linkToFind)
        {
            foreach (var ovl in UWTileMap.current_tilemap.Overlays)
            {
                if (ovl != null)
                {
                    if (ovl.link == linkToFind)
                    {
                        return ovl;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Ends a running overlay and destroys it's world instance.
        /// </summary>
        /// <param name="ovl"></param>
        public static void EndOverlay_DEPRECIATED(AnimationOverlay ovl)
        {
            ovl.Duration = 0;
            ObjectRemover_OLD.DeleteObjectFromTile_DEPRECIATED(
                tileX: ovl.tileX,
                tileY: ovl.tileY,
                indexToDelete: (short)ovl.link);
            ovl.link = 0;
            ovl.tileX = 0;
            ovl.tileY = 0;
        }


        /// <summary>
        /// Removes animation overlays by the vanilla method which is to move the overlay data in the last overlay to replace the overlay data to be removed.
        /// </summary>
        /// <param name="overlaylink"></param>
        public static void RemoveAnimationOverlay(int overlaylink)
        {
            for (int i = 0; i < NoOfAnimationOverlays; i++)
            {
                if (UWTileMap.current_tilemap.Overlays[i].link == overlaylink)
                {
                    var toRemove = UWTileMap.current_tilemap.Overlays[i];

                    //copy the raw data of the last overlay over the current overlay
                    var lastOverlay = UWTileMap.current_tilemap.Overlays[NoOfAnimationOverlays-1];
                    if (lastOverlay.index != i)
                    {
                        //copy the last overlay over the overlay to remove if it is a different overlay.
                        Buffer.BlockCopy(src: UWTileMap.current_tilemap.lev_ark_block.Data, srcOffset: lastOverlay.PTR, dst: UWTileMap.current_tilemap.lev_ark_block.Data, dstOffset: toRemove.PTR, count: 6);
                    }
                    //clear the data of the last overlay.                    
                    for (int b = 0; b < 6; b++)
                    {
                        UWTileMap.current_tilemap.lev_ark_block.Data[lastOverlay.PTR + b] = 0;
                    }
                    UWTileMap.current_tilemap.Overlays[NoOfAnimationOverlays-1] = null;
                    NoOfAnimationOverlays--;
                    return;
                }
            }
        }
    }//end class
}//end namespace