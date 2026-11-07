type: feature

Add `PropertySelector.Resolve`, which turns a property-selector lambda such as `x => x.Name` into its validated `PropertyInfo`, so the per-library schema builders share one selector rule.
