using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Underworld.Save.Tests;

/// <summary>
/// The LEV.ARK object free lists are stacks. DOS reads the entry at the top and then
/// lowers the pointer, and on release raises the pointer and then writes. The stored
/// words at 0x7C02 and 0x7C04 are the top index, so entries 0..top are free and an
/// empty list is -1 (0xFFFF).
///
/// The partition helpers below read the raw block bytes rather than going through
/// uwObject, so they stay independent of the accessors under test.
/// </summary>
[Collection("UWClassState")]
public class ObjectFreeListTests : IDisposable
{
    private const int MobileFreeListBase = 0x7300;
    private const int StaticFreeListBase = 0x74FC;
    private const int ActiveMobilesBase = 0x7AFC;
    private const int MobileTopOffset = 0x7C02;
    private const int StaticTopOffset = 0x7C04;
    private const int MobileCapacity = 254;   // objects 2..255
    private const int StaticCapacity = 768;   // objects 256..1023

    private readonly string _origBasePath;
    private readonly byte _origRes;
    private readonly byte[] _origLevArkFileData;
    private readonly UWTileMap[] _origDungeons;
    private readonly UWTileMap _origCurrent;

    public ObjectFreeListTests()
    {
        _origBasePath = UWClass.BasePath;
        _origRes = UWClass._RES;
        _origLevArkFileData = LevArkLoader.lev_ark_file_data;
        _origDungeons = UWTileMap.dungeons;
        _origCurrent = UWTileMap.current_tilemap;
    }

    public void Dispose()
    {
        UWClass.BasePath = _origBasePath;
        UWClass._RES = _origRes;
        LevArkLoader.lev_ark_file_data = _origLevArkFileData;
        UWTileMap.dungeons = _origDungeons;
        UWTileMap.current_tilemap = _origCurrent;
    }

    private static UWTileMap LoadLevel(byte res, string folder, string gameDir, int level)
    {
        UWClass.BasePath = Path.Combine(TestData.UW2GogRoot, gameDir);
        UWClass._RES = res;
        LevArkLoader.LoadLevArkFileData(folder: folder);
        UWTileMap.dungeons = new UWTileMap[UWTileMap.NO_OF_LEVELS];
        var tm = new UWTileMap(level);
        UWTileMap.dungeons[level] = tm;
        UWTileMap.current_tilemap = tm;
        return tm;
    }

    private static int Word(byte[] b, int offset) => b[offset] | (b[offset + 1] << 8);

    private static int ObjectPtr(int index) =>
        index < 256 ? 0x4000 + index * 27 : 0x4000 + 256 * 27 + (index - 256) * 8;

    /// <summary>
    /// Builds an object bound to the level block, the same way BuildObjectListUW does.
    /// </summary>
    private static uwObject ObjectAt(UWTileMap tm, int index) => new uwObject
    {
        IsStatic = index >= 256,
        index = (short)index,
        PTR = ObjectPtr(index),
        DataBuffer = tm.lev_ark_block.Data
    };

    /// <summary>
    /// Objects reachable from any tile chain, plus everything linked inside a container.
    /// </summary>
    private static HashSet<int> Allocated(byte[] b)
    {
        var reached = new HashSet<int>();
        var pending = new Stack<int>();

        for (int ty = 0; ty < 64; ty++)
        {
            for (int tx = 0; tx < 64; tx++)
            {
                int i = (Word(b, (ty * 64 + tx) * 4 + 2) >> 6) & 0x3FF;
                int guard = 0;
                while (i != 0 && guard++ < 2048)
                {
                    if (!reached.Add(i)) break;
                    pending.Push(i);
                    i = (Word(b, ObjectPtr(i) + 4) >> 6) & 0x3FF;
                }
            }
        }

        while (pending.Count > 0)
        {
            int i = pending.Pop();
            bool isQuantity = (Word(b, ObjectPtr(i)) & 0x8000) != 0;
            if (isQuantity) continue;   // the link field is a quantity, not a child pointer

            int child = (Word(b, ObjectPtr(i) + 6) >> 6) & 0x3FF;
            int guard = 0;
            while (child != 0 && guard++ < 2048)
            {
                if (!reached.Add(child)) break;
                pending.Push(child);
                child = (Word(b, ObjectPtr(child) + 4) >> 6) & 0x3FF;
            }
        }

        return reached;
    }

    private static HashSet<int> FreeListed(byte[] b, bool inclusive)
    {
        var free = new HashSet<int>();
        int mobileCount = Word(b, MobileTopOffset) + (inclusive ? 1 : 0);
        int staticCount = Word(b, StaticTopOffset) + (inclusive ? 1 : 0);

        for (int i = 0; i < mobileCount; i++) free.Add(Word(b, MobileFreeListBase + i * 2));
        for (int i = 0; i < staticCount; i++) free.Add(Word(b, StaticFreeListBase + i * 2));
        return free;
    }

