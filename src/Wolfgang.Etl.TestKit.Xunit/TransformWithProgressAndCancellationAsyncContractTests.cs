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
/// <see cref="ITransformWithProgressAndCancellationAsync{TSource, TDestination, TProgress}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must implement
/// <see cref="ITransformWithProgressAndCancellationAsync{TSource, TDestination, TProgress}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the transformer consumes and produces.</typeparam>
/// <typeparam name="TProgress">The type of the progress report.</typeparam>
/// <remarks>
/// Inherit from this class to verify that your transformer satisfies the full
/// <see cref="ITransformWithProgressAndCancellationAsync{TSource, TDestination, TProgress}"/>
/// contract — covering all four transformer interfaces in the hierarchy.
/// </remarks>
/// <example>
/// <code>
/// public class MyTransformerFullContractTests
///     : TransformWithProgressAndCancellationAsyncContractTests&lt;MyTransformer, MyRecord, MyProgress&gt;
/// {
///     protected override MyTransformer CreateSut(int itemCount) =>
///         new MyTransformer();
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateExpectedItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c") };
/// }
/// </code>
/// </example>
public abstract class TransformWithProgressAndCancellationAsyncContractTests<TSut, TItem, TProgress>
    where TSut : ITransformWithProgressAndCancellationAsync<TItem, TItem, TProgress>
    where TItem : notnull
    where TProgress : notnull
{
    // ------------------------------------------------------------------
    // Factory methods
    // ------------------------------------------------------------------

    /// <summary>Creates the system under test.</summary>
    /// <returns>A new, fully initialised instance of <typeparamref name="TSut"/>.</returns>
    /// <summary>
    /// Creates the system under test configured to yield exactly <paramref name="itemCount"/> items.
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
    // Tests — ITransformAsync<TItem, TItem>
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;)</c> returns a
    /// non-null sequence.
    /// </summary>
    [Fact]
    public void TransformAsync_returns_non_null_sequence()
    {
        var sut = CreateSut();
        var result = sut.TransformAsync(AsyncEnumerable.Empty<TItem>());
        Assert.NotNull(result);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;)</c> yields the
    /// expected items in order.
    /// </summary>
    [Fact]
    public async Task TransformAsync_yields_expected_items_in_order_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();

        var actual = await sut.TransformAsync(expected.ToAsyncEnumerable()).ToListAsync().ConfigureAwait(false);

        Assert.Equal(expected, actual);
    }



    // ------------------------------------------------------------------
    // Tests — ITransformWithCancellationAsync<TItem, TItem>
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;, CancellationToken)</c>
    /// yields the expected items when passed <see cref="CancellationToken.None"/>.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_cancellation_token_yields_expected_items_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();

        var actual = await sut.TransformAsync(expected.ToAsyncEnumerable(), CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.Equal(expected, actual);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;, CancellationToken)</c>
    /// throws <see cref="OperationCanceledException"/> and stops yielding items when the
    /// token is cancelled mid-sequence.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_cancellation_token_stops_when_token_is_cancelled_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        Assert.True(expected.Count >= 3, "CreateExpectedItems() must return at least 3 items.");

        using var cts = new CancellationTokenSource();
        var received = new List<TItem>();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in sut.TransformAsync(expected.ToAsyncEnumerable(), cts.Token).ConfigureAwait(false))
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
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;, CancellationToken)</c>
    /// throws <see cref="OperationCanceledException"/> immediately when passed an
    /// already-cancelled token.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_already_cancelled_token_throws_OperationCanceledException_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        using var cts = new CancellationTokenSource();
#if NET8_0_OR_GREATER
        await cts.CancelAsync().ConfigureAwait(false);
#else
        cts.Cancel();
#endif

        var received = new List<TItem>();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in sut.TransformAsync(expected.ToAsyncEnumerable(), cts.Token).ConfigureAwait(false))
            {
                received.Add(item);
            }
        }).ConfigureAwait(false);

        Assert.Empty(received);
    }



    // ------------------------------------------------------------------
    // Tests — ITransformWithProgressAsync<TItem, TItem, TProgress>
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;)</c>
    /// throws <see cref="ArgumentNullException"/> when <c>progress</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public void TransformAsync_with_null_progress_throws_ArgumentNullException()
    {
        var sut = CreateSut();

        var ex = Assert.Throws<ArgumentNullException>(() =>
        {
            _ = sut.TransformAsync(AsyncEnumerable.Empty<TItem>(), (IProgress<TProgress>)null!);
        });

        Assert.Equal("progress", ex.ParamName);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;)</c>
    /// yields the expected items when a valid progress instance is supplied.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_yields_expected_items_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        var progress = new Progress<TProgress>();

        var actual = await sut.TransformAsync(expected.ToAsyncEnumerable(), progress).ToListAsync().ConfigureAwait(false);

        Assert.Equal(expected, actual);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;)</c>
    /// invokes the progress callback at least once during transformation.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_invokes_callback_at_least_once_Async()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.TransformAsync(CreateExpectedItems().ToAsyncEnumerable(), progress).ToListAsync().ConfigureAwait(false);

        Assert.True(callbackCount >= 1);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(items, IProgress&lt;TProgress&gt;)</c> invokes the
    /// progress callback at least once even when the source is empty.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_and_empty_source_invokes_callback_at_least_once_Async()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.TransformAsync(AsyncEnumerable.Empty<TItem>(), progress).ToListAsync().ConfigureAwait(false);

        Assert.True(callbackCount >= 1);
    }



    // ------------------------------------------------------------------
    // Tests — ITransformWithProgressAndCancellationAsync<TItem, TItem, TProgress>
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// throws <see cref="ArgumentNullException"/> when <c>progress</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public void TransformAsync_with_null_progress_and_token_throws_ArgumentNullException()
    {
        var sut = CreateSut();

        var ex = Assert.Throws<ArgumentNullException>(() =>
        {
            _ = sut.TransformAsync(AsyncEnumerable.Empty<TItem>(), (IProgress<TProgress>)null!, CancellationToken.None);
        });

        Assert.Equal("progress", ex.ParamName);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// yields the expected items when supplied valid arguments.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_and_token_yields_expected_items_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        var progress = new Progress<TProgress>();

        var actual = await sut.TransformAsync(expected.ToAsyncEnumerable(), progress, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.Equal(expected, actual);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// invokes the progress callback at least once during transformation.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_and_token_invokes_callback_at_least_once_Async()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.TransformAsync(CreateExpectedItems().ToAsyncEnumerable(), progress, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.True(callbackCount >= 1);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// invokes the progress callback at least once even when the source is empty.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_and_token_and_empty_source_invokes_callback_at_least_once_Async()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.TransformAsync(AsyncEnumerable.Empty<TItem>(), progress, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.True(callbackCount >= 1);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// throws <see cref="OperationCanceledException"/> and stops yielding when the token
    /// is cancelled mid-sequence.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_and_cancelled_token_stops_enumeration_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        Assert.True(expected.Count >= 3, "CreateExpectedItems() must return at least 3 items.");

        using var cts = new CancellationTokenSource();
        var received = new List<TItem>();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in sut.TransformAsync(expected.ToAsyncEnumerable(), progress, cts.Token).ConfigureAwait(false))
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
