using System.Diagnostics;
using Godot;
using System;

namespace Underworld
{
    /// <summary>
    /// Tile map class for storing and accessing the tilemap and tile properties..
    /// </summary>
    public class UWTileMap : Loader
    {
        //Raw Data
        public static Underworld.UWTileMap current_tilemap;
        /// <summary>
        /// Data containing and tiles, objects and in UW2 the overlays
        /// </summary>
        public UWBlock lev_ark_block = new UWBlock();
        /// <summary>
        /// Data containing the texture maap
        /// </summary>
        public UWBlock tex_ark_block = new UWBlock();
        /// <summary>
        /// Data containing animation overlays (UW1 only)
        /// </summary>
        public UWBlock ovl_ark_block = new UWBlock();

        //Tile Types for UW1 & 2 and SS1. Note the diag tiles are flipped around in SS1.
        public const short TILE_SOLID = 0;
        public const short TILE_OPEN = 1;
        public const short TILE_DIAG_SE = 2;
        public const short TILE_DIAG_SW = 3;
        public const short TILE_DIAG_NE = 4;
        public const short TILE_DIAG_NW = 5;
        public const short TILE_SLOPE_N = 6;
        public const short TILE_SLOPE_S = 7;
        public const short TILE_SLOPE_E = 8;
        public const short TILE_SLOPE_W = 9;

        /// <summary>
        /// The tile map size along the x axis
        /// </summary>
        public const short TileMapSizeX = 63; //0 to 63

        /// <summary>
        /// The tile map size along the y axis.
        /// </summary>
        public const short TileMapSizeY = 63; //0 to 63

        public const int TileMapDataSize = 0x7c08;

        /// <summary>
        /// Locaton X and Y of the object storage tile location where off-map objects are instantiated.
        /// </summary>
        public const short ObjectStorageTile = 99;

        //Visible faces indices. Used in sorting tile surface visiblity.
        public const short vTOP = 0;
        public const short vEAST = 1;
        public const short vBOTTOM = 2;
        public const short vWEST = 3;
        public const short vNORTH = 4;
        public const short vSOUTH = 5;

        public const int UW1_TEXTUREMAPSIZE = 64;
        public const int UW2_TEXTUREMAPSIZE = 70;
        public const int UWDEMO_TEXTUREMAPSIZE = 63;

        // public const int UW1_NO_OF_LEVELS = 9;
        // public const int UW2_NO_OF_LEVELS = 80;

        /// <summary>
        /// The ceiling texture for this level
        /// </summary>
        public short UWCeilingTexture;

        /// <summary>
        /// Returns the string value at the end of the tile map data (should spell uw)
        /// </summary>
        public string uw
        {
            get
            {                
                return $"{(char)this.lev_ark_block.Data[0x7c07]}{(char)this.lev_ark_block.Data[0x7c06]}";
            }
        }

        /// <summary>
        /// Animation overlay. Controls how long an animated effect appears for.
        /// </summary>
        // public struct Overlay
        // {
        //     public int header;
        //     public int link;
        //     public int duration;
        //     public int tileX;
        //     public int tileY;
        // };

        /// <summary>
        /// Lists of overlays for controlling animated items.
        /// </summary>
        public AnimationOverlay[] Overlays = new AnimationOverlay[64];

        public int thisLevelNo; //The number of this level
        public const int UW_CEILING_HEIGHT = 32;
        //public short CEILING_HEIGHT;
        // public short SHOCK_CEILING_HEIGHT;

        /// <summary>
        /// The texture indices for the current map.
        /// </summary>
        public short[] texture_map = new short[272];


        /// <summary>
        /// Tile info storage class
        /// </summary>
        public TileInfo[,] Tiles = new TileInfo[TileMapSizeX + 1, TileMapSizeY + 1];

        /// <summary>
        /// The current tile X that the player is in
        /// </summary>
        public static short visitTileX;
        /// <summary>
        /// The current tile Y that the player is in.
        /// </summary>
        public static short visitTileY;

        /// The tile X that the player was in the previous frame
        /// </summary>
        public static short visitedTileX;
        /// <summary>
        /// The current tile Y that the player was in the previous frame
        /// </summary>
        public static short visitedTileY;

        /// <summary>
        /// Reference to the objects list for this level.
        /// </summary>
        public uwObject[] LevelObjects = new uwObject[1024];



        /// <summary>
        /// Pointer to the next slot in the static freelist
        /// </summary>
        public int StaticFreeListPtr
        {
            get
            {
                return (int)getAt(lev_ark_block.Data, 0x7C04, 16);
            }
            set
            {
                setAt(lev_ark_block.Data, 0x7C04, 16, value);
            }
        }

        /// <summary>
        /// Remember to move pointer before changing!
        /// </summary>
        public int StaticFreeListObject
        {
            get
            {
                return (int)getAt(lev_ark_block.Data,  0x74fc + (StaticFreeListPtr * 2), 16);
            }
            set
            {
                setAt(lev_ark_block.Data,  0x74fc + (StaticFreeListPtr * 2), 16, value);
            }
        }

        /// <summary>
        /// Pointer to the next slot in the mobile freelist
        /// </summary>
        public int MobileFreeListPtr
        {
            get
            {
                return (int)getAt(lev_ark_block.Data, 0x7C02, 16);
            }
            set
            {
                 setAt(lev_ark_block.Data, 0x7C02, 16, value);
            }
        }

        /// <summary>
        /// Remember to move pointer before changing!
        /// </summary>
        public int MobileFreeListObject
        {
            get
            {
                return (int)getAt(lev_ark_block.Data,  0x7300 + (MobileFreeListPtr * 2), 16);
            }
            set
            {
                setAt(lev_ark_block.Data,  0x7300 + (MobileFreeListPtr * 2), 16, value);
            }
        }

