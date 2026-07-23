// Sustained-load workload for GC / allocation profiling (#212).
//
// Runs an in-memory extract -> transform -> load pipeline in a loop for a
// wall-clock duration (env GC_WORKLOAD_SECONDS or argv[0], default 600), so
// `dotnet-counters` / `dotnet-trace` can characterise gen0/1/2 promotion rates,
// LOH pressure, finalizer-queue depth, and thread-pool starvation under a real
// streaming ETL pattern rather than the micro-scale BDN benchmarks.
//
// Not a benchmark — the scale / cycle counts are arbitrary. Meaningful metrics
// come from the EventPipe trace the outer workflow captures, not the wall time
// this process reports.
//
// Refs #212.

using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions;

const int recordsPerCycle = 50_000;
var durationSeconds = ParseDuration(args);

Console.WriteLine($"[gc-workload] Version  : {typeof(Report).Assembly.GetName().Version}");
Console.WriteLine($"[gc-workload] Runtime  : {Environment.Version}");
Console.WriteLine($"[gc-workload] ServerGC : {GCSettings.IsServerGC}");
Console.WriteLine($"[gc-workload] Duration : {durationSeconds}s");
Console.WriteLine($"[gc-workload] Records  : {recordsPerCycle:N0} / cycle");
Console.WriteLine($"[gc-workload] PID      : {Environment.ProcessId}");

// Progress callbacks are part of the hot path (the base classes fire a timer and
// allocate a Report per tick); a no-op sink keeps that path live.
var progress = new Progress<Report>(_ => { });
var stopwatch = Stopwatch.StartNew();
long cycles = 0;
long totalRecords = 0;

while (stopwatch.Elapsed.TotalSeconds < durationSeconds)
{
    var loader = new CountingLoader();

    await Pipeline
        .Extract(new RangeExtractor(recordsPerCycle))
        .WithProgress(progress)
        .Transform(new DoublingTransformer())
        .Load(loader)
        .WithProgress(progress)
        .WithName("gc-profile")
        .DisposeStagesOnCompletion()
        .RunAsync(CancellationToken.None);

    cycles++;
    totalRecords += loader.Count;

    if (cycles % 10 == 0)
    {
        Console.WriteLine(
            $"[gc-workload] cycle={cycles} records={totalRecords:N0} " +
            $"gen0={GC.CollectionCount(0)} gen1={GC.CollectionCount(1)} gen2={GC.CollectionCount(2)} " +
            $"alloc={GC.GetTotalAllocatedBytes(precise: true) / 1048576.0:F1}MB " +
            $"heap={GC.GetTotalMemory(forceFullCollection: false) / (1024 * 1024)}MB");
    }
}

stopwatch.Stop();
Console.WriteLine(
    $"[gc-workload] DONE cycles={cycles} records={totalRecords:N0} elapsed={stopwatch.Elapsed.TotalSeconds:F1}s");
Console.WriteLine(
    $"[gc-workload] FINAL gen0={GC.CollectionCount(0)} gen1={GC.CollectionCount(1)} gen2={GC.CollectionCount(2)} " +
    $"alloc={GC.GetTotalAllocatedBytes(precise: true) / 1048576.0:F1}MB");

return 0;


static int ParseDuration(string[] args)
{
    var env = Environment.GetEnvironmentVariable("GC_WORKLOAD_SECONDS");
    if (int.TryParse(env, out var fromEnv) && fromEnv > 0)
    {
        return fromEnv;
    }

    if (args.Length > 0 && int.TryParse(args[0], out var fromArg) && fromArg > 0)
    {
        return fromArg;
    }

    return 600;
}


// Base-class stages so the workload exercises the real hot paths — async
// iteration, the Interlocked item counters, per-tick Report allocation, the
// progress timer, and stage disposal.
internal sealed class RangeExtractor(int count) : ExtractorBase<int, Report>
{
    protected override async IAsyncEnumerable<int> ExtractWorkerAsync(
        [EnumeratorCancellation] CancellationToken token)
    {
        for (var i = 0; i < count; i++)
        {
            IncrementCurrentItemCount();
            yield return i;
            if ((i & 4095) == 0)
            {
                await Task.Yield();
            }
        }
    }

    protected override Report CreateProgressReport() => new(CurrentItemCount);
}


internal sealed class DoublingTransformer : TransformerBase<int, int, Report>
{
    protected override async IAsyncEnumerable<int> TransformWorkerAsync(
        IAsyncEnumerable<int> items,
        [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var item in items.WithCancellation(token))
        {
            yield return item * 2;
        }
    }

    protected override Report CreateProgressReport() => new(CurrentItemCount);
}


internal sealed class CountingLoader : LoaderBase<int, Report>
{
    public long Count { get; private set; }

    protected override async Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
    {
        await foreach (var item in items.WithCancellation(token))
        {
            IncrementCurrentItemCount();
            _ = item;
            Count++;
        }
    }

    protected override Report CreateProgressReport() => new(CurrentItemCount);
}
