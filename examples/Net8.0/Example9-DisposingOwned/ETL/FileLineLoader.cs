using Wolfgang.Etl.Abstractions;

namespace Example9_DisposingOwned.ETL;

// A sink that writes each record as a line to a stream it was GIVEN — it does not own or
// dispose the stream (note leaveOpen: true). That ownership belongs to whoever opened it.
// In a real format package this is what a path-based sink factory wraps, handing stream
// cleanup to DisposingOwned.
internal sealed class FileLineLoader(Stream stream) : LoaderBase<string, Report>
{
    protected override async Task LoadWorkerAsync(IAsyncEnumerable<string> items, CancellationToken token)
    {
        await using var writer = new StreamWriter(stream, leaveOpen: true);

        await foreach (var item in items.WithCancellation(token))
        {
            token.ThrowIfCancellationRequested();
            await writer.WriteLineAsync(item.AsMemory(), token);
            IncrementCurrentItemCount();
        }
    }


    protected override Report CreateProgressReport()
    {
        return new Report(CurrentItemCount);
    }
}
