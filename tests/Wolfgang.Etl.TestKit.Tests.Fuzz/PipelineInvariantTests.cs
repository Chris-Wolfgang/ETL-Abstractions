using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CsCheck;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Fuzz;

/// <summary>
/// Property-based pipeline invariants (#373): for any input sequence and any set of fault positions in
/// the extractor and in the loader, with both stages skipping their faults, an
/// <see cref="EtlPipeline"/> run must satisfy every invariant below. CsCheck generates the cases and
/// shrinks a failure to a minimal input with a replayable seed.
/// <list type="number">
///   <item><description><b>Loaded set</b>: the loader receives the input minus the extractor's fault
///   positions, then keeps that stream minus its own fault positions, in order.</description></item>
///   <item><description><b>Counter reconciliation</b>: each stage's <c>CurrentItemCount</c> equals
///   the items it saw (faulted items included, since a fault fires after the item is counted), its
///   <c>CurrentErrorItemCount</c> equals its fault count, and its <c>CurrentSkippedItemCount</c> stays 0
///   (error skips are not <c>SkipItemCount</c> skips).</description></item>
///   <item><description><b>Every fault reported once</b>: each stage's <c>CapturedErrors</c> holds
///   exactly one entry per injected fault, with the 1-based item number of that position and the
///   injected exception instance.</description></item>
///   <item><description><b>Pipeline totals</b>: the final <see cref="EtlPipelineProgress"/> reports
///   extracted = input - extractor faults, loaded = extracted, and ErrorItemCount = both stages'
///   faults.</description></item>
/// </list>
/// The case count is <c>FUZZ_ITER</c> (default 1000 per run), the same knob as the other fuzz tests;
/// the scheduled fuzz workflow raises it.
/// </summary>
public class PipelineInvariantTests
{
    private static long Iterations =>
        long.TryParse(Environment.GetEnvironmentVariable("FUZZ_ITER"), out var n) && n > 0
            ? n
            : 1000;



    // An input sequence, a fault mask over it for the extractor, and a fault mask over the stream that
    // survives the extractor for the loader.
    private static readonly Gen<(List<int> Items, bool[] ExtractFaults, bool[] LoadFaults)> Cases =
        Gen.Int.List[0, 30]
            .SelectMany
            (
                items => Gen.Bool.Array[items.Count]
                    .SelectMany
                    (
                        extractMask => Gen.Bool.Array[extractMask.Count(f => !f)]
                            .Select(loadMask => (items, extractMask, loadMask))
                    )
            );



    [Fact]
    public void Pipeline_with_skipped_faults_satisfies_every_invariant()
    {
        Cases.Sample
        (
            c => AssertInvariants(c.Items, c.ExtractFaults, c.LoadFaults),
            iter: Iterations
        );
    }



    private static void AssertInvariants(List<int> items, bool[] extractFaults, bool[] loadFaults)
    {
        var extractFaultIndices = Indices(extractFaults);
        var loadFaultIndices = Indices(loadFaults);

        var extractor = new FaultyExtractor<int>(items).SkipErrors();
        var extractExceptions = extractFaultIndices.ToDictionary(i => i, i => new InvalidOperationException($"extract {i}"));
        foreach (var pair in extractExceptions)
        {
            extractor.ThrowAt(pair.Key, pair.Value);
        }

        var loader = new FaultyLoader<int>(collectItems: true).SkipErrors();
        var loadExceptions = loadFaultIndices.ToDictionary(i => i, i => new InvalidOperationException($"load {i}"));
        foreach (var pair in loadExceptions)
        {
            loader.ThrowAt(pair.Key, pair.Value);
        }

        EtlPipelineProgress? final = null;
        EtlPipeline
            .Create()
            .From(extractor)
            .To(loader)
            .RunAsync(new InlineProgress<EtlPipelineProgress>(p => final = p), CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        var extracted = items.Where((_, i) => !extractFaults[i]).ToList();
        var expectedLoaded = extracted.Where((_, i) => !loadFaults[i]).ToList();

        // 1. Loaded set
        Assert.Equal(expectedLoaded, loader.GetCollectedItems());

        // 2. Counter reconciliation
        Assert.Equal(extracted.Count, extractor.CurrentItemCount);
        Assert.Equal(extractFaultIndices.Count, extractor.CurrentErrorItemCount);
        Assert.Equal(0, extractor.CurrentSkippedItemCount);
        Assert.Equal(expectedLoaded.Count, loader.CurrentItemCount);
        Assert.Equal(loadFaultIndices.Count, loader.CurrentErrorItemCount);
        Assert.Equal(0, loader.CurrentSkippedItemCount);

        // 3. Every fault reported exactly once, with its position and its exception
        AssertCaptured(extractExceptions, extractor.CapturedErrors);
        AssertCaptured(loadExceptions, loader.CapturedErrors);

        // 4. Pipeline totals
        Assert.NotNull(final);
        Assert.Equal(extracted.Count, final!.ExtractedItemCount);
        Assert.Equal(extracted.Count, final.LoadedItemCount);
        Assert.Equal(extractFaultIndices.Count + loadFaultIndices.Count, final.ErrorItemCount);
    }



    private static void AssertCaptured
    (
        Dictionary<int, InvalidOperationException> injected,
        IReadOnlyList<ItemErrorContext> captured
    )
    {
        Assert.Equal
        (
            injected.Keys.Select(i => (long)i + 1).OrderBy(n => n),
            captured.Select(c => c.ItemNumber).OrderBy(n => n)
        );

        foreach (var context in captured)
        {
            Assert.Same(injected[(int)(context.ItemNumber - 1)], context.Exception);
        }
    }



    private static List<int> Indices(bool[] mask) =>
        Enumerable
            .Range(0, mask.Length)
            .Where(i => mask[i])
            .ToList();



    // Reports on the calling thread, so the last value is visible as soon as RunAsync returns.
    private sealed class InlineProgress<T> : IProgress<T>
    {
        private readonly Action<T> _report;


        public InlineProgress(Action<T> report) => _report = report;


        public void Report(T value) => _report(value);
    }
}
