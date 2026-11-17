type: fix

`EtlPipelineProgress.LoadedItemCount` is now the loader's own count of loaded records (its `CurrentItemCount`) instead of the number of records the loader pulled, so it no longer over-reports when the loader skips records (`SkipItemCount`), discards one through an error policy, or stops at `MaximumItemCount`. An in-flight snapshot therefore reports the record that has just reached the loader as extracted but not yet loaded.
