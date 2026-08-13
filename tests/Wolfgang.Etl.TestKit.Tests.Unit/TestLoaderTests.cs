using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

public class TestLoaderTests
{
    // ------------------------------------------------------------------
    // Constructor — argument validation
    // ------------------------------------------------------------------

    [Fact]
    public void Constructor_with_timer_when_timer_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new TestLoaderWithTimer(collectItems: false, null!)
        );
    }



    // ------------------------------------------------------------------
    // collectItems: true — collection behaviour
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_when_collectItems_is_true_GetCollectedItems_returns_all_items_in_order()
    {
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3 });
        var loader    = new TestLoader<int>(collectItems: true);

        await loader.LoadAsync(extractor.ExtractAsync());

        var items = loader.GetCollectedItems();

        Assert.NotNull(items);
        Assert.Equal
        (
            new[] { 1, 2, 3 },
            items
        );
    }



    [Fact]
    public async Task LoadAsync_when_collectItems_is_true_and_source_is_empty_GetCollectedItems_returns_empty_list()
    {
        var extractor = new TestExtractor<int>(new List<int>());
        var loader    = new TestLoader<int>(collectItems: true);

        await loader.LoadAsync(extractor.ExtractAsync());

        var items = loader.GetCollectedItems();

        Assert.NotNull(items);
        Assert.Empty(items);
    }



    [Fact]
    public async Task LoadAsync_when_collectItems_is_true_and_source_has_one_item_GetCollectedItems_returns_that_item()
    {
        var extractor = new TestExtractor<int>(new List<int> { 42 });
        var loader    = new TestLoader<int>(collectItems: true);

        await loader.LoadAsync(extractor.ExtractAsync());

        var items = loader.GetCollectedItems();

        Assert.NotNull(items);
        Assert.Equal(new[] { 42 }, items);
    }



    [Fact]
    public async Task LoadAsync_when_collectItems_is_false_and_source_has_one_item_still_enumerates_it()
    {
        var extractor = new TestExtractor<int>(new List<int> { 42 });
        var loader    = new TestLoader<int>(collectItems: false);

        await loader.LoadAsync(extractor.ExtractAsync());

        Assert.Equal(1, loader.CurrentItemCount);
    }



    [Fact]
    public async Task LoadAsync_when_collectItems_is_true_second_run_replaces_first_run_results()
    {
        var loader = new TestLoader<int>(collectItems: true);

        await loader.LoadAsync(new TestExtractor<int>(new List<int> { 1, 2, 3 }).ExtractAsync());
        await loader.LoadAsync(new TestExtractor<int>(new List<int> { 7, 8 }).ExtractAsync());

        var items = loader.GetCollectedItems();

        Assert.NotNull(items);
        Assert.Equal
        (
            new[] { 7, 8 },
            items
        );
    }



    [Fact]
    public async Task GetCollectedItems_when_collectItems_is_true_returns_independent_snapshot_on_each_call()
    {
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3 });
        var loader    = new TestLoader<int>(collectItems: true);

        await loader.LoadAsync(extractor.ExtractAsync());

        var first  = loader.GetCollectedItems();
        var second = loader.GetCollectedItems();

        // Each call returns a new list — mutating one does not affect the other.
        Assert.NotSame(first, second);
        Assert.Equal(first, second);
    }



    // ------------------------------------------------------------------
    // collectItems: false — null return
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_when_collectItems_is_false_GetCollectedItems_returns_null()
    {
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3 });
        var loader    = new TestLoader<int>(collectItems: false);

        await loader.LoadAsync(extractor.ExtractAsync());

        Assert.Null(loader.GetCollectedItems());
    }



    [Fact]
    public void GetCollectedItems_when_collectItems_is_true_before_any_load_returns_empty_list()
    {
        var loader = new TestLoader<int>(collectItems: true);

        var items = loader.GetCollectedItems();

        Assert.NotNull(items);
        Assert.Empty(items);
    }



    [Fact]
    public void GetCollectedItems_when_collectItems_is_false_before_any_load_returns_null()
    {
        var loader = new TestLoader<int>(collectItems: false);

        Assert.Null(loader.GetCollectedItems());
    }



    [Fact]
    public async Task LoadAsync_when_collectItems_is_false_still_enumerates_all_items()
    {
        // Verifies the loop runs even when not collecting — important for benchmarks.
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3, 4, 5 });
        var loader    = new TestLoader<int>(collectItems: false);

        await loader.LoadAsync(extractor.ExtractAsync());

        Assert.Equal(5, loader.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // CurrentItemCount
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_when_collectItems_is_true_increments_CurrentItemCount()
    {
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3 });
        var loader    = new TestLoader<int>(collectItems: true);

        await loader.LoadAsync(extractor.ExtractAsync());

        Assert.Equal(3, loader.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // CreateProgressReport
    // ------------------------------------------------------------------

    [Fact]
    public async Task CreateProgressReport_returns_Report_reflecting_CurrentItemCount()
    {
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3 });
        var loader = new ExposedTestLoader<int>(collectItems: false);

        await loader.LoadAsync(extractor.ExtractAsync());

        var report = loader.GetProgressReport();

        Assert.Equal(3, report.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // Full pipeline
    // ------------------------------------------------------------------

    [Fact]
    public async Task Full_pipeline_extractor_transformer_loader_produces_correct_results()
    {
        var extractor   = new TestExtractor<int>(new List<int> { 10, 20, 30 });
        var transformer = new TestTransformer<int>();
        var loader      = new TestLoader<int>(collectItems: true);

        await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync()));

        var items = loader.GetCollectedItems();

        Assert.NotNull(items);
        Assert.Equal
        (
            new[] { 10, 20, 30 },
            items
        );
        Assert.Equal(3, extractor.CurrentItemCount);
        Assert.Equal(3, transformer.CurrentItemCount);
        Assert.Equal(3, loader.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // SkipItemCount
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_skips_items_up_to_SkipItemCount()
    {
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3, 4, 5 });
        var loader    = new TestLoader<int>(collectItems: true);
        loader.SkipItemCount = 2;

        await loader.LoadAsync(extractor.ExtractAsync());

        var items = loader.GetCollectedItems();

        Assert.NotNull(items);
        Assert.Equal(new[] { 3, 4, 5 }, items);
    }



    // ------------------------------------------------------------------
    // MaximumItemCount
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_stops_at_MaximumItemCount()
    {
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3, 4, 5 });
        var loader    = new TestLoader<int>(collectItems: true);
        loader.MaximumItemCount = 2;

        await loader.LoadAsync(extractor.ExtractAsync());

        var items = loader.GetCollectedItems();

        Assert.NotNull(items);
        Assert.Equal(new[] { 1, 2 }, items);
    }



    // ------------------------------------------------------------------
    // Timer constructor — collectItems behaviour
    // ------------------------------------------------------------------

    [Fact]
    public async Task Constructor_with_timer_and_collectItems_true_collects_items()
    {
        using var timer = new ManualProgressTimer();
        var loader = new TestLoaderWithTimer(collectItems: true, timer);

        await loader.LoadAsync(new TestExtractor<int>(new List<int> { 1, 2, 3 }).ExtractAsync());

        var items = loader.GetCollectedItems();

        Assert.NotNull(items);
        Assert.Equal(new[] { 1, 2, 3 }, items);
    }



    [Fact]
    public async Task Constructor_with_timer_and_collectItems_false_does_not_collect_items()
    {
        using var timer = new ManualProgressTimer();
        var loader = new TestLoaderWithTimer(collectItems: false, timer);

        await loader.LoadAsync(new TestExtractor<int>(new List<int> { 1, 2, 3 }).ExtractAsync());

        Assert.Null(loader.GetCollectedItems());
        Assert.Equal(3, loader.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // Progress + injected timer / Dispose
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_with_progress_and_injected_timer_reports_progress_when_timer_fires()
    {
        using var timer = new ManualProgressTimer();
        var loader = new TestLoaderWithTimer(collectItems: true, timer);
        Report? captured = null;
        var progress = new SynchronousProgress<Report>(r => captured = r);
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var task = loader.LoadAsync(GatedSourceAsync(gate), progress);
        timer.Fire();
        gate.SetResult(true);
        await task;

        Assert.NotNull(captured);
        Assert.True(captured!.CurrentItemCount >= 1);
    }



    [Fact]
    public async Task Dispose_after_timer_wired_runs_unsubscribe_branch_without_error()
    {
        using var timer = new ManualProgressTimer();
        var progress = new SynchronousProgress<Report>(_ => { });

        var loader = new TestLoaderWithTimer(collectItems: false, timer);

        // A completed load wires (and leaves wired) the Elapsed handler. Disposing the
        // loader then exercises the unsubscribe branch of Dispose(bool). The injected
        // timer is owned by the caller, so disposing the loader must not dispose it.
        await loader.LoadAsync(new TestExtractor<int>(new List<int> { 1, 2, 3 }).ExtractAsync(), progress);

        var exception = Record.Exception(() => loader.Dispose());

        Assert.Null(exception);
    }



    private static async IAsyncEnumerable<int> GatedSourceAsync(TaskCompletionSource<bool> gate)
    {
        yield return 1;
        await gate.Task.ConfigureAwait(false);
        yield return 2;
        yield return 3;
    }



    private sealed class ExposedTestLoader<T>(bool collectItems) : TestLoader<T>(collectItems) where T : notnull
    {
        public Report GetProgressReport() => CreateProgressReport();
    }



    private sealed class TestLoaderWithTimer : TestLoader<int>
    {
        public TestLoaderWithTimer(bool collectItems, IProgressTimer timer)
            : base(collectItems, timer) { }
    }
}
