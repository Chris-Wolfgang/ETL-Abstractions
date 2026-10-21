using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Streams;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract xUnit contract tests for a loader built on
/// <see cref="MultiStreamLoaderBase{TRecord, TProgress}"/> (#359). Derive once per library to verify
/// that records are routed to the destination the selector picks and read back intact, that every
/// writer the loader opened is disposed (also when the load fails), that an empty input opens nothing,
/// and that progress counts the destinations.
/// </summary>
/// <typeparam name="TSut">The loader under test.</typeparam>
/// <typeparam name="TRecord">The record type it consumes. Must have value equality.</typeparam>
/// <typeparam name="TProgress">Its progress type.</typeparam>
/// <example>
/// <code>
/// public class CsvMultiStreamLoaderContractTests
///     : MultiStreamLoaderBaseContractTests&lt;CsvMultiStreamLoader, CsvRow, MultiStreamProgress&gt;
/// {
///     protected override CsvMultiStreamLoader CreateSut(Func&lt;CsvRow, NamedStreamDestination&gt; selector) =&gt;
///         new CsvMultiStreamLoader(selector);
///
///     protected override IReadOnlyList&lt;CsvRow&gt; CreateRecords(int count) =&gt;
///         Enumerable.Range(1, count).Select(i =&gt; new CsvRow($"r{i}")).ToList();
///
///     protected override IReadOnlyList&lt;CsvRow&gt; ParseDestination(string text) =&gt;
///         text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(n =&gt; new CsvRow(n)).ToList();
/// }
/// </code>
/// </example>
public abstract class MultiStreamLoaderBaseContractTests<TSut, TRecord, TProgress>
    where TSut : MultiStreamLoaderBase<TRecord, TProgress>
    where TRecord : notnull
    where TProgress : MultiStreamProgress
{
    // ------------------------------------------------------------------
    // Factory methods
    // ------------------------------------------------------------------

    /// <summary>
    /// Creates the loader under test with the given destination selector.
    /// </summary>
    /// <param name="destinationSelector">The selector to pass to the loader.</param>
    /// <returns>A new loader.</returns>
    protected abstract TSut CreateSut(Func<TRecord, NamedStreamDestination> destinationSelector);



    /// <summary>
    /// Creates <paramref name="count"/> distinct records that survive a round trip through the loader and
    /// <see cref="ParseDestination"/> unchanged.
    /// </summary>
    /// <param name="count">The number of records.</param>
    /// <returns>The records.</returns>
    protected abstract IReadOnlyList<TRecord> CreateRecords(int count);



    /// <summary>
    /// Reads back the records your loader wrote to one destination, in order.
    /// </summary>
    /// <param name="text">Everything written to the destination's stream.</param>
    /// <returns>The records in that text.</returns>
    protected abstract IReadOnlyList<TRecord> ParseDestination(string text);



    // ------------------------------------------------------------------
    // Tests
    // ------------------------------------------------------------------

    /// <summary>
    /// Records go to the destination the selector picks, in order, and every opened writer is disposed.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadAsync_routes_each_record_to_its_destination_and_disposes_every_writer()
    {
        var records = CreateRecords(4);
        var targets = new Targets();
        var sut = CreateSut(r => IndexOf(records, r) % 2 == 0 ? targets.Left : targets.Right);

        await sut.LoadAsync(AsAsync(records)).ConfigureAwait(false);

        Assert.Equal(new[] { records[0], records[2] }, ParseDestination(targets.LeftStream.Text));
        Assert.Equal(new[] { records[1], records[3] }, ParseDestination(targets.RightStream.Text));
        Assert.True(targets.LeftStream.IsDisposed, "The first destination's writer was not disposed.");
        Assert.True(targets.RightStream.IsDisposed, "The second destination's writer was not disposed.");
        Assert.Equal(records.Count, sut.CurrentItemCount);
    }



    /// <summary>An empty input opens no destination.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadAsync_with_no_records_opens_no_destination()
    {
        var targets = new Targets();
        var sut = CreateSut(_ => targets.Left);

        await sut.LoadAsync(AsAsync(Array.Empty<TRecord>())).ConfigureAwait(false);

        Assert.Equal(0, targets.LeftStream.Length);
        Assert.False(targets.LeftStream.IsDisposed, "A destination that was never chosen was disposed.");
    }



    /// <summary>When the input fails, every writer the loader opened is still disposed.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadAsync_when_the_input_fails_disposes_every_opened_writer()
    {
        var records = CreateRecords(1);
        var targets = new Targets();
        var sut = CreateSut(_ => targets.Left);

        await Assert.ThrowsAsync<InvalidDataException>(() => sut.LoadAsync(Failing(records))).ConfigureAwait(false);

        Assert.True(targets.LeftStream.IsDisposed, "The opened destination's writer was not disposed after a failure.");
    }



    /// <summary>Progress counts the destinations opened and names the last one written.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Progress_counts_destinations_and_names_the_last_one_written()
    {
        var records = CreateRecords(3);
        var targets = new Targets();
        var reports = new List<TProgress>();
        var sut = CreateSut(r => IndexOf(records, r) == 2 ? targets.Right : targets.Left);

        await sut.LoadAsync(AsAsync(records), new SynchronousProgress<TProgress>(reports.Add)).ConfigureAwait(false);

        var final = reports[reports.Count - 1];
        Assert.Equal(2, final.CurrentSourceNumber);
        Assert.Equal("right", final.CurrentSourceName);
        Assert.Equal(-1, final.TotalSourceCount);
    }



    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static int IndexOf(IReadOnlyList<TRecord> records, TRecord record)
    {
        var index = 0;
        while (!EqualityComparer<TRecord>.Default.Equals(records[index], record))
        {
            index++;
        }

        return index;
    }



    private static async IAsyncEnumerable<TRecord> AsAsync(IEnumerable<TRecord> records)
    {
        foreach (var record in records)
        {
            await Task.Yield();
            yield return record;
        }
    }



    private static async IAsyncEnumerable<TRecord> Failing(IEnumerable<TRecord> records)
    {
        await foreach (var record in AsAsync(records).ConfigureAwait(false))
        {
            yield return record;
        }

        throw new InvalidDataException("The input failed.");
    }



    private sealed class Targets
    {
        public Targets()
        {
            Left = new NamedStreamDestination(new StreamWriter(LeftStream), "left");
            Right = new NamedStreamDestination(new StreamWriter(RightStream), "right");
        }



        public DisposalTrackingStream LeftStream { get; } = new();



        public DisposalTrackingStream RightStream { get; } = new();



        public NamedStreamDestination Left { get; }



        public NamedStreamDestination Right { get; }
    }
}
