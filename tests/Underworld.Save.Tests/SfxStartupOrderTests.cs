using System;
using System.IO;

namespace Underworld.Save.Tests;

/// <summary>
/// Pins the order of work in <c>SfxStreamPlayer._Ready</c>. See issue #79.
///
/// Two orderings matter, at opposite ends.
///
/// The sound directory must be resolved before the player and the producer thread exist.
/// <c>Path.Combine</c> throws when no game path is configured, and a throw after the generator
/// was playing and the thread was running left both going against a construction that had
/// already failed. The chip and the singleton are created earlier still and are taken down by
/// <c>_ExitTree</c>, so this is about what gets started, not about leaks.
///
/// <c>SoundEffects.Initialize</c> must come last, because it is what makes this node reachable
/// from <c>SoundEffects.Play</c>. Publishing it earlier would mean a failure while building the
/// player or starting the thread left callers able to enqueue against a producer that never
/// ran, which is the same defect the other way round.
///
/// This reads the file as text rather than running the node, because constructing it needs a
/// Godot scene tree and an audio device, neither of which exists in a headless test process.
/// It is therefore a check on the source and not on behaviour, which is worth knowing when it
/// fails: the fix is to restore the ordering, not to satisfy the string match.
/// </summary>
public class SfxStartupOrderTests
{
    private static string Source => File.ReadAllText(Path.Combine(
        TestData.RepoRoot, "src", "audio", "sfx", "godot", "SfxStreamPlayer.cs"));

    private static int PositionOf(string source, string needle)
    {
        int at = source.IndexOf(needle, StringComparison.Ordinal);
        Assert.True(at >= 0, $"SfxStreamPlayer.cs no longer contains \"{needle}\"");
        return at;
    }

    [Fact]
    public void TheSoundDirectoryIsResolvedBeforeAnythingIsStarted()
    {
        string s = Source;

        int combine = PositionOf(s, "Path.Combine(UWClass.BasePath");
        int play = PositionOf(s, "_player.Play()");
        int startThread = PositionOf(s, "_audioThread.Start()");

        Assert.True(combine < play,
            "the sound directory must be resolved before the generator plays, or a failure to "
            + "resolve it leaves audio playing that nothing asked to start (#79)");
        Assert.True(combine < startThread,
            "the sound directory must be resolved before the producer thread starts (#79)");
    }

    [Fact]
    public void AMissingGamePathIsRefusedRatherThanThrown()
    {
        string s = Source;

        int guard = PositionOf(s, "string.IsNullOrWhiteSpace(UWClass.BasePath)");
        int combine = PositionOf(s, "Path.Combine(UWClass.BasePath");

        Assert.True(guard < combine,
            "BasePath must be checked before Path.Combine uses it, which throws on null (#79)");
    }

    [Fact]
    public void TheBackendIsPublishedOnlyOnceThereIsSomethingBehindIt()
    {
        string s = Source;

        int initialise = PositionOf(s, "SoundEffects.Initialize(uwsettings");
        int play = PositionOf(s, "_player.Play()");
        int startThread = PositionOf(s, "_audioThread.Start()");

        Assert.True(initialise > play,
            "SoundEffects.Initialize makes this node reachable from SoundEffects.Play, so it "
            + "must come after the player exists");
        Assert.True(initialise > startThread,
            "publishing before the producer thread starts would let callers enqueue against a "
            + "producer that never ran");
    }
}
