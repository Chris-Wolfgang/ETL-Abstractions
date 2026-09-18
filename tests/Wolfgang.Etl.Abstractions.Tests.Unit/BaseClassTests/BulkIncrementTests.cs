using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// The bulk <c>IncrementCurrentItemCount(int)</c> / <c>IncrementCurrentSkippedItemCount(int)</c> overloads (#475):
/// one interlocked add instead of <c>count</c> calls, for stages whose source skips or batches for them.
/// </summary>
public sealed class BulkIncrementTests
{
    private sealed class CountingExtractor : ExtractorBase<int, EtlProgress>
    {
        public void AddItems(int count) => IncrementCurrentItemCount(count);

        public void AddSkipped(int count) => IncrementCurrentSkippedItemCount(count);

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            yield break;
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }

    private sealed class CountingLoader : LoaderBase<int, EtlProgress>
    {
        public void AddItems(int count) => IncrementCurrentItemCount(count);

        public void AddSkipped(int count) => IncrementCurrentSkippedItemCount(count);

        protected override Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token) => Task.CompletedTask;

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }

    private sealed class CountingTransformer : TransformerBase<int, int, EtlProgress>
    {
        public void AddItems(int count) => IncrementCurrentItemCount(count);

        public void AddSkipped(int count) => IncrementCurrentSkippedItemCount(count);

        protected override async IAsyncEnumerable<int> TransformWorkerAsync(IAsyncEnumerable<int> items, [EnumeratorCancellation] CancellationToken token)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            yield break;
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }



    [Fact]
    public void IncrementCurrentItemCount_with_count_adds_the_count_in_one_step()
    {
        using var sut = new CountingExtractor();

        sut.AddItems(250);
        sut.AddItems(3);

        Assert.Equal(253, sut.CurrentItemCount);
    }



    [Fact]
    public void IncrementCurrentSkippedItemCount_with_count_adds_the_count_in_one_step()
    {
        using var sut = new CountingExtractor();

        sut.AddSkipped(10_000);

        Assert.Equal(10_000, sut.CurrentSkippedItemCount);
        Assert.Equal(0, sut.CurrentItemCount);
    }



    [Fact]
    public void Bulk_increment_with_zero_is_a_no_op_on_every_base()
    {
        using var extractor = new CountingExtractor();
        using var loader = new CountingLoader();
        using var transformer = new CountingTransformer();

        extractor.AddItems(0);
        extractor.AddSkipped(0);
        loader.AddItems(0);
        loader.AddSkipped(0);
        transformer.AddItems(0);
        transformer.AddSkipped(0);

        Assert.Equal(0, extractor.CurrentItemCount + extractor.CurrentSkippedItemCount);
        Assert.Equal(0, loader.CurrentItemCount + loader.CurrentSkippedItemCount);
        Assert.Equal(0, transformer.CurrentItemCount + transformer.CurrentSkippedItemCount);
    }



    [Fact]
    public void Bulk_increment_on_the_loader_and_transformer_bases_adds_the_count()
    {
        using var loader = new CountingLoader();
        using var transformer = new CountingTransformer();

        loader.AddItems(7);
        loader.AddSkipped(2);
        transformer.AddItems(11);
        transformer.AddSkipped(4);

        Assert.Equal((7, 2), (loader.CurrentItemCount, loader.CurrentSkippedItemCount));
        Assert.Equal((11, 4), (transformer.CurrentItemCount, transformer.CurrentSkippedItemCount));
    }



    [Fact]
    public void Bulk_increment_with_a_negative_count_throws_ArgumentOutOfRangeException_naming_count()
    {
        using var extractor = new CountingExtractor();
        using var loader = new CountingLoader();
        using var transformer = new CountingTransformer();

        Assert.Equal("count", Assert.Throws<ArgumentOutOfRangeException>(() => extractor.AddItems(-1)).ParamName);
        Assert.Equal("count", Assert.Throws<ArgumentOutOfRangeException>(() => extractor.AddSkipped(-1)).ParamName);
        Assert.Equal("count", Assert.Throws<ArgumentOutOfRangeException>(() => loader.AddItems(-1)).ParamName);
        Assert.Equal("count", Assert.Throws<ArgumentOutOfRangeException>(() => loader.AddSkipped(-1)).ParamName);
        Assert.Equal("count", Assert.Throws<ArgumentOutOfRangeException>(() => transformer.AddItems(-1)).ParamName);
        Assert.Equal("count", Assert.Throws<ArgumentOutOfRangeException>(() => transformer.AddSkipped(-1)).ParamName);
        Assert.Equal(0, extractor.CurrentItemCount + extractor.CurrentSkippedItemCount);
    }



    [Fact]
    public void Bulk_increment_is_atomic_under_concurrent_callers()
    {
        using var sut = new CountingExtractor();

        Parallel.For(0, 64, _ =>
        {
            for (var i = 0; i < 100; i++)
            {
                sut.AddItems(3);
                sut.AddSkipped(2);
            }
        });

        Assert.Equal(64 * 100 * 3, sut.CurrentItemCount);
        Assert.Equal(64 * 100 * 2, sut.CurrentSkippedItemCount);
    }
}
