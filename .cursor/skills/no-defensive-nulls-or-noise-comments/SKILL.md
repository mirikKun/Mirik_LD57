---
name: no-defensive-nulls-or-noise-comments
description: >-
  Project coding style: do not add defensive null checks when a reference must
  always be assigned, and do not write comments except for complex methods or
  fields. Use when writing or editing C# in this Unity project, adding
  validation, guarding against NullReferenceException, or documenting code.
---

# No Defensive Nulls / Noise Comments

## Null checks

- If a field/reference is required by design (SerializeField that must be wired, component that must exist, state that must be initialized), do **not** add null checks, `?.`, early `return`, or soft `Debug.LogError` + continue.
- Let `NullReferenceException` surface in the console. The user assigns the missing reference from the stack trace.
- Null checks are OK only when null is a **real supported option** the user can intentionally leave empty (optional component, optional list entry, nullable API by design).

```csharp
// Bad — hides a missing required reference
if (_mover == null) return;
_mover.CheckForGround();

// Good — required; NRE shows the bug
_mover.CheckForGround();

// OK — optional by design
if (_ceilingDetector != null) _ceilingDetector.Reset();
```

## Comments

- Do **not** create comments for obvious code, routine methods, or fields.
- Comments are allowed only for genuinely complex methods/fields where intent is hard to read from the code alone.
- Prefer clear names over explanatory comments.

```csharp
// Bad
/// <summary>Called on game start to initialize darkness.</summary>
public void GameStart() { ... }

// Good — no comment; name is enough
public void GameStart() { ... }
```
