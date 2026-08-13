using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

public class TestTransformerTests
{
    // ------------------------------------------------------------------
    // Constructor — argument validation
    // ------------------------------------------------------------------

    [Fact]
    public void Constructor_with_timer_when_timer_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new TestTransformerWithTimer(null!)
        );
    }



    // ------------------------------------------------------------------
    // Pass-through behaviour
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_yields_all_items_unchanged_and_in_order()
    {
        var extractor    = new TestExtractor<int>(new List<int> { 1, 2, 3 });
        var transformer  = new TestTransformer<int>();

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Equal
        (
            new[] { 1, 2, 3 },
            results
        );
    }



    [Fact]
    public async Task TransformAsync_with_empty_source_yields_no_items()
    {
        var extractor   = new TestExtractor<string>(new List<string>());
        var transformer = new TestTransformer<string>();

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Empty(results);
    }



    [Fact]
    public async Task TransformAsync_with_single_item_source_yields_that_item_unchanged()
    {
        var extractor   = new TestExtractor<int>(new List<int> { 42 });
        var transformer = new TestTransformer<int>();

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Equal(new[] { 42 }, results);
    }



    [Fact]
    public async Task TransformAsync_with_reference_type_yields_same_references()
    {
        // Verifies pass-through — no copying or wrapping of reference types.
        var a = new object();
        var b = new object();
        var extractor   = new TestExtractor<object>(new List<object> { a, b });
        var transformer = new TestTransformer<object>();

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Same(a, results[0]);
        Assert.Same(b, results[1]);
    }



    // ------------------------------------------------------------------
    // SkipItemCount
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_skips_items_up_to_SkipItemCount()
    {
        var extractor   = new TestExtractor<int>(new List<int> { 1, 2, 3, 4, 5 });
        var transformer = new TestTransformer<int>();
        transformer.SkipItemCount = 2;

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Equal(new[] { 3, 4, 5 }, results);
    }



    // ------------------------------------------------------------------
    // MaximumItemCount
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_stops_at_MaximumItemCount()
    {
        var extractor   = new TestExtractor<int>(new List<int> { 1, 2, 3, 4, 5 });
        var transformer = new TestTransformer<int>();
        transformer.MaximumItemCount = 2;

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Equal(new[] { 1, 2 }, results);
    }



    // ------------------------------------------------------------------
    // CurrentItemCount
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_increments_CurrentItemCount()
    {
        var extractor   = new TestExtractor<int>(new List<int> { 10, 20, 30, 40 });
        var transformer = new TestTransformer<int>();

        await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Equal(4, transformer.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // CreateProgressReport
    // ------------------------------------------------------------------

    [Fact]
    public async Task CreateProgressReport_returns_Report_reflecting_CurrentItemCount()
    {
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3 });
        var transformer = new ExposedTestTransformer<int>();

        await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        var report = transformer.GetProgressReport();

        Assert.Equal(3, report.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // Timer constructor — success path
    // ------------------------------------------------------------------

    [Fact]
    public async Task Constructor_with_timer_creates_transformer_that_yields_items()
    {
        using var timer = new ManualProgressTimer();
        var transformer = new TestTransformerWithTimer(timer);
        var extractor   = new TestExtractor<int>(new List<int> { 1, 2, 3 });

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Equal(new[] { 1, 2, 3 }, results);
    }



    // ------------------------------------------------------------------
    // Progress + injected timer / Dispose
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_with_progress_and_injected_timer_reports_progress_when_timer_fires()
    {
        using var timer = new ManualProgressTimer();
        var transformer = new TestTransformerWithTimer(timer);
        var extractor   = new TestExtractor<int>(new List<int> { 1, 2, 3 });
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
    public async Task Dispose_after_timer_wired_unsubscribes_so_firing_timer_does_not_report()
    {
        using var timer = new ManualProgressTimer();
        var reportCount = 0;
        var progress = new SynchronousProgress<Report>(_ => reportCount++);

        var transformer = new TestTransformerWithTimer(timer);
        var extractor   = new TestExtractor<int>(new List<int> { 1, 2, 3 });

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



    private sealed class ExposedTestTransformer<T> : TestTransformer<T> where T : notnull
    {
        public Report GetProgressReport() => CreateProgressReport();
    }



    private sealed class TestTransformerWithTimer : TestTransformer<int>
    {
        public TestTransformerWithTimer(IProgressTimer timer) : base(timer) { }
    }
}
