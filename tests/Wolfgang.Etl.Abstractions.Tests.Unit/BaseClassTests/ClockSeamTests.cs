using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// Covers the #338 clock seam: when an <c>ITimeSource</c> is injected, the base classes' timing
/// metrics (<c>StartedAt</c> / <c>Elapsed</c>) and <c>EtlRunState</c>'s elapsed derive from it
/// deterministically; with no injection the real system clock is used. The seam is internal (surfaced
/// to tests via <c>InternalsVisibleTo</c>); no public API change.
/// </summary>
public class ClockSeamTests
{
    private static readonly DateTimeOffset Start = new(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);


    [Fact]
    public async Task Extractor_StartedAt_and_Elapsed_derive_from_the_injected_time_source()
    {
        var clock = new FakeTimeSource();
        var sut = new ClockExtractor { TimeSource = clock };

        await Drain(sut.ExtractAsync(CancellationToken.None));   // first item captures start

        Assert.Equal(Start, sut.PeekStartedAt);
        Assert.Equal(TimeSpan.Zero, sut.PeekElapsed);

        clock.Advance(TimeSpan.FromSeconds(5));

        Assert.Equal(TimeSpan.FromSeconds(5), sut.PeekElapsed);
        Assert.Equal(Start, sut.PeekStartedAt);   // StartedAt is the captured value, not "now"
    }


    [Fact]
    public async Task Loader_Elapsed_derives_from_the_injected_time_source()
    {
        var clock = new FakeTimeSource();
        var sut = new ClockLoader { TimeSource = clock };

        await sut.LoadAsync(AsyncSource(1), CancellationToken.None);
        clock.Advance(TimeSpan.FromSeconds(2));

        Assert.Equal(Start, sut.PeekStartedAt);
        Assert.Equal(TimeSpan.FromSeconds(2), sut.PeekElapsed);
    }


    [Fact]
    public async Task Transformer_Elapsed_derives_from_the_injected_time_source()
    {
        var clock = new FakeTimeSource();
        var sut = new ClockTransformer { TimeSource = clock };

        await Drain(sut.TransformAsync(AsyncSource(1), CancellationToken.None));
        clock.Advance(TimeSpan.FromSeconds(9));

        Assert.Equal(Start, sut.PeekStartedAt);
        Assert.Equal(TimeSpan.FromSeconds(9), sut.PeekElapsed);
    }


    [Fact]
    public void Before_the_first_item_StartedAt_is_null_and_Elapsed_is_zero()
    {
        var sut = new ClockExtractor { TimeSource = new FakeTimeSource() };

        Assert.Null(sut.PeekStartedAt);
        Assert.Equal(TimeSpan.Zero, sut.PeekElapsed);
    }


    [Fact]
    public async Task With_no_injected_source_the_real_system_clock_is_used()
    {
        var sut = new ClockExtractor();   // TimeSource null -> SystemTimeSource

        var before = DateTimeOffset.UtcNow;
        await Drain(sut.ExtractAsync(CancellationToken.None));
        var after = DateTimeOffset.UtcNow;

        Assert.NotNull(sut.PeekStartedAt);
        Assert.InRange(sut.PeekStartedAt!.Value, before.AddSeconds(-1), after.AddSeconds(1));
        Assert.True(sut.PeekElapsed >= TimeSpan.Zero);
    }


    [Fact]
    public void EtlRunState_elapsed_derives_from_the_injected_time_source()
    {
        var clock = new FakeTimeSource();
        var state = new EtlRunState(clock);

        clock.Advance(TimeSpan.FromSeconds(3));

        Assert.Equal(TimeSpan.FromSeconds(3), state.Snapshot().Elapsed);
    }


    // ---------- helpers ----------

    private static async IAsyncEnumerable<int> AsyncSource(params int[] items)
    {
        foreach (var item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }


    private static async Task Drain(IAsyncEnumerable<int> source)
    {
        await foreach (var _ in source.ConfigureAwait(false))
        {
        }
    }


    // ---------- doubles ----------

    // A fake clock: UtcNow and a monotonic tick counter the test advances by hand. The tick counter
    // starts non-zero so a captured start never collides with the base's "not started" sentinel (0).
    private sealed class FakeTimeSource : ITimeSource
    {
        public DateTimeOffset UtcNow { get; private set; } = Start;

        public long Timestamp { get; private set; } = TimeSpan.TicksPerSecond;

        public long TimestampFrequency => TimeSpan.TicksPerSecond;

        public long GetTimestamp() => Timestamp;

        public void Advance(TimeSpan by)
        {
            UtcNow += by;
            Timestamp += (long)(by.TotalSeconds * TimestampFrequency);
        }
    }


    private sealed class ClockExtractor : ExtractorBase<int, EtlProgress>
    {
        public DateTimeOffset? PeekStartedAt => StartedAt;

        public TimeSpan PeekElapsed => Elapsed;

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            await Task.Yield();
            IncrementCurrentItemCount();
            yield return 1;
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    private sealed class ClockLoader : LoaderBase<int, EtlProgress>
    {
        public DateTimeOffset? PeekStartedAt => StartedAt;

        public TimeSpan PeekElapsed => Elapsed;

        protected override async Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
        {
            await foreach (var _ in items.WithCancellation(token).ConfigureAwait(false))
            {
                IncrementCurrentItemCount();
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    private sealed class ClockTransformer : TransformerBase<int, int, EtlProgress>
    {
        public DateTimeOffset? PeekStartedAt => StartedAt;

        public TimeSpan PeekElapsed => Elapsed;

        protected override async IAsyncEnumerable<int> TransformWorkerAsync(
            IAsyncEnumerable<int> items, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
            {
                IncrementCurrentItemCount();
                yield return item;
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }
}
