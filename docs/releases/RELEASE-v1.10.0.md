# Release Notes: ElBruno.AspireMonitor v1.10.0

**Release Date:** 2026-05-11
**Focus:** Clickable Aspire dashboard access from running notifications

## What's New

- Running notifications include the current Aspire dashboard URL.
- Clicking the running notification opens that dashboard in the default browser.
- If a detected dashboard URL is available, it is preferred; otherwise `aspireEndpoint` is used.
- Stopped notifications do not launch the dashboard and clear stale click targets.

## Validation

- Focused notification tests: 13/13 passing.
- Full monitor test project: 410/410 passing.
- NuGet package validation: `dotnet pack src\ElBruno.AspireMonitor.Tool\ElBruno.AspireMonitor.Tool.csproj -c Release --no-restore --verbosity minimal`.

## Upgrade

```bash
dotnet tool update --global ElBruno.AspireMonitor
```

No configuration changes are required. Keep `notifyOnStateChange` enabled to receive clickable running notifications.
