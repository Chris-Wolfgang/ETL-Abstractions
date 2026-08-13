using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests for a stage that opts into the
/// per-item error hook introduced in <c>Wolfgang.Etl.Abstractions</c> 0.18.0
/// (<c>OnItemError</c> / <c>HandleItemError</c> / <c>CurrentErrorItemCount</c>,
/// <see cref="ItemErrorAction"/>, <see cref="ItemErrorContext"/>).
/// </summary>
/// <typeparam name="TSut">The stage type under test.</typeparam>
/// <remarks>
/// <para>
/// Inherit from this class to verify that your stage honours the error-hook contract: that a
/// policy of <see cref="ItemErrorAction.Skip"/> discards the failed item and lets the run
/// continue while counting it as an <em>error</em>, and that a policy of
/// <see cref="ItemErrorAction.Abort"/> (the default) re-throws. The hook is opt-in — the base
/// classes do not catch a worker's per-item failure — so this contract is the guarantee that an
/// opted-in stage skips-and-continues correctly and never absorbs a failure into its intentional
/// skip budget.
/// </para>
/// <para>
/// The base is stage-agnostic: implement <see cref="RunSingleFaultScenarioAsync"/> to create
/// your stage configured with the supplied policy over a scenario that contains
/// <em>exactly one failing item</em> plus at least one item that succeeds, drive it to
/// completion (catching the re-thrown failure when the policy aborts), and report the resulting
/// counters via an <see cref="ErrorHandlingOutcome"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyExtractorErrorHandlingTests
///     : ErrorHandlingContractTests&lt;MyExtractor&gt;
/// {
///     protected override async Task&lt;ErrorHandlingOutcome&gt; RunSingleFaultScenarioAsync(ItemErrorAction policy)
///     {
///         var sut = new MyExtractor(SourceWithOneBadRow()) { ErrorPolicy = policy };
///         var aborted = false;
///         try { await foreach (var _ in sut.ExtractAsync()) { } }
///         catch { aborted = true; }
///         return new ErrorHandlingOutcome(aborted, sut.CurrentItemCount, sut.CurrentErrorItemCount, sut.CurrentSkippedItemCount);
///     }
/// }
/// </code>
/// </example>
public abstract class ErrorHandlingContractTests<TSut>
    where TSut : notnull
{
    // ------------------------------------------------------------------
    // Factory / harness method
    // ------------------------------------------------------------------

    /// <summary>
    /// Creates the stage under test configured with the supplied error <paramref name="policy"/>
    /// over a scenario containing <em>exactly one failing item</em> and at least one item that
    /// succeeds, drives it to completion (catching the re-thrown failure if the policy aborts),
    /// and reports the resulting counters.
    /// </summary>
    /// <param name="policy">
    /// The action the stage's error policy returns for the failing item —
    /// <see cref="ItemErrorAction.Skip"/> or <see cref="ItemErrorAction.Abort"/>.
    /// </param>
    /// <returns>The observable outcome of the run.</returns>
    protected abstract Task<ErrorHandlingOutcome> RunSingleFaultScenarioAsync(ItemErrorAction policy);



    // ------------------------------------------------------------------
    // Skip policy contract
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that a <see cref="ItemErrorAction.Skip"/> policy lets the run complete rather
    /// than re-throwing the failing item's exception.
    /// </summary>
    [Fact]
    public async Task Skip_policy_completes_the_run_Async()
    {
        var outcome = await RunSingleFaultScenarioAsync(ItemErrorAction.Skip).ConfigureAwait(false);

        Assert.False(outcome.Aborted, "Expected a Skip policy to complete the run, not abort.");
    }

    /// <summary>
    /// Verifies that a <see cref="ItemErrorAction.Skip"/> policy counts the failed item as an
    /// error (<c>CurrentErrorItemCount</c>), so the failure is never silent.
    /// </summary>
    [Fact]
    public async Task Skip_policy_counts_the_failure_as_an_error_Async()
    {
        var outcome = await RunSingleFaultScenarioAsync(ItemErrorAction.Skip).ConfigureAwait(false);

        Assert.Equal(1, outcome.ErrorItemCount);
    }

    /// <summary>
    /// Verifies that a <see cref="ItemErrorAction.Skip"/> policy does <em>not</em> fold the
    /// failure into <c>CurrentSkippedItemCount</c> — the 0.18 guarantee that an error-skip is
    /// distinct from an intentional skip.
    /// </summary>
    [Fact]
    public async Task Skip_policy_does_not_count_the_failure_as_an_intentional_skip_Async()
    {
        var outcome = await RunSingleFaultScenarioAsync(ItemErrorAction.Skip).ConfigureAwait(false);

        Assert.Equal(0, outcome.SkippedItemCount);
    }

    /// <summary>
    /// Verifies that a <see cref="ItemErrorAction.Skip"/> policy still processes the items that
    /// do not fail — the run is not vacuously "completed" by producing nothing.
    /// </summary>
    [Fact]
    public async Task Skip_policy_processes_the_surviving_items_Async()
    {
        var outcome = await RunSingleFaultScenarioAsync(ItemErrorAction.Skip).ConfigureAwait(false);

        Assert.True(outcome.ItemCount > 0, "Expected the surviving items to be processed after a skipped failure.");
    }



    // ------------------------------------------------------------------
    // Abort policy contract
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that an <see cref="ItemErrorAction.Abort"/> policy re-throws the failing item's
    /// exception, aborting the run (the default fail-fast behaviour).
    /// </summary>
    [Fact]
    public async Task Abort_policy_rethrows_and_aborts_Async()
    {
        var outcome = await RunSingleFaultScenarioAsync(ItemErrorAction.Abort).ConfigureAwait(false);

        Assert.True(outcome.Aborted, "Expected an Abort policy to re-throw and abort the run.");
    }

    /// <summary>
    /// Verifies that an <see cref="ItemErrorAction.Abort"/> policy does <em>not</em> increment
    /// <c>CurrentErrorItemCount</c> — the error counter tracks discarded (skipped) failures, not
    /// aborting ones.
    /// </summary>
    [Fact]
    public async Task Abort_policy_does_not_count_an_error_Async()
    {
        var outcome = await RunSingleFaultScenarioAsync(ItemErrorAction.Abort).ConfigureAwait(false);

        Assert.Equal(0, outcome.ErrorItemCount);
    }
}
