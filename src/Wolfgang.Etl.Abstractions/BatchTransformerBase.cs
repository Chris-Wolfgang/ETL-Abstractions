using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// A <see cref="TransformerBase{TSource, TDestination, TProgress}"/> that accumulates the source
/// items into fixed-size batches, yielding each batch as an <see cref="IReadOnlyList{T}"/>. A final
/// partial batch is flushed when the source stream completes, so no items are dropped.
/// </summary>
/// <remarks>
/// Bulk operations (bulk SQL inserts, batched API calls, chunked writes) are a common ETL pattern,
/// and the accumulate-and-flush logic is easy to get wrong — most often by forgetting the trailing
/// partial batch. This base class implements it once. The inherited <c>CurrentItemCount</c> counts
/// <em>batches</em> yielded, not input items, since the batch is the unit of work the downstream
/// loader sees.
/// </remarks>
/// <typeparam name="TSource">The type of the source items being batched.</typeparam>
/// <typeparam name="TProgress">The type of the progress report.</typeparam>
/// <example>
/// <code>
/// var extracted = extractor.ExtractAsync(token);          // IAsyncEnumerable&lt;OrderRecord&gt;
/// var batched = batcher.TransformAsync(extracted, token); // IAsyncEnumerable&lt;IReadOnlyList&lt;OrderRecord&gt;&gt;
/// await loader.LoadAsync(batched, token);                 // one bulk insert per batch
/// </code>
/// </example>
public abstract class BatchTransformerBase<TSource, TProgress>
    : TransformerBase<TSource, IReadOnlyList<TSource>, TProgress>
    where TSource : notnull
    where TProgress : notnull
{
    /// <summary>
    /// The number of source items in each batch. The final batch may contain fewer items when the
    /// source stream does not divide evenly. Defaults to 100. Must be greater than or equal to 1.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The specified value is less than 1.</exception>
    public int BatchSize
    {
        get;
        set
        {
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
#else
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Batch size must be greater than or equal to 1.");
            }
#endif
            field = value;
        }
    } = 100;



    /// <summary>
    /// Accumulates <paramref name="items"/> into batches of <see cref="BatchSize"/> and yields each
    /// as an <see cref="IReadOnlyList{T}"/>, flushing the final partial batch when the source ends.
    /// </summary>
    /// <param name="items">The source items to batch.</param>
    /// <param name="token">A token observed while enumerating the source and between batches.</param>
    /// <returns>A sequence of batches, each containing up to <see cref="BatchSize"/> items.</returns>
    protected sealed override async IAsyncEnumerable<IReadOnlyList<TSource>> TransformWorkerAsync
    (
        IAsyncEnumerable<TSource> items,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        var batch = new List<TSource>(BatchSize);

        await foreach (var item in items.WithCancellation(token))
        {
            batch.Add(item);

            if (batch.Count >= BatchSize)
            {
                IncrementCurrentItemCount();
                yield return batch;
                batch = new List<TSource>(BatchSize);
            }
        }

        if (batch.Count > 0)
        {
            IncrementCurrentItemCount();
            yield return batch;
        }
    }
}
