using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Streams;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Runs the multi-stream contract suites (#359) against a minimal line-per-record extractor and
/// loader, so the contracts themselves are exercised here.
/// </summary>
public class LineMultiStreamExtractorContractTests
    : MultiStreamExtractorBaseContractTests<LineMultiStreamExtractor, string, MultiStreamProgress>
{
    protected override LineMultiStreamExtractor CreateSut
    (
        IEnumerable<NamedStreamSource> sources,
        MultiStreamErrorAction errorAction,
        Action<StreamSourceError>? errorHandler
    ) =>
        new(sources)
        {
            SourceErrorAction = errorAction,
            SourceErrorHandler = errorHandler,
        };



    protected override IReadOnlyList<string> CreateRecords(int count) =>
        Enumerable
            .Range(1, count)
            .Select(i => "r" + i.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .ToList();



    protected override string FormatSource(IReadOnlyList<string> records) =>
        "#header\n" + string.Concat(records.Select(r => r + "\n"));



    protected override string MalformedSourceText => "no header\n";
}



public class LineMultiStreamLoaderContractTests
    : MultiStreamLoaderBaseContractTests<LineMultiStreamLoader, string, MultiStreamProgress>
{
    protected override LineMultiStreamLoader CreateSut(Func<string, NamedStreamDestination> destinationSelector) =>
        new(destinationSelector);



    protected override IReadOnlyList<string> CreateRecords(int count) =>
        Enumerable
            .Range(1, count)
            .Select(i => "r" + i.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .ToList();



    protected override IReadOnlyList<string> ParseDestination(string text) =>
        text
            .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .ToList();
}



/// <summary>
/// Reads "#header" then one record per line; a stream without the header is malformed.
/// </summary>
public sealed class LineMultiStreamExtractor : MultiStreamExtractorBase<string, MultiStreamProgress>
{
    public LineMultiStreamExtractor(IEnumerable<NamedStreamSource> sources)
        : base(sources)
    {
    }



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
        var lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0 || !string.Equals(lines[0], "#header", StringComparison.Ordinal))
        {
            throw new InvalidDataException("The stream has no header.");
        }

        foreach (var line in lines.Skip(1))
        {
            yield return line;
        }
    }



    protected override MultiStreamProgress CreateProgressReport() => CreateMultiStreamProgress();
}



/// <summary>
/// Writes "#header" to each destination, then one record per line.
/// </summary>
public sealed class LineMultiStreamLoader : MultiStreamLoaderBase<string, MultiStreamProgress>
{
    public LineMultiStreamLoader(Func<string, NamedStreamDestination> destinationSelector)
        : base(destinationSelector)
    {
    }



    protected override Task OnDestinationOpenedAsync(NamedStreamDestination destination, CancellationToken token) =>
        destination.Writer.WriteAsync("#header\n");



    protected override Task WriteRecordAsync(string record, NamedStreamDestination destination, CancellationToken token) =>
        destination.Writer.WriteAsync(record + "\n");



    protected override MultiStreamProgress CreateProgressReport() => CreateMultiStreamProgress();
}
