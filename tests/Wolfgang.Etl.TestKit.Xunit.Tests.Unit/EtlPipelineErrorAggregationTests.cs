using System;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Kit self-tests for the Abstractions 0.20 <c>IReportsItemErrors</c> aggregation: an
/// <see cref="EtlPipeline"/> sums each stage's <c>CurrentErrorItemCount</c> into
/// <see cref="EtlPipelineProgress.ErrorItemCount"/>, so a skipped fault in any stage — extractor,
/// transformer, or loader — is reflected in the pipeline's progress.
/// </summary>
public class EtlPipelineErrorAggregationTests
{
    [Fact]
    public async Task Pipeline_ErrorItemCount_sums_errors_across_extractor_and_loader_Async()
    {
        var source = new FaultyExtractor<int>(new[] { 0, 1, 2, 3, 4 })
            .ThrowAt(2, new FormatException("extractor fault"))
            .SkipErrors();
        var sink = new FaultyLoader<int>(collectItems: true)
            .ThrowAt(1, new FormatException("loader fault"))
            .SkipErrors();
        var capture = new ProgressCapture<EtlPipelineProgress>();

        await EtlPipeline
            .Create()
            .From(source)
            .To(sink)
            .RunAsync(capture)
            ;

        var final = capture.FinalReport;
        Assert.NotNull(final);
        // One error skipped in the extractor + one in the loader.
        Assert.Equal(2, final.ErrorItemCount);
    }



    [Fact]
    public async Task Pipeline_ErrorItemCount_sums_errors_across_all_three_stages_Async()
    {
        var source = new FaultyExtractor<int>(new[] { 0, 1, 2, 3, 4 })
            .ThrowAt(2, new FormatException("extractor fault"))
            .SkipErrors();
        var transformer = new FaultyTransformer<int>()
            .ThrowAt(1, new FormatException("transformer fault"))
            .SkipErrors();
        var sink = new FaultyLoader<int>(collectItems: true)
            .ThrowAt(1, new FormatException("loader fault"))
            .SkipErrors();
        var capture = new ProgressCapture<EtlPipelineProgress>();

        await EtlPipeline
            .Create()
            .From(source)
            .Through(transformer)
            .To(sink)
            .RunAsync(capture)
            ;

        var final = capture.FinalReport;
        Assert.NotNull(final);
        // One error skipped in each of the extractor, transformer, and loader.
        Assert.Equal(3, final.ErrorItemCount);
    }
}
