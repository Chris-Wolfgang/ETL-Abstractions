using System;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests.TestDoubles;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests;

/// <summary>
/// Exercises the paths that fire when a progress-capable stage is appended but
/// <c>WithProgress</c> is never called on it — the pipeline falls back to the stage's
/// no-progress overload. These paths are internal plumbing between
/// <c>ExtractStageWithProgress</c> / <c>TransformStageWithProgress</c> and the no-progress
/// <c>ExtractStage</c> / <c>TransformStage</c> / <c>PipelineImpl</c> / <c>PipelineWithLoadProgress</c>.
/// </summary>
public class NoProgressPathTests
{
    [Fact]
    public async Task Transform_progress_only_without_WithProgress_calls_parameterless_overload()
    {
        var extractor = new BareExtractor<int>(new[] { 1, 2, 3 });
        var transformer = new ProgressOnlyTransformer<int, int, string>(x => x + 1, "t");
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Transform(transformer)   // no .WithProgress() -> no-progress overload fires
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 2, 3, 4 }, loader.Loaded);
        Assert.False(transformer.ProgressOverloadWasCalled);
        Assert.True(transformer.ParameterlessOverloadWasCalled);
    }


    [Fact]
    public async Task Transform_full_without_WithProgress_calls_token_only_overload()
    {
        var extractor = new BareExtractor<int>(new[] { 1, 2, 3 });
        var transformer = new FullTransformer<int, int, string>(x => x + 1, "t");
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Transform(transformer)   // no .WithProgress() -> token-only overload fires
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 2, 3, 4 }, loader.Loaded);
        Assert.False(transformer.FullOverloadWasCalled);
        Assert.True(transformer.TokenOnlyOverloadWasCalled);
    }


    [Fact]
    public async Task Load_progress_only_without_WithProgress_calls_parameterless_overload()
    {
        var extractor = new BareExtractor<int>(new[] { 1, 2 });
        var loader = new ProgressOnlyLoader<int, string>("l");

        await Pipeline
            .Extract(extractor)
            .Load(loader)            // no .WithProgress() -> no-progress overload fires
            .RunAsync();

        Assert.Equal(new[] { 1, 2 }, loader.Loaded);
        Assert.False(loader.ProgressOverloadWasCalled);
        Assert.True(loader.ParameterlessOverloadWasCalled);
    }


    [Fact]
    public async Task Load_full_without_WithProgress_calls_token_only_overload()
    {
        var extractor = new BareExtractor<int>(new[] { 1, 2 });
        var loader = new FullLoader<int, string>("l");

        await Pipeline
            .Extract(extractor)
            .Load(loader)            // no .WithProgress() -> token-only overload fires
            .RunAsync();

        Assert.Equal(new[] { 1, 2 }, loader.Loaded);
        Assert.False(loader.FullOverloadWasCalled);
        Assert.True(loader.TokenOnlyOverloadWasCalled);
    }


    [Fact]
    public async Task Load_progress_only_after_Transform_without_WithProgress_calls_parameterless_overload()
    {
        var extractor = new BareExtractor<int>(new[] { 1, 2 });
        var transformer = new BareTransformer<int, int>(x => x * 2);
        var loader = new ProgressOnlyLoader<int, string>("l");

        await Pipeline
            .Extract(extractor)
            .Transform(transformer)
            .Load(loader)            // no .WithProgress() -> no-progress overload fires
            .RunAsync();

        Assert.Equal(new[] { 2, 4 }, loader.Loaded);
        Assert.False(loader.ProgressOverloadWasCalled);
        Assert.True(loader.ParameterlessOverloadWasCalled);
    }


    [Fact]
    public async Task Transform_progress_only_after_Transform_without_WithProgress_calls_parameterless_overload()
    {
        var extractor = new BareExtractor<int>(new[] { 1, 2 });
        var t1 = new BareTransformer<int, int>(x => x * 10);
        var t2 = new ProgressOnlyTransformer<int, int, string>(x => x + 1, "t2");
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Transform(t1)
            .Transform(t2)           // no .WithProgress() -> no-progress overload fires
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 11, 21 }, loader.Loaded);
        Assert.False(t2.ProgressOverloadWasCalled);
        Assert.True(t2.ParameterlessOverloadWasCalled);
    }


    [Fact]
    public async Task ExtractStageWithProgress_without_WithProgress_Transform_cancel_only()
    {
        var extractor = new FullExtractor<int, string>(new[] { 1, 2 }, "e");
        var transformer = new CancelOnlyTransformer<int, int>(x => x + 100);
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor)       // progress-capable
            .Transform(transformer)   // but WithProgress never called
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 101, 102 }, loader.Loaded);
        Assert.True(transformer.TokenOverloadWasCalled);
    }


    [Fact]
    public async Task ExtractStageWithProgress_without_WithProgress_Load_bare()
    {
        var extractor = new FullExtractor<int, string>(new[] { 1, 2 }, "e");
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor)       // progress-capable
            .Load(loader)             // bare loader
            .RunAsync();

        Assert.Equal(new[] { 1, 2 }, loader.Loaded);
        Assert.Equal(1, loader.LoadAsyncCallCount);
    }


    [Fact]
    public async Task ExtractStageWithProgress_without_WithProgress_Load_cancel_only()
    {
        var extractor = new FullExtractor<int, string>(new[] { 1, 2 }, "e");
        var loader = new CancelOnlyLoader<int>();

        await Pipeline
            .Extract(extractor)       // progress-capable
            .Load(loader)             // cancel-only loader
            .RunAsync();

        Assert.Equal(new[] { 1, 2 }, loader.Loaded);
        Assert.True(loader.TokenOverloadWasCalled);
    }
}
