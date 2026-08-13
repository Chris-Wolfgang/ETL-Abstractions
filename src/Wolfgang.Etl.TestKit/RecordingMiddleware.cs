using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// A test <see cref="IItemMiddleware{T}"/> that records every item passed through it and, by default,
/// keeps each item flowing. Supply a policy to transform or drop items, and inspect
/// <see cref="Observed"/> to assert exactly what the middleware saw, in order.
/// </summary>
/// <typeparam name="T">The type of item flowing through the middleware.</typeparam>
/// <remarks>
/// Apply it to a stream with the <c>Wolfgang.Etl.Abstractions</c> 0.20
/// <see cref="MiddlewareExtensions.WithMiddleware{T}(System.Collections.Generic.IAsyncEnumerable{T}, IItemMiddleware{T}, System.Threading.CancellationToken)"/>
/// extension. <see cref="Observed"/> reflects every item the middleware was handed (including ones a
/// policy later drops), so it is a faithful record of what reached this stage of the pipeline.
/// </remarks>
/// <example>
/// <code>
/// // Record and pass through:
/// var recorder = new RecordingMiddleware&lt;int&gt;();
/// await foreach (var item in source.WithMiddleware(recorder)) { /* item = one the middleware let through */ }
/// // recorder.Observed lists every item the middleware saw.
///
/// // Drop odds, double evens:
/// var shaping = new RecordingMiddleware&lt;int&gt;(i =&gt;
///     i % 2 == 0 ? MiddlewareResult.Continue(i * 2) : MiddlewareResult.Drop&lt;int&gt;());
/// </code>
/// </example>
public sealed class RecordingMiddleware<T> : IItemMiddleware<T>
{
    private readonly List<T> _observed = new List<T>();
    private readonly Func<T, MiddlewareResult<T>>? _policy;



    /// <summary>
    /// Initializes a new <see cref="RecordingMiddleware{T}"/> that records each item and keeps it flowing.
    /// </summary>
    public RecordingMiddleware()
    {
    }



    /// <summary>
    /// Initializes a new <see cref="RecordingMiddleware{T}"/> that records each item and then applies
    /// <paramref name="policy"/> to transform or drop it.
    /// </summary>
    /// <param name="policy">
    /// Maps a recorded item to a <see cref="MiddlewareResult{T}"/> — use
    /// <see cref="MiddlewareResult.Continue{T}(T)"/> to keep (optionally transformed) or
    /// <see cref="MiddlewareResult.Drop{T}"/> to discard it.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="policy"/> is <see langword="null"/>.</exception>
    public RecordingMiddleware(Func<T, MiddlewareResult<T>> policy)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
    }



    /// <summary>Gets every item this middleware was handed, in order.</summary>
    public IReadOnlyList<T> Observed => _observed.ToArray();



    /// <inheritdoc/>
    public ValueTask<MiddlewareResult<T>> OnItemAsync(T item, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        _observed.Add(item);

        var result = _policy is null
            ? MiddlewareResult.Continue(item)
            : _policy(item);

        return new ValueTask<MiddlewareResult<T>>(result);
    }
}
