type: fix

`EtlPipeline.RunAsync(progress)` no longer allocates and reports an `EtlPipelineProgress` snapshot for every record: it reports the first record, then at most one snapshot every 100 ms, then the exact final snapshot when the run completes.
