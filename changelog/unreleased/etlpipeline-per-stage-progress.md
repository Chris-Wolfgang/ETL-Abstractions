type: feature

`EtlPipeline` reports each stage's own typed progress: `From(extractor, progress)`, `Through(transformer, progress)` and `To(loader, progress)` are the counterparts of `WithProgress` on the fluent `Pipeline` builder, and `docs/migrating-pipeline-to-etlpipeline.md` maps every `Pipeline` call shape to its `EtlPipeline` equivalent.
