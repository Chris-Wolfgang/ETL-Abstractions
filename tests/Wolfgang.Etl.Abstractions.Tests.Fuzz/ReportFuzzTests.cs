using System;
using CsCheck;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.Fuzz;

/// <summary>
/// Property-based fuzz tests (#196) over <see cref="Report"/>'s derived metrics.
/// The throughput/completion estimates are pure functions of the snapshot inputs
/// (<c>CurrentItemCount</c>, <c>Elapsed</c>, <c>TotalItemCount</c>), so a report is
/// internally consistent for any inputs. Case count is <c>FUZZ_ITER</c> (default
/// 1000 per PR); fuzz.yaml raises it for the deep sweep. On failure CsCheck shrinks
/// to a minimal case and prints a replayable seed.
/// </summary>
public class ReportFuzzTests
{
    private static long Iterations =>
        long.TryParse(Environment.GetEnvironmentVariable("FUZZ_ITER"), out var n) && n > 0
            ? n
            : 1000;


    // (currentItemCount, elapsedSeconds, totalItemCount-or-null)
    private static readonly Gen<(int Current, double Elapsed, int? Total)> Inputs =
        Gen.Select(
            Gen.Int[0, int.MaxValue],
            Gen.Double[0.0, 1_000_000.0],
            Gen.Bool,
            Gen.Int[0, int.MaxValue],
            (current, elapsed, hasTotal, total) => (current, elapsed, hasTotal ? (int?)total : null));


    [Fact]
    public void Report_metrics_are_internally_consistent()
    {
        Inputs.Sample(
            input =>
            {
                var (current, elapsedSeconds, total) = input;

                var report = new Report(current)
                {
                    Elapsed = TimeSpan.FromSeconds(elapsedSeconds),
                    TotalItemCount = total,
                };

                // Throughput is never negative.
                Assert.True(report.ItemsPerSecond >= 0d);

                if (total is null)
                {
                    // Completion estimates require a known total.
                    Assert.Null(report.PercentComplete);
                    Assert.Null(report.EstimatedRemaining);
                }
                else
                {
                    // Percentage is always clamped to [0, 100].
                    Assert.NotNull(report.PercentComplete);
                    Assert.InRange(report.PercentComplete!.Value, 0d, 100d);

                    // Once the count reaches the total, nothing remains.
                    if (current >= total.Value)
                    {
                        Assert.Equal(TimeSpan.Zero, report.EstimatedRemaining);
                    }
                }

                // When an estimate is produced, it is never negative.
                if (report.EstimatedRemaining is { } remaining)
                {
                    Assert.True(remaining >= TimeSpan.Zero);
                }
            },
            iter: Iterations);
    }


    [Fact]
    public void Report_ctor_rejects_negative_count()
    {
        Gen.Int[int.MinValue, -1].Sample(
            n => Assert.Throws<ArgumentOutOfRangeException>(() => new Report(n)),
            iter: Iterations);
    }


    [Fact]
    public void Report_TotalItemCount_rejects_negative()
    {
        Gen.Int[int.MinValue, -1].Sample(
            n => Assert.Throws<ArgumentOutOfRangeException>(() => new Report(0) { TotalItemCount = n }),
            iter: Iterations);
    }
}
