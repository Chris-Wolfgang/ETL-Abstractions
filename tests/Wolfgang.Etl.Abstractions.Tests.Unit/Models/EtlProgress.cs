namespace Wolfgang.Etl.Abstractions.Tests.Unit.Models;

public record EtlProgress
{
    public EtlProgress(int currentItemCount)
    {
        if (currentItemCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentItemCount), "Current item count cannot be less than 0.");
        }

        CurrentItemCount = currentItemCount;
    }



    public int CurrentItemCount { get; }

}
