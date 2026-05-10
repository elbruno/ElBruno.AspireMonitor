# 🚀 ElBruno.AspireMonitor v1.6.0

**Date:** 2026-05-10
**Lead:** Leia (Release Manager)

## Summary

This release brings the monitor forward for Aspire 13.3 with standard dashboard defaults, richer resource telemetry, and a maintained Aspire sample harness used for end-to-end and regression validation.

## What's in 1.6.0

### Added
- **Aspire 13.3 dashboard alignment.** The monitor defaults to the local Aspire dashboard URL documented by Aspire 13.3, `http://localhost:18888`.
- **Rich resource telemetry.** Resource cards now surface type, disk usage percentage, endpoint counts, and compact environment summaries.
- **Sample harness.** `src/SampleHarness/` provides a representative Aspire solution for validating monitor behavior across services.

### Changed
- Configuration and settings now preserve dashboard URL and development-resource filtering behavior.
- Documentation and governance describe the sample harness as the canonical validation target.

## Distribution

- NuGet global tool: `dotnet tool install -g ElBruno.AspireMonitor` (launch with `aspiremon`)
- Published via `.github/workflows/publish.yml` on GitHub Release creation (OIDC trusted publishing to NuGet.org)

## Validation

- `dotnet test src\ElBruno.AspireMonitor.Tests\ElBruno.AspireMonitor.Tests.csproj -c Release --no-build`
- `dotnet test src\SampleHarness\SampleHarness.Tests\SampleHarness.Tests.csproj -c Release --no-build`

---

**Leia, Lead & Release Manager**
2026-05-10