    /// <summary>
    /// Every object slot must be either in use or free, never both and never neither.
    /// Object 0 is the null object and object 1 is always the avatar, so both are skipped.
    /// </summary>
    private static (int both, int neither) PartitionFaults(byte[] b, bool inclusive)
    {
        var allocated = Allocated(b);
        var free = FreeListed(b, inclusive);
        int both = 0, neither = 0;

        for (int i = 2; i < 1024; i++)
        {
            bool a = allocated.Contains(i);
            bool f = free.Contains(i);
            if (a && f) both++;
            else if (!a && !f) neither++;
        }

        return (both, neither);
    }

    [Fact]
    public void Uw1ShippedLevArk_EveryLevelPartitionsExactly_UnderInclusiveReading()
    {
        for (int level = 0; level < 9; level++)
        {
            var tm = LoadLevel(UWClass.GAME_UW1, "DATA", "UW1", level);
            var (both, neither) = PartitionFaults(tm.lev_ark_block.Data, inclusive: true);

            Assert.True(both == 0 && neither == 0,
                $"UW1 level {level + 1}: expected a clean partition under the inclusive " +
                $"reading, got {both} objects both in use and free, {neither} in neither list");
        }
    }

    [Fact]
    public void Uw1ShippedLevArk_ExclusiveReadingStrandsTheTopEntryOfEachList()
    {
        // Guards the test above: the exclusive reading the port used before this change
        // drops the top entry of both lists, so each level orphans exactly two objects.
        for (int level = 0; level < 9; level++)
        {
            var tm = LoadLevel(UWClass.GAME_UW1, "DATA", "UW1", level);
            var (both, neither) = PartitionFaults(tm.lev_ark_block.Data, inclusive: false);

            Assert.Equal(0, both);
            Assert.Equal(2, neither);
        }
    }

    [Fact]
    public void Uw2LevArk_PartitionsExactly_UnderInclusiveReading()
    {
        var tm = LoadLevel(UWClass.GAME_UW2, "SAVE0", "UW2", 0);
        var (both, neither) = PartitionFaults(tm.lev_ark_block.Data, inclusive: true);

        Assert.True(both == 0 && neither == 0,
            $"UW2 level 1: {both} objects both in use and free, {neither} in neither list");
    }

    [Fact]
    public void Allocate_ReturnsEntryAtTop_ThenLowersPointer()
    {
        var tm = LoadLevel(UWClass.GAME_UW1, "DATA", "UW1", 0);
        int top = tm.StaticFreeListPtr;
        int expected = Word(tm.lev_ark_block.Data, StaticFreeListBase + top * 2);

        int got = ObjectFreeLists.GetAvailableObjectSlot(ObjectFreeLists.ObjectListType.StaticList);

        Assert.Equal(expected, got);
        Assert.Equal(top - 1, tm.StaticFreeListPtr);
    }

    [Fact]
    public void Release_RaisesPointer_ThenWritesEntry()
    {
        var tm = LoadLevel(UWClass.GAME_UW1, "DATA", "UW1", 0);
        int top = tm.StaticFreeListPtr;
        int entryAtOldTop = Word(tm.lev_ark_block.Data, StaticFreeListBase + top * 2);

        int slot = ObjectFreeLists.GetAvailableObjectSlot(ObjectFreeLists.ObjectListType.StaticList);
        ObjectFreeLists.ReleaseFreeObject(ObjectAt(tm, slot));

        Assert.Equal(top, tm.StaticFreeListPtr);
        // The entry that was live at the old top must survive untouched.
        Assert.Equal(entryAtOldTop, Word(tm.lev_ark_block.Data, StaticFreeListBase + top * 2));
    }

    [Fact]
    public void ReleaseThenAllocate_ReturnsTheSameObject()
    {
        var tm = LoadLevel(UWClass.GAME_UW1, "DATA", "UW1", 0);
        const int Slot = 900;

        ObjectFreeLists.ReleaseFreeObject(ObjectAt(tm, Slot));
        int got = ObjectFreeLists.GetAvailableObjectSlot(ObjectFreeLists.ObjectListType.StaticList);

        Assert.Equal(Slot, got);
    }

