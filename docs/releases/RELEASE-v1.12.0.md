# Release Notes: ElBruno.AspireMonitor v1.12.0

**Release Date:** 2026-05-24T10:21:51.482-04:00
**Focus:** Mini Console live log monitoring

## What's New

- Added a tray-launched Mini Console window for quick live log inspection.
- Reused the existing `MainViewModel` log buffer so the console stays in sync with the main app state.
- Kept live stream handling cancellation-safe, with duplicate stream protection and buffer clamping.
- Preserved the existing mini monitor behavior while refreshing its last-update state from the main model.

## Validation

- Added coverage for duplicate stream starts, cancellation, buffer trimming, status mapping, and dashboard visibility.
- Verified the mini monitor still opens and renders without changing its existing behavior.
- Verified the full release suite locally: 425 tests passing, Services/Models coverage gate at 85.21%.

## Upgrade

```bash
dotnet tool update --global ElBruno.AspireMonitor
```
