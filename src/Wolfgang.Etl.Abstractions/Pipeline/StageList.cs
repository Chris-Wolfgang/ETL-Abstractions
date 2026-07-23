using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Helpers for accumulating the stage instances (extractor, transformers, loader) that make up a
/// pipeline as it is built, and for disposing them after the run when
/// <see cref="IPipeline.DisposeStagesOnCompletion"/> has been requested.
/// </summary>
internal static class StageList
{
    /// <summary>
    /// Returns a new immutable list containing every element of <paramref name="existing"/> followed
    /// by <paramref name="next"/>. The builder is immutable, so each stage carries its own snapshot
    /// of the accumulated stages and branched/repeated calls cannot alias shared state.
    /// </summary>
    public static IReadOnlyList<object> Append(IReadOnlyList<object> existing, object next)
    {
        var result = new object[existing.Count + 1];
        for (var i = 0; i < existing.Count; i++)
        {
            result[i] = existing[i];
        }

        result[existing.Count] = next;
        return result;
    }


    /// <summary>
    /// Runs <paramref name="run"/> and then disposes every stage in <paramref name="stages"/>,
    /// whether the run succeeded or threw. If the run threw, that exception propagates unchanged
    /// (with its original stack trace) after disposal completes; otherwise any exceptions thrown
    /// during disposal are surfaced as an <see cref="AggregateException"/>.
    /// </summary>
    /// <param name="run">The pipeline run delegate.</param>
    /// <param name="stages">The stage instances to dispose once the run finishes.</param>
    /// <param name="token">The cancellation token forwarded to the run.</param>
    /// <returns>A task that completes when the run and the subsequent disposal have finished.</returns>
    /// <exception cref="AggregateException">
    /// The run completed successfully but one or more stages threw while being disposed.
    /// </exception>
    public static async Task RunThenDisposeStagesAsync
    (
        Func<CancellationToken, Task> run,
        IReadOnlyList<object> stages,
        CancellationToken token
    )
    {
        ExceptionDispatchInfo? runError = null;
        try
        {
            // Stryker disable once Boolean: equivalent under test — no SynchronizationContext in the
            // test host, so ConfigureAwait(false)->(true) schedules the continuation identically.
            await run(token).ConfigureAwait(false);
        }
#pragma warning disable CA1031 // Do not catch general exception types — intentional: capture to rethrow after disposal.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            runError = ExceptionDispatchInfo.Capture(ex);
        }

        // Stryker disable once Boolean: equivalent under test — see above (no SynchronizationContext).
        var disposalError = await DisposeStagesAsync(stages).ConfigureAwait(false);

        // The run's own failure is the primary signal — rethrow it unchanged. Disposal still ran.
        runError?.Throw();

        if (disposalError is not null)
        {
            throw disposalError;
        }
    }


    // Disposes each stage that is IAsyncDisposable (preferred) or IDisposable, skipping the rest,
    // continuing past any failure and collecting the failures into an AggregateException (or null).
    // Order: reverse construction order (loader → transformers → extractor), matching the LIFO
    // convention of nested `using`/`await using` blocks and DI-container scope disposal — a
    // downstream stage that transitively references an upstream stage's resources gets torn down
    // before the upstream stage's cleanup runs.
    private static async Task<AggregateException?> DisposeStagesAsync(IReadOnlyList<object> stages)
    {
        List<Exception>? errors = null;
        for (var i = stages.Count - 1; i >= 0; i--)
        {
            var stage = stages[i];
            try
            {
                if (stage is IAsyncDisposable asyncDisposable)
                {
                    // Stryker disable once Boolean: equivalent under test — no SynchronizationContext.
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                else if (stage is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types — intentional: dispose every stage, then aggregate.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                (errors ??= new List<Exception>()).Add(ex);
            }
        }

        return errors is null
            ? null
            : new AggregateException("One or more pipeline stages threw while being disposed.", errors);
    }
}
