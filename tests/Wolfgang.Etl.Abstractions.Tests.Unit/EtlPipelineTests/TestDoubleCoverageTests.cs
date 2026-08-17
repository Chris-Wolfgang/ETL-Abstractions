using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.EtlPipelineTests;

/// <summary>
/// Directly exercises members of the shared <c>EtlPipeline</c> test doubles that the pipeline binding
/// path does not reach: the component-level progress report and the parameterless transform overload.
/// </summary>
public class TestDoubleCoverageTests
{
    [Fact]
    public async Task SeededExtractor_reports_component_progress_when_run_standalone()
    {
        var sut = new SeededExtractor<int>(new[] { 1, 2, 3 });

        await foreach (var _ in sut.ExtractAsync(new Progress<EtlProgress>()))
        {
        }

        Assert.Equal(3, sut.CurrentItemCount);
    }


    [Fact]
    public async Task CollectingLoader_reports_component_progress_when_run_standalone()
    {
        var sut = new CollectingLoader<int>();

        await sut.LoadAsync(new[] { 1, 2, 3 }.ToAsyncEnumerable(), new Progress<EtlProgress>());

        Assert.Equal(new[] { 1, 2, 3 }, sut.Loaded);
        Assert.Equal(3, sut.CurrentItemCount);
    }


    [Fact]
    public async Task TokenRecordingTransformer_parameterless_overload_forwards_no_cancellation()
    {
        var sut = new TokenRecordingTransformer<int>();

        var result = await sut.TransformAsync(new[] { 1, 2, 3 }.ToAsyncEnumerable()).ToListAsync();

        Assert.Equal(new[] { 1, 2, 3 }, result);
        Assert.Equal(CancellationToken.None, sut.LastToken);
    }
}
