namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// The observable outcome of running a stage over a scenario containing exactly one failing
/// item, used by <see cref="ErrorHandlingContractTests{TSut}"/>.
/// </summary>
public sealed class ErrorHandlingOutcome
{
    /// <summary>
    /// Initialises a new <see cref="ErrorHandlingOutcome"/>.
    /// </summary>
    /// <param name="aborted">
    /// <see langword="true"/> if the run re-threw the item's failure (aborted); otherwise
    /// <see langword="false"/> (the run completed).
    /// </param>
    /// <param name="itemCount">The stage's <c>CurrentItemCount</c> after the run.</param>
    /// <param name="errorItemCount">The stage's <c>CurrentErrorItemCount</c> after the run.</param>
    /// <param name="skippedItemCount">The stage's <c>CurrentSkippedItemCount</c> after the run.</param>
    public ErrorHandlingOutcome(bool aborted, int itemCount, int errorItemCount, int skippedItemCount)
    {
        Aborted          = aborted;
        ItemCount        = itemCount;
        ErrorItemCount   = errorItemCount;
        SkippedItemCount = skippedItemCount;
    }

    /// <summary>Whether the run re-threw the failing item's exception (aborted the run).</summary>
    public bool Aborted { get; }

    /// <summary>The stage's <c>CurrentItemCount</c> (successfully processed items) after the run.</summary>
    public int ItemCount { get; }

    /// <summary>The stage's <c>CurrentErrorItemCount</c> (error-discarded items) after the run.</summary>
    public int ErrorItemCount { get; }

    /// <summary>The stage's <c>CurrentSkippedItemCount</c> (intentionally skipped items) after the run.</summary>
    public int SkippedItemCount { get; }
}
