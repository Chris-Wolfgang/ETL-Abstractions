using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Coyote;
using Microsoft.Coyote.SystematicTesting;
using Wolfgang.Etl.TestKit;
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

    // A cancellation arriving concurrently with enumeration must surface as a
    // clean OperationCanceledException (or simply complete) — never a torn state,
    // an unexpected exception type, or a hang.
    private static async Task CancellationScenario()
    {
        using var cts = new CancellationTokenSource();
        var extractor = new TestExtractor<int>(Enumerable.Range(0, 8));

        var enumerate = Task.Run(async () =>
        {
            try
            {
                await foreach (var _ in extractor.ExtractAsync(cts.Token))
                {
                }
            }
            catch (OperationCanceledException)
            {
                // Expected — cancellation is the point.
            }
        });

        var cancel = Task.Run(() => cts.Cancel());

        await Task.WhenAll(enumerate, cancel);
        extractor.Dispose();
    }

    // Disposing the extractor while an enumeration is in flight must not throw an
    // unexpected exception or deadlock, regardless of interleaving. If dispose
    // wins the race an ObjectDisposedException is a legitimate outcome.
    private static async Task DisposeDuringEnumerationScenario()
    {
        var extractor = new TestExtractor<int>(Enumerable.Range(0, 8));

        var enumerate = Task.Run(async () =>
        {
            try
            {
                await foreach (var _ in extractor.ExtractAsync())
                {
                }
            }
            catch (ObjectDisposedException)
            {
                // Legitimate if dispose won the race.
            }
        });

        var dispose = Task.Run(() => extractor.Dispose());

        await Task.WhenAll(enumerate, dispose);
    }

    private static void RunSystematic(Func<Task> scenario)
    {
        // These tests require the assembly to be rewritten by `coyote rewrite`
        // (the concurrency workflow does this before running them). Run
        // un-rewritten — e.g. in the normal PR test sweep, which discovers every
        // project under tests/ — Coyote cannot control the Task schedule and
        // reports false deadlocks. The workflow sets COYOTE_REWRITTEN=1 after
        // rewriting; without it, skip so the normal sweep stays green.
        if (!string.Equals(Environment.GetEnvironmentVariable("COYOTE_REWRITTEN"), "1", StringComparison.Ordinal))
        {
            return;
        }

        var configuration = Configuration.Create()
            .WithTestingIterations(Iterations)
            .WithMaxSchedulingSteps(500);

        using var engine = TestingEngine.Create(configuration, scenario);
        engine.Run();

        var report = engine.TestReport;
        Assert.True
        (
            report.NumOfFoundBugs == 0,
            report.NumOfFoundBugs == 0
                ? string.Empty
                : "Coyote found a concurrency bug: " + report.BugReports.FirstOrDefault()
        );
    }
}
