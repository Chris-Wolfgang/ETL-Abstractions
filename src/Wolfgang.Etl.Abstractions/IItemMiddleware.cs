using System.Threading;
using System.Threading.Tasks;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// A composable, reusable hook for cross-cutting per-item behaviour (logging, validation, metrics,
/// throttling, deduplication) that can be attached to any stream without modifying the extractor,
/// transformer, or loader that produced it. Attach one or more with
/// <see cref="MiddlewareExtensions.WithMiddleware{T}(System.Collections.Generic.IAsyncEnumerable{T}, IItemMiddleware{T}, CancellationToken)"/>;
/// they run in the order attached, each seeing the item the previous one passed on.
/// </summary>
/// <typeparam name="T">The item type flowing through the pipeline.</typeparam>
public interface IItemMiddleware<T>
{
    /// <summary>
    /// Invoked once per item. Return <see cref="MiddlewareResult.Continue{T}(T)"/> to keep the item
    /// flowing (optionally replacing it), or <see cref="MiddlewareResult.Drop{T}"/> to remove it from
    /// the stream.
    /// </summary>
    /// <param name="item">The item to process.</param>
    /// <param name="token">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The outcome describing whether to keep or drop the item.</returns>
    ValueTask<MiddlewareResult<T>> OnItemAsync(T item, CancellationToken token);
}
