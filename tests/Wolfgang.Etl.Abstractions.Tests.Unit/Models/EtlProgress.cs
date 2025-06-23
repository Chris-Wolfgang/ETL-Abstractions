namespace Wolfgang.Etl.Abstractions.Tests.Unit.Models;

internal record EtlProgress(int CurrentCount)
{
    public int CurrentCount { get; } = CurrentCount;
}