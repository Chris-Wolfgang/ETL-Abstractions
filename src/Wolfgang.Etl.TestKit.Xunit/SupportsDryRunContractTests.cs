using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests for any stage that supports dry-run
/// mode — one that can be configured to run its full pipeline while skipping the external
/// side effect that would otherwise mutate its destination or source.
/// </summary>
/// <remarks>
/// <para>
/// Inherit from this class to verify that your stage actually <em>skips its external side
/// effect</em> when configured for dry run, rather than merely exposing a setting. The ETL
/// base classes have no notion of dry run, so this contract is the only guarantee that a
/// stage truly performs no write/acknowledgement in dry-run mode.
/// </para>
/// <para>
/// The contract is behavioural and mechanism-agnostic: implement
/// <see cref="RunAndReportSideEffectAsync"/> to create your stage with the supplied dry-run
/// setting — normally through the <c>IsDryRun</c> member of its options record, per ADR-0009 —
/// drive it to completion (a loader's <c>LoadAsync</c>, an extractor's enumerated
/// <c>ExtractAsync</c>, and so on), and report whether the external side effect occurred.
/// How the value is applied is the implementer's business; this base makes no claim about it.
/// </para>
/// <para>
/// That the pipeline still runs fully in dry-run mode (progress counters advancing, items
/// enumerated) is covered by the stage's own base-class contract tests — for example
/// <see cref="LoaderBaseContractTests{TSut, TItem, TProgress}"/> — which a dry-run stage
/// should also inherit.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyLoaderDryRunContractTests : SupportsDryRunContractTests
/// {
///     protected override async Task&lt;bool&gt; RunAndReportSideEffectAsync(bool isDryRun)
///     {
///         var loader = new MyLoader(connectionString, new MyLoaderOptions { IsDryRun = isDryRun });
///         await loader.LoadAsync(CreateSourceItems().ToAsyncEnumerable());
///         return await CountRowsAsync(connectionString) &gt; 0;
///     }
/// }
/// </code>
/// </example>
public abstract class SupportsDryRunContractTests
{
    // ------------------------------------------------------------------
    // Harness
    // ------------------------------------------------------------------

    /// <summary>
    /// Creates a stage configured with the supplied dry-run setting, runs it through a
    /// complete execution — the same call a consumer would make — and reports whether the
    /// external side effect occurred (for example rows written, a message acknowledged, or a
    /// file moved).
    /// </summary>
    /// <param name="isDryRun">The dry-run setting to construct the stage with.</param>
    /// <returns><see langword="true"/> if the side effect occurred; otherwise <see langword="false"/>.</returns>
    protected abstract Task<bool> RunAndReportSideEffectAsync(bool isDryRun);



    // The ISupportDryRun interface, and with it this base's TSut type parameter, CreateSut()
    // factory and the three IsDryRun property tests, were removed in ETL-Abstractions #456.
    // Once dry run became construction-time configuration supplied through a stage's options
    // record (ADR-0009), the interface could no longer configure anything, and a fleet-wide
    // search found no other consumer. The property tests asserted a value round-trip; the
    // contract that matters — a stage configured for dry run skips its side effect, and one
    // configured normally performs it — is the two behavioural tests below.



    // ------------------------------------------------------------------
    // Dry-run behaviour contract
    // ------------------------------------------------------------------

    /// <summary>
    /// Control case: verifies that when dry run is <see langword="false"/>, a full run
    /// <em>does</em> perform the external side effect. Without this, a stage that never
    /// produces a side effect could pass the dry-run test vacuously.
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
    /// Verifies that when dry run is <see langword="true"/>, a full run completes but the
    /// external side effect is <em>skipped</em>.
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
