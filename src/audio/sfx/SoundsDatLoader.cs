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
        for (int i = 0; i < count; i++)
        {
            int o = 1 + i * BlockSize;
            entries[i] = new SoundEntry(
                PatchNum:     data[o + 0],
                Note:         data[o + 1],
                Velocity:     data[o + 2],
                DurationWord: (ushort)(data[o + 3] | (data[o + 4] << 8)));
        }
        return entries;
    }
}
