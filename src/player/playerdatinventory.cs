using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Godot;

namespace Underworld
{
    //Player dat inventory
    public partial class playerdat : Loader
    {
        public static uwObject[] InventoryObjects = new uwObject[512];
        public static byte[] Inventorybuffer = new byte[512 * 8];

        /// <summary>
        /// Looks up all inventory slots list indices. starting at helm and ending with backpacks
        /// </summary>
        /// <param name="slot"></param>
        public static int GetInventorySlotListHead(int slot)
        {
            int startOffset = 0xF8;
            if (_RES==GAME_UW2){
                startOffset = 0x3A3;
            }
            return GetAt16(startOffset + slot * 2);
        }

        public static void SetInventorySlotListHead(int slot, int value)
        {
            int startOffset = 0xF8;
            if (_RES==GAME_UW2){
                startOffset = 0x3A3;
            }
            SetAt16(startOffset + slot * 2, value);
        }

        /// <summary>
        /// Object index for the item at the helm slot
        /// </summary>
        public static int Helm
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3A3) >> 6;
                    default:
                        return GetAt16(0xF8) >> 6;
                }
            }
            set
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        SetAt16(0x3A3 , value << 6);break;
                    default:
                        SetAt16(0xF8 , value << 6);break;
                }
            }
        }


        /// <summary>
        /// Direct reference to the object at the helm slot
        /// </summary>
        public static uwObject HelmObject
        {
            get
            {
                if (Helm != 0)
                {
                    return InventoryObjects[Helm];
                }
                return null;
            }
        }

        public static int ChestArmour
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3A5) >> 6;
                    default:
                        return GetAt16(0xFA) >> 6;
                }
            }
        }


        /// <summary>
        /// Direct reference to the object at the chest armour slot
        /// </summary>
        public static uwObject ChestArmourObject
        {
            get
            {
                if (ChestArmour != 0)
                {
                    return InventoryObjects[ChestArmour];
                }
                return null;
            }
        }

        public static int Gloves
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3A7) >> 6;
                    default:
                        return GetAt16(0xFC) >> 6;
                }
            }
        }


        /// <summary>
        /// Direct reference to the object at the gloves slot
        /// </summary>
        public static uwObject GlovesObject
        {
            get
            {
                if (Gloves != 0)
                {
                    return InventoryObjects[Gloves];
                }
                return null;
            }
        }

        public static int Leggings
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3A9) >> 6;
                    default:
                        return GetAt16(0xFE) >> 6;
                }
            }
        }


        /// <summary>
        /// Direct reference to the object at the leggings slot
        /// </summary>
        public static uwObject LeggingsObject
        {
            get
            {
                if (Leggings != 0)
                {
                    return InventoryObjects[Leggings];
                }
                return null;
            }
        }

        public static int Boots
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3AB) >> 6;
                    default:
                        return GetAt16(0x100) >> 6;
                }
            }
        }


        /// <summary>
        /// Direct reference to the object at the boots slot
        /// </summary>
        public static uwObject BootsObject
        {
            get
            {
                if (Boots != 0)
                {
                    return InventoryObjects[Boots];
                }
                return null;
            }
        }


        public static int RightShoulder
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3AD) >> 6;
                    default:
                        return GetAt16(0x102) >> 6;
                }
            }
        }


        /// <summary>
        /// Direct reference to the object at the boots slot
        /// </summary>
        public static uwObject RightShoulderObject
        {
            get
            {
                if (RightShoulder != 0)
                {
                    return InventoryObjects[RightShoulder];
                }
                return null;
            }
        }

        public static int LeftShoulder
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3AF) >> 6;
                    default:
                        return GetAt16(0x104) >> 6;
                }
            }
        }

        /// <summary>
        /// Direct reference to the object at the boots slot
        /// </summary>
        public static uwObject LeftShoulderObject
        {
            get
            {
                if (LeftShoulder != 0)
                {
                    return InventoryObjects[LeftShoulder];
                }
                return null;
            }
        }

        public static int RightHand
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3B1) >> 6;
                    default:
                        return GetAt16(0x106) >> 6;
                }
            }
        }

        /// <summary>
        /// Direct reference to the object at the boots slot
        /// </summary>
        public static uwObject RightHandObject
        {
            get
            {
                if (RightHand != 0)
                {
                    return InventoryObjects[RightHand];
                }
                return null;
            }
        }

        public static int LeftHand
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3B3) >> 6;
                    default:
                        return GetAt16(0x108) >> 6;
                }
            }
        }

        /// <summary>
        /// Direct reference to the object at the boots slot
        /// </summary>
        public static uwObject LeftHandObject
        {
            get
            {
                if (LeftHand != 0)
                {
                    return InventoryObjects[LeftHand];
                }
                return null;
            }
        }


        public static int RightRing
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3B5) >> 6;
                    default:
                        return GetAt16(0x10A) >> 6;
                }
            }
        }

        public static uwObject RightRingObject
        {
            get
            {
                if (RightRing != 0)
                {
                    return InventoryObjects[RightRing];
                }
                return null;
            }
        }



        public static int LeftRing
        {
            get
            {
                switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3B7) >> 6;
                    default:
                        return GetAt16(0x10C) >> 6;
                }
            }
        }

        
        public static uwObject LeftRingObject
        {
            get
            {
                if (LeftRing != 0)
                {
                    return InventoryObjects[LeftRing];
                }
                return null;
            }
        }

        public static int BackPack(int slot)
        {
            switch (_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3B9 + slot * 2) >> 6;
                    default:
                        return GetAt16(0x10E + slot * 2) >> 6;
                }
        }

        public static uwObject BackPackObject(int slot)
        {
            if (BackPack(slot) != 0)
            {
                return InventoryObjects[BackPack(slot)];
            }
            return null;
        }
        
    } //end class
} //end namespace