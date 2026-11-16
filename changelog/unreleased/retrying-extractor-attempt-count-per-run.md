type: fix

`RetryingExtractor.AttemptCount` now resets at the start of each run, as documented, so a second run on the same instance injects its transient faults again.
