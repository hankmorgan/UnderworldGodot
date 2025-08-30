using System;
using System.Diagnostics;
using Godot;
namespace Underworld
{
    public class TileInfo : Loader
    {
        /// <summary>
        /// Flags that a tile needs to be redrawn on the next frame
        /// </summary>
        public bool Redraw;

        /// <summary>
        /// PTR to the file data in the UW Block for this tile.
        /// </summary>
        public long Ptr
        {
            get
            {
                return tileX * 4 + tileY * 256;
            }
        }

        /// <summary>
        /// Reference to the tilemap containing this tile
        /// </summary>
        public UWTileMap map;

        /// <summary>
        /// What type of tile this is
        /// </summary>
        public short tileType
        {
            get
            {
                return (short)(map.lev_ark_block.Data[Ptr] & 0x0F);
            }
            set
            {
                int val = map.lev_ark_block.Data[Ptr];
                val &= 0xF0;
                val |= (value & 0xF);
                map.lev_ark_block.Data[Ptr] = (byte)val;
            }
        }

        /// <summary>
        /// How high is the floor.
        /// </summary>
        public short floorHeight
        {
            get
            {
                return (short)((map.lev_ark_block.Data[Ptr] & 0xF0) >> 4);
            }
            set
            {
                int val = map.lev_ark_block.Data[Ptr];
                val &= 0x0F;
                val |= ((value) & 0xF) << 4;
                map.lev_ark_block.Data[Ptr] = (byte)val;
            }
        }

        /// <summary>
        /// How low is the ceiling
        /// </summary>
        /// Constant in UW. Variable in shock
        public short ceilingHeight;

        //Index into the texture map table for the actual floor texture of this tile.
        public short floorTexture
        {
            get
            {
                return (short)((map.lev_ark_block.Data[Ptr + 1] >> 2) & 0x0F);
                //return _floorTexture;
            }
            set
            {
                int val = map.lev_ark_block.Data[Ptr + 1];
                val &= 0xC3;
                val |= ((value & 0xF) << 2);
                map.lev_ark_block.Data[Ptr + 1] = (byte)val;
            }
        }

        /// <summary>
        /// Index into texture map for the wall texture presented to other tiles by this tile.
        /// </summary>
        public short wallTexture
        {
            get
            {
                return (short)(map.lev_ark_block.Data[Ptr + 2] & 0x3F);
            }
            set
            {
                int val = map.lev_ark_block.Data[Ptr + 2];
                val &= 0xC0;
                val |= ((value & 0x3F));
                map.lev_ark_block.Data[Ptr + 2] = (byte)val;
            }
        }

        /// <summary>
        /// /Points to a linked list of objects in the objects block
        /// </summary>
        public short indexObjectList
        {
            get
            {
                return (short)(getAt(map.lev_ark_block.Data, Ptr + 2, 16) >> 6);
            }
            set
            {
                int val = ((value & 0x3FF) << 6) | (wallTexture & 0x3F);
                map.lev_ark_block.Data[Ptr + 2] = (byte)(val & 0xFF);
                map.lev_ark_block.Data[Ptr + 3] = (byte)((val >> 8) & 0xFF);
            }
        }

        /// <summary>
        /// Does this tile contain a door per uw-formats.txt
        /// </summary>    /
        public short doorBit
        {
            get
            {
                return (short)((map.lev_ark_block.Data[Ptr + 1] >> 7) & 0x01);
            }
            set
            {
                int val = map.lev_ark_block.Data[Ptr + 1];
                val &= 0x7F;
                val |= ((value & 0x1) << 7);
                map.lev_ark_block.Data[Ptr + 1] = (byte)val;
            }
        }
        /// <summary>
        /// Set when ever a tile does contain a door regardless of the door bit above.
        /// </summary>
        public bool HasDoor
        {
            get
            {
                return DoorIndex != 0;
            }
        }
        /// <summary>
        /// Index of the door at this tile.
        /// </summary>
        public int DoorIndex;

        /// <summary>
        /// If set then we output this tile. Is off when it is a subpart of a group or is hidden from sight.
        /// </summary>
        public bool Render = true;

