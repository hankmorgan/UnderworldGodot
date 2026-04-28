using System;
using System.IO;

namespace Underworld.Save.Tests;

/// <summary>
/// Test fixtures and path resolution for save-game tests.
/// </summary>
public static class TestData
{
    /// <summary>
    /// Resolves the repository root by walking up from AppContext.BaseDirectory.
    /// Uses sentinel-based lookup: stops at the first directory containing Underworld.csproj.
    /// </summary>
    public static string RepoRoot
    {
        get
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current != null)
            {
                if (File.Exists(Path.Combine(current.FullName, "Underworld.csproj")))
                {
                    return current.FullName;
                }
                current = current.Parent;
            }

            throw new InvalidOperationException(
                $"Could not locate repo root (Underworld.csproj) walking up from {AppContext.BaseDirectory}");
        }
    }

    /// <summary>
    /// GOG game data root: resolved as RepoRoot/../UW2GOG.
    /// </summary>
    public static string UW2GogRoot => Path.GetFullPath(Path.Combine(RepoRoot, "..", "UW2GOG"));

    /// <summary>
    /// Returns the path to a file in UW2/SAVE0 (the first save slot).
    /// </summary>
    public static string Uw2Save0(string filename) => Path.Combine(UW2GogRoot, "UW2", "SAVE0", filename);

    /// <summary>
    /// Returns the path to a file in UW1/DATA.
    /// </summary>
    public static string Uw1Data(string filename) => Path.Combine(UW2GogRoot, "UW1", "DATA", filename);
}
