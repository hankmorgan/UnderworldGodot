namespace Underworld.Sfx;

/// <summary>
/// One trigger dispatched from the game thread to the audio producer thread.
/// Carries the patch reference (immutable, safely shared) and the per-trigger
/// lifetime in 60 Hz service ticks (-1 = infinite).
/// </summary>
public readonly record struct SfxCommand(TvfxPatch Patch, int LifetimeTicks);
