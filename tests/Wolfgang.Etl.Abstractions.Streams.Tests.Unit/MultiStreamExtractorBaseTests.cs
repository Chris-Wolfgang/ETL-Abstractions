using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Streams.Tests.Unit;

public class MultiStreamExtractorBaseTests
{
    private static async Task<List<string>> DrainAsync(IAsyncEnumerable<string> source, CancellationToken token = default)
    {
        var result = new List<string>();
        await foreach (var item in source.WithCancellation(token))
        {
            result.Add(item);
        }

        return result;
    }



    [Fact]
    public void Constructor_when_sources_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new LineExtractor(null!));

        Assert.Equal("sources", ex.ParamName);
    }



    [Fact]
    public async Task ExtractAsync_reads_every_source_in_order_and_counts_every_record()
    {
        var a = Sources.Of("a1\na2\n", "a.csv");
        var b = Sources.Of("b1\n", "b.csv");
        var sut = new LineExtractor(new[] { a.Source, b.Source });

        var records = await DrainAsync(sut.ExtractAsync());

        Assert.Equal(new[] { "a1", "a2", "b1" }, records);
        Assert.Equal(3, sut.CurrentItemCount);
    }



    [Fact]
    public async Task ExtractAsync_when_there_are_no_sources_yields_nothing_and_reports_a_total_of_zero()
    {
        var sut = new LineExtractor(Array.Empty<NamedStreamSource>());

        var records = await DrainAsync(sut.ExtractAsync());

        Assert.Empty(records);
        Assert.Equal(0, sut.Snapshot().TotalSourceCount);
        Assert.Equal(0, sut.Snapshot().CurrentSourceNumber);
    }



    [Fact]
    public async Task ExtractAsync_when_a_source_is_empty_moves_on_to_the_next_one()
    {
        var empty = Sources.Of(string.Empty);
        var b = Sources.Of("b1\n");
        var sut = new LineExtractor(new List<NamedStreamSource> { empty.Source, b.Source });

        var records = await DrainAsync(sut.ExtractAsync());

        Assert.Equal(new[] { "b1" }, records);
        Assert.True(empty.Stream.IsDisposed);
    }



    [Fact]
    public async Task ExtractAsync_disposes_each_reader_after_its_records_are_read()
    {
        var a = Sources.Of("a1\n");
        var b = Sources.Of("b1\n");
        var sut = new LineExtractor(new[] { a.Source, b.Source });

        await using var enumerator = sut.ExtractAsync().GetAsyncEnumerator();
        Assert.True(await enumerator.MoveNextAsync());
        var firstOpenWhileRead = !a.Stream.IsDisposed;
        Assert.True(await enumerator.MoveNextAsync());
        var firstDisposedAfterwards = a.Stream.IsDisposed;
        Assert.False(await enumerator.MoveNextAsync());

        Assert.True(firstOpenWhileRead);
        Assert.True(firstDisposedAfterwards);
        Assert.True(b.Stream.IsDisposed);
    }



    [Fact]
    public async Task ExtractAsync_by_default_stops_on_a_failing_source_and_leaves_later_sources_untouched()
    {
        var bad = Sources.Of("x1\nboom\n");
        var later = Sources.Of("y1\n");
        var sut = new LineExtractor(new[] { bad.Source, later.Source });

        await Assert.ThrowsAsync<InvalidDataException>(() => DrainAsync(sut.ExtractAsync()));

        Assert.Equal(MultiStreamErrorAction.Stop, sut.SourceErrorAction);
        Assert.True(bad.Stream.IsDisposed);
        Assert.False(later.Stream.IsDisposed);
    }



    [Fact]
    public async Task ExtractAsync_with_Continue_skips_the_failing_source_reports_it_and_reads_the_next()
    {
        var errors = new List<StreamSourceError>();
        var good = Sources.Of("g1\n", "good.csv");
        var bad = Sources.Of("x1\nboom\nx3\n", "bad.csv");
        var next = Sources.Of("n1\n", "next.csv");
        var sut = new LineExtractor(new[] { good.Source, bad.Source, next.Source })
        {
            SourceErrorAction = MultiStreamErrorAction.Continue,
            SourceErrorHandler = errors.Add,
        };

        var records = await DrainAsync(sut.ExtractAsync());

        Assert.Equal(new[] { "g1", "x1", "n1" }, records);
        var error = Assert.Single(errors);
        Assert.Equal("bad.csv", error.Name);
        Assert.Equal(1, error.SourceIndex);
        Assert.IsType<InvalidDataException>(error.Exception);
        Assert.Equal(1, sut.Snapshot().CurrentFailedSourceCount);
        Assert.True(bad.Stream.IsDisposed);
    }



    [Fact]
    public async Task ExtractAsync_with_Continue_and_no_handler_still_skips_and_counts_the_failure()
    {
        var bad = Sources.Of("boom\n");
        var next = Sources.Of("n1\n");
        var sut = new LineExtractor(new[] { bad.Source, next.Source })
        {
            SourceErrorAction = MultiStreamErrorAction.Continue,
        };

        var records = await DrainAsync(sut.ExtractAsync());

        Assert.Equal(new[] { "n1" }, records);
        Assert.Null(sut.SourceErrorHandler);
        Assert.Equal(1, sut.Snapshot().CurrentFailedSourceCount);
    }



    [Fact]
    public async Task ExtractAsync_with_Continue_never_treats_cancellation_as_a_source_error()
    {
        var errors = new List<StreamSourceError>();
        var cancelling = Sources.Of("cancel\n");
        var sut = new LineExtractor(new[] { cancelling.Source })
        {
            SourceErrorAction = MultiStreamErrorAction.Continue,
            SourceErrorHandler = errors.Add,
        };

        await Assert.ThrowsAsync<OperationCanceledException>(() => DrainAsync(sut.ExtractAsync()));

        Assert.Empty(errors);
        Assert.True(cancelling.Stream.IsDisposed);
    }



    [Fact]
    public async Task ExtractAsync_when_the_token_is_cancelled_before_a_source_throws_OperationCanceledException()
    {
        var a = Sources.Of("a1\n");
        var sut = new LineExtractor(new[] { a.Source });
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => DrainAsync(sut.ExtractAsync(cts.Token), cts.Token));

        Assert.False(a.Stream.IsDisposed);
    }



    [Fact]
    public async Task Progress_reports_the_current_source_and_the_known_total()
    {
        var a = Sources.Of("a1\n", "a.csv");
        var b = Sources.Of("b1\n", "b.csv");
        var sut = new LineExtractor(new[] { a.Source, b.Source });
        var snapshots = new List<MultiStreamProgress>();

        await foreach (var _ in sut.ExtractAsync())
        {
            snapshots.Add(sut.Snapshot());
        }

        Assert.Equal(new[] { 1, 2 }, snapshots.Select(s => s.CurrentSourceNumber));
        Assert.Equal(new[] { "a.csv", "b.csv" }, snapshots.Select(s => s.CurrentSourceName));
        Assert.All(snapshots, s => Assert.Equal(2, s.TotalSourceCount));
        Assert.All(snapshots, s => Assert.Equal(0, s.CurrentFailedSourceCount));
        Assert.Equal(2, snapshots[1].CurrentItemCount);
    }



    [Fact]
    public async Task Progress_reports_a_total_of_minus_one_for_a_lazy_source_sequence()
    {
        var a = Sources.Of("a1\n");
        var sut = new LineExtractor(Lazy(a.Source));
        var totals = new List<int>();

        await foreach (var _ in sut.ExtractAsync())
        {
            totals.Add(sut.Snapshot().TotalSourceCount);
        }

        Assert.Equal(new[] { -1 }, totals);
    }



    [Fact]
    public async Task Progress_reports_a_null_name_for_an_anonymous_source()
    {
        var a = Sources.Of("a1\n");
        var sut = new LineExtractor(new[] { a.Source });
        string? name = "unset";

        await foreach (var _ in sut.ExtractAsync())
        {
            name = sut.Snapshot().CurrentSourceName;
        }

        Assert.Null(name);
    }



    [Fact]
    public async Task ExtractAsync_applies_SkipItemCount_and_MaximumItemCount_across_sources()
    {
        var a = Sources.Of("a1\na2\n");
        var b = Sources.Of("b1\nb2\nb3\n");
        var sut = new LineExtractor
        (
            new[] { a.Source, b.Source },
            new ExtractorOptions { SkipItemCount = 1, MaximumItemCount = 3 }
        );

        var records = await DrainAsync(sut.ExtractAsync());

        Assert.Equal(new[] { "a2", "b1", "b2" }, records);
        Assert.Equal(1, sut.CurrentSkippedItemCount);
        Assert.Equal(3, sut.CurrentItemCount);
    }



    [Fact]
    public async Task ExtractAsync_when_MaximumItemCount_is_reached_does_not_open_the_next_source()
    {
        var a = Sources.Of("a1\na2\n");
        var b = Sources.Of("b1\n");
        var sut = new LineExtractor
        (
            new[] { a.Source, b.Source },
            new ExtractorOptions { MaximumItemCount = 2 }
        );

        var records = await DrainAsync(sut.ExtractAsync());

        Assert.Equal(new[] { "a1", "a2" }, records);
        Assert.True(a.Stream.IsDisposed);
        Assert.False(b.Stream.IsDisposed);
    }



    [Fact]
    public async Task ExtractAsync_when_a_source_is_null_throws_InvalidOperationException()
    {
        var a = Sources.Of("a1\n");
        var sut = new LineExtractor(new[] { a.Source, null! });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => DrainAsync(sut.ExtractAsync()));

        Assert.Contains("index 1", ex.Message, StringComparison.Ordinal);
    }



    [Fact]
    public async Task Progress_reports_the_count_of_a_read_only_collection_of_sources()
    {
        var a = Sources.Of("a1\n");
        var b = Sources.Of("b1\n");
        var sources = new ReadOnlySources(new[] { a.Source, b.Source });
        var sut = new LineExtractor(sources);
        var totals = new List<int>();

        await foreach (var _ in sut.ExtractAsync())
        {
            totals.Add(sut.Snapshot().TotalSourceCount);
        }

        Assert.Equal(new[] { 2, 2 }, totals);

        // The test double must enumerate the same sources through the non-generic interface too.
        var nonGeneric = ((System.Collections.IEnumerable)sources).GetEnumerator();
        Assert.True(nonGeneric.MoveNext());
        Assert.Same(a.Source, nonGeneric.Current);
    }



    [Fact]
    public async Task ExtractAsync_with_progress_reports_a_final_MultiStreamProgress()
    {
        var a = Sources.Of("a1\na2\n", "a.csv");
        var sut = new LineExtractor(new[] { a.Source });
        var reports = new List<MultiStreamProgress>();

        await DrainAsync(sut.ExtractAsync(new InlineProgress<MultiStreamProgress>(reports.Add)));

        var final = reports[reports.Count - 1];
        Assert.Equal(2, final.CurrentItemCount);
        Assert.Equal("a.csv", final.CurrentSourceName);
    }



    private static IEnumerable<NamedStreamSource> Lazy(NamedStreamSource source)
    {
        yield return source;
    }



    // Implements IReadOnlyCollection but not ICollection, to exercise that branch of the count.
    private sealed class ReadOnlySources : IReadOnlyCollection<NamedStreamSource>
    {
        private readonly NamedStreamSource[] _items;


        public ReadOnlySources(NamedStreamSource[] items) => _items = items;


        public int Count => _items.Length;


        public IEnumerator<NamedStreamSource> GetEnumerator() => ((IEnumerable<NamedStreamSource>)_items).GetEnumerator();


        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
