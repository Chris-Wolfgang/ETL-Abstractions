type: feature

Add `IdempotentLoaderContractTests<TSut, TItem, TProgress>` and `IdempotentTransformerContractTests<TSut, TItem, TProgress>`, which read `CurrentItemCount` from the stage itself, so a loader or transformer with its own progress type no longer fails the idempotency contract with an `InvalidCastException`. The two-parameter bases are now `[Obsolete]`; they keep working for `Report`-progress stages.
