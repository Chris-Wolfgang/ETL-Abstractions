using System.Runtime.CompilerServices;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// Covers the #344 convenience base classes — <see cref="ExtractorBase{TSource}"/>,
/// <see cref="LoaderBase{TDestination}"/>, and <see cref="TransformerBase{TSource, TDestination}"/> —
/// which fix the progress type to the built-in <see cref="Report"/> and supply a default
/// <c>CreateProgressReport()</c>, so a derived component only implements its worker method.
/// </summary>
public class ConvenienceBaseTests
{
    [Fact]
    public async Task Extractor_convenience_base_yields_items_and_reports_the_built_in_Report()
    {
        var sut = new ConvenienceExtractor();

        var items = await Drain(sut.ExtractAsync(CancellationToken.None));
        var report = sut.Report();

        Assert.Equal(new[] { 1, 2, 3 }, items);
        Assert.Equal(3, report.CurrentItemCount);
        Assert.NotNull(report.StartedAt);              // captured once the first item was processed
        Assert.True(report.Elapsed >= TimeSpan.Zero);
    }


    [Fact]
    public async Task Loader_convenience_base_loads_items_and_reports_the_built_in_Report()
    {
        var sut = new ConvenienceLoader();

        await sut.LoadAsync(AsyncSource(1, 2, 3), CancellationToken.None);
        var report = sut.Report();

        Assert.Equal(new[] { 1, 2, 3 }, sut.Loaded);
        Assert.Equal(3, report.CurrentItemCount);
    }


    [Fact]
    public async Task Transformer_convenience_base_transforms_items_and_reports_the_built_in_Report()
    {
        var sut = new ConvenienceTransformer();

        var items = await Drain(sut.TransformAsync(AsyncSource(1, 2, 3), CancellationToken.None));
        var report = sut.Report();

        Assert.Equal(new[] { 10, 20, 30 }, items);
        Assert.Equal(3, report.CurrentItemCount);
    }


    [Fact]
    public async Task Convenience_base_CreateProgressReport_can_still_be_overridden()
    {
        // The default CreateProgressReport is not sealed — a component can enrich it (here: a known total).
        var sut = new TotalAwareExtractor(total: 10);

        await Drain(sut.ExtractAsync(CancellationToken.None));

        Assert.Equal(10, sut.Report().TotalItemCount);
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


    private static async Task<List<int>> Drain(IAsyncEnumerable<int> source)
    {
        var result = new List<int>();
        await foreach (var item in source.ConfigureAwait(false))
        {
            result.Add(item);
        }

        return result;
    }


    // ---------- doubles (each implements ONLY its worker — no progress record, no CreateProgressReport) ----------

    private sealed class ConvenienceExtractor : ExtractorBase<int>
    {
        public Report Report() => CreateProgressReport();

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            foreach (var i in new[] { 1, 2, 3 })
            {
                await Task.Yield();
                IncrementCurrentItemCount();
                yield return i;
            }
        }
    }


    private sealed class ConvenienceLoader : LoaderBase<int>
    {
        public List<int> Loaded { get; } = new();

        public Report Report() => CreateProgressReport();

        protected override async Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
        {
            await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
            {
                Loaded.Add(item);
                IncrementCurrentItemCount();
            }
        }
    }


    private sealed class ConvenienceTransformer : TransformerBase<int, int>
    {
        public Report Report() => CreateProgressReport();

        protected override async IAsyncEnumerable<int> TransformWorkerAsync(
            IAsyncEnumerable<int> items, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
            {
                IncrementCurrentItemCount();
                yield return item * 10;
            }
        }
    }


    // Shows a component enriching the default report (proves CreateProgressReport isn't sealed).
    private sealed class TotalAwareExtractor : ExtractorBase<int>
    {
        private readonly int _total;

        public TotalAwareExtractor(int total) => _total = total;

        public Report Report() => CreateProgressReport();

        protected override Report CreateProgressReport() => new(CurrentItemCount, StartedAt, Elapsed, _total);

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync([EnumeratorCancellation] CancellationToken token)
        {
            await Task.Yield();
            IncrementCurrentItemCount();
            yield return 1;
        }
    }
}
