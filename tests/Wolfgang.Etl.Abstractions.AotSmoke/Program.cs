using System.Threading.Channels;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Abstractions.AotSmoke;
using Wolfgang.Etl.ErrorPolicies;

// Native-AOT / trim smoke (#213). Exercises the library's public surface — both
// pipeline builders, all three base classes, progress reporting, Report, the
// PropertySelector reflection path and the Wolfgang.Etl.ErrorPolicies policies — so
// that a reflection / dynamic-codegen path that breaks under AOT surfaces here as
// a NotSupportedException / MissingMethodException at run time (or a trim warning
// at publish) instead of in a consumer's AOT-published app.

Console.WriteLine("AOT smoke: exercising Wolfgang.Etl.Abstractions and Wolfgang.Etl.ErrorPolicies...");

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

// Report metrics: fixed arithmetic, no reflection. Built via the timing constructor
// (cross-assembly-safe on every target framework) rather than the init form.
var report = new Report(50, startedAt: null, elapsed: TimeSpan.FromSeconds(10), totalItemCount: 100);

Require(report.CurrentItemCount == 50, "Report.CurrentItemCount");
Require(Near(report.ItemsPerSecond, 5d), "Report.ItemsPerSecond");
Require(Near(report.PercentComplete, 50d), "Report.PercentComplete");
Require(report.EstimatedRemaining == TimeSpan.FromSeconds(10), "Report.EstimatedRemaining");

// PropertySelector: the library's one reflection path (expression tree -> PropertyInfo).
// The property must survive trimming and resolve by name on the declaring type.
var amount = PropertySelector.Resolve((SmokeOrder order) => order.Amount);

Require(string.Equals(amount.Name, nameof(SmokeOrder.Amount), StringComparison.Ordinal), "PropertySelector.Resolve name");
Require(amount.DeclaringType == typeof(SmokeOrder), "PropertySelector.Resolve declaring type");
Require(amount.GetValue(new SmokeOrder { Amount = 12.5m }) is 12.5m, "PropertySelector.Resolve getter");

var boxed = PropertySelector.Resolve<SmokeOrder, object>(order => order.Amount);

Require(string.Equals(boxed.Name, nameof(SmokeOrder.Amount), StringComparison.Ordinal), "PropertySelector.Resolve boxed");

// Wolfgang.Etl.ErrorPolicies: every ready-made policy, including the logging ones, whose
// message formatting is where a trimmed consumer would notice a missing member.
var failure = new ItemErrorContext(7, new InvalidOperationException("bad item"), () => "raw line");
var logger = new RecordingLogger();
var deadLetters = new List<ItemErrorContext>();
var deadLetterChannel = Channel.CreateUnbounded<ItemErrorContext>();

Require(ItemErrorPolicy.Skip(failure) == ItemErrorAction.Skip, "ItemErrorPolicy.Skip");
Require(ItemErrorPolicy.Abort(failure) == ItemErrorAction.Abort, "ItemErrorPolicy.Abort");
Require(ItemErrorPolicy.SkipAndLog(logger)(failure) == ItemErrorAction.Skip, "ItemErrorPolicy.SkipAndLog");
Require(ItemErrorPolicy.SkipAndDeadLetter(deadLetters)(failure) == ItemErrorAction.Skip, "ItemErrorPolicy.SkipAndDeadLetter (collection)");
Require(ItemErrorPolicy.SkipAndDeadLetter(deadLetterChannel.Writer)(failure) == ItemErrorAction.Skip, "ItemErrorPolicy.SkipAndDeadLetter (channel)");
Require(ItemErrorPolicy.SkipDeadLetterAndLog(deadLetters, logger)(failure) == ItemErrorAction.Skip, "ItemErrorPolicy.SkipDeadLetterAndLog (collection)");
Require(ItemErrorPolicy.SkipDeadLetterAndLog(deadLetterChannel.Writer, logger)(failure) == ItemErrorAction.Skip, "ItemErrorPolicy.SkipDeadLetterAndLog (channel)");

Require(deadLetters.Count == 2 && deadLetters[0].ItemNumber == 7, "dead-letter collection");
Require(deadLetterChannel.Reader.Count == 2, "dead-letter channel");
Require(logger.Messages.Count == 3, "logged skips");
Require(logger.Messages.TrueForAll(message => message.Contains('7', StringComparison.Ordinal)), "logged item number");

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


static bool Near(double? actual, double expected)
{
    // A missing value (null) never matches; the tolerance absorbs floating-point rounding.
    return actual.HasValue && Math.Abs(actual.Value - expected) < 1e-9;
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
