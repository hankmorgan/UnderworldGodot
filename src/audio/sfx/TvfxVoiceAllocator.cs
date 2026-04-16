namespace Underworld.Sfx;

/// <summary>
/// 9-voice pool, one TvfxVoice per OPL2 channel. Allocate() returns the first
/// Idle voice (or null when the chip is saturated — match the authentic
/// "drop trigger" behaviour rather than voice-stealing, which would produce
/// crackle when many SFX overlap).
/// </summary>
public sealed class TvfxVoiceAllocator
{
    private readonly TvfxVoice[] _voices = new TvfxVoice[9];

    public TvfxVoiceAllocator()
    {
        for (int i = 0; i < 9; i++) _voices[i] = new TvfxVoice(i);
    }

    /// <summary>First Idle voice, or null when all 9 are busy.</summary>
    public TvfxVoice? Allocate()
    {
        foreach (var v in _voices)
            if (v.Phase == TvfxPhase.Idle) return v;
        return null;
    }

    /// <summary>
    /// Service every non-Idle voice for one 60 Hz tick: advance its stream VM,
    /// then emit its current register state. Voices that go Idle this tick are
    /// still emitted once so a final KeyOff-equivalent reaches the chip.
    /// </summary>
    public void ServiceAll(IOplRegisterSink sink)
    {
        foreach (var v in _voices)
        {
            if (v.Phase == TvfxPhase.Idle) continue;
            v.ServiceTick();
            v.EmitRegisters(sink);
        }
    }

    /// <summary>Total active (non-Idle) voices. Used by tests and diagnostics.</summary>
    public int ActiveCount
    {
        get
        {
            int n = 0;
            foreach (var v in _voices) if (v.Phase != TvfxPhase.Idle) n++;
            return n;
        }
    }

    /// <summary>Read access to a specific voice slot (mainly for tests).</summary>
    public TvfxVoice Voice(int channel) => _voices[channel];
}
