type: fix

`LoaderBase.LoadAsync` overloads without progress now return a faulted task when the worker throws before its first `await`, matching the progress overloads, instead of throwing synchronously.
