using System.Runtime.CompilerServices;

namespace Wolfgang.Etl.Abstractions.AotSmoke;

internal sealed class NumberExtractor : ExtractorBase<int, Report>
{
    private readonly int _count;

    public NumberExtractor(int count) => _count = count;

    protected override async IAsyncEnumerable<int> ExtractWorkerAsync(
        [EnumeratorCancellation] CancellationToken token)
    {
        for (var i = 1; i <= _count; i++)
        {
            token.ThrowIfCancellationRequested();
            IncrementCurrentItemCount();
            yield return i;
            await Task.Yield();
        }
    }

    protected override Report CreateProgressReport() => new(CurrentItemCount);
}


internal sealed class DoubleTransformer : TransformerBase<int, int, Report>
{
    protected override async IAsyncEnumerable<int> TransformWorkerAsync(
        IAsyncEnumerable<int> items,
        [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var item in items.WithCancellation(token))
        {
            yield return item * 2;
        }
    }

    protected override Report CreateProgressReport() => new(CurrentItemCount);
}


internal sealed class PlusOneTransformer : ITransformAsync<int, int>
{
    public async IAsyncEnumerable<int> TransformAsync(IAsyncEnumerable<int> items)
    {
        await foreach (var item in items)
        {
            yield return item + 1;
        }
    }
}


internal sealed class ListLoader : LoaderBase<int, Report>
{
    private readonly List<int> _items = new();

    public IReadOnlyList<int> Items => _items;

    protected override async Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
    {
        await foreach (var item in items.WithCancellation(token))
        {
            IncrementCurrentItemCount();
            _items.Add(item);
        }
    }

    protected override Report CreateProgressReport() => new(CurrentItemCount);
}
