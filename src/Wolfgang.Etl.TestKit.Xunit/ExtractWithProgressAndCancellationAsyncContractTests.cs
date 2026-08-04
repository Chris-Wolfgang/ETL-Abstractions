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
/// <see cref="IExtractWithProgressAndCancellationAsync{TSource, TProgress}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must implement
/// <see cref="IExtractWithProgressAndCancellationAsync{TItem, TProgress}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the extractor yields.</typeparam>
/// <typeparam name="TProgress">The type of the progress report.</typeparam>
/// <remarks>
/// Inherit from this class to verify that your extractor satisfies the full
/// <see cref="IExtractWithProgressAndCancellationAsync{TSource, TProgress}"/> contract —
/// both the progress callback behaviour and cancellation while progress reporting is active.
/// </remarks>
/// <example>
/// <code>
/// public class MyExtractorFullContractTests
///     : ExtractWithProgressAndCancellationAsyncContractTests&lt;MyExtractor, MyRecord, MyProgress&gt;
/// {
///     protected override MyExtractor CreateSut(int itemCount) =>
///         new MyExtractor(GetTestData(), itemCount);
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateExpectedItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c") };
/// }
/// </code>
/// </example>
public abstract class ExtractWithProgressAndCancellationAsyncContractTests<TSut, TItem, TProgress>
    where TSut : IExtractWithProgressAndCancellationAsync<TItem, TProgress>
    where TItem : notnull
    where TProgress : notnull
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

    /// <summary>Creates the SUT with an empty source.</summary>
    private TSut CreateSutWithNoItems() => CreateSut(0);

    /// <summary>
    /// Returns the expected items that the SUT should yield when created with
    /// <see cref="CreateSut(int)"/>. Must return at least 5 items.
    /// </summary>
    protected abstract IReadOnlyList<TItem> CreateExpectedItems();



    // ------------------------------------------------------------------
    // Tests — IExtractAsync<TItem>
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>ExtractAsync()</c> returns a non-null sequence.
    /// </summary>
    [Fact]
    public void ExtractAsync_returns_non_null_sequence()
    {
        var sut = CreateSut();
        var result = sut.ExtractAsync();
        Assert.NotNull(result);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync()</c> yields the expected items in the expected order.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_yields_expected_items_in_order_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();

        var actual = await sut.ExtractAsync().ToListAsync().ConfigureAwait(false);

        Assert.Equal(expected, actual);
    }



    // ------------------------------------------------------------------
    // Tests — IExtractWithCancellationAsync<TItem>
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



    // ------------------------------------------------------------------
    // Tests — IExtractWithProgressAsync<TItem, TProgress>
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;)</c> throws
    /// <see cref="ArgumentNullException"/> when <c>progress</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public void ExtractAsync_with_null_progress_throws_ArgumentNullException()
    {
        var sut = CreateSut();

        var ex = Assert.Throws<ArgumentNullException>(() =>
        {
            _ = sut.ExtractAsync((IProgress<TProgress>)null!);
        });

        Assert.Equal("progress", ex.ParamName);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;)</c> yields the expected
    /// items when a valid progress instance is supplied.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_yields_expected_items_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        var progress = new Progress<TProgress>();

        var actual = await sut.ExtractAsync(progress).ToListAsync().ConfigureAwait(false);

        Assert.Equal(expected, actual);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;)</c> invokes the progress
    /// callback at least once during extraction.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_invokes_callback_at_least_once_Async()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.ExtractAsync(progress).ToListAsync().ConfigureAwait(false);

        Assert.True(callbackCount >= 1);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;)</c> invokes the progress
    /// callback at least once even when the source is empty.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_and_empty_source_invokes_callback_at_least_once_Async()
    {
        var sut = CreateSutWithNoItems();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.ExtractAsync(progress).ToListAsync().ConfigureAwait(false);

        Assert.True(callbackCount >= 1);
    }



    // ------------------------------------------------------------------
    // Tests — IExtractWithProgressAndCancellationAsync<TItem, TProgress>
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// throws <see cref="ArgumentNullException"/> when <c>progress</c> is
    /// <see langword="null"/>.
    /// </summary>
    [Fact]
    public void ExtractAsync_with_null_progress_and_token_throws_ArgumentNullException()
    {
        var sut = CreateSut();

        var ex = Assert.Throws<ArgumentNullException>(() =>
        {
            _ = sut.ExtractAsync((IProgress<TProgress>)null!, CancellationToken.None);
        });

        Assert.Equal("progress", ex.ParamName);
    }

    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// yields the expected items when supplied valid arguments.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_and_token_yields_expected_items_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        var progress = new Progress<TProgress>();

        var actual = await sut.ExtractAsync(progress, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// invokes the progress callback at least once during extraction.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_and_token_invokes_callback_at_least_once_Async()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.ExtractAsync(progress, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.True(callbackCount >= 1);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// invokes the progress callback at least once even when the source is empty.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_and_token_and_empty_source_invokes_callback_at_least_once_Async()
    {
        var sut = CreateSutWithNoItems();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.ExtractAsync(progress, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.True(callbackCount >= 1);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// throws <see cref="OperationCanceledException"/> and stops yielding when the token
    /// is cancelled mid-sequence.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_and_cancelled_token_stops_enumeration_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        Assert.True(expected.Count >= 3, "CreateExpectedItems() must return at least 3 items.");

        using var cts = new CancellationTokenSource();
        var received = new List<TItem>();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in sut.ExtractAsync(progress, cts.Token).ConfigureAwait(false))
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

        Assert.Equal(1, received.Count);
    }
}
