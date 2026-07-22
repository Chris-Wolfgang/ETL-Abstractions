# Globalization / CultureInfo invariance (#215)

The default PR test matrix runs under `en-US`. A whole class of bugs — the Turkish
dotted-I, German decimal-comma, Chinese collation, Arabic right-to-left digit
shaping, Japanese full-width digits — only surfaces under a non-`en-US` culture.
[`CultureInvarianceTests`](CultureInvarianceTests.cs) re-exercises the library's
observable behaviour under six hostile cultures (`en-US`, `tr-TR`, `de-DE`,
`zh-CN`, `ar-SA`, `ja-JP`), swapping **both** `CultureInfo.CurrentCulture` and
`CultureInfo.CurrentUICulture` per test and restoring them after (the `CultureScope`
helper), so a hostile culture can't leak into the next test.

Because these run as ordinary xunit theories, they execute in the existing PR test
job on **every** target framework — no separate CI matrix required.

## Allowlist of intentionally culture-sensitive public members

**None.** `Wolfgang.Etl.Abstractions` is orchestration and contracts:

- The base classes (`ExtractorBase`, `LoaderBase`, `TransformerBase`) pull items
  through `IAsyncEnumerable<T>` and track **integer** counters — no formatting or
  parsing.
- `Report` exposes numeric metrics (`ItemsPerSecond`, `PercentComplete`,
  `EstimatedRemaining`) computed with culture-invariant arithmetic.
- Field-level parsing/formatting (decimal separators, date formats, digit shaping)
  is the **format packages'** responsibility (`Wolfgang.Etl.Csv`, `.FixedWidth`, …),
  and is verified in those repos.

Every public member is therefore culture-invariant **by contract**, and the tests
assert it. If a future change makes a member intentionally culture-sensitive, add
it to this list with the rationale and give it its own culture-specific test rather
than an invariance assertion.

> Exception *message* text can format numbers via the current culture — that is UI
> text, not behaviour, and is deliberately not asserted here.
