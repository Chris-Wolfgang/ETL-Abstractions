#if NET6_0_OR_GREATER
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// Serializes <see cref="AllocationRegressionTests"/>: it reads the process-wide GC allocation
/// counter, so it must not run alongside other allocating tests.
/// </summary>
[CollectionDefinition("Allocation", DisableParallelization = true)]
public sealed class AllocationCollection
{
}
#endif
