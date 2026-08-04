namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// The result a derived <see cref="RetryContractTests{TSut}"/> reports back to the base after driving
/// its stage under a fault with a retry policy.
/// </summary>
public sealed class RetryOutcome
{
    /// <summary>
    /// Initializes a new <see cref="RetryOutcome"/>.
    /// </summary>
    /// <param name="succeeded">
    /// <see langword="true"/> if the run completed without the fault propagating out.
    /// </param>
    /// <param name="attemptCount">The number of worker invocations (initial try plus retries) made.</param>
    /// <param name="itemCount">The number of items the run produced.</param>
    public RetryOutcome(bool succeeded, int attemptCount, int itemCount)
    {
        Succeeded    = succeeded;
        AttemptCount = attemptCount;
        ItemCount    = itemCount;
    }



    /// <summary>Gets a value indicating whether the run completed without the fault propagating out.</summary>
    public bool Succeeded { get; }



    /// <summary>Gets the number of worker invocations (initial try plus retries) the run made.</summary>
    public int AttemptCount { get; }



    /// <summary>Gets the number of items the run produced.</summary>
    public int ItemCount { get; }
}
