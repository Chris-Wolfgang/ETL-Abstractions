using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// Mutation-hardening tests (#346) for <see cref="FaultyLoader{T}"/> — each targets a
/// specific surviving mutant that the behavioural tests do not otherwise pin down.
/// </summary>
public class FaultyLoaderMutationTests
{
    // ------------------------------------------------------------------
    // GetCollectedItems ternary — L123
    // ------------------------------------------------------------------

    [Fact]
    public void GetCollectedItems_when_collectItems_is_false_returns_null()
    {
        // The always-true conditional mutant returns an (empty) array instead of null.
        var loader = new FaultyLoader<int>(collectItems: false);

        Assert.Null(loader.GetCollectedItems());
    }



    // ------------------------------------------------------------------
    // OnItemError ternary (base-vs-policy) — L296
    // ------------------------------------------------------------------

    [Fact]
    public void OnItemError_when_no_policy_configured_falls_back_to_base_abort()
    {
        var probe = new OnItemErrorProbe();

        var action = probe.Probe(new ItemErrorContext(1, new IOException("boom")));

        Assert.Equal(ItemErrorAction.Abort, action);
    }



    [Fact]
    public void OnItemError_when_policy_configured_uses_the_policy_result()
    {
        var probe = new OnItemErrorProbe();
        probe.SkipErrors();

        var action = probe.Probe(new ItemErrorContext(1, new IOException("boom")));

        Assert.Equal(ItemErrorAction.Skip, action);
    }



    // ------------------------------------------------------------------
    // HandleInjectedFault — Abort path must not count the item — L317
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_when_policy_returns_Abort_does_not_count_the_failing_item()
    {
        var loader = new FaultyLoader<int>(collectItems: false)
            .ThrowAt(2, new IOException("fatal"))
            .HandleErrorsWith(_ => ItemErrorAction.Abort);

        await Assert.ThrowsAsync<IOException>
        (
            async () => await loader.LoadAsync(Enumerable.Range(1, 5).ToAsyncEnumerable())
        );

        Assert.Equal(2, loader.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // Progress-timer wiring seam — L332 / L334 / L336
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_firing_the_injected_timer_reports_progress_before_completion()
    {
        // Capture the report count while the load is still gated, so only a wired Elapsed
        // handler (not the forced completion report) can satisfy it. Kills the "never wire"
        // and "-=" mutants.
        using var timer = new ManualProgressTimer();
        var loader = new LoaderTimerProbe(collectItems: false, timer);
        var reports = 0;
        var progress = new SynchronousProgress<Report>(_ => reports++);
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var task = loader.LoadAsync(GatedSourceAsync(gate), progress);
        timer.Fire();
        var afterFire = reports;
        gate.SetResult(true);
        await task;

        Assert.True(afterFire >= 1, $"Firing the injected timer must report progress; saw {afterFire}.");
    }



    [Fact]
    public async Task LoadAsync_wires_the_elapsed_handler_only_once_across_runs()
    {
        // Two overlapping loads on one instance make CreateProgressTimer run twice against the
        // same (undisposed) timer. With the _progressTimerWired guard exactly one handler is
        // subscribed, so one Fire() yields one report; the "wired = false" mutant subscribes
        // twice, yielding two.
        using var timer = new ManualProgressTimer();
        var loader = new LoaderTimerProbe(collectItems: false, timer);
        var reports = 0;
        var progress = new SynchronousProgress<Report>(_ => reports++);
        var g1 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var g2 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var t1 = loader.LoadAsync(GatedSourceAsync(g1), progress);
        var t2 = loader.LoadAsync(GatedSourceAsync(g2), progress);

        reports = 0;
        timer.Fire();
        var afterFire = reports;

        g1.SetResult(true);
        g2.SetResult(true);
        await Task.WhenAll(t1, t2);

        Assert.Equal(1, afterFire);
    }



    // ------------------------------------------------------------------
    // Dispose(bool) unsubscribe guard — L354 / L356
    // ------------------------------------------------------------------

    [Fact]
    public async Task Dispose_with_disposing_true_unsubscribes_the_handler()
    {
        // A completed-then-disposed loader must not report again when the timer fires. Kills the
        // negate/equality mutants that leave the handler subscribed, and the "+=" mutant that
        // subscribes a second time.
        using var timer = new ManualProgressTimer();
        var loader = new LoaderTimerProbe(collectItems: false, timer);
        var reports = 0;
        var progress = new SynchronousProgress<Report>(_ => reports++);
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var task = loader.LoadAsync(GatedSourceAsync(gate), progress);
        loader.Dispose();
        reports = 0;
        timer.Fire();
        var afterFire = reports;

        gate.SetResult(true);
        await task;

        Assert.Equal(0, afterFire);
    }



    [Fact]
    public async Task Dispose_with_disposing_false_does_not_unsubscribe_the_handler()
    {
        // The guard requires disposing == true; the logical-operator mutants make it true on the
        // finalizer path (disposing == false), which would unsubscribe and silence reports.
        using var timer = new ManualProgressTimer();
        var loader = new LoaderTimerProbe(collectItems: false, timer);
        var reports = 0;
        var progress = new SynchronousProgress<Report>(_ => reports++);
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var task = loader.LoadAsync(GatedSourceAsync(gate), progress);
        loader.CallDispose(disposing: false);
        reports = 0;
        timer.Fire();
        var afterFire = reports;

        gate.SetResult(true);
        await task;

        Assert.Equal(1, afterFire);
    }



    // ------------------------------------------------------------------
    // base.Dispose(disposing) is invoked — L360
    // ------------------------------------------------------------------

    [Fact]
    public async Task Dispose_marks_the_loader_disposed_so_a_later_LoadAsync_throws()
    {
        // Removing base.Dispose(disposing) leaves _disposed unset, so the use-after-dispose
        // guard on LoadAsync would no longer fire.
        var loader = new FaultyLoader<int>(collectItems: false);
        loader.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>
        (
            async () => await loader.LoadAsync(new[] { 1 }.ToAsyncEnumerable())
        );
    }



    // ------------------------------------------------------------------
    // Cancellation checks — L378 (pre-loop) / L388 (in-loop, skip phase)
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_with_no_items_and_a_pre_cancelled_token_still_throws()
    {
        var loader = new FaultyLoader<int>(collectItems: false);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            async () => await loader.LoadAsync(EmptyIgnoringTokenAsync(), cts.Token)
        );
    }



    [Fact]
    public async Task LoadAsync_cancelling_during_the_skip_phase_throws_promptly()
    {
        using var cts = new CancellationTokenSource();
        var loader = new FaultyLoader<int>(collectItems: false);
        loader.SkipItemCount = 10;

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            async () => await loader.LoadAsync(CancellingSkipSourceAsync(cts), cts.Token)
        );

        Assert.Equal(2, loader.CurrentSkippedItemCount);
    }



