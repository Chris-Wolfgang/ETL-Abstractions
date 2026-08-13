using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing opt-in xUnit idempotency contract tests for any type that
/// implements <see cref="IExtractWithProgressAsync{TSource, TProgress}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must implement
/// <see cref="IExtractWithProgressAsync{TItem, TProgress}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the extractor yields.</typeparam>
/// <typeparam name="TProgress">The type of the progress report.</typeparam>
/// <remarks>
/// <para>
/// <b>This base class is opt-in.</b> Inherit from it <em>only</em> if your extractor supports
/// being run more than once on the <em>same</em> instance and is expected to produce the same
/// result each time (idempotent extraction). Single-use components — for example a
/// stream-backed extractor that consumes a forward-only source on its first run — must
/// <em>not</em> inherit this class, because a second extraction would observe an exhausted
/// source rather than a repeatable one.
/// </para>
/// <para>
/// Each test calls <see cref="CreateSut(int)"/> exactly once and then exercises the SUT twice,
/// asserting that the second run behaves identically to the first.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyExtractorIdempotencyTests
///     : IdempotentExtractorContractTests&lt;MyExtractor, MyRecord, Report&gt;
/// {
///     protected override MyExtractor CreateSut(int itemCount) =>
///         new MyExtractor(GetReEnumerableTestData(), itemCount);
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateExpectedItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c"), new("d"), new("e") };
/// }
/// </code>
/// </example>
public abstract class IdempotentExtractorContractTests<TSut, TItem, TProgress>
    where TSut : ExtractorBase<TItem, TProgress>, IExtractAsync<TItem>, IExtractWithProgressAsync<TItem, TProgress>
    where TItem : notnull
    where TProgress : notnull
{
    // CurrentItemCount resets at the start of each run as of Wolfgang.Etl.Abstractions 0.14.0
    // (ETL-Abstractions#246). ExtractAsync_when_called_twice_CurrentItemCount_resets_Async below
    // verifies that per-run reset. These tests rely on the default MaximumItemCount.

    // ------------------------------------------------------------------
    // Factory methods
    // ------------------------------------------------------------------

    /// <summary>
    /// Creates the system under test configured to yield exactly <paramref name="itemCount"/> items.
    /// The items yielded must match the first <paramref name="itemCount"/> items returned by
    /// <see cref="CreateExpectedItems"/>, and must be re-yieldable so the instance can be run twice.
    /// </summary>
    /// <param name="itemCount">The number of items the SUT should yield. Pass 0 for an empty source.</param>
    protected abstract TSut CreateSut(int itemCount);

    private const int DefaultItemCount = 5;

    /// <summary>Creates the SUT with <see cref="DefaultItemCount"/> items.</summary>
    private TSut CreateSut() => CreateSut(DefaultItemCount);

    /// <summary>
    /// Returns the expected items that the SUT should yield when created with
    /// <see cref="CreateSut(int)"/>. Must return at least 5 items.
    /// </summary>
    protected abstract IReadOnlyList<TItem> CreateExpectedItems();



    // ------------------------------------------------------------------
    // Tests
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that extracting twice from the same instance yields identical items, in the
    /// same order, on both runs.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_when_called_twice_yields_identical_items_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();

        var firstRun = await sut
            .ExtractAsync(new Progress<TProgress>())
            .ToListAsync()
            .ConfigureAwait(false);

        var secondRun = await sut
            .ExtractAsync(new Progress<TProgress>())
            .ToListAsync()
            .ConfigureAwait(false);

        Assert.Equal(expected, firstRun);
        Assert.Equal(firstRun, secondRun);
        Assert.Equal(expected, secondRun);
    }



    /// <summary>
    /// Verifies that extracting twice from the same instance reports progress on each run.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_when_called_twice_with_progress_both_runs_report_Async()
    {
        var sut = CreateSut();

        var firstCapture = new ProgressCapture<TProgress>();
        await sut.ExtractAsync(firstCapture).ToListAsync().ConfigureAwait(false);

        var secondCapture = new ProgressCapture<TProgress>();
        await sut.ExtractAsync(secondCapture).ToListAsync().ConfigureAwait(false);

        ProgressAssert.HasReports(firstCapture);
        ProgressAssert.HasReports(secondCapture);
    }



    /// <summary>
    /// Verifies that <c>CurrentItemCount</c> reflects only the most recent run: after extracting
    /// twice from the same instance, the count equals the number of items yielded by the second
    /// run alone, not the cumulative total across both runs.
    /// </summary>
    /// <remarks>
    /// This verifies the per-run reset contract introduced in
    /// <c>Wolfgang.Etl.Abstractions</c> 0.14.0 (ETL-Abstractions#246), where
    /// <c>CurrentItemCount</c> and <c>CurrentSkippedItemCount</c> reset at the start of each run.
    /// </remarks>
    [Fact]
    public async Task ExtractAsync_when_called_twice_CurrentItemCount_resets_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();

        await sut.ExtractAsync().ToListAsync().ConfigureAwait(false);

        await sut.ExtractAsync().ToListAsync().ConfigureAwait(false);

        Assert.Equal(expected.Count, sut.CurrentItemCount);
    }
}
