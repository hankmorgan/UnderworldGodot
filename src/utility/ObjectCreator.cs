using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using Godot;

namespace Underworld
{
    /// <summary>
    /// Class for creating instances of objects and calling updates
    /// </summary>
    public class ObjectCreator : UWClass
    {
        //List of active NPCs
        //public static List<npc> npcs;
        public static bool printlabels = false;

        /// <summary>
        /// object art
        /// </summary>
        public static GRLoader grObjects;

        /// <summary>
        /// The node objects will be attached to
        /// </summary>
        public static Node3D worldobjects;

        public enum ObjectListType
        {
            StaticList = 0,
            MobileList = 1
        };

        /// <summary>
        /// Creates a default static object int the tile,
        /// </summary>
        /// <param name="itemid"></param>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        /// <returns></returns>
        public static uwObject spawnObjectInTile(int itemid, int tileX, int tileY, short xpos, short ypos, short zpos, ObjectListType WhichList = ObjectListType.StaticList)
        {
            var slot = ObjectCreator.PrepareNewObject(itemid, WhichList);
            //add to critter object list
            var obj = UWTileMap.current_tilemap.LevelObjects[slot];
            //Insert at the head of the tile list.
            var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
            obj.next = tile.indexObjectList;
            tile.indexObjectList = obj.index;
            obj.xpos = xpos; obj.ypos = ypos; obj.zpos = zpos;
            obj.tileX = tileX; obj.tileY = tileY;
            RenderObject(obj, UWTileMap.current_tilemap);
            return obj;
        }

        /// <summary>
        /// Spawns an object directly in hand
        /// </summary>
        /// <param name="itemid"></param>
        /// <returns></returns>
        public static int SpawnObjectInHand(int itemid, bool changeInteractionmode = true)
        {
            var slot = ObjectCreator.PrepareNewObject(itemid);
            var obj = UWTileMap.current_tilemap.LevelObjects[slot];
            playerdat.ObjectInHand = slot;
            uimanager.instance.mousecursor.SetCursorToObject(obj.item_id);
            if (changeInteractionmode)
            {
                uimanager.InteractionModeToggle(uimanager.InteractionModes.ModePickup);
            }
            return slot;
        }

        /// <summary>
        /// Allocates data for a new object using the defaults
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static int PrepareNewObject(int item_id, ObjectListType WhichList = ObjectListType.StaticList)
        {
            int slot = GetAvailableObjectSlot(WhichList);
            if (slot != 0)
            {
                var obj = UWTileMap.current_tilemap.LevelObjects[slot];
                obj.quality = 0x28;
                obj.item_id = item_id;
                obj.zpos = 0;
                obj.doordir = 0;
                obj.invis = 0;
                obj.enchantment = 0;
                obj.flags = 0; //DBLCHK (0x11)
                obj.xpos = 3;
                obj.ypos = 3;
                obj.heading = 0;
                obj.next = 0;
                obj.owner = 0;
                //allocate static props
                var stackable = commonObjDat.stackable(item_id);

                switch (stackable)
                {
                    case 0:
                    case 2:
                        obj.link = 1;
                        obj.is_quant = 1;
                        break;
                    case 1:
                    case 3:
                    default:
                        obj.is_quant = 0;
                        obj.link = 0;
                        break;
                }
                if (WhichList == ObjectListType.MobileList)
                {
                    //mobile object                    
                    if (obj.majorclass == 1) //NPC
                    {
                        //in uw2 critter is initialised here, uw1 skips this. leaving it like this for now as I've yet to find the uw1 equivilant to study
                        InitialiseCritter(obj);
                    }
                    else
                    {
                        //todo projectile props
                    }
                }
            }
            return slot;
        }

        /// <summary>
        /// Gets an object slot that can be allocated for a new object
        /// </summary>
        /// <param name="WhichList"></param>
        /// <returns></returns>
        public static int GetAvailableObjectSlot(ObjectListType WhichList = ObjectListType.StaticList)
        {
            //look up object free list
            switch (WhichList)
            {
                case ObjectListType.StaticList:
                    //Move PTR down, get object at that point.
                    UWTileMap.current_tilemap.StaticFreeListPtr--;
                    Debug.Print($"Allocating Static {UWTileMap.current_tilemap.StaticFreeListObject} Pointer decremented to {UWTileMap.current_tilemap.StaticFreeListPtr}");
                    return UWTileMap.current_tilemap.StaticFreeListObject;
                case ObjectListType.MobileList:
                    UWTileMap.current_tilemap.MobileFreeListPtr--;
                    Debug.Print($"Allocating Mobile {UWTileMap.current_tilemap.MobileFreeListObject} Pointer decremented to {UWTileMap.current_tilemap.MobileFreeListPtr}");
                    //add to the active mobiles list                    
                    var newslot = UWTileMap.current_tilemap.MobileFreeListObject;
                    UWTileMap.current_tilemap.SetActiveMobileAtIndex(UWTileMap.current_tilemap.NoOfActiveMobiles, newslot);
                    UWTileMap.current_tilemap.NoOfActiveMobiles++;
                    return newslot;

            }
            return 0;
        }

        /// <summary>
        /// Removes object from the game world and allocates free ptrs Does not update object chains
        /// </summary>
        /// <param name="obj"></param>
        public static void RemoveObject(uwObject obj)
        {
            //remove from world
            if (obj.index < 256)
            {//mobile
                UWTileMap.current_tilemap.MobileFreeListObject = obj.index;
                UWTileMap.current_tilemap.MobileFreeListPtr++;
                Debug.Print($"Freeing Mobile {obj.index} Pointer incremented to {UWTileMap.current_tilemap.MobileFreeListPtr}");
                for (int i=0; i<UWTileMap.current_tilemap.NoOfActiveMobiles;i++)
                {
                    var atSlot = UWTileMap.current_tilemap.GetActiveMobileAtIndex(i);
                    if (atSlot == obj.index)
                    {                                           
                        UWTileMap.current_tilemap.NoOfActiveMobiles--;
                        if (i<UWTileMap.current_tilemap.NoOfActiveMobiles)
                        {
                            var atEnd = UWTileMap.current_tilemap.GetActiveMobileAtIndex(UWTileMap.current_tilemap.NoOfActiveMobiles); 
                            //shift down the object at the end of this list
                            UWTileMap.current_tilemap.SetActiveMobileAtIndex(i, atEnd);
                        }
                        break;
                    }
                }
            }
            else
            {//static
                UWTileMap.current_tilemap.StaticFreeListObject = obj.index;
                UWTileMap.current_tilemap.StaticFreeListPtr++;
                Debug.Print($"Freeing Static {obj.index} Pointer incremented to {UWTileMap.current_tilemap.StaticFreeListPtr}");
            }

            if (obj.instance != null)
            {
                if (obj.instance.uwnode != null)
                {
                    obj.instance.uwnode.QueueFree();
                }
                obj.instance = null;
            }
            obj.item_id = 0;//set to default
           // obj.link = 0; obj.next = 0; //force remove from chains. Full updates to chains should have been done before calling this function
            obj.tileX=99; obj.tileY=99;
        }

        /// <summary>
        /// Process object list
        /// </summary>
        /// <param name="worldparent"></param>
        /// <param name="objects"></param>
        /// <param name="grObjects"></param>
        /// <param name="a_tilemap"></param>
        public static void GenerateObjects(uwObject[] objects, UWTileMap a_tilemap)
        {

           // npcs = new();
            for (int x = 0; x <= a_tilemap.Tiles.GetUpperBound(0); x++)
            {
                for (int y = 0; y <= a_tilemap.Tiles.GetUpperBound(0); y++)
                {
                    var next = a_tilemap.Tiles[x, y].indexObjectList;
                    while (next != 0)
                    {
                        var obj = a_tilemap.LevelObjects[next];
                        RenderObject(obj, a_tilemap);
                        next = obj.next;
                    }
                }
            }
            // foreach (var obj in objects)
            // {
            //     if (obj.item_id <= 463)
            //     {
            //         RenderObject(obj, a_tilemap);
            //     }
            // }
        }


