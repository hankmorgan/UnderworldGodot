using System;
using System.Threading;

namespace Underworld.Sfx;

/// <summary>
/// Single-producer single-consumer lock-free ring. Capacity must be a power of
/// two so the modulo can be a bitmask. One slot is sacrificed to distinguish
/// full from empty (head==tail).
/// </summary>
public sealed class SpscQueue<T> where T : struct
{
    private readonly T[] _buf;
    private readonly int _mask;
    private int _head;        // producer-only writes
    private int _tail;        // consumer-only writes

    public SpscQueue(int capacityPow2)
    {
        if (capacityPow2 < 2 || (capacityPow2 & (capacityPow2 - 1)) != 0)
            throw new ArgumentException("capacity must be a power of 2 >= 2", nameof(capacityPow2));
        _buf = new T[capacityPow2];
        _mask = capacityPow2 - 1;
    }

    /// <summary>Producer: enqueue, return false if full.</summary>
    public bool TryEnqueue(T item)
    {
        int h = _head;
        int t = Volatile.Read(ref _tail);
        if (((h + 1) & _mask) == (t & _mask)) return false;     // full
        _buf[h & _mask] = item;
        Volatile.Write(ref _head, h + 1);
        return true;
    }

    /// <summary>Consumer: dequeue, return false if empty.</summary>
    public bool TryDequeue(out T item)
    {
        int t = _tail;
        int h = Volatile.Read(ref _head);
        if ((t & _mask) == (h & _mask)) { item = default; return false; }
        item = _buf[t & _mask];
        Volatile.Write(ref _tail, t + 1);
        return true;
    }
}
