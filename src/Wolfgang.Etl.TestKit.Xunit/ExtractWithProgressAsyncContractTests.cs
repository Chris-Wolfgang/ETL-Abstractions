using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests for any type that implements
/// <see cref="IExtractWithProgressAsync{TSource, TProgress}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must implement
/// <see cref="IExtractWithProgressAsync{TItem, TProgress}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the extractor yields.</typeparam>
/// <typeparam name="TProgress">The type of the progress report.</typeparam>
/// <remarks>
/// <para>
/// Inherit from this class and implement the abstract factory methods to verify that
/// your extractor correctly satisfies the
/// <see cref="IExtractWithProgressAsync{TSource, TProgress}"/> contract.
/// </para>
/// <para>
/// These tests verify the <c>ArgumentNullException</c> guard on the <c>progress</c>
/// parameter and that extraction still yields the expected items when progress reporting
/// is enabled.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyExtractorProgressContractTests
///     : ExtractWithProgressAsyncContractTests&lt;MyExtractor, MyRecord, MyProgress&gt;
/// {
///     protected override MyExtractor CreateSut(int itemCount) =>
///         new MyExtractor(GetTestData(), itemCount);
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateExpectedItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c") };
/// }
/// </code>
/// </example>
public abstract class ExtractWithProgressAsyncContractTests<TSut, TItem, TProgress>
    where TSut : IExtractWithProgressAsync<TItem, TProgress>
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
    // Tests
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
            // The exception is thrown when the method is called, before enumeration starts.
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
}
