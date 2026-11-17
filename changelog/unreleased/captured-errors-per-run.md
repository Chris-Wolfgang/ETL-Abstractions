type: fix

`CapturedErrors` on `FaultyExtractor`, `FaultyTransformer` and `FaultyLoader` is now cleared at the start of each run, like the stage counters, so a reused double reports only the errors of its latest run.
