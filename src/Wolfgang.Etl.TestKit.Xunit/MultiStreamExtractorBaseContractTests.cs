using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Streams;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract xUnit contract tests for an extractor built on
/// <see cref="MultiStreamExtractorBase{TRecord, TProgress}"/> (#359). Derive once per library
/// (CSV, JSON, XML) to verify the multi-stream behaviour the base class promises: zero, one and many
/// sources in order, an empty source, named and anonymous sources in progress, a reader disposed per
/// source, and a failing source under <see cref="MultiStreamErrorAction.Stop"/> and
/// <see cref="MultiStreamErrorAction.Continue"/>.
/// </summary>
/// <typeparam name="TSut">The extractor under test.</typeparam>
/// <typeparam name="TRecord">The record type it produces. Must have value equality.</typeparam>
/// <typeparam name="TProgress">Its progress type.</typeparam>
/// <remarks>
/// The contract builds every source itself, over an in-memory stream it can check for disposal, from
/// the text your <see cref="FormatSource"/> returns. A format with a header (CSV) returns the header
/// alone for an empty record list; that is the "header-only source" case.
/// </remarks>
/// <example>
/// <code>
/// public class CsvMultiStreamExtractorContractTests
///     : MultiStreamExtractorBaseContractTests&lt;CsvMultiStreamExtractor, CsvRow, MultiStreamProgress&gt;
/// {
///     protected override CsvMultiStreamExtractor CreateSut
///     (
///         IEnumerable&lt;NamedStreamSource&gt; sources,
///         MultiStreamErrorAction errorAction,
///         Action&lt;StreamSourceError&gt;? errorHandler
///     ) =&gt;
///         new CsvMultiStreamExtractor(sources) { SourceErrorAction = errorAction, SourceErrorHandler = errorHandler };
///
///     protected override IReadOnlyList&lt;CsvRow&gt; CreateRecords(int count) =&gt;
///         Enumerable.Range(1, count).Select(i =&gt; new CsvRow($"r{i}")).ToList();
///
///     protected override string FormatSource(IReadOnlyList&lt;CsvRow&gt; records) =&gt;
///         "Name\n" + string.Concat(records.Select(r =&gt; r.Name + "\n"));
///
///     protected override string MalformedSourceText =&gt; "Name\n\"unterminated";
/// }
/// </code>
/// </example>
public abstract class MultiStreamExtractorBaseContractTests<TSut, TRecord, TProgress>
    where TSut : MultiStreamExtractorBase<TRecord, TProgress>
    where TRecord : notnull
    where TProgress : MultiStreamProgress
{
    // ------------------------------------------------------------------
    // Factory methods
    // ------------------------------------------------------------------

    /// <summary>
    /// Creates the extractor under test over <paramref name="sources"/>, configured with the given
    /// error policy and handler.
    /// </summary>
    /// <param name="sources">The sources, in order.</param>
    /// <param name="errorAction">The value for <c>SourceErrorAction</c>.</param>
    /// <param name="errorHandler">The value for <c>SourceErrorHandler</c>.</param>
    /// <returns>A new extractor.</returns>
    protected abstract TSut CreateSut
    (
        IEnumerable<NamedStreamSource> sources,
        MultiStreamErrorAction errorAction,
        Action<StreamSourceError>? errorHandler
    );



    /// <summary>
    /// Creates <paramref name="count"/> distinct records that survive a round trip through
    /// <see cref="FormatSource"/> and the extractor unchanged.
    /// </summary>
    /// <param name="count">The number of records.</param>
    /// <returns>The records.</returns>
    protected abstract IReadOnlyList<TRecord> CreateRecords(int count);



    /// <summary>
    /// Returns the text of a stream that holds exactly <paramref name="records"/>, in order, in your
    /// format. For an empty list, return a valid stream with no records (for CSV, the header only).
    /// </summary>
    /// <param name="records">The records to encode.</param>
    /// <returns>The stream text.</returns>
    protected abstract string FormatSource(IReadOnlyList<TRecord> records);



    /// <summary>
    /// The text of a stream your extractor fails to read: reading it throws an exception other than
    /// <see cref="OperationCanceledException"/>.
    /// </summary>
    protected abstract string MalformedSourceText { get; }



    // ------------------------------------------------------------------
    // Tests
    // ------------------------------------------------------------------

    /// <summary>No sources: no records, and a known total of 0.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ExtractAsync_when_there_are_no_sources_yields_no_records()
    {
        var reports = new List<TProgress>();
        var sut = CreateSut(Array.Empty<NamedStreamSource>(), MultiStreamErrorAction.Stop, errorHandler: null);

        var records = await DrainAsync(sut, new SynchronousProgress<TProgress>(reports.Add)).ConfigureAwait(false);

        Assert.Empty(records);
        Assert.Equal(0, reports[reports.Count - 1].TotalSourceCount);
    }



    /// <summary>One source: its records, in order.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ExtractAsync_with_one_source_yields_its_records_in_order()
    {
        var expected = CreateRecords(3);
        var sut = CreateSut(new[] { Source(expected, "one").Named }, MultiStreamErrorAction.Stop, errorHandler: null);

        var records = await DrainAsync(sut).ConfigureAwait(false);

        Assert.Equal(expected, records);
    }



    /// <summary>Several sources: every record, source after source, each counted once.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ExtractAsync_with_several_sources_yields_every_record_in_source_order()
    {
        var all = CreateRecords(5);
        var first = all.Take(2).ToList();
        var second = all.Skip(2).ToList();
        var sut = CreateSut
        (
            new[] { Source(first, "first").Named, Source(second, "second").Named },
            MultiStreamErrorAction.Stop,
            errorHandler: null
        );

        var records = await DrainAsync(sut).ConfigureAwait(false);

        Assert.Equal(all, records);
        Assert.Equal(all.Count, sut.CurrentItemCount);
    }



    /// <summary>An empty (or header-only) source contributes nothing and does not stop the run.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ExtractAsync_with_an_empty_source_moves_on_to_the_next_source()
    {
        var expected = CreateRecords(2);
        var sut = CreateSut
        (
            new[] { Source(Array.Empty<TRecord>(), "empty").Named, Source(expected, "full").Named },
            MultiStreamErrorAction.Stop,
            errorHandler: null
        );

        var records = await DrainAsync(sut).ConfigureAwait(false);

        Assert.Equal(expected, records);
    }



    /// <summary>Each source's reader is disposed after its records have been read.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ExtractAsync_disposes_the_reader_of_every_source_it_reads()
    {
        var records = CreateRecords(2);
        var first = Source(records.Take(1).ToList(), "first");
        var second = Source(records.Skip(1).ToList(), "second");
        var sut = CreateSut(new[] { first.Named, second.Named }, MultiStreamErrorAction.Stop, errorHandler: null);

        _ = await DrainAsync(sut).ConfigureAwait(false);

        Assert.True(first.Stream.IsDisposed, "The first source's reader was not disposed.");
        Assert.True(second.Stream.IsDisposed, "The second source's reader was not disposed.");
    }



    /// <summary>Progress reports the name of a named source.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Progress_reports_the_name_of_a_named_source()
    {
        var reports = new List<TProgress>();
        var sut = CreateSut(new[] { Source(CreateRecords(1), "named.dat").Named }, MultiStreamErrorAction.Stop, errorHandler: null);

        _ = await DrainAsync(sut, new SynchronousProgress<TProgress>(reports.Add)).ConfigureAwait(false);

        var final = reports[reports.Count - 1];
        Assert.Equal("named.dat", final.CurrentSourceName);
        Assert.Equal(1, final.CurrentSourceNumber);
        Assert.Equal(1, final.TotalSourceCount);
    }



    /// <summary>Progress reports a null name for an anonymous source.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Progress_reports_a_null_name_for_an_anonymous_source()
    {
        var records = CreateRecords(2);
        var reports = new List<TProgress>();
        var sut = CreateSut
        (
            new[] { Source(records.Take(1).ToList(), "named.dat").Named, Source(records.Skip(1).ToList(), name: null).Named },
            MultiStreamErrorAction.Stop,
            errorHandler: null
        );

        _ = await DrainAsync(sut, new SynchronousProgress<TProgress>(reports.Add)).ConfigureAwait(false);

        var final = reports[reports.Count - 1];
        Assert.Null(final.CurrentSourceName);
        Assert.Equal(2, final.CurrentSourceNumber);
    }



    /// <summary>A failing source under <see cref="MultiStreamErrorAction.Stop"/> ends the run with its exception.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ExtractAsync_with_Stop_rethrows_a_failing_source_and_does_not_open_later_sources()
    {
        var failing = Source(MalformedSourceText, "bad");
        var later = Source(CreateRecords(1), "later");
        var sut = CreateSut(new[] { failing.Named, later.Named }, MultiStreamErrorAction.Stop, errorHandler: null);

        var ex = await Record.ExceptionAsync(() => DrainAsync(sut)).ConfigureAwait(false);

        Assert.NotNull(ex);
        Assert.False(ex is OperationCanceledException, "A malformed source must fail with an error, not a cancellation.");
        Assert.True(failing.Stream.IsDisposed, "The failing source's reader was not disposed.");
        Assert.False(later.Stream.IsDisposed, "A source after the failure was opened.");
    }



    /// <summary>
    /// A failing source under <see cref="MultiStreamErrorAction.Continue"/> is skipped, counted and reported,
    /// and the next source is read.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ExtractAsync_with_Continue_skips_a_failing_source_reports_it_and_reads_the_next()
    {
        var errors = new List<StreamSourceError>();
        var reports = new List<TProgress>();
        var expected = CreateRecords(2);
        var sut = CreateSut
        (
            new[] { Source(MalformedSourceText, "bad.dat").Named, Source(expected, "good.dat").Named },
            MultiStreamErrorAction.Continue,
            errors.Add
        );

        var records = await DrainAsync(sut, new SynchronousProgress<TProgress>(reports.Add)).ConfigureAwait(false);

        Assert.Equal(expected, records);
        var error = Assert.Single(errors);
        Assert.Equal("bad.dat", error.Name);
        Assert.Equal(0, error.SourceIndex);
        Assert.Equal(1, reports[reports.Count - 1].CurrentFailedSourceCount);
    }



    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private ContractSource Source(IReadOnlyList<TRecord> records, string? name) =>
        Source(FormatSource(records), name);



    private static ContractSource Source(string text, string? name)
    {
        var stream = new DisposalTrackingStream(Encoding.UTF8.GetBytes(text));
        return new ContractSource(new NamedStreamSource(new StreamReader(stream), name), stream);
    }



    private static async Task<List<TRecord>> DrainAsync(TSut sut, IProgress<TProgress>? progress = null)
    {
        var records = new List<TRecord>();
        var stream = progress is null ? sut.ExtractAsync() : sut.ExtractAsync(progress);
        await foreach (var record in stream.ConfigureAwait(false))
        {
            records.Add(record);
        }

        return records;
    }



    private sealed class ContractSource
    {
        public ContractSource(NamedStreamSource named, DisposalTrackingStream stream)
        {
            Named = named;
            Stream = stream;
        }



        public NamedStreamSource Named { get; }



        public DisposalTrackingStream Stream { get; }
    }
}
