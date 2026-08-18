using Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests.TestDoubles;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests;

/// <summary>
/// Directly exercises the interface-required overloads (and their argument-null guards) on the
/// multi-capability pipeline test doubles that a bound pipeline path never routes to, so every
/// overload each double must implement is verified rather than left dead.
/// </summary>
public class OverloadDoubleCoverageTests
{
    private static readonly int[] Items = { 1, 2, 3 };

    private static IAsyncEnumerable<int> Source() => Items.ToAsyncEnumerable();


    // ------------------------------------------------------------------
    // Extractors
    // ------------------------------------------------------------------

    [Fact]
    public async Task FullExtractor_yields_the_sequence_from_the_overloads_the_pipeline_skips()
    {
        var sut = new FullExtractor<int, string>(Items, "p");

        Assert.Equal(Items, await sut.ExtractAsync().ToListAsync());
        Assert.True(sut.ParameterlessOverloadWasCalled);

        Assert.Equal(Items, await sut.ExtractAsync(new Progress<string>()).ToListAsync());
        Assert.True(sut.ProgressOnlyOverloadWasCalled);
    }


    [Fact]
    public async Task FullExtractor_progress_overloads_reject_null_progress()
    {
        var sut = new FullExtractor<int, string>(Items, "p");

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await sut.ExtractAsync(null!).ToListAsync());

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await sut.ExtractAsync(null!, CancellationToken.None).ToListAsync());
    }


    [Fact]
    public async Task ProgressOnlyExtractor_progress_overload_rejects_null_progress()
    {
        var sut = new ProgressOnlyExtractor<int, string>(Items, "p");

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await sut.ExtractAsync(null!).ToListAsync());
    }


    // ------------------------------------------------------------------
    // Loaders
    // ------------------------------------------------------------------

    [Fact]
    public async Task FullLoader_loads_from_the_overloads_the_pipeline_skips()
    {
        var sut = new FullLoader<int, string>("p");

        await sut.LoadAsync(Source());
        Assert.True(sut.ParameterlessOverloadWasCalled);

        var progressLoader = new FullLoader<int, string>("p");
        await progressLoader.LoadAsync(Source(), new Progress<string>());
        Assert.True(progressLoader.ProgressOnlyOverloadWasCalled);
    }


    [Fact]
    public async Task FullLoader_progress_overloads_reject_null_progress()
    {
        var sut = new FullLoader<int, string>("p");

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await sut.LoadAsync(Source(), null!));

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await sut.LoadAsync(Source(), null!, CancellationToken.None));
    }


    [Fact]
    public async Task ProgressOnlyLoader_progress_overload_rejects_null_progress()
    {
        var sut = new ProgressOnlyLoader<int, string>("p");

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await sut.LoadAsync(Source(), null!));
    }


    // ------------------------------------------------------------------
    // Transformers
    // ------------------------------------------------------------------

    [Fact]
    public async Task FullTransformer_transforms_from_the_overloads_the_pipeline_skips()
    {
        var sut = new FullTransformer<int, int, string>(x => x, "p");

        Assert.Equal(Items, await sut.TransformAsync(Source()).ToListAsync());
        Assert.True(sut.ParameterlessOverloadWasCalled);

        Assert.Equal(Items, await sut.TransformAsync(Source(), new Progress<string>()).ToListAsync());
        Assert.True(sut.ProgressOnlyOverloadWasCalled);
    }


    [Fact]
    public async Task FullTransformer_progress_overloads_reject_null_progress()
    {
        var sut = new FullTransformer<int, int, string>(x => x, "p");

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await sut.TransformAsync(Source(), null!).ToListAsync());

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await sut.TransformAsync(Source(), null!, CancellationToken.None).ToListAsync());
    }


    [Fact]
    public async Task ProgressOnlyTransformer_progress_overload_rejects_null_progress()
    {
        var sut = new ProgressOnlyTransformer<int, int, string>(x => x, "p");

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await sut.TransformAsync(Source(), null!).ToListAsync());
    }
}
