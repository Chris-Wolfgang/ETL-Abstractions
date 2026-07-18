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
    public async Task Through_pipes_records_through_the_transformer()
    {
        var loader = new CollectingLoader<string>();

        await EtlPipeline.From(AsyncSource(1, 2, 3))
            .Through(new MapTransformer<int, string>(x => $"n{x}"))
            .To(loader)
            .RunAsync();

        Assert.Equal(new[] { "n1", "n2", "n3" }, loader.Loaded);
    }


    [Fact]
    public async Task Through_can_be_chained()
    {
        var loader = new CollectingLoader<int>();

        await EtlPipeline.From(AsyncSource(1, 2, 3))
            .Through(new MapTransformer<int, int>(x => x + 1))
            .Through(new MapTransformer<int, int>(x => x * 10))
            .To(loader)
            .RunAsync();

        Assert.Equal(new[] { 20, 30, 40 }, loader.Loaded);
    }


    [Fact]
    public async Task Through_forwards_the_cancellation_token_to_a_cancellation_aware_transformer()
    {
        using var cts = new CancellationTokenSource();
        var transformer = new TokenRecordingTransformer<int>();
        var loader = new CollectingLoader<int>();

        await EtlPipeline.From(AsyncSource(1, 2, 3))
            .Through(transformer)
            .To(loader)
            .RunAsync(null, cts.Token);

        Assert.Equal(cts.Token, transformer.LastToken);
        Assert.Equal(new[] { 1, 2, 3 }, loader.Loaded);
    }


    [Fact]
    public async Task AsAsyncEnumerable_exposes_the_composed_stream()
    {
        var stream = EtlPipeline.From(AsyncSource(1, 2, 3))
            .Through(new MapTransformer<int, int>(x => x * 100))
            .AsAsyncEnumerable();

        var result = await Collect(stream);

        Assert.Equal(new[] { 100, 200, 300 }, result);
    }


    [Fact]
    public async Task RunAsync_propagates_an_exception_thrown_by_a_transformer()
    {
        var loader = new CollectingLoader<int>();

        var sink = EtlPipeline.From(AsyncSource(1, 2, 3))
            .Through(new MapTransformer<int, int>(_ => throw new InvalidOperationException("boom")))
            .To(loader);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sink.RunAsync());
        Assert.Equal("boom", ex.Message);
    }


    [Fact]
    public async Task RunAsync_observes_cancellation_mid_stream()
    {
        using var cts = new CancellationTokenSource();
        var loader = new CollectingLoader<int>();

        var sink = EtlPipeline.From(AsyncSource(1, 2, 3, 4, 5))
            .Through(new CancelingTransformer<int>(cts, 2))
            .To(loader);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sink.RunAsync(null, cts.Token));
        Assert.True(loader.Loaded.Count < 5);
    }


    [Fact]
    public async Task RunAsync_reports_extracted_and_loaded_counters()
    {
        var reports = new List<EtlPipelineProgress>();
        var progress = new SynchronousProgress<EtlPipelineProgress>(reports.Add);
        var loader = new CollectingLoader<int>();

        await EtlPipeline.From(AsyncSource(1, 2, 3, 4, 5))
            .To(loader)
            .RunAsync(progress);

        var final = reports.Last();
        Assert.Equal(5, final.RecordsExtracted);
        Assert.Equal(5, final.RecordsLoaded);
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
    public void Through_when_transformer_is_null_throws_ArgumentNullException()
    {
        var pipeline = EtlPipeline.From(AsyncSource(1));
        Assert.Throws<ArgumentNullException>(() => pipeline.Through((ITransformAsync<int, int>)null!));
    }


    [Fact]
    public void Through_when_cancellation_aware_transformer_is_null_throws_ArgumentNullException()
    {
        var pipeline = EtlPipeline.From(AsyncSource(1));
        Assert.Throws<ArgumentNullException>(() => pipeline.Through((ITransformWithCancellationAsync<int, int>)null!));
    }


    [Fact]
    public void To_when_loader_is_null_throws_ArgumentNullException()
    {
        var pipeline = EtlPipeline.From(AsyncSource(1));
        Assert.Throws<ArgumentNullException>(() => pipeline.To<EtlProgress>(null!));
    }
}