        public UWTileMap(int NewLevelNo)
        {
            thisLevelNo = NewLevelNo;
            lev_ark_block = LevArkLoader.LoadLevArkBlock(NewLevelNo);
		    tex_ark_block = LevArkLoader.LoadTexArkBlock(NewLevelNo);
		    ovl_ark_block = LevArkLoader.LoadOverlayBlock(NewLevelNo);
        }

/// <summary>
	/// Loads the tilemap for the specified level number (dungeon_level-1)
	/// </summary>
	/// <param name="newLevelNo"></param>
	/// <returns></returns>
	public static void LoadTileMap(int newLevelNo, string datafolder, bool fromMainMenu)
	{
		ObjectCreator.worldobjects = main.instance.GetNode<Node3D>("/root/Underworld/worldobjects");
		Node3D the_tiles = main.instance.GetNode<Node3D>("/root/Underworld/tilemap");

		LevArkLoader.LoadLevArkFileData(folder: datafolder);
		current_tilemap = new(newLevelNo);

		current_tilemap.BuildTileMapUW(
            levelNo: newLevelNo, 
            lev_ark: current_tilemap.lev_ark_block, 
            tex_ark: current_tilemap.tex_ark_block, 
            ovl_ark: current_tilemap.ovl_ark_block);

		ObjectCreator.GenerateObjects(
            objects: current_tilemap.LevelObjects, 
            a_tilemap: current_tilemap);

		the_tiles.Position = new Vector3(0f, 0f, 0f);

		tileMapRender.GenerateLevelFromTileMap(
            parent: the_tiles, 
            Level: current_tilemap, 
            objList: current_tilemap.LevelObjects, 
            UpdateOnly: false);

		switch (_RES)
		{
			case GAME_UW2:
				automap.automaps = new automap[80]; 
                automapnote.automapsnotes= new automapnote[80];
                break;
			default:
				automap.automaps = new automap[9]; 
                automapnote.automapsnotes= new automapnote[9];
                break;
		}
		automap.automaps[newLevelNo] = new automap(newLevelNo, (int)_RES);
        automapnote.automapsnotes[newLevelNo] = new automapnote(newLevelNo, (int)_RES);


        playerdat.PlayerStatusUpdate();

		Debug.Print($"{current_tilemap.uw}");

        if (fromMainMenu)
        {
            uimanager.EnableDisable(uimanager.instance.PanelMainMenu,false);
        }
        uimanager.InGame = true;		
	}



        /// <summary>
        /// Checks to see if the tile at a specified location is within the valid game world. (eg is rendered and is not a solid).
        /// Assumes the map is positioned at 0,0,0
        /// </summary>
        /// <returns><c>true</c>, if tile was valided, <c>false</c> otherwise.</returns>
        /// <param name="location">Location.</param>
        public bool ValidTile(Vector3 location)
        {
            int tileX = (int)(location.X / 1.2f);
            int tileY = (int)(location.Y / 1.2f);
            if ((tileX > TileMapSizeX) || (tileX < 0) || (tileY > TileMapSizeY) || (tileY < 0))
            {//Location is outside the map
                return false;
            }
            int tileType = GetTileType(tileX, tileY);
            bool isRendered = GetTileRender(tileX, tileY);

            return ((tileType != TILE_SOLID) && (isRendered));
        }

        /// <summary>
        /// Validates the tile to see if it is within the range of tiles.
        /// </summary>
        /// <returns><c>true</c>, if tile was valided, <c>false</c> otherwise.</returns>
        /// <param name="tileX">Tile x.</param>
        /// <param name="tileY">Tile y.</param>
        public static bool ValidTile(int tileX, int tileY)
        {
            return (((tileX >= 0) && (tileX <= TileMapSizeX)) && ((tileY >= 0) && (tileY <= TileMapSizeY)));
        }

