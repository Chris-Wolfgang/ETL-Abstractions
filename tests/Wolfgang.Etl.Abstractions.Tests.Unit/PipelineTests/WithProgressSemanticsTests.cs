using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;
using Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests.TestDoubles;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests;

/// <summary>
/// Verifies the semantics of <c>WithProgress</c> on the various stage types:
/// - Extract/Transform builder stages are immutable: calling <c>WithProgress</c> on one builder
///   twice produces two independent downstream stages, and using a builder for both a
///   with-progress and a no-progress branch does not cause aliasing.
/// - <c>IPipelineWithLoadProgress.WithProgress</c> is one-shot: a second call throws
///   <see cref="InvalidOperationException"/>.
/// </summary>
public class WithProgressSemanticsTests
{
    // ---------------------------------------------------------------
    // Builder immutability (Extract / Transform stages)
    // ---------------------------------------------------------------

    [Fact]
    public async Task Extract_WithProgress_twice_produces_independent_stages()
    {
        var reportsA = new List<string>();
        var reportsB = new List<string>();
        var pA = new SynchronousProgress<string>(reportsA.Add);
        var pB = new SynchronousProgress<string>(reportsB.Add);

        var builder = Pipeline.Extract
        (
            new ProgressOnlyExtractor<int, string>(new[] { 1, 2 }, "e")
        );

        var loaderA = new BareLoader<int>();
        var loaderB = new BareLoader<int>();

        await builder.WithProgress(pA).Load(loaderA).RunAsync();
        await builder.WithProgress(pB).Load(loaderB).RunAsync();

        Assert.Equal(new[] { "e", "e" }, reportsA);
        Assert.Equal(new[] { "e", "e" }, reportsB);
    }


    [Fact]
    public async Task Extract_builder_WithProgress_does_not_leak_into_no_progress_branch()
    {
        var reports = new List<string>();
        var progress = new SynchronousProgress<string>(reports.Add);

        var extractor = new ProgressOnlyExtractor<int, string>(new[] { 1, 2 }, "e");
        var builder = Pipeline.Extract(extractor);

        // Branch A: with progress
        var loaderA = new BareLoader<int>();
        await builder.WithProgress(progress).Load(loaderA).RunAsync();

        // Branch B: from the SAME builder, without WithProgress
        // Construct a fresh extractor so ParameterlessOverloadWasCalled is meaningful for B.
        var extractorB = new ProgressOnlyExtractor<int, string>(new[] { 1, 2 }, "e");
        var builderB = Pipeline.Extract(extractorB);
        var loaderB = new BareLoader<int>();
        await builderB.Load(loaderB).RunAsync();

        Assert.True(extractorB.ParameterlessOverloadWasCalled);
        Assert.False(extractorB.ProgressOverloadWasCalled);
    }


    [Fact]
    public async Task Transform_WithProgress_twice_produces_independent_stages()
    {
        var reportsA = new List<string>();
        var reportsB = new List<string>();
        var pA = new SynchronousProgress<string>(reportsA.Add);
        var pB = new SynchronousProgress<string>(reportsB.Add);

        // Need to rebuild the pipeline per run because a pipeline is one-shot.
        async Task RunWithAsync(IProgress<string> progress, List<int> loaded)
        {
            var extractor = new BareExtractor<int>(new[] { 1, 2 });
            var transformer = new ProgressOnlyTransformer<int, int, string>(x => x + 10, "t");
            var loader = new BareLoader<int>();
            await Pipeline
                .Extract(extractor)
                .Transform(transformer).WithProgress(progress)
                .Load(loader)
                .RunAsync();
            loaded.AddRange(loader.Loaded);
        }

        var resultsA = new List<int>();
        var resultsB = new List<int>();
        await RunWithAsync(pA, resultsA);
        await RunWithAsync(pB, resultsB);

        Assert.Equal(new[] { 11, 12 }, resultsA);
        Assert.Equal(new[] { 11, 12 }, resultsB);
        Assert.Equal(new[] { "t", "t" }, reportsA);
        Assert.Equal(new[] { "t", "t" }, reportsB);
    }


    // ---------------------------------------------------------------
    // Pipeline (IPipelineWithLoadProgress) is one-shot on WithProgress
    // ---------------------------------------------------------------

    [Fact]
    public void Pipeline_WithProgress_called_twice_throws_InvalidOperationException()
    {
        var progressA = new SynchronousProgress<string>(_ => { });
        var progressB = new SynchronousProgress<string>(_ => { });

        var pipeline = Pipeline
            .Extract(new BareExtractor<int>(new[] { 1 }))
            .Load(new ProgressOnlyLoader<int, string>("l"));

        pipeline.WithProgress(progressA);

        Assert.Throws<InvalidOperationException>(() => pipeline.WithProgress(progressB));
    }


    // ---------------------------------------------------------------
    // Async-contract normalization: sync throws become Task faults.
    // Covers the intent of #3/#4 from the code review.
    // ---------------------------------------------------------------

    [Fact]
    public async Task RunAsync_second_call_fault_is_delivered_through_returned_Task()
    {
        var pipeline = Pipeline
            .Extract(new BareExtractor<int>(new[] { 1 }))
            .Load(new BareLoader<int>());

        await pipeline.RunAsync();

        // Second call: the one-shot flag fires. The exception must surface on the returned Task,
        // NOT synchronously at the call site.
        var task = pipeline.RunAsync();
        Assert.True(task.IsFaulted);
        Assert.IsType<InvalidOperationException>(task.Exception?.InnerException);
    }


    [Fact]
    public async Task RunAsync_second_call_on_load_progress_pipeline_fault_is_delivered_through_returned_Task()
    {
        var pipeline = Pipeline
            .Extract(new BareExtractor<int>(new[] { 1 }))
            .Load(new ProgressOnlyLoader<int, string>("l"));

        await pipeline.RunAsync();

        var task = pipeline.RunAsync();
        Assert.True(task.IsFaulted);
        Assert.IsType<InvalidOperationException>(task.Exception?.InnerException);
    }
}
