using System.Runtime.CompilerServices;
using Microsoft.Coyote.Specifications;
using Microsoft.Coyote.SystematicTesting;

namespace Wolfgang.Etl.Abstractions.Tests.Concurrency;

/// <summary>
/// Concurrency / race-condition stress tests (#207) driven by Microsoft Coyote's
/// systematic scheduler. Each <c>[Test]</c> method is replayed under thousands of
/// distinct task interleavings by <c>coyote test</c> (see
/// <c>.github/workflows/coyote.yaml</c>); a race, deadlock, or assertion violation
/// on any explored schedule fails the run.
/// </summary>
public static class ConcurrencyTests
{
    /// <summary>
    /// Two tasks concurrently drive the base class's per-item counter. Under every
    /// interleaving Coyote explores, no increment may be lost — the counter is an
    /// <see cref="System.Threading.Interlocked"/> operation. A non-atomic <c>++</c>
    /// would surface here as a lost update on some schedule.
    /// </summary>
    [Microsoft.Coyote.SystematicTesting.Test]
    public static async Task Concurrent_item_count_increments_never_lose_an_update()
    {
        var harness = new CounterHarness();
        const int perTask = 5;

        var first = Task.Run(() =>
        {
            for (var i = 0; i < perTask; i++)
            {
                harness.Bump();
            }
        });

        var second = Task.Run(() =>
        {
            for (var i = 0; i < perTask; i++)
            {
                harness.Bump();
            }
        });

        await Task.WhenAll(first, second);

        Specification.Assert(
            harness.Count == 2 * perTask,
            $"Lost update under a concurrent schedule: expected {2 * perTask}, got {harness.Count}.");
    }


    /// <summary>
    /// Races <see cref="System.IAsyncDisposable.DisposeAsync"/> against an in-flight
    /// enumeration of the same extractor. Under every interleaving the run must
    /// terminate — no deadlock, and no unobserved exception other than the expected
    /// cancellation/disposal signals.
    /// </summary>
    [Microsoft.Coyote.SystematicTesting.Test]
    public static async Task Dispose_racing_enumeration_never_deadlocks()
    {
        var extractor = new CounterHarness();

        var consume = Task.Run(async () =>
        {
            try
            {
                await foreach (var _ in extractor.ExtractAsync())
                {
                    // drain
                }
            }
            catch (ObjectDisposedException)
            {
                // acceptable outcome of racing disposal
            }
            catch (OperationCanceledException)
            {
                // acceptable outcome of racing disposal
            }
        });

        var dispose = Task.Run(async () => await extractor.DisposeAsync());

        await Task.WhenAll(consume, dispose);
        // Reaching here means neither task deadlocked on any explored schedule.
    }


    // Surfaces the protected per-item counter for the increment race, and provides a
    // trivial async source for the dispose race.
    private sealed class CounterHarness : ExtractorBase<int, Report>
    {
        public void Bump() => IncrementCurrentItemCount();

        public int Count => CurrentItemCount;

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync(
            [EnumeratorCancellation] CancellationToken token)
        {
            for (var i = 0; i < 4; i++)
            {
                token.ThrowIfCancellationRequested();
                yield return i;
                await Task.Yield();
            }
        }

        protected override Report CreateProgressReport() => new(CurrentItemCount);
    }
}
