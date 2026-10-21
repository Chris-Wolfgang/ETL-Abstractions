using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Streams.Tests.Unit;

public class MultiStreamLoaderBaseTests
{
    private sealed class Destinations
    {
        public TrackingStream EvenStream { get; } = new();

        public TrackingStream OddStream { get; } = new();

        public NamedStreamDestination Even { get; }

        public NamedStreamDestination Odd { get; }


        public Destinations()
        {
            Even = new NamedStreamDestination(new StreamWriter(EvenStream), "even.txt");
            Odd = new NamedStreamDestination(new StreamWriter(OddStream), "odd.txt");
        }


        public NamedStreamDestination Select(string record) =>
            (record[1] - '0') % 2 == 0 ? Even : Odd;
    }



    [Fact]
    public void Constructor_when_selector_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new LineLoader(null!));

        Assert.Equal("destinationSelector", ex.ParamName);
    }



    [Fact]
    public async Task LoadAsync_routes_each_record_to_its_destination_with_one_header_and_footer_each()
    {
        var d = new Destinations();
        var sut = new LineLoader(d.Select) { WriteHeaderAndFooter = true };

        await sut.LoadAsync(Sources.Async(new[] { "r1", "r2", "r3", "r4" }));

        Assert.Equal("header|r1|r3|footer", d.OddStream.Text);
        Assert.Equal("header|r2|r4|footer", d.EvenStream.Text);
        Assert.Equal(4, sut.CurrentItemCount);
        Assert.True(d.OddStream.IsDisposed);
        Assert.True(d.EvenStream.IsDisposed);
    }



    [Fact]
    public async Task LoadAsync_with_the_default_hooks_writes_only_the_records()
    {
        var d = new Destinations();
        var sut = new PlainLoader(d.Select);

        await sut.LoadAsync(Sources.Async(new[] { "r1", "r3" }));

        Assert.Equal("r1r3", d.OddStream.Text);
        Assert.Equal(string.Empty, d.EvenStream.Text);
        Assert.False(d.EvenStream.IsDisposed);
    }



    [Fact]
    public async Task LoadAsync_when_a_record_fails_disposes_every_opened_writer_and_writes_no_footer()
    {
        var d = new Destinations();
        var sut = new LineLoader(d.Select) { WriteHeaderAndFooter = true };

        await Assert.ThrowsAsync<InvalidDataException>(() => sut.LoadAsync(Sources.Async(new[] { "r1", "r2", "r5!" })));

        Assert.True(d.OddStream.IsDisposed);
        Assert.True(d.EvenStream.IsDisposed);
        Assert.DoesNotContain("footer", d.OddStream.Text, StringComparison.Ordinal);
    }



    [Fact]
    public async Task LoadAsync_when_the_selector_returns_null_throws_InvalidOperationException()
    {
        var d = new Destinations();
        var sut = new LineLoader(r => string.Equals(r, "r2", StringComparison.Ordinal) ? null! : d.Select(r));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.LoadAsync(Sources.Async(new[] { "r1", "r2" })));

        Assert.Equal("The destination selector returned null.", ex.Message);
        Assert.True(d.OddStream.IsDisposed);
    }



    [Fact]
    public async Task LoadAsync_applies_SkipItemCount_and_MaximumItemCount()
    {
        var d = new Destinations();
        var sut = new LineLoader(d.Select, new LoaderOptions { SkipItemCount = 1, MaximumItemCount = 2 });

        await sut.LoadAsync(Sources.Async(new[] { "r1", "r2", "r3", "r4" }));

        Assert.Equal("r2|", d.EvenStream.Text);
        Assert.Equal("r3|", d.OddStream.Text);
        Assert.Equal(1, sut.CurrentSkippedItemCount);
        Assert.Equal(2, sut.CurrentItemCount);
    }



    [Fact]
    public async Task Progress_counts_distinct_destinations_and_names_the_last_one_written()
    {
        var d = new Destinations();
        var sut = new LineLoader(d.Select);

        await sut.LoadAsync(Sources.Async(new[] { "r1", "r3", "r2" }));
        var progress = sut.Snapshot();

        Assert.Equal(2, progress.CurrentSourceNumber);
        Assert.Equal("even.txt", progress.CurrentSourceName);
        Assert.Equal(-1, progress.TotalSourceCount);
        Assert.Equal(0, progress.CurrentFailedSourceCount);
        Assert.Equal(3, progress.CurrentItemCount);
    }



    [Fact]
    public async Task LoadAsync_with_progress_reports_a_final_MultiStreamProgress()
    {
        var d = new Destinations();
        var lineReports = new List<MultiStreamProgress>();
        var plainReports = new List<MultiStreamProgress>();

        await new LineLoader(d.Select).LoadAsync(Sources.Async(new[] { "r1" }), new InlineProgress<MultiStreamProgress>(lineReports.Add));
        await new PlainLoader(new Destinations().Select).LoadAsync(Sources.Async(new[] { "r2" }), new InlineProgress<MultiStreamProgress>(plainReports.Add));

        Assert.Equal(1, lineReports[lineReports.Count - 1].CurrentItemCount);
        Assert.Equal("even.txt", plainReports[plainReports.Count - 1].CurrentSourceName);
    }



    [Fact]
    public async Task LoadAsync_resets_the_destination_counter_for_each_run()
    {
        var first = new Destinations();
        var second = new Destinations();
        var useSecond = false;
        var sut = new LineLoader(r => useSecond ? second.Select(r) : first.Select(r));

        await sut.LoadAsync(Sources.Async(new[] { "r1", "r2" }));
        useSecond = true;
        await sut.LoadAsync(Sources.Async(new[] { "r3" }));

        Assert.Equal(1, sut.Snapshot().CurrentSourceNumber);
        Assert.Equal(new[] { "r3|" }, new[] { second.OddStream.Text }.ToArray());
    }
}
