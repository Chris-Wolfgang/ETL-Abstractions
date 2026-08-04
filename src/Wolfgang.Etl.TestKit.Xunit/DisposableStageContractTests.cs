using System;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests for the disposability guarantees the ETL
/// base classes gained in <c>Wolfgang.Etl.Abstractions</c> 0.14.0 (<see cref="IDisposable"/> /
/// <see cref="IAsyncDisposable"/>) and 0.17.0 (using a disposed stage throws
/// <see cref="ObjectDisposedException"/>).
/// </summary>
/// <typeparam name="TSut">
/// The stage type under test. Must be an <see cref="IDisposable"/> and
/// <see cref="IAsyncDisposable"/> — every <c>ExtractorBase</c>/<c>LoaderBase</c>/
/// <c>TransformerBase</c> is.
/// </typeparam>
/// <remarks>
/// <para>
/// Inherit from this class to verify that your stage honours the dispose contract: that its
/// public operation throws <see cref="ObjectDisposedException"/> once the stage has been
/// disposed (via either <see cref="IDisposable.Dispose"/> or
/// <see cref="IAsyncDisposable.DisposeAsync"/>), and that disposing twice is a harmless no-op.
/// </para>
/// <para>
/// Implement <see cref="CreateSut"/> to construct a fresh stage (used by the idempotent-dispose
/// tests), and <see cref="InvokeReportsObjectDisposedAsync"/> to construct a stage, optionally
/// dispose it, drive one public operation to completion, and report whether it threw
/// <see cref="ObjectDisposedException"/>. The operation lives in the derived class — rather than
/// receiving the stage as a parameter — so the contract stays stage-agnostic without forcing
/// null-argument validation on the override.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyExtractorDisposableTests
///     : DisposableStageContractTests&lt;MyExtractor&gt;
/// {
///     protected override MyExtractor CreateSut() => new MyExtractor(source);
///
///     protected override async Task&lt;bool&gt; InvokeReportsObjectDisposedAsync(bool disposeFirst, bool useAsyncDispose)
///     {
///         var sut = CreateSut();
///         if (disposeFirst)
///         {
///             if (useAsyncDispose) await sut.DisposeAsync(); else sut.Dispose();
///         }
///         try { await foreach (var _ in sut.ExtractAsync()) { } return false; }
///         catch (System.ObjectDisposedException) { return true; }
///     }
/// }
/// </code>
/// </example>
public abstract class DisposableStageContractTests<TSut>
    where TSut : class, IDisposable, IAsyncDisposable
{
    // ------------------------------------------------------------------
    // Factory / harness methods
    // ------------------------------------------------------------------

    /// <summary>
    /// Creates a fresh, undisposed stage under test. Used by the idempotent-dispose tests.
    /// </summary>
    protected abstract TSut CreateSut();

    /// <summary>
    /// Creates a fresh stage, optionally disposes it, drives one public operation to completion,
    /// and reports whether the operation threw <see cref="ObjectDisposedException"/>.
    /// </summary>
    /// <param name="disposeFirst">
    /// When <see langword="true"/>, dispose the stage before invoking the operation.
    /// </param>
    /// <param name="useAsyncDispose">
    /// When disposing, use <see cref="IAsyncDisposable.DisposeAsync"/> if <see langword="true"/>,
    /// otherwise <see cref="IDisposable.Dispose"/>. Ignored when <paramref name="disposeFirst"/>
    /// is <see langword="false"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the operation threw <see cref="ObjectDisposedException"/>;
    /// otherwise <see langword="false"/>.
    /// </returns>
    protected abstract Task<bool> InvokeReportsObjectDisposedAsync(bool disposeFirst, bool useAsyncDispose);



    // ------------------------------------------------------------------
    // Dispose contract
    // ------------------------------------------------------------------

    /// <summary>
    /// Control case: verifies the public operation does <em>not</em> throw
    /// <see cref="ObjectDisposedException"/> on an undisposed stage, so the after-dispose tests
    /// are not passing vacuously.
    /// </summary>
    [Fact]
    public async Task Public_operation_before_dispose_does_not_throw_ObjectDisposedException_Async()
    {
        Assert.False
        (
            await InvokeReportsObjectDisposedAsync(disposeFirst: false, useAsyncDispose: false).ConfigureAwait(false),
            "Expected no ObjectDisposedException before the stage is disposed."
        );
    }

    /// <summary>
    /// Verifies the public operation throws <see cref="ObjectDisposedException"/> after
    /// <see cref="IDisposable.Dispose"/>.
    /// </summary>
    [Fact]
    public async Task Public_operation_after_Dispose_throws_ObjectDisposedException_Async()
    {
        Assert.True
        (
            await InvokeReportsObjectDisposedAsync(disposeFirst: true, useAsyncDispose: false).ConfigureAwait(false),
            "Expected ObjectDisposedException after Dispose()."
        );
    }

    /// <summary>
    /// Verifies the public operation throws <see cref="ObjectDisposedException"/> after
    /// <see cref="IAsyncDisposable.DisposeAsync"/>.
    /// </summary>
    [Fact]
    public async Task Public_operation_after_DisposeAsync_throws_ObjectDisposedException_Async()
    {
        Assert.True
        (
            await InvokeReportsObjectDisposedAsync(disposeFirst: true, useAsyncDispose: true).ConfigureAwait(false),
            "Expected ObjectDisposedException after DisposeAsync()."
        );
    }

    /// <summary>
    /// Verifies that calling <see cref="IDisposable.Dispose"/> twice is a harmless no-op.
    /// </summary>
    [Fact]
    public void Dispose_is_idempotent()
    {
        var sut = CreateSut();
        sut.Dispose();
        sut.Dispose();
    }

    /// <summary>
    /// Verifies that calling <see cref="IAsyncDisposable.DisposeAsync"/> twice is a harmless
    /// no-op.
    /// </summary>
    [Fact]
    public async Task DisposeAsync_is_idempotent_Async()
    {
        var sut = CreateSut();
        await sut.DisposeAsync().ConfigureAwait(false);
        await sut.DisposeAsync().ConfigureAwait(false);
    }
}