        /// <summary>
        /// Adds an object instance to the tilemap
        /// </summary>
        /// <param name="worldparent"></param>
        /// <param name="grObjects"></param>
        /// <param name="obj"></param>
        /// <param name="a_tilemap"></param>
        public static void RenderObject(uwObject obj, UWTileMap a_tilemap)
        {
            bool unimplemented = true;
            var name = $"{obj.index}_{GameStrings.GetObjectNounUW(obj.item_id)}";
            var newNode = new Node3D();
            newNode.Name = name;
            newNode.Position = obj.GetCoordinate(obj.tileX, obj.tileY);
            worldobjects.AddChild(newNode);

            switch (obj.majorclass)
            {
                case 0://Weapons
                    break;
                case 1://npcs
                    {
                        if (obj.item_id < 127)
                        {
                            //npcs.Add(
                            npc.CreateInstance(newNode, obj, name);
                                //);
                            unimplemented = false;
                        }
                        break;
                    }
                case 2://misc items incl containers, food, and lights.                
                    break;
                case 3: // misc objects
                    {
                        unimplemented = MajorClass3(obj, newNode, grObjects, name);
                        break;
                    }
                case 4://keys, usables and readables
                    {
                        unimplemented = MajorClass4(obj, newNode, grObjects, name);
                        break;
                    }
                case 5: //doors, 3d models, buttons/switches
                    {
                        unimplemented = MajorClass5(obj, newNode, a_tilemap, name);
                        break;
                    }
                case 6://Traps and Triggers
                    {
                        unimplemented = MajorClass6(obj, newNode, a_tilemap, name);
                    }
                    break;
                case 7://Animos
                    {
                        unimplemented = MajorClass7(obj, newNode, a_tilemap, name);
                        break;
                    }
                default:
                    unimplemented = true; break;

            }
            if (unimplemented)
            {
                //just render a sprite.
                obj.instance = CreateSpriteInstance(grObjects, obj, newNode, $"{name}");
            }
            if (printlabels)
            {
                PrintObjectLabel(obj, name, newNode);
            }
            if (obj.instance != null)
            {
                obj.instance.uwnode = newNode;
            }
            else
            {
                Debug.Print($"{name} is null!");
            }
        }

        private static void PrintObjectLabel(uwObject obj, string name, Node3D newNode)
        {
            var collider = commonObjDat.ActivatedByCollision(obj.item_id);
            Label3D obj_lbl = new();
            obj_lbl.Text = $"{name} {obj.xpos},{obj.ypos},{obj.zpos} Activated by collision:{collider}";
            obj_lbl.Font = uimanager.instance.Font4X5P;
            obj_lbl.FontSize = 16;
            obj_lbl.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
            obj_lbl.Position = new Vector3(0f, 0.4f, 0f);
            newNode.AddChild(obj_lbl);
        }



        /// <summary>
        /// Runestones and some misc objects
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="parent"></param>
        /// <param name="grObjects"></param>
        /// <returns></returns>
        private static bool MajorClass3(uwObject obj, Node3D parent, GRLoader grObjects, string name)
        {
            if ((obj.minorclass == 2) && (obj.classindex == 0))
            {
                obj.instance = runestone.CreateInstance(parent, obj, grObjects, name);
                return false;
            }
            if (((obj.minorclass == 2) && (obj.classindex >= 8))
                || (obj.minorclass == 3)
                || ((obj.minorclass == 2) && (obj.classindex == 0)))
            {//runestones
                obj.instance = runestone.CreateInstance(parent, obj, grObjects, name);
                return false;
            }

            return true;
        }

        private static bool MajorClass4(uwObject obj, Node3D parent, GRLoader grObjects, string name)
        {
            switch (obj.minorclass)
            {
                case 2:
                    {
                        switch (obj.classindex)
                        {
                            case 9:
                                {
                                    if (_RES != GAME_UW2)
                                    {
                                        glowing_rock.CreateGlowingRock(grObjects, obj, parent, name);
                                        return false;
                                    }
                                    break;
                                }
                        }
                        break;
                    }
            }
            return true;
        }

