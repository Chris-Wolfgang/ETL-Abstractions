using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CsCheck;
using Wolfgang.Etl.TestKit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Fuzz;

/// <summary>
/// Property-based fuzz tests (#115) over the doubles' public surface using
/// CsCheck. The invariants exercised:
/// <list type="bullet">
///   <item><description>Identity — the extractor / transformer / loader move an
///   arbitrary item sequence through unchanged (round-trip).</description></item>
///   <item><description>Windowing — <c>SkipItemCount</c> / <c>MaximumItemCount</c>
///   yield exactly <c>Skip(s).Take(m)</c> for arbitrary <c>s</c>/<c>m</c>,
///   including the boundaries (<c>s</c> past the end, <c>s + m</c> past the end)
///   where off-by-one / negative-width bugs hide.</description></item>
/// </list>
/// The case count is <c>FUZZ_ITER</c> (default 1000 per PR run); the scheduled
/// fuzz.yaml sets it far higher. On failure CsCheck shrinks to a minimal input
/// and prints a replayable seed.
/// </summary>
public class TestKitFuzzTests
{
    private static long Iterations =>
        long.TryParse(System.Environment.GetEnvironmentVariable("FUZZ_ITER"), out var n) && n > 0
            ? n
            : 1000;

    private static readonly Gen<List<int>> Items = Gen.Int.List[0, 40];

    // Arbitrary items plus an arbitrary skip/max, deliberately allowed to exceed
    // the item count so the clamping boundaries are exercised. Max is >= 1 (0 is a
    // distinct "unlimited vs none" semantic tested separately in the unit suite).
    private static readonly Gen<(List<int> Items, int Skip, int Max)> Windowed =
        Gen.Select(Items, Gen.Int[0, 45], Gen.Int[1, 45], (items, skip, max) => (items, skip, max));

    [Fact]
    public void Extractor_yields_input_unchanged()
    {
        Items.Sample(
            items =>
            {
                using var extractor = new TestExtractor<int>(items);
                Assert.Equal(items, Drain(extractor.ExtractAsync(CancellationToken.None)));
            },
            iter: Iterations);
    }

    [Fact]
    public void Transformer_yields_input_unchanged()
    {
        Items.Sample(
            items =>
            {
                using var transformer = new TestTransformer<int>();
                Assert.Equal(items, Drain(transformer.TransformAsync(ToAsync(items), CancellationToken.None)));
            },
            iter: Iterations);
    }

    [Fact]
    public void Loader_collects_input_unchanged()
    {
        Items.Sample(
            items =>
            {
                using var loader = new TestLoader<int>(collectItems: true);
                loader.LoadAsync(ToAsync(items), CancellationToken.None).GetAwaiter().GetResult();
                Assert.Equal(items, loader.GetCollectedItems());
            },
            iter: Iterations);
    }

    [Fact]
    public void Extractor_windows_by_skip_then_max()
    {
        Windowed.Sample(
            t =>
            {
                using var extractor = new TestExtractor<int>(t.Items)
                {
                    SkipItemCount = t.Skip,
                    MaximumItemCount = t.Max,
                };

                var expected = t.Items.Skip(t.Skip).Take(t.Max).ToList();
                Assert.Equal(expected, Drain(extractor.ExtractAsync(CancellationToken.None)));
            },
            iter: Iterations);
    }

    // CsCheck.Sample is synchronous, so the async pipeline is drained by blocking.
    private static List<T> Drain<T>(IAsyncEnumerable<T> source)
    {
        var results = new List<T>();
        var enumerator = source.GetAsyncEnumerator(CancellationToken.None);
        try
        {
            while (enumerator.MoveNextAsync().AsTask().GetAwaiter().GetResult())
            {
                results.Add(enumerator.Current);
            }
        }
        finally
        {
            enumerator.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        return results;
    }

    private static async IAsyncEnumerable<int> ToAsync(IEnumerable<int> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await System.Threading.Tasks.Task.CompletedTask;
    }
}