    // ------------------------------------------------------------------
    // _buffer.Clear() between runs — L380
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_clears_the_buffer_between_runs()
    {
        var loader = new FaultyLoader<int>(collectItems: true);

        await loader.LoadAsync(new[] { 1, 2 }.ToAsyncEnumerable());
        await loader.LoadAsync(new[] { 3, 4 }.ToAsyncEnumerable());

        // Without the clear, the second run's buffer would still hold the first run's items.
        Assert.Equal(new[] { 3, 4 }, loader.GetCollectedItems());
    }



    // ------------------------------------------------------------------
    // DuplicateAt boundary against MaximumItemCount — L415
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_when_duplicate_would_exceed_MaximumItemCount_suppresses_it()
    {
        var loader = new FaultyLoader<int>(collectItems: true).DuplicateAt(1);
        loader.MaximumItemCount = 2;

        await loader.LoadAsync(new[] { 1, 2, 3 }.ToAsyncEnumerable());

        Assert.Equal(new[] { 1, 2 }, loader.GetCollectedItems());
        Assert.Equal(2, loader.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // finally block — StopTimer — L430
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_stops_the_injected_timer_in_the_finally()
    {
        var timer = new RecordingProgressTimer();
        var loader = new LoaderTimerProbe(collectItems: false, timer);

        await loader.LoadAsync(new[] { 1, 2, 3 }.ToAsyncEnumerable());

        Assert.True(timer.StopTimerCallCount >= 1, "The worker's finally must call StopTimer.");
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



    private static async IAsyncEnumerable<int> EmptyIgnoringTokenAsync
    (
        [EnumeratorCancellation] CancellationToken token = default
    )
    {
        // Ignores the token so only the loader's own pre-loop check can observe cancellation.
        _ = token;
        await Task.CompletedTask.ConfigureAwait(false);
        yield break;
    }



    private static async IAsyncEnumerable<int> CancellingSkipSourceAsync
    (
        CancellationTokenSource cts,
        [EnumeratorCancellation] CancellationToken token = default
    )
    {
        // Ignores the incoming token; cancels the source's own CTS mid-skip so only the loader's
        // in-loop check can observe it.
        _ = token;
        yield return 1;    // skipped
        yield return 2;    // skipped
        cts.Cancel();      // still in the skip phase
        yield return 3;    // in-loop check throws before the skip branch
        yield return 4;
    }



    // ------------------------------------------------------------------
    // Test doubles
    // ------------------------------------------------------------------

    private sealed class OnItemErrorProbe : FaultyLoader<int>
    {
        public OnItemErrorProbe() : base(collectItems: false) { }


        public ItemErrorAction Probe(ItemErrorContext context) => OnItemError(context);
    }



    private sealed class LoaderTimerProbe : FaultyLoader<int>
    {
        public LoaderTimerProbe(bool collectItems, IProgressTimer timer) : base(collectItems, timer) { }


        public void CallDispose(bool disposing) => Dispose(disposing);
    }



    private sealed class RecordingProgressTimer : IProgressTimer
    {
        public int StopTimerCallCount { get; private set; }


#pragma warning disable CS0067 // Elapsed is never fired by this recording double.
        public event Action? Elapsed;
#pragma warning restore CS0067


        public void Start(int intervalMilliseconds) { }


        public void StopTimer() => StopTimerCallCount++;


        public void Dispose() { }
    }
}
