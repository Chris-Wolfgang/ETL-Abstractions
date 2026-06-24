using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// Covers <see cref="BatchTransformerBase{TSource, TProgress}"/> (issue #89): fixed-size batching,
/// trailing-partial-batch flush, batch-level counting, and <c>BatchSize</c> validation.
/// </summary>
public class BatchTransformerBaseTests
{
    private sealed class IntBatcher : BatchTransformerBase<int, EtlProgress>
    {
        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    private static async IAsyncEnumerable<int> Range(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return i;
        }

        await Task.CompletedTask;
    }


    private static async Task<List<IReadOnlyList<int>>> CollectAsync(IAsyncEnumerable<IReadOnlyList<int>> batches)
    {
        var result = new List<IReadOnlyList<int>>();
        await foreach (var batch in batches)
        {
            result.Add(batch);
        }

        return result;
    }


    [Fact]
    public void BatchSize_defaults_to_100()
    {
        Assert.Equal(100, new IntBatcher().BatchSize);
    }


    [Fact]
    public void BatchSize_when_set_below_1_throws_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new IntBatcher { BatchSize = 0 });
    }


    [Fact]
    public async Task Evenly_divisible_source_yields_full_batches()
    {
        var sut = new IntBatcher { BatchSize = 5 };

        var batches = await CollectAsync(sut.TransformAsync(Range(10)));

        Assert.Equal(2, batches.Count);
        Assert.All(batches, b => Assert.Equal(5, b.Count));
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, batches[0]);
        Assert.Equal(new[] { 5, 6, 7, 8, 9 }, batches[1]);
    }


    [Fact]
    public async Task Remainder_is_flushed_as_a_final_partial_batch()
    {
        var sut = new IntBatcher { BatchSize = 3 };

        var batches = await CollectAsync(sut.TransformAsync(Range(7)));

        Assert.Equal(3, batches.Count);
        Assert.Equal(new[] { 0, 1, 2 }, batches[0]);
        Assert.Equal(new[] { 3, 4, 5 }, batches[1]);
        Assert.Equal(new[] { 6 }, batches[2]);
    }


    [Fact]
    public async Task Source_smaller_than_batch_size_yields_a_single_partial_batch()
    {
        var sut = new IntBatcher { BatchSize = 5 };

        var batches = await CollectAsync(sut.TransformAsync(Range(2)));

        Assert.Single(batches);
        Assert.Equal(new[] { 0, 1 }, batches[0]);
    }


    [Fact]
    public async Task Empty_source_yields_no_batches()
    {
        var sut = new IntBatcher { BatchSize = 5 };

        var batches = await CollectAsync(sut.TransformAsync(Range(0)));

        Assert.Empty(batches);
    }


    [Fact]
    public async Task CurrentItemCount_counts_batches_not_input_items()
    {
        var sut = new IntBatcher { BatchSize = 3 };

        await CollectAsync(sut.TransformAsync(Range(7)));

        // 7 items at batch size 3 => 3 batches (3 + 3 + 1).
        Assert.Equal(3, sut.CurrentItemCount);
    }
}
