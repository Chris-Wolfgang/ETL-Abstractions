using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests for any type that inherits
/// from <see cref="TransformerBase{TSource,TDestination,TProgress}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must inherit from
/// <see cref="TransformerBase{TSource, TDestination, TProgress}"/>.
/// </typeparam>
/// <typeparam name="TItem">
/// The item type — both the source and destination type for the transformer under test.
/// </typeparam>
/// <typeparam name="TProgress">The type of the progress report.</typeparam>
/// <remarks>
/// <para>
/// Inherit from this class to get a comprehensive suite of contract tests covering all
/// public behaviour defined by <see cref="TransformerBase{TSource, TDestination, TProgress}"/>:
/// </para>
/// <list type="bullet">
///   <item><description>All four <c>TransformAsync</c> overloads yield the expected items.</description></item>
///   <item><description>Null guards on <c>items</c> and <c>progress</c> arguments.</description></item>
///   <item><description>Cancellation is honoured on all cancellable overloads.</description></item>
///   <item><description><c>CurrentItemCount</c> is incremented as items are transformed.</description></item>
///   <item><description><c>ReportingInterval</c> rejects values less than 1.</description></item>
///   <item><description><c>MaximumItemCount</c> stops transformation at the specified limit.</description></item>
///   <item><description><c>MaximumItemCount</c> rejects values less than 1.</description></item>
///   <item><description><c>SkipItemCount</c> skips the specified number of items.</description></item>
///   <item><description><c>SkipItemCount</c> rejects values less than 0.</description></item>
///   <item><description>Progress callbacks fire when the timer fires.</description></item>
/// </list>
/// <para>
/// You are responsible for implementing <c>MaximumItemCount</c> and
/// <c>SkipItemCount</c> behaviour in your <c>TransformWorkerAsync</c> override.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyTransformerContractTests
///     : TransformerBaseContractTests&lt;MyTransformer, MyRecord, MyProgress&gt;
/// {
///     protected override MyTransformer CreateSut(int itemCount) =>
///         new MyTransformer();
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateExpectedItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c"), new("d"), new("e") };
/// }
/// </code>
/// </example>
public abstract class TransformerBaseContractTests<TSut, TItem, TProgress>
    where TSut : TransformerBase<TItem, TItem, TProgress>
    where TItem : notnull
    where TProgress : notnull
{
    // ------------------------------------------------------------------
    // Factory methods
    // ------------------------------------------------------------------

    /// <summary>
    /// Creates the system under test configured to yield exactly <paramref name="itemCount"/> items.
    /// </summary>
    /// <param name="itemCount">The number of items the SUT should yield. Pass 0 for an empty source.</param>
    protected abstract TSut CreateSut(int itemCount);

    /// <summary>
    /// <b>Deprecated.</b> The contract now drives progress timing via
    /// <see cref="ManualProgressTimerCore"/> and <c>WithManualProgressTimer</c>, which need no
    /// per-component timer plumbing — so overriding this is no longer required. Retained for source
    /// compatibility with existing overrides; remove your override (and the component's
    /// <c>IProgressTimer</c>-injection ctor) and it will be dropped in a future major version.
    /// </summary>
    /// <param name="timer">Unused by the contract.</param>
    /// <returns>A new instance of <typeparamref name="TSut"/> (in existing overrides only).</returns>
    /// <exception cref="NotSupportedException">
    /// Always, if the base (non-overridden) implementation is invoked — the contract no longer calls it.
    /// </exception>
    protected virtual TSut CreateSutWithTimer(IProgressTimer timer) =>
        throw new NotSupportedException
        (
            "CreateSutWithTimer is no longer used by the contract; progress timing is driven via " +
            "ManualProgressTimerCore + WithManualProgressTimer. Remove this override."
        );


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

    /// <summary>Returns the expected items as an async enumerable for feeding into the transformer.</summary>
    private IAsyncEnumerable<TItem> CreateInputItemsAsync() =>
        CreateExpectedItems().ToAsyncEnumerable();



    // ------------------------------------------------------------------
    // TransformAsync(IAsyncEnumerable) — basic transformation
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>TransformAsync(items)</c> returns a non-null sequence.
    /// </summary>
    [Fact]
    public void TransformAsync_returns_non_null_sequence()
    {
        var sut = CreateSut();
        var result = sut.TransformAsync(AsyncEnumerable.Empty<TItem>());
        Assert.NotNull(result);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(items)</c> yields all expected items in order.
    /// </summary>
    [Fact]
    public async Task TransformAsync_yields_all_expected_items_in_order_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();

        var actual = await sut.TransformAsync(CreateInputItemsAsync()).ToListAsync().ConfigureAwait(false);

        Assert.Equal(expected, actual);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(items)</c> yields an empty sequence without
    /// error when the source contains no items.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_empty_source_yields_no_items_Async()
    {
        var sut = CreateSutWithNoItems();

        var actual = await sut.TransformAsync(AsyncEnumerable.Empty<TItem>()).ToListAsync().ConfigureAwait(false);

        Assert.Empty(actual);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(items)</c> throws <see cref="ArgumentNullException"/>
    /// when <c>items</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public void TransformAsync_with_null_items_throws_ArgumentNullException()
    {
        var sut = CreateSut();

        var ex = Assert.Throws<ArgumentNullException>(() =>
        {
            _ = sut.TransformAsync(null!);
        });

        Assert.Equal("items", ex.ParamName);
    }

    /// <summary>
    /// Verifies that <c>CurrentItemCount</c> equals the number of items transformed
    /// after a full run via <c>TransformAsync(items)</c>.
    /// </summary>
    [Fact]
    public async Task TransformAsync_increments_CurrentItemCount_for_each_item_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();

        await sut.TransformAsync(CreateInputItemsAsync()).ToListAsync().ConfigureAwait(false);

        Assert.Equal(expected.Count, sut.CurrentItemCount);
    }



    /// <summary>
    /// Verifies that <c>CurrentItemCount</c> reflects the exact number of items
    /// yielded so far at every step of enumeration: it is <c>0</c> before the
    /// first item is pulled and equals <c>k</c> immediately after the k-th item
    /// is yielded.
    /// </summary>
    /// <remarks>
    /// This is a stronger guarantee than
    /// <see cref="TransformAsync_increments_CurrentItemCount_for_each_item_Async"/>,
    /// which only checks the final total. Enumerating one item at a time catches
    /// implementations that increment the counter at the wrong point relative to
    /// <c>yield return</c>.
    /// </remarks>
    [Fact]
    public async Task TransformAsync_CurrentItemCount_tracks_each_yielded_item_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        Assert.True(expected.Count >= 1, "CreateExpectedItems() must return at least 1 item.");

        await using var enumerator = sut.TransformAsync(CreateInputItemsAsync()).GetAsyncEnumerator();

        Assert.Equal(0, sut.CurrentItemCount);

        var count = 0;
        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            count++;
            Assert.Equal(count, sut.CurrentItemCount);
        }

        Assert.Equal(expected.Count, sut.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // TransformAsync(IAsyncEnumerable, CancellationToken) — cancellation
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>TransformAsync(items, CancellationToken)</c> throws
    /// <see cref="ArgumentNullException"/> when <c>items</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public void TransformAsync_with_token_and_null_items_throws_ArgumentNullException()
    {
        var sut = CreateSut();

        var ex = Assert.Throws<ArgumentNullException>(() =>
        {
            _ = sut.TransformAsync(null!, CancellationToken.None);
        });

        Assert.Equal("items", ex.ParamName);
    }

    /// <summary>
    /// Verifies that <c>TransformAsync(items, CancellationToken)</c> yields all expected
    /// items when passed <see cref="CancellationToken.None"/>.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_token_yields_all_expected_items_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();

        var actual = await sut.TransformAsync(CreateInputItemsAsync(), CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.Equal(expected, actual);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(items, CancellationToken)</c> yields an empty
    /// sequence without error when the source contains no items.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_token_and_empty_source_yields_no_items_Async()
    {
        var sut = CreateSutWithNoItems();

        var actual = await sut.TransformAsync(AsyncEnumerable.Empty<TItem>(), CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.Empty(actual);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(items, CancellationToken)</c> throws
    /// <see cref="OperationCanceledException"/> and stops yielding when the token is cancelled.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_token_stops_when_token_is_cancelled_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        Assert.True(expected.Count >= 3, "CreateExpectedItems() must return at least 3 items.");

        using var cts = new CancellationTokenSource();
        var received = new List<TItem>();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in sut.TransformAsync(CreateInputItemsAsync(), cts.Token).ConfigureAwait(false))
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
    /// Verifies that <c>TransformAsync(items, CancellationToken)</c> throws
    /// <see cref="OperationCanceledException"/> immediately when passed an already-cancelled token.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_already_cancelled_token_throws_OperationCanceledException_Async()
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
            await foreach (var item in sut.TransformAsync(CreateInputItemsAsync(), cts.Token).ConfigureAwait(false))
            {
                received.Add(item);
            }
        }).ConfigureAwait(false);

        Assert.Empty(received);
    }



    // ------------------------------------------------------------------
    // TransformAsync(IAsyncEnumerable, IProgress) — progress only
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>TransformAsync(items, progress)</c> throws
    /// <see cref="ArgumentNullException"/> when <c>items</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public void TransformAsync_with_progress_and_null_items_throws_ArgumentNullException()
    {
        var sut = CreateSut();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        var ex = Assert.Throws<ArgumentNullException>(() =>
        {
            _ = sut.TransformAsync(null!, progress);
        });

        Assert.Equal("items", ex.ParamName);
    }

    /// <summary>
    /// Verifies that <c>TransformAsync(items, progress)</c> throws
    /// <see cref="ArgumentNullException"/> when <c>progress</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public void TransformAsync_with_null_progress_throws_ArgumentNullException()
    {
        var sut = CreateSut();

        var ex = Assert.Throws<ArgumentNullException>(() =>
        {
            _ = sut.TransformAsync(CreateInputItemsAsync(), null!);
        });

        Assert.Equal("progress", ex.ParamName);
    }

    /// <summary>
    /// Verifies that <c>TransformAsync(items, progress)</c> yields all expected items
    /// when valid arguments are supplied.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_yields_all_expected_items_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        var actual = await sut.TransformAsync(CreateInputItemsAsync(), progress).ToListAsync().ConfigureAwait(false);

        Assert.Equal(expected, actual);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(items, progress)</c> yields an empty sequence
    /// without error when the source contains no items.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_and_empty_source_yields_no_items_Async()
    {
        var sut = CreateSutWithNoItems();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        var actual = await sut.TransformAsync(AsyncEnumerable.Empty<TItem>(), progress).ToListAsync().ConfigureAwait(false);

        Assert.Empty(actual);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(items, progress)</c> invokes the progress
    /// callback when the <see cref="IProgressTimer"/> fires.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_invokes_callback_when_timer_fires_Async()
    {
        var timer = new ManualProgressTimerCore();
        var sut = CreateSut();
        sut.WithManualProgressTimer(timer);
        TProgress? captured = default;
        var progress = new SynchronousProgress<TProgress>(r => captured = r);

        await using var enumerator = sut.TransformAsync(CreateInputItemsAsync(), progress).GetAsyncEnumerator();
        await enumerator.MoveNextAsync().ConfigureAwait(false);
        timer.Tick();

        Assert.NotNull(captured);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(items, IProgress&lt;TProgress&gt;)</c> invokes the
    /// progress callback at least once during transformation when using a standard
    /// <see cref="Progress{T}"/>.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_invokes_callback_at_least_once_Async()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.TransformAsync(CreateInputItemsAsync(), progress).ToListAsync().ConfigureAwait(false);

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
    // TransformAsync(IAsyncEnumerable, IProgress, CancellationToken)
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>TransformAsync(items, progress, CancellationToken)</c> throws
    /// <see cref="ArgumentNullException"/> when <c>items</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public void TransformAsync_with_progress_and_token_and_null_items_throws_ArgumentNullException()
    {
        var sut = CreateSut();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        var ex = Assert.Throws<ArgumentNullException>(() =>
        {
            _ = sut.TransformAsync(null!, progress, CancellationToken.None);
        });

        Assert.Equal("items", ex.ParamName);
    }

    /// <summary>
    /// Verifies that <c>TransformAsync(items, progress, CancellationToken)</c> throws
    /// <see cref="ArgumentNullException"/> when <c>progress</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public void TransformAsync_with_progress_and_token_and_null_progress_throws_ArgumentNullException()
    {
        var sut = CreateSut();

        var ex = Assert.Throws<ArgumentNullException>(() =>
        {
            _ = sut.TransformAsync(CreateInputItemsAsync(), null!, CancellationToken.None);
        });

        Assert.Equal("progress", ex.ParamName);
    }

    /// <summary>
    /// Verifies that <c>TransformAsync(items, progress, CancellationToken)</c> yields all
    /// expected items when valid arguments are supplied.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_and_token_yields_all_expected_items_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        var actual = await sut.TransformAsync(CreateInputItemsAsync(), progress, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.Equal(expected, actual);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(items, progress, CancellationToken)</c> yields an
    /// empty sequence without error when the source contains no items.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_and_token_and_empty_source_yields_no_items_Async()
    {
        var sut = CreateSutWithNoItems();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        var actual = await sut.TransformAsync(AsyncEnumerable.Empty<TItem>(), progress, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.Empty(actual);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(items, progress, CancellationToken)</c> throws
    /// <see cref="OperationCanceledException"/> and stops yielding when the token is cancelled.
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
            await foreach (var item in sut.TransformAsync(CreateInputItemsAsync(), progress, cts.Token).ConfigureAwait(false))
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
    /// Verifies that <c>TransformAsync(items, progress, CancellationToken)</c> invokes the
    /// progress callback when the <see cref="IProgressTimer"/> fires.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_and_token_invokes_callback_when_timer_fires_Async()
    {
        var timer = new ManualProgressTimerCore();
        var sut = CreateSut();
        sut.WithManualProgressTimer(timer);
        TProgress? captured = default;
        var progress = new SynchronousProgress<TProgress>(r => captured = r);

        await using var enumerator = sut.TransformAsync(CreateInputItemsAsync(), progress, CancellationToken.None).GetAsyncEnumerator();
        await enumerator.MoveNextAsync().ConfigureAwait(false);
        timer.Tick();

        Assert.NotNull(captured);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(items, IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// invokes the progress callback at least once during transformation when using a standard
    /// <see cref="Progress{T}"/>.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_and_token_invokes_callback_at_least_once_Async()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.TransformAsync(CreateInputItemsAsync(), progress, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.True(callbackCount >= 1);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(items, IProgress&lt;TProgress&gt;, CancellationToken)</c>
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



    // ------------------------------------------------------------------
    // ReportingInterval
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>ReportingInterval</c> defaults to 1000.
    /// </summary>
    [Fact]
    public void ReportingInterval_defaults_to_1000()
    {
        var sut = CreateSut();
        Assert.Equal(1_000, sut.ReportingInterval);
    }

    /// <summary>
    /// Verifies that setting <c>ReportingInterval</c> to a positive value succeeds.
    /// </summary>
    [Fact]
    public void ReportingInterval_can_be_set_to_positive_value()
    {
        var sut = CreateSut();
        sut.ReportingInterval = 1;
        Assert.Equal(1, sut.ReportingInterval);
    }

    /// <summary>
    /// Verifies that setting <c>ReportingInterval</c> to zero throws
    /// <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void ReportingInterval_set_to_zero_throws_ArgumentOutOfRangeException()
    {
        var sut = CreateSut();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.ReportingInterval = 0);
    }

    /// <summary>
    /// Verifies that setting <c>ReportingInterval</c> to a negative value throws
    /// <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void ReportingInterval_set_to_negative_throws_ArgumentOutOfRangeException()
    {
        var sut = CreateSut();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.ReportingInterval = -1);
    }



    // ------------------------------------------------------------------
    // MaximumItemCount
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>MaximumItemCount</c> defaults to <see cref="int.MaxValue"/>.
    /// </summary>
    [Fact]
    public void MaximumItemCount_defaults_to_int_MaxValue()
    {
        var sut = CreateSut();
        Assert.Equal(int.MaxValue, sut.MaximumItemCount);
    }

    /// <summary>
    /// Verifies that setting <c>MaximumItemCount</c> to a positive value succeeds.
    /// </summary>
    [Fact]
    public void MaximumItemCount_can_be_set_to_positive_value()
    {
        var sut = CreateSut();
        sut.MaximumItemCount = 10;
        Assert.Equal(10, sut.MaximumItemCount);
    }

    /// <summary>
    /// Verifies that setting <c>MaximumItemCount</c> to zero throws
    /// <see cref="ArgumentOutOfRangeException"/> — the minimum for a transformer is 1.
    /// </summary>
    [Fact]
    public void MaximumItemCount_set_to_zero_throws_ArgumentOutOfRangeException()
    {
        var sut = CreateSut();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.MaximumItemCount = 0);
    }

    /// <summary>
    /// Verifies that setting <c>MaximumItemCount</c> to a negative value throws
    /// <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void MaximumItemCount_set_to_negative_throws_ArgumentOutOfRangeException()
    {
        var sut = CreateSut();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.MaximumItemCount = -1);
    }

    /// <summary>
    /// Verifies that the transformer stops yielding items once <c>MaximumItemCount</c>
    /// is reached.
    /// </summary>
    [Fact]
    public async Task TransformAsync_stops_at_MaximumItemCount_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        Assert.True(expected.Count >= 3, "CreateExpectedItems() must return at least 3 items.");

        sut.MaximumItemCount = 1;

        var actual = await sut.TransformAsync(CreateInputItemsAsync()).ToListAsync().ConfigureAwait(false);

        Assert.Equal(1, actual.Count);
        Assert.Equal(expected[0], actual[0]);
    }

    /// <summary>
    /// Verifies that the transformer stops pulling items from its source once
    /// <c>MaximumItemCount</c> is reached, rather than draining the whole source
    /// and discarding the surplus. A pull-based stage that over-reads holds
    /// upstream resources (connections, cursors) far longer than necessary.
    /// </summary>
    /// <remarks>
    /// The source is wrapped in a counter. Once enumeration completes with a
    /// limit of <c>M</c>, the wrapper must have observed at most <c>M + 1</c>
    /// pulls — the <c>+1</c> allows for the single read that discovers the limit
    /// has been reached.
    /// </remarks>
    [Fact]
    public async Task TransformAsync_stops_reading_source_at_MaximumItemCount_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        Assert.True(expected.Count >= 3, "CreateExpectedItems() must return at least 3 items.");

        var pulled = 0;

        async IAsyncEnumerable<TItem> CountingSourceAsync()
        {
            await Task.CompletedTask.ConfigureAwait(false);
            foreach (var item in expected)
            {
                pulled++;
                yield return item;
            }
        }

        sut.MaximumItemCount = expected.Count - 2;

        var actual = await sut.TransformAsync(CountingSourceAsync()).ToListAsync().ConfigureAwait(false);

        Assert.Equal(sut.MaximumItemCount, actual.Count);
        Assert.True
        (
            pulled <= sut.MaximumItemCount + 1,
            $"Expected at most {sut.MaximumItemCount + 1} source pulls but the source was read {pulled} times."
        );
    }

    /// <summary>
    /// Verifies that when <c>MaximumItemCount</c> exceeds the sequence length, all items
    /// are still yielded.
    /// </summary>
    [Fact]
    public async Task TransformAsync_yields_all_items_when_MaximumItemCount_exceeds_sequence_length_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        sut.MaximumItemCount = expected.Count + 100;

        var actual = await sut.TransformAsync(CreateInputItemsAsync()).ToListAsync().ConfigureAwait(false);

        Assert.Equal(expected, actual);
    }



    // ------------------------------------------------------------------
    // SkipItemCount
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>SkipItemCount</c> defaults to zero.
    /// </summary>
    [Fact]
    public void SkipItemCount_defaults_to_zero()
    {
        var sut = CreateSut();
        Assert.Equal(0, sut.SkipItemCount);
    }

    /// <summary>
    /// Verifies that setting <c>SkipItemCount</c> to a positive value succeeds.
    /// </summary>
    [Fact]
    public void SkipItemCount_can_be_set_to_positive_value()
    {
        var sut = CreateSut();
        sut.SkipItemCount = 5;
        Assert.Equal(5, sut.SkipItemCount);
    }

    /// <summary>
    /// Verifies that setting <c>SkipItemCount</c> to a negative value throws
    /// <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void SkipItemCount_set_to_negative_throws_ArgumentOutOfRangeException()
    {
        var sut = CreateSut();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.SkipItemCount = -1);
    }

    /// <summary>
    /// Verifies that the transformer skips the specified number of items before
    /// yielding.
    /// </summary>
    [Fact]
    public async Task TransformAsync_skips_items_up_to_SkipItemCount_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        Assert.True(expected.Count >= 3, "CreateExpectedItems() must return at least 3 items.");

        sut.SkipItemCount = 1;

        var actual = await sut.TransformAsync(CreateInputItemsAsync()).ToListAsync().ConfigureAwait(false);

        Assert.Equal(expected.Count - 1, actual.Count);
        Assert.Equal(expected.Skip(1).ToList(), actual);
    }

    /// <summary>
    /// Verifies that <c>CurrentItemCount</c> is zero on a freshly created transformer, before any
    /// transformation has run.
    /// </summary>
    [Fact]
    public void CurrentItemCount_defaults_to_zero()
    {
        var sut = CreateSut();

        Assert.Equal(0, sut.CurrentItemCount);
    }

    /// <summary>
    /// Verifies that <c>CurrentSkippedItemCount</c> is zero on a freshly created transformer,
    /// before any transformation has run.
    /// </summary>
    [Fact]
    public void CurrentSkippedItemCount_defaults_to_zero()
    {
        var sut = CreateSut();

        Assert.Equal(0, sut.CurrentSkippedItemCount);
    }

    /// <summary>
    /// Verifies that <c>CurrentSkippedItemCount</c> reflects the exact number of items skipped by
    /// <c>SkipItemCount</c> after a run.
    /// </summary>
    [Fact]
    public async Task TransformAsync_CurrentSkippedItemCount_reflects_the_number_of_items_skipped_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        Assert.True(expected.Count >= 3, "CreateExpectedItems() must return at least 3 items.");

        sut.SkipItemCount = 2;

        await sut.TransformAsync(CreateInputItemsAsync()).ToListAsync().ConfigureAwait(false);

        Assert.Equal(2, sut.CurrentSkippedItemCount);
    }

    /// <summary>
    /// Verifies that <c>CurrentErrorItemCount</c> is zero on a freshly created transformer, before
    /// any item has failed (Abstractions 0.18.0 error hook).
    /// </summary>
    [Fact]
    public void CurrentErrorItemCount_defaults_to_zero()
    {
        var sut = CreateSut();

        Assert.Equal(0, sut.CurrentErrorItemCount);
    }



    // ------------------------------------------------------------------
    // No over-read (issue #49)
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that once <c>MaximumItemCount</c> is reached the transformer stops pulling from
    /// its source rather than draining it — at most M+1 reads (the +1 discovers the limit).
    /// </summary>
    [Fact]
    public async Task TransformAsync_does_not_over_read_past_MaximumItemCount_Async()
    {
        var sut = CreateSut();
        sut.MaximumItemCount = 3;
        var counter = new PullCounter();

        await sut.TransformAsync(counter.CountAsync(CreateInputItemsAsync())).ToListAsync().ConfigureAwait(false);

        Assert.True(counter.Count <= 4, $"Expected at most 4 upstream reads, saw {counter.Count}.");
    }

    // Cancel() runs synchronously to cancel mid-enumeration; CancelAsync is net8.0+ only and
    // this base targets net462+.