        /// <summary>
        ///  The dimensions on the x-axis of this tile. 1 for a regular tile.
        /// </summary>
        public short DimX = 1;
        /// <summary>
        /// The dimensions on the y-axis of this tile. 1 for a regular tile.
        /// </summary>
        public short DimY = 1;
        /// <summary>
        /// indicates the tile is a child of a group pareted by a tile of DimX>1 or DimY>1
        /// </summary>
        public bool Grouped = false;
        /// <summary>
        /// Which faces are visible on a tile. Used to reduce mesh complexity.
        /// </summary>
        public bool[] VisibleFaces = { true, true, true, true, true, true };
        /// <summary>
        /// The texture to display on the north face of the tile
        /// </summary>
        /// Based on the actual wall texture value of the tile located in that direction 
        public short North;
        /// <summary>
        /// The texture to display on the south face of the tile
        /// </summary>
        /// Based on the actual wall texture value of the tile located in that direction 
        public short South;
        /// <summary>
        /// The texture to display on the east face of the tile
        /// </summary>
        /// Based on the actual wall texture value of the tile located in that direction 
        public short East;
        /// <summary>
        /// The texture to display on the west face of the tile
        /// </summary>
        /// Based on the actual wall texture value of the tile located in that direction 
        public short West;

        /// <summary>
        /// Is the terrain land?
        /// </summary>
        public bool isLand
        {
            get
            {
                return !((isWater) || (isLava) || (isNothing));
            }
        }
        public bool isStair;

        /// <summary>
        /// Checks if the tile is water.
        /// </summary>
        public bool isWater
        {
            get
            {
                return UWTileMap.isTerrainWater(terrain);
            }
        }
        /// <summary>
        /// Checks if the tile is icy
        /// </summary>
        public bool isIce
        {
            get
            {
                return UWTileMap.isTerrainIce(terrain);
            }
        }

        /// <summary>
        /// Check if the tile on on lava
        /// </summary>
        public bool isLava
        {
            get
            {
                return UWTileMap.isTerrainLava(terrain);
            }
        }
        /// <summary>
        /// Set when the tile contains a bridge.
        /// </summary>
        public bool hasBridge;
        /// <summary>
        /// Set when the tile has the nothing textures
        /// </summary>
        public bool isNothing;
        /// <summary>
        /// Index to the contigous room area that the tile is part of.
        /// </summary>
        /// Used for AI decision making
        public short roomRegion;
        /// <summary>
        /// The x position of this tile
        /// </summary>
        public short tileX;
        /// <summary>
        /// The y position of this tile.
        /// </summary>
        public short tileY;


        /// <summary>
        /// Index of trigger to fire when entering this tile
        /// </summary>
        //public int EnterTrigger =0;

        /// <summary>
        /// Index of trigger to fire when exiting this tile
        /// </summary>
        //public int ExitTrigger =0;

        /// <summary>
        /// UW Tile flags - Unknown purpose
        /// </summary>
        public short flags
        {
            get
            {
                return (short)(map.lev_ark_block.Data[Ptr + 1] & 0x03);
            }
            set
            {
                int val = map.lev_ark_block.Data[Ptr + 1];
                val &= 0xFC;
                val |= (value & 0x3);
                map.lev_ark_block.Data[Ptr + 1] = (byte)val;
            }
        }

        public short lightFlag
        {
            get
            {
                return (short)(map.lev_ark_block.Data[Ptr + 1] & 0x1);
            }
        }


        /// <summary>
        /// Used to determine if NPC/character can use a magic attack from this tile
        /// </summary>
        public short noMagic
        {
            get
            {
                return (short)((map.lev_ark_block.Data[Ptr + 1] >> 6) & 0x01);
            }
            set
            {
                int val = map.lev_ark_block.Data[Ptr + 1];
                val &= 0xBF;
                val |= ((value & 0x1) << 6);
                map.lev_ark_block.Data[Ptr + 1] = (byte)val;
            }
        }

        //Shock Specific Stuff
        //public short shockSlopeFlag = TileMap.SLOPE_FLOOR_ONLY;    //For controlling ceiling slopes for shock.
        //public short shockCeilingTexture;

        //public short _shockTileSlopeSteepness;
        public short TileSlopeSteepness
        {
            get
            {
                switch (_RES)
                {
                    default:
                        if (tileType >= 2)
                        {
                            return 2;
                        }
                        else
                        {
                            return 0;
                        }
                }
            }
            set
            {
                switch (_RES)
                {
                    default:
                        //do nothing read only.
                        break;
                }
            }
        }

