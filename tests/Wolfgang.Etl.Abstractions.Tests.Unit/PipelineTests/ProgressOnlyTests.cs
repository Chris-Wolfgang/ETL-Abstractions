using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;
using Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests.TestDoubles;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests;

/// <summary>
/// End-to-end tests using real classes that implement the with-progress interfaces but
/// NOT the with-cancellation interfaces.
/// </summary>
public class ProgressOnlyTests
{
    [Fact]
    public async Task Extract_Load_without_WithProgress_runs_no_progress_path()
    {
        var extractor = new ProgressOnlyExtractor<int, string>(new[] { 1, 2, 3 }, reportValue: "e");
        var loader = new ProgressOnlyLoader<int, string>(reportValue: "l");

        await Pipeline
            .Extract(extractor)
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 1, 2, 3 }, loader.Loaded);
        Assert.False(extractor.ProgressOverloadWasCalled);
        Assert.False(loader.ProgressOverloadWasCalled);
    }


    [Fact]
    public async Task Extract_WithProgress_forwards_sink_to_progress_only_extractor()
    {
        var reports = new List<string>();
        var progress = new SynchronousProgress<string>(reports.Add);
        var extractor = new ProgressOnlyExtractor<int, string>(new[] { 1, 2, 3 }, reportValue: "e");
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor).WithProgress(progress)
            .Load(loader)
            .RunAsync();

        Assert.True(extractor.ProgressOverloadWasCalled);
        Assert.Same(progress, extractor.LastReceivedProgress);
        Assert.Equal(new[] { "e", "e", "e" }, reports);
    }


    [Fact]
    public async Task Load_WithProgress_forwards_sink_to_progress_only_loader()
    {
        var reports = new List<string>();
        var progress = new SynchronousProgress<string>(reports.Add);
        var extractor = new BareExtractor<int>(new[] { 1, 2, 3 });
        var loader = new ProgressOnlyLoader<int, string>(reportValue: "l");

        await Pipeline
            .Extract(extractor)
            .Load(loader).WithProgress(progress)
            .RunAsync();

        Assert.True(loader.ProgressOverloadWasCalled);
        Assert.Same(progress, loader.LastReceivedProgress);
        Assert.Equal(new[] { "l", "l", "l" }, reports);
    }


    [Fact]
    public async Task Transform_WithProgress_forwards_sink_to_progress_only_transformer()
    {
        var reports = new List<string>();
        var progress = new SynchronousProgress<string>(reports.Add);
        var extractor = new BareExtractor<int>(new[] { 1, 2, 3 });
        var transformer = new ProgressOnlyTransformer<int, int, string>(x => x * 2, reportValue: "t");
        var loader = new BareLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Transform(transformer).WithProgress(progress)
            .Load(loader)
            .RunAsync();

        Assert.True(transformer.ProgressOverloadWasCalled);
        Assert.Same(progress, transformer.LastReceivedProgress);
        Assert.Equal(new[] { "t", "t", "t" }, reports);
        Assert.Equal(new[] { 2, 4, 6 }, loader.Loaded);
    }


    [Fact]
    public async Task RunAsync_with_CancellationToken_does_not_forward_to_progress_only_stages()
    {
        var extractor = new ProgressOnlyExtractor<int, string>(new[] { 1, 2 }, reportValue: "e");
        var loader = new ProgressOnlyLoader<int, string>(reportValue: "l");

        using var cts = new CancellationTokenSource();
        await Pipeline
            .Extract(extractor)
            .Load(loader)
            .RunAsync(cts.Token);

        Assert.Equal(new[] { 1, 2 }, loader.Loaded);
    }
}
