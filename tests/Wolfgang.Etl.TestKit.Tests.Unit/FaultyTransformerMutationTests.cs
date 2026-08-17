using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// Targeted tests that kill Stryker mutation survivors in <see cref="FaultyTransformer{T}"/>
/// (issue #346). Each test names the survivor it pins.
/// </summary>
public class FaultyTransformerMutationTests
{
    // ------------------------------------------------------------------
    // OnItemError policy ternary (Conditional)
    // ------------------------------------------------------------------

    [Fact]
    public void OnItemError_when_no_policy_configured_returns_base_Abort()
    {
        // Kills Conditional(true): forcing the ternary condition true would invoke the
        // null policy delegate and throw NullReferenceException instead of returning base.
        var transformer = new ExposedFaultyTransformer();

        var action = transformer.CallOnItemError
        (
            new ItemErrorContext(1, new InvalidOperationException())
        );

        Assert.Equal(ItemErrorAction.Abort, action);
    }



    [Fact]
    public void OnItemError_when_policy_configured_returns_policy_result()
    {
        // Kills Conditional(false): forcing the condition false would return base.Abort
        // rather than the configured policy's Skip.
        var transformer = new ExposedFaultyTransformer();
        transformer.HandleErrorsWith(_ => ItemErrorAction.Skip);

        var action = transformer.CallOnItemError
        (
            new ItemErrorContext(1, new InvalidOperationException())
        );

        Assert.Equal(ItemErrorAction.Skip, action);
    }



    // ------------------------------------------------------------------
    // HandleInjectedFault — Abort branch throws (Statement)
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_when_policy_returns_Abort_throws_without_counting_the_failing_item()
    {
        // Kills the `throw exception` Statement in the Abort branch: removing it would fall
        // through to the no-policy path, which counts the failing item before throwing.
        var expected    = new InvalidOperationException("abort");
        var extractor   = new FaultyExtractor<int>(new[] { 1, 2, 3, 4, 5 });
        var transformer = new FaultyTransformer<int>()
            .HandleErrorsWith(_ => ItemErrorAction.Abort)
            .ThrowAt(2, expected);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>
        (
            async () => await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync()
        );

        Assert.Same(expected, actual);

        // Items 0 and 1 were processed; the Abort branch throws before counting item 2.
        Assert.Equal(2, transformer.CurrentItemCount);
    }



    [Fact]
    public async Task TransformAsync_when_policy_returns_Skip_discards_the_failing_item_and_continues()
    {
        // Kills the `return true` Statement and the `== Skip` equality: the failing item is
        // dropped, counted as an error, and the run continues.
        var extractor   = new FaultyExtractor<int>(new[] { 10, 20, 30 });
        var transformer = new FaultyTransformer<int>()
            .SkipErrors()
            .ThrowAt(1, new InvalidOperationException("boom"));

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Equal(new[] { 10, 30 }, results);
        Assert.Equal(1, transformer.CurrentErrorItemCount);
        Assert.Single(transformer.CapturedErrors);
    }



    // ------------------------------------------------------------------
    // Progress-timer wiring seam (CreateProgressTimer)
    // ------------------------------------------------------------------

    [Fact]
    public async Task CreateProgressTimer_across_two_runs_wires_the_Elapsed_handler_exactly_once()
    {
        // Kills the wiring seam: `!_progressTimerWired` (never wire), `_progressTimerWired = true`
        // (re-wire every run -> two subscriptions), and `Elapsed +=` -> `-=` (never subscribe).
        using var timer = new CountingProgressTimer();
        var transformer = new TimedFaultyTransformer(timer);
        var reportCount = 0;
        var progress    = new SynchronousProgress<Report>(_ => reportCount++);

        await transformer
            .TransformAsync(new FaultyExtractor<int>(new[] { 1, 2, 3 }).ExtractAsync(), progress)
            .ToListAsync();
        await transformer
            .TransformAsync(new FaultyExtractor<int>(new[] { 4, 5, 6 }).ExtractAsync(), progress)
            .ToListAsync();

        // The guard latches after the first run, so exactly one handler is subscribed.
        Assert.Equal(1, timer.SubscriberCount);

        reportCount = 0;
        timer.Fire();

        Assert.Equal(1, reportCount);
    }



    // ------------------------------------------------------------------
    // Worker finally — StopTimer (Statement)
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_stops_the_injected_timer_when_the_worker_completes()
    {
        // Kills the `_progressTimer?.StopTimer()` Statement in the worker's finally.
        using var timer = new CountingProgressTimer();
        var transformer = new TimedFaultyTransformer(timer);

        await transformer
            .TransformAsync(new FaultyExtractor<int>(new[] { 1, 2, 3 }).ExtractAsync())
            .ToListAsync();

        Assert.Equal(1, timer.StopTimerCallCount);
    }



    // ------------------------------------------------------------------
    // Cancellation — pre-loop and in-loop ThrowIfCancellationRequested
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_with_empty_source_and_pre_cancelled_token_still_throws()
    {
        // Kills the pre-loop ThrowIfCancellationRequested Statement: with an empty source the
        // in-loop check never runs, so only the pre-loop check can throw. The source ignores
        // the token, isolating the transformer's own check.
        var transformer = new FaultyTransformer<int>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            async () =>
            {
                await foreach (var _ in transformer.TransformAsync(EmptyIgnoringCancellationAsync(), cts.Token))
                {
                }
            }
        );
    }



    [Fact]
    public async Task TransformAsync_cancelling_mid_stream_throws_and_stops_promptly()
    {
        // Kills the in-loop ThrowIfCancellationRequested Statement: the source ignores the
        // token, so only the transformer's per-iteration check can stop the stream.
        var transformer = new FaultyTransformer<int>();
        using var cts = new CancellationTokenSource();
        var processed = 0;

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            async () =>
            {
                await foreach (var _ in transformer.TransformAsync(CountingSourceIgnoringCancellationAsync(), cts.Token))
                {
                    processed++;
                    if (processed == 3)
                    {
#pragma warning disable CA1849, VSTHRD103 // sync Cancel() — CancelAsync is net8+ only
                        cts.Cancel();
#pragma warning restore CA1849, VSTHRD103
                    }
                }
            }
        );

        Assert.Equal(3, processed);
    }



    // ------------------------------------------------------------------
    // DuplicateAt guard — CurrentItemCount < MaximumItemCount boundary
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_when_DuplicateAt_would_exceed_MaximumItemCount_does_not_emit_the_duplicate()
    {
        // Kills the `CurrentItemCount < MaximumItemCount` guard on the duplicate path: after the
        // first item is counted, count (1) is not < max (1), so no duplicate is emitted.
        var extractor   = new FaultyExtractor<int>(new[] { 5, 6 });
        var transformer = new FaultyTransformer<int>().DuplicateAt(0);
        transformer.MaximumItemCount = 1;

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Equal(new[] { 5 }, results);
    }



    [Fact]
    public async Task TransformAsync_when_DuplicateAt_fits_within_MaximumItemCount_emits_the_duplicate()
    {
        // Complements the boundary test above: with room under the max, the duplicate IS emitted.
        var extractor   = new FaultyExtractor<int>(new[] { 5, 6 });
        var transformer = new FaultyTransformer<int>().DuplicateAt(0);
        transformer.MaximumItemCount = 2;

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Equal(new[] { 5, 5 }, results);
    }



    // ------------------------------------------------------------------
    // Dispose — no injected timer
    // ------------------------------------------------------------------

    [Fact]
    public void Dispose_when_no_injected_timer_does_not_throw()
    {
        var transformer = new FaultyTransformer<int>();

        var exception = Record.Exception(() => transformer.Dispose());

        Assert.Null(exception);
    }



    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static async IAsyncEnumerable<int> EmptyIgnoringCancellationAsync()
    {
        await Task.CompletedTask.ConfigureAwait(false);
        yield break;
    }



    private static async IAsyncEnumerable<int> CountingSourceIgnoringCancellationAsync()
    {
        for (var i = 1; i <= 100; i++)
        {
            await Task.Yield();
            yield return i;
        }
    }



    private sealed class ExposedFaultyTransformer : FaultyTransformer<int>
    {
        public ItemErrorAction CallOnItemError(ItemErrorContext context) => OnItemError(context);
    }



    private sealed class TimedFaultyTransformer : FaultyTransformer<int>
    {
        public TimedFaultyTransformer(IProgressTimer timer) : base(timer)
        {
        }
    }
}
