using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests that verify a stage honours cancellation
/// <em>promptly</em> — it stops shortly after the token is cancelled rather than draining its source,
/// throws <see cref="System.OperationCanceledException"/>, and processes nothing when handed an
/// already-cancelled token.
/// </summary>
/// <typeparam name="TSut">The system under test (an extractor, loader, or transformer).</typeparam>
/// <remarks>
/// <para>
/// This complements the per-overload cancellation checks on the extractor/loader/transformer contract
/// bases (which assert that cancellation eventually throws) by asserting the stronger property that a
/// mid-stream cancel takes effect <em>quickly</em> and leaves the processed-item count consistent.
/// Pair it with a latent source such as <c>DelayingExtractor&lt;T&gt;</c> so a cancel interrupts an
/// in-flight wait.
/// </para>
/// <para>
/// The derived class owns and drives its stage — the base never receives the SUT, so overrides need
/// no null-argument validation. Implement <see cref="RunAndCancelMidStreamAsync"/> (drive the stage
/// over <c>itemCount</c> items, cancel once <c>cancelAfter</c> items have been
/// processed, and report the <see cref="CancellationOutcome"/>) and
/// <see cref="RunWithPreCancelledTokenAsync"/> (drive the stage with an already-cancelled token).
/// Both must let <see cref="System.OperationCanceledException"/> propagate out of the run so the
/// harness can observe it via the reported outcome.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class MyExtractorCancellationTests
///     : CancellationContractTests&lt;MyExtractor&gt;
/// {
///     protected override async Task&lt;CancellationOutcome&gt; RunAndCancelMidStreamAsync(int itemCount, int cancelAfter)
///     {
///         var sut = new MyExtractor(source, count: itemCount);
///         using var cts = new CancellationTokenSource();
///         var processed = 0;
///         var canceled = false;
///         try
///         {
///             await foreach (var _ in sut.ExtractAsync(cts.Token))
///             {
///                 processed++;
///                 if (processed == cancelAfter) cts.Cancel();
///             }
///         }
///         catch (OperationCanceledException) { canceled = true; }
///         return new CancellationOutcome(canceled, processed);
///     }
///     // ... RunWithPreCancelledTokenAsync ...
/// }
/// </code>
/// </example>
public abstract class CancellationContractTests<TSut>
{
    /// <summary>The number of items the stage's source should make available for the run.</summary>
    protected virtual int ItemCount => 100;



    /// <summary>The number of processed items after which the mid-stream run cancels its token.</summary>
    protected virtual int CancelAfter => 5;



    /// <summary>
    /// The tolerance, in items, allowed between the cancel signal and the stage actually stopping. The
    /// default of 1 permits the item whose processing was already in flight when the token was cancelled.
    /// </summary>
    protected virtual int PromptStopSlack => 1;



    /// <summary>
    /// Drives the stage over <paramref name="itemCount"/> items and cancels the token once
    /// <paramref name="cancelAfter"/> items have been fully processed, letting
    /// <see cref="System.OperationCanceledException"/> propagate.
    /// </summary>
    /// <returns>The observed outcome — whether cancellation threw and how many items were processed.</returns>
    protected abstract Task<CancellationOutcome> RunAndCancelMidStreamAsync(int itemCount, int cancelAfter);



    /// <summary>
    /// Drives the stage over <paramref name="itemCount"/> items with an already-cancelled token,
    /// letting <see cref="System.OperationCanceledException"/> propagate.
    /// </summary>
    /// <returns>The observed outcome — whether cancellation threw and how many items were processed.</returns>
    protected abstract Task<CancellationOutcome> RunWithPreCancelledTokenAsync(int itemCount);



    /// <summary>
    /// Verifies that cancelling mid-stream surfaces <see cref="System.OperationCanceledException"/>.
    /// </summary>
    [Fact]
    public async Task Mid_stream_cancellation_throws_OperationCanceledException_Async()
    {
        var outcome = await RunAndCancelMidStreamAsync(ItemCount, CancelAfter).ConfigureAwait(false);

        Assert.True
        (
            outcome.Canceled,
            "Cancelling the token mid-stream should surface an OperationCanceledException."
        );
    }



    /// <summary>
    /// Verifies that the stage stops promptly after cancellation — within
    /// <see cref="PromptStopSlack"/> items of the cancel signal, and well before the source is drained.
    /// </summary>
    [Fact]
    public async Task Mid_stream_cancellation_stops_promptly_Async()
    {
        Assert.True(ItemCount > CancelAfter + PromptStopSlack, "ItemCount must exceed CancelAfter + slack for this test to be meaningful.");

        var outcome = await RunAndCancelMidStreamAsync(ItemCount, CancelAfter).ConfigureAwait(false);

        Assert.True
        (
            outcome.ProcessedItemCount <= CancelAfter + PromptStopSlack,
            $"Expected the stage to stop within {PromptStopSlack} item(s) of cancellation " +
            $"(≤ {CancelAfter + PromptStopSlack}), but it processed {outcome.ProcessedItemCount}."
        );

        Assert.True
        (
            outcome.ProcessedItemCount < ItemCount,
            $"The stage drained all {ItemCount} items instead of stopping on cancellation."
        );
    }



    /// <summary>
    /// Verifies that an already-cancelled token makes the stage process nothing and throw
    /// <see cref="System.OperationCanceledException"/>.
    /// </summary>
    [Fact]
    public async Task Pre_cancelled_token_processes_nothing_and_throws_Async()
    {
        var outcome = await RunWithPreCancelledTokenAsync(ItemCount).ConfigureAwait(false);

        Assert.True(outcome.Canceled, "An already-cancelled token should surface an OperationCanceledException.");
        Assert.Equal(0, outcome.ProcessedItemCount);
    }
}
