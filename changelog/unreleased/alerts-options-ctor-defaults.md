type: internal

The six base-stage options constructors drop their dead `= null` default: the parameterless constructor already wins `new X()`, so the default could never be used (S3427). Binary signature unchanged, source-compatible; only the PublicAPI text changes.
