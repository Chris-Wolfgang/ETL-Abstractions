namespace Wolfgang.Etl.Abstractions.Tests.Unit.Models;

internal record EtlProgress
{
    public EtlProgress(long currentCount)
    {
        if (currentCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentCount), "Current count cannot be less than 0.");
        }

        CurrentCount = currentCount;
    }

    //public long CurrentCount { get; } = CurrentCount;
    public long CurrentCount { get; }

}