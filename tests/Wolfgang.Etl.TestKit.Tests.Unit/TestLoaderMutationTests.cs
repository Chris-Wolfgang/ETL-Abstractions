using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// Targeted tests that kill Stryker mutation survivors in <see cref="TestLoader{T}"/>
/// (issue #346). Each test names the survivor it pins.
/// </summary>
public class TestLoaderMutationTests
{
    // ------------------------------------------------------------------
    // CreateProgressTimer — `_progressTimer is null` guard (Equality)
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_with_progress_and_no_injected_timer_loads_without_error()
    {
        // Kills the `_progressTimer is null` Equality: flipping it to `is not null` skips the
        // base-timer path for a default-constructed loader and dereferences the null timer.
        var loader   = new TestLoader<int>(collectItems: false);
        var progress = new SynchronousProgress<Report>(_ => { });

        await loader.LoadAsync
        (
            new TestExtractor<int>(new List<int> { 1, 2, 3 }).ExtractAsync(),
            progress
        );

        Assert.Equal(3, loader.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // Progress-timer wiring seam
    // ------------------------------------------------------------------

    [Fact]
    public async Task CreateProgressTimer_across_two_runs_wires_the_Elapsed_handler_exactly_once()
    {
        // Kills the wiring seam: `!_progressTimerWired` (never wire), `_progressTimerWired = true`
        // (re-wire every run -> two subscriptions), and `Elapsed +=` -> `-=` (never subscribe).
        using var timer = new CountingProgressTimer();
        var loader      = new TimedTestLoader(collectItems: false, timer);
        var reportCount = 0;
        var progress    = new SynchronousProgress<Report>(_ => reportCount++);

        await loader.LoadAsync(new TestExtractor<int>(new List<int> { 1, 2, 3 }).ExtractAsync(), progress);
        await loader.LoadAsync(new TestExtractor<int>(new List<int> { 4, 5, 6 }).ExtractAsync(), progress);

        Assert.Equal(1, timer.SubscriberCount);

        reportCount = 0;
        timer.Fire();

        Assert.Equal(1, reportCount);
    }



    // ------------------------------------------------------------------
    // Dispose — unsubscribe guard (Equality ×2) and Elapsed -= (assignment)
    // ------------------------------------------------------------------

    [Fact]
    public async Task Dispose_after_timer_wired_unsubscribes_so_firing_produces_no_report()
    {
        // Kills the Dispose guard's `is not null` Equality mutations (either flip would skip the
        // unsubscribe) and the `Elapsed -=` -> `+=` mutation (which would add a second handler).
        using var timer = new CountingProgressTimer();
        var reportCount = 0;
        var progress    = new SynchronousProgress<Report>(_ => reportCount++);
        var loader      = new TimedTestLoader(collectItems: false, timer);

        await loader.LoadAsync(new TestExtractor<int>(new List<int> { 1, 2, 3 }).ExtractAsync(), progress);

        Assert.Equal(1, timer.SubscriberCount);

        loader.Dispose();

        Assert.Equal(0, timer.SubscriberCount);

        reportCount = 0;
        timer.Fire();

        Assert.Equal(0, reportCount);
    }



    // ------------------------------------------------------------------
    // Dispose — base.Dispose(disposing) (Statement)
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_after_Dispose_throws_ObjectDisposedException()
    {
        // Kills the `base.Dispose(disposing)` Statement: without it the disposed flag is never
        // set, so the use-after-dispose guard would not fire.
        var loader = new TestLoader<int>(collectItems: true);
        loader.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>
        (
            async () => await loader.LoadAsync(new TestExtractor<int>(new List<int> { 1 }).ExtractAsync())
        );
    }



    // ------------------------------------------------------------------
    // Worker finally — StopTimer (Statement)
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_stops_the_injected_timer_when_the_worker_completes()
    {
        // Kills the `_progressTimer?.StopTimer()` Statement in the worker's finally.
        using var timer = new CountingProgressTimer();
        var loader = new TimedTestLoader(collectItems: false, timer);

        await loader.LoadAsync(new TestExtractor<int>(new List<int> { 1, 2, 3 }).ExtractAsync());

        Assert.Equal(1, timer.StopTimerCallCount);
    }



    // ------------------------------------------------------------------
    // Cancellation — pre-loop and in-loop ThrowIfCancellationRequested
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_with_empty_source_and_pre_cancelled_token_still_throws()
    {
        // Kills the pre-loop ThrowIfCancellationRequested Statement: with an empty, token-ignoring
        // source the in-loop check never runs, so only the pre-loop check can throw.
        var loader = new TestLoader<int>(collectItems: true);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            async () => await loader.LoadAsync(EmptyIgnoringCancellationAsync(), cts.Token)
        );
    }



    [Fact]
    public async Task LoadAsync_cancelling_mid_stream_throws_after_processing_the_items_seen_so_far()
    {
        // Kills the in-loop ThrowIfCancellationRequested Statement: the source cancels the token
        // as it produces the 4th item but never observes the token itself, so only the loader's
        // per-iteration check can stop the load.
        using var cts = new CancellationTokenSource();
        var loader = new TestLoader<int>(collectItems: true);

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            async () => await loader.LoadAsync(CancelAfterThreeAsync(cts), cts.Token)
        );

        Assert.Equal(3, loader.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // Buffering guard — `_collectItems && !IsDryRun` (Logical / Boolean)
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_when_collectItems_true_and_IsDryRun_true_enumerates_but_buffers_nothing()
    {
        // Kills the buffering guard mutations: `&&` -> `||` and dropping `!` would both buffer the
        // items in dry-run mode. The items must be counted but not collected.
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3 });
        var loader    = new TestLoader<int>(collectItems: true) { IsDryRun = true };

        await loader.LoadAsync(extractor.ExtractAsync());

        var items = loader.GetCollectedItems();

        Assert.NotNull(items);
        Assert.Empty(items);
        Assert.Equal(3, loader.CurrentItemCount);
    }



    [Fact]
    public async Task LoadAsync_when_collectItems_true_and_IsDryRun_false_buffers_the_items()
    {
        // Complements the dry-run test: the buffering guard must be true when not dry-running.
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3 });
        var loader    = new TestLoader<int>(collectItems: true) { IsDryRun = false };

        await loader.LoadAsync(extractor.ExtractAsync());

        Assert.Equal(new[] { 1, 2, 3 }, loader.GetCollectedItems());
    }



    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    [ExcludeFromCodeCoverage]
    private static async IAsyncEnumerable<int> EmptyIgnoringCancellationAsync()
    {
        await Task.CompletedTask.ConfigureAwait(false);
        yield break;
    }



    private static async IAsyncEnumerable<int> CancelAfterThreeAsync(CancellationTokenSource cts)
    {
        for (var i = 1; i <= 100; i++)
        {
            if (i == 4)
            {
#pragma warning disable CA1849, VSTHRD103 // sync Cancel() — CancelAsync is net8+ only
                cts.Cancel();
#pragma warning restore CA1849, VSTHRD103
            }

            await Task.Yield();
            yield return i;
        }
    }



    private sealed class TimedTestLoader : TestLoader<int>
    {
        public TimedTestLoader(bool collectItems, IProgressTimer timer) : base(collectItems, timer)
        {
        }
    }
}
