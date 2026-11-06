type: fix

`StartedAt` on `ExtractorBase`, `TransformerBase` and `LoaderBase` is now published together with the start timestamp, so a progress report built on the timer thread can no longer read `StartedAt` as `0001-01-01` at the start of a run.
