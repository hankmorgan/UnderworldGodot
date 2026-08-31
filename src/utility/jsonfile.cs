using System;
using System.IO;
using System.Text.Json;

namespace Underworld;

/// <summary>Why a read did not produce a value.</summary>
public enum JsonReadOutcome
{
    Ok,
    /// <summary>There is no file at that path.</summary>
    NotFound,
    /// <summary>There is a file, and it could not be read or did not parse.</summary>
    Unreadable,
}

/// <summary>
/// Reading the small JSON documents the game keeps in its user directory.
///
/// This is deliberately free of Godot types so it can be unit tested. <see cref="uwsettings"/>
/// resolves its path through <c>ProjectSettings.GlobalizePath</c>, and touching that class at
/// all runs its static constructor, which a headless test process cannot do.
/// </summary>
public static class JsonFile
{
    /// <summary>
    /// Read and deserialise <paramref name="path"/>, reporting failure rather than throwing.
    ///
    /// The outcome distinguishes a missing file from one that is present and unusable, so the
    /// caller does not have to go back to the filesystem to find out which it was. Asking twice
    /// is a race: the file can appear, change or vanish in between, and the caller would then
    /// act on the wrong answer and move a perfectly good file aside.
    /// </summary>
    public static JsonReadOutcome TryRead<T>(
        string path, JsonSerializerOptions options, out T value, out string error)
    {
        value = default;
        error = null;

        FileStream stream;
        try
        {
            stream = File.OpenRead(path);
        }
        catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException)
        {
            error = "no such file";
            return JsonReadOutcome.NotFound;
        }
        catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
        {
            // Present but cannot be opened, e.g. permissions. Distinct from absent, and it must
            // not escape: the caller runs from a static constructor, where an exception faults
            // the type for the rest of the process.
            error = e.Message;
            return JsonReadOutcome.Unreadable;
        }

        try
        {
            using (stream)
            {
                value = JsonSerializer.Deserialize<T>(stream, options);
            }
            if (value == null)
            {
                // A document holding literal "null" parses and gives us nothing to use.
                error = "the document holds no object";
                return JsonReadOutcome.Unreadable;
            }
            return JsonReadOutcome.Ok;
        }
        catch (Exception e) when (e is JsonException || e is IOException || e is NotSupportedException)
        {
            error = e.Message;
            value = default;
            return JsonReadOutcome.Unreadable;
        }
    }

    /// <summary>
    /// Move a file that could not be read to one side, so it survives being replaced by the
    /// defaults that take over from it. Returns where it was put, or null if it could not be
    /// moved.
    /// </summary>
    public static string PreserveUnreadable(string path)
    {
        // Never overwrite a copy already kept. The first bad file is usually the one holding
        // the settings somebody actually wants back, and a second bad file arriving later
        // would otherwise destroy it. Number them instead, and stop rather than sprawl.
        const int MaxKept = 10;
        for (int n = 0; n < MaxKept; n++)
        {
            string kept = path + ".corrupt" + (n == 0 ? "" : "." + (n + 1));
            try
            {
                // overwrite: false, so an existing copy makes this throw and we try the next
                // name rather than replacing it.
                File.Move(path, kept);
                return kept;
            }
            catch (IOException) when (File.Exists(kept) || Directory.Exists(kept))
            {
                // Something is already at that name, so try the next one. File.Exists alone
                // reports false for a directory, which would end the search on the first slot.
                continue;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                return null;
            }
        }
        return null;
    }
}