        /// <summary>
        /// Indicates that the tile can change into another type of tile or moves in someway. Eg because change terrain trap.
        /// </summary>
        /// Used to ensure this tile is rendered as a single tile.
        public bool TerrainChange;  //

        public int terrain
        {
            get
            {
                //Set the terrain type for the tile when the texture changes
                switch (_RES)
                {
                    case GAME_UWDEMO:
                    case GAME_UW1:
                        return TerrainDatLoader.Terrain[46 + map.texture_map[floorTexture + 48]];
                    case GAME_UW2:
                        return TerrainDatLoader.Terrain[map.texture_map[floorTexture]];
                    default:
                        return 0;
                }
            }
        }

        public string DescriptionFloor
        {
            get
            {
                return TextureName(floorTexture, true);
            }
        }

        public string DescriptionWall
        {
            get
            {
                return TextureName(wallTexture, false);
            }
        }

        public string DescriptionNorth
        {
            get
            {
                return TextureName(North, false);
            }
        }

        public string DescriptionSouth
        {
            get
            {
                return TextureName(South, false);
            }
        }

        public string DescriptionEast
        {
            get
            {
                return TextureName(East, false);
            }
        }

        public string DescriptionWest
        {
            get
            {
                return TextureName(West, false);
            }
        }


        /// <summary>
        /// Gets the texture name for the specified index (into texturemap) based on the surface type
        /// </summary>
        /// <param name="index"></param>
        /// <param name="floor"></param>
        /// <returns></returns>
        static string TextureName(int index, bool floor = true)
        {
            int offset = 0;
            if ((_RES != GAME_UW2) && (floor)) { offset = 48; }
            var textureNo = UWTileMap.current_tilemap.texture_map[index + offset];
            if (_RES == GAME_UW2)
            {
                if (floor)
                {
                    return GameStrings.GetString(10, 510 - textureNo);
                }
                else
                {
                    return GameStrings.GetString(10, textureNo);
                }
            }
            else
            {
                if (textureNo < 210)
                {//Return a wall texture.
                    return GameStrings.GetString(10, textureNo);
                }
                else
                {//return a floor texture in reverse order.
                    return GameStrings.GetString(10, 510 - textureNo + 210);
                }
            }
        }

        public static string GetTileSurfaceDescription(Vector3 normal, int tileX, int tileY)
        {
            var t = UWTileMap.current_tilemap.Tiles[tileX, tileY];
            if (t == null)
            {
                return "";
            }
            //look at tile
            //uimanager.AddToMessageScroll($"{tileX},{tileY}");
            //parse the normal into a tile surface.
            if (normal.Y > 0)
            {
                //this is a floor
                return t.DescriptionFloor;
            }
            else
            {
                if (normal == Vector3.Forward)
                {
                    return t.DescriptionSouth;
                }
                if (normal == Vector3.Back)
                {
                    return t.DescriptionNorth;
                }
                if (normal == Vector3.Left)
                {
                    return t.DescriptionEast;
                }
                if (normal == Vector3.Right)
                {
                    return t.DescriptionWest;
                }
            }
            return t.DescriptionWall; //default self wall
        }

        /// <summary>
        /// Initialise a tile with parameters for source data and X,Y offset into data.
        /// </summary>
        /// <param name="tm"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public TileInfo(UWTileMap tm, short X, short Y)
        {
            map = tm;
            tileX = X;
            tileY = Y;

            //Init default render textures.
            North = wallTexture;
            South = wallTexture;
            East = wallTexture;
            West = wallTexture;
        }



