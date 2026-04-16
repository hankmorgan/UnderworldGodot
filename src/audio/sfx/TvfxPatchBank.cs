using System;
using System.Collections.Generic;
using System.IO;

namespace Underworld.Sfx;

/// <summary>
/// UW.AD bank loader. The file begins with an index of 6-byte entries:
///   uint8 patch, uint8 bank, uint32 offset
/// terminated by (patch=0xFF, bank=0xFF). Each offset points at a patch whose
/// first two bytes are a little-endian uint16 size, followed by that many bytes
/// of patch data. Size dispatches type:
///   14   → legacy OPL2 melodic (ignored for SFX)
///   248  → MT-32 (ignored — the MT-32 backend reads UW.MT)
///   else → TVFX effect patch
///
/// Reference: uw_patch.cpp:22-107 (khedoros/uw-engine).
/// </summary>
public sealed class TvfxPatchBank
{
    private readonly Dictionary<(byte bank, byte patch), TvfxPatch> _tvfx = new();

    private TvfxPatchBank() { }

    /// <summary>Look up a TVFX patch. Returns null if not present.</summary>
    public TvfxPatch? GetTvfx(byte patch, byte bank = 1)
        => _tvfx.TryGetValue((bank, patch), out var p) ? p : null;

    public IEnumerable<TvfxPatch> AllTvfx() => _tvfx.Values;

    public static TvfxPatchBank Load(string path)
    {
        var data = File.ReadAllBytes(path);
        var bank = new TvfxPatchBank();

        int i = 0;
        while (i + 6 <= data.Length)
        {
            byte patch  = data[i + 0];
            byte bankId = data[i + 1];
            if (patch == 0xFF && bankId == 0xFF) break;

            uint off = (uint)(data[i + 2]
                       | (data[i + 3] << 8)
                       | (data[i + 4] << 16)
                       | (data[i + 5] << 24));
            i += 6;

            if (off + 2 > data.Length) continue;
            ushort size = (ushort)(data[off] | (data[off + 1] << 8));
            if (off + size > data.Length) continue;

            if (size == 14 || size == 248) continue;    // melodic or MT-32

            var raw = new byte[size];
            Array.Copy(data, (int)off, raw, 0, size);
            bank._tvfx[(bankId, patch)] = new TvfxPatch(raw);
        }

        return bank;
    }
}
