using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Coyote;
using Microsoft.Coyote.Specifications;
using Microsoft.Coyote.SystematicTesting;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Concurrency;

/// <summary>
/// Systematic concurrency tests (#126) using Microsoft Coyote. Each test hands a
/// racy scenario to Coyote's <see cref="TestingEngine"/>, which replays it under
/// many controlled thread interleavings and fails if any schedule violates the
/// invariant — surfacing races that a single-run test would miss because it only
/// ever observes one schedule.
/// </summary>
/// <remarks>
/// The scenarios use only the public API and exercise the concurrency the doubles
/// must tolerate (the exact shapes called out in the issue): a cancellation token
/// cancelled while an enumeration is in flight, and a <c>Dispose</c> racing an
/// in-flight enumeration. For Coyote to control the Task schedule, this assembly
/// is rewritten by <c>coyote rewrite</c> in the concurrency workflow; run
/// un-rewritten it still executes but explores far fewer schedules.
/// </remarks>
public sealed class ConcurrencyTests
{
    private const int Iterations = 500;

    [Fact]
    public void Cancellation_during_enumeration_is_race_safe()
    {
        RunSystematic(CancellationScenario);
    }



    [Fact]
    public void Dispose_racing_enumeration_is_race_safe()
    {
        RunSystematic(DisposeDuringEnumerationScenario);
    }



    // The races' extremes, pinned deterministically so every outcome the
    // scenarios accept is exercised on every run, whichever side wins the race.

    [Fact]
    public async Task DrainAsync_when_never_cancelled_or_disposed_yields_every_item()
    {
        using var extractor = new TestExtractor<int>(Enumerable.Range(0, 8));

        var (observed, fault) = await DrainAsync(() => extractor.ExtractAsync());

        Assert.Null(fault);
        Assert.Equal(8, observed);
    }



    [Fact]
    public async Task DrainAsync_when_cancelled_before_enumeration_reports_OperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        using var extractor = new TestExtractor<int>(Enumerable.Range(0, 8));
        cts.Cancel();

        var (observed, fault) = await DrainAsync(() => extractor.ExtractAsync(cts.Token));

        Assert.IsAssignableFrom<OperationCanceledException>(fault);
        Assert.Equal(0, observed);
    }



    [Fact]
    public async Task DrainAsync_when_disposed_before_enumeration_reports_ObjectDisposedException()
    {
        var extractor = new TestExtractor<int>(Enumerable.Range(0, 8));
        extractor.Dispose();

        var (observed, fault) = await DrainAsync(() => extractor.ExtractAsync());

        Assert.IsType<ObjectDisposedException>(fault);
        Assert.Equal(0, observed);
    }



    // A cancellation arriving concurrently with enumeration must surface as a
    // clean OperationCanceledException (or simply complete) — never a torn state,
    // an unexpected exception type, or a hang.
    private static async Task CancellationScenario()
    {
        using var cts = new CancellationTokenSource();
        var extractor = new TestExtractor<int>(Enumerable.Range(0, 8));

        var enumerate = Task.Run(() => DrainAsync(() => extractor.ExtractAsync(cts.Token)));
        var cancel = Task.Run(() => cts.Cancel());

        await Task.WhenAll(enumerate, cancel);
        extractor.Dispose();

        var fault = (await enumerate).Fault;
        Specification.Assert
        (
            fault is null or OperationCanceledException,
            "Cancellation surfaced as an unexpected exception: {0}",
            fault
        );
    }



    // Dispose racing an in-flight enumeration must either let the enumeration
    // finish or surface ObjectDisposedException — never any other exception.
    private static async Task DisposeDuringEnumerationScenario()
    {
        var extractor = new TestExtractor<int>(Enumerable.Range(0, 8));

        var enumerate = Task.Run(() => DrainAsync(() => extractor.ExtractAsync()));
        var dispose = Task.Run(() => extractor.Dispose());

        await Task.WhenAll(enumerate, dispose);

        var fault = (await enumerate).Fault;
        Specification.Assert
        (
            fault is null or ObjectDisposedException,
            "Dispose racing enumeration surfaced an unexpected exception: {0}",
            fault
        );
    }



    // Starts and enumerates to the end, returning how many items were seen and
    // what ended the enumeration (ExtractAsync itself throws when the extractor
    // is already disposed, so the call happens inside the try), so the scenarios never branch on which side of
    // the race won.
    private static async Task<(int Observed, Exception? Fault)> DrainAsync(Func<IAsyncEnumerable<int>> extract)
    {
        var observed = 0;
        try
        {
            await foreach (var _ in extract())
            {
                observed++;
            }

            return (observed, null);
        }
        catch (Exception ex)
        {
            return (observed, ex);
        }
    }



    // Runs the scenario under Coyote's TestingEngine. When the coyote workflow
    // has rewritten the assembly (COYOTE_REWRITTEN=1), Coyote controls the Task
    // schedule and explores interleavings systematically. Un-rewritten (the
    // normal PR sweep) it cannot control the schedule, so it runs in systematic
    // fuzzing mode instead, which perturbs timing without needing the rewrite.
    private static void RunSystematic(Func<Task> scenario)
    {
        var rewritten = string.Equals(Environment.GetEnvironmentVariable("COYOTE_REWRITTEN"), "1", StringComparison.Ordinal);
        var configuration = Configuration.Create()
            .WithTestingIterations(Iterations)
            .WithMaxSchedulingSteps(500)
            .WithSystematicFuzzingEnabled(!rewritten);

        using var engine = TestingEngine.Create(configuration, scenario);
        engine.Run();

        var report = engine.TestReport;
        Assert.True
        (
            report.NumOfFoundBugs == 0,
            "Coyote found a concurrency bug: " + report.BugReports.FirstOrDefault()
        );
    }
}
