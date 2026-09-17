type: breaking

`ExtractorBaseContractTests`, `LoaderBaseContractTests` and `TransformerBaseContractTests` now build the SUT through `CreateSut(int itemCount, int maximumItemCount, int skipItemCount, int reportingInterval)` (forward the three values into the options record unvalidated); `CreateSutOverSource` gains `int maximumItemCount`. A protected `CreateSut(int itemCount)` forwarder keeps derived tests compiling. Six contract tests are renamed from `*_set_to_*` to `*_configured_*`.
