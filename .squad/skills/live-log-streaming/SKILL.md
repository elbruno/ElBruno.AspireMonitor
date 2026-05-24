---
name: "live-log-streaming"
description: "Stream selected-resource logs into a WPF view without restarting on every poll"
domain: "logging"
confidence: "medium"
source: "earned"
---

## Context

Use this when a monitoring UI needs a live log panel tied to a selected resource and the resource list is refreshed periodically in the background.

## Patterns

- Keep the active stream keyed by resource name or id, not by view refresh cycle.
- Stop the previous stream before starting a new selection.
- Use cancellation tokens to shut down log readers cleanly.
- Ignore stale log events once the selection has moved.
- Marshal UI updates through the dispatcher, with a null-safe fallback for tests.
- Reuse a shared `MainViewModel` log buffer for tray windows when the main view already owns the stream.
- Expose an empty-state message and `HasLogLines` flag so a mini console can render safe fallback UI without extra service calls.

## Examples

```csharp
_activeLogStreamResourceName = resource.Name;
_ = _liveLogsService.StartStreamingAsync(resource.Name);
```

```csharp
private void OnLiveLogLineReceived(object? sender, LogLineReceivedEventArgs args)
{
    if (!string.Equals(args.ResourceName, _activeLogStreamResourceName, StringComparison.OrdinalIgnoreCase))
        return;

    AddLogLine(args.LogLine);
}
```

## Anti-Patterns

- Restarting the log stream every time polling updates the resource list.
- Using `Application.Current.Dispatcher` without a fallback in test contexts.
- Letting stale log events append after the user has changed selection.

## Mini Console Reuse

- When a tray-only console is added, bind it to the existing `MainViewModel` instead of creating a second streaming service.
- Keep the window owned by `App.xaml.cs` so tray menu actions can open, hide, and dispose it consistently.
- Let the console close by hiding it; keep the shared log buffer alive so reopening is instant.
