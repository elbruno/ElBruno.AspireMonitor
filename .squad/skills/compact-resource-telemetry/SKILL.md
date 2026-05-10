---
name: "compact-resource-telemetry"
description: "Show richer resource telemetry in a dense monitoring card without cluttering the main status row."
domain: "wpf-ui"
confidence: "medium"
source: "observed"
---

## Context
Use this when a monitoring UI needs to add more metrics to an existing resource row but must keep the card compact and scannable.

## Patterns
- Keep the primary status row focused on name, CPU, memory, and the action link.
- Add a smaller secondary row for supplemental metrics like type, disk usage, and endpoint count.
- Bind display text from the ViewModel instead of formatting in XAML.

## Examples
- `ResourceViewModel.ResourceTypeText`
- `ResourceViewModel.DiskUsageText`
- `ResourceViewModel.EndpointCountText`
- `Views/MainWindow.xaml` resource item template

## Anti-Patterns
- Don't widen the primary row until it wraps awkwardly.
- Don't invent new telemetry sources just to fill space.
- Don't put display formatting logic directly in the view.
