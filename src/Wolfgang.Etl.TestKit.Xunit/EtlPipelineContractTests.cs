using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests for composing a source and a sink into the
/// <c>Wolfgang.Etl.Abstractions</c> 0.16 <see cref="EtlPipeline"/>
/// (<c>EtlPipeline.Create().From(...).To(...).RunAsync()</c>) and for the
/// <see cref="EtlPipelineProgress"/> it reports.
/// </summary>
/// <typeparam name="TItem">The item type flowing through the pipeline.</typeparam>
/// <typeparam name="TProgress">The sink loader's progress report type.</typeparam>
/// <remarks>
/// <para>
/// Inherit from this class to verify that your source composes with a loader sink into an
/// <see cref="EtlPipeline"/> that runs end-to-end and delivers every item, and that its
/// <see cref="EtlPipelineProgress"/> counts the records pulled from the source
/// (<see cref="EtlPipelineProgress.ExtractedItemCount"/>) and delivered to the sink
/// (<see cref="EtlPipelineProgress.LoadedItemCount"/>).
/// </para>
/// <para>
/// Implement <see cref="CreateSourceItems"/> (the data the source streams), <see cref="CreateSink"/>
/// (the loader), and <see cref="GetLoadedItems"/> (read what the harness-composed <see cref="Sink"/>
/// received, or <see langword="null"/> if the loader does not expose it). The <see cref="Sink"/>
/// property — rather than a parameter — carries the composed loader so the override needs no
/// null-argument validation.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyPipelineContractTests
///     : EtlPipelineContractTests&lt;MyRecord, MyProgress&gt;
/// {
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateSourceItems() => new[] { ... };
///     protected override LoaderBase&lt;MyRecord, MyProgress&gt; CreateSink() => new MyLoader();
///     protected override IReadOnlyList&lt;MyRecord&gt;? GetLoadedItems() => ((MyLoader)Sink).Written;
/// }
/// </code>
/// </example>
public abstract class EtlPipelineContractTests<TItem, TProgress>
    where TItem : notnull
    where TProgress : notnull
{
    /// <summary>
    /// The loader sink the harness composed into the pipeline for the current test. Read it from
    /// <see cref="GetLoadedItems"/> to inspect what was delivered.
    /// </summary>
    protected LoaderBase<TItem, TProgress> Sink { get; private set; } = default!;

    /// <summary>The items the source streams into the pipeline. Must return at least one item.</summary>
    protected abstract IReadOnlyList<TItem> CreateSourceItems();

    /// <summary>Creates the loader sink under test.</summary>
    protected abstract LoaderBase<TItem, TProgress> CreateSink();

    /// <summary>
    /// Returns the items the harness-composed <see cref="Sink"/> received, or <see langword="null"/>
    /// if the loader does not expose them (then the delivery assertion is skipped).
    /// </summary>
    protected abstract IReadOnlyList<TItem>? GetLoadedItems();



    /// <summary>
    /// Verifies that <c>EtlPipeline.Create().From(source).To(sink).RunAsync()</c> runs end-to-end
    /// and the sink receives every source item, in order.
    /// </summary>
    [Fact]
    public async Task Pipeline_delivers_all_source_items_to_the_sink_Async()
    {
        var source = CreateSourceItems();
        Assert.True(source.Count >= 1, "CreateSourceItems() must return at least one item.");

        await RunPipelineAsync(source, progress: null).ConfigureAwait(false);

        var loaded = GetLoadedItems();
        if (loaded is not null)
        {
            Assert.Equal(source, loaded);
        }
    }

    /// <summary>
    /// Verifies that the pipeline's <see cref="EtlPipelineProgress"/> reports every source record as
    /// extracted and delivered, with no error-discarded records on a clean run.
    /// </summary>
    [Fact]
    public async Task Pipeline_progress_counts_all_records_extracted_and_loaded_Async()
    {
        var source = CreateSourceItems();
        Assert.True(source.Count >= 1, "CreateSourceItems() must return at least one item.");

        var capture = new ProgressCapture<EtlPipelineProgress>();

        await RunPipelineAsync(source, capture).ConfigureAwait(false);

        var final = capture.FinalReport;
        Assert.NotNull(final);
        Assert.Equal(source.Count, final!.ExtractedItemCount);
        Assert.Equal(source.Count, final.LoadedItemCount);
        Assert.Equal(0, final.ErrorItemCount);
    }

    private Task RunPipelineAsync(IReadOnlyList<TItem> source, IProgress<EtlPipelineProgress>? progress)
    {
        Sink = CreateSink();

        return EtlPipeline
            .Create()
            .From(source.ToAsyncEnumerable())
            .To(Sink)
            .RunAsync(progress);
    }
}
