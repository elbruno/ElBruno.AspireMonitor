# 🚀 ElBruno.AspireMonitor v1.5.0

**Date:** 2026-04-27
**Lead:** Leia (Release Manager)

## Summary

A small, focused release that tightens the mini monitor window. The Start / Stop actions now live on the header row beside the close button as compact single-glyph buttons, removing the dedicated controls row entirely. The window is shorter, controls stay glanceable, and tooltips preserve discoverability.

## What's in 1.5.0

### Changed
- **Compact mini-window header.** Start/Stop buttons sit on the header row alongside ✕, sized ~24×22 px, single-glyph (▶ / ⏹), with tooltips and the same color cues (green Start, red Stop). The Control Buttons row and its dividers are gone; the XAML grid drops from 8 to 6 rows. Bindings to `MainViewModel.StartAspireCommand` / `StopAspireCommand` are unchanged.

### Added
- Refreshed promotional content (blog, LinkedIn, Twitter) and AI-generated visuals under `docs/promotional/` carried forward into 1.5.0.

## Distribution

- NuGet global tool: `dotnet tool install -g ElBruno.AspireMonitor` (launch with `aspiremon`)
- Published via `.github/workflows/publish.yml` on GitHub Release creation (OIDC trusted publishing to NuGet.org)

## Files Touched
- `src/ElBruno.AspireMonitor/ElBruno.AspireMonitor.csproj` — Version 1.4.0 → 1.5.0
- `src/ElBruno.AspireMonitor.Tool/ElBruno.AspireMonitor.Tool.csproj` — Version 1.4.0 → 1.5.0
- `src/ElBruno.AspireMonitor/Views/MiniMonitorWindow.xaml` — header + layout
- `CHANGELOG.md` — new 1.5.0 entry (file created this release)

---

**Leia, Lead & Release Manager**
2026-04-27
