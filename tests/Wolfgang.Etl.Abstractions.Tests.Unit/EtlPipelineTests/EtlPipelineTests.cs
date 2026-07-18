using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.EtlPipelineTests;

public class EtlPipelineTests
{
    private static async IAsyncEnumerable<T> AsyncSource<T>(params T[] items)
    {
        foreach (var item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }


    private static async Task<List<T>> Collect<T>(IAsyncEnumerable<T> source, CancellationToken token = default)
    {
        var result = new List<T>();
        await foreach (var item in source.WithCancellation(token))
        {
            result.Add(item);
        }

        return result;
    }


    [Fact]
    public async Task RunAsync_when_source_is_IAsyncEnumerable_delivers_all_records_to_the_loader()
    {
        var loader = new CollectingLoader<int>();

        await EtlPipeline.From(AsyncSource(1, 2, 3))
            .To(loader)
            .RunAsync();

        Assert.Equal(new[] { 1, 2, 3 }, loader.Loaded);
    }


    [Fact]
    public async Task RunAsync_when_source_is_an_extractor_delivers_all_records_to_the_loader()
    {
        var loader = new CollectingLoader<int>();
        var extractor = new SeededExtractor<int>(new[] { 10, 20, 30 });

        await EtlPipeline.From(extractor)
            .To(loader)
            .RunAsync();

        Assert.Equal(new[] { 10, 20, 30 }, loader.Loaded);
    }


    [Fact]
    public async Task Where_keeps_only_matching_records()
    {
        var loader = new CollectingLoader<int>();

        await EtlPipeline.From(AsyncSource(1, 2, 3, 4, 5))
            .Where(x => x % 2 == 0)
            .To(loader)
            .RunAsync();

        Assert.Equal(new[] { 2, 4 }, loader.Loaded);
    }


    [Fact]
    public async Task Where_async_keeps_only_matching_records()
    {
        var loader = new CollectingLoader<int>();

        await EtlPipeline.From(AsyncSource(1, 2, 3, 4, 5))
            .Where(x => new ValueTask<bool>(x > 3))
            .To(loader)
            .RunAsync();

        Assert.Equal(new[] { 4, 5 }, loader.Loaded);
    }


    [Fact]
    public async Task Select_projects_each_record()
    {
        var loader = new CollectingLoader<string>();

        await EtlPipeline.From(AsyncSource(1, 2, 3))
            .Select(x => $"n{x}")
            .To(loader)
            .RunAsync();

        Assert.Equal(new[] { "n1", "n2", "n3" }, loader.Loaded);
    }


    [Fact]
    public async Task Select_async_projects_each_record()
    {
        var loader = new CollectingLoader<int>();

        await EtlPipeline.From(AsyncSource(1, 2, 3))
            .Select(x => new ValueTask<int>(x * 10))
            .To(loader)
            .RunAsync();

        Assert.Equal(new[] { 10, 20, 30 }, loader.Loaded);
    }


    [Fact]
    public async Task SelectMany_flattens_projected_streams()
    {
        var loader = new CollectingLoader<int>();

        await EtlPipeline.From(AsyncSource(1, 2))
            .SelectMany(x => AsyncSource(x, x))
            .To(loader)
            .RunAsync();

        Assert.Equal(new[] { 1, 1, 2, 2 }, loader.Loaded);
    }


    [Fact]
    public async Task Distinct_removes_duplicate_keys()
    {
        var loader = new CollectingLoader<int>();

        await EtlPipeline.From(AsyncSource(1, 2, 2, 3, 1, 3))
            .Distinct(x => x)
            .To(loader)
            .RunAsync();

        Assert.Equal(new[] { 1, 2, 3 }, loader.Loaded);
    }


    [Fact]
    public async Task Distinct_uses_the_supplied_comparer()
    {
        var loader = new CollectingLoader<string>();

        await EtlPipeline.From(AsyncSource("a", "A", "b", "B"))
            .Distinct(x => x, StringComparer.OrdinalIgnoreCase)
            .To(loader)
            .RunAsync();

        Assert.Equal(new[] { "a", "b" }, loader.Loaded);
    }


    [Fact]
    public async Task Take_stops_after_the_requested_count()
    {
        var loader = new CollectingLoader<int>();

        await EtlPipeline.From(AsyncSource(1, 2, 3, 4, 5))
            .Take(2)
            .To(loader)
            .RunAsync();

        Assert.Equal(new[] { 1, 2 }, loader.Loaded);
    }


    [Fact]
    public async Task Take_zero_delivers_nothing()
    {
        var loader = new CollectingLoader<int>();

        await EtlPipeline.From(AsyncSource(1, 2, 3))
            .Take(0)
            .To(loader)
            .RunAsync();

        Assert.Empty(loader.Loaded);
    }


    [Fact]
    public async Task Skip_discards_the_leading_records()
    {
        var loader = new CollectingLoader<int>();

        await EtlPipeline.From(AsyncSource(1, 2, 3, 4, 5))
            .Skip(3)
            .To(loader)
            .RunAsync();

        Assert.Equal(new[] { 4, 5 }, loader.Loaded);
    }


    [Fact]
    public async Task Tap_runs_the_side_effect_without_altering_the_stream()
    {
        var seen = new List<int>();
        var loader = new CollectingLoader<int>();

        await EtlPipeline.From(AsyncSource(1, 2, 3))
            .Tap(seen.Add)
            .To(loader)
            .RunAsync();

        Assert.Equal(new[] { 1, 2, 3 }, seen);
        Assert.Equal(new[] { 1, 2, 3 }, loader.Loaded);
    }


    [Fact]
    public async Task Tap_async_runs_the_side_effect_without_altering_the_stream()
    {
        var seen = new List<int>();
        var loader = new CollectingLoader<int>();

        await EtlPipeline.From(AsyncSource(1, 2, 3))
            .Tap(x =>
            {
                seen.Add(x);
                return default;
            })
            .To(loader)
            .RunAsync();

        Assert.Equal(new[] { 1, 2, 3 }, seen);
        Assert.Equal(new[] { 1, 2, 3 }, loader.Loaded);
    }


    [Fact]
    public async Task Buffer_batches_records_including_a_smaller_final_batch()
    {
        var loader = new CollectingLoader<IReadOnlyList<int>>();

        await EtlPipeline.From(AsyncSource(1, 2, 3, 4, 5))
            .Buffer(2)
            .To(loader)
            .RunAsync();

        Assert.Equal(3, loader.Loaded.Count);
        Assert.Equal(new[] { 1, 2 }, loader.Loaded[0]);
        Assert.Equal(new[] { 3, 4 }, loader.Loaded[1]);
        Assert.Equal(new[] { 5 }, loader.Loaded[2]);
    }


    [Fact]
    public async Task AsAsyncEnumerable_exposes_the_composed_stream()
    {
        var stream = EtlPipeline.From(AsyncSource(1, 2, 3, 4))
            .Where(x => x % 2 == 0)
            .Select(x => x * 100)
            .AsAsyncEnumerable();

        var result = await Collect(stream);

        Assert.Equal(new[] { 200, 400 }, result);
    }


    [Fact]
    public async Task RunAsync_propagates_an_exception_thrown_by_an_operator()
    {
        var loader = new CollectingLoader<int>();

        var sink = EtlPipeline.From(AsyncSource(1, 2, 3))
            .Select((Func<int, int>)(_ => throw new InvalidOperationException("boom")))
            .To(loader);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sink.RunAsync());
        Assert.Equal("boom", ex.Message);
    }


