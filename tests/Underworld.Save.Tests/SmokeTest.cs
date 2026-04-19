using System;
using System.IO;

namespace Underworld.Save.Tests;

/// <summary>
/// Smoke test to verify test infrastructure and fixture accessibility.
/// </summary>
public class SmokeTest
{
    /// <summary>
    /// Verify that BGLOBALS.DAT exists at the expected UW2/SAVE0 path.
    /// </summary>
    [Fact]
    public void Uw2Save0BGlobalsExists()
    {
        var path = TestData.Uw2Save0("BGLOBALS.DAT");
        Assert.True(File.Exists(path),
            $"BGLOBALS.DAT not found at {path}. RepoRoot resolved to: {TestData.RepoRoot}");
    }
}
