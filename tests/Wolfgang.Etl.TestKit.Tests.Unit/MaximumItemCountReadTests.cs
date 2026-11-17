using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// A double limited to <c>MaximumItemCount</c> items reads exactly that many from its source: it
/// does not read one more record only to find the limit and throw the record away.
/// </summary>
public sealed class MaximumItemCountReadTests
{
    private const int Limit = 3;
    private const int SourceLength = 10;



    [Fact]
    public async Task TestExtractor_when_limited_leaves_the_next_item_in_a_caller_owned_enumerator()
    {
        using var enumerator = Enumerable.Range(1, SourceLength).GetEnumerator();
        var extractor = new TestExtractor<int>(enumerator, new ExtractorOptions { MaximumItemCount = Limit });

        var extracted = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(new[] { 1, 2, 3 }, extracted);
        Assert.True(enumerator.MoveNext());
        Assert.Equal(4, enumerator.Current);
    }



    [Fact]
    public async Task DelayingExtractor_when_limited_reads_exactly_MaximumItemCount_items()
    {
        var source = new CountingSource();
        var extractor = new DelayingExtractor<int>
        (
            source.Items(),
            TimeSpan.Zero,
            new ExtractorOptions { MaximumItemCount = Limit }
        );

        var extracted = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(Limit, extracted.Count);
        Assert.Equal(Limit, source.ReadCount);
    }



    [Fact]
    public async Task RetryingExtractor_when_limited_reads_exactly_MaximumItemCount_items()
    {
        var source = new CountingSource();
        var extractor = new RetryingExtractor<int>
        (
            source.Items(),
            failFirstAttempts: 0,
            maxAttempts: 1,
            new ExtractorOptions { MaximumItemCount = Limit }
        );

        var extracted = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(Limit, extracted.Count);
        Assert.Equal(Limit, source.ReadCount);
    }



    [Fact]
    public async Task FaultyExtractor_when_limited_reads_exactly_MaximumItemCount_items()
    {
        var source = new CountingSource();
        var extractor = new FaultyExtractor<int>(source.Items(), new ExtractorOptions { MaximumItemCount = Limit });

        var extracted = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(Limit, extracted.Count);
        Assert.Equal(Limit, source.ReadCount);
    }



    [Fact]
    public async Task TestTransformer_when_limited_reads_exactly_MaximumItemCount_items()
    {
        var source = new CountingSource();
        var transformer = new TestTransformer<int>(new TransformerOptions { MaximumItemCount = Limit });

        var transformed = await transformer.TransformAsync(source.ItemsAsync()).ToListAsync();

        Assert.Equal(Limit, transformed.Count);
        Assert.Equal(Limit, source.ReadCount);
    }



    [Fact]
    public async Task FaultyTransformer_when_limited_reads_exactly_MaximumItemCount_items()
    {
        var source = new CountingSource();
        var transformer = new FaultyTransformer<int>(new TransformerOptions { MaximumItemCount = Limit });

        var transformed = await transformer.TransformAsync(source.ItemsAsync()).ToListAsync();

        Assert.Equal(Limit, transformed.Count);
        Assert.Equal(Limit, source.ReadCount);
    }



    [Fact]
    public async Task TestLoader_when_limited_reads_exactly_MaximumItemCount_items()
    {
        var source = new CountingSource();
        var loader = new TestLoader<int>(collectItems: true, new LoaderOptions { MaximumItemCount = Limit });

        await loader.LoadAsync(source.ItemsAsync());

        Assert.Equal(Limit, loader.CurrentItemCount);
        Assert.Equal(Limit, source.ReadCount);
    }



    [Fact]
    public async Task SnapshotTestLoader_when_limited_reads_exactly_MaximumItemCount_items()
    {
        var source = new CountingSource();
        var loader = new SnapshotTestLoader<int>(new LoaderOptions { MaximumItemCount = Limit });

        await loader.LoadAsync(source.ItemsAsync());

        Assert.Equal(Limit, loader.CurrentItemCount);
        Assert.Equal(Limit, source.ReadCount);
    }



    [Fact]
    public async Task FaultyLoader_when_limited_reads_exactly_MaximumItemCount_items()
    {
        var source = new CountingSource();
        var loader = new FaultyLoader<int>(collectItems: true, new LoaderOptions { MaximumItemCount = Limit });

        await loader.LoadAsync(source.ItemsAsync());

        Assert.Equal(Limit, loader.CurrentItemCount);
        Assert.Equal(Limit, source.ReadCount);
    }



    // A ten-item source that counts how many items a stage has read from it.
    private sealed class CountingSource
    {
        public int ReadCount { get; private set; }



        public IEnumerable<int> Items()
        {
            for (var i = 1; i <= SourceLength; i++)
            {
                ReadCount++;
                yield return i;
            }
        }



        public async IAsyncEnumerable<int> ItemsAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            foreach (var item in Items())
            {
                token.ThrowIfCancellationRequested();
                await Task.Yield();
                yield return item;
            }
        }
    }
}
