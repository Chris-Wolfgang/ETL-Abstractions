using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// Mutation-hardening tests (#346) for <see cref="ManualTimeSource"/>: they pin the
/// <see cref="ManualTimeSource.Advance"/> validation boundary, its exception-message text, and the
/// direction the clock moves.
/// </summary>
public class ManualTimeSourceMutationTests
{
    [Fact]
    public void Advance_by_zero_is_allowed()
    {
        // Boundary: the guard is `delta < TimeSpan.Zero`, so a zero delta is VALID. The `<= Zero`
        // mutant would reject it — advancing by zero must not throw.
        var clock = new ManualTimeSource();

        clock.Advance(TimeSpan.Zero);
    }



    [Fact]
    public void Advance_when_delta_is_negative_reports_the_reason()
    {
        // Kills the string mutant on the negative-delta ArgumentOutOfRangeException message.
        var clock = new ManualTimeSource();

        var ex = Assert.Throws<ArgumentOutOfRangeException>
        (
            () => clock.Advance(TimeSpan.FromSeconds(-1))
        );

        Assert.Contains("Cannot advance time by a negative amount", ex.Message);
    }



    [Fact]
    public async Task Advance_moves_the_wall_clock_forward_by_the_delta()
    {
        // The wall clock (surfaced as Report.StartedAt when a run begins) must move FORWARD by the
        // delta. The `_utcNow -= delta` mutant would move it backward instead. Advancing before the
        // run means StartedAt is captured at the advanced instant.
        var start = new DateTimeOffset(2020, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = new ManualTimeSource(start);
        var sut   = new ExposedExtractor<int>(new[] { 1, 2, 3 });
        sut.WithTimeSource(clock);

        clock.Advance(TimeSpan.FromSeconds(10));
        await sut.ExtractAsync(CancellationToken.None).ToListAsync();

        var report = sut.GetProgressReport();

        Assert.Equal(start + TimeSpan.FromSeconds(10), report.StartedAt);
    }



    private sealed class ExposedExtractor<T> : TestExtractor<T>
        where T : notnull
    {
        public ExposedExtractor(IEnumerable<T> items)
            : base(items)
        {
        }



        public Report GetProgressReport() => CreateProgressReport();
    }
}
