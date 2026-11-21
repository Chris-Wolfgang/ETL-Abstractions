using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wolfgang.Etl.Abstractions.Streams.Tests.Unit;

/// <summary>
/// A <see cref="MemoryStream"/> that records whether it has been disposed.
/// </summary>
internal sealed class TrackingStream : MemoryStream
{
    public TrackingStream()
    {
    }



    public TrackingStream(byte[] buffer)
        : base(buffer)
    {
    }



    public bool IsDisposed { get; private set; }



    public string Text => Encoding.UTF8.GetString(ToArray());



    protected override void Dispose(bool disposing)
    {
        IsDisposed = true;
        base.Dispose(disposing);
    }
}



/// <summary>
/// A named source plus the stream under it, for disposal checks.
/// </summary>
internal sealed class SourceUnderTest
{
    public SourceUnderTest(NamedStreamSource source, TrackingStream stream)
    {
        Source = source;
        Stream = stream;
    }



    public NamedStreamSource Source { get; }



    public TrackingStream Stream { get; }
}



/// <summary>
/// Builds named sources over in-memory text and keeps their streams for disposal checks.
/// </summary>
internal static class Sources
{
    public static SourceUnderTest Of(string text, string? name = null)
    {
        var stream = new TrackingStream(Encoding.UTF8.GetBytes(text));
        return new SourceUnderTest(new NamedStreamSource(new StreamReader(stream), name), stream);
    }



    public static async IAsyncEnumerable<T> Async<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }
}



/// <summary>
/// Reads one line per record; a line "boom" throws <see cref="InvalidDataException"/>, and a line
/// "cancel" throws <see cref="OperationCanceledException"/>.
/// </summary>
internal sealed class LineExtractor : MultiStreamExtractorBase<string, MultiStreamProgress>
{
    public LineExtractor(IEnumerable<NamedStreamSource> sources, ExtractorOptions? options = null)
        : base(sources, options)
    {
    }



    public MultiStreamProgress Snapshot() => CreateMultiStreamProgress();



    protected override async IAsyncEnumerable<string> ExtractSourceAsync
    (
        StreamReader reader,
        [EnumeratorCancellation] CancellationToken token
    )
    {
#if NET7_0_OR_GREATER
        var text = await reader.ReadToEndAsync(token).ConfigureAwait(false);
#else
        var text = await reader.ReadToEndAsync().ConfigureAwait(false);
#endif
        foreach (var line in text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (string.Equals(line, "boom", StringComparison.Ordinal))
            {
                throw new InvalidDataException("boom");
            }

            if (string.Equals(line, "cancel", StringComparison.Ordinal))
            {
                throw new OperationCanceledException("cancel");
            }

            yield return line;
        }
    }



    protected override MultiStreamProgress CreateProgressReport() => CreateMultiStreamProgress();
}



/// <summary>
/// Writes each record as a line, with an optional header and footer per destination; a record
/// ending in "!" throws <see cref="InvalidDataException"/>.
/// </summary>
internal sealed class LineLoader : MultiStreamLoaderBase<string, MultiStreamProgress>
{
    public LineLoader(Func<string, NamedStreamDestination> selector, LoaderOptions? options = null)
        : base(selector, options)
    {
    }



    public bool WriteHeaderAndFooter { get; set; }



    public MultiStreamProgress Snapshot() => CreateMultiStreamProgress();



    protected override async Task OnDestinationOpenedAsync(NamedStreamDestination destination, CancellationToken token)
    {
        if (WriteHeaderAndFooter)
        {
            await destination.Writer.WriteAsync("header|").ConfigureAwait(false);
        }
    }



    protected override async Task OnDestinationClosingAsync(NamedStreamDestination destination, CancellationToken token)
    {
        if (WriteHeaderAndFooter)
        {
            await destination.Writer.WriteAsync("footer").ConfigureAwait(false);
        }
    }



    protected override async Task WriteRecordAsync(string record, NamedStreamDestination destination, CancellationToken token)
    {
        if (record.EndsWith("!", StringComparison.Ordinal))
        {
            throw new InvalidDataException("boom");
        }

        await destination.Writer.WriteAsync(record + "|").ConfigureAwait(false);
    }



    protected override MultiStreamProgress CreateProgressReport() => CreateMultiStreamProgress();
}



/// <summary>
/// A loader that relies on the base class's default open / close hooks.
/// </summary>
internal sealed class PlainLoader : MultiStreamLoaderBase<string, MultiStreamProgress>
{
    public PlainLoader(Func<string, NamedStreamDestination> selector)
        : base(selector)
    {
    }



    protected override Task WriteRecordAsync(string record, NamedStreamDestination destination, CancellationToken token) =>
        destination.Writer.WriteAsync(record);



    protected override MultiStreamProgress CreateProgressReport() => CreateMultiStreamProgress();
}



/// <summary>
/// Reports on the calling thread, so the last report is visible as soon as the run returns.
/// </summary>
internal sealed class InlineProgress<T> : IProgress<T>
{
    private readonly Action<T> _report;



    public InlineProgress(Action<T> report) => _report = report;



    public void Report(T value) => _report(value);
}
