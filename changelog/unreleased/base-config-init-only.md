type: breaking

`ReportingInterval`, `MaximumItemCount` and `SkipItemCount` on `ExtractorBase`, `LoaderBase` and `TransformerBase` are now `{ get; init; }`: configuration is fixed at construction (options record or object initializer) and can no longer be mutated mid-run. Source break for post-construction assignment (CS8852); binary break for every assembly that assigned them (`set` → `init` cannot be shimmed), so consumers must recompile.