    [Fact]
    public void Allocate_WhenListIsEmpty_ReturnsZero()
    {
        var tm = LoadLevel(UWClass.GAME_UW1, "DATA", "UW1", 0);
        tm.StaticFreeListPtr = -1;
        tm.MobileFreeListPtr = -1;

        Assert.Equal(0, ObjectFreeLists.GetAvailableObjectSlot(ObjectFreeLists.ObjectListType.StaticList));
        Assert.Equal(0, ObjectFreeLists.GetAvailableObjectSlot(ObjectFreeLists.ObjectListType.MobileList));
        Assert.Equal(-1, tm.StaticFreeListPtr);
        Assert.Equal(-1, tm.MobileFreeListPtr);
    }

    [Fact]
    public void EmptyList_IsStoredAs0xFFFF_AndReadsBackAsMinusOne()
    {
        var tm = LoadLevel(UWClass.GAME_UW1, "DATA", "UW1", 0);
        tm.StaticFreeListPtr = -1;
        tm.MobileFreeListPtr = -1;

        Assert.Equal(0xFFFF, Word(tm.lev_ark_block.Data, StaticTopOffset));
        Assert.Equal(0xFFFF, Word(tm.lev_ark_block.Data, MobileTopOffset));
        Assert.Equal(-1, tm.StaticFreeListPtr);
        Assert.Equal(-1, tm.MobileFreeListPtr);
    }

    [Fact]
    public void Allocate_AtTopZero_SucceedsAndLeavesTheListEmpty()
    {
        var tm = LoadLevel(UWClass.GAME_UW1, "DATA", "UW1", 0);
        tm.StaticFreeListPtr = 0;
        int expected = Word(tm.lev_ark_block.Data, StaticFreeListBase);

        int got = ObjectFreeLists.GetAvailableObjectSlot(ObjectFreeLists.ObjectListType.StaticList);

        Assert.Equal(expected, got);
        Assert.Equal(-1, tm.StaticFreeListPtr);
    }

    [Fact]
    public void ResetMap_RebuildsBothFreeListsToFullCapacity()
    {
        var tm = LoadLevel(UWClass.GAME_UW1, "DATA", "UW1", 0);
        for (int x = 0; x < tm.Tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tm.Tiles.GetLength(1); y++)
            {
                tm.Tiles[x, y] = new TileInfo(tm, (short)x, (short)y);
            }
        }
        tm.LevelObjects = new uwObject[1024];
        for (int i = 0; i < 1024; i++)
        {
            tm.LevelObjects[i] = ObjectAt(tm, i);
        }

        UWTileMap.ResetMap(0);

        // Releasing objects 2..1023 into two empty lists must fill each list exactly.
        Assert.Equal(MobileCapacity - 1, tm.MobileFreeListPtr);
        Assert.Equal(StaticCapacity - 1, tm.StaticFreeListPtr);

        // Released in ascending order, so the entries run in ascending order too.
        Assert.Equal(2, Word(tm.lev_ark_block.Data, MobileFreeListBase));
        Assert.Equal(255, Word(tm.lev_ark_block.Data, MobileFreeListBase + (MobileCapacity - 1) * 2));
        Assert.Equal(256, Word(tm.lev_ark_block.Data, StaticFreeListBase));
        Assert.Equal(1023, Word(tm.lev_ark_block.Data, StaticFreeListBase + (StaticCapacity - 1) * 2));
    }

    [Fact]
    public void FillingTheMobileListFromEmpty_StopsAtCapacity_WithoutSpillingIntoStatic()
    {
        var tm = LoadLevel(UWClass.GAME_UW1, "DATA", "UW1", 0);
        tm.MobileFreeListPtr = -1;
        int staticEntry0Before = Word(tm.lev_ark_block.Data, StaticFreeListBase);

        for (int i = 2; i < 256; i++)
        {
            ObjectFreeLists.ReleaseFreeObject(ObjectAt(tm, i));
        }

        Assert.Equal(MobileCapacity - 1, tm.MobileFreeListPtr);
        Assert.Equal(staticEntry0Before, Word(tm.lev_ark_block.Data, StaticFreeListBase));
    }

    [Fact]
    public void FillingTheStaticListFromEmpty_StopsAtCapacity_WithoutSpillingIntoActiveMobiles()
    {
        var tm = LoadLevel(UWClass.GAME_UW1, "DATA", "UW1", 0);
        tm.StaticFreeListPtr = -1;
        int activeMobilesBefore = Word(tm.lev_ark_block.Data, ActiveMobilesBase);

        for (int i = 256; i < 1024; i++)
        {
            ObjectFreeLists.ReleaseFreeObject(ObjectAt(tm, i));
        }

        Assert.Equal(StaticCapacity - 1, tm.StaticFreeListPtr);
        Assert.Equal(activeMobilesBefore, Word(tm.lev_ark_block.Data, ActiveMobilesBase));
    }
}
