using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

public class FaultyTransformerTests
{
    // ------------------------------------------------------------------
    // Constructor — argument validation
    // ------------------------------------------------------------------

    [Fact]
    public void Constructor_with_timer_when_timer_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new FaultyTransformerWithTimer(null!)
        );
    }



    [Fact]
    public async Task Constructor_with_timer_creates_transformer_that_yields_items()
    {
        using var timer  = new ManualProgressTimer();
        var transformer  = new FaultyTransformerWithTimer(timer);
        var extractor    = new FaultyExtractor<int>(new[] { 1, 2, 3 });

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Equal(new[] { 1, 2, 3 }, results);
    }



    // ------------------------------------------------------------------
    // No faults — pass-through
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_when_no_faults_configured_yields_all_items_in_order()
    {
        var extractor   = new FaultyExtractor<int>(new[] { 1, 2, 3 });
        var transformer = new FaultyTransformer<int>();

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

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
    public async Task TransformAsync_when_ThrowAt_configured_throws_that_exception_reaching_the_index()
    {
        var expected    = new InvalidOperationException("bad record");
        var extractor   = new FaultyExtractor<int>(new[] { 10, 20, 30, 40, 50 });
        var transformer = new FaultyTransformer<int>().ThrowAt(2, expected);

        var collected = new List<int>();

        var actual = await Assert.ThrowsAsync<InvalidOperationException>
        (
            async () =>
            {
                await foreach (var item in transformer.TransformAsync(extractor.ExtractAsync()))
                {
                    collected.Add(item);
                }
            }
        );

        Assert.Same(expected, actual);
        Assert.Equal(new[] { 10, 20 }, collected);
    }



    [Fact]
    public async Task TransformAsync_when_ThrowAt_configured_counts_the_failing_item()
    {
        var extractor   = new FaultyExtractor<int>(new[] { 10, 20, 30, 40, 50 });
        var transformer = new FaultyTransformer<int>().ThrowAt(2, new InvalidOperationException("bad record"));

        await Assert.ThrowsAsync<InvalidOperationException>
        (
            async () => await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync()
        );

        // Index 2 is the third item; the failing item is counted, so 2 + 1 == 3.
        Assert.Equal(3, transformer.CurrentItemCount);
    }



    [Fact]
    public async Task TransformAsync_when_ThrowAt_called_twice_for_same_index_last_exception_wins()
    {
        var first       = new InvalidOperationException("first");
        var second      = new TimeoutException("second");
        var extractor   = new FaultyExtractor<int>(new[] { 1, 2, 3 });
        var transformer = new FaultyTransformer<int>()
            .ThrowAt(1, first)
            .ThrowAt(1, second);

        var actual = await Assert.ThrowsAsync<TimeoutException>
        (
            async () => await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync()
        );

        Assert.Same(second, actual);
    }



    // ------------------------------------------------------------------
    // ThrowAfterCompletion
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_when_ThrowAfterCompletion_configured_yields_all_items_then_throws()
    {
        var expected    = new InvalidOperationException("flush failed");
        var extractor   = new FaultyExtractor<int>(new[] { 1, 2, 3 });
        var transformer = new FaultyTransformer<int>().ThrowAfterCompletion(expected);

        var collected = new List<int>();

        var actual = await Assert.ThrowsAsync<InvalidOperationException>
        (
            async () =>
            {
                await foreach (var item in transformer.TransformAsync(extractor.ExtractAsync()))
                {
                    collected.Add(item);
                }
            }
        );

        Assert.Same(expected, actual);
        Assert.Equal(new[] { 1, 2, 3 }, collected);
        Assert.Equal(3, transformer.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // DuplicateAt
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_when_DuplicateAt_configured_yields_that_item_twice_consecutively()
    {
        var extractor   = new FaultyExtractor<int>(new[] { 1, 2, 3 });
        var transformer = new FaultyTransformer<int>().DuplicateAt(1);

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Equal(new[] { 1, 2, 2, 3 }, results);
        Assert.Equal(4, transformer.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // Composability
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_when_DuplicateAt_and_later_ThrowAt_configured_duplicate_emitted_then_throw_fires()
    {
        var expected    = new InvalidOperationException("boom");
        var extractor   = new FaultyExtractor<int>(new[] { 1, 2, 3, 4, 5 });
        var transformer = new FaultyTransformer<int>()
            .DuplicateAt(1)
            .ThrowAt(3, expected);

        var collected = new List<int>();

        var actual = await Assert.ThrowsAsync<InvalidOperationException>
        (
            async () =>
            {
                await foreach (var item in transformer.TransformAsync(extractor.ExtractAsync()))
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
        var transformer = new FaultyTransformer<int>();

        var ex = Assert.Throws<ArgumentOutOfRangeException>
        (
            () => transformer.ThrowAt(-1, new InvalidOperationException())
        );

        Assert.Equal("index", ex.ParamName);
    }



    [Fact]
    public void ThrowAt_when_exception_is_null_throws_ArgumentNullException()
    {
        var transformer = new FaultyTransformer<int>();

        var ex = Assert.Throws<ArgumentNullException>
        (
            () => transformer.ThrowAt(0, null!)
        );

        Assert.Equal("exception", ex.ParamName);
    }



    [Fact]
    public void ThrowAfterCompletion_when_exception_is_null_throws_ArgumentNullException()
    {
        var transformer = new FaultyTransformer<int>();

        var ex = Assert.Throws<ArgumentNullException>
        (
            () => transformer.ThrowAfterCompletion(null!)
        );

        Assert.Equal("exception", ex.ParamName);
    }



    [Fact]
    public void DuplicateAt_when_index_is_negative_throws_ArgumentOutOfRangeException()
    {
        var transformer = new FaultyTransformer<int>();

        var ex = Assert.Throws<ArgumentOutOfRangeException>
        (
            () => transformer.DuplicateAt(-1)
        );

        Assert.Equal("index", ex.ParamName);
    }



    // ------------------------------------------------------------------
    // Fluent chaining
    // ------------------------------------------------------------------

    [Fact]
    public void ThrowAt_returns_same_instance()
    {
        var transformer = new FaultyTransformer<int>();

        Assert.Same(transformer, transformer.ThrowAt(0, new InvalidOperationException()));
    }



    [Fact]
    public void ThrowAfterCompletion_returns_same_instance()
    {
        var transformer = new FaultyTransformer<int>();

        Assert.Same(transformer, transformer.ThrowAfterCompletion(new InvalidOperationException()));
    }



    [Fact]
    public void DuplicateAt_returns_same_instance()
    {
        var transformer = new FaultyTransformer<int>();

        Assert.Same(transformer, transformer.DuplicateAt(0));
    }



    // ------------------------------------------------------------------
    // Progress + injected timer
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_with_progress_and_injected_timer_reports_progress_when_timer_fires()
    {
        using var timer = new ManualProgressTimer();
        var transformer = new FaultyTransformerWithTimer(timer);
        var extractor   = new FaultyExtractor<int>(new[] { 1, 2, 3 });
        Report? captured = null;
        var progress = new SynchronousProgress<Report>(r => captured = r);

        await using var enumerator =
            transformer.TransformAsync(extractor.ExtractAsync(), progress).GetAsyncEnumerator();
        await enumerator.MoveNextAsync();
        timer.Fire();
        while (await enumerator.MoveNextAsync()) { }

        Assert.NotNull(captured);
        Assert.True(captured!.CurrentItemCount >= 1);
    }



    [Fact]
    public async Task TransformAsync_with_progress_and_no_injected_timer_yields_all_items()
    {
        var transformer = new FaultyTransformer<int>();
        var extractor   = new FaultyExtractor<int>(new[] { 1, 2, 3 });
        var progress    = new SynchronousProgress<Report>(_ => { });

        var results = await transformer.TransformAsync(extractor.ExtractAsync(), progress).ToListAsync();

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

        var transformer = new FaultyTransformerWithTimer(timer);
        var extractor   = new FaultyExtractor<int>(new[] { 1, 2, 3 });

        // Pull one item so the timer is wired, but leave the enumerator undrained so
        // the base class does not dispose the injected timer in its finally. Disposing
        // the SUT runs the unsubscribe branch; firing afterwards must produce no report.
        var enumerator =
            transformer.TransformAsync(extractor.ExtractAsync(), progress).GetAsyncEnumerator();
        await enumerator.MoveNextAsync();

        transformer.Dispose();
        timer.Fire();

        Assert.Equal(0, reportCount);
    }



    // ------------------------------------------------------------------
    // SkipItemCount / MaximumItemCount
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_skips_items_up_to_SkipItemCount()
    {
        var extractor   = new FaultyExtractor<int>(new[] { 1, 2, 3, 4, 5 });
        var transformer = new FaultyTransformer<int>();
        transformer.SkipItemCount = 2;

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Equal(new[] { 3, 4, 5 }, results);
        Assert.Equal(2, transformer.CurrentSkippedItemCount);
    }



    [Fact]
    public async Task TransformAsync_stops_at_MaximumItemCount()
    {
        var extractor   = new FaultyExtractor<int>(new[] { 1, 2, 3, 4, 5 });
        var transformer = new FaultyTransformer<int>();
        transformer.MaximumItemCount = 2;

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Equal(new[] { 1, 2 }, results);
    }



    private sealed class FaultyTransformerWithTimer : FaultyTransformer<int>
    {
        public FaultyTransformerWithTimer(IProgressTimer timer) : base(timer) { }
    }
}
