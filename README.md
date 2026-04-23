# Wolfgang.Etl.Abstractions

Interface and base classes for building ETLs

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Multi--Targeted-purple.svg)](https://dotnet.microsoft.com/)
[![GitHub](https://img.shields.io/badge/GitHub-Repository-181717?logo=github)](https://github.com/Chris-Wolfgang/ETL-Abstractions)

---

## 📦 Installation

```bash
dotnet add package Wolfgang.Etl.Abstractions
```

**NuGet Package:** Available on NuGet.org

---

## 📄 License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

## 📚 Documentation

- **GitHub Repository:** [https://github.com/Chris-Wolfgang/ETL-Abstractions](https://github.com/Chris-Wolfgang/ETL-Abstractions)
- **API Documentation:** https://Chris-Wolfgang.github.io/ETL-Abstractions/
- **Formatting Guide:** [README-FORMATTING.md](docs/README-FORMATTING.md)
- **Contributing Guide:** [CONTRIBUTING.md](CONTRIBUTING.md)

---

## 🚀 Quick Start

```csharp
// Create a custom extractor by inheriting from ExtractorBase
public class MyExtractor : ExtractorBase<string, Report>
{
    protected override async IAsyncEnumerable<string> ExtractWorkerAsync(
        [EnumeratorCancellation] CancellationToken token)
    {
        // Yield items from your data source
        yield return "item1";
        IncrementCurrentItemCount();
    }

    protected override Report CreateProgressReport()
    {
        return new Report(CurrentItemCount);
    }
}

// Use the extractor
var extractor = new MyExtractor();
await foreach (var item in extractor.ExtractAsync())
{
    Console.WriteLine(item);
}
```

---

## 🔗 Fluent Pipeline API

Compose an extractor, zero or more transformers, and a loader into a single executable
pipeline with a fluent, strongly-typed chain. The compiler enforces that each stage's
output type matches the next stage's input — mismatches surface as build errors, not
runtime exceptions.

```csharp
await Pipeline
    .Extract(csvExtractor).WithProgress(extractProgress)
    .Transform(parseRecord)
    .Transform(enrichFromApi).WithProgress(enrichProgress)
    .Load(sqlLoader).WithProgress(loadProgress)
    .WithName("nightly-import")
    .RunAsync(cancellationToken);
```

**Key properties:**

- **Compile-time type safety** — each `.Transform(...)` is constrained to accept the
  previous stage's output type; `.Load(...)` must match the final stage.
- **Opt-in progress** — `.WithProgress(...)` is only available on stages whose underlying
  extractor/transformer/loader implements the progress-capable interface. Calling it on
  a stage that doesn't support progress is a compile error.
- **One-shot execution** — calling `RunAsync` a second time on the same pipeline throws
  `InvalidOperationException`. Construct a new pipeline per run.
- **Raw exception propagation** — stage exceptions bubble up unchanged; stage instances
  retain their `CurrentItemCount` and other state for post-mortem inspection.
- **Caller-owned lifetimes** — the pipeline never disposes the stages you hand it.

The pipeline is syntactic sugar over the existing `IAsyncEnumerable` composition — there
is no new runtime behavior, no buffering, and no additional allocations per item.

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| Async Streaming | Built on `IAsyncEnumerable<T>` for efficient, non-blocking data pipelines |
| Fluent Pipeline | `Pipeline.Extract(...).Transform(...).Load(...).RunAsync()` with compile-time stage typing |
| Progress Reporting | Built-in `IProgress<T>` support with configurable reporting intervals |
| Cancellation | Full `CancellationToken` support across all operations |
| Multi-TFM | Targets .NET Framework 4.6.2+, .NET Standard 2.0+, and .NET 5.0–10.0 |
| Skip & Limit | `SkipItemCount` and `MaximumItemCount` for partial extraction/loading |
| Thread Safety | `Interlocked`-based counters for safe concurrent progress tracking |

---

## 🎯 Target Frameworks

| Framework | Versions |
|-----------|----------|
| .Net Framework | .net 4.6.2, .net 4.7.0, .net 4.7.1, .net 4.7.2, .net 4.8, .net 4.8.1 | 
| .Net Core | |
| .Net | .net 5.0, .net 6.0, .net 7.0, .net 8.0, .net 9.0, .net 10.0 |

---

## 🔍 Code Quality & Static Analysis

This project enforces **strict code quality standards** through **7 specialized analyzers** and custom async-first rules:

### Analyzers in Use

1. **Microsoft.CodeAnalysis.NetAnalyzers** - Built-in .NET analyzers for correctness and performance
2. **Roslynator.Analyzers** - Advanced refactoring and code quality rules
3. **AsyncFixer** - Async/await best practices and anti-pattern detection
4. **Microsoft.VisualStudio.Threading.Analyzers** - Thread safety and async patterns
5. **Microsoft.CodeAnalysis.BannedApiAnalyzers** - Prevents usage of banned synchronous APIs
6. **Meziantou.Analyzer** - Comprehensive code quality rules
7. **SonarAnalyzer.CSharp** - Industry-standard code analysis

### Async-First Enforcement

This library uses **`BannedSymbols.txt`** to prohibit synchronous APIs and enforce async-first patterns:

**Blocked APIs Include:**
- ❌ `Task.Wait()`, `Task.Result` - Use `await` instead
- ❌ `Thread.Sleep()` - Use `await Task.Delay()` instead
- ❌ Synchronous file I/O (`File.ReadAllText`) - Use async versions
- ❌ Synchronous stream operations - Use `ReadAsync()`, `WriteAsync()`
- ❌ `Parallel.For/ForEach` - Use `Task.WhenAll()` or `Parallel.ForEachAsync()`
- ❌ Obsolete APIs (`WebClient`, `BinaryFormatter`)

**Why?** To ensure all code is **truly async** and **non-blocking** for optimal performance in async contexts.

---

## 🛠️ Building from Source

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later
- Optional: [PowerShell Core](https://github.com/PowerShell/PowerShell) for formatting scripts

### Build Steps

```bash
# Clone the repository
git clone https://github.com/Chris-Wolfgang/ETL-Abstractions.git
cd ETL-Abstractions

# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release

# Run code formatting (PowerShell Core)
pwsh ./scripts/format.ps1
```

### Code Formatting

This project uses `.editorconfig` and `dotnet format`:

```bash
# Format code
dotnet format

# Verify formatting (as CI does)
dotnet format --verify-no-changes
```

See [README-FORMATTING.md](docs/README-FORMATTING.md) for detailed formatting guidelines.

### Building Documentation

This project uses [DocFX](https://dotnet.github.io/docfx/) to generate API documentation:

```bash
# Install DocFX (one-time setup)
dotnet tool install -g docfx

# Generate API metadata and build documentation
cd docfx_project
docfx metadata  # Extract API metadata from source code
docfx build     # Build HTML documentation

# Documentation is generated in the docs/ folder at the repository root
```

The documentation is automatically built and deployed to GitHub Pages when changes are pushed to the `main` branch.

**Local Preview:**
```bash
# Serve documentation locally (with live reload)
cd docfx_project
docfx build --serve

# Open http://localhost:8080 in your browser
```

**Documentation Structure:**
- `docfx_project/` - DocFX configuration and source files
- `docs/` - Generated HTML documentation (published to GitHub Pages)
- `docfx_project/index.md` - Main landing page content
- `docfx_project/docs/` - Additional documentation articles
- `docfx_project/api/` - Auto-generated API reference YAML files

---

## 🤝 Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for:
- Code quality standards
- Build and test instructions
- Pull request guidelines
- Analyzer configuration details

---


## 🙏 Acknowledgments

- Built with [.NET](https://dotnet.microsoft.com/) and the async streaming APIs
- Static analysis powered by Roslyn, Roslynator, Meziantou, and SonarAnalyzer
- Documentation generated with [DocFX](https://dotnet.github.io/docfx/)

