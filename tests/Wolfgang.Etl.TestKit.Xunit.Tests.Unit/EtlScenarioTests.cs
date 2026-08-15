using System;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

public class EtlScenarioTests
{
    [Fact]
    public async Task Clean_run_loads_every_item_with_no_errors_Async()
    {
        await EtlScenario
            .From(1, 2, 3)
            .RunAndAssertAsync(new[] { 1, 2, 3 }, expectedErrors: 0)
            ;
    }



    [Fact]
    public async Task Skipped_extractor_fault_drops_the_item_and_counts_one_error_Async()
    {
        await EtlScenario
            .From(1, 2, 3, 4)
            .WithExtractorFault(index: 2, new FormatException("bad row"))
            .RunAndAssertAsync(new[] { 1, 2, 4 }, expectedErrors: 1)
            ;
    }



    [Fact]
    public async Task Skipped_loader_fault_drops_the_item_and_counts_one_error_Async()
    {
        await EtlScenario
            .From(1, 2, 3, 4)
            .WithLoaderFault(index: 1, new FormatException("bad write"))
            .RunAndAssertAsync(new[] { 1, 3, 4 }, expectedErrors: 1)
            ;
    }



    [Fact]
    public async Task Transform_stage_is_applied_between_source_and_sink_Async()
    {
        await EtlScenario
            .From(1, 2, 3)
            .Through(new TestTransformer<int>())
            .RunAndAssertAsync(new[] { 1, 2, 3 }, expectedErrors: 0)
            ;
    }



    [Fact]
    public async Task Non_skipped_fault_propagates_as_the_expected_exception_Async()
    {
        await EtlScenario
            .From(1, 2, 3)
            .WithExtractorFault(index: 1, new InvalidOperationException("boom"), skip: false)
            .RunAndAssertThrowsAsync<InvalidOperationException>()
            ;
    }
}
