using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsCheck;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.Fuzz;

/// <summary>
/// Property-based fuzz tests (#196) over the generic <see cref="EtlPipeline"/> core:
/// a source of arbitrary length flows to the sink preserving order and count, a
/// <c>Through</c> map is applied elementwise, and progress counts both ends
/// (extracted at the source, loaded at the sink).
/// </summary>
public class EtlPipelineFuzzTests
{
    // Each case spins up async pipeline machinery, so cap the per-property count —
    // the pure Report properties carry the deep sweep cheaply.
    private static long Iterations
    {
        get
        {
            var n = long.TryParse(Environment.GetEnvironmentVariable("FUZZ_ITER"), out var v) && v > 0 ? v : 1000;
            return Math.Min(n, 20_000);
        }
    }


    private static readonly Gen<int[]> Source = Gen.Int[-1_000_000, 1_000_000].Array[0, 64];


    [Fact]
    public void From_To_delivers_the_source_in_order_and_counts_both_ends()
    {
        Source.Sample(
            items =>
            {
                var loader = new CollectingLoader<int>();
                EtlPipelineProgress? last = null;
                var progress = new InlineProgress<EtlPipelineProgress>(p => last = p);

                EtlPipeline
                    .Create()
                    .From(AsyncSeq(items))
                    .To(loader)
                    .RunAsync(progress)
                    .GetAwaiter()
                    .GetResult();

                Assert.Equal(items, loader.Loaded);
                Assert.NotNull(last);
                Assert.Equal(items.Length, last!.RecordsExtracted);
                Assert.Equal(items.Length, last.RecordsLoaded);
            },
            iter: Iterations);
    }


    [Fact]
    public void Through_map_is_applied_elementwise_preserving_order()
    {
        Source.Sample(
            items =>
            {
                var loader = new CollectingLoader<long>();

                EtlPipeline
                    .Create()
                    .From(AsyncSeq(items))
                    .Through(new MapTransformer<int, long>(x => (long)x * 2 + 1))
                    .To(loader)
                    .RunAsync()
                    .GetAwaiter()
                    .GetResult();

                Assert.Equal(items.Select(x => (long)x * 2 + 1), loader.Loaded);
            },
            iter: Iterations);
    }


    private static async IAsyncEnumerable<T> AsyncSeq<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
