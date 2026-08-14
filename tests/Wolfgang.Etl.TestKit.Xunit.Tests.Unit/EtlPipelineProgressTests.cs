using System;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Kit self-tests for the two <see cref="EtlPipeline"/> surfaces the contract base cannot reach with
/// a plain source: the error count the pipeline aggregates when a stage skips a faulty item, and the
/// <c>DisposingOwned</c> sink wrapper that disposes pipeline-owned resources after the run.
/// </summary>
public class EtlPipelineProgressTests
{
    [Fact]
    public async Task Pipeline_progress_ErrorItemCount_reflects_a_skipped_faulty_item_Async()
    {
        var source = new FaultyExtractor<int>(new[] { 0, 1, 2, 3, 4 })
            .ThrowAt(2, new FormatException("bad row"))
            .SkipErrors();
        var sink = new TestLoader<int>(collectItems: true);
        var capture = new ProgressCapture<EtlPipelineProgress>();

        await EtlPipeline
            .Create()
            .From(source)
            .To(sink)
            .RunAsync(capture)
            ;

        var final = capture.FinalReport;
        Assert.NotNull(final);
        Assert.Equal(1, final!.ErrorItemCount);
        Assert.Equal(4, final.LoadedItemCount);
    }



    [Fact]
    public async Task DisposingOwned_disposes_pipeline_owned_resources_after_the_run_Async()
    {
        var owned = new TrackingDisposable();
        var sink = new TestLoader<int>(collectItems: false);

        await EtlPipeline
            .Create()
            .From(new[] { 1, 2, 3 }.ToAsyncEnumerable())
            .To(sink)
            .DisposingOwned(owned)
            .RunAsync()
            ;

        Assert.True(owned.WasDisposed);
    }



    private sealed class TrackingDisposable : IDisposable
    {
        public bool WasDisposed { get; private set; }



        public void Dispose() => WasDisposed = true;
    }
}
