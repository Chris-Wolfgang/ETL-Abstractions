type: internal

Code-scanning clean-up: the six base-stage options constructors drop their dead `= null` default (the parameterless constructor already wins that call shape — S3427; the API text changes, the binary signature does not); constructor `<see cref>`s in the TestKit doubles use the `Type{T}(…)` form both Roslyn and InspectCode resolve (26 × CS1580); the deliberate `[Obsolete]` setters are excluded from S1133 per file; polyfill namespaces and a handful of test-code findings (redundant usings/casts, `using` initialisers, override defaults, namespaces) fixed.
