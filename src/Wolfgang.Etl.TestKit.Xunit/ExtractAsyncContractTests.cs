using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests for any type that implements
/// <see cref="IExtractAsync{TSource}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must implement <see cref="IExtractAsync{TItem}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the extractor yields.</typeparam>
/// <remarks>
/// <para>
/// Inherit from this class and implement the abstract factory methods to verify that
/// your extractor correctly satisfies the <see cref="IExtractAsync{TSource}"/> contract.
/// </para>
/// <para>
/// At minimum you must provide a SUT instance and a non-empty expected item sequence
/// containing at least three items. The same expected sequence must be returned on every
/// call to <see cref="CreateExpectedItems"/> within a single test run.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyExtractorContractTests
///     : ExtractAsyncContractTests&lt;MyExtractor, MyRecord&gt;
/// {
///     protected override MyExtractor CreateSut(int itemCount) =>
///         new MyExtractor(GetTestData(), itemCount);
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateExpectedItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c") };
/// }
/// </code>
/// </example>
public abstract class ExtractAsyncContractTests<TSut, TItem>
    where TSut : IExtractAsync<TItem>
    where TItem : notnull
{
    // ------------------------------------------------------------------
    // Factory methods
    // ------------------------------------------------------------------

    /// <summary>
    /// Creates the system under test configured to yield exactly <paramref name="itemCount"/> items.
    /// The items yielded must match the first <paramref name="itemCount"/> items returned by
    /// <see cref="CreateExpectedItems"/>.
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
    /// Verifies that <see cref="IExtractAsync{TSource}.ExtractAsync"/> returns a non-null sequence.
    /// </summary>
    [Fact]
    public void ExtractAsync_returns_non_null_sequence()
    {
        var sut = CreateSut();
        var result = sut.ExtractAsync();
        Assert.NotNull(result);
    }

    /// <summary>
    /// Verifies that <see cref="IExtractAsync{TSource}.ExtractAsync"/> yields the expected items
    /// in the expected order.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_yields_expected_items_in_order_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();

        var actual = await sut.ExtractAsync().ToListAsync().ConfigureAwait(false);

        Assert.Equal(expected, actual);
    }
}
