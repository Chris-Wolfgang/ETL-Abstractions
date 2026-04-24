using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests.TestDoubles;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests;

/// <summary>
/// End-to-end tests using real classes that implement the with-cancellation interfaces but
/// NOT the with-progress interfaces.
/// </summary>
public class CancelOnlyTests
{
    [Fact]
    public async Task Extract_Load_runs_through_real_cancel_only_classes()
    {
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var extractor = new CancelOnlyExtractor<int>(source);
        var loader = new CancelOnlyLoader<int>();

        await Pipeline
            .Extract(extractor)
            .Load(loader)
            .RunAsync();

        Assert.Equal(source, loader.Loaded);
        Assert.True(extractor.TokenOverloadWasCalled);
        Assert.True(loader.TokenOverloadWasCalled);
    }


    [Fact]
    public async Task RunAsync_forwards_cancellation_token_to_cancel_only_stages()
    {
        var extractor = new CancelOnlyExtractor<int>(new[] { 1, 2 });
        var transformer = new CancelOnlyTransformer<int, int>(x => x * 2);
        var loader = new CancelOnlyLoader<int>();

        using var cts = new CancellationTokenSource();
        await Pipeline
            .Extract(extractor)
            .Transform(transformer)
            .Load(loader)
            .RunAsync(cts.Token);

        Assert.Equal(cts.Token, extractor.LastReceivedToken);
        Assert.Equal(cts.Token, transformer.LastReceivedToken);
        Assert.Equal(cts.Token, loader.LastReceivedToken);
    }


    [Fact]
    public async Task RunAsync_with_pre_cancelled_token_throws_OperationCanceledException()
    {
        var extractor = new CancelOnlyExtractor<int>(new[] { 1, 2, 3 });
        var loader = new CancelOnlyLoader<int>();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            () => Pipeline
                .Extract(extractor)
                .Load(loader)
                .RunAsync(cts.Token)
        );

        Assert.Empty(loader.Loaded);
    }


    [Fact]
    public async Task Extract_Transform_Load_runs_through_real_cancel_only_classes()
    {
        var extractor = new CancelOnlyExtractor<int>(new[] { 1, 2, 3 });
        var transformer = new CancelOnlyTransformer<int, string>
        (
            x => x.ToString(System.Globalization.CultureInfo.InvariantCulture)
        );
        var loader = new CancelOnlyLoader<string>();

        await Pipeline
            .Extract(extractor)
            .Transform(transformer)
            .Load(loader)
            .RunAsync();

        Assert.Equal(new[] { "1", "2", "3" }, loader.Loaded);
        Assert.True(transformer.TokenOverloadWasCalled);
    }
}
