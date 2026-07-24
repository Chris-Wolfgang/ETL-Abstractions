namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// The action a stage takes for an item that failed to process — returned by a stage's
/// <c>OnItemError</c> policy and applied by the worker that called <c>HandleItemError</c>.
/// </summary>
public enum ItemErrorAction
{
    /// <summary>
    /// Re-throw the failure and stop the run. This is the default when no error policy is
    /// configured, preserving the fail-fast behaviour of a stage that does not opt in.
    /// </summary>
    Abort,

    /// <summary>
    /// Discard the failed item and continue with the next one. The stage's skipped-item count is
    /// incremented so the skip is never silent.
    /// </summary>
    Skip,
}
