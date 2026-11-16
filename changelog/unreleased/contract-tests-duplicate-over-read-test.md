type: internal

`LoaderBaseContractTests.LoadAsync_does_not_over_read_past_MaximumItemCount_Async` and `TransformerBaseContractTests.TransformAsync_does_not_over_read_past_MaximumItemCount_Async` are no longer tests: each repeated the check its `..._stops_reading_source_at_MaximumItemCount_Async` neighbour makes. Both are marked `[Obsolete]`, so a derived test class reports one test fewer per base.
