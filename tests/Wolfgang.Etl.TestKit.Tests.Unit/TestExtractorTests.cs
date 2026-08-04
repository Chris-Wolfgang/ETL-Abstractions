using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

public class TestExtractorTests
{
    // ------------------------------------------------------------------
    // IEnumerable<T> constructor — argument validation
    // ------------------------------------------------------------------

    [Fact]
    public void Constructor_with_enumerable_when_items_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractor<int>((IEnumerable<int>)null!)
        );
    }



    [Fact]
    public void Constructor_with_enumerable_and_timer_when_items_is_null_throws_ArgumentNullException()
    {
        var timer = new ManualProgressTimer();
        Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractorWithTimer((IEnumerable<int>)null!, timer)
        );
        timer.Dispose();
    }



    [Fact]
    public void Constructor_with_enumerable_and_timer_when_timer_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractorWithTimer(new List<int> { 1 }, null!)
        );
    }



    // ------------------------------------------------------------------
    // IEnumerator<T> constructor — argument validation
    // ------------------------------------------------------------------

    [Fact]
    public void Constructor_with_enumerator_when_enumerator_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractor<int>((IEnumerator<int>)null!)
        );
    }



    [Fact]
    public void Constructor_with_enumerator_and_timer_when_enumerator_is_null_throws_ArgumentNullException()
    {
        var timer = new ManualProgressTimer();
        Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractorWithTimer((IEnumerator<int>)null!, timer)
        );
        timer.Dispose();
    }



    [Fact]
    public void Constructor_with_enumerator_and_timer_when_timer_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractorWithTimer(new List<int> { 1 }.GetEnumerator(), null!)
        );
    }



    [Fact]
    public async Task Constructor_with_enumerable_and_timer_creates_extractor_that_yields_items()
    {
        using var timer = new ManualProgressTimer();
        var sut = new TestExtractorWithTimer(new List<int> { 1, 2, 3 }, timer);

        var results = await sut.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 1, 2, 3 }, results);
    }

    [Fact]
    public async Task Constructor_with_enumerator_and_timer_creates_extractor_that_yields_items()
    {
        using var timer = new ManualProgressTimer();
        var sut = new TestExtractorWithTimer(new List<int> { 1, 2, 3 }.GetEnumerator(), timer);

        var results = await sut.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 1, 2, 3 }, results);
    }



    // ------------------------------------------------------------------
    // IEnumerable<T> — extraction
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_with_enumerable_yields_all_items_in_order()
    {
        var items = new List<int> { 1, 2, 3 };
        var extractor = new TestExtractor<int>(items);

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal
        (
            new[] { 1, 2, 3 },
            results
        );
    }



    [Fact]
    public async Task ExtractAsync_with_empty_enumerable_yields_no_items()
    {
        var extractor = new TestExtractor<int>(new List<int>());

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Empty(results);
    }



    [Fact]
    public async Task ExtractAsync_with_single_item_enumerable_yields_that_item()
    {
        var extractor = new TestExtractor<int>(new List<int> { 42 });

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 42 }, results);
    }



    [Fact]
    public async Task ExtractAsync_with_enumerable_increments_CurrentItemCount()
    {
        var extractor = new TestExtractor<int>(new List<int> { 10, 20, 30 });

        await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(3, extractor.CurrentItemCount);
    }



    [Fact]
    public async Task ExtractAsync_with_enumerable_can_be_reused_across_multiple_runs()
    {
        var items = new List<string> { "a", "b" };
        var extractor = new TestExtractor<string>(items);

        var first = await extractor.ExtractAsync().ToListAsync();
        var second = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(first, second);
    }



    // ------------------------------------------------------------------
    // IEnumerator<T> — extraction
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_with_enumerator_yields_all_items_in_order()
    {
        var extractor = new TestExtractor<int>(GenerateInts(3));

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal
        (
            new[] { 0, 1, 2 },
            results
        );
    }



    [Fact]
    public async Task ExtractAsync_with_empty_enumerator_yields_no_items()
    {
        var extractor = new TestExtractor<int>(GenerateInts(0));

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Empty(results);
    }



    [Fact]
    public async Task ExtractAsync_with_single_item_enumerator_yields_that_item()
    {
        var extractor = new TestExtractor<int>(GenerateInts(1));

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 0 }, results);
    }



    [Fact]
    public async Task ExtractAsync_with_enumerator_increments_CurrentItemCount()
    {
        var extractor = new TestExtractor<int>(GenerateInts(5));

        await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(5, extractor.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // Cancellation
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_with_enumerable_respects_cancellation()
    {
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var extractor = new TestExtractor<int>(items);
        using var cts = new CancellationTokenSource();

        var results = new List<int>();
        await Assert.ThrowsAsync<OperationCanceledException>
        (
            async () =>
            {
                await foreach (var item in extractor.ExtractAsync(cts.Token))
                {
                    results.Add(item);
                    if (results.Count == 2)
                    {
                        cts.Cancel();
                    }
                }
            }
        );

        Assert.Equal(2, results.Count);
    }



    [Fact]
    public async Task ExtractAsync_with_enumerator_respects_cancellation()
    {
        var extractor = new TestExtractor<int>(GenerateInts(100));
        using var cts = new CancellationTokenSource();

        var results = new List<int>();
        await Assert.ThrowsAsync<OperationCanceledException>
        (
            async () =>
            {
                await foreach (var item in extractor.ExtractAsync(cts.Token))
                {
                    results.Add(item);
                    if (results.Count == 3)
                    {
                        cts.Cancel();
                    }
                }
            }
        );

        Assert.Equal(3, results.Count);
    }



    // ------------------------------------------------------------------
    // CreateProgressReport
    // ------------------------------------------------------------------

    [Fact]
    public async Task CreateProgressReport_returns_Report_reflecting_CurrentItemCount()
    {
        var extractor = new ExposedTestExtractor<int>(new List<int> { 1, 2, 3 });

        await extractor.ExtractAsync().ToListAsync();

        var report = extractor.GetProgressReport();

        Assert.Equal(3, report.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // Enumerator ownership
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_with_enumerable_disposes_enumerator_after_extraction()
    {
        var enumerator = new TrackingEnumerator<int>(new[] { 1, 2, 3 });
        var extractor = new TestExtractor<int>((IEnumerable<int>)enumerator);

        await extractor.ExtractAsync().ToListAsync();

        Assert.True(enumerator.WasDisposed);
    }



    [Fact]
    public async Task ExtractAsync_with_enumerator_does_not_dispose_enumerator_after_extraction()
    {
        var enumerator = new TrackingEnumerator<int>(new[] { 1, 2, 3 });
        var extractor = new TestExtractor<int>((IEnumerator<int>)enumerator);

        await extractor.ExtractAsync().ToListAsync();

        Assert.False(enumerator.WasDisposed);
    }



    [Fact]
    public void TrackingEnumerator_GetEnumerator_returns_self()
    {
        var enumerator = new TrackingEnumerator<int>(new[] { 1, 2, 3 });

        Assert.Same(enumerator, ((System.Collections.Generic.IEnumerable<int>)enumerator).GetEnumerator());
        Assert.Same(enumerator, ((System.Collections.IEnumerable)enumerator).GetEnumerator());
    }

    [Fact]
    public void TrackingEnumerator_Reset_repositions_to_before_first_element()
    {
        var enumerator = new TrackingEnumerator<int>(new[] { 1, 2 });
        enumerator.MoveNext();

        enumerator.Reset();
        enumerator.MoveNext();

        Assert.Equal(1, enumerator.Current);
        Assert.Equal(1, ((System.Collections.IEnumerator)enumerator).Current);
    }



    // ------------------------------------------------------------------
    // SkipItemCount
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_with_enumerable_skips_items_up_to_SkipItemCount()
    {
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3, 4, 5 });
        extractor.SkipItemCount = 2;

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 3, 4, 5 }, results);
    }



    [Fact]
    public async Task ExtractAsync_with_enumerator_skips_items_up_to_SkipItemCount()
    {
        var extractor = new TestExtractor<int>(GenerateInts(5));
        extractor.SkipItemCount = 2;

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 2, 3, 4 }, results);
    }



    [Fact]
    public async Task ExtractAsync_with_enumerable_skips_items_and_CurrentSkippedItemCount_is_correct()
    {
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3, 4, 5 });
        extractor.SkipItemCount = 3;

        await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(3, extractor.CurrentSkippedItemCount);
    }



    // ------------------------------------------------------------------
    // MaximumItemCount
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_with_enumerable_stops_at_MaximumItemCount()
    {
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3, 4, 5 });
        extractor.MaximumItemCount = 2;

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 1, 2 }, results);
    }



    [Fact]
    public async Task ExtractAsync_with_enumerator_stops_at_MaximumItemCount()
    {
        var extractor = new TestExtractor<int>(GenerateInts(10));
        extractor.MaximumItemCount = 3;

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 0, 1, 2 }, results);
    }



    [Fact]
    public async Task ExtractAsync_with_enumerator_combines_SkipItemCount_and_MaximumItemCount()
    {
        var extractor = new TestExtractor<int>(GenerateInts(10));
        extractor.SkipItemCount = 2;
        extractor.MaximumItemCount = 3;

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 2, 3, 4 }, results);
    }



    // ------------------------------------------------------------------
    // Func<T> factory constructor
    // ------------------------------------------------------------------

    [Fact]
    public void Constructor_with_func_factory_when_factory_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractor<int>((Func<int>)null!)
        );

        Assert.Equal("factory", ex.ParamName);
    }



    [Fact]
    public async Task ExtractAsync_with_func_factory_invokes_factory_once_per_item()
    {
        var counter = 0;
        var extractor = new TestExtractor<int>(() => counter++, 5);

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, results);
    }



    [Fact]
    public async Task ExtractAsync_with_func_factory_and_count_yields_count_items()
    {
        var extractor = new TestExtractor<int>(() => 7, 3);

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 7, 7, 7 }, results);
    }



    [Fact]
    public void Constructor_with_func_factory_and_count_when_factory_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractor<int>((Func<int>)null!, 3)
        );

        Assert.Equal("factory", ex.ParamName);
    }



    [Fact]
    public void Constructor_with_func_factory_and_count_when_count_is_negative_throws_ArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>
        (
            () => new TestExtractor<int>(() => 1, -1)
        );

        Assert.Equal("count", ex.ParamName);
    }



    [Fact]
    public async Task ExtractAsync_with_indefinite_func_factory_yields_MaximumItemCount_items()
    {
        var extractor = new TestExtractor<int>(() => 9);
        extractor.MaximumItemCount = 4;

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 9, 9, 9, 9 }, results);
    }



    // ------------------------------------------------------------------
    // Func<int, T> indexed factory constructor
    // ------------------------------------------------------------------

    [Fact]
    public void Constructor_with_indexed_factory_when_factory_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractor<int>((Func<int, int>)null!)
        );

        Assert.Equal("factory", ex.ParamName);
    }



    [Fact]
    public async Task ExtractAsync_with_indexed_factory_and_count_yields_expected_items()
    {
        var extractor = new TestExtractor<int>(i => i * 2, 5);

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 0, 2, 4, 6, 8 }, results);
    }



    [Fact]
    public void Constructor_with_indexed_factory_and_count_when_factory_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractor<int>((Func<int, int>)null!, 3)
        );

        Assert.Equal("factory", ex.ParamName);
    }



    [Fact]
    public void Constructor_with_indexed_factory_and_count_when_count_is_negative_throws_ArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>
        (
            () => new TestExtractor<int>(i => i, -1)
        );

        Assert.Equal("count", ex.ParamName);
    }



    [Fact]
    public async Task ExtractAsync_with_indefinite_indexed_factory_yields_MaximumItemCount_items()
    {
        var extractor = new TestExtractor<int>(i => i);
        extractor.MaximumItemCount = 3;

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 0, 1, 2 }, results);
    }



    // ------------------------------------------------------------------
    // Factory + timer constructors
    // ------------------------------------------------------------------

    [Fact]
    public async Task Constructor_with_func_factory_and_timer_yields_indefinitely_until_bounded()
    {
        using var timer = new ManualProgressTimer();
        var sut = new TestExtractorWithTimer(() => 5, timer);
        sut.MaximumItemCount = 3;

        var results = await sut.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 5, 5, 5 }, results);
    }



    [Fact]
    public async Task Constructor_with_func_factory_count_and_timer_yields_count_items()
    {
        using var timer = new ManualProgressTimer();
        var sut = new TestExtractorWithTimer(() => 5, 2, timer);

        var results = await sut.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 5, 5 }, results);
    }



    [Fact]
    public async Task Constructor_with_indexed_factory_and_timer_yields_indefinitely_until_bounded()
    {
        using var timer = new ManualProgressTimer();
        var sut = new TestExtractorWithTimer(i => i, timer);
        sut.MaximumItemCount = 3;

        var results = await sut.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 0, 1, 2 }, results);
    }



    [Fact]
    public async Task Constructor_with_indexed_factory_count_and_timer_yields_count_items()
    {
        using var timer = new ManualProgressTimer();
        var sut = new TestExtractorWithTimer(i => i * 3, 3, timer);

        var results = await sut.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 0, 3, 6 }, results);
    }



    // ------------------------------------------------------------------
    // Factory + timer constructors — argument validation
    // ------------------------------------------------------------------

    [Fact]
    public void Constructor_with_func_factory_and_timer_when_factory_is_null_throws_ArgumentNullException()
    {
        using var timer = new ManualProgressTimer();
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractorWithTimer((Func<int>)null!, timer)
        );

        Assert.Equal("factory", ex.ParamName);
    }



    [Fact]
    public void Constructor_with_func_factory_and_timer_when_timer_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractorWithTimer(() => 1, null!)
        );

        Assert.Equal("timer", ex.ParamName);
    }



    [Fact]
    public void Constructor_with_func_factory_count_and_timer_when_factory_is_null_throws_ArgumentNullException()
    {
        using var timer = new ManualProgressTimer();
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractorWithTimer((Func<int>)null!, 3, timer)
        );

        Assert.Equal("factory", ex.ParamName);
    }



    [Fact]
    public void Constructor_with_func_factory_count_and_timer_when_count_is_negative_throws_ArgumentOutOfRangeException()
    {
        using var timer = new ManualProgressTimer();
        var ex = Assert.Throws<ArgumentOutOfRangeException>
        (
            () => new TestExtractorWithTimer(() => 1, -1, timer)
        );

        Assert.Equal("count", ex.ParamName);
    }



    [Fact]
    public void Constructor_with_func_factory_count_and_timer_when_timer_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractorWithTimer(() => 1, 3, null!)
        );

        Assert.Equal("timer", ex.ParamName);
    }



    [Fact]
    public void Constructor_with_indexed_factory_and_timer_when_factory_is_null_throws_ArgumentNullException()
    {
        using var timer = new ManualProgressTimer();
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractorWithTimer((Func<int, int>)null!, timer)
        );

        Assert.Equal("factory", ex.ParamName);
    }



    [Fact]
    public void Constructor_with_indexed_factory_and_timer_when_timer_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractorWithTimer((Func<int, int>)(i => i), null!)
        );

        Assert.Equal("timer", ex.ParamName);
    }



    [Fact]
    public void Constructor_with_indexed_factory_count_and_timer_when_factory_is_null_throws_ArgumentNullException()
    {
        using var timer = new ManualProgressTimer();
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractorWithTimer((Func<int, int>)null!, 3, timer)
        );

        Assert.Equal("factory", ex.ParamName);
    }



    [Fact]
    public void Constructor_with_indexed_factory_count_and_timer_when_count_is_negative_throws_ArgumentOutOfRangeException()
    {
        using var timer = new ManualProgressTimer();
        var ex = Assert.Throws<ArgumentOutOfRangeException>
        (
            () => new TestExtractorWithTimer((Func<int, int>)(i => i), -1, timer)
        );

        Assert.Equal("count", ex.ParamName);
    }



    [Fact]
    public void Constructor_with_indexed_factory_count_and_timer_when_timer_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractorWithTimer((Func<int, int>)(i => i), 3, null!)
        );

        Assert.Equal("timer", ex.ParamName);
    }



    // ------------------------------------------------------------------
    // Progress + injected timer / Dispose
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_with_progress_and_injected_timer_reports_progress_when_timer_fires()
    {
        using var timer = new ManualProgressTimer();
        var sut = new TestExtractorWithTimer(new List<int> { 1, 2, 3 }, timer);
        Report? captured = null;
        var progress = new SynchronousProgress<Report>(r => captured = r);

        await using var enumerator = sut.ExtractAsync(progress).GetAsyncEnumerator();
        await enumerator.MoveNextAsync();
        timer.Fire();
        while (await enumerator.MoveNextAsync()) { }

        Assert.NotNull(captured);
        Assert.True(captured!.CurrentItemCount >= 1);
    }



    [Fact]
    public async Task Dispose_after_timer_wired_unsubscribes_so_firing_timer_does_not_report()
    {
        using var timer = new ManualProgressTimer();
        var reportCount = 0;
        var progress = new SynchronousProgress<Report>(_ => reportCount++);

        var sut = new TestExtractorWithTimer(new List<int> { 1, 2, 3 }, timer);

        // Pull one item so the timer is created and the Elapsed handler is wired,
        // but leave the enumerator undrained so the base class does not dispose the
        // injected timer in its finally. Disposing the SUT now runs the unsubscribe
        // branch of Dispose(bool); firing afterwards must produce no report.
        var enumerator = sut.ExtractAsync(progress).GetAsyncEnumerator();
        await enumerator.MoveNextAsync();

        sut.Dispose();
        timer.Fire();

        Assert.Equal(0, reportCount);
    }



    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private static IEnumerator<int> GenerateInts(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return i;
        }
    }



    private sealed class ExposedTestExtractor<T>(IEnumerable<T> items) : TestExtractor<T>(items) where T : notnull
    {
        public Report GetProgressReport() => CreateProgressReport();
    }



    private sealed class TestExtractorWithTimer : TestExtractor<int>
    {
        public TestExtractorWithTimer(IEnumerable<int> items, IProgressTimer timer)
            : base(items, timer) { }

        public TestExtractorWithTimer(IEnumerator<int> enumerator, IProgressTimer timer)
            : base(enumerator, timer) { }

        public TestExtractorWithTimer(Func<int> factory, IProgressTimer timer)
            : base(factory, timer) { }

        public TestExtractorWithTimer(Func<int> factory, int count, IProgressTimer timer)
            : base(factory, count, timer) { }

        public TestExtractorWithTimer(Func<int, int> factory, IProgressTimer timer)
            : base(factory, timer) { }

        public TestExtractorWithTimer(Func<int, int> factory, int count, IProgressTimer timer)
            : base(factory, count, timer) { }
    }



    private sealed class TrackingEnumerator<T> : IEnumerator<T>, IEnumerable<T>
    {
        private readonly IEnumerator<T> _inner;

        public bool WasDisposed { get; private set; }

        public TrackingEnumerator(IEnumerable<T> source)
        {
            _inner = source.GetEnumerator();
        }

        // Returns this so TestExtractor disposes the TrackingEnumerator itself,
        // not a compiler-generated wrapper, enabling the ownership tests above.
        public IEnumerator<T> GetEnumerator() => this;
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this;

        public T Current => _inner.Current;
        object? System.Collections.IEnumerator.Current => ((System.Collections.IEnumerator)_inner).Current;
        public bool MoveNext() => _inner.MoveNext();
        public void Reset() => _inner.Reset();

        public void Dispose()
        {
            WasDisposed = true;
            _inner.Dispose();
        }
    }
}
