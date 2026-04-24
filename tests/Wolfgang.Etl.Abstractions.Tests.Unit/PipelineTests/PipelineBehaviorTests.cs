using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests.TestDoubles;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests;

/// <summary>
/// Capability-agnostic behavior tests: null guards, WithName, one-shot enforcement, and error
/// propagation. These use whichever test double fits; the behavior they exercise is the same
/// regardless of progress/cancellation support.
/// </summary>
public class PipelineBehaviorTests
{
    // ---------------------------------------------------------------
    // Null-argument guards on every Pipeline factory overload
    // ---------------------------------------------------------------

    [Fact]
    public void Extract_bare_when_extractor_is_null_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => Pipeline.Extract((IExtractAsync<int>)null!)
        );
    }


    [Fact]
    public void Extract_cancel_only_when_extractor_is_null_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => Pipeline.Extract((IExtractWithCancellationAsync<int>)null!)
        );
    }


    [Fact]
    public void Extract_progress_only_when_extractor_is_null_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => Pipeline.Extract((IExtractWithProgressAsync<int, string>)null!)
        );
    }


    [Fact]
    public void Extract_full_when_extractor_is_null_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => Pipeline.Extract((IExtractWithProgressAndCancellationAsync<int, string>)null!)
        );
    }


    // ---------------------------------------------------------------
    // Null-argument guards on stage-method overloads
    // ---------------------------------------------------------------

    [Fact]
    public void Bare_Transform_when_transformer_is_null_throws()
    {
        var stage = Pipeline.Extract(new BareExtractor<int>(new[] { 1 }));
        Assert.Throws<ArgumentNullException>
        (
            () => stage.Transform((ITransformAsync<int, int>)null!)
        );
    }


    [Fact]
    public void CancelOnly_Transform_when_transformer_is_null_throws()
    {
        var stage = Pipeline.Extract(new CancelOnlyExtractor<int>(new[] { 1 }));
        Assert.Throws<ArgumentNullException>
        (
            () => stage.Transform((ITransformWithCancellationAsync<int, int>)null!)
        );
    }


    [Fact]
    public void ProgressOnly_Transform_when_transformer_is_null_throws()
    {
        var stage = Pipeline.Extract(new BareExtractor<int>(new[] { 1 }));
        Assert.Throws<ArgumentNullException>
        (
            () => stage.Transform((ITransformWithProgressAsync<int, int, string>)null!)
        );
    }


    [Fact]
    public void Full_Transform_when_transformer_is_null_throws()
    {
        var stage = Pipeline.Extract(new BareExtractor<int>(new[] { 1 }));
        Assert.Throws<ArgumentNullException>
        (
            () => stage.Transform((ITransformWithProgressAndCancellationAsync<int, int, string>)null!)
        );
    }


    [Fact]
    public void Bare_Load_when_loader_is_null_throws()
    {
        var stage = Pipeline.Extract(new BareExtractor<int>(new[] { 1 }));
        Assert.Throws<ArgumentNullException>(() => stage.Load((ILoadAsync<int>)null!));
    }


    [Fact]
    public void CancelOnly_Load_when_loader_is_null_throws()
    {
        var stage = Pipeline.Extract(new BareExtractor<int>(new[] { 1 }));
        Assert.Throws<ArgumentNullException>(() => stage.Load((ILoadWithCancellationAsync<int>)null!));
    }


    [Fact]
    public void ProgressOnly_Load_when_loader_is_null_throws()
    {
        var stage = Pipeline.Extract(new BareExtractor<int>(new[] { 1 }));
        Assert.Throws<ArgumentNullException>(() => stage.Load((ILoadWithProgressAsync<int, string>)null!));
    }


    [Fact]
    public void Full_Load_when_loader_is_null_throws()
    {
        var stage = Pipeline.Extract(new BareExtractor<int>(new[] { 1 }));
        Assert.Throws<ArgumentNullException>(() => stage.Load((ILoadWithProgressAndCancellationAsync<int, string>)null!));
    }


    // ---------------------------------------------------------------
    // WithProgress null guards
    // ---------------------------------------------------------------

    [Fact]
    public void Extract_WithProgress_when_progress_is_null_throws()
    {
        var stage = Pipeline.Extract(new ProgressOnlyExtractor<int, string>(new[] { 1 }, "e"));
        Assert.Throws<ArgumentNullException>(() => stage.WithProgress(null!));
    }


    [Fact]
    public void Transform_WithProgress_when_progress_is_null_throws()
    {
        var stage = Pipeline
            .Extract(new BareExtractor<int>(new[] { 1 }))
            .Transform(new ProgressOnlyTransformer<int, int, string>(x => x, "t"));

        Assert.Throws<ArgumentNullException>(() => stage.WithProgress(null!));
    }


    [Fact]
    public void Load_WithProgress_when_progress_is_null_throws()
    {
        var pipeline = Pipeline
            .Extract(new BareExtractor<int>(new[] { 1 }))
            .Load(new ProgressOnlyLoader<int, string>("l"));

        Assert.Throws<ArgumentNullException>(() => pipeline.WithProgress(null!));
    }


    // ---------------------------------------------------------------
    // WithName and Name
    // ---------------------------------------------------------------

    [Fact]
    public void Name_defaults_to_null_on_bare_pipeline()
    {
        var pipeline = Pipeline
            .Extract(new BareExtractor<int>(new[] { 1 }))
            .Load(new BareLoader<int>());

        Assert.Null(pipeline.Name);
    }


    [Fact]
    public void Name_defaults_to_null_on_load_progress_pipeline()
    {
        var pipeline = Pipeline
            .Extract(new BareExtractor<int>(new[] { 1 }))
            .Load(new ProgressOnlyLoader<int, string>("l"));

        Assert.Null(pipeline.Name);
    }


    [Fact]
    public void WithName_sets_Name_on_bare_pipeline()
    {
        var pipeline = Pipeline
            .Extract(new BareExtractor<int>(new[] { 1 }))
            .Load(new BareLoader<int>())
            .WithName("nightly-import");

        Assert.Equal("nightly-import", pipeline.Name);
    }


    [Fact]
    public void WithName_sets_Name_on_load_progress_pipeline()
    {
        var pipeline = Pipeline
            .Extract(new BareExtractor<int>(new[] { 1 }))
            .Load(new ProgressOnlyLoader<int, string>("l"))
            .WithName("nightly-import");

        Assert.Equal("nightly-import", pipeline.Name);
    }


    [Fact]
    public void Bare_pipeline_WithName_when_name_is_null_throws()
    {
        var pipeline = Pipeline
            .Extract(new BareExtractor<int>(new[] { 1 }))
            .Load(new BareLoader<int>());

        Assert.Throws<ArgumentNullException>(() => pipeline.WithName(null!));
    }


    [Fact]
    public void Load_progress_pipeline_WithName_when_name_is_null_throws()
    {
        var pipeline = Pipeline
            .Extract(new BareExtractor<int>(new[] { 1 }))
            .Load(new ProgressOnlyLoader<int, string>("l"));

        Assert.Throws<ArgumentNullException>(() => pipeline.WithName(null!));
    }


    // ---------------------------------------------------------------
    // One-shot enforcement
    // ---------------------------------------------------------------

    [Fact]
    public async Task Bare_pipeline_second_RunAsync_throws()
    {
        var pipeline = Pipeline
            .Extract(new BareExtractor<int>(new[] { 1 }))
            .Load(new BareLoader<int>());

        await pipeline.RunAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline.RunAsync());
    }


    [Fact]
    public async Task Load_progress_pipeline_second_RunAsync_throws()
    {
        var pipeline = Pipeline
            .Extract(new BareExtractor<int>(new[] { 1 }))
            .Load(new ProgressOnlyLoader<int, string>("l"));

        await pipeline.RunAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline.RunAsync(CancellationToken.None));
    }


    // ---------------------------------------------------------------
    // Error propagation
    // ---------------------------------------------------------------

    [Fact]
    public async Task RunAsync_propagates_exception_from_transformer_unchanged()
    {
        var boom = new InvalidOperationException("boom");
        var extractor = new BareExtractor<int>(new[] { 1, 2, 3 });
        var thrower = new BareTransformer<int, int>(_ => throw boom);
        var loader = new BareLoader<int>();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>
        (
            () => Pipeline
                .Extract(extractor)
                .Transform(thrower)
                .Load(loader)
                .RunAsync()
        );

        Assert.Same(boom, ex);
    }


    [Fact]
    public async Task RunAsync_after_failure_loader_state_is_partially_populated()
    {
        var extractor = new BareExtractor<int>(new[] { 1, 2, 3, 4, 5 });
        var thrower = new BareTransformer<int, int>
        (
            x => x == 3 ? throw new InvalidOperationException("boom") : x
        );
        var loader = new BareLoader<int>();

        await Assert.ThrowsAsync<InvalidOperationException>
        (
            () => Pipeline
                .Extract(extractor)
                .Transform(thrower)
                .Load(loader)
                .RunAsync()
        );

        Assert.True(loader.Loaded.Count < 5);
        Assert.DoesNotContain(5, loader.Loaded);
    }
}
