using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// Mutation-hardening tests (#346) for <see cref="RetryingExtractor{T}"/>: they pin the exact
/// exception-message text, the <c>maxAttempts</c> validation boundary, and the two cancellation
/// checks in the worker that broader behavioural tests leave as surviving mutants.
/// </summary>
public class RetryingExtractorMutationTests
{
    // ------------------------------------------------------------------
    // Constructor — exception message text
    // ------------------------------------------------------------------

    [Fact]
    public void Constructor_when_failFirstAttempts_is_negative_reports_the_reason()
    {
        // Kills the string mutant on the failFirstAttempts ArgumentOutOfRangeException message.
        var ex = Assert.Throws<ArgumentOutOfRangeException>
        (
            () => new RetryingExtractor<int>(new[] { 1 }, failFirstAttempts: -1, maxAttempts: 1)
        );

        Assert.Contains("Must not be negative", ex.Message);
    }



    [Fact]
    public void Constructor_when_maxAttempts_is_less_than_one_reports_the_reason()
    {
        // Kills the string mutant on the maxAttempts ArgumentOutOfRangeException message.
        var ex = Assert.Throws<ArgumentOutOfRangeException>
        (
            () => new RetryingExtractor<int>(new[] { 1 }, failFirstAttempts: 0, maxAttempts: 0)
        );

        Assert.Contains("Must be at least 1", ex.Message);
    }



    // ------------------------------------------------------------------
    // maxAttempts validation boundary
    // ------------------------------------------------------------------

    [Fact]
    public async Task Constructor_when_maxAttempts_is_exactly_one_is_valid()
    {
        // Boundary: the guard is `maxAttempts < 1`, so 1 is the smallest VALID value. The `<= 1`
        // mutant would reject it — this run must construct cleanly and extract every item.
        var sut = new RetryingExtractor<int>(new[] { 1, 2, 3 }, failFirstAttempts: 0, maxAttempts: 1);

        var items = await sut.ExtractAsync(CancellationToken.None).ToListAsync();

        Assert.Equal(new[] { 1, 2, 3 }, items);
        Assert.Equal(1, sut.AttemptCount);
    }



    // ------------------------------------------------------------------
    // Transient-fault message — the LAST fault propagates
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_when_budget_is_exhausted_propagates_the_last_faults_message()
    {
        // failFirstAttempts (5) >= maxAttempts (3): every attempt faults and the budget runs out on
        // attempt 3, so the fault carried by attempt 3 is the one that propagates. Pins the
        // interpolated fault-message string and that the newest (not an earlier) fault surfaces.
        var sut = new RetryingExtractor<int>(new[] { 1, 2, 3 }, failFirstAttempts: 5, maxAttempts: 3);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>
        (
            async () =>
            {
                await foreach (var _ in sut.ExtractAsync(CancellationToken.None))
                {
                }
            }
        );

        Assert.Equal("Transient fault on attempt 3.", ex.Message);
        Assert.Equal(3, sut.AttemptCount);
    }



    // ------------------------------------------------------------------
    // Cancellation — the pre-loop check (empty source removes the in-loop mask)
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_with_empty_source_and_a_pre_cancelled_token_still_throws()
    {
        // With no items the in-loop ThrowIfCancellationRequested never runs, so the pre-loop check is
        // the only cancellation guard — dropping it would let an empty, already-cancelled run
        // complete silently. This isolates and kills that pre-loop statement.
        var sut = new RetryingExtractor<int>(Array.Empty<int>(), failFirstAttempts: 0, maxAttempts: 3);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            async () =>
            {
                await foreach (var _ in sut.ExtractAsync(cts.Token))
                {
                }
            }
        );
    }



    // ------------------------------------------------------------------
    // Cancellation — the in-loop check (cancelled only after the pre-loop check passes)
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_cancelled_while_iterating_items_throws_from_the_in_loop_check()
    {
        // The source cancels the token as its first item is pulled — after the pre-loop check has
        // already passed — so only the in-loop ThrowIfCancellationRequested can observe it. Without
        // that check every item would be yielded and the run would complete normally.
        using var cts = new CancellationTokenSource();
        var sut = new RetryingExtractor<int>
        (
            new CancellingSequence(cts, cancelAtIndex: 0, count: 5),
            failFirstAttempts: 0,
            maxAttempts: 3
        );

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            async () =>
            {
                await foreach (var _ in sut.ExtractAsync(cts.Token))
                {
                }
            }
        );

        // Cancellation is not a retryable fault, so exactly one worker attempt ran.
        Assert.Equal(1, sut.AttemptCount);
    }



    // A synchronous sequence that cancels the supplied source as a chosen item is produced, so the
    // token flips from not-cancelled (at the pre-loop check) to cancelled (at the in-loop check).
    private sealed class CancellingSequence : IEnumerable<int>
    {
        private readonly CancellationTokenSource _cts;
        private readonly int _cancelAtIndex;
        private readonly int _count;

        public CancellingSequence(CancellationTokenSource cts, int cancelAtIndex, int count)
        {
            _cts           = cts;
            _cancelAtIndex = cancelAtIndex;
            _count         = count;
        }



        public IEnumerator<int> GetEnumerator()
        {
            for (var i = 0; i < _count; i++)
            {
                if (i == _cancelAtIndex)
                {
                    _cts.Cancel();
                }

                yield return i;
            }
        }



        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
