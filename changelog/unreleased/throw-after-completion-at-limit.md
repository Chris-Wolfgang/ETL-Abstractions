type: fix

`FaultyExtractor` and `FaultyTransformer` now throw the `ThrowAfterCompletion` exception when the run stops at `MaximumItemCount`, as `FaultyLoader` already did.