        /// <summary>
        /// Tells if the tile is one of the square open types
        /// </summary>
        /// <returns><c>true</c>, if tile open was ised, <c>false</c> otherwise.</returns>
        /// <param name="TileType">Tile type.</param>
        public static bool isTileOpen(int TileType)
        {
            switch (TileType)
            {
                case TILE_OPEN:
                case TILE_SLOPE_N:
                case TILE_SLOPE_S:
                case TILE_SLOPE_E:
                case TILE_SLOPE_W:
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        /// <summary>
        /// Gets the height of the floor for the specified tile.
        /// </summary>
        /// <returns>The floor height.</returns>
        /// <param name="tileX">Tile x.</param>
        /// <param name="tileY">Tile y.</param>
        public int GetFloorHeight(int tileX, int tileY)
        {
            if (ValidTile(tileX, tileY))
            {
                return Tiles[tileX, tileY].floorHeight;
            }
            else
            {
                // Debug.Log("invalid tile for height at " + tileX + "," + tileY);
                return 0;
            }
        }

        /// <summary>
        /// Gets the type of the tile.
        /// </summary>
        /// <returns>The tile type.</returns>
        /// <param name="tileX">Tile x.</param>
        /// <param name="tileY">Tile y.</param>
        public int GetTileType(int tileX, int tileY)
        {
            //if ((tileX>TileMap.TileMapSizeX) || (tileY>TileMap.TileMapSizeY) || (tileX<0) || (tileY<0))
            if (!ValidTile(tileX, tileY))
            {//Assume out of bounds is solid
                return TILE_SOLID;
            }
            else
            {
                return Tiles[tileX, tileY].tileType;
            }
        }

        /// <summary>
        /// Gets the tile render state. 
        /// </summary>
        /// <returns>The tile render.</returns>
        /// <param name="LevelNo">Level no.</param>
        /// <param name="tileX">Tile x.</param>
        /// <param name="tileY">Tile y.</param>
        private bool GetTileRender(int tileX, int tileY)
        {
            return Tiles[tileX, tileY].Render == true;
        }

        public bool BuildTileMapUW(int levelNo, UWBlock lev_ark, UWBlock tex_ark, UWBlock ovl_ark)
        {
            long address_pointer = 0;
            short CeilingTexture = 0;

            // UW_CEILING_HEIGHT = 32; // ((128 >> 2) * 8 >> 3);  //Shifts the scale of the level. Idea borrowed from abysmal

            //CEILING_HEIGHT = UW_CEILING_HEIGHT;
            BuildTextureMap(tex_ark, ref CeilingTexture, levelNo);
            this.UWCeilingTexture = CeilingTexture;
            for (short y = 0; y <= TileMapSizeY; y++)
            {
                for (short x = 0; x <= TileMapSizeX; x++)
                {
                    Tiles[x, y] = new TileInfo(this, x, y);
                    address_pointer += 4;
                }
            }

            SetTileMapWallFacesUW();

            BuildObjectListUW();

            //Set x and y for on map objects.
            for (int y = 0; y <= 63; y++)
            {
                for (int x = 0; x <= 63; x++)
                {
                    if (Tiles[x, y].indexObjectList != 0)
                    {
                        int index = Tiles[x, y].indexObjectList;
                        while (index != 0)
                        {
                            var obj = LevelObjects[index];
                            obj.tileX = x; obj.tileY = y;
                            index = LevelObjects[index].next;
                        }
                    }
                }
            }

            //debug enchantments, print all normally accessible enchanted objects.
            for (int y = 0; y <= 63; y++)
            {
                for (int x = 0; x <= 63; x++)
                {
                    if (Tiles[x, y].indexObjectList != 0)
                    {
                        ListEnchantmentsInLinkedList(Tiles[x, y].indexObjectList, x, y);
                    }
                }
            }

            //if (OverlayAddress!=0)
            switch (_RES)
            {
                case GAME_UW1:
                    {
                        if (ovl_ark.DataLen != 0)
                        {//read in the next 64 entries of length 6 bytes	
                            //long OverlayAddress = 0;
                            for (int overlayIndex = 0; overlayIndex < 64; overlayIndex++)
                            {
                                Overlays[overlayIndex] = new AnimationOverlay(overlayIndex);
                            }
                        }
                        break;
                    }
                case GAME_UW2:
                    {
                        //long OverlayAddress = 31752;
                        for (int overlayIndex = 0; overlayIndex < 64; overlayIndex++)
                        {
                            Overlays[overlayIndex] = new AnimationOverlay(overlayIndex);
                        }
                        break;
                    }
            }
            //Reduce map complexity.
            CleanUp();

            return true;
        }

        private void ListEnchantmentsInLinkedList(int listhead, int x,int y)
        {
            var nextObject = listhead;
            while (nextObject != 0)
            {
                var obj = LevelObjects[nextObject];
                if ((obj.is_quant==0) && (obj.link!=0))
                {
                    ListEnchantmentsInLinkedList(obj.link,x ,y);
                }
                try
                {
                    var spelleffect = MagicEnchantment.GetSpellEnchantment(obj, LevelObjects);
                    if (spelleffect != null)
                    {
                        var effectname = spelleffect.NameEnchantment(obj: obj, objList: LevelObjects);
                        Debug.Print($"({x},{y}) {obj.a_name} {obj.index} Link: {obj.link} Enchant:{obj.enchantment} isquant:{obj.is_quant} flag2:{obj.flags2} has spelleffect {spelleffect.SpellMajorClass},{spelleffect.SpellMinorClass} {effectname}");
                    }
                }
                catch (Exception e)
                {
                    Debug.Print($"{e}");
                }
                nextObject = obj.next;
            }
        }


        void BuildObjectListUW()
        {
            LevelObjects = new uwObject[1024];
            int address_pointer = 0;
            int objectsAddress = (64 * 64 * 4);
            for (short x = 0; x < 1024; x++)
            {   //read in master object list
                var uwobj = new uwObject
                {
                    isInventory = false,
                    IsStatic = (x >= 256),
                    index = x,
                    PTR = 0 + objectsAddress + address_pointer,
                    DataBuffer = this.lev_ark_block.Data
                };

                LevelObjects[x] = uwobj;

                //Debug.Print(StringLoader.GetObjectNounUW(uwobj.item_id));
                // if (uwobj.npc_whoami != 0)
                // {
                //     Debug.Print(StringLoader.GetString(7, uwobj.npc_whoami + 16));
                // }

                if (x < 256)
                {
                    address_pointer += 27;
                }
                else
                {
                    address_pointer += 8;
                }

                // objList[x] = new ObjectLoaderInfo(x, map, true)
                // {
                //     map = map,
                //     parentList = this,
                //     index = x,                
                //     address = map.lev_ark_block.Address + objectsAddress + address_pointer
                // };          

                // if ((objList[x].item_id >= 464) && ((_RES == GAME_UW1) || (_RES == GAME_UWDEMO)))//Fixed for bugged out of range items
                // {
                //     objList[x].item_id = 0;
                // }
                // HandleMovingDoors(objList, x);
                // SetObjectTextureValue(objList, map.texture_map, x);
            }
        }

        /// <summary>
        /// Creates the tile map wall textures for each north, south, east and west faces
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public void SetTileMapWallFacesUW()
        {
            short x; short y;
            for (y = 0; y <= TileMapSizeY; y++)
            {
                for (x = 0; x <= TileMapSizeX; x++)
                {
                    SetTileWallFacesUW(x, y);
                }
            }
        }

        /// <summary>
        /// Sets the tile wall faces for the selected tile
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public void SetTileWallFacesUW(short x, short y)
        {
            if (Tiles[x, y].tileType >= 0)//was just solid only. Note: If textures are all wrong it's probably caused here!
            {
                //assign it's north texture
                if (y < TileMapSizeY)
                {
                    Tiles[x, y].North = Tiles[x, y + 1].wallTexture;
                }
                else
                {
                    Tiles[x, y].North = -1;
                }
                //assign it's southern
                if (y > 0)
                {
                    Tiles[x, y].South = Tiles[x, y - 1].wallTexture;
                }
                else
                {
                    Tiles[x, y].South = -1;
                }
                //it's east
                if (x < TileMapSizeX)
                {
                    Tiles[x, y].East = Tiles[x + 1, y].wallTexture;
                }
                else
                {
                    Tiles[x, y].East = -1;
                }
                //assign it's West
                if (x > 0)
                {
                    Tiles[x, y].West = Tiles[x - 1, y].wallTexture;
                }
                else
                {
                    Tiles[x, y].West = -1;
                }
            }
        }

        /// <summary>
        /// Cleans up the tilemap. Splits up the tiles into strips of tiles along the x or y axis and sets tile face visibility as required
        /// </summary>
        /// <param name="game">Game.</param>
        /// Although the tile map renderer supports tiles of size X*Y I'm only smart enought to optimise the tilemap into strips of X*1 or Y*1 !!
        public void CleanUp()
        {
            int x; int y;

            for (x = 0; x <= TileMapSizeX; x++)
            {
                for (y = 0; y <= TileMapSizeY; y++)
                {
                    //Set some easy tile visible settings
                    switch (Tiles[x, y].tileType)
                    {
                        case TILE_SOLID:
                            //Bottom and top are invisible
                            Tiles[x, y].VisibleFaces[vBOTTOM] = false;
                            Tiles[x, y].VisibleFaces[vTOP] = false;
                            break;
                        default:
                            //Bottom and top is invisible
                            Tiles[x, y].VisibleFaces[vBOTTOM] = false;
                            Tiles[x, y].VisibleFaces[vTOP] = false;
                            break;
                    }
                }

                for (x = 0; x <= TileMapSizeX; x++)
                {
                    for (y = 0; y <= TileMapSizeY; y++)
                    {
                        //lets test this tile for visibility
                        //A tile is invisible if it only touches other solid tiles and has no objects or does not have a terrain change.
                        if ((Tiles[x, y].tileType == 0) && (Tiles[x, y].indexObjectList == 0) && (Tiles[x, y].TerrainChange == false))
                        {
                            switch (y)
                            {
                                case 0: //bottom row
                                    switch (x)
                                    {
                                        case 0: //bl corner
                                            if ((Tiles[x + 1, y].tileType == 0) && (Tiles[x, y + 1].tileType == 0)
                                                    && (Tiles[x + 1, y].TerrainChange == false) && (Tiles[x, y + 1].TerrainChange == false))
                                            { Tiles[x, y].Render = false; ; break; }
                                            else { Tiles[x, y].Render = true; break; }
                                        case TileMapSizeX://br corner
                                            if ((Tiles[x - 1, y].tileType == 0) && (Tiles[x, y + 1].tileType == 0)
                                                    && (Tiles[x - 1, y].TerrainChange == false) && (Tiles[x, y + 1].TerrainChange == false))
                                            { Tiles[x, y].Render = false; break; }
                                            else { Tiles[x, y].Render = true; break; }
                                        default: // invert t
                                            if ((Tiles[x + 1, y].tileType == 0) && (Tiles[x, y + 1].tileType == 0) && (Tiles[x + 1, y].tileType == 0)
                                                    && (Tiles[x + 1, y].TerrainChange == false) && (Tiles[x, y + 1].TerrainChange == false) && (Tiles[x + 1, y].TerrainChange == false))
                                            { Tiles[x, y].Render = false; break; }
                                            else { Tiles[x, y].Render = true; break; }
                                    }
                                    break;
                                case TileMapSizeY: //Top row
                                    switch (x)
                                    {
                                        case 0: //tl corner
                                            if ((Tiles[x + 1, y].tileType == 0) && (Tiles[x, y - 1].tileType == 0)
                                                    && (Tiles[x + 1, y].TerrainChange == false) && (Tiles[x, y - 1].TerrainChange == false))
                                            { Tiles[x, y].Render = false; break; }
                                            else { Tiles[x, y].Render = true; break; }
                                        case TileMapSizeX://tr corner
                                            if ((Tiles[x - 1, y].tileType == 0) && (Tiles[x, y - 1].tileType == 0)
                                                    && (Tiles[x - 1, y].TerrainChange == false) && (Tiles[x, y - 1].TerrainChange == false))
                                            { Tiles[x, y].Render = false; break; }
                                            else { Tiles[x, y].Render = true; break; }
                                        default: //  t
                                            if ((Tiles[x + 1, y].tileType == 0) && (Tiles[x, y - 1].tileType == 0) && (Tiles[x - 1, y].tileType == 0)
                                                    && (Tiles[x + 1, y].TerrainChange == false) && (Tiles[x, y - 1].TerrainChange == false) && (Tiles[x - 1, y].TerrainChange == false))
                                            { Tiles[x, y].Render = false; break; }
                                            else { Tiles[x, y].Render = true; break; }
                                    }
                                    break;
                                default: //
                                    switch (x)
                                    {
                                        case 0:     //left edge
                                            if ((Tiles[x, y + 1].tileType == 0) && (Tiles[x + 1, y].tileType == 0) && (Tiles[x, y - 1].tileType == 0)
                                                    && (Tiles[x, y + 1].TerrainChange == false) && (Tiles[x + 1, y].TerrainChange == false) && (Tiles[x, y - 1].TerrainChange == false))
                                            { Tiles[x, y].Render = false; break; }
                                            else { Tiles[x, y].Render = true; break; }
                                        case TileMapSizeX:  //right edge
                                            if ((Tiles[x, y + 1].tileType == 0) && (Tiles[x - 1, y].tileType == 0) && (Tiles[x, y - 1].tileType == 0)
                                                    && (Tiles[x, y + 1].TerrainChange == false) && (Tiles[x - 1, y].TerrainChange == false) && (Tiles[x, y - 1].TerrainChange == false))
                                            { Tiles[x, y].Render = false; break; }
                                            else { Tiles[x, y].Render = true; break; }
                                        default:        //+
                                            if ((Tiles[x, y + 1].tileType == 0) && (Tiles[x + 1, y].tileType == 0) && (Tiles[x, y - 1].tileType == 0) && (Tiles[x - 1, y].tileType == 0)
                                                    && (Tiles[x, y + 1].TerrainChange == false) && (Tiles[x + 1, y].TerrainChange == false) && (Tiles[x, y - 1].TerrainChange == false) && (Tiles[x - 1, y].TerrainChange == false))
                                            { Tiles[x, y].Render = false; break; }
                                            else { Tiles[x, y].Render = true; break; }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            //return;
            // if (game == GAME_SHOCK)
            // {//TODO:FIx some z-fighting due to tile visibility.
            //     return;
            // }
            //return;
            int j;
            //Now lets combine the solids along particular axis
            for (x = 0; x < TileMapSizeX; x++)
            {
                for (y = 0; y < TileMapSizeY; y++)
                {
                    if ((Tiles[x, y].Grouped == false))
                    {
                        j = 1;
                        while ((Tiles[x, y].Render == true) && (Tiles[x, y + j].Render == true) && (Tiles[x, y + j].Grouped == false))      //&& (Tiles[x,y].tileType ==0) && (Tiles[x,y+j].tileType ==0)
                        {
                            //combine these two if they match and they are not already part of a group
                            if (DoTilesMatch(Tiles[x, y], Tiles[x, y + j]))
                            {
                                Tiles[x, y + j].Render = false;
                                Tiles[x, y + j].Grouped = true;
                                Tiles[x, y].Grouped = true;
                                //Tiles[x,y].DimY++;
                                j++;
                            }
                            else
                            {
                                break;
                            }

                        }
                        Tiles[x, y].DimY = (short)(Tiles[x, y].DimY + j - 1);
                    }
                }
            }

            ////Now lets combine solids along the other axis
            for (y = 0; y < TileMapSizeY; y++)
            {
                for (x = 0; x < TileMapSizeX; x++)
                {
                    if ((Tiles[x, y].Grouped == false))
                    {
                        j = 1;
                        while ((Tiles[x, y].Render == true) && (Tiles[x + j, y].Render == true) && (Tiles[x + j, y].Grouped == false))      //&& (Tiles[x,y].tileType ==0) && (Tiles[x,y+j].tileType ==0)
                        {
                            //combine these two if they  match and they are not already part of a group
                            if (DoTilesMatch(Tiles[x, y], Tiles[x + j, y]))
                            {
                                Tiles[x + j, y].Render = false;
                                Tiles[x + j, y].Grouped = true;
                                Tiles[x, y].Grouped = true;
                                //Tiles[x,y].DimY++;
                                j++;
                            }
                            else
                            {
                                break;
                            }

                        }
                        Tiles[x, y].DimX = (short)(Tiles[x, y].DimX + j - 1);
                    }
                }
            }

            //Clear invisible faces on solid tiles. 
            //TODO:Support all 64x64 tiles
            for (y = 0; y <= TileMapSizeY; y++)
            {
                for (x = 0; x <= TileMapSizeX; x++)
                {
                     Tiles[x, y].VisibleFaces[vBOTTOM]=false;//hide bottom of tile
                    if ((Tiles[x, y].tileType == TILE_SOLID))
                    {                       
                        int dimx = Tiles[x, y].DimX;
                        int dimy = Tiles[x, y].DimY;

                        if (x == 0)
                        {
                            Tiles[x, y].VisibleFaces[vWEST] = false;
                        }
                        if (x == TileMapSizeX)
                        {
                            Tiles[x, y].VisibleFaces[vEAST] = false;
                        }
                        if (y == 0)
                        {
                            Tiles[x, y].VisibleFaces[vSOUTH] = false;
                        }

                        if (y == TileMapSizeY)
                        {
                            Tiles[x, y].VisibleFaces[vNORTH] = false;
                        }
                        if ((x + dimx <= TileMapSizeX) && (y + dimy <= TileMapSizeY))
                        {
                            if ((Tiles[x + dimx, y].tileType == TILE_SOLID) && (Tiles[x + dimx, y].TerrainChange == false) && (Tiles[x, y].TerrainChange == false))//Tile to the east is a solid
                            {
                                Tiles[x, y].VisibleFaces[vEAST] = false;
                                Tiles[x + dimx, y].VisibleFaces[vWEST] = false;
                            }
                            if ((Tiles[x, y + dimy].tileType == TILE_SOLID) && (Tiles[x, y].TerrainChange == false) && (Tiles[x, y + dimy].TerrainChange == false))//TIle to the north is a solid
                            {
                                Tiles[x, y].VisibleFaces[vNORTH] = false;
                                Tiles[x, y + dimy].VisibleFaces[vSOUTH] = false;
                            }
                        }
                    }
                }
            }

            //Clear invisible faces on diagonals
            for (y = 1; y < TileMapSizeY; y++)
            {
                for (x = 1; x < TileMapSizeX; x++)
                {
                    switch (Tiles[x, y].tileType)
                    {
                        case TILE_DIAG_NW:
                            {
                                if ((Tiles[x, y - 1].tileType == TILE_SOLID) && (Tiles[x, y - 1].TerrainChange == false))
                                {
                                    Tiles[x, y].VisibleFaces[vSOUTH] = false;
                                    Tiles[x, y - 1].VisibleFaces[vNORTH] = false;
                                }
                                if ((Tiles[x + 1, y].tileType == TILE_SOLID) && (Tiles[x + 1, y].TerrainChange == false))
                                {
                                    Tiles[x, y].VisibleFaces[vEAST] = false;
                                    Tiles[x + 1, y].VisibleFaces[vWEST] = false;
                                }
                            }
                            break;
                        case TILE_DIAG_NE:
                            {
                                if ((Tiles[x, y - 1].tileType == TILE_SOLID) && (Tiles[x, y - 1].TerrainChange == false))
                                {
                                    Tiles[x, y].VisibleFaces[vSOUTH] = false;
                                    Tiles[x, y - 1].VisibleFaces[vNORTH] = false;
                                }
                                if ((Tiles[x - 1, y].tileType == TILE_SOLID) && (Tiles[x - 1, y].TerrainChange == false))
                                {
                                    Tiles[x, y].VisibleFaces[vWEST] = false;
                                    Tiles[x - 1, y].VisibleFaces[vEAST] = false;
                                }
                            }
                            break;
                        case TILE_DIAG_SE:
                            {
                                if ((Tiles[x, y + 1].tileType == TILE_SOLID) && (Tiles[x, y + 1].TerrainChange == false))
                                {
                                    Tiles[x, y].VisibleFaces[vNORTH] = false;
                                    Tiles[x, y + 1].VisibleFaces[vSOUTH] = false;
                                }
                                if ((Tiles[x - 1, y].tileType == TILE_SOLID) && (Tiles[x - 1, y].TerrainChange == false))
                                {
                                    Tiles[x, y].VisibleFaces[vWEST] = false;
                                    Tiles[x - 1, y].VisibleFaces[vEAST] = false;
                                }
                            }
                            break;
                        case TILE_DIAG_SW:
                            {
                                if ((Tiles[x, y + 1].tileType == TILE_SOLID) && (Tiles[x, y + 1].TerrainChange == false))
                                {
                                    Tiles[x, y].VisibleFaces[vNORTH] = false;
                                    Tiles[x, y + 1].VisibleFaces[vSOUTH] = false;
                                }
                                if ((Tiles[x + 1, y].tileType == TILE_SOLID) && (Tiles[x + 1, y].TerrainChange == false))
                                {
                                    Tiles[x, y].VisibleFaces[vEAST] = false;
                                    Tiles[x + 1, y].VisibleFaces[vWEST] = false;
                                }
                            }
                            break;
                    }

                }

            }

            for (y = 1; y < TileMapSizeY; y++)
            {
                for (x = 1; x < TileMapSizeX; x++)
                {
                    if ((Tiles[x, y].tileType == TILE_OPEN) && (Tiles[x, y].TerrainChange == false))
                    {
                        if (
                                ((Tiles[x + 1, y].tileType == TILE_OPEN) && (Tiles[x + 1, y].TerrainChange == false) && (Tiles[x + 1, y].floorHeight >= Tiles[x, y].floorHeight))
                                ||
                                (Tiles[x + 1, y].tileType == TILE_SOLID) && (Tiles[x + 1, y].TerrainChange == false)
                        )
                        {
                            Tiles[x, y].VisibleFaces[vEAST] = false;
                        }


                        if (
                                ((Tiles[x - 1, y].tileType == TILE_OPEN) && (Tiles[x - 1, y].TerrainChange == false) && (Tiles[x - 1, y].floorHeight >= Tiles[x, y].floorHeight))
                                ||
                                (Tiles[x - 1, y].tileType == TILE_SOLID) && (Tiles[x - 1, y].TerrainChange == false)
                        )
                        {
                            Tiles[x, y].VisibleFaces[vWEST] = false;
                        }


                        if (
                                ((Tiles[x, y + 1].tileType == TILE_OPEN) && (Tiles[x, y + 1].TerrainChange == false) && (Tiles[x, y + 1].floorHeight >= Tiles[x, y].floorHeight))
                                ||
                                (Tiles[x, y + 1].tileType == TILE_SOLID) && (Tiles[x, y + 1].TerrainChange == false)
                        )
                        {
                            Tiles[x, y].VisibleFaces[vNORTH] = false;
                        }

                        if (
                                ((Tiles[x, y - 1].tileType == TILE_OPEN) && (Tiles[x, y - 1].TerrainChange == false) && (Tiles[x, y - 1].floorHeight >= Tiles[x, y].floorHeight))
                                ||
                                (Tiles[x, y - 1].tileType == TILE_SOLID) && (Tiles[x, y - 1].TerrainChange == false)
                        )
                        {
                            Tiles[x, y].VisibleFaces[vSOUTH] = false;
                        }
                    }

                }
            }
            //Make sure solids & opens are still consistently visible.
            for (y = 1; y < TileMapSizeY; y++)
            {
                for (x = 1; x < TileMapSizeX; x++)
                {

                    if ((Tiles[x, y].tileType == TILE_SOLID) || (Tiles[x, y].tileType == TILE_OPEN))
                    {
                        int dimx = Tiles[x, y].DimX;
                        int dimy = Tiles[x, y].DimY;
                        if (dimx > 1)
                        {//Make sure the ends are set properly
                            Tiles[x, y].VisibleFaces[vEAST] = Tiles[x + dimx - 1, y].VisibleFaces[vEAST];
                        }
                        if (dimy > 1)
                        {
                            Tiles[x, y].VisibleFaces[vNORTH] = Tiles[x, y + dimy - 1].VisibleFaces[vNORTH];
                        }

                        //Check along each axis
                        for (int i = 0; i < Tiles[x, y].DimX; i++)
                        {
                            if (Tiles[x + i, y].VisibleFaces[vNORTH] == true)
                            {
                                Tiles[x, y].VisibleFaces[vNORTH] = true;
                            }
                            if (Tiles[x + i, y].VisibleFaces[vSOUTH] == true)
                            {
                                Tiles[x, y].VisibleFaces[vSOUTH] = true;
                            }
                        }

                        for (int i = 0; i < Tiles[x, y].DimY; i++)
                        {
                            if (Tiles[x, y + i].VisibleFaces[vEAST] == true)
                            {
                                Tiles[x, y].VisibleFaces[vEAST] = true;
                            }
                            if (Tiles[x, y + i].VisibleFaces[vWEST] == true)
                            {
                                Tiles[x, y].VisibleFaces[vWEST] = true;
                            }
                        }

                    }
                }
            }
            for (y = 0; y <= TileMapSizeY; y++)
            {
                Tiles[0, y].VisibleFaces[vEAST] = true;
                Tiles[TileMapSizeX, y].VisibleFaces[vWEST] = true;
            }
            for (x = 0; x <= TileMapSizeX; x++)
            {
                Tiles[x, 0].VisibleFaces[vNORTH] = true;
                Tiles[x, TileMapSizeY].VisibleFaces[vSOUTH] = true;
            }
        }


        /// <summary>
        /// Check if two tiles are alike
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        bool DoTilesMatch(TileInfo t1, TileInfo t2)
        {//TODO:Tiles have a lot more properties now.
            // if (_RES == GAME_SHOCK)
            // { return false; }
            //if ((t1.tileType >1) || (t1.hasElevator==1) || (t1.TerrainChange ==1) ||  (t2.hasElevator==1) || (t2.TerrainChange ==1) || (t1.isWater ==1) || (t2.isWater ==1)){	//autofail no none solid/open/special.
            if ((t1.tileType > 1) || (t1.TerrainChange == true) || (t2.TerrainChange == true))
            {   //autofail no none solid/open/special.
                return false;
            }
            else
            {
                if ((t1.tileType == 0) && (t2.tileType == 0))   //solid
                {
                    return ((t1.wallTexture == t2.wallTexture) 
                        && (t1.West == t2.West) 
                        && (t1.South == t2.South) 
                        && (t1.East == t2.East) 
                        && (t1.North == t2.North));
                }
                else
                {
                    return (t1.North == t2.North)
                            && (t1.South == t2.South)
                            && (t1.East == t2.East)
                            && (t1.West == t2.West)
                            && (t1.floorTexture == t2.floorTexture)
                            && (t1.floorHeight == t2.floorHeight)
                            && (t1.ceilingHeight == t2.ceilingHeight)
                            && (t1.DimX == t2.DimX) && (t1.DimY == t2.DimY)
                            && (t1.wallTexture == t2.wallTexture)
                            && (t1.tileType == t2.tileType)
                            && (t1.IsDoorForNPC == false) && (t2.IsDoorForNPC == false);//
                }
            }
        }



        public static bool isTerrainWater(int terraintype)
        {
            switch (terraintype)
            {
                case TerrainDatLoader.Water:
                case TerrainDatLoader.Waterfall:
                case TerrainDatLoader.WaterFlowEast:
                case TerrainDatLoader.WaterFlowWest:
                case TerrainDatLoader.WaterFlowNorth:
                case TerrainDatLoader.WaterFlowSouth:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the terrain is lava
        /// </summary>
        /// <param name="terraintype"></param>
        /// <returns>True if it lava</returns>
        public static bool isTerrainLava(int terraintype)
        {
            switch (terraintype)
            {
                case TerrainDatLoader.Lava:
                case TerrainDatLoader.Lavafall:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if Terrain is ice
        /// </summary>
        /// <param name="terraintype"></param>
        /// <returns></returns>
        public static bool isTerrainIce(int terraintype)
        {
            switch (terraintype)
            {
                case TerrainDatLoader.Ice_wall:
                case TerrainDatLoader.IceNonSlip:
                case TerrainDatLoader.Ice_walls:
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Builds a texture map from file data
        /// </summary>
        /// <param name="tex_ark"></param>
        /// <param name="CeilingTexture"></param>
        /// <param name="LevelNo"></param>
        void BuildTextureMap(UWBlock tex_ark, ref short CeilingTexture, int LevelNo)
        {
            short textureMapSize;//=UW1_TEXTUREMAPSIZE;
            switch (_RES)
            {
                case GAME_UW2:
                    textureMapSize = UW2_TEXTUREMAPSIZE;
                    break;
                case GAME_UWDEMO:
                    textureMapSize = UWDEMO_TEXTUREMAPSIZE;
                    break;
                default:
                    textureMapSize = UW1_TEXTUREMAPSIZE;
                    break;
            }
            int offset = 0;
            for (int i = 0; i < textureMapSize; i++)//256
            {
                //TODO: Only use this for texture lookups.
                switch (_RES)
                {
                    case GAME_UWDEMO:
                        {
                            if (i < 48)//Wall textures
                            {
                                texture_map[i] = (short)DataLoader.getValAtAddress(tex_ark, offset, 16);
                                offset += 2;
                            }
                            else
                            if (i <= 57)//Floor textures are 49 to 56, ceiling is 57
                            {
                                texture_map[i] = (short)(DataLoader.getValAtAddress(tex_ark, offset, 16) + 48);
                                offset += 2;
                                if (i == 57)
                                {
                                    CeilingTexture = (short)i;
                                }
                            }
                            else
                            {
                                //door textures are int 8s
                                texture_map[i] = (short)DataLoader.getValAtAddress(tex_ark, offset, 8);
                                //+210; //(i * 1)
                                offset++;
                            }
                            break;
                        }
                    case GAME_UW1:
                        {
                            if (i < 48)//Wall textures
                            {
                                texture_map[i] = (short)DataLoader.getValAtAddress(tex_ark, offset, 16);
                                offset += 2;
                            }
                            else
                            if (i <= 57)//Floor textures are 48 to 56, ceiling is 57
                            {
                                texture_map[i] = (short)(DataLoader.getValAtAddress(tex_ark, offset, 16) + 210);
                                offset += 2;
                                if (i == 57)
                                {
                                    CeilingTexture = (short)i;
                                }
                            }
                            else
                            {
                                //door textures are int 8s
                                texture_map[i] = (short)DataLoader.getValAtAddress(tex_ark, offset, 8);
                                offset++;
                            }
                            break;
                        }
                    case GAME_UW2://uw2
                        {
                            if (i < 64)
                            {
                                texture_map[i] = (short)DataLoader.getValAtAddress(tex_ark, offset, 16);
                                //tmp //textureAddress+//(i*2)
                                offset += 2;
                            }
                            else
                            {
                                //door textures
                                texture_map[i] = (short)DataLoader.getValAtAddress(tex_ark, offset, 8);
                                //tmp //textureAddress+//(i*2)
                                offset++;
                            }
                        }
                        if (i == 0xf)
                        {
                            CeilingTexture = (short)i;
                        }
                        if ((LevelNo == (int)(worlds.UW2_LevelNos.Ethereal4)) && (i == 16))
                        {
                            //Not sure why this is an exceptional case!
                            CeilingTexture = (short)i;
                        }
                        break;
                }
            }
        }


        /// <summary>
        /// Gets the tile in the facing direction from the player at distance magnitude.
        /// </summary>
        /// <param name="magnitude"></param>
        public static TileInfo GetTileInDirection(float magnitude)
        {
            Vector3 targetpos = GetPositionInDirection(magnitude);
            var tileX = -(int)(targetpos.X / 1.2f);
            var tileY = (int)(targetpos.Z / 1.2f);
            Debug.Print($"From {main.gamecam.Position} to {targetpos} {tileX},{tileY}");


            if (ValidTile(tileX, tileY))
            {
                return current_tilemap.Tiles[tileX, tileY];
            }

            return null;
        }

        public static Vector3 GetPositionInDirection(float magnitude)
        {
            var direction = main.gamecam.GlobalTransform.Basis.Z;
            var targetpos = main.gamecam.Position - (direction.Normalized() * magnitude);
            return targetpos;
        }


        /// <summary>
        /// Finds a random spot in the specified tile to spawn an object on
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="xpos"></param>
        /// <param name="ypos"></param>
        /// <param name="zpos"></param>
        public static void GetRandomXYZForTile(TileInfo tile, out int xpos, out int ypos, out int zpos)
        {
            switch (tile.tileType)
            {
                case UWTileMap.TILE_DIAG_NE:
                    zpos = tile.floorHeight << 2;
                    xpos = Rng.r.Next(1, 8);
                    ypos = Rng.r.Next(7 - xpos, 8); //(i >= 7 - j)
                    return;
                case UWTileMap.TILE_DIAG_SE:
                    zpos = tile.floorHeight << 2;
                    xpos = Rng.r.Next(1, 8);
                    ypos = Rng.r.Next(1, xpos); // (i >= j)
                    return;
                case UWTileMap.TILE_DIAG_NW:
                    zpos = tile.floorHeight << 2;
                    xpos = Rng.r.Next(1, 8);
                    ypos = Rng.r.Next(xpos, 8); // ((i <= j)
                    return;
                case UWTileMap.TILE_DIAG_SW:
                    zpos = tile.floorHeight << 2;
                    xpos = Rng.r.Next(1, 8);
                    ypos = Rng.r.Next(0, 8 - xpos); // (7 - i >= j)
                    return;
                case UWTileMap.TILE_SLOPE_S:
                    xpos = Rng.r.Next(0, 8);
                    ypos = Rng.r.Next(0, 8);
                    zpos = (8 - ypos) + (tile.floorHeight << 2);
                    return;
                case UWTileMap.TILE_SLOPE_N:
                    xpos = Rng.r.Next(0, 8);
                    ypos = Rng.r.Next(0, 8);
                    zpos = (ypos) + (tile.floorHeight << 2);
                    return;
                case UWTileMap.TILE_SLOPE_E:
                    xpos = Rng.r.Next(0, 8);
                    ypos = Rng.r.Next(0, 8);
                    zpos = (xpos) + (tile.floorHeight << 2);
                    return;
                case UWTileMap.TILE_SLOPE_W:
                    xpos = Rng.r.Next(0, 8);
                    ypos = Rng.r.Next(0, 8);
                    zpos = (8 - xpos) + (tile.floorHeight << 2);
                    return;
                default:
                case UWTileMap.TILE_OPEN:
                case UWTileMap.TILE_SOLID:
                    xpos = Rng.r.Next(0, 8);
                    ypos = Rng.r.Next(0, 8);
                    zpos = tile.floorHeight << 2;
                    return;
            }
        }

    } //end class
}//end namespace