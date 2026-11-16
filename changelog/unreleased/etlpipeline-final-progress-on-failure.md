type: fix

`EtlPipeline` runs now report the final `EtlPipelineProgress` snapshot, with the exact counts, when the run fails or is cancelled, not only when it completes.
