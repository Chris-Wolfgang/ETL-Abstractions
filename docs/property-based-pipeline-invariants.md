# Property-based pipeline invariants

Example-based tests check the inputs someone thought of. A property-based test states an invariant
and lets a generator try many inputs, including empty sequences, faults on the first and last item,
and faults on every item. This repository checks the pipeline invariants below with
[CsCheck](https://github.com/AnthonyLloyd/CsCheck) in
`tests/Wolfgang.Etl.TestKit.Tests.Fuzz/PipelineInvariantTests.cs` (#373).

## Dependency rule

The generator library is a **test-project-only** dependency, the same rule as Verify: no shipped
package (`Wolfgang.Etl.Abstractions`, `Wolfgang.Etl.ErrorPolicies`, `Wolfgang.Etl.TestKit`,
`Wolfgang.Etl.TestKit.Xunit`) references CsCheck or any other generator. A downstream library that
wants the same checks references the generator from its own test project, as in the example below.

## The invariants

For any input sequence, any set of fault positions in the extractor, and any set of fault positions
in the loader, with both stages set to skip their faults (`SkipErrors()`):

1. **Loaded set.** The loader receives the input minus the extractor's fault positions, then keeps
   that stream minus its own fault positions. Order is preserved.
2. **Counter reconciliation.** Each stage's `CurrentItemCount` equals the number of items it saw
   (a fault fires after the item is counted, so faulted items are included); its
   `CurrentErrorItemCount` equals its number of faults; its `CurrentSkippedItemCount` stays 0,
   because an error skip is not a `SkipItemCount` skip.
3. **Every fault reported once.** Each stage's `CapturedErrors` holds exactly one entry per injected
   fault, with the 1-based item number of that position and the injected exception instance.
4. **Pipeline totals.** The final `EtlPipelineProgress` reports `ExtractedItemCount` = input minus
   extractor faults, `LoadedItemCount` = `ExtractedItemCount`, and `ErrorItemCount` = the faults of
   both stages.

## Running them

They run with the rest of the test suite. `FUZZ_ITER` sets the number of generated cases (default
1000); the scheduled fuzz workflow raises it. On a failure CsCheck shrinks the case to a minimal input
and prints a seed that replays it.

```bash
FUZZ_ITER=20000 dotnet test tests/Wolfgang.Etl.TestKit.Tests.Fuzz --filter PipelineInvariantTests
```

## Example: the same check in a downstream library

```csharp
using System.Linq;
using CsCheck;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit;
using Xunit;

public class CsvPipelineInvariantTests
{
    [Fact]
    public void Every_record_the_extractor_does_not_fault_on_is_loaded_in_order()
    {
        Gen.Int.List[0, 30]
            .SelectMany(items => Gen.Bool.Array[items.Count].Select(faults => (items, faults)))
            .Sample(c =>
            {
                var extractor = new FaultyExtractor<int>(c.items).SkipErrors();
                for (var i = 0; i < c.faults.Length; i++)
                {
                    if (c.faults[i])
                    {
                        extractor.ThrowAt(i, new FormatException());
                    }
                }

                var loader = new TestLoader<int>(collectItems: true);
                EtlPipeline
                    .Create()
                    .From(extractor)
                    .To(loader)
                    .RunAsync()
                    .GetAwaiter()
                    .GetResult();

                Assert.Equal(c.items.Where((_, i) => !c.faults[i]), loader.GetCollectedItems());
            });
    }
}
```

Replace `FaultyExtractor` with your own extractor and the fault mask with the malformed inputs your
format can produce, and the same shape checks that your extractor never drops or reorders a good record.
