namespace Underworld.Save.Tests;

/// <summary>
/// Covers <see cref="LevArkLoader.UW1BlockLength"/>, which measures a block in a UW1 LEV.ARK
/// from the offset table because the format carries no per-block length.
///
/// Reported on PR #71. Two callers measured against a subset of the blocks and both ran past
/// the real neighbour. The writer searched forward from blockNo + 1 and took the first offset larger
/// than this one, which assumes blocks sit in the file in index order. The note reader looked
/// only at the nine note blocks. DOS writes blocks in whatever order it likes, and the block
/// that physically follows one is often of another type. Notes are counted as Data.Length / 54,
/// so the surplus was drawn as note records along the bottom of the automap.
/// </summary>
public class SourceBlockLengthTests
{
    /// <summary>
    /// Build a UW1 LEV.ARK header: Int16 block count, then an Int32 offset per block.
    /// Offset 0 means the block is absent.
    /// </summary>
    private static byte[] Archive(int fileLength, params (int block, int offset)[] blocks)
    {
        int count = 135;
        var d = new byte[fileLength];
        d[0] = (byte)(count & 0xFF);
        d[1] = (byte)(count >> 8);
        foreach (var (b, o) in blocks)
        {
            d[2 + b * 4 + 0] = (byte)(o & 0xFF);
            d[2 + b * 4 + 1] = (byte)((o >> 8) & 0xFF);
            d[2 + b * 4 + 2] = (byte)((o >> 16) & 0xFF);
            d[2 + b * 4 + 3] = (byte)((o >> 24) & 0xFF);
        }
        return d;
    }

    [Fact]
    public void ABlockIsMeasuredToTheNextBlockByPosition_NotByIndex()
    {
        // Block 36 sits between 28 and 29 in the file, which is the shape that breaks a
        // forward-only search: the next block physically is 29, a *lower* index.
        var src = Archive(4000, (28, 1000), (36, 2000), (29, 2200), (30, 3000));

        Assert.Equal(1000, LevArkLoader.UW1BlockLength(src, 28, 135));
        Assert.Equal(200, LevArkLoader.UW1BlockLength(src, 36, 135));
        Assert.Equal(800, LevArkLoader.UW1BlockLength(src, 29, 135));
    }

    [Fact]
    public void TheBlockOrderFromTheReportedSave_MeasuresEveryBlockCorrectly()
    {
        // The real tail order from the DOS save attached to PR #71: 28, 36, 29, 30, 39, 31, 38.
        // Every block is 100 bytes long, laid out back to back, so each answer must be 100.
        var order = new[] { 28, 36, 29, 30, 39, 31, 38 };
        var placed = new (int, int)[order.Length];
        for (int k = 0; k < order.Length; k++) { placed[k] = (order[k], 1000 + k * 100); }
        var src = Archive(1000 + order.Length * 100, placed);

        foreach (int b in order)
        {
            Assert.Equal(100, LevArkLoader.UW1BlockLength(src, b, 135));
        }
    }

    [Fact]
    public void TheOldForwardOnlySearchWouldHaveOvershot()
    {
        // Pins the defect itself rather than only the fix. Searching forward from 37 for the
        // first offset larger than block 36's would skip 29 and 30 and land on 39, giving 300
        // instead of 100. If this ever equals 300 again the bug is back.
        var src = Archive(1400, (28, 1000), (36, 1100), (29, 1200), (30, 1300), (39, 1400));

        // 300 is what the forward-by-index scan returned, so equality with 100 is the check.
        Assert.Equal(100, LevArkLoader.UW1BlockLength(src, 36, 135));
    }

    [Fact]
    public void TheLastBlockByPositionRunsToTheEndOfTheFile()
    {
        // Block 28 has the higher offset, so it is the last one by position and runs to the
        // end of the file. Block 36 runs up to it, even though 28 is the lower index.
        var src = Archive(5000, (36, 1000), (28, 4000));
        Assert.Equal(3000, LevArkLoader.UW1BlockLength(src, 36, 135));
        Assert.Equal(1000, LevArkLoader.UW1BlockLength(src, 28, 135));
    }

    [Fact]
    public void AnAbsentBlockHasNoLength()
    {
        // Offset 0 records an absent block. It is not a block sitting at position 0, so it
        // must not be treated as the neighbour of anything either.
        var src = Archive(3000, (28, 1000), (36, 0), (29, 2000));

        Assert.Equal(0, LevArkLoader.UW1BlockLength(src, 36, 135));
        Assert.Equal(1000, LevArkLoader.UW1BlockLength(src, 28, 135));
        Assert.Equal(1000, LevArkLoader.UW1BlockLength(src, 29, 135));
    }

    [Fact]
    public void TwoBlocksSharingAnOffsetBothRunToTheNextDistinctOne()
    {
        // The offset table cannot express anything else, so both get the same extent.
        var src = Archive(3000, (28, 1000), (29, 1000), (30, 2000));

        Assert.Equal(1000, LevArkLoader.UW1BlockLength(src, 28, 135));
        Assert.Equal(1000, LevArkLoader.UW1BlockLength(src, 29, 135));
    }

    [Fact]
    public void AMalformedTableIsMeasuredAsNothingRatherThanThrowing()
    {
        // The block count comes from the first two bytes of the file, so none of this is
        // trustworthy input. Measuring nothing is recoverable; an exception out of a loader is
        // not, and a negative length would reach an array allocation.
        Assert.Equal(0, LevArkLoader.UW1BlockLength(null, 36, 135));
        Assert.Equal(0, LevArkLoader.UW1BlockLength(Archive(3000, (36, 1000)), -1, 135));
        Assert.Equal(0, LevArkLoader.UW1BlockLength(Archive(3000, (36, 1000)), 135, 135));

        // A count larger than the file can hold.
        var stub = new byte[20];
        stub[0] = 135;
        Assert.Equal(0, LevArkLoader.UW1BlockLength(stub, 3, 135));

        // An offset inside the header, and one past the end of the file.
        Assert.Equal(0, LevArkLoader.UW1BlockLength(Archive(3000, (36, 10)), 36, 135));
        Assert.Equal(0, LevArkLoader.UW1BlockLength(Archive(3000, (36, 99999)), 36, 135));
    }

    [Fact]
    public void ANeighbourOffsetPastTheEndOfTheFileIsIgnored()
    {
        // Otherwise a single bad entry elsewhere in the table silently truncates a good block.
        var src = Archive(4000, (28, 1000), (29, 99999), (30, 3000));
        Assert.Equal(2000, LevArkLoader.UW1BlockLength(src, 28, 135));
    }

    [Fact]
    public void OffsetsOutsideTheCallerSuppliedBoundAreNotConsultedAsNeighbours()
    {
        // headerBlocks is supplied by the caller and bounds how much of the table is read.
        // Anything beyond it is not part of the table being measured and must not shorten a
        // real block. The header here always declares 135; this pins the bound, not the header.
        var src = Archive(4000, (28, 1000), (40, 2000), (50, 1500));

        Assert.Equal(1000, LevArkLoader.UW1BlockLength(src, 28, 45));
        Assert.Equal(500, LevArkLoader.UW1BlockLength(src, 28, 135));
    }
}
