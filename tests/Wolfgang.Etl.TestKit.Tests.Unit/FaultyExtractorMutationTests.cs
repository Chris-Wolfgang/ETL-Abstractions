using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// Mutation-hardening tests (#346) for <see cref="FaultyExtractor{T}"/> — each targets a
/// specific surviving mutant that the behavioural tests do not otherwise pin down.
/// </summary>
public class FaultyExtractorMutationTests
{
    // ------------------------------------------------------------------
    // OnItemError ternary (base-vs-policy) — L285
    // ------------------------------------------------------------------

    [Fact]
    public void OnItemError_when_no_policy_configured_falls_back_to_base_abort()
    {
        // With no policy the ternary must take the base branch (Abort). The always-true
        // conditional mutant would instead invoke the null policy delegate and throw.
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
    // HandleInjectedFault — Abort path must not count the item — L306
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_when_policy_returns_Abort_does_not_count_the_failing_item()
    {
        // With a policy present and an Abort decision, the throw fires BEFORE the fail-fast
        // IncrementCurrentItemCount. Removing that throw would fall through to the counting
        // path, so the count would be one higher.
        var sut = new FaultyExtractor<int>(new[] { 1, 2, 3, 4, 5 })
            .ThrowAt(2, new IOException("fatal"))
            .HandleErrorsWith(_ => ItemErrorAction.Abort);

        await Assert.ThrowsAsync<IOException>
        (
            async () => await sut.ExtractAsync().ToListAsync()
        );

        // Items at index 0 and 1 were counted; the index-2 fault aborts without counting.
        Assert.Equal(2, sut.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // Progress-timer wiring seam — L321 / L323 / L325
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_firing_the_injected_timer_reports_progress_before_completion()
    {
        // The base always reports once on completion, so asserting a report exists is not
        // enough. Capture the count immediately after Fire() — before the run finishes — so
        // only a wired Elapsed handler can satisfy it. Kills the "never wire" (!wired -> wired)
        // and the "-=" (subscribe -> unsubscribe) mutants.
        using var timer = new ManualProgressTimer();
        var sut = new TimerProbe(new List<int> { 1, 2, 3 }, timer);
        var reports = 0;
        var progress = new SynchronousProgress<Report>(_ => reports++);

        var enumerator = sut.ExtractAsync(progress).GetAsyncEnumerator();
        await enumerator.MoveNextAsync();
        timer.Fire();
        var afterFire = reports;

        Assert.True(afterFire >= 1, $"Firing the injected timer must report progress; saw {afterFire}.");
    }



    [Fact]
    public async Task ExtractAsync_wires_the_elapsed_handler_only_once_across_runs()
    {
        // The _progressTimerWired guard prevents a duplicate Elapsed subscription when the
        // timer is reused. Start two overlapping runs on one instance so CreateProgressTimer
        // runs twice against the same (undisposed) timer: with the guard, exactly one handler
        // is subscribed, so one Fire() yields one report. The "wired = false" mutant subscribes
        // twice, yielding two.
        using var timer = new ManualProgressTimer();
        var sut = new TimerProbe(new List<int> { 1, 2, 3 }, timer);
        var reports = 0;
        var progress = new SynchronousProgress<Report>(_ => reports++);

        var first = sut.ExtractAsync(progress).GetAsyncEnumerator();
        await first.MoveNextAsync();
        var second = sut.ExtractAsync(progress).GetAsyncEnumerator();
        await second.MoveNextAsync();

        reports = 0;
        timer.Fire();

        Assert.Equal(1, reports);
    }



    // ------------------------------------------------------------------
    // Dispose(bool) unsubscribe guard — L343
    // ------------------------------------------------------------------

    [Fact]
    public async Task Dispose_with_disposing_false_does_not_unsubscribe_the_handler()
    {
        // The guard requires disposing == true. The logical-operator mutants make it true on
        // the finalizer path (disposing == false), which would unsubscribe the handler and
        // silence subsequent timer reports.
        using var timer = new ManualProgressTimer();
        var sut = new TimerProbe(new List<int> { 1, 2, 3 }, timer);
        var reports = 0;
        var progress = new SynchronousProgress<Report>(_ => reports++);

        var enumerator = sut.ExtractAsync(progress).GetAsyncEnumerator();
        await enumerator.MoveNextAsync();

        sut.CallDispose(disposing: false);
        reports = 0;
        timer.Fire();

        Assert.Equal(1, reports);
    }



    [Fact]
    public void Dispose_when_no_timer_was_injected_does_not_throw()
    {
        // Without an injected timer the guard is false and the body is skipped. A mutant that
        // ignores the null-timer operand would dereference the null timer and throw.
        var sut = new FaultyExtractor<int>(new[] { 1, 2, 3 });

        var exception = Record.Exception(() => sut.Dispose());

        Assert.Null(exception);
    }



    // ------------------------------------------------------------------
    // Cancellation checks — L370 (pre-loop) / L384 (in-loop, skip phase)
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_with_no_items_and_a_pre_cancelled_token_still_throws()
    {
        // With an empty source the in-loop check never runs, so only the pre-loop
        // ThrowIfCancellationRequested can observe the cancellation.
        var sut = new FaultyExtractor<int>(Array.Empty<int>());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            async () =>
            {
                await foreach (var _ in sut.ExtractAsync(cts.Token))
                {
                }
            }
        );
    }



    [Fact]
    public async Task ExtractAsync_cancelling_during_the_skip_phase_throws_promptly()
    {
        // Skip everything, and have the source cancel the token mid-skip. The token is NOT
        // cancelled at method entry, so the pre-loop check passes; only the in-loop check at
        // the top of the loop can catch it during the skip run.
        using var cts = new CancellationTokenSource();

        IEnumerable<int> Source()
        {
            yield return 1;    // skipped
            yield return 2;    // skipped
            cts.Cancel();      // still in the skip phase
            yield return 3;    // in-loop check throws before the skip branch
            yield return 4;
        }

        var sut = new FaultyExtractor<int>(Source()) { SkipItemCount = 10 };

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            async () =>
            {
                await foreach (var _ in sut.ExtractAsync(cts.Token))
                {
                }
            }
        );

        // Only the first two were reached before the cancel; a dropped in-loop check would
        // instead skip all four and complete without throwing.
        Assert.Equal(2, sut.CurrentSkippedItemCount);
    }



    // ------------------------------------------------------------------
    // DuplicateAt boundary against MaximumItemCount — L410
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_when_duplicate_would_exceed_MaximumItemCount_suppresses_it()
    {
        // After yielding item at index 1, CurrentItemCount == MaximumItemCount (2). The guard
        // uses strict "<", so the duplicate is suppressed. The "<="/"off-by-one" mutant would
        // emit a third item.
        var sut = new FaultyExtractor<int>(new[] { 1, 2, 3 })
            .DuplicateAt(1);
        sut.MaximumItemCount = 2;

        var results = await sut.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 1, 2 }, results);
        Assert.Equal(2, sut.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // finally block — StopTimer (L421) and enumerator.Dispose (L422)
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_stops_the_injected_timer_in_the_finally()
    {
        var timer = new RecordingProgressTimer();
        var sut = new TimerProbe(new List<int> { 1, 2, 3 }, timer);

        await sut.ExtractAsync().ToListAsync();

        Assert.True(timer.StopTimerCallCount >= 1, "The worker's finally must call StopTimer.");
    }



    [Fact]
    public async Task ExtractAsync_disposes_the_source_enumerator_in_the_finally()
    {
        var source = new DisposeTrackingEnumerable<int>(new[] { 1, 2, 3 });
        var sut = new FaultyExtractor<int>(source);

        await sut.ExtractAsync().ToListAsync();

        Assert.True(source.EnumeratorDisposed, "The worker's finally must dispose the source enumerator.");
    }



    // ------------------------------------------------------------------
    // Test doubles
    // ------------------------------------------------------------------

    private sealed class OnItemErrorProbe : FaultyExtractor<int>
    {
        public OnItemErrorProbe() : base(new[] { 1 }) { }


        public ItemErrorAction Probe(ItemErrorContext context) => OnItemError(context);
    }



    private sealed class TimerProbe : FaultyExtractor<int>
    {
        public TimerProbe(IEnumerable<int> items, IProgressTimer timer) : base(items, timer) { }


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



    private sealed class DisposeTrackingEnumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _inner;


        public DisposeTrackingEnumerable(IEnumerable<T> inner) => _inner = inner;


        public bool EnumeratorDisposed { get; private set; }


        public IEnumerator<T> GetEnumerator() => new TrackingEnumerator(this, _inner.GetEnumerator());


        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        private sealed class TrackingEnumerator : IEnumerator<T>
        {
            private readonly DisposeTrackingEnumerable<T> _owner;
            private readonly IEnumerator<T> _inner;


            public TrackingEnumerator(DisposeTrackingEnumerable<T> owner, IEnumerator<T> inner)
            {
                _owner = owner;
                _inner = inner;
            }


            public T Current => _inner.Current;


            object? IEnumerator.Current => Current;


            public bool MoveNext() => _inner.MoveNext();


            public void Reset() => _inner.Reset();


            public void Dispose()
            {
                _owner.EnumeratorDisposed = true;
                _inner.Dispose();
            }
        }
    }



    [Fact]
    public async Task Dispose_marks_the_extractor_disposed_so_a_later_ExtractAsync_throws()
    {
        // Removing base.Dispose(disposing) leaves _disposed unset, so the use-after-dispose
        // guard on ExtractAsync would no longer fire.
        var extractor = new FaultyExtractor<int>(new[] { 1 });
        extractor.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>
        (
            async () => await extractor.ExtractAsync().ToListAsync()
        );
    }
}
