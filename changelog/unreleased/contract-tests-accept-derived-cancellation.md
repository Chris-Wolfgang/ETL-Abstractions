type: fix

The six mid-stream cancellation contract tests in `Wolfgang.Etl.TestKit.Xunit` now accept any `OperationCanceledException` (including `TaskCanceledException`), like the rest of the cancellation contract tests.
