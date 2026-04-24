using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;
using Wolfgang.Etl.TestKit;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests;

public class PipelineTests
{
    // ---------------------------------------------------------------
    // Null-argument guards
    // ---------------------------------------------------------------

    [Fact]
    public void Extract_when_extractor_is_null_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => Pipeline.Extract((IExtractWithCancellationAsync<int>)null!)
        );
    }


    [Fact]
    public void Extract_with_progress_when_extractor_is_null_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => Pipeline.Extract((IExtractWithProgressAndCancellationAsync<int, Report>)null!)
        );
    }


    [Fact]
    public void Transform_when_transformer_is_null_throws()
    {
        var stage = Pipeline.Extract(new TestExtractor<int>(new List<int> { 1, 2 }));

        Assert.Throws<ArgumentNullException>
        (
            () => stage.Transform((ITransformWithCancellationAsync<int, int>)null!)
        );
    }


    [Fact]
    public void Load_when_loader_is_null_throws()
    {
        var stage = Pipeline.Extract(new TestExtractor<int>(new List<int> { 1, 2 }));

        Assert.Throws<ArgumentNullException>
        (
            () => stage.Load((ILoadWithCancellationAsync<int>)null!)
        );
    }


    [Fact]
    public void WithProgress_on_extract_when_progress_is_null_throws()
    {
        var stage = Pipeline.Extract(new TestExtractor<int>(new List<int> { 1, 2 }));

        Assert.Throws<ArgumentNullException>
        (
            () => stage.WithProgress(null!)
        );
    }


    [Fact]
    public void WithName_when_name_is_null_throws()
    {
        var pipeline = Pipeline
            .Extract(new TestExtractor<int>(new List<int> { 1 }))
            .Load(new TestLoader<int>(collectItems: true));

        Assert.Throws<ArgumentNullException>
        (
            () => pipeline.WithName(null!)
        );
    }


    // ---------------------------------------------------------------
    // Composition
    // ---------------------------------------------------------------

    [Fact]
    public async Task RunAsync_extract_then_load_delivers_all_items()
    {
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var extractor = new TestExtractor<int>(source);
        var loader = new TestLoader<int>(collectItems: true);

        await Pipeline
            .Extract(extractor)
            .Load(loader)
            .RunAsync();

        Assert.Equal(source, loader.GetCollectedItems());
    }


    [Fact]
    public async Task RunAsync_extract_transform_load_transforms_each_item()
    {
        var source = new List<int> { 1, 2, 3 };
        var extractor = new TestExtractor<int>(source);
        var doubler = new MappingTransformer<int, int>(x => x * 2);
        var loader = new TestLoader<int>(collectItems: true);

        await Pipeline
            .Extract(extractor)
            .Transform(doubler)
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 2, 4, 6 }, loader.GetCollectedItems());
    }


    [Fact]
    public async Task RunAsync_with_multiple_transformers_chains_in_order()
    {
        var source = new List<int> { 1, 2, 3 };
        var extractor = new TestExtractor<int>(source);
        var addOne = new MappingTransformer<int, int>(x => x + 1);
        var mulTen = new MappingTransformer<int, int>(x => x * 10);
        var toString = new MappingTransformer<int, string>
        (
            x => x.ToString(CultureInfo.InvariantCulture)
        );
        var loader = new TestLoader<string>(collectItems: true);

        await Pipeline
            .Extract(extractor)
            .Transform(addOne)
            .Transform(mulTen)
            .Transform(toString)
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { "20", "30", "40" }, loader.GetCollectedItems());
    }


    // ---------------------------------------------------------------
    // Progress
    // ---------------------------------------------------------------

    [Fact]
    public async Task WithProgress_on_extractor_forwards_reports()
    {
        var reports = new List<Report>();
        var progress = new SynchronousProgress<Report>(reports.Add);

        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3 })
        {
            ReportingInterval = 1
        };
        var loader = new TestLoader<int>(collectItems: true);

        await Pipeline
            .Extract(extractor).WithProgress(progress)
            .Load(loader)
            .RunAsync();

        Assert.NotEmpty(reports);
    }


    [Fact]
    public async Task WithProgress_on_loader_forwards_reports()
    {
        var reports = new List<Report>();
        var progress = new SynchronousProgress<Report>(reports.Add);

        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3 });
        var loader = new TestLoader<int>(collectItems: true)
        {
            ReportingInterval = 1
        };

        await Pipeline
            .Extract(extractor)
            .Load(loader).WithProgress(progress)
            .RunAsync();

        Assert.NotEmpty(reports);
    }


    [Fact]
    public async Task WithProgress_on_transformer_forwards_reports()
    {
        var reports = new List<Report>();
        var progress = new SynchronousProgress<Report>(reports.Add);

        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3 });
        var mapper = new MappingTransformer<int, int>(x => x * 2)
        {
            ReportingInterval = 1
        };
        var loader = new TestLoader<int>(collectItems: true);

        await Pipeline
            .Extract(extractor)
            .Transform(mapper).WithProgress(progress)
            .Load(loader)
            .RunAsync();

        Assert.NotEmpty(reports);
    }


    // ---------------------------------------------------------------
    // Name
    // ---------------------------------------------------------------

    [Fact]
    public void Name_defaults_to_null()
    {
        var pipeline = Pipeline
            .Extract(new TestExtractor<int>(new List<int> { 1 }))
            .Load(new TestLoader<int>(collectItems: true));

        Assert.Null(pipeline.Name);
    }


    [Fact]
    public void WithName_sets_Name_property()
    {
        var pipeline = Pipeline
            .Extract(new TestExtractor<int>(new List<int> { 1 }))
            .Load(new TestLoader<int>(collectItems: true))
            .WithName("nightly-import");

        Assert.Equal("nightly-import", pipeline.Name);
    }


    // ---------------------------------------------------------------
    // One-shot enforcement
    // ---------------------------------------------------------------

    [Fact]
    public async Task RunAsync_second_call_throws_InvalidOperationException()
    {
        var pipeline = Pipeline
            .Extract(new TestExtractor<int>(new List<int> { 1, 2 }))
            .Load(new TestLoader<int>(collectItems: true));

        await pipeline.RunAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline.RunAsync());
    }


    [Fact]
    public async Task RunAsync_second_call_on_pipeline_with_load_progress_throws()
    {
        var reports = new List<Report>();
        var pipeline = Pipeline
            .Extract(new TestExtractor<int>(new List<int> { 1, 2 }))
            .Load(new TestLoader<int>(collectItems: true))
            .WithProgress(new SynchronousProgress<Report>(reports.Add));

        await pipeline.RunAsync();

        await Assert.ThrowsAsync<InvalidOperationException>
        (
            () => pipeline.RunAsync(CancellationToken.None)
        );
    }


    // ---------------------------------------------------------------
    // Cancellation
    // ---------------------------------------------------------------

    [Fact]
    public async Task RunAsync_forwards_cancellation_token_to_stages()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var extractor = new TestExtractor<int>(InfiniteSequence());
        var loader = new TestLoader<int>(collectItems: false);

        var pipeline = Pipeline
            .Extract(extractor)
            .Load(loader);

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            () => pipeline.RunAsync(cts.Token)
        );
    }


    private static IEnumerator<int> InfiniteSequence()
    {
        var i = 0;
        while (true)
        {
            yield return i++;
        }
    }


    // ---------------------------------------------------------------
    // Error semantics
    // ---------------------------------------------------------------

    [Fact]
    public async Task RunAsync_propagates_exception_from_transformer()
    {
        var boom = new InvalidOperationException("boom");
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3 });
        var thrower = new MappingTransformer<int, int>(_ => throw boom);
        var loader = new TestLoader<int>(collectItems: true);

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
    public async Task RunAsync_after_failure_stage_state_is_observable()
    {
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3, 4, 5 });
        var thrower = new MappingTransformer<int, int>
        (
            x => x == 3 ? throw new InvalidOperationException("boom") : x
        );
        var loader = new TestLoader<int>(collectItems: true);

        await Assert.ThrowsAsync<InvalidOperationException>
        (
            () => Pipeline
                .Extract(extractor)
                .Transform(thrower)
                .Load(loader)
                .RunAsync()
        );

        Assert.True(extractor.CurrentItemCount >= 3);
        Assert.True((loader.GetCollectedItems()?.Count ?? 0) < 5);
    }


    // ---------------------------------------------------------------
    // Non-progress code paths (ExtractStage / TransformStage / PipelineImpl)
    //
    // The TestKit doubles all implement the progress-capable interfaces, so
    // overload resolution on Pipeline.Extract/.Transform/.Load normally binds
    // to the progress-capable overloads. These tests cast to the no-progress
    // interfaces explicitly to exercise ExtractStage, TransformStage, and
    // PipelineImpl, which would otherwise be dead code at test time.
    // ---------------------------------------------------------------

    [Fact]
    public async Task RunAsync_non_progress_extract_and_load_delivers_all_items()
    {
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var extractor = new TestExtractor<int>(source);
        var loader = new TestLoader<int>(collectItems: true);

        await Pipeline
            .Extract((IExtractWithCancellationAsync<int>)extractor)
            .Load((ILoadWithCancellationAsync<int>)loader)
            .RunAsync();

        Assert.Equal(source, loader.GetCollectedItems());
    }


    [Fact]
    public async Task RunAsync_non_progress_extract_transform_load_transforms_items()
    {
        var source = new List<int> { 1, 2, 3 };
        var extractor = new TestExtractor<int>(source);
        var doubler = new MappingTransformer<int, int>(x => x * 2);
        var loader = new TestLoader<int>(collectItems: true);

        await Pipeline
            .Extract((IExtractWithCancellationAsync<int>)extractor)
            .Transform((ITransformWithCancellationAsync<int, int>)doubler)
            .Load((ILoadWithCancellationAsync<int>)loader)
            .RunAsync();

        Assert.Equal(new[] { 2, 4, 6 }, loader.GetCollectedItems());
    }


    [Fact]
    public void Non_progress_pipeline_Name_defaults_to_null()
    {
        var pipeline = Pipeline
            .Extract((IExtractWithCancellationAsync<int>)new TestExtractor<int>(new List<int> { 1 }))
            .Load((ILoadWithCancellationAsync<int>)new TestLoader<int>(collectItems: true));

        Assert.Null(pipeline.Name);
    }


    [Fact]
    public void Non_progress_pipeline_WithName_sets_Name_property()
    {
        var pipeline = Pipeline
            .Extract((IExtractWithCancellationAsync<int>)new TestExtractor<int>(new List<int> { 1 }))
            .Load((ILoadWithCancellationAsync<int>)new TestLoader<int>(collectItems: true))
            .WithName("nightly-import");

        Assert.Equal("nightly-import", pipeline.Name);
    }


    [Fact]
    public void Non_progress_pipeline_WithName_when_name_is_null_throws()
    {
        var pipeline = Pipeline
            .Extract((IExtractWithCancellationAsync<int>)new TestExtractor<int>(new List<int> { 1 }))
            .Load((ILoadWithCancellationAsync<int>)new TestLoader<int>(collectItems: true));

        Assert.Throws<ArgumentNullException>(() => pipeline.WithName(null!));
    }


    [Fact]
    public async Task Non_progress_pipeline_RunAsync_second_call_throws()
    {
        var pipeline = Pipeline
            .Extract((IExtractWithCancellationAsync<int>)new TestExtractor<int>(new List<int> { 1 }))
            .Load((ILoadWithCancellationAsync<int>)new TestLoader<int>(collectItems: true));

        await pipeline.RunAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline.RunAsync());
    }


    [Fact]
    public void Non_progress_Transform_when_transformer_is_null_throws()
    {
        var stage = Pipeline.Extract
        (
            (IExtractWithCancellationAsync<int>)new TestExtractor<int>(new List<int> { 1 })
        );

        Assert.Throws<ArgumentNullException>
        (
            () => stage.Transform((ITransformWithCancellationAsync<int, int>)null!)
        );
    }


    [Fact]
    public void Non_progress_Load_after_Transform_when_loader_is_null_throws()
    {
        var stage = Pipeline
            .Extract((IExtractWithCancellationAsync<int>)new TestExtractor<int>(new List<int> { 1 }))
            .Transform((ITransformWithCancellationAsync<int, int>)new MappingTransformer<int, int>(x => x));

        Assert.Throws<ArgumentNullException>
        (
            () => stage.Load((ILoadWithCancellationAsync<int>)null!)
        );
    }


    [Fact]
    public void Non_progress_Transform_on_TransformStage_when_transformer_is_null_throws()
    {
        var stage = Pipeline
            .Extract((IExtractWithCancellationAsync<int>)new TestExtractor<int>(new List<int> { 1 }))
            .Transform((ITransformWithCancellationAsync<int, int>)new MappingTransformer<int, int>(x => x));

        Assert.Throws<ArgumentNullException>
        (
            () => stage.Transform((ITransformWithCancellationAsync<int, int>)null!)
        );
    }
}
