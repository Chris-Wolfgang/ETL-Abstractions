using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests for any type that implements
/// <see cref="ISupportDryRun"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must implement <see cref="ISupportDryRun"/>.
/// </typeparam>
/// <remarks>
/// <para>
/// Inherit from this class to verify that your stage honours the
/// <see cref="ISupportDryRun"/> contract — that it actually <em>skips its external
/// side effect</em> when <see cref="ISupportDryRun.IsDryRun"/> is <see langword="true"/>,
/// rather than merely exposing the property. The interface is opt-in and the ETL base
/// classes deliberately do not implement it, so this contract is the only guarantee
/// that an implementer truly performs no write/acknowledgement in dry-run mode.
/// </para>
/// <para>
/// The base is stage-agnostic: implement <see cref="RunAndReportSideEffectAsync"/> to
/// create your stage with the supplied dry-run setting, drive it to completion (a loader's
/// <c>LoadAsync</c>, an extractor's enumerated <c>ExtractAsync</c>, and so on), and report
/// whether the external side effect occurred.
/// </para>
/// <para>
/// This contract covers <see cref="ISupportDryRun.IsDryRun"/> only. That the pipeline
/// still runs fully in dry-run mode (progress counters advancing, items enumerated) is
/// covered by the stage's own base-class contract tests — for example
/// <see cref="LoaderBaseContractTests{TSut, TItem, TProgress}"/> — which a dry-run loader
/// should also inherit.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyLoaderDryRunContractTests
///     : SupportsDryRunContractTests&lt;MyLoader&gt;
/// {
///     protected override MyLoader CreateSut() => new MyLoader(connectionString);
///
///     protected override async Task&lt;bool&gt; RunAndReportSideEffectAsync(bool isDryRun)
///     {
///         var loader = new MyLoader(connectionString) { IsDryRun = isDryRun };
///         await loader.LoadAsync(CreateSourceItems().ToAsyncEnumerable());
///         return await CountRowsAsync(connectionString) &gt; 0;
///     }
/// }
/// </code>
/// </example>
public abstract class SupportsDryRunContractTests<TSut>
    where TSut : class, ISupportDryRun
{
    // ------------------------------------------------------------------
    // Factory / harness methods
    // ------------------------------------------------------------------

    /// <summary>
    /// Creates the system under test with <see cref="ISupportDryRun.IsDryRun"/> at its
    /// default value. Used by the property-contract tests.
    /// </summary>
    protected abstract TSut CreateSut();

    /// <summary>
    /// Creates a stage with <see cref="ISupportDryRun.IsDryRun"/> set to
    /// <paramref name="isDryRun"/>, runs it through a complete execution — the same call a
    /// consumer would make — and reports whether the external side effect occurred (for
    /// example rows written, a message acknowledged, or a file moved).
    /// </summary>
    /// <param name="isDryRun">The value to assign to <see cref="ISupportDryRun.IsDryRun"/>.</param>
    /// <returns><see langword="true"/> if the side effect occurred; otherwise <see langword="false"/>.</returns>
    protected abstract Task<bool> RunAndReportSideEffectAsync(bool isDryRun);



    // ------------------------------------------------------------------
    // IsDryRun property contract
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="ISupportDryRun.IsDryRun"/> defaults to <see langword="false"/>.
    /// </summary>
    [Fact]
    public void IsDryRun_defaults_to_false()
    {
        var sut = CreateSut();
        Assert.False(sut.IsDryRun);
    }

    /// <summary>
    /// Verifies that <see cref="ISupportDryRun.IsDryRun"/> can be set to <see langword="true"/>.
    /// </summary>
    [Fact]
    public void IsDryRun_can_be_set_to_true()
    {
        var sut = CreateSut();
        sut.IsDryRun = true;
        Assert.True(sut.IsDryRun);
    }

    /// <summary>
    /// Verifies that <see cref="ISupportDryRun.IsDryRun"/> can be set back to
    /// <see langword="false"/> after being set to <see langword="true"/>.
    /// </summary>
    [Fact]
    public void IsDryRun_can_be_set_back_to_false()
    {
        var sut = CreateSut();
        sut.IsDryRun = true;
        sut.IsDryRun = false;
        Assert.False(sut.IsDryRun);
    }



    // ------------------------------------------------------------------
    // Dry-run behaviour contract
    // ------------------------------------------------------------------

    /// <summary>
    /// Control case: verifies that when <see cref="ISupportDryRun.IsDryRun"/> is
    /// <see langword="false"/>, a full run <em>does</em> perform the external side effect.
    /// Without this, a stage that never produces a side effect could pass the dry-run
    /// test vacuously.
    /// </summary>
    [Fact]
    public async Task When_IsDryRun_is_false_side_effect_occurs_Async()
    {
        Assert.True
        (
            await RunAndReportSideEffectAsync(isDryRun: false).ConfigureAwait(false),
            "Expected the side effect to occur when IsDryRun is false."
        );
    }

    /// <summary>
    /// Verifies that when <see cref="ISupportDryRun.IsDryRun"/> is <see langword="true"/>,
    /// a full run completes but the external side effect is <em>skipped</em>.
    /// </summary>
    [Fact]
    public async Task When_IsDryRun_is_true_side_effect_is_skipped_Async()
    {
        Assert.False
        (
            await RunAndReportSideEffectAsync(isDryRun: true).ConfigureAwait(false),
            "Expected the side effect to be skipped when IsDryRun is true."
        );
    }
}
