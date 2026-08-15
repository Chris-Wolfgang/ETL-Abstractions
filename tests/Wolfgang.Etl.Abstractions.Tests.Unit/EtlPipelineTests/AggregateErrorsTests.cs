using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.EtlPipelineTests;

/// <summary>
/// Covers #335: <see cref="EtlPipelineProgress.ErrorItemCount"/> aggregates the per-item error counts
/// of every stage that reports them (source, transformers, sink) via <see cref="IReportsItemErrors"/>,
/// not just the extractor.
/// </summary>
public class AggregateErrorsTests
{
    [Fact]
    public void All_three_base_classes_implement_IReportsItemErrors()
    {
        Assert.IsAssignableFrom<IReportsItemErrors>(new ErroringExtractor(good: 0, errors: 0));
        Assert.IsAssignableFrom<IReportsItemErrors>(new ErroringTransformer(errors: 0));
        Assert.IsAssignableFrom<IReportsItemErrors>(new ErroringLoader(errors: 0));
    }


    [Fact]
    public async Task ErrorItemCount_sums_extractor_transformer_and_loader_errors()
    {
        var reports = new List<EtlPipelineProgress>();
        var extractor = new ErroringExtractor(good: 3, errors: 2);
        var transformer = new ErroringTransformer(errors: 4);
        var loader = new ErroringLoader(errors: 1);

        await EtlPipeline
            .Create()
            .From(extractor)
            .Through(transformer)
            .To(loader)
            .RunAsync(new SyncProgress(reports.Add));

        var final = reports[^1];
        Assert.Equal(3, final.ExtractedItemCount);
        Assert.Equal(3, final.LoadedItemCount);
        Assert.Equal(7, final.ErrorItemCount);   // 2 (extract) + 4 (transform) + 1 (load)
    }


    [Fact]
    public async Task ErrorItemCount_counts_only_stages_that_report_errors()
    {
        // A delegate transform stage does not implement IReportsItemErrors, so it contributes nothing.
        var reports = new List<EtlPipelineProgress>();
        var extractor = new ErroringExtractor(good: 2, errors: 5);
        var loader = new ErroringLoader(errors: 3);

        await EtlPipeline
            .Create()
            .From(extractor)
            .Through(s => s)
            .To(loader)
            .RunAsync(new SyncProgress(reports.Add));

        Assert.Equal(8, reports[^1].ErrorItemCount);   // 5 (extract) + 3 (load); delegate adds 0
    }


    [Fact]
    public async Task ErrorItemCount_includes_a_plain_ITransformAsync_stage_that_reports_errors()
    {
        // Binds the non-cancellation Through overload (a bare ITransformAsync, not a TransformerBase).
        var reports = new List<EtlPipelineProgress>();
        var extractor = new ErroringExtractor(good: 2, errors: 1);
        var transform = new ReportingPassThroughTransform(errors: 5);
        var loader = new ErroringLoader(errors: 0);

        await EtlPipeline
            .Create()
            .From(extractor)
            .Through(transform)
            .To(loader)
            .RunAsync(new SyncProgress(reports.Add));

        Assert.Equal(6, reports[^1].ErrorItemCount);   // 1 (extract) + 5 (plain transform)
    }


    [Fact]
    public async Task ErrorItemCount_is_zero_when_no_stage_reports_errors()
    {
        var reports = new List<EtlPipelineProgress>();
        var loader = new ErroringLoader(errors: 0);

        await EtlPipeline
            .Create()
            .From(AsyncSource(1, 2, 3))          // raw IAsyncEnumerable — not an IReportsItemErrors stage
            .To(loader)
            .RunAsync(new SyncProgress(reports.Add));

        Assert.Equal(0, reports[^1].ErrorItemCount);
    }


    // ---------- helpers ----------

    private static async IAsyncEnumerable<int> AsyncSource(params int[] items)
    {
        foreach (var item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }


    // A bare ITransformAsync (not a TransformerBase) that still reports an error count — exercises the
    // non-cancellation Through overload's error-reader registration.
    private sealed class ReportingPassThroughTransform : ITransformAsync<int, int>, IReportsItemErrors
    {
        public ReportingPassThroughTransform(int errors) => CurrentErrorItemCount = errors;

        public int CurrentErrorItemCount { get; }

        public IAsyncEnumerable<int> TransformAsync(IAsyncEnumerable<int> items) => items;
    }


    private sealed class SyncProgress : IProgress<EtlPipelineProgress>
    {
        private readonly Action<EtlPipelineProgress> _report;

        public SyncProgress(Action<EtlPipelineProgress> report) => _report = report;

        public void Report(EtlPipelineProgress value) => _report(value);
    }


    // ---------- error-reporting doubles (route real #84 errors through HandleItemError) ----------

    private sealed class ErroringExtractor : ExtractorBase<int, EtlProgress>
    {
        private readonly int _good;
        private readonly int _errors;

        public ErroringExtractor(int good, int errors)
        {
            _good = good;
            _errors = errors;
        }

        protected override ItemErrorAction OnItemError(ItemErrorContext context) => ItemErrorAction.Skip;

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            for (var i = 0; i < _good; i++)
            {
                await Task.Yield();
                IncrementCurrentItemCount();
                yield return i;
            }

            for (var e = 0; e < _errors; e++)
            {
                // Route a synthetic failure through the base #84 hook: with OnItemError => Skip,
                // HandleItemError increments CurrentErrorItemCount and returns without rethrowing.
                try
                {
                    throw new InvalidOperationException("bad item");
                }
                catch (InvalidOperationException ex)
                {
                    HandleItemError(new ItemErrorContext(e, ex));
                }
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    private sealed class ErroringTransformer : TransformerBase<int, int, EtlProgress>
    {
        private readonly int _errors;

        public ErroringTransformer(int errors) => _errors = errors;

        protected override ItemErrorAction OnItemError(ItemErrorContext context) => ItemErrorAction.Skip;

        protected override async IAsyncEnumerable<int> TransformWorkerAsync(
            IAsyncEnumerable<int> items, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var item in items.WithCancellation(token))
            {
                IncrementCurrentItemCount();
                yield return item;
            }

            for (var e = 0; e < _errors; e++)
            {
                // Route a synthetic failure through the base #84 hook: with OnItemError => Skip,
                // HandleItemError increments CurrentErrorItemCount and returns without rethrowing.
                try
                {
                    throw new InvalidOperationException("bad item");
                }
                catch (InvalidOperationException ex)
                {
                    HandleItemError(new ItemErrorContext(e, ex));
                }
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    private sealed class ErroringLoader : LoaderBase<int, EtlProgress>
    {
        private readonly int _errors;

        public ErroringLoader(int errors) => _errors = errors;

        protected override ItemErrorAction OnItemError(ItemErrorContext context) => ItemErrorAction.Skip;

        protected override async Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
        {
            await foreach (var _ in items.WithCancellation(token))
            {
                IncrementCurrentItemCount();
            }

            for (var e = 0; e < _errors; e++)
            {
                // Route a synthetic failure through the base #84 hook: with OnItemError => Skip,
                // HandleItemError increments CurrentErrorItemCount and returns without rethrowing.
                try
                {
                    throw new InvalidOperationException("bad item");
                }
                catch (InvalidOperationException ex)
                {
                    HandleItemError(new ItemErrorContext(e, ex));
                }
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }
}
