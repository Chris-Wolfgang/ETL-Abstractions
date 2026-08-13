using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Opt-in xUnit contract-test base asserting that a repeatable operation's hot path stays
/// within a declared per-item allocation budget — so a downstream author can lock in that
/// <em>their own</em> extractor / loader / transformer keeps an allocation-free (or
/// below-a-declared-N) hot path.
/// </summary>
/// <typeparam name="TSut">The type whose hot path is measured.</typeparam>
/// <remarks>
/// <para>
/// This generalizes the internal per-record allocation guard the kit uses on its own doubles
/// into a reusable base, alongside the opt-in <see cref="IdempotentExtractorContractTests{TSut,TItem,TProgress}"/>
/// / <see cref="SupportsDryRunContractTests{TSut}"/> family. Zero-allocation is <em>not</em>
/// universal — a real CSV/JSON extractor legitimately materializes a record per row — so this
/// is <b>opt-in</b> and the consumer <b>declares</b> the budget via <see cref="MaxBytesPerItem"/>
/// (<c>0</c> for a truly allocation-free path, or a small documented N).
/// </para>
/// <para>
/// The harness measures the <b>marginal</b> allocation per item —
/// <c>(alloc(10N) - alloc(N)) / (9N)</c> — so a one-time setup cost (buffers, the async state
/// machine) cancels out and only genuine per-item allocation counts. The cost of
/// <see cref="CreateSut"/> is excluded (it runs before the measurement window); exercise the
/// current stage via <see cref="Sut"/> from <see cref="ExerciseHotPathAsync"/>. It settles the
/// GC before each reading and takes the minimum across <see cref="Attempts"/> runs to shed
/// transient background-allocation noise.
/// </para>
/// <para>
/// <b>IMPORTANT — serialization.</b> The measurement uses the process-wide
/// <c>GC.GetTotalAllocatedBytes</c> counter, so a test allocating on another thread
/// <em>at the same time</em> pollutes the reading. Derived allocation tests <b>must run
/// serialized</b> — put them in a shared xUnit collection (a
/// <c>[CollectionDefinition("Allocation")]</c> in your test assembly plus
/// <c>[Collection("Allocation")]</c> on each), or disable test parallelization.
/// </para>
/// <para>
/// <b>Framework support.</b> <c>GC.GetTotalAllocatedBytes</c> exists on
/// .NET 6.0+ (net-core-app 3.0 / netstandard 2.1) only. On the kit's older target frameworks
/// (net462 / netstandard2.0) the budget test skips (passes without measuring).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [Collection("Allocation")] // serialize — the counter is process-wide
/// public sealed class MyExtractorAllocationTests
///     : AllocationBudgetContractTests&lt;MyExtractor&gt;
/// {
///     protected override MyExtractor CreateSut(int itemCount) => new MyExtractor(itemCount);
///
///     protected override async Task ExerciseHotPathAsync(CancellationToken ct)
///     {
///         await foreach (var _ in Sut.ExtractAsync(ct)) { }
///     }
///
///     // A record-materializing extractor would instead declare, say:
///     // protected override double MaxBytesPerItem =&gt; 48;
/// }
/// </code>
/// </example>
public abstract class AllocationBudgetContractTests<TSut>
{
    // ------------------------------------------------------------------
    // Factory / harness
    // ------------------------------------------------------------------

    /// <summary>
    /// The stage the current <see cref="ExerciseHotPathAsync"/> call should exercise. The harness
    /// creates it (via <see cref="CreateSut"/>) before the measurement window opens — outside it,
    /// so its construction cost is not measured — and disposes it afterwards.
    /// </summary>
    protected TSut Sut { get; private set; } = default!;

    /// <summary>
    /// Creates a system under test sized to produce or process exactly
    /// <paramref name="itemCount"/> items.
    /// </summary>
    /// <param name="itemCount">The number of items the hot path should run over.</param>
    protected abstract TSut CreateSut(int itemCount);

    /// <summary>
    /// Fully exercises <see cref="Sut"/>'s hot path — drain the enumeration, run the load, and so
    /// on. Only work done here is measured.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    protected abstract Task ExerciseHotPathAsync(CancellationToken cancellationToken);

    /// <summary>
    /// The maximum marginal allocation per item, in bytes. Defaults to <c>0</c>
    /// (allocation-free). Override to declare a per-item budget for a path that legitimately
    /// materializes state per item.
    /// </summary>
    protected virtual double MaxBytesPerItem => 0.0;

    /// <summary>
    /// The baseline item count; the harness also measures 10× this. A large denominator
    /// amortizes any fixed background-allocation spike inside a measurement window.
    /// </summary>
    protected virtual int BaseItemCount => 50_000;

    /// <summary>
    /// The number of measurement attempts. The minimum marginal reading is used, which sheds
    /// transient background-allocation noise.
    /// </summary>
    protected virtual int Attempts => 5;



    // ------------------------------------------------------------------
    // Contract
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that the SUT's hot path stays within <see cref="MaxBytesPerItem"/> of marginal
    /// allocation per item. Skips (passes) on target frameworks without
    /// <c>GC.GetTotalAllocatedBytes</c>.
    /// </summary>
    [Fact]
    public Task Hot_path_stays_within_the_allocation_budget_Async()
    {
#if NET6_0_OR_GREATER
        return RunBudgetAsync();
#else
        // GC.GetTotalAllocatedBytes is net6.0+ (netcoreapp3.0 / netstandard2.1) only; skip here.
        return Task.CompletedTask;
#endif
    }

#if NET6_0_OR_GREATER
    private async Task RunBudgetAsync()
    {
        // Warm up so JIT / first-run allocations do not land in the measurement window.
        _ = await MeasureAsync(BaseItemCount).ConfigureAwait(false);
        _ = await MeasureAsync(BaseItemCount * 10).ConfigureAwait(false);

        var best = double.MaxValue;

        for (var attempt = 0; attempt < Attempts; attempt++)
        {
            var small = await MeasureAsync(BaseItemCount).ConfigureAwait(false);
            var large = await MeasureAsync(BaseItemCount * 10).ConfigureAwait(false);

            var perItem = (double)(large - small) / ((BaseItemCount * 10) - BaseItemCount);
            best = Math.Min(best, perItem);
        }

        Assert.True
        (
            best <= MaxBytesPerItem,
            $"Marginal allocation {best:F3} B/item exceeds the {MaxBytesPerItem} B/item budget — the hot path allocates per item."
        );
    }

    // Creates a fresh SUT (its construction cost is excluded from the reading), settles the GC,
    // then measures the process-wide allocation delta across ExerciseHotPathAsync.
    private async Task<long> MeasureAsync(int itemCount)
    {
        Sut = CreateSut(itemCount);

        try
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var before = GC.GetTotalAllocatedBytes(precise: true);
            await ExerciseHotPathAsync(CancellationToken.None).ConfigureAwait(false);
            return GC.GetTotalAllocatedBytes(precise: true) - before;
        }
        finally
        {
            switch (Sut)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }

            Sut = default!;
        }
    }
#endif
}
