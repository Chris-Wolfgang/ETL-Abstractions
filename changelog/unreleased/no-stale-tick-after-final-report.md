type: fix

Disposing the built-in progress timer now waits for a tick that is already reporting, so a stage's final progress report can no longer be followed by a stale periodic one.
