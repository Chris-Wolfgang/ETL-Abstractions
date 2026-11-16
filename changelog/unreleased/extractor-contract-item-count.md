type: fix

The extractor contract test bases no longer require exactly five expected items: the SUT is created with as many items as `CreateExpectedItems()` returns (at least three), and the docs and examples now say so consistently.
