type: docs

XML docs now match the code: `ReportingInterval` is in milliseconds, `ItemErrorAction.Skip` and `EtlPipelineProgress.ErrorItemCount` describe the error-item count (summed across every stage), `Dispose`/`DisposeAsync` say the stage then throws `ObjectDisposedException`, and the `MaximumItemCount`/`SkipItemCount` examples call the `Increment*` helpers.
