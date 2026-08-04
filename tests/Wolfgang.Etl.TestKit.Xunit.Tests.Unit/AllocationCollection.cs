using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Serializes the allocation-budget tests: <see cref="AllocationBudgetContractTests{TSut}"/>
/// reads the process-wide GC allocation counter, so it must not run alongside other allocating
/// tests. Consumers reference this pattern with <c>[Collection("Allocation")]</c>.
/// </summary>
[CollectionDefinition("Allocation", DisableParallelization = true)]
public sealed class AllocationCollection
{
}
