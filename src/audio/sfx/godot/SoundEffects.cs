using System;
using System.IO;
using Godot;

namespace Underworld.Sfx;

/// <summary>
/// Game-facing SFX entry point. Game code calls
/// <see cref="Play(int, byte, byte)"/>; the façade resolves the sound id,
/// dispatches to the active backend, and silently no-ops if SFX failed to
/// initialise (missing data files, unsupported synth choice).
///
/// v1 ships only the TVFX/OPL backend. <c>synth=cm32l|mt32|soundfont</c> users
/// see a one-time warning at startup; SFX is silent for them until the MT-32
/// backend lands in a follow-up.
/// </summary>
public static class SoundEffects
{
    private static SoundEntry[] _sounds = Array.Empty<SoundEntry>();
    private static ISfxBackend _backend;

    /// <summary>
    /// Boot-time setup. Call once after <see cref="SfxStreamPlayer.Instance"/>
    /// is in the scene tree and after <c>uwsettings.instance</c> is loaded.
    /// Caller is responsible for synth selection match: pass the same string
    /// that selected the music synth.
    /// </summary>
    /// <param name="synth">music-synth string from uwsettings — "opl" routes to TVFX, others log + no-op.</param>
    /// <param name="uw1SoundDir">absolute path to UW1's SOUND/ directory.</param>
    public static void Initialize(string synth, string uw1SoundDir)
    {
        _sounds = Array.Empty<SoundEntry>();
        _backend = null;

        // Always load SOUNDS.DAT — the file is small and the trigger lookups
        // need it regardless of which backend is active. If it's missing,
        // disable SFX entirely.
        var soundsPath = Path.Combine(uw1SoundDir, "SOUNDS.DAT");
        if (!File.Exists(soundsPath))
        {
            GD.PushWarning($"SoundEffects: SOUNDS.DAT not found at {soundsPath}; SFX disabled.");
            return;
        }
        try { _sounds = SoundsDatLoader.Load(soundsPath); }
        catch (Exception e)
        {
            GD.PushWarning($"SoundEffects: SOUNDS.DAT parse failed: {e.Message}; SFX disabled.");
            return;
        }

        // Backend selection. Lower-cased synth string per uwsettings convention.
        string s = synth?.ToLowerInvariant() ?? "soundfont";
        switch (s)
        {
            case "opl":
                _backend = TryCreateOplBackend(uw1SoundDir);
                break;
            case "cm32l":
            case "mt32":
            case "soundfont":
                GD.PushWarning(
                    $"SoundEffects: synth='{s}' uses MT-32 SFX, which is not yet implemented. " +
                    "v1 ships only the OPL backend. Set synth=opl in uwsettings.json for SFX, " +
                    "or wait for the MT-32 backend follow-up.");
                break;
            default:
                GD.PushWarning($"SoundEffects: unknown synth='{s}'; SFX disabled.");
                break;
        }
    }

    private static ISfxBackend TryCreateOplBackend(string uw1SoundDir)
    {
        var bankPath = Path.Combine(uw1SoundDir, "UW.AD");
        if (!File.Exists(bankPath))
        {
            GD.PushWarning($"SoundEffects: UW.AD not found at {bankPath}; OPL SFX disabled.");
            return null;
        }
        var player = SfxStreamPlayer.Instance;
        if (player == null)
        {
            GD.PushError("SoundEffects: SfxStreamPlayer.Instance is null. " +
                         "Add SfxStreamPlayer to the scene tree before calling SoundEffects.Initialize.");
            return null;
        }
        try
        {
            var bank = TvfxPatchBank.Load(bankPath);
            return new TvfxSfxBackend(bank, player);
        }
        catch (Exception e)
        {
            GD.PushWarning($"SoundEffects: UW.AD load failed: {e.Message}; OPL SFX disabled.");
            return null;
        }
    }

    /// <summary>
    /// Trigger a sound effect. <paramref name="soundId"/> is the SOUNDS.DAT
    /// entry index (UW1 has 24 entries, 0..23). Out-of-range ids are dropped
    /// with a one-line warning. Pan is a standard MIDI 0..0x7F (OPL backend
    /// ignores it; future MT-32 backend honours it).
    /// </summary>
    public static void Play(int soundId, byte pan = 0x40, byte velocityOffset = 0)
    {
        if (_backend == null) return;
        if ((uint)soundId >= (uint)_sounds.Length)
        {
            GD.PushWarning($"SoundEffects.Play: id {soundId} out of range (have {_sounds.Length} entries)");
            return;
        }
        _backend.Play(_sounds[soundId], pan, velocityOffset);
    }

    /// <summary>Tear-down for orderly shutdown. Does not destroy the
    /// <see cref="SfxStreamPlayer"/> node — that is managed by the scene tree.</summary>
    public static void Shutdown()
    {
        _backend?.Dispose();
        _backend = null;
        _sounds = Array.Empty<SoundEntry>();
    }
}
