# 3. Ship the test doubles and the contract-test base classes as two packages

## Status

Accepted

## Context

The kit provides two distinct kinds of test support:

1. **Test doubles** (`TestExtractor<T>`, `TestLoader<T>`, `TestTransformer<T>`,
   and their `Faulty*` variants) — concrete, framework-agnostic implementations
   of the Abstractions base classes, usable from any test runner (or none).
2. **Contract-test base classes** (`ExtractorBaseContractTests<…>`, etc.) —
   abstract xUnit `[Fact]`/`[Theory]` suites a downstream library subclasses to
   verify its own extractor/loader/transformer honours the Abstractions contract.

The second kind hard-depends on xUnit; the first does not. Bundling them would
force every consumer of the doubles to take an xUnit dependency, including
consumers on MSTest/NUnit or using the doubles outside a test project (e.g. in
benchmarks or samples).

## Decision

We will ship two packages: **`Wolfgang.Etl.TestKit`** (the doubles, no test-
framework dependency) and **`Wolfgang.Etl.TestKit.Xunit`** (the contract-test
base classes, depends on `Wolfgang.Etl.TestKit` + xUnit). The Xunit package
references the core package by ProjectReference in-repo and by NuGet version
downstream.

## Consequences

- Consumers who only need the doubles do not pay for xUnit.
- The xUnit-specific contract suites live behind an explicit, separately-versioned
  package boundary; a future MSTest/NUnit contract package can be added without
  touching the core.
- Both packages must be versioned and released together in lock-step for the
  Xunit package's dependency on the core to resolve; the release pipeline packs
  both.
