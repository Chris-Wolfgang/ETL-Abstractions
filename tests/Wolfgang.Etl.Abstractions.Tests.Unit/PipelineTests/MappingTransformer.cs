using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests;

/// <summary>
/// Test helper transformer that maps each <typeparamref name="TSource"/> to
/// <typeparamref name="TDestination"/> via a user-supplied function.
/// </summary>
internal sealed class MappingTransformer<TSource, TDestination>
    : TransformerBase<TSource, TDestination, Report>
    where TSource : notnull
    where TDestination : notnull
{
    private readonly Func<TSource, TDestination> _map;


    public MappingTransformer(Func<TSource, TDestination> map)
    {
        _map = map;
    }


    protected override async IAsyncEnumerable<TDestination> TransformWorkerAsync
    (
        IAsyncEnumerable<TSource> items,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
        {
            IncrementCurrentItemCount();
            yield return _map(item);
        }
    }


    protected override Report CreateProgressReport()
    {
        return new Report(CurrentItemCount);
    }
}
