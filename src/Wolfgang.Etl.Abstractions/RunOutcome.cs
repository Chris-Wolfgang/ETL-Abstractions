using System;
using System.Threading.Tasks;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Records whether a run of an extractor or transformer is ending with an exception, so the final
/// progress report can tell a failed run from one that completed or was abandoned by its consumer.
/// </summary>
/// <remarks>
/// A class and not a local flag because the progress iterators cannot note the failure in place:
/// a <see langword="yield"/> cannot sit inside a <see langword="try"/> that has a
/// <see langword="catch"/>, and the worker is disposed in a <see langword="finally"/> block.
/// </remarks>
internal sealed class RunOutcome
{
    /// <summary>
    /// <see langword="true"/> once the worker has thrown, from an item or from its cleanup.
    /// </summary>
    internal bool Failed { get; set; }



    /// <summary>
    /// Disposes the worker's enumerator. A worker whose cleanup throws has failed, so the
    /// exception is noted here before it continues to the caller.
    /// </summary>
    /// <param name="enumerator">The worker's enumerator.</param>
    internal async ValueTask DisposeAsync(IAsyncDisposable enumerator)
    {
        try
        {
            // Stryker disable once Boolean : ConfigureAwait(false) vs (true) is equivalent under the test host (#452).
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }
        catch
        {
            Failed = true;
            throw;
        }
    }
}
