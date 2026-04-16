using System.Collections.Generic;
using Underworld.Sfx;
using Underworld.Sfx.Tests;

namespace Underworld.Sfx.Tests;

public class TvfxVoiceAllocatorTests
{
    private sealed class NullSink : IOplRegisterSink
    {
        public void WriteReg(int addr, byte val) { }
    }

    [Fact]
    public void Initial_state_is_nine_idle_voices()
    {
        var a = new TvfxVoiceAllocator();
        Assert.Equal(0, a.ActiveCount);
        for (int ch = 0; ch < 9; ch++) Assert.Equal(TvfxPhase.Idle, a.Voice(ch).Phase);
    }

    [Fact]
    public void Allocate_returns_distinct_channels_until_saturated()
    {
        var a = new TvfxVoiceAllocator();
        var p = SyntheticPatch.Build(0xFFFF, 0x8000);

        var seen = new HashSet<int>();
        for (int i = 0; i < 9; i++)
        {
            var v = a.Allocate();
            Assert.NotNull(v);
            v!.StartKeyon(p, lifetimeTicks: -1);
            seen.Add(v.Channel);
        }
        Assert.Equal(9, seen.Count);
        Assert.Null(a.Allocate());        // saturated → drop
    }

    [Fact]
    public void Idle_voices_are_reallocatable()
    {
        var a = new TvfxVoiceAllocator();
        // Allocate without StartKeyon: voice stays Idle, so subsequent Allocate
        // returns the same slot. Proves the allocator keys off Phase, not on
        // a separate "checked-out" flag.
        var v1 = a.Allocate();
        var v2 = a.Allocate();
        Assert.NotNull(v1); Assert.NotNull(v2);
        Assert.Equal(v1!.Channel, v2!.Channel);
    }

    [Fact]
    public void ServiceAll_skips_idle_voices()
    {
        var a = new TvfxVoiceAllocator();
        var p = SyntheticPatch.Build(0xFFFF, 0x8000);

        // Activate 3 voices.
        for (int i = 0; i < 3; i++) a.Allocate()!.StartKeyon(p, lifetimeTicks: -1);

        var calls = 0;
        var sink = new CountingSink(() => calls++);
        a.ServiceAll(sink);

        // Each active voice emits 13 register writes per tick.
        Assert.Equal(3 * 13, calls);
    }

    [Fact]
    public void ServiceAll_progresses_voices_toward_idle()
    {
        var a = new TvfxVoiceAllocator();
        var p = SyntheticPatch.Build(0xFFFF, 0x8000);
        var v = a.Allocate()!;
        v.StartKeyon(p, lifetimeTicks: 2);

        var sink = new NullSink();
        a.ServiceAll(sink);                 // lifetime 2 → 1
        a.ServiceAll(sink);                 // lifetime 1 → 0, voice goes Idle
        Assert.Equal(0, a.ActiveCount);
    }

    private sealed class CountingSink : IOplRegisterSink
    {
        private readonly System.Action _onWrite;
        public CountingSink(System.Action onWrite) { _onWrite = onWrite; }
        public void WriteReg(int addr, byte val) => _onWrite();
    }
}
