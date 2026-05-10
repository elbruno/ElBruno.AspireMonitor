# Governance & Architectural Decisions

This file records standing decisions that apply to all contributors and all pull
requests in this repository.

---

## Decision 1 — SampleHarness is the canonical validation target

**Date adopted:** 2026-05-10  
**Status:** Active

### Context

AspireMonitor is a live monitor for .NET Aspire applications. To validate it
end-to-end a real Aspire solution is needed. Rather than depending on an external
project, a minimal harness lives inside this repository.

### Decision

`src/SampleHarness/` is the **maintained, canonical validation target** for all
AspireMonitor features.

### Consequences

1. **The harness must stay current.** Any PR that adds or changes an AspireMonitor
   feature must update the harness or its tests if the feature changes observable
   behavior (new resource fields, new topology requirements, etc.).

2. **Tests in `SampleHarness.Tests` are part of the required test suite.** CI must
   run them alongside `ElBruno.AspireMonitor.Tests`. A failing harness test blocks
   merge.

3. **Keep the harness minimal.** Services should be simple, deterministic, and quick
   to start. Dependencies on Docker images or external APIs must not be added without
   a governance discussion.

4. **Topology changes require a doc update.** If the resource topology changes
   (services added/removed, names changed) `docs/sample-harness.md` must be updated
   in the same PR.

---

## Decision 2 — No regression of existing monitor tests

**Date adopted:** 2026-05-10  
**Status:** Active

The 88 tests in `ElBruno.AspireMonitor.Tests` represent the baseline contract of the
monitor application. They must continue to pass on every PR. Adding the harness must
never reduce that count or change any test outcome.

---

## Decision 3 — Target framework alignment

**Date adopted:** 2026-05-10  
**Status:** Active

| Component | Target framework | Rationale |
|---|---|---|
| Monitor (WPF) | net10.0-windows | Uses WPF; follows latest stable .NET |
| SampleHarness services | net9.0 | Aspire 13.x templates default; stable LTS |
| SampleHarness.Tests | net9.0 | Must match AppHost SDK version |

Upgrading either framework requires updating this table and verifying all tests pass.
