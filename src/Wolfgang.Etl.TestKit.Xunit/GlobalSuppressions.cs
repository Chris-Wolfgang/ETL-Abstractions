// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// CA1707: Identifiers should not contain underscores
// Contract test method names intentionally use underscores to describe
// the behaviour under test (e.g. ExtractAsync_returns_non_null_sequence).
// This is standard xUnit naming convention and is by design.
[assembly: SuppressMessage(
    "Naming",
    "CA1707:Identifiers should not contain underscores",
    Justification = "Contract test method names use underscores by design (xUnit naming convention).",
    Scope = "module")]
