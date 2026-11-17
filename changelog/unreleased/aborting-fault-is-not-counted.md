type: breaking

TestKit: an aborting fault in `FaultyExtractor`, `FaultyLoader` and `FaultyTransformer` no longer counts the failing item. After `ThrowAt(n, ex)` aborts a run, `CurrentItemCount` is the number of items processed before the failing one, the same value whether the abort comes from the fail-fast default or from an error policy that returns `ItemErrorAction.Abort`. Previously the fail-fast default counted the failing item and the policy path did not. Tests that assert `CurrentItemCount` after an unhandled `ThrowAt` need the expected value lowered by one.
