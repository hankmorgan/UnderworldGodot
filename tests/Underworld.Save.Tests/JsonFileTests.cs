using System;
using System.IO;
using System.Text.Json;

namespace Underworld.Save.Tests;

/// <summary>
/// Covers <see cref="JsonFile"/>, which is how the settings file is read.
///
/// Issue #77 reported that a settings file could be left holding the tail of an older, longer
/// document, and 1f46059f fixed the write that caused it. What is covered here is the other
/// half: a settings file that cannot be read for any reason used to take the game down with it,
/// because LoadSettings runs from a static constructor and nothing caught the failure. That is
/// still reachable for a file corrupted by an earlier build, and for a write interrupted between
/// truncating the file and filling it.
/// </summary>
public class JsonFileTests
{
    private static readonly JsonSerializerOptions Opts = new() { WriteIndented = true };

    private sealed class Settings
    {
        public string game { get; set; } = "";
        public string synth { get; set; } = "";
    }

    private static string TempDir()
    {
        string d = Path.Combine(Path.GetTempPath(), "uwjson-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(d);
        return d;
    }

    [Fact]
    public void TryRead_WithATailFromAnOlderDocument_SaysItIsUnreadable()
    {
        string dir = TempDir();
        try
        {
            string path = Path.Combine(dir, "settings.json");
            // Exactly the shape #77 produced: a complete document with stale bytes after it.
            File.WriteAllText(path, "{\n  \"game\": \"UW1\"\n}: \"\"\n}\n");

            Assert.Equal(JsonReadOutcome.Unreadable,
                JsonFile.TryRead<Settings>(path, Opts, out var value, out string error));
            Assert.Null(value);
            Assert.False(string.IsNullOrEmpty(error));
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void TryRead_WithATruncatedDocument_SaysItIsUnreadable()
    {
        string dir = TempDir();
        try
        {
            // What a write interrupted after truncating but before finishing leaves behind.
            string path = Path.Combine(dir, "settings.json");
            File.WriteAllText(path, "{\n  \"game\": \"UW");

            Assert.Equal(JsonReadOutcome.Unreadable,
                JsonFile.TryRead<Settings>(path, Opts, out var value, out _));
            Assert.Null(value);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void TryRead_WithNoFile_SaysNotFoundRatherThanUnreadable()
    {
        string dir = TempDir();
        try
        {
            // The caller acts differently on the two, so they must not be conflated: one loads
            // defaults quietly, the other moves the file aside and warns.
            Assert.Equal(JsonReadOutcome.NotFound, JsonFile.TryRead<Settings>(
                Path.Combine(dir, "absent.json"), Opts, out var value, out _));
            Assert.Null(value);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void TryRead_WhenTheFileCannotBeOpened_SaysUnreadableRatherThanThrowing()
    {
        string dir = TempDir();
        try
        {
            // A directory where the file should be. Opening it fails in the same way a
            // permissions problem does, and the point is that nothing escapes: the caller runs
            // from a static constructor, where an exception faults the type for the process.
            string path = Path.Combine(dir, "settings.json");
            Directory.CreateDirectory(path);

            Assert.Equal(JsonReadOutcome.Unreadable,
                JsonFile.TryRead<Settings>(path, Opts, out var value, out string error));
            Assert.Null(value);
            Assert.False(string.IsNullOrEmpty(error));
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void TryRead_WithALiteralNull_SaysUnreadable()
    {
        string dir = TempDir();
        try
        {
            string path = Path.Combine(dir, "settings.json");
            File.WriteAllText(path, "null");
            Assert.Equal(JsonReadOutcome.Unreadable,
                JsonFile.TryRead<Settings>(path, Opts, out var value, out _));
            Assert.Null(value);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void TryRead_WithAGoodDocument_ReturnsIt()
    {
        string dir = TempDir();
        try
        {
            string path = Path.Combine(dir, "settings.json");
            File.WriteAllText(path, JsonSerializer.Serialize(
                new Settings { game = "UW2", synth = "mt32" }, Opts));

            Assert.Equal(JsonReadOutcome.Ok,
                JsonFile.TryRead<Settings>(path, Opts, out var value, out string error));
            Assert.Null(error);
            Assert.Equal("UW2", value.game);
            Assert.Equal("mt32", value.synth);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void PreserveUnreadable_MovesTheFileAsideAndReportsWhere()
    {
        string dir = TempDir();
        try
        {
            string path = Path.Combine(dir, "settings.json");
            File.WriteAllText(path, "not json at all");

            string kept = JsonFile.PreserveUnreadable(path);

            Assert.Equal(path + ".corrupt", kept);
            Assert.False(File.Exists(path));
            Assert.Equal("not json at all", File.ReadAllText(kept));
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void PreserveUnreadable_DoesNotDestroyACopyItAlreadyKept()
    {
        string dir = TempDir();
        try
        {
            string path = Path.Combine(dir, "settings.json");

            File.WriteAllText(path, "the first bad one");
            string first = JsonFile.PreserveUnreadable(path);

            File.WriteAllText(path, "the second bad one");
            string second = JsonFile.PreserveUnreadable(path);

            // The first copy is usually the one holding settings somebody wants back. A second
            // bad file arriving later must not overwrite it.
            Assert.NotEqual(first, second);
            Assert.Equal("the first bad one", File.ReadAllText(first));
            Assert.Equal("the second bad one", File.ReadAllText(second));
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void PreserveUnreadable_StopsRatherThanKeepingCopiesForever()
    {
        string dir = TempDir();
        try
        {
            string path = Path.Combine(dir, "settings.json");
            for (int i = 0; i < 10; i++)
            {
                File.WriteAllText(path, "bad " + i);
                Assert.NotNull(JsonFile.PreserveUnreadable(path));
            }

            // Eleventh. The caller uses the null to say the file is still in place.
            File.WriteAllText(path, "one too many");
            Assert.Null(JsonFile.PreserveUnreadable(path));
            Assert.Equal("one too many", File.ReadAllText(path));
            Assert.Equal(10, Directory.GetFiles(dir, "*.corrupt*").Length);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void PreserveUnreadable_WithNothingToMove_ReportsFailureRatherThanThrowing()
    {
        string dir = TempDir();
        try
        {
            Assert.Null(JsonFile.PreserveUnreadable(Path.Combine(dir, "absent.json")));
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void PreserveUnreadable_StepsPastADirectorySittingOnTheName()
    {
        string dir = TempDir();
        try
        {
            string path = Path.Combine(dir, "settings.json");
            File.WriteAllText(path, "the bad one");
            // A directory where the first copy would go. File.Exists reports false for one, so
            // a collision check that only asks File.Exists gives up here instead of trying the
            // next name, and the bad file is left to be overwritten by the next save.
            Directory.CreateDirectory(path + ".corrupt");

            string kept = JsonFile.PreserveUnreadable(path);

            Assert.NotNull(kept);
            Assert.Equal(path + ".corrupt.2", kept);
            Assert.Equal("the bad one", File.ReadAllText(kept));
            Assert.False(File.Exists(path));
        }
        finally { Directory.Delete(dir, true); }
    }
}
