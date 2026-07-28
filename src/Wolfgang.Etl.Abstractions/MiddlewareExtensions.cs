using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Extension methods that attach <see cref="IItemMiddleware{T}"/> to an
/// <see cref="IAsyncEnumerable{T}"/> stream, so cross-cutting per-item behaviour composes onto any
/// extractor / transformer output or loader input — and inside an <c>EtlPipeline</c> via
/// <c>Through(stream =&gt; stream.WithMiddleware(...))</c> — without changing the component itself.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Pipes every item of <paramref name="source"/> through <paramref name="middleware"/>. Items the
    /// middleware drops (<see cref="MiddlewareResult.Drop{T}"/>) are removed from the stream; otherwise
    /// the (possibly replaced) item is yielded.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="source">The stream to decorate.</param>
    /// <param name="middleware">The middleware to run for each item.</param>
    /// <param name="token">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The decorated stream.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="middleware"/> is <see langword="null"/>.</exception>
    public static async IAsyncEnumerable<T> WithMiddleware<T>
    (
        this IAsyncEnumerable<T> source,
        IItemMiddleware<T> middleware,
        [EnumeratorCancellation] CancellationToken token = default
    )
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (middleware is null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        await foreach (var item in source.WithCancellation(token))
        {
            // Stryker disable once Boolean: equivalent — with no synchronization context in play, ConfigureAwait(false) and (true) are indistinguishable.
            var result = await middleware.OnItemAsync(item, token).ConfigureAwait(false);
            if (!result.Skip)
            {
                yield return result.Item;
            }
        }
    }



    /// <summary>
    /// Pipes every item of <paramref name="source"/> through <paramref name="middlewares"/> in order:
    /// each middleware sees the item the previous one passed on. If any middleware drops the item
    /// (<see cref="MiddlewareResult.Drop{T}"/>), the remaining middleware is not run and the item is
    /// removed from the stream.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="source">The stream to decorate.</param>
    /// <param name="middlewares">The middleware chain, applied in enumeration order.</param>
    /// <param name="token">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The decorated stream.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="middlewares"/> is <see langword="null"/>, or a member of <paramref name="middlewares"/> is <see langword="null"/>.</exception>
    public static async IAsyncEnumerable<T> WithMiddleware<T>
    (
        this IAsyncEnumerable<T> source,
        IEnumerable<IItemMiddleware<T>> middlewares,
        [EnumeratorCancellation] CancellationToken token = default
    )
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (middlewares is null)
        {
            throw new ArgumentNullException(nameof(middlewares));
        }

        // Snapshot the chain once so the same ordered set runs for every item.
        var chain = new List<IItemMiddleware<T>>(middlewares);
        foreach (var middleware in chain)
        {
            if (middleware is null)
            {
                // Stryker disable once String: the exception message is diagnostic-only, not a behavioural contract asserted by tests.
                throw new ArgumentNullException(nameof(middlewares), "A middleware in the chain is null.");
            }
        }

        await foreach (var item in source.WithCancellation(token))
        {
            var current = item;
            var dropped = false;

            foreach (var middleware in chain)
            {
                // Stryker disable once Boolean: equivalent — with no synchronization context in play, ConfigureAwait(false) and (true) are indistinguishable.
                var result = await middleware.OnItemAsync(current, token).ConfigureAwait(false);
                if (result.Skip)
                {
                    dropped = true;
                    break;
                }

                current = result.Item;
            }

            if (!dropped)
            {
                yield return current;
            }
        }
    }
}
