using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;
using Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests.TestDoubles;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests;

/// <summary>
/// End-to-end tests using real classes that implement the full progress-and-cancellation
/// interfaces.
/// </summary>
public class FullTests
{
    [Fact]
    public async Task Extract_Load_without_WithProgress_uses_no_progress_overload()
    {
        var extractor = new FullExtractor<int, string>(new[] { 1, 2, 3 }, reportValue: "e");
        var loader = new FullLoader<int, string>(reportValue: "l");

        await Pipeline
            .Extract(extractor)
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { 1, 2, 3 }, loader.Loaded);
        Assert.False(extractor.FullOverloadWasCalled);  // no WithProgress => pipeline calls ExtractAsync(token)
        Assert.False(loader.FullOverloadWasCalled);
    }


    [Fact]
    public async Task Extract_WithProgress_and_Load_WithProgress_forward_sinks_and_tokens()
    {
        var extractReports = new List<string>();
        var loadReports = new List<string>();
        var extractProgress = new SynchronousProgress<string>(extractReports.Add);
        var loadProgress = new SynchronousProgress<string>(loadReports.Add);

        var extractor = new FullExtractor<int, string>(new[] { 1, 2, 3 }, reportValue: "e");
        var loader = new FullLoader<int, string>(reportValue: "l");

        using var cts = new CancellationTokenSource();
        await Pipeline
            .Extract(extractor).WithProgress(extractProgress)
            .Load(loader).WithProgress(loadProgress)
            .RunAsync(cts.Token);

        Assert.True(extractor.FullOverloadWasCalled);
        Assert.Same(extractProgress, extractor.LastReceivedProgress);
        Assert.Equal(cts.Token, extractor.LastReceivedToken);

        Assert.True(loader.FullOverloadWasCalled);
        Assert.Same(loadProgress, loader.LastReceivedProgress);
        Assert.Equal(cts.Token, loader.LastReceivedToken);

        Assert.Equal(new[] { "e", "e", "e" }, extractReports);
        Assert.Equal(new[] { "l", "l", "l" }, loadReports);
    }


    [Fact]
    public async Task Full_transformer_WithProgress_forwards_sink_and_token()
    {
        var reports = new List<string>();
        var progress = new SynchronousProgress<string>(reports.Add);

        var extractor = new FullExtractor<int, string>(new[] { 1, 2, 3 }, reportValue: "e");
        var transformer = new FullTransformer<int, int, string>(x => x + 100, reportValue: "t");
        var loader = new FullLoader<int, string>(reportValue: "l");

        using var cts = new CancellationTokenSource();
        await Pipeline
            .Extract(extractor)
            .Transform(transformer).WithProgress(progress)
            .Load(loader)
            .RunAsync(cts.Token);

        Assert.True(transformer.FullOverloadWasCalled);
        Assert.Same(progress, transformer.LastReceivedProgress);
        Assert.Equal(cts.Token, transformer.LastReceivedToken);
        Assert.Equal(new[] { 101, 102, 103 }, loader.Loaded);
    }


    [Fact]
    public async Task RunAsync_with_pre_cancelled_token_cancels_full_pipeline()
    {
        var extractor = new FullExtractor<int, string>(new[] { 1, 2, 3 }, reportValue: "e");
        var loader = new FullLoader<int, string>(reportValue: "l");

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            () => Pipeline
                .Extract(extractor)
                .Load(loader)
                .RunAsync(cts.Token)
        );
    }
}
