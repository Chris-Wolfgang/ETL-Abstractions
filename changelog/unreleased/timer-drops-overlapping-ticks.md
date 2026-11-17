type: fix

The built-in progress timer now drops a tick that arrives while the previous one is still reporting, instead of queuing it: a progress sink slower than `ReportingInterval` no longer ties up thread-pool threads or delays the end of the run.
