using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests.TestDoubles;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests;

/// <summary>
/// End-to-end tests using real classes that implement ONLY the base
/// <see cref="IExtractAsync{T}"/>, <see cref="ITransformAsync{TSource, TDestination}"/>, and
/// <see cref="ILoadAsync{T}"/> interfaces — no progress, no cancellation.
/// </summary>
public class BareTests
{
    [Fact]
    public async Task Extract_Load_runs_through_real_bare_classes()
    {
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var extractor = new BareExtractor<int>(source);
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Load(loader)
            .RunAsync();

        Assert.Equal(source, loader.Loaded);
        Assert.Equal(1, extractor.ExtractAsyncCallCount);
        Assert.Equal(1, loader.LoadAsyncCallCount);
    }


    [Fact]
    public async Task Extract_Transform_Load_runs_through_real_bare_classes()
    {
        var source = new List<int> { 1, 2, 3 };
        var extractor = new BareExtractor<int>(source);
        var transformer = new BareTransformer<int, int>(x => x * 10);
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Transform(transformer)
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 10, 20, 30 }, loader.Loaded);
        Assert.Equal(1, transformer.TransformAsyncCallCount);
    }


    [Fact]
    public async Task Extract_Transform_Transform_Load_chains_multiple_bare_transformers()
    {
        var extractor = new BareExtractor<int>(new[] { 1, 2, 3 });
        var addOne = new BareTransformer<int, int>(x => x + 1);
        var toString = new BareTransformer<int, string>(x => x.ToString(System.Globalization.CultureInfo.InvariantCulture));
        var loader = new BareLoader<string>();

        await Pipeline
            .Extract(extractor)
            .Transform(addOne)
            .Transform(toString)
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { "2", "3", "4" }, loader.Loaded);
    }


    [Fact]
    public async Task RunAsync_with_CancellationToken_does_not_forward_to_bare_stages()
    {
        // Bare stages have no ExtractAsync(CancellationToken) overload; the pipeline must call
        // the parameterless ExtractAsync(), meaning the caller's token is dropped on the floor
        // before reaching the bare extractor. Run with a token that was never cancelled — the
        // pipeline should still complete normally.
        var extractor = new BareExtractor<int>(new[] { 1, 2 });
        var loader = new BareLoader<int>();

        using var cts = new CancellationTokenSource();
        await Pipeline
            .Extract(extractor)
            .Load(loader)
            .RunAsync(cts.Token);

        Assert.Equal(new[] { 1, 2 }, loader.Loaded);
    }
}
