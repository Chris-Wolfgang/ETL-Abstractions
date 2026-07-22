using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Abstractions.AotSmoke;

// Native-AOT / trim smoke (#213). Exercises the library's public surface — both
// pipeline builders, all three base classes, progress reporting, and Report — so
// that a reflection / dynamic-codegen path that breaks under AOT surfaces here as
// a NotSupportedException / MissingMethodException at run time (or a trim warning
// at publish) instead of in a consumer's AOT-published app.

Console.WriteLine("AOT smoke: exercising Wolfgang.Etl.Abstractions...");

var extractProgress = new Progress<Report>(_ => { });
var loadProgress = new Progress<Report>(_ => { });

// Fluent Pipeline: base-class extractor -> transformer -> loader, with per-stage
// progress, naming, and stage disposal — exercises the base-class machinery.
var pipelineLoader = new ListLoader();
await Pipeline
    .Extract(new NumberExtractor(5))
    .WithProgress(extractProgress)
    .Transform(new DoubleTransformer())
    .Load(pipelineLoader)
    .WithProgress(loadProgress)
    .WithName("aot-smoke")
    .DisposeStagesOnCompletion()
    .RunAsync(CancellationToken.None);

Require(pipelineLoader.Items.SequenceEqual(new[] { 2, 4, 6, 8, 10 }), "Pipeline output");

// Generic EtlPipeline: From (IAsyncEnumerable) -> Through (transformer) ->
// To (base-class loader), with pipeline-level progress.
var sink = new ListLoader();
var etlProgress = new Progress<EtlPipelineProgress>(_ => { });
await EtlPipeline
    .Create()
    .From(Numbers(3))
    .Through(new PlusOneTransformer())
    .To(sink)
    .RunAsync(etlProgress, CancellationToken.None);

Require(sink.Items.SequenceEqual(new[] { 2, 3, 4 }), "EtlPipeline output");

// Escape hatch.
var seen = 0;
await foreach (var _ in EtlPipeline.Create().From(Numbers(4)).AsAsyncEnumerable(CancellationToken.None))
{
    seen++;
}

Require(seen == 4, "AsAsyncEnumerable");

// Report metrics: fixed arithmetic, no reflection.
var report = new Report(50)
{
    TotalItemCount = 100,
    Elapsed = TimeSpan.FromSeconds(10),
};

Require(report.CurrentItemCount == 50, "Report.CurrentItemCount");
Require(report.ItemsPerSecond == 5d, "Report.ItemsPerSecond");
Require(report.PercentComplete == 50d, "Report.PercentComplete");
Require(report.EstimatedRemaining == TimeSpan.FromSeconds(10), "Report.EstimatedRemaining");

Console.WriteLine("AOT smoke: OK");
return 0;


static async IAsyncEnumerable<int> Numbers(int count)
{
    for (var i = 1; i <= count; i++)
    {
        yield return i;
        await Task.Yield();
    }
}


static void Require(bool ok, string what)
{
    if (ok)
    {
        return;
    }

    Console.Error.WriteLine($"AOT smoke FAILED: {what}");
    Environment.Exit(1);
}