#pragma warning disable CA1849, VSTHRD103
    /// <summary>
    /// Verifies that cancelling mid-run stops the transformer pulling from its source at the
    /// next check, rather than draining the already-available items.
    /// </summary>
    [Fact]
    public async Task TransformAsync_stops_reading_on_cancellation_Async()
    {
        var sut = CreateSut();
        using var cts = new CancellationTokenSource();
        var counter = new PullCounter();
        var seen = 0;

        try
        {
            var source = counter.CountAsync(CreateInputItemsAsync(), token: cts.Token);

            await foreach (var _ in sut.TransformAsync(source, cts.Token).ConfigureAwait(false))
            {
                if (++seen == 3)
                {
                    cts.Cancel();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected: cancellation is the scenario under test; the assertion below verifies the pull count.
        }

        Assert.True(counter.Count <= 4, $"Expected at most 4 upstream reads, saw {counter.Count}.");
    }
#pragma warning restore CA1849, VSTHRD103

    /// <summary>
    /// Verifies that a pre-cancelled token short-circuits the transformer before it pulls any
    /// item from its source.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_a_pre_cancelled_token_reads_nothing_Async()
    {
        var sut = CreateSut();
        var token = new CancellationToken(canceled: true);
        var counter = new PullCounter();

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            () => sut.TransformAsync(counter.CountAsync(CreateInputItemsAsync(), token: token), token).ToListAsync(token).AsTask()
        ).ConfigureAwait(false);

        Assert.Equal(0, counter.Count);
    }
}
