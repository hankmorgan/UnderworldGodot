using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    /// <summary>
    /// Class for creating instances of objects and calling updates
    /// </summary>
    public class ObjectCreator : UWClass
    {
        //List of active NPCs
        public static List<npc> npcs;
        public static bool printlabels = true;

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
        /// Spawns an object directly in hand
        /// </summary>
        /// <param name="itemid"></param>
        /// <returns></returns>
        public static int SpawnObjectInHand(int itemid, bool changeInteractionmode = true)
        {
            var slot = ObjectCreator.PrepareNewObject(itemid);
            var obj = UWTileMap.current_tilemap.LevelObjects[slot];
            playerdat.ObjectInHand = slot;
            uimanager.instance.mousecursor.SetCursorArt(obj.item_id);
            if (changeInteractionmode)
            {
                uimanager.InteractionModeToggle(uimanager.InteractionModes.ModePickup);
            }
            return slot;
        }

        /// <summary>
        /// Allocates data for a new object
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

                if (obj.majorclass == 1) //NPC
                {
                    //TODO INITIALISE CRITTER
                }
            }
            return slot;
        }


        public static int GetAvailableObjectSlot(ObjectListType WhichList = ObjectListType.StaticList)
        {
            //look up object free list
            switch (WhichList)
            {
                case ObjectListType.StaticList:
                    //Move PTR down, get object at that point.
                    UWTileMap.current_tilemap.StaticFreeListPtr--;
                    Debug.Print($"Allocating {UWTileMap.current_tilemap.StaticFreeListObject} Pointer decremented to {UWTileMap.current_tilemap.StaticFreeListPtr}");
                    return UWTileMap.current_tilemap.StaticFreeListObject;
                case ObjectListType.MobileList:
                    return 0; //TODO
            }
            return 0;
        }

        /// <summary>
        /// Removes object from the game world
        /// </summary>
        /// <param name="obj"></param>
        public static void RemoveObject(uwObject obj)
        {
            //remove from world
            UWTileMap.current_tilemap.StaticFreeListObject = obj.index;
            UWTileMap.current_tilemap.StaticFreeListPtr++;
            Debug.Print($"Freeing {obj.index} Pointer incremented to {UWTileMap.current_tilemap.StaticFreeListPtr}");

            if (obj.instance != null)
            {
                if (obj.instance.uwnode != null)
                {
                    obj.instance.uwnode.QueueFree();

                }
                obj.instance = null;
            }
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
            npcs = new();
            foreach (var obj in objects)
            {
                if (obj.item_id <= 463)
                {
                    RenderObject(obj, a_tilemap);
                }
            }
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
                case 1://npcs
                    {
                        if (obj.item_id < 127)
                        {
                            npcs.Add(npc.CreateInstance(newNode, obj, name));
                            unimplemented = false;
                        }
                        break;
                    }
                case 3: // misc objects
                    {
                        unimplemented = MajorClass3(obj, newNode, grObjects, name);
                        break;
                    }
                case 5: //doors, 3d models, buttons/switches
                    {
                        unimplemented = MajorClass5(obj, newNode, a_tilemap, name);
                        break;
                    }

                case 0://Weapons
                case 2://misc items incl containers, food, and lights.                
                case 4://keys, usables and readables
                    break;
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
            switch (obj.minorclass)
            {
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
                                if (_RES==GAME_UW2)
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
                        uimanager.instance.mousecursor.ResetCursor();
                    }
                    ObjectCreator.RemoveObject(obj);
                }
            }
        }

    } //end class
} //end namesace