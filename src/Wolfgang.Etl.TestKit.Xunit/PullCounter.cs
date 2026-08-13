using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Counts how many items a stage pulls from a wrapped source, so the "no over-read" contract
/// tests (issue #49) can assert that a stage stops reading upstream once
/// <c>MaximumItemCount</c> is reached or the run is cancelled, rather than draining the whole
/// source. Internal test-harness helper.
/// </summary>
internal sealed class PullCounter
{
    private int _count;

    /// <summary>The number of items pulled from the wrapped source so far.</summary>
    public int Count => Volatile.Read(ref _count);

    /// <summary>
    /// Wraps an async source, incrementing <see cref="Count"/> as each item is pulled and
    /// awaiting <paramref name="onPull"/> after each increment (used to trigger cancellation
    /// after a chosen number of pulls).
    /// </summary>
    public async IAsyncEnumerable<T> CountAsync<T>
    (
        IAsyncEnumerable<T> source,
        Action? onPull = null,
        [EnumeratorCancellation] CancellationToken token = default
    )
    {
        await foreach (var item in source.WithCancellation(token).ConfigureAwait(false))
        {
            _ = Interlocked.Increment(ref _count);
            onPull?.Invoke();
            yield return item;
        }
    }

    /// <summary>
    /// Wraps a synchronous source (an extractor's in-memory sequence), incrementing
    /// <see cref="Count"/> as each item is pulled.
    /// </summary>
    public IEnumerable<T> CountSync<T>(IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            _ = Interlocked.Increment(ref _count);
            yield return item;
        }
    }
}
