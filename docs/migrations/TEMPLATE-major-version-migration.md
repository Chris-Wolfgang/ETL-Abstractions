# Migrating from vX to vY

> Copy this template to `vX-to-vY.md` during release prep for a major version
> bump, fill in each section, and link it from the GitHub Release notes. Delete
> any section that does not apply (but prefer "None" over deletion so readers know
> it was considered).

## Summary

One paragraph: who is affected, roughly how much work the upgrade is, and whether
a compatibility shim exists.

## Breaking-change inventory

| API | Change | Replacement |
| --- | --- | --- |
| `OldType.OldMember` | removed / renamed / behaviour change | `NewType.NewMember` |

## Before / after

```csharp
// Before (vX)
```

```csharp
// After (vY)
```

Repeat per breaking change that needs a code edit.

## Behavioural changes (no signature change)

Changes that compile unchanged but behave differently at runtime (default-value
changes, nullability flips, parsing-tolerance changes). These are the dangerous
ones — call each out explicitly.

## Deprecation timeline

- **vX**: member marked `[Obsolete]` (warning).
- **vY**: member removed.

State when deprecated members were first warned about and when they were removed.

## Verifying the upgrade

How a consumer confirms the migration succeeded (build clean, tests green, ABI
check via the release `api-compat` gate).
