namespace Underworld.Sfx;

/// <summary>
/// One trigger dispatched from the game thread to the audio producer thread.
/// Carries the patch reference (immutable, safely shared), the per-trigger
/// lifetime in 60 Hz service ticks (-1 = infinite), and a composite volume
/// scale byte (0..127) computed via <see cref="TvfxVelocity.ComputeVolScale"/>.
/// The consumer (<see cref="TvfxVoice.EmitRegisters"/>) multiplies the
/// carrier's TVFX-computed linear volume by VolScale/127 before inverting
/// to OPL Total Level. VolScale=127 is the identity (no extra attenuation).
/// </summary>
public readonly record struct SfxCommand(TvfxPatch Patch, int LifetimeTicks, byte VolScale);
