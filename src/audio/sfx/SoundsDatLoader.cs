using System.IO;

namespace Underworld.Sfx;

public static class SoundsDatLoader
{
    static int BlockSize
    {
        get
        {
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                return 8;
            }
            else
            {
                return 5;
            }
        }
    }
    public static SoundEntry[] Load(string path)
    {
        var data = File.ReadAllBytes(path);
        if (data.Length < 1)
            throw new InvalidDataException("SOUNDS.DAT is empty");

        int count = data[0];
        int need = 1 + count * BlockSize;
        if (data.Length < need)
            throw new InvalidDataException(
                $"SOUNDS.DAT truncated: need {need} bytes for {count} entries, got {data.Length}");

        var entries = new SoundEntry[count];
        bool uw2 = UWClass._RES == UWClass.GAME_UW2;
        for (int i = 0; i < count; i++)
        {
            int o = 1 + i * BlockSize;
            // Duration word endianness differs by game:
            //   UW1 SOUNDS.DAT: bytes 3..4 are little-endian.
            //   UW2 SOUNDS.DAT: bytes 3..4 are big-endian. Source:
            //     uw2_asm.asm:83683-83688  "shl ax, 8 ; add ax, dx"
            //     with al=byte[3], dl=byte[4] → word = (byte[3] << 8) | byte[4].
            ushort dur = uw2
                ? (ushort)((data[o + 3] << 8) | data[o + 4])
                : (ushort)(data[o + 3] | (data[o + 4] << 8));
            entries[i] = new SoundEntry(
                PatchNum:     data[o + 0],
                Note:         data[o + 1],
                Velocity:     data[o + 2],
                DurationWord: dur);
        }
        return entries;
    }
}
