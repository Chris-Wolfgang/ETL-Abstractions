type: fix

`FaultyExtractor`, `FaultyLoader` and `FaultyTransformer` now apply an `ErrorPolicy` supplied through the options record to injected `ThrowAt` faults, not only one set with `SkipErrors` / `HandleErrorsWith`.
