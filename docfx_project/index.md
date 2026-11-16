---
_layout: landing
---

# Wolfgang.Etl.Abstractions Documentation

Welcome to the Wolfgang.Etl.Abstractions documentation. This site contains comprehensive guides, API reference, and examples to help you get started.

## Quick Links

- [Getting Started](docs/getting-started.md) - Learn the basics
- [API Reference](xref:Wolfgang.Etl.Abstractions) - Complete API documentation
- [GitHub Repository](https://github.com/Chris-Wolfgang/ETL-Abstractions) - View source code

## About Wolfgang.Etl.Abstractions

Interface and base classes for building ETLs

## Packages

This repository ships four packages, versioned and released together:

| Package | What it is |
|---|---|
| [`Wolfgang.Etl.Abstractions`](https://github.com/Chris-Wolfgang/ETL-Abstractions/blob/main/src/Wolfgang.Etl.Abstractions/README.md) | The core `ExtractorBase` / `LoaderBase` / `TransformerBase` types, the `EtlPipeline` composer, progress reporting, per-item error handling, and resilience seams. |
| [`Wolfgang.Etl.ErrorPolicies`](https://github.com/Chris-Wolfgang/ETL-Abstractions/blob/main/src/Wolfgang.Etl.ErrorPolicies/README.md) | Ready-made item-error policies (skip, log, dead-letter) to assign to a stage's `ErrorPolicy`. |
| [`Wolfgang.Etl.TestKit`](https://github.com/Chris-Wolfgang/ETL-Abstractions/blob/main/src/Wolfgang.Etl.TestKit/README.md) | Test doubles and harnesses: `TestExtractor` / `TestLoader` / `TestTransformer`, fault injection, a deterministic clock and progress timer. |
| [`Wolfgang.Etl.TestKit.Xunit`](https://github.com/Chris-Wolfgang/ETL-Abstractions/blob/main/src/Wolfgang.Etl.TestKit.Xunit/README.md) | xUnit contract-test base classes that verify a component against the behavioural contract of its base type. |

## Installation

```bash
dotnet add package Wolfgang.Etl.Abstractions
```

Add `Wolfgang.Etl.ErrorPolicies` for the ready-made error policies, and `Wolfgang.Etl.TestKit` /
`Wolfgang.Etl.TestKit.Xunit` to your test projects.

## Documentation Sections

### 📖 [Documentation](docs/getting-started.md)
Step-by-step guides and tutorials to help you use Wolfgang.Etl.Abstractions effectively.

### 📚 [API Reference](xref:Wolfgang.Etl.Abstractions)
Complete API documentation automatically generated from source code XML comments.

## Additional Resources

- [Contributing Guidelines](https://github.com/Chris-Wolfgang/ETL-Abstractions/blob/main/CONTRIBUTING.md)
- [Code of Conduct](https://github.com/Chris-Wolfgang/ETL-Abstractions/blob/main/CODE_OF_CONDUCT.md)
- [License](https://github.com/Chris-Wolfgang/ETL-Abstractions/blob/main/LICENSE)

---

*Documentation built with [DocFX](https://dotnet.github.io/docfx/)*

