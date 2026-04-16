using System;
using System.Threading;
using System.Threading.Tasks;
using Underworld.Sfx;

namespace Underworld.Sfx.Tests;

public class SpscQueueTests
{
    [Fact]
    public void Empty_dequeue_returns_false()
    {
        var q = new SpscQueue<int>(4);
        Assert.False(q.TryDequeue(out _));
    }

    [Fact]
    public void Enqueue_then_dequeue_roundtrip_preserves_value()
    {
        var q = new SpscQueue<int>(4);
        Assert.True(q.TryEnqueue(42));
        Assert.True(q.TryDequeue(out var got));
        Assert.Equal(42, got);
    }

    [Fact]
    public void Fifo_order_preserved()
    {
        var q = new SpscQueue<int>(8);
        for (int i = 0; i < 5; i++) Assert.True(q.TryEnqueue(i));
        for (int i = 0; i < 5; i++)
        {
            Assert.True(q.TryDequeue(out var got));
            Assert.Equal(i, got);
        }
    }

    [Fact]
    public void Full_returns_false_with_capacity_minus_one_usable_slots()
    {
        // Capacity 4 → 3 usable slots (one sacrificed).
        var q = new SpscQueue<int>(4);
        Assert.True(q.TryEnqueue(1));
        Assert.True(q.TryEnqueue(2));
        Assert.True(q.TryEnqueue(3));
        Assert.False(q.TryEnqueue(4));      // full
    }

    [Fact]
    public void Capacity_must_be_power_of_two()
    {
        Assert.Throws<ArgumentException>(() => new SpscQueue<int>(3));
        Assert.Throws<ArgumentException>(() => new SpscQueue<int>(0));
        Assert.Throws<ArgumentException>(() => new SpscQueue<int>(7));
    }

    [Fact]
    public void Wraparound_works_after_many_enqueue_dequeue_cycles()
    {
        var q = new SpscQueue<int>(4);                      // 3 usable
        for (int round = 0; round < 100; round++)
        {
            Assert.True(q.TryEnqueue(round));
            Assert.True(q.TryDequeue(out var got));
            Assert.Equal(round, got);
        }
    }

    [Fact]
    public void Concurrent_producer_consumer_preserves_order_and_count()
    {
        // Producer pushes ints 0..N-1; consumer drains in order. Smoke-test for
        // the lock-free path under real thread contention.
        const int N = 100_000;
        var q = new SpscQueue<int>(1024);
        int next = 0;
        var done = false;

        var consumer = Task.Run(() =>
        {
            int expected = 0;
            while (!done || expected < N)
            {
                if (q.TryDequeue(out var got))
                {
                    Assert.Equal(expected, got);
                    expected++;
                    if (expected == N) return;
                }
                else
                {
                    Thread.SpinWait(8);
                }
            }
        });

        var producer = Task.Run(() =>
        {
            while (next < N)
                if (q.TryEnqueue(next)) next++;
                else Thread.SpinWait(8);
            done = true;
        });

        Assert.True(Task.WaitAll(new[] { producer, consumer }, TimeSpan.FromSeconds(10)));
    }
}