    [Fact]
    public async Task RunAsync_observes_cancellation_mid_stream()
    {
        using var cts = new CancellationTokenSource();
        var count = 0;
        var loader = new CollectingLoader<int>();

        var sink = EtlPipeline.From(AsyncSource(1, 2, 3, 4, 5))
            .Tap(_ =>
            {
                count++;
                if (count == 2)
                {
                    cts.Cancel();
                }
            })
            .To(loader);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sink.RunAsync(null, cts.Token));
        Assert.True(loader.Loaded.Count < 5);
    }


    [Fact]
    public async Task RunAsync_reports_progress_counters()
    {
        var reports = new List<EtlPipelineProgress>();
        var progress = new SynchronousProgress<EtlPipelineProgress>(reports.Add);
        var loader = new CollectingLoader<int>();

        await EtlPipeline.From(AsyncSource(1, 2, 3, 4, 5))
            .Where(x => x % 2 == 1)
            .To(loader)
            .RunAsync(progress);

        var final = reports.Last();
        Assert.Equal(5, final.RecordsExtracted);
        Assert.Equal(3, final.RecordsLoaded);
        Assert.Equal(2, final.RecordsFiltered);
        Assert.Equal(0, final.RecordsErrored);
    }


    [Fact]
    public void Source_sentinel_is_available_for_format_package_extensions()
    {
        Assert.NotNull(EtlPipeline.Source);
    }


    [Fact]
    public void From_when_stream_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => EtlPipeline.From<int>((IAsyncEnumerable<int>)null!));
    }


    [Fact]
    public void From_when_extractor_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => EtlPipeline.From<int, EtlProgress>(null!));
    }


    [Fact]
    public void Where_when_predicate_is_null_throws_ArgumentNullException()
    {
        var pipeline = EtlPipeline.From(AsyncSource(1));
        Assert.Throws<ArgumentNullException>(() => pipeline.Where((Func<int, bool>)null!));
    }


    [Fact]
    public void Select_when_selector_is_null_throws_ArgumentNullException()
    {
        var pipeline = EtlPipeline.From(AsyncSource(1));
        Assert.Throws<ArgumentNullException>(() => pipeline.Select((Func<int, string>)null!));
    }


    [Fact]
    public void To_when_loader_is_null_throws_ArgumentNullException()
    {
        var pipeline = EtlPipeline.From(AsyncSource(1));
        Assert.Throws<ArgumentNullException>(() => pipeline.To<EtlProgress>(null!));
    }


    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Take_when_count_is_negative_throws_ArgumentOutOfRangeException(int count)
    {
        var pipeline = EtlPipeline.From(AsyncSource(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => pipeline.Take(count));
    }


    [Fact]
    public void Skip_when_count_is_negative_throws_ArgumentOutOfRangeException()
    {
        var pipeline = EtlPipeline.From(AsyncSource(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => pipeline.Skip(-1));
    }


    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Buffer_when_size_is_less_than_one_throws_ArgumentOutOfRangeException(int size)
    {
        var pipeline = EtlPipeline.From(AsyncSource(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => pipeline.Buffer(size));
    }
}
