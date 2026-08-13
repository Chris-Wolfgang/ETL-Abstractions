using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

public class FaultyExtractorTests
{
    // ------------------------------------------------------------------
    // Constructor — argument validation
    // ------------------------------------------------------------------

    [Fact]
    public void Constructor_when_items_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new FaultyExtractor<int>(null!)
        );
    }



    [Fact]
    public void Constructor_with_timer_when_timer_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new FaultyExtractorWithTimer(new List<int> { 1 }, null!)
        );
    }



    [Fact]
    public async Task Constructor_with_timer_creates_extractor_that_yields_items()
    {
        using var timer = new ManualProgressTimer();
        var sut = new FaultyExtractorWithTimer(new List<int> { 1, 2, 3 }, timer);

        var results = await sut.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 1, 2, 3 }, results);
    }



    // ------------------------------------------------------------------
    // No faults — pass-through
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_when_no_faults_configured_yields_all_items_in_order()
    {
        var sut = new FaultyExtractor<int>(new List<int> { 1, 2, 3 });

        var results = await sut.ExtractAsync().ToListAsync();

        Assert.Equal
        (
            new[] { 1, 2, 3 },
            results
        );
    }



    // ------------------------------------------------------------------
    // ThrowAt
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_when_ThrowAt_configured_throws_that_exception_reaching_the_index()
    {
        var expected = new InvalidOperationException("boom");
        var sut      = new FaultyExtractor<int>(new[] { 10, 20, 30, 40, 50 })
            .ThrowAt(2, expected);

        var collected = new List<int>();

        var actual = await Assert.ThrowsAsync<InvalidOperationException>
        (
            async () =>
            {
                await foreach (var item in sut.ExtractAsync())
                {
                    collected.Add(item);
                }
            }
        );

        Assert.Same(expected, actual);
        Assert.Equal(new[] { 10, 20 }, collected);
    }



    [Fact]
    public async Task ExtractAsync_when_ThrowAt_configured_counts_the_failing_item()
    {
        var sut = new FaultyExtractor<int>(new[] { 10, 20, 30, 40, 50 })
            .ThrowAt(2, new InvalidOperationException("boom"));

        await Assert.ThrowsAsync<InvalidOperationException>
        (
            async () => await sut.ExtractAsync().ToListAsync()
        );

        // Index 2 is the third item; the failing item is counted, so 2 + 1 == 3.
        Assert.Equal(3, sut.CurrentItemCount);
    }



    [Fact]
    public async Task ExtractAsync_when_ThrowAt_called_twice_for_same_index_last_exception_wins()
    {
        var first  = new InvalidOperationException("first");
        var second = new TimeoutException("second");
        var sut    = new FaultyExtractor<int>(new[] { 1, 2, 3 })
            .ThrowAt(1, first)
            .ThrowAt(1, second);

        var actual = await Assert.ThrowsAsync<TimeoutException>
        (
            async () => await sut.ExtractAsync().ToListAsync()
        );

        Assert.Same(second, actual);
    }



    // ------------------------------------------------------------------
    // ThrowAfterCompletion
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_when_ThrowAfterCompletion_configured_yields_all_items_then_throws()
    {
        var expected = new InvalidOperationException("finalize failed");
        var sut      = new FaultyExtractor<int>(new[] { 1, 2, 3 })
            .ThrowAfterCompletion(expected);

        var collected = new List<int>();

        var actual = await Assert.ThrowsAsync<InvalidOperationException>
        (
            async () =>
            {
                await foreach (var item in sut.ExtractAsync())
                {
                    collected.Add(item);
                }
            }
        );

        Assert.Same(expected, actual);
        Assert.Equal(new[] { 1, 2, 3 }, collected);
        Assert.Equal(3, sut.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // DuplicateAt
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_when_DuplicateAt_configured_yields_that_item_twice_consecutively()
    {
        var sut = new FaultyExtractor<int>(new[] { 1, 2, 3 })
            .DuplicateAt(1);

        var results = await sut.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 1, 2, 2, 3 }, results);
        Assert.Equal(4, sut.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // Composability
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_when_DuplicateAt_and_later_ThrowAt_configured_duplicate_emitted_then_throw_fires()
    {
        var expected = new InvalidOperationException("boom");
        var sut      = new FaultyExtractor<int>(new[] { 1, 2, 3, 4, 5 })
            .DuplicateAt(1)
            .ThrowAt(3, expected);

        var collected = new List<int>();

        var actual = await Assert.ThrowsAsync<InvalidOperationException>
        (
            async () =>
            {
                await foreach (var item in sut.ExtractAsync())
                {
                    collected.Add(item);
                }
            }
        );

        Assert.Same(expected, actual);
        Assert.Equal(new[] { 1, 2, 2, 3 }, collected);
    }



    // ------------------------------------------------------------------
    // Argument validation
    // ------------------------------------------------------------------

    [Fact]
    public void ThrowAt_when_index_is_negative_throws_ArgumentOutOfRangeException()
    {
        var sut = new FaultyExtractor<int>(new[] { 1 });

        var ex = Assert.Throws<ArgumentOutOfRangeException>
        (
            () => sut.ThrowAt(-1, new InvalidOperationException())
        );

        Assert.Equal("index", ex.ParamName);
    }



    [Fact]
    public void ThrowAt_when_exception_is_null_throws_ArgumentNullException()
    {
        var sut = new FaultyExtractor<int>(new[] { 1 });

        var ex = Assert.Throws<ArgumentNullException>
        (
            () => sut.ThrowAt(0, null!)
        );

        Assert.Equal("exception", ex.ParamName);
    }



    [Fact]
    public void ThrowAfterCompletion_when_exception_is_null_throws_ArgumentNullException()
    {
        var sut = new FaultyExtractor<int>(new[] { 1 });

        var ex = Assert.Throws<ArgumentNullException>
        (
            () => sut.ThrowAfterCompletion(null!)
        );

        Assert.Equal("exception", ex.ParamName);
    }



    [Fact]
    public void DuplicateAt_when_index_is_negative_throws_ArgumentOutOfRangeException()
    {
        var sut = new FaultyExtractor<int>(new[] { 1 });

        var ex = Assert.Throws<ArgumentOutOfRangeException>
        (
            () => sut.DuplicateAt(-1)
        );

        Assert.Equal("index", ex.ParamName);
    }



    // ------------------------------------------------------------------
    // Fluent chaining
    // ------------------------------------------------------------------

    [Fact]
    public void ThrowAt_returns_same_instance()
    {
        var sut = new FaultyExtractor<int>(new[] { 1 });

        Assert.Same(sut, sut.ThrowAt(0, new InvalidOperationException()));
    }



    [Fact]
    public void ThrowAfterCompletion_returns_same_instance()
    {
        var sut = new FaultyExtractor<int>(new[] { 1 });

        Assert.Same(sut, sut.ThrowAfterCompletion(new InvalidOperationException()));
    }



    [Fact]
    public void DuplicateAt_returns_same_instance()
    {
        var sut = new FaultyExtractor<int>(new[] { 1 });

        Assert.Same(sut, sut.DuplicateAt(0));
    }



    // ------------------------------------------------------------------
    // Progress + injected timer
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_with_progress_and_injected_timer_reports_progress_when_timer_fires()
    {
        using var timer = new ManualProgressTimer();
        var sut = new FaultyExtractorWithTimer(new List<int> { 1, 2, 3 }, timer);
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
    public async Task ExtractAsync_with_progress_and_no_injected_timer_yields_all_items()
    {
        var sut = new FaultyExtractor<int>(new List<int> { 1, 2, 3 });
        var progress = new SynchronousProgress<Report>(_ => { });

        var results = await sut.ExtractAsync(progress).ToListAsync();

        Assert.Equal(new[] { 1, 2, 3 }, results);
    }



    // ------------------------------------------------------------------
    // Dispose
    // ------------------------------------------------------------------

    [Fact]
    public async Task Dispose_after_timer_wired_unsubscribes_so_firing_timer_does_not_report()
    {
        using var timer = new ManualProgressTimer();
        var reportCount = 0;
        var progress = new SynchronousProgress<Report>(_ => reportCount++);

        var sut = new FaultyExtractorWithTimer(new List<int> { 1, 2, 3 }, timer);

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
    // SkipItemCount / MaximumItemCount
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_skips_items_up_to_SkipItemCount()
    {
        var sut = new FaultyExtractor<int>(new[] { 1, 2, 3, 4, 5 });
        sut.SkipItemCount = 2;

        var results = await sut.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 3, 4, 5 }, results);
        Assert.Equal(2, sut.CurrentSkippedItemCount);
    }



    [Fact]
    public async Task ExtractAsync_stops_at_MaximumItemCount()
    {
        var sut = new FaultyExtractor<int>(new[] { 1, 2, 3, 4, 5 });
        sut.MaximumItemCount = 2;

        var results = await sut.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 1, 2 }, results);
    }



    private sealed class FaultyExtractorWithTimer : FaultyExtractor<int>
    {
        public FaultyExtractorWithTimer(IEnumerable<int> items, IProgressTimer timer)
            : base(items, timer) { }
    }
}
