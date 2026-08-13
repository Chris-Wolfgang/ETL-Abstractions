using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

public class FaultyLoaderTests
{
    // ------------------------------------------------------------------
    // Constructor — argument validation
    // ------------------------------------------------------------------

    [Fact]
    public void Constructor_with_timer_when_timer_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new FaultyLoaderWithTimer(collectItems: false, null!)
        );
    }



    [Fact]
    public async Task Constructor_with_timer_creates_loader_that_collects_items()
    {
        using var timer = new ManualProgressTimer();
        var loader = new FaultyLoaderWithTimer(collectItems: true, timer);

        await loader.LoadAsync(new FaultyExtractor<int>(new[] { 1, 2, 3 }).ExtractAsync());

        Assert.Equal(new[] { 1, 2, 3 }, loader.GetCollectedItems());
    }



    // ------------------------------------------------------------------
    // No faults — pass-through
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_when_no_faults_configured_loads_all_items_in_order()
    {
        var loader = new FaultyLoader<int>(collectItems: true);

        await loader.LoadAsync(new FaultyExtractor<int>(new[] { 1, 2, 3 }).ExtractAsync());

        Assert.Equal
        (
            new[] { 1, 2, 3 },
            loader.GetCollectedItems()
        );
    }



    // ------------------------------------------------------------------
    // ThrowAt
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_when_ThrowAt_configured_throws_that_exception_reaching_the_index()
    {
        var expected = new TimeoutException("connection lost");
        var loader   = new FaultyLoader<int>(collectItems: true)
            .ThrowAt(2, expected);

        var actual = await Assert.ThrowsAsync<TimeoutException>
        (
            async () => await loader.LoadAsync(new FaultyExtractor<int>(new[] { 10, 20, 30, 40, 50 }).ExtractAsync())
        );

        Assert.Same(expected, actual);
        Assert.Equal(new[] { 10, 20 }, loader.GetCollectedItems());
    }



    [Fact]
    public async Task LoadAsync_when_ThrowAt_configured_counts_the_failing_item()
    {
        var loader = new FaultyLoader<int>(collectItems: true)
            .ThrowAt(2, new TimeoutException("connection lost"));

        await Assert.ThrowsAsync<TimeoutException>
        (
            async () => await loader.LoadAsync(new FaultyExtractor<int>(new[] { 10, 20, 30, 40, 50 }).ExtractAsync())
        );

        // Index 2 is the third item; the failing item is counted, so 2 + 1 == 3.
        Assert.Equal(3, loader.CurrentItemCount);
    }



    [Fact]
    public async Task LoadAsync_when_ThrowAt_called_twice_for_same_index_last_exception_wins()
    {
        var first  = new InvalidOperationException("first");
        var second = new TimeoutException("second");
        var loader = new FaultyLoader<int>(collectItems: false)
            .ThrowAt(1, first)
            .ThrowAt(1, second);

        var actual = await Assert.ThrowsAsync<TimeoutException>
        (
            async () => await loader.LoadAsync(new FaultyExtractor<int>(new[] { 1, 2, 3 }).ExtractAsync())
        );

        Assert.Same(second, actual);
    }



    // ------------------------------------------------------------------
    // ThrowAfterCompletion
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_when_ThrowAfterCompletion_configured_loads_all_items_then_throws()
    {
        var expected = new InvalidOperationException("commit failed");
        var loader   = new FaultyLoader<int>(collectItems: true)
            .ThrowAfterCompletion(expected);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>
        (
            async () => await loader.LoadAsync(new FaultyExtractor<int>(new[] { 1, 2, 3 }).ExtractAsync())
        );

        Assert.Same(expected, actual);
        Assert.Equal(new[] { 1, 2, 3 }, loader.GetCollectedItems());
        Assert.Equal(3, loader.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // DuplicateAt
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_when_DuplicateAt_configured_loads_that_item_twice_consecutively()
    {
        var loader = new FaultyLoader<int>(collectItems: true)
            .DuplicateAt(1);

        await loader.LoadAsync(new FaultyExtractor<int>(new[] { 1, 2, 3 }).ExtractAsync());

        Assert.Equal(new[] { 1, 2, 2, 3 }, loader.GetCollectedItems());
        Assert.Equal(4, loader.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // Composability
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_when_DuplicateAt_and_later_ThrowAt_configured_duplicate_loaded_then_throw_fires()
    {
        var expected = new TimeoutException("boom");
        var loader   = new FaultyLoader<int>(collectItems: true)
            .DuplicateAt(1)
            .ThrowAt(3, expected);

        var actual = await Assert.ThrowsAsync<TimeoutException>
        (
            async () => await loader.LoadAsync(new FaultyExtractor<int>(new[] { 1, 2, 3, 4, 5 }).ExtractAsync())
        );

        Assert.Same(expected, actual);
        Assert.Equal(new[] { 1, 2, 2, 3 }, loader.GetCollectedItems());
    }



    // ------------------------------------------------------------------
    // Argument validation
    // ------------------------------------------------------------------

    [Fact]
    public void ThrowAt_when_index_is_negative_throws_ArgumentOutOfRangeException()
    {
        var loader = new FaultyLoader<int>(collectItems: false);

        var ex = Assert.Throws<ArgumentOutOfRangeException>
        (
            () => loader.ThrowAt(-1, new InvalidOperationException())
        );

        Assert.Equal("index", ex.ParamName);
    }



    [Fact]
    public void ThrowAt_when_exception_is_null_throws_ArgumentNullException()
    {
        var loader = new FaultyLoader<int>(collectItems: false);

        var ex = Assert.Throws<ArgumentNullException>
        (
            () => loader.ThrowAt(0, null!)
        );

        Assert.Equal("exception", ex.ParamName);
    }



    [Fact]
    public void ThrowAfterCompletion_when_exception_is_null_throws_ArgumentNullException()
    {
        var loader = new FaultyLoader<int>(collectItems: false);

        var ex = Assert.Throws<ArgumentNullException>
        (
            () => loader.ThrowAfterCompletion(null!)
        );

        Assert.Equal("exception", ex.ParamName);
    }



    [Fact]
    public void DuplicateAt_when_index_is_negative_throws_ArgumentOutOfRangeException()
    {
        var loader = new FaultyLoader<int>(collectItems: false);

        var ex = Assert.Throws<ArgumentOutOfRangeException>
        (
            () => loader.DuplicateAt(-1)
        );

        Assert.Equal("index", ex.ParamName);
    }



    // ------------------------------------------------------------------
    // Fluent chaining
    // ------------------------------------------------------------------

    [Fact]
    public void ThrowAt_returns_same_instance()
    {
        var loader = new FaultyLoader<int>(collectItems: false);

        Assert.Same(loader, loader.ThrowAt(0, new InvalidOperationException()));
    }



    [Fact]
    public void ThrowAfterCompletion_returns_same_instance()
    {
        var loader = new FaultyLoader<int>(collectItems: false);

        Assert.Same(loader, loader.ThrowAfterCompletion(new InvalidOperationException()));
    }



    [Fact]
    public void DuplicateAt_returns_same_instance()
    {
        var loader = new FaultyLoader<int>(collectItems: false);

        Assert.Same(loader, loader.DuplicateAt(0));
    }



    // ------------------------------------------------------------------
    // Progress + injected timer
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_with_progress_and_injected_timer_reports_progress_when_timer_fires()
    {
        using var timer = new ManualProgressTimer();
        var loader = new FaultyLoaderWithTimer(collectItems: true, timer);
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
    public async Task LoadAsync_with_progress_and_no_injected_timer_loads_all_items()
    {
        var loader = new FaultyLoader<int>(collectItems: true);
        var progress = new SynchronousProgress<Report>(_ => { });

        await loader.LoadAsync(new FaultyExtractor<int>(new[] { 1, 2, 3 }).ExtractAsync(), progress);

        Assert.Equal(new[] { 1, 2, 3 }, loader.GetCollectedItems());
    }



    // ------------------------------------------------------------------
    // Dispose
    // ------------------------------------------------------------------

    [Fact]
    public async Task Dispose_after_timer_wired_runs_unsubscribe_branch_without_error()
    {
        using var timer = new ManualProgressTimer();
        var progress = new SynchronousProgress<Report>(_ => { });

        var loader = new FaultyLoaderWithTimer(collectItems: false, timer);

        // A completed load wires (and leaves wired) the Elapsed handler. Disposing the
        // loader then exercises the unsubscribe branch of Dispose(bool). The injected
        // timer is owned by the caller, so disposing the loader must not dispose it.
        await loader.LoadAsync(new FaultyExtractor<int>(new[] { 1, 2, 3 }).ExtractAsync(), progress);

        var exception = Record.Exception(() => loader.Dispose());

        Assert.Null(exception);
    }



    // ------------------------------------------------------------------
    // SkipItemCount / MaximumItemCount
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_skips_items_up_to_SkipItemCount()
    {
        var loader = new FaultyLoader<int>(collectItems: true);
        loader.SkipItemCount = 2;

        await loader.LoadAsync(new FaultyExtractor<int>(new[] { 1, 2, 3, 4, 5 }).ExtractAsync());

        Assert.Equal(new[] { 3, 4, 5 }, loader.GetCollectedItems());
        Assert.Equal(2, loader.CurrentSkippedItemCount);
    }



    [Fact]
    public async Task LoadAsync_stops_at_MaximumItemCount()
    {
        var loader = new FaultyLoader<int>(collectItems: true);
        loader.MaximumItemCount = 2;

        await loader.LoadAsync(new FaultyExtractor<int>(new[] { 1, 2, 3, 4, 5 }).ExtractAsync());

        Assert.Equal(new[] { 1, 2 }, loader.GetCollectedItems());
    }



    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static async IAsyncEnumerable<int> GatedSourceAsync(TaskCompletionSource<bool> gate)
    {
        yield return 1;
        await gate.Task.ConfigureAwait(false);
        yield return 2;
        yield return 3;
    }



    private sealed class FaultyLoaderWithTimer : FaultyLoader<int>
    {
        public FaultyLoaderWithTimer(bool collectItems, IProgressTimer timer)
            : base(collectItems, timer) { }
    }
}