        /// <summary>
        /// Doors, 3d models, buttons/switches
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="unimplemented"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private static bool MajorClass5(uwObject obj, Node3D parent, UWTileMap a_tilemap, string name)
        {
            if (obj.tileX >= 65) { return true; }//don't render offmap models.
            switch (obj.minorclass)
            {
                case 0: //doors
                    {
                        obj.instance = door.CreateInstance(parent, obj, a_tilemap, name);
                        doorway.CreateInstance(parent, obj, a_tilemap, name);
                        return false;
                    }
                case 1: //3D Models
                    {
                        if (obj.classindex == 0)
                        {//bench
                            obj.instance = bench.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if ((obj.classindex >= 3) && (obj.classindex <= 6))
                        {//boulders
                            obj.instance = boulder.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if (obj.classindex == 7)
                        {//shrine
                            obj.instance = shrine.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if (obj.classindex == 8)
                        {
                            obj.instance = table.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if (obj.classindex == 9)
                        {
                            obj.instance = beam.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if (obj.classindex == 0xA)
                        {
                            obj.instance = moongate.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if (obj.classindex == 0xB)
                        {
                            obj.instance = barrel.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if (obj.classindex == 0xC)
                        {
                            obj.instance = chair.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if (obj.classindex == 0xD)
                        {
                            obj.instance = chest.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if (obj.classindex == 0xE)
                        {
                            obj.instance = nightstand.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if (obj.classindex == 0xF)
                        {
                            obj.instance = lotus.CreateInstance(parent, obj, name);
                            return false;
                        }
                        break;
                    }
                case 2: //3D models
                    {
                        if (obj.classindex == 0)
                        {//pillar 352
                            obj.instance = pillar.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if ((obj.classindex == 1) || (obj.classindex == 2))
                        {//353 and 354, rotary switches
                            obj.instance = buttonrotary.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if ((_RES == GAME_UW2) && (obj.classindex == 3))
                        {  //or item id 163
                            obj.instance = painting.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if (obj.classindex == 4)
                        {//bridge 356
                            obj.instance = bridge.CreateInstance(parent, obj, name, a_tilemap);
                            return false;
                        }
                        if (obj.classindex == 5)
                        {//gravestone
                            obj.instance = gravestone.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if (obj.classindex == 6)
                        {//some_writing 358
                            obj.instance = writing.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if ((_RES == GAME_UW2) && (obj.classindex == 7))
                        {  //or item id 359
                            obj.instance = bed.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if ((_RES == GAME_UW2) && (obj.classindex == 8))
                        {  //or item id 360
                            obj.instance = largeblackrockgem.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if ((_RES == GAME_UW2) && (obj.classindex == 9))
                        {  //or item id 361
                            obj.instance = shelf.CreateInstance(parent, obj, name);
                            return false;
                        }
                        if ((obj.classindex == 0xE) || (obj.classindex == 0xF))
                        {//tmaps
                            obj.instance = tmap.CreateInstance(parent, obj, a_tilemap, name);
                            return false;
                        }
                        break;
                    }
                case 3:
                    {   //buttons
                        obj.instance = button.CreateInstance(parent, obj, name);
                        return false;
                    }
            }

            return true;
        }

        /// <summary>
        /// Traps and Triggers
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="parent"></param>
        /// <param name="a_tilemap"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool MajorClass6(uwObject obj, Node3D parent, UWTileMap a_tilemap, string name)
        {
            Debug.Print($"Trap {obj.a_name} {obj.index} {obj.tileX},{obj.tileY} Class {obj.majorclass}-{obj.minorclass}-{obj.classindex} Params[F:{obj.flags} Q:{obj.quality}, O:{obj.owner}] Link {obj.link}");
            switch (obj.minorclass)
            {
                case 0: //traps
                    {
                        switch (obj.classindex)
                        {
                            case 3://do trap 6-0-3
                                {
                                    return hack_trap.CreateDoTrap(parent, obj, name);
                                }
                        }
                        break;
                    }
                case 2:
                    {
                        switch (obj.classindex)
                        {
                            case 0://move trigger, 6-2-0
                                return trigger.CreateMoveTrigger(obj, parent);
                        }
                        break;
                    }
                case 3: //uw1 does not have class 6-3-xx
                    {
                        switch (obj.classindex)
                        {
                            case 0://move trigger 6-3-0
                                if (_RES == GAME_UW2)
                                {
                                    return trigger.CreateMoveTrigger(obj, parent);
                                }
                                break;
                        }
                        break;
                    }
            }
            return true;
        }



        /// <summary>
        /// Animos and moving doors
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="parent"></param>
        /// <param name="a_tilemap"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool MajorClass7(uwObject obj, Node3D parent, UWTileMap a_tilemap, string name)
        {//animos and the moving door
            //class 7 has only a single minor class. jump straight to the class index
            switch (obj.classindex)
            {
                case 0xF://moving door special case
                    obj.instance = door.CreateInstance(parent, obj, a_tilemap, name);
                    doorway.CreateInstance(parent, obj, a_tilemap, name);
                    return false;
                default:
                    obj.instance = animo.CreateInstance(parent, obj, name);
                    return false;
            }
        }

        public static genericsprite CreateSpriteInstance(GRLoader grObjects, uwObject obj, Node3D parent, string name)
        {
            var inst = new genericsprite(obj);
            if (obj.invis == 0)
            {
                CreateSprite(grObjects, obj.item_id, parent, name);
            }
            return inst;
        }

        public static void CreateSprite(GRLoader gr, int spriteNo, Node3D parent, string name, bool EnableCollision = true)
        {
            // var a_sprite = new Sprite3D();
            // a_sprite.Name = name;
            // var img = gr.LoadImageAt(spriteNo);
            // var NewSize = new Vector2(
            //             ArtLoader.SpriteScale * img.GetWidth(),
            //             ArtLoader.SpriteScale * img.GetHeight()
            //             );
            // a_sprite.Texture = gr.LoadImageAt(spriteNo);
            // a_sprite.MaterialOverride =  gr.GetMaterial(spriteNo);
            // parent.AddChild(a_sprite);
            // a_sprite.Position = new Vector3(0, NewSize.Y / 2, 0);            
            //res://src/utility/uwmeshinstance3d.gd
            var a_sprite = new uwMeshInstance3D(); //MeshInstance3D(); //new Sprite3D();
            a_sprite.Name = name;
            a_sprite.Mesh = new QuadMesh();
            Vector2 NewSize;
            var img = gr.LoadImageAt(spriteNo);
            if (img != null)
            {
                a_sprite.Mesh.SurfaceSetMaterial(0, gr.GetMaterial(spriteNo));
                NewSize = new Vector2(
                        ArtLoader.SpriteScale * img.GetWidth(),
                        ArtLoader.SpriteScale * img.GetHeight()
                        );
                a_sprite.Mesh.Set("size", NewSize);
                parent.AddChild(a_sprite);
                a_sprite.Position = new Vector3(0, NewSize.Y / 2f, 0);
                if (EnableCollision)
                {
                    a_sprite.CreateConvexCollision();
                }
            }
        }

        /// <summary>
        /// Removes or reduces the qty of the object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="UsedFromInventory"></param>
        public static void Consume(uwObject obj, bool UsedFromInventory)
        {
            if (obj.ObjectQuantity > 1)
            {
                obj.link--;
                if (UsedFromInventory)
                {
                    uimanager.UpdateInventoryDisplay();
                }
            }
            else
            {
                //Remove Object From Inventory or world
                if (UsedFromInventory)
                {
                    playerdat.RemoveFromInventory(obj.index);
                    uimanager.UpdateInventoryDisplay();
                }
                else
                {
                    if (playerdat.ObjectInHand == obj.index)
                    {//used from object in hnand
                        playerdat.ObjectInHand = -1;
                        uimanager.instance.mousecursor.SetCursorToCursor();
                        RemoveObject(obj);
                    }
                    else
                    {//used from a tile
                        DeleteObjectFromTile(obj.tileX, obj.tileY, obj.index);
                    }
                }
            }
        }



        /// <summary>
        /// Initialises the default critter properties
        /// </summary>
        /// <param name="critter"></param>
        public static void InitialiseCritter(uwObject critter)
        {
            critter.npc_xhome = 32;
            critter.npc_yhome = 32;
            critter.quality = 32;
            critter.owner = 32;
            critter.npc_hp = (byte)((0x10 + Rng.r.Next(0, 0x18)) / 0x20);
            critter.ProjectileHeading = (short)(critter.heading << 5);
            critter.npc_goal = 8;
            critter.npc_gtarg = 0;
            critter.TargetTileX = 0;
            critter.TargetTileY = 0;
            critter.TargetZHeight = 0;
            critter.UnkBit_0XD_Bit9 = 0;
            critter.IsPowerfull = 0;
            critter.UnkBit_0XD_Bit11 = 0;
            critter.UnkBit_0XD_Bit8 = 0;
            critter.UnkBit_0x18_5 = 0;//possbily used to indicate npc is at their target
            critter.Swing = 0;
            critter.UnkBit_0XA_Bit0123 = 0;
            critter.Projectile_Speed = 4;
            critter.npc_animation = 0;
            critter.AnimationFrame = 0;
            critter.npc_attack = 10;
            critter.UnkBit_0X13_Bit7 = 0;
            critter.UnkBit_0X13_Bit0to6 = 0;
            critter.AccumulatedDamage = 0;
            critter.ProjectileSourceID = 0;
            critter.UnkBit_0x18_7 = 0;
            critter.UnkBit_0x18_6 = 0;
            critter.UnkBits_0x16_0_F = 0;
            critter.UnkBit_0X15_Bit6 = 0;
            critter.npc_whoami = 0;
            critter.UnkBit_0x19_0_likelyincombat = 0;
            critter.UnkBit_0x19_4 = 0;
            critter.UnkBit_0x19_5 = 0;
            critter.UnkBit_0x19_6_MaybeAlly = 0;
            critter.UnkBit_0x19_7 = 0;
            critter.LootSpawnedFlag = 0;
            critter.npc_talkedto = 0;
            critter.npc_attitude = 2;
            critter.UnkBit_0XA_Bit7 = 0;//active?
            critter.npc_spellindex = 0;
            critter.UnkBit_0XA_Bit456 = 0;

        }


        /// <summary>
        /// Deletes the specified object from the tile (searches first level only, does not remove from containers)
        /// </summary>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        /// <param name="indexToDelete"></param>
        public static void DeleteObjectFromTile(int tileX, int tileY, short indexToDelete, bool RemoveFromWorld = true)
        {
            var objList = UWTileMap.current_tilemap.LevelObjects;
            if (indexToDelete != 0)
            {
                var tile = UWTileMap.current_tilemap.Tiles[tileX, tileY];
                var objectToDelete = objList[indexToDelete];
                if (objectToDelete != null)
                {
                    if (tile.indexObjectList == indexToDelete)
                    {
                        tile.indexObjectList = objectToDelete.next;
                        objectToDelete.next = 0;
                        if (RemoveFromWorld)
                        {
                            ObjectCreator.RemoveObject(objectToDelete);
                        }
                        return;
                    }
                    else
                    {
                        //search
                        var next = tile.indexObjectList;

                        while (next != 0)
                        {
                            var nextObject = objList[next];
                            if (nextObject.next == indexToDelete)
                            {
                                nextObject.next = objectToDelete.next;
                                objectToDelete.next = 0;
                                if (RemoveFromWorld)
                                {
                                    ObjectCreator.RemoveObject(objectToDelete);
                                }
                                return;
                            }
                            next = nextObject.next;
                        }
                        Debug.Print($"Was unable to find {indexToDelete} to delete it in {tileX},{tileY}");
                    }
                }
            }
        }


        /// <summary>
        /// Unlinks an object from the top level of a tile.
        /// </summary>
        /// <param name="ObjectToUnlink"></param>
        /// <returns></returns>
        public static bool UnlinkObjectFromTile(uwObject ObjectToUnlink)
        {
            //unlink trap from it's tile if needed
            var tile = UWTileMap.current_tilemap.Tiles[ObjectToUnlink.tileX, ObjectToUnlink.tileY];
            var previous = 0;
            var next = tile.indexObjectList;
            while (next != 0)
            {
                var nextObj = UWTileMap.current_tilemap.LevelObjects[next];
                if (next == ObjectToUnlink.index)
                {
                    if (previous == 0)
                    {
                        tile.indexObjectList = ObjectToUnlink.next;
                    }
                    else
                    {
                        var prevObject = UWTileMap.current_tilemap.LevelObjects[previous];
                        prevObject.next = ObjectToUnlink.next;
                        ObjectToUnlink.next = 0;
                    }
                    return true;
                }
                previous = nextObj.index;
                next = nextObj.next;
            }
            return false;//no unlinking
        }

        public static void RefreshSprite(uwObject objToRefresh)
        {//assumes sprite to sprite refresh
            if (objToRefresh.instance != null)
            {
                if (objToRefresh.instance.uwnode != null)
                {
                    var nd = (uwMeshInstance3D)objToRefresh.instance.uwnode.GetChild(0);
                    if (nd != null)
                    {
                        nd.Mesh.SurfaceSetMaterial(0, ObjectCreator.grObjects.GetMaterial(objToRefresh.item_id));
                    }
                }
            }
        }

        public static bool RemoveObjectFromLinkedList(int listhead, int toRemove, uwObject[] objlist)
        {
            //var obj = objlist[toRemove];
            var next = listhead;
            while (next!=0)
            {
                var nextObject = objlist[next];
                var headObject = objlist[listhead];
                if (nextObject.index == toRemove)
                {
                    headObject.next = nextObject.next;
                    nextObject.next = 0;
                    return true;
                }
                listhead = next;//move the listhead on.
                next = objlist[next].next;//get the next object
            }               

            return false;
        }
    } //end class
} //end namesace