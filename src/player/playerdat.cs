using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace Underworld
{
    /// <summary>
    /// Class for all operations relating to player.dat
    /// </summary>
    public class playerdat : Loader
    {
        public static uwObject[] InventoryObjects = new uwObject[512];
        public static byte[] Inventorybuffer = new byte[512 * 8];
        private static byte[] pdat;

        /// <summary>
        /// Size of encrypted area in UW1 player.dat
        /// </summary>
        private const int NoOfEncryptedBytes = 0xD2;

        // Func load pdat
        public static void Load(string folder)
        {
            var path = System.IO.Path.Combine(BasePath, folder, "PLAYER.DAT");
            byte[] encoded;
            if (ReadStreamFile(path, out encoded))
            {
                int xOrValue = (int)encoded[0];
                switch (_RES)
                {
                    case GAME_UW2:
                        {
                            //uw2 encoding                                            
                            pdat = EncryptDecryptUW2(encoded, (byte)xOrValue);
                            break;
                        }
                    default:
                        {
                            //uw1 decoding
                            pdat = EncryptDecryptUW1(encoded, xOrValue);
                            break;
                        }
                }

                //Copy and initialise inventory
                var InventoryPtr = 0x138;
                if (_RES == GAME_UW2)
                {
                    InventoryPtr = 0x3E3;
                }
                int oIndex = 1; //starts at one since there is no object zero
                Inventorybuffer = new byte[512*8];
                
                while (InventoryPtr < pdat.GetUpperBound(0))
                {
                    for (int i =0; i<8; i++)
                    {//Copy bytes into storage
                        Inventorybuffer[i + oIndex * 8 ] = pdat[InventoryPtr + i];
                    }
                    //Create new objects for the object list
                    var uwobj = new uwObject
                    {
                        isInventory = true,
                        IsStatic = true,
                        index = (short)(oIndex),
                        PTR = oIndex * 8,
                        DataBuffer = Inventorybuffer
                    };
                    Debug.Print($"{GameStrings.GetObjectNounUW(uwobj.item_id)}");
                    InventoryObjects[oIndex] = uwobj;
                    oIndex++;
                    InventoryPtr += 8;
                    
                }
            }
        }   //end load

        /// <summary>
        /// Encrypts or decrypts a UW1 player dat file.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="xOrValue"></param>
        public static byte[] EncryptDecryptUW1(byte[] buffer, int xOrValue)
        {
            int incrnum = 3;
            var output = buffer.ToArray();
            for (int i = 1; i <= NoOfEncryptedBytes; i++)
            {
                if ((i == 81) | (i == 161))
                {
                    incrnum = 3;
                }
                output[i] ^= (byte)((xOrValue + incrnum) & 0xFF);
                incrnum += 3;
            }
            return output;
        } //end decrypt uw1

        // Func decode/encode UW2
        /// <summary>
        /// Encrypts or decrypts a UW2 player dat file.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="xOrValue"></param>
        public static byte[] EncryptDecryptUW2(byte[] pDat, byte MS)
        {
            int[] MA = new int[80];
            MS += 7;
            for (int i = 0; i < 80; ++i)
            {
                MS += 6;
                MA[i] = MS;
            }
            for (int i = 0; i < 16; ++i)
            {
                MS += 7;
                MA[i * 5] = MS;
            }
            for (int i = 0; i < 4; ++i)
            {
                MS += 0x29;
                MA[i * 12] = MS;
            }
            for (int i = 0; i < 11; ++i)
            {
                MS += 0x49;
                MA[i * 7] = MS;
            }
            byte[] buffer = new byte[pDat.GetUpperBound(0) + 1];
            int offset = 1;
            int byteCounter = 0;
            for (int l = 0; l <= 11; l++)
            {
                buffer[0 + offset] = (byte)(pDat[0 + offset] ^ MA[0]);
                byteCounter++;
                for (int i = 1; i < 0x50; ++i)
                {
                    if (byteCounter < 0x37D)
                    {
                        buffer[i + offset] = (byte)(((pDat[i + offset] & 0xff) ^ ((buffer[i - 1 + offset] & 0xff) + (pDat[i - 1 + offset] & 0xff) + (MA[i] & 0xff))) & 0xff);
                        byteCounter++;
                    }
                }
                offset += 80;
            }
            //Copy the remainder of the plaintext data
            while (byteCounter <= pDat.GetUpperBound(0))
            {
                buffer[byteCounter] = pDat[byteCounter];
                byteCounter++;
            }
            buffer[0] = pDat[0];
            return buffer;
        } //end decrypt uw2
        public static byte GetAt(int index)
        {
            return pdat[index];
        }
        public static int GetAt16(int index)
        {
            return (int)DataLoader.getAt(pdat, index, 16);
        }
        public static int GetAt32(int index)
        {
            return (int)DataLoader.getAt(pdat, index, 32);
        }
        public static void SetAt(int index, byte value)
        {
            pdat[index] = value;
        }
        public static void SetAt16(int index, int value)
        {
            DataLoader.setAt(pdat, index, 16, value);
        }
        public static void SetAt32(int index, int value)
        {
            DataLoader.setAt(pdat, index, 32, value);
        }
        public static string CharName
        {
            get
            {
                var _charname = "";
                for (int i = 1; i < 14; i++)
                {
                    var alpha = GetAt(i);
                    if (alpha.ToString() != "\0")
                    {
                        _charname += (char)alpha;
                    }
                }
                return _charname;
            }
            set
            {
                var _chararray = value.ToCharArray();
                for (int i = 1; i < 14; i++)
                {
                    if (i - 1 < value.Length)
                    {
                        SetAt(i, (byte)_chararray[i - 1]);
                    }
                    else
                    {
                        SetAt(i, (byte)0);
                    }
                }
            }
        }
        public static int Body
        {
            get
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                return (GetAt(offset) >> 2) & 0x7;
            }
            set
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                byte existingValue = GetAt(offset);
                existingValue = (byte)(existingValue & 0xE3);
                value = value << 2;
                existingValue = (byte)(existingValue | value);
                SetAt(offset, existingValue);
            }
        }
        public static int CharClass
        {
            get
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                return (GetAt(offset) >> 5) & 0x7;
            }
            set
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                byte existingValue = GetAt(offset);
                existingValue = (byte)(existingValue & 0x1F); //mask out charclass
                value = value << 5;
                existingValue = (byte)(existingValue | value);
                SetAt(offset, existingValue);
            }
        }
        public static int CharLevel
        {
            get { return GetAt(0x3E); }
            set { SetAt(0x3E, (byte)value); }
        }
        public static int Exp
        {
            get
            {
                return (int)GetAt32(0x4F) / 10;
            }
            set
            {
                SetAt32(0x4F, value * 10);
            }
        }
        public static int TrainingPoints
        {
            get { return GetAt(0x53); }
            set { SetAt(0x53, (byte)value); }
        }
        public static bool isFemale
        {
            get
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                return ((int)(GetAt(offset) >> 1) & 0x1) == 0x1;
            }
            set
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                byte existingValue = GetAt(offset);
                byte mask = (1 << 1);
                if (value)
                {//set
                    existingValue |= mask;
                }
                else
                {//unset
                    existingValue = (byte)(existingValue & (~mask));
                }
                SetAt(offset, existingValue);
            }
        }
        public static bool isLefty
        {
            get
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                return ((int)(GetAt(offset)) & 0x1) == 0x0;
            }
            set
            {
                int offset = 0x65;
                if (_RES == GAME_UW2) { offset = 0x66; }
                byte existingValue = GetAt(offset);
                byte mask = (1);
                if (value)
                {//set
                    existingValue |= mask;
                }
                else
                {//unset
                    existingValue = (byte)(existingValue & (~mask));
                }
                SetAt(offset, existingValue);
            }
        }

        //Location Data
        public static int dungeon_level
        {
            get
            {
                return GetAt(0x5D);
            }
            set
            {
                SetAt(0x53, (byte)value);
            }
        }
        //Character attributes
        public static int X
        {
            get
            {
                return GetAt16(0x55);
            }
        }

        public static int tileX
        {
            get
            {
                return X >> 8;
            }
        }

        public static int Y
        {
            get
            {
                return GetAt16(0x57);
            }
        }

        public static int xpos
        {
            get
            {
                return X & 0x7;// need to confirm if correct
            }
        }
        public static int tileY
        {
            get
            {
                return Y >> 8;
            }
        }

        public static int ypos
        {
            get
            {
                return Y & 0x7;// need to confirm if correct
            }
        }

        public static int Z
        {
            get
            {
                return GetAt16(0x59);
            }
        }

        public static int zpos
        {
            get
            {
                return Z >> 3;
            }
        }

        public static int camerazpos
        {
            get
            {
                return zpos + commonObjDat.height(127);
            }
        }

        public static int heading
        {
            get
            {
                return GetAt(0x5C);
            }
        }

        public static int STR
        {
            get
            {
                int value = (int)GetAt(0x1F);
                return value;
            }
            set
            {
                SetAt(0x1F, (byte)(value));
            }
        }
        public static int DEX
        {
            get
            {
                return (int)GetAt(0x20);
            }
            set
            {
                SetAt(0x20, (byte)(value));
            }
        }
        public static int INT
        {
            get
            {
                return (int)GetAt(0x21);
            }
            set
            {
                SetAt(0x21, (byte)(value));
            }
        }

        //Character skills
        public static int Attack
        {
            get
            {
                return (int)GetAt(0x22);
            }
            set
            {
                SetAt(0x22, (byte)(value));
            }
        }
        public static int Defense
        {
            get
            {
                return (int)GetAt(0x23);
            }
            set
            {
                SetAt(0x23, (byte)(value));
            }
        }
        public static int Unarmed
        {
            get
            {
                return (int)GetAt(0x24);
            }
            set
            {
                SetAt(0x24, (byte)(value));
            }
        }
        public static int Sword
        {
            get
            {
                return (int)GetAt(0x25);
            }
            set
            {
                SetAt(0x25, (byte)(value));
            }
        }
        public static int Axe
        {
            get
            {
                return (int)GetAt(0x26);
            }
            set
            {
                SetAt(0x26, (byte)(value));
            }
        }
        public static int Mace
        {
            get
            {
                return (int)GetAt(0x27);
            }
            set
            {
                SetAt(0x27, (byte)(value));
            }
        }
        public static int Missile
        {
            get
            {
                return (int)GetAt(0x28);
            }
            set
            {
                SetAt(0x28, (byte)(value));
            }
        }
        public static int ManaSkill
        {
            get
            {
                return (int)GetAt(0x29);
            }
            set
            {
                SetAt(0x29, (byte)(value));
            }
        }
        public static int Lore
        {
            get
            {
                return (int)GetAt(0x2A);
            }
            set
            {
                SetAt(0x2A, (byte)(value));
            }
        }
        public static int Casting
        {
            get
            {
                return (int)GetAt(0x2B);
            }
            set
            {
                SetAt(0x2B, (byte)(value));
            }
        }
        public static int Traps
        {
            get
            {
                return (int)GetAt(0x2C);
            }
            set
            {
                SetAt(0x2C, (byte)(value));
            }
        }
        public static int Search
        {
            get
            {
                return (int)GetAt(0x2D);
            }
            set
            {
                SetAt(0x2D, (byte)(value));
            }
        }
        public static int Track
        {
            get
            {
                return (int)GetAt(0x2E);
            }
            set
            {
                SetAt(0x2E, (byte)(value));
            }
        }
        public static int Sneak
        {
            get
            {
                return (int)GetAt(0x2F);
            }
            set
            {
                SetAt(0x2F, (byte)(value));
            }
        }
        public static int Repair
        {
            get
            {
                return (int)GetAt(0x30);
            }
            set
            {
                SetAt(0x30, (byte)(value));
            }
        }
        public static int Charm
        {
            get
            {
                return (int)GetAt(0x31);
            }
            set
            {
                SetAt(0x31, (byte)(value));
            }
        }
        public static int PickLock
        {
            get
            {
                return (int)GetAt(0x32);
            }
            set
            {
                SetAt(0x32, (byte)(value));
            }
        }
        public static int Acrobat
        {
            get
            {
                return (int)GetAt(0x33);
            }
            set
            {
                SetAt(0x33, (byte)(value));
            }
        }
        public static int Appraise
        {
            get
            {
                return (int)GetAt(0x34);
            }
            set
            {
                SetAt(0x34, (byte)(value));
            }
        }
        public static int Swimming
        {
            get
            {
                return (int)GetAt(0x35);
            }
            set
            {
                SetAt(0x35, (byte)(value));
            }
        }


        //Inventory

        /// <summary>
        /// Object index for the item at the helm slot
        /// </summary>
        public static int Helm
        {
            get
            {
                switch(_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3A3)>>6;
                    default:
                        return GetAt16(0xF8)>>6;
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
                if (Helm!=0)
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
                switch(_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3A5)>>6;
                    default:
                        return GetAt16(0xFA)>>6;
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
                if (ChestArmour!=0)
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
                switch(_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3A7)>>6;
                    default:
                        return GetAt16(0xFC)>>6;
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
                if (Gloves!=0)
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
                switch(_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3A9)>>6;
                    default:
                        return GetAt16(0xFE)>>6;
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
                if (Leggings!=0)
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
                switch(_RES)
                {
                    case GAME_UW2:
                        return GetAt16(0x3AB)>>6;
                    default:
                        return GetAt16(0x100)>>6;
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
                if (Boots!=0)
                {
                    return InventoryObjects[Boots];
                }
                return null;
            }
        }



    } //end class

}//end namespace