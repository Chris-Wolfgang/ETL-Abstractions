using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

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


internal sealed class SmokeOrder
{
    public decimal Amount { get; set; }
}


// A logger that formats and keeps every message, so the policies' log calls run their
// formatting code rather than being skipped by a disabled logger.
internal sealed class RecordingLogger : ILogger
{
    public List<string> Messages { get; } = new();


    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
        => null;


    public bool IsEnabled(LogLevel logLevel) => true;


    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Messages.Add(formatter(state, exception));
    }
}
