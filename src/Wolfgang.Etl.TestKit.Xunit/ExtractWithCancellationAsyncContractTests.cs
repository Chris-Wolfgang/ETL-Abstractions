using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests for any type that implements
/// <see cref="IExtractWithCancellationAsync{TSource}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must implement <see cref="IExtractWithCancellationAsync{TItem}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the extractor yields.</typeparam>
/// <remarks>
/// <para>
/// Inherit from this class and implement the abstract factory methods to verify that
/// your extractor correctly satisfies the <see cref="IExtractWithCancellationAsync{TSource}"/> contract.
/// </para>
/// <para>
/// The expected items sequence must contain at least three items so that cancellation
/// mid-sequence can be meaningfully tested.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyExtractorCancellationContractTests
///     : ExtractWithCancellationAsyncContractTests&lt;MyExtractor, MyRecord&gt;
/// {
///     protected override MyExtractor CreateSut(int itemCount) =>
///         new MyExtractor(GetTestData(), itemCount);
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateExpectedItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c") };
/// }
/// </code>
/// </example>
public abstract class ExtractWithCancellationAsyncContractTests<TSut, TItem>
    where TSut : IExtractWithCancellationAsync<TItem>
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
    /// Verifies that <c>ExtractAsync(CancellationToken)</c> yields the expected items
    /// when passed <see cref="CancellationToken.None"/>.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_cancellation_token_yields_expected_items_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();

        var actual = await sut.ExtractAsync(CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Verifies that <c>ExtractAsync(CancellationToken)</c> throws
    /// <see cref="OperationCanceledException"/> and stops yielding items when the
    /// token is cancelled mid-sequence.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_cancellation_token_stops_when_token_is_cancelled_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        Assert.True(expected.Count >= 3, "CreateExpectedItems() must return at least 3 items.");

        using var cts = new CancellationTokenSource();
        var received = new List<TItem>();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in sut.ExtractAsync(cts.Token).ConfigureAwait(false))
            {
                received.Add(item);
                if (received.Count == 1)
                {
                    #if NET8_0_OR_GREATER
                    await cts.CancelAsync().ConfigureAwait(false);
                    #else
                    cts.Cancel();
                    #endif
                }
            }
        }).ConfigureAwait(false);

        // Must have received at least the item that triggered cancellation,
        // but fewer than the full sequence.
        Assert.Equal(1, received.Count);
    }

    /// <summary>
    /// Verifies that <c>ExtractAsync(CancellationToken)</c> throws
    /// <see cref="OperationCanceledException"/> immediately when passed an
    /// already-cancelled token.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_already_cancelled_token_throws_OperationCanceledException_Async()
    {
        var sut = CreateSut();
        using var cts = new CancellationTokenSource();
#if NET8_0_OR_GREATER
        await cts.CancelAsync().ConfigureAwait(false);
#else
        cts.Cancel();
#endif

        var received = new List<TItem>();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in sut.ExtractAsync(cts.Token).ConfigureAwait(false))
            {
                received.Add(item);
            }
        }).ConfigureAwait(false);

        Assert.Empty(received);
    }
}
