using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests for a stage whose
/// <see cref="Wolfgang.Etl.Abstractions"/> 0.20 <c>WrapWorkerExecution</c> override adds a retry /
/// resilience strategy: a transient fault that clears within the retry budget completes the run, and a
/// fault that never clears fails after the maximum number of attempts (no infinite loop).
/// </summary>
/// <typeparam name="TSut">The system under test (an extractor, loader, or transformer with retry).</typeparam>
/// <remarks>
/// <para>
/// The derived class owns and drives its stage — the base never receives the SUT, so overrides need no
/// null-argument validation. Implement <see cref="RunWithTransientFaultAsync"/> (a stage that throws on
/// its first <c>failFirstAttempts</c> worker invocations and then succeeds, with a retry budget of
/// <c>maxAttempts</c>) and <see cref="RunWithPermanentFaultAsync"/> (a stage whose fault never clears),
/// each reporting a <see cref="RetryOutcome"/>. <c>RetryingExtractor&lt;T&gt;</c> (in the core package)
/// is a ready-made component these overrides can drive.
/// </para>
/// </remarks>
public abstract class RetryContractTests<TSut>
{
    /// <summary>The number of leading worker invocations that fail before one succeeds, for the transient case.</summary>
    protected virtual int FailFirstAttempts => 2;



    /// <summary>The maximum number of worker invocations (initial try plus retries).</summary>
    protected virtual int MaxAttempts => 5;



    /// <summary>
    /// Drives a stage that throws a transient fault on its first <paramref name="failFirstAttempts"/>
    /// worker invocations and then succeeds, with a retry budget of <paramref name="maxAttempts"/>.
    /// </summary>
    /// <returns>The observed outcome — whether it succeeded, how many attempts, and how many items.</returns>
    protected abstract Task<RetryOutcome> RunWithTransientFaultAsync(int failFirstAttempts, int maxAttempts);



    /// <summary>
    /// Drives a stage whose fault never clears, with a retry budget of <paramref name="maxAttempts"/>,
    /// letting the final fault propagate.
    /// </summary>
    /// <returns>The observed outcome — it should not have succeeded, having made <paramref name="maxAttempts"/> attempts.</returns>
    protected abstract Task<RetryOutcome> RunWithPermanentFaultAsync(int maxAttempts);



    /// <summary>
    /// Verifies that a transient fault clearing within the retry budget lets the run complete, having
    /// retried exactly as many times as the fault required and still delivered its items.
    /// </summary>
    [Fact]
    public async Task Transient_fault_within_budget_eventually_succeeds_Async()
    {
        Assert.True(FailFirstAttempts < MaxAttempts, "FailFirstAttempts must be less than MaxAttempts for this test to be meaningful.");

        var outcome = await RunWithTransientFaultAsync(FailFirstAttempts, MaxAttempts).ConfigureAwait(false);

        Assert.True(outcome.Succeeded, "A transient fault clearing within the retry budget should let the run complete.");
        Assert.Equal(FailFirstAttempts + 1, outcome.AttemptCount);
        Assert.True(outcome.ItemCount > 0, "The successful attempt should have delivered items.");
    }



    /// <summary>
    /// Verifies that retry actually happened (more than a single attempt) for the transient case.
    /// </summary>
    [Fact]
    public async Task Transient_fault_causes_more_than_one_attempt_Async()
    {
        var outcome = await RunWithTransientFaultAsync(FailFirstAttempts, MaxAttempts).ConfigureAwait(false);

        Assert.True(outcome.AttemptCount > 1, "A transient fault should have triggered at least one retry.");
    }



    /// <summary>
    /// Verifies that a fault that never clears fails after the maximum number of attempts — the retry
    /// gives up rather than looping forever.
    /// </summary>
    [Fact]
    public async Task Permanent_fault_fails_after_max_attempts_Async()
    {
        var outcome = await RunWithPermanentFaultAsync(MaxAttempts).ConfigureAwait(false);

        Assert.False(outcome.Succeeded, "A fault that never clears should not complete the run.");
        Assert.Equal(MaxAttempts, outcome.AttemptCount);
    }
}
