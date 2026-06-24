using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Abstractions.Benchmarks;

/// <summary>
/// Measures the cost of composing and running a full Extract → Transform → Load
/// pipeline. Compares the fluent <see cref="Pipeline"/> builder against hand-wired
/// <see cref="IAsyncEnumerable{T}"/> composition over the same in-memory stages,
/// so the delta is the builder's plumbing overhead (it should be negligible — the
/// builder is documented as zero-extra-allocation sugar over the same composition).
/// </summary>
[MemoryDiagnoser]
public class PipelineBenchmarks
{
    [Params(1_000, 100_000)]
    public int RecordCount { get; set; }



    [Benchmark(Baseline = true)]
    public async Task FluentPipeline()
    {
        await Pipeline
            .Extract(new RangeExtractor(RecordCount))
            .Transform(new IdentityTransformer())
            .Load(new SinkLoader())
            .RunAsync();
    }



    [Benchmark]
    public async Task ManualComposition()
    {
        var extractor = new RangeExtractor(RecordCount);
        var transformer = new IdentityTransformer();
        var loader = new SinkLoader();

        await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync()));
    }



    // Same Extract -> Transform -> Load shape, but wired with the base-class
    // components (ExtractorBase / TransformerBase / LoaderBase) so the result
    // captures the full abstraction overhead — Interlocked item counting and
    // the async-iterator wrappers on every stage.
    [Benchmark]
    public async Task BaseClassComposition()
    {
        var extractor = new SequenceExtractor(RecordCount);
        var transformer = new PassThroughTransformer();
        var loader = new CountingLoader();

        await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync()));
    }
}
