namespace Wolfgang.Etl.Abstractions.Tests.Unit.Models;

public record EtlProgress(int CurrentCount)
{
    public int CurrentCount { get; } = CurrentCount;
}