*Last Updated: 2026-05-24 (Mini Console implementation merge)
*Phase: Recent decisions only

---

### 2026-05-24T10:03:12.875-04:00: Mini Console implementation and live log behavior (consolidated)

**By:** Han, Luke, Yoda

**What:**
- Add a tray-launched `MiniConsoleWindow` owned by `App.xaml.cs`.
- Bind it directly to the existing `MainViewModel` live log state.
- Keep the mini monitor behavior untouched.
- Keep live log streams selection-scoped and cancellation-driven.
- Treat cancellation as a normal close event.
- Clamp live-log buffer size to at least 1.
- Refresh mini monitor last-update state from `MainViewModel.LastUpdated`.
- Cover duplicate stream starts, cancellation, buffer trimming, status mapping, and dashboard visibility in tests.

**Why:**
- Reuses the current live log pipeline without introducing a second stream.
- Keeps tray-only window creation consistent with the existing app shell.
- Prevents stale mini monitor state and empty-buffer edge cases.
- Makes stop/dispose flows predictable and testable.

**Files:**
- `src\ElBruno.AspireMonitor\App.xaml.cs`
- `src\ElBruno.AspireMonitor\Views\MiniConsoleWindow.xaml`
- `src\ElBruno.AspireMonitor\Views\MiniConsoleWindow.xaml.cs`
- `src\ElBruno.AspireMonitor\ViewModels\MainViewModel.cs`
- `src\ElBruno.AspireMonitor\Services\AspireLiveLogsService.cs`
- `src\ElBruno.AspireMonitor\ViewModels\MiniMonitorViewModel.cs`
- `src\ElBruno.AspireMonitor.Tests\Services\AspireLiveLogsServiceEdgeCaseTests.cs`
- `src\ElBruno.AspireMonitor.Tests\ViewModels\MiniMonitorViewModelStatusTests.cs`
