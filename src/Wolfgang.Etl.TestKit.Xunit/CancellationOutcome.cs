namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// The result a derived <see cref="CancellationContractTests{TSut}"/> reports back to the base after
/// driving its stage under cancellation.
/// </summary>
public sealed class CancellationOutcome
{
    /// <summary>
    /// Initializes a new <see cref="CancellationOutcome"/>.
    /// </summary>
    /// <param name="canceled">
    /// <see langword="true"/> if the run threw <see cref="System.OperationCanceledException"/>
    /// (or a derived type such as <see cref="System.Threading.Tasks.TaskCanceledException"/>).
    /// </param>
    /// <param name="processedItemCount">
    /// The number of items the stage fully processed before it stopped.
    /// </param>
    public CancellationOutcome(bool canceled, int processedItemCount)
    {
        Canceled          = canceled;
        ProcessedItemCount = processedItemCount;
    }



    /// <summary>
    /// Gets a value indicating whether the run threw
    /// <see cref="System.OperationCanceledException"/> (or a derived type).
    /// </summary>
    public bool Canceled { get; }



    /// <summary>
    /// Gets the number of items the stage fully processed before it stopped.
    /// </summary>
    public int ProcessedItemCount { get; }
}
