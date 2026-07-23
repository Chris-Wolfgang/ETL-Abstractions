// GC.GetAllocatedBytesForCurrentThread was added to .NET Framework in 4.8, so this
// suite compiles on net48+ / netcoreapp3.1+ / net5.0+ but not net462/net472. The
// allocation behaviour under test is TFM-agnostic, so the modern targets cover it.
#if !NET462 && !NET472
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.Performance;

/// <summary>
/// Allocation-free hot-path verification (#217). Some members are read or invoked
/// once per record (or once per progress tick) and must not allocate — a change
/// that "looks identical but allocates" silently regresses throughput and GC
/// pressure for high-volume consumers. Each test measures managed allocation on
/// the current thread across a tight loop and asserts it rounds to zero per call.
///
/// <para><b>Documented allocation-free surface (the allowlist).</b> Every member
/// below is asserted zero-alloc; anything not listed makes no such promise:</para>
/// <list type="bullet">
///   <item><see cref="Report.CurrentItemCount"/> — field read.</item>
///   <item><see cref="Report.ItemsPerSecond"/> — value-type arithmetic.</item>
///   <item><see cref="Report.PercentComplete"/> — returns a value-type <c>double?</c>.</item>
///   <item><see cref="Report.EstimatedRemaining"/> — returns a value-type <c>TimeSpan?</c>.</item>
///   <item><c>ExtractorBase.IncrementCurrentItemCount()</c> — <see cref="Interlocked"/> increment.</item>
///   <item><c>ExtractorBase.IncrementCurrentSkippedItemCount()</c> — <see cref="Interlocked"/> increment.</item>
/// </list>
///
/// <para>The per-call threshold is a small non-zero epsilon (not a hard 0) to
/// absorb one-time tiered-JIT recompilation during the measured loop, which is
/// amortised to a fraction of a byte across the iteration count. A real
/// per-call allocation (e.g. a boxed value or a captured closure) is at least 24
/// bytes and fails cleanly.</para>
/// </summary>
public sealed class AllocationFreeTests
{
    [Fact]
    public void Report_CurrentItemCount_is_allocation_free()
    {
        var report = new Report(42);
        AssertAllocationFree(() => _ = report.CurrentItemCount);
    }


    [Fact]
    public void Report_ItemsPerSecond_is_allocation_free()
    {
        var report = new Report(50) { Elapsed = TimeSpan.FromSeconds(10) };
        AssertAllocationFree(() => _ = report.ItemsPerSecond);
    }


    [Fact]
    public void Report_PercentComplete_is_allocation_free()
    {
        var report = new Report(50) { TotalItemCount = 100 };
        AssertAllocationFree(() => _ = report.PercentComplete);
    }


    [Fact]
    public void Report_EstimatedRemaining_is_allocation_free()
    {
        var report = new Report(50) { TotalItemCount = 100, Elapsed = TimeSpan.FromSeconds(10) };
        AssertAllocationFree(() => _ = report.EstimatedRemaining);
    }


    [Fact]
    public void IncrementCurrentItemCount_is_allocation_free()
    {
        var harness = new CounterHarness();
        AssertAllocationFree(harness.Increment);
    }


    [Fact]
    public void IncrementCurrentSkippedItemCount_is_allocation_free()
    {
        var harness = new CounterHarness();
        AssertAllocationFree(harness.IncrementSkipped);
    }


    // Measures managed bytes allocated on the current thread across a tight loop and
    // asserts the per-call average is below the threshold. GC.GetAllocatedBytesForCurrentThread
    // is available on every target framework (net462+, netcoreapp3.1+). The loop runs on the
    // test thread with no await, so the measurement is not polluted by other threads.
    private static void AssertAllocationFree(
        Action hotPath,
        double maxBytesPerCall = 1.0,
        int iterations = 10_000,
        [CallerMemberName] string caller = "")
    {
        // Warm up so JIT + tiered compilation settle before measuring.
        for (var i = 0; i < 200; i++)
        {
            hotPath();
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < iterations; i++)
        {
            hotPath();
        }

        var after = GC.GetAllocatedBytesForCurrentThread();
        var perCall = (after - before) / (double)iterations;

        Assert.True(
            perCall < maxBytesPerCall,
            $"{caller}: allocated {perCall:F3} bytes/call over {iterations} iterations (threshold {maxBytesPerCall} B/call).");
    }


    // Surfaces the protected counter increments for measurement.
    private sealed class CounterHarness : ExtractorBase<int, Report>
    {
        public void Increment() => IncrementCurrentItemCount();

        public void IncrementSkipped() => IncrementCurrentSkippedItemCount();

        protected override IAsyncEnumerable<int> ExtractWorkerAsync(CancellationToken token)
            => throw new NotSupportedException();

        protected override Report CreateProgressReport() => new(CurrentItemCount);
    }
}
#endif