        /// <summary>
        /// Changes the layout of a range of tiles
        /// </summary>
        /// <param name="StartTileX"></param>
        /// <param name="StartTileY"></param>
        /// <param name="newWallTexture"></param>
        /// <param name="newFloorTexture"></param>
        /// <param name="newHeight"></param>
        /// <param name="newType"></param>
        /// <param name="DimX"></param>
        /// <param name="DimY"></param>
        /// <param name="HeightAdjustFlag"></param>
        public static void ChangeTile(int StartTileX, int StartTileY, int newWallTexture = 0x3F, int newFloorTexture = 0xF, int newHeight = 0xF, int newType = 0xA, int DimX = 0, int DimY = 0, int HeightAdjustFlag = 0)
        {
            for (int currentX = StartTileX; currentX <= StartTileX + DimX; currentX++)
            {
                for (int currentY = StartTileY; currentY <= StartTileY + DimY; currentY++)
                {
                    if (UWTileMap.ValidTile(currentX, currentY))
                    {
                        var tileToChange = UWTileMap.current_tilemap.Tiles[currentX, currentY];
                        var initialheight = tileToChange.floorHeight;//to later check if objects need to be moved.

                        if ((HeightAdjustFlag == 1) || (HeightAdjustFlag == 3))
                        {
                            newHeight = initialheight + 2 - HeightAdjustFlag;
                        }

                        if ((newHeight >= 0) && (newHeight <= 0xE))
                        {
                            tileToChange.floorHeight = (short)newHeight;

                        }

                        if ((newHeight == 15) && (HeightAdjustFlag == 4))
                        {
                            tileToChange.floorHeight = 0xF;
                        }

                        if ((newFloorTexture < 0xF) && (_RES == GAME_UW2) || ((newFloorTexture < 0xB) && (_RES != GAME_UW2)))
                        {
                            tileToChange.floorTexture = (short)newFloorTexture;
                            //TODO some terrain changes happen here too.
                        }

                        //TODO wall textures
                        if (newWallTexture < 0x3F)
                        {
                            tileToChange.wallTexture = (short)newWallTexture;
                            //Update NSEW of neighbours
                        }

                        bool NewTileIsSolid = false;
                        if (newType < 0xA)
                        {
                            tileToChange.tileType = (short)newType;
                            if (newType == UWTileMap.TILE_SOLID)
                            {
                                NewTileIsSolid = true;
                            }
                        }




                        var tileObjectMoveXMin = Math.Max(currentX - 1, 0);
                        var tileObjectMoveYMin = Math.Max(currentY - 1, 0);
                        var tileObjectMoveXMax = Math.Max(currentX + 1, 63);
                        var tileObjectMoveYMax = Math.Max(currentY + 1, 63);
                        if (tileToChange.floorHeight != initialheight)
                        {
                            tileToChange.floorHeight = (short)newHeight; //UW seems to set this again here?
                            bool HasRaised = tileToChange.floorHeight >= initialheight;
                            //TODO move objects in the affected tiles
                            //move objects in tile up or down
                            for (int xObjectMove = tileObjectMoveXMin; xObjectMove <= tileObjectMoveXMax; xObjectMove++)
                            {
                                for (int yObjectMove = tileObjectMoveYMin; yObjectMove <= tileObjectMoveYMax; yObjectMove++)
                                {
                                    var tileForObjectHeightChange = UWTileMap.current_tilemap.Tiles[xObjectMove, yObjectMove];

                                    var next = tileForObjectHeightChange.indexObjectList;
                                    while (next != 0)
                                    {
                                        var obj = UWTileMap.current_tilemap.LevelObjects[next];
                                        if (WillObjectMoveWithTileHeightChange(tileXToCheck: xObjectMove, tileYToCheck: yObjectMove, currentX: currentX, currentY: currentY, InitialTileHeight: initialheight, NewHeight: newHeight, obj: obj))//Checks if the object (based on it's position will move with this tile height change)
                                        {
                                            if (HasRaised)
                                            {
                                                next = (short)RaiseObjectInChangingTile(obj: obj, NewTileHeight: newHeight, TileIsSolid: NewTileIsSolid);
                                            }
                                            else
                                            {
                                                next = (short)LowerObjectInChangingTile(obj: obj, InitialTileHeight: initialheight,  NewTileHeight: newHeight, TileIsSolid: NewTileIsSolid, HeightAdjustFlag: HeightAdjustFlag);                                                
                                            }
                                        }
                                        else
                                        {
                                            next = obj.next;
                                        }
                                    }
                                }
                            }
                        }

                        tileToChange.Render = true;
                        tileToChange.Redraw = true;
                        main.DoRedraw = true;

                        MakeFacesVisible(tileToChange);
                        //update neighbours to ensure their faces are always visible.
                        MakeNeighbourTileFacesVisible(tileToChange);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if object should move with a tile height change
        /// </summary>
        /// <returns></returns>
        static bool WillObjectMoveWithTileHeightChange(int tileXToCheck, int tileYToCheck, int currentX, int currentY, int InitialTileHeight, int NewHeight, uwObject obj)
        {
            //if a 3d model or trap/trigger it will not move
            if ((obj.majorclass == 6) || (obj.majorclass == 5))
            {
                return false;
            }
            //Will possibly move if in the same tile. (height checks happen in the move up/down function later on)
            if ((tileXToCheck == currentX) && (tileYToCheck == currentY))
            {
                return true;
            }
            else
            {
                InitialTileHeight = InitialTileHeight << 3; // convert to a zpos value
                NewHeight = NewHeight << 3;
                if (InitialTileHeight <= NewHeight)
                {
                    //tile has raised
                    if (obj.zpos < InitialTileHeight)
                    {
                        return false;
                    }
                    else
                    {
                        if (obj.zpos >= NewHeight)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //tile has lowered.
                    if (obj.zpos != InitialTileHeight)
                    {
                        return false;
                    }
                }
                //ovr110_E9E
                //if in the same tile and height clips it will move
            }

            var XCoordinate = ((tileXToCheck - currentX) << 3) + obj.xpos;
            var YCoordinate = ((tileYToCheck - currentY) << 3) + obj.ypos;
            var radius = commonObjDat.radius(obj.item_id);

            if (XCoordinate < 0)
            {
                if (XCoordinate + radius >= 0)
                {
                    goto evalY;
                }
            }

            if (XCoordinate > 7)
            {
                if (XCoordinate + radius <= 7)
                {
                    goto evalY;
                }
            }
            if (tileXToCheck != currentX)
            {
                return false;
            }


        evalY:
            if (YCoordinate < 0)
            {
                if (YCoordinate + radius >= 0)
                {
                    return true;
                }
            }
            if (YCoordinate > 7)
            {
                if (YCoordinate - radius <= 7)
                {
                    return true;
                }
            }
            if (tileYToCheck != currentY)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        static int LowerObjectInChangingTile(uwObject obj, int NewTileHeight, int InitialTileHeight, bool TileIsSolid, int HeightAdjustFlag)
        {
            //todo, moves the object in a moving and returns the relevant NEXT index to assess as the object may be destroyed by this action
            var nextObj = obj.next;
            if (obj.zpos == InitialTileHeight << 3)
            {
                obj.zpos = (short)NewTileHeight;
                if (obj == playerdat.playerObject)
                {
                    if ((HeightAdjustFlag == 1) || (HeightAdjustFlag == 3))
                    {
                        motion.playerMotionParams.z_4 = (short)(NewTileHeight << 6);
                    }
                    else
                    {
                        motion.ProcessPlayerTileState(0x10, 1);
                    }
                    playerdat.PositionPlayerObject();
                }
                else
                {
                    if (obj.IsStatic)
                    {
                        if (TileIsSolid)
                        {
                            Debug.Print("Object deletion in LowerObject, Replace this function call with proper version of RemoveObject()");
                            ObjectRemover.DeleteObjectFromTile_DEPRECIATED(obj.tileX, obj.tileY, obj.index, true);
                        }
                    }
                    else
                    {
                        if (obj.majorclass == 1)
                        {
                            // mobile npc
                            obj.UnkBit_0X13_Bit7 = 1;
                        }
                        else
                        {
                            //non npc mobile
                            obj.CoordinateZ = NewTileHeight << 6;
                        }
                        objectInstance.Reposition(obj);
                    }
                }
            }
            //ovr110_DF9
            return nextObj;
        }

        /// <summary>
        /// Moves the object up in the tile. If the tile has become solid may destroy the object if static, will apply raw crushing damage if the height of the object clips the ceiling.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="NewTileHeight"></param>
        /// <param name="TileIsSolid"></param>
        /// <returns>The objects Next value</returns>
        static int RaiseObjectInChangingTile(uwObject obj, int NewTileHeight, bool TileIsSolid)
        {
            var nextObj = obj.next;
            if ((NewTileHeight << 3) + commonObjDat.height(obj.item_id) >= 0x7F)
            {
                damage.DamageObject(obj, 0xFF, 0, UWTileMap.current_tilemap.LevelObjects, true, 0); //apply raw crushing damage to the object.
            }
            if (obj.zpos < (NewTileHeight << 3))
            {
                //object is located below new tileheight.
                obj.zpos = (short)(NewTileHeight << 3);
                
                if ((obj.IsStatic) || (obj.majorclass == 1))  //static or npc
                {
                    if (obj == playerdat.playerObject)
                    {
                        //object is the player.
                        motion.playerMotionParams.z_4 = (short)(NewTileHeight << 6);
                        playerdat.PositionPlayerObject();
                    }
                    else
                    {
                        if (obj.IsStatic)
                        {
                            //static objects
                            if (TileIsSolid)
                            {
                                Debug.Print("Object deletion in RaiseObject, Replace this function call with proper version of RemoveObject()");
                                ObjectRemover.DeleteObjectFromTile_DEPRECIATED(obj.tileX, obj.tileY, obj.index, true);
                            }
                            else
                            {
                                objectInstance.Reposition(obj);
                            }
                        }
                    }
                }
                else
                {
                    //mobile non-npc
                    obj.CoordinateZ = NewTileHeight << 6;
                    objectInstance.Reposition(obj);
                }
            }

            // ovr110_CEE
            return nextObj;
        }

        private static void MakeNeighbourTileFacesVisible(TileInfo tileToChange, int newWall = 0x3F)
        {
            if (UWTileMap.ValidTile(tileToChange.tileX + 1, tileToChange.tileY))
            {
                MakeFacesVisible(UWTileMap.current_tilemap.Tiles[tileToChange.tileX + 1, tileToChange.tileY]);
                if (newWall < 0x3F)
                {
                    UWTileMap.current_tilemap.Tiles[tileToChange.tileX + 1, tileToChange.tileY].South = (short)newWall;
                }
            }
            if (UWTileMap.ValidTile(tileToChange.tileX - 1, tileToChange.tileY))
            {
                MakeFacesVisible(UWTileMap.current_tilemap.Tiles[tileToChange.tileX - 1, tileToChange.tileY]);
                if (newWall < 0x3F)
                {
                    UWTileMap.current_tilemap.Tiles[tileToChange.tileX - 1, tileToChange.tileY].North = (short)newWall;
                }
            }
            if (UWTileMap.ValidTile(tileToChange.tileX, tileToChange.tileY + 1))
            {
                MakeFacesVisible(UWTileMap.current_tilemap.Tiles[tileToChange.tileX, tileToChange.tileY + 1]);
                if (newWall < 0x3F)
                {
                    UWTileMap.current_tilemap.Tiles[tileToChange.tileX, tileToChange.tileY + 1].West = (short)newWall;
                }
            }
            if (UWTileMap.ValidTile(tileToChange.tileX, tileToChange.tileY - 1))
            {
                MakeFacesVisible(UWTileMap.current_tilemap.Tiles[tileToChange.tileX, tileToChange.tileY - 1]);
                if (newWall < 0x3F)
                {
                    UWTileMap.current_tilemap.Tiles[tileToChange.tileX, tileToChange.tileY - 1].East = (short)newWall;
                }
            }
        }


        private static void MakeFacesVisible(TileInfo tileToChange)
        {
            tileToChange.Render = true;
            //Ensure tile faces are always visible
            for (int i = 0; i <= tileToChange.VisibleFaces.GetUpperBound(0); i++)
            {
                if (!tileToChange.VisibleFaces[i])
                {
                    tileToChange.VisibleFaces[i] = true;
                    main.DoRedraw = true;
                    tileToChange.Redraw = true;
                }
            }
        }


        /// <summary>
        /// Returns true if playerobject is more than range tiles away from the targetX/Y. Always returns true if not player
        /// </summary>
        /// <param name="targetX"></param>
        /// <param name="targetY"></param>
        /// <param name="isPlayer"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool CheckIfOutsideRange(int targetX, int targetY, int isPlayer, int range = 8)
        {
            if (isPlayer != 0)
            {
                return (
                    (System.Math.Abs(playerdat.playerObject.tileX - targetX) >= range)
                    ||
                    (System.Math.Abs(playerdat.playerObject.tileY - targetY) >= range)
                );
            }
            else
            {
                return true;
            }
        }
    }//end class
}//end namespace