using System;
using System.IO;

namespace Underworld.Sfx.Tests;

/// <summary>
/// Helper for locating UW1 test fixtures (SOUNDS.DAT, UW.AD). These files are
/// copyright-protected game data and not committed to the repo — users must
/// provide them locally (see fixtures/uw1/README.md). Tests that need them
/// should early-return via <see cref="SkipIfUnavailable"/> when absent.
/// </summary>
internal static class Fixtures
{
    public static string Uw1Dir =>
        Path.Combine(AppContext.BaseDirectory, "fixtures", "uw1");

    public static string SoundsDat => Path.Combine(Uw1Dir, "SOUNDS.DAT");
    public static string UwAd      => Path.Combine(Uw1Dir, "UW.AD");

    public static bool Available => File.Exists(SoundsDat) && File.Exists(UwAd);

    /// <summary>
    /// Call at the top of a test that requires UW1 fixtures. Returns true if
    /// they're present (continue with test). Returns false + writes a clear
    /// console message if absent (caller should return early). Tests pass
    /// vacuously when skipped — we don't fail the build for missing game
    /// data, since upstream CI likely doesn't have it either.
    /// </summary>
    public static bool SkipIfUnavailable()
    {
        if (Available) return true;
        Console.WriteLine(
            $"SKIP: UW1 fixtures not found at {Uw1Dir}. " +
            $"Copy SOUNDS.DAT and UW.AD there to enable this test. " +
            $"See fixtures/uw1/README.md.");
        return false;
    }
}
