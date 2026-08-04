using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing opt-in xUnit idempotency contract tests for any type that
/// implements <see cref="ITransformAsync{TSource, TDestination}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must implement <see cref="ITransformAsync{TSource, TDestination}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the transformer consumes and produces.</typeparam>
/// <remarks>
/// <para>
/// <b>This base class is opt-in.</b> Inherit from it <em>only</em> if your transformer supports
/// being run more than once on the <em>same</em> instance and produces the same output for the
/// same input each time, with no accumulated side effects. Single-use components — for example a
/// transformer that maintains running state intended to span a single pipeline — must
/// <em>not</em> inherit this class.
/// </para>
/// <para>
/// Each test calls <see cref="CreateSut(int)"/> exactly once and then transforms the same input
/// twice, asserting that the second run is unaffected by the first.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyTransformerIdempotencyTests
///     : IdempotentTransformerContractTests&lt;MyTransformer, MyRecord&gt;
/// {
///     protected override MyTransformer CreateSut(int itemCount) =>
///         new MyTransformer();
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateExpectedItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c"), new("d"), new("e") };
/// }
/// </code>
/// </example>
public abstract class IdempotentTransformerContractTests<TSut, TItem>
    where TSut : ITransformAsync<TItem, TItem>
    where TItem : notnull
{
    // CurrentItemCount resets at the start of each run as of Wolfgang.Etl.Abstractions 0.14.0
    // (ETL-Abstractions#246). TransformAsync_when_called_twice_CurrentItemCount_resets_Async below
    // verifies that per-run reset. These tests rely on the default MaximumItemCount.

    // ------------------------------------------------------------------
    // Factory methods
    // ------------------------------------------------------------------

    /// <summary>Creates the system under test.</summary>
    /// <param name="itemCount">
    /// The number of items the SUT is sized for. Pass 0 for an empty source.
    /// </param>
    protected abstract TSut CreateSut(int itemCount);

    private const int DefaultItemCount = 5;

    /// <summary>Creates the SUT with <see cref="DefaultItemCount"/> items.</summary>
    private TSut CreateSut() => CreateSut(DefaultItemCount);

    /// <summary>
    /// Returns the items used as input to the transformer and expected back unchanged. Must
    /// return at least 5 items.
    /// </summary>
    protected abstract IReadOnlyList<TItem> CreateExpectedItems();

    /// <summary>
    /// Returns the value of <paramref name="sut"/>'s <c>CurrentItemCount</c> after a transform.
    /// </summary>
    /// <remarks>
    /// The default implementation reads
    /// <see cref="TransformerBase{TSource, TDestination, TProgress}.CurrentItemCount"/> assuming the
    /// transformer derives from <see cref="TransformerBase{TSource, TDestination, TProgress}"/> with a
    /// <see cref="Report"/> progress type, which is the convention for ETL transformers. Override this
    /// when the transformer exposes its progress count through a different progress type or member.
    /// </remarks>
    /// <param name="sut">The system under test, after a transform has completed.</param>
    protected virtual int GetCurrentItemCount(TSut sut) =>
        ((TransformerBase<TItem, TItem, Report>)(object)sut!).CurrentItemCount;



    // ------------------------------------------------------------------
    // Tests
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that transforming the same input twice on the same instance yields identical
    /// output, in the same order, on both runs.
    /// </summary>
    [Fact]
    public async Task TransformAsync_when_called_twice_yields_identical_items_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();

        var firstRun = await sut
            .TransformAsync(expected.ToAsyncEnumerable())
            .ToListAsync()
            .ConfigureAwait(false);

        var secondRun = await sut
            .TransformAsync(expected.ToAsyncEnumerable())
            .ToListAsync()
            .ConfigureAwait(false);

        Assert.Equal(expected, firstRun);
        Assert.Equal(firstRun, secondRun);
        Assert.Equal(expected, secondRun);
    }



    /// <summary>
    /// Verifies that a second transform run is not influenced by the first: the second run's
    /// output shows no growth, duplication, or other accumulated side effect.
    /// </summary>
    [Fact]
    public async Task TransformAsync_when_called_twice_no_accumulated_side_effects_Async()
    {
        var sut = CreateSut();
        var input = CreateExpectedItems();

        var firstRun = await sut
            .TransformAsync(input.ToAsyncEnumerable())
            .ToListAsync()
            .ConfigureAwait(false);

        var secondRun = await sut
            .TransformAsync(input.ToAsyncEnumerable())
            .ToListAsync()
            .ConfigureAwait(false);

        Assert.Equal(firstRun.Count, secondRun.Count);
        Assert.Equal(input.Count, secondRun.Count);
        Assert.Equal(firstRun, secondRun);
    }



    /// <summary>
    /// Verifies that <c>CurrentItemCount</c> reflects only the most recent run: after transforming
    /// the same input twice on the same instance, the count equals the number of items produced by
    /// the second run alone, not the cumulative total across both runs.
    /// </summary>
    /// <remarks>
    /// This verifies the per-run reset contract introduced in
    /// <c>Wolfgang.Etl.Abstractions</c> 0.14.0 (ETL-Abstractions#246), where
    /// <c>CurrentItemCount</c> and <c>CurrentSkippedItemCount</c> reset at the start of each run.
    /// </remarks>
    [Fact]
    public async Task TransformAsync_when_called_twice_CurrentItemCount_resets_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();

        await sut
            .TransformAsync(expected.ToAsyncEnumerable())
            .ToListAsync()
            .ConfigureAwait(false);

        await sut
            .TransformAsync(expected.ToAsyncEnumerable())
            .ToListAsync()
            .ConfigureAwait(false);

        Assert.Equal(expected.Count, GetCurrentItemCount(sut));
    }
}
