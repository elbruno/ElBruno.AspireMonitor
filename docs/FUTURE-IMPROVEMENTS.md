# Future Improvements Analysis

**Document Owner:** Leia (Lead Architect)  
**Date:** 2026-04-27  
**Version:** 1.3.0 Analysis  
**Stakeholder:** Bruno Capuano (@elbruno)

---

## 1. Executive Summary

ElBruno.AspireMonitor v1.3.0 is a stable, Windows-only WPF system tray application distributed as a .NET global tool (`aspiremon`). It monitors local Aspire AppHost instances via working folder polling using the Aspire CLI, displaying real-time resource status with color-coded health indicators. The codebase is clean (~250 tests, >80% coverage, MVVM architecture) but fundamentally Windows-bound. **Strategic direction:** Prioritize operator experience polish and distribution reach first (quick wins), then decide whether cross-platform or deeper observability features represent the better long-term investment.

---

## 2. Themes

### Theme A: Cross-Platform Reach
**Why now:** Windows-only limits audience to ~60% of developers. macOS and Linux Aspire users have no tray monitoring option today. Expanding reach multiplies impact with each new feature.

### Theme B: Observability Depth
**Why now:** Current metrics are ephemeral (no history, no alerting). Developers debugging production-like issues need trends, thresholds, and actionable notifications—not just a snapshot.

### Theme C: Distribution & Onboarding
**Why now:** NuGet tool install works but isn't discoverable. Adding winget/Chocolatey/scoop reaches developers who never search NuGet. Reducing friction accelerates adoption.

### Theme D: Operator Experience
**Why now:** Settings live in a JSON file; no in-app configuration. Users ask for quick toggles, pinned resources, dashboard links. These are high-leverage, low-effort polish items.

### Theme E: Architecture & Tech Debt
**Why now:** The `EndOfStream` check in `AspireCliService.cs` is a known async anti-pattern (CA2024). As the codebase grows, addressing debt early prevents compounding maintenance costs.

---

## 3. Improvements Catalogue

---

### 3.1 Add macOS Support via Avalonia Tray

| Field | Value |
|-------|-------|
| **Theme** | Cross-Platform Reach |
| **Problem** | macOS Aspire developers have no native tray monitor. WPF is Windows-only. |
| **Proposed Approach** | Port UI layer to Avalonia (or .NET MAUI). Keep Services/ViewModels unchanged (they're already platform-agnostic). Create `ElBruno.AspireMonitor.Avalonia` project referencing shared `Core` library. Distribute via Homebrew cask or dmg. |
| **Effort** | XL (2-3 weeks, unfamiliar framework) |
| **Impact** | High — opens ~30% new user base |
| **Risks** | Avalonia tray API maturity varies; macOS notarization adds release complexity. |
| **Dependencies** | Extract shared Core library first (item 3.18) |

---

### 3.2 Add Linux Tray Support

| Field | Value |
|-------|-------|
| **Theme** | Cross-Platform Reach |
| **Problem** | Linux developers (especially WSL2 and server operators) cannot use the monitor. |
| **Proposed Approach** | Avalonia or GTK# tray integration. Same shared Core library. Distribute via Snap, Flatpak, or AppImage. |
| **Effort** | L (2 weeks if Avalonia already done) |
| **Impact** | Medium — smaller audience than macOS, but vocal Linux dev community |
| **Risks** | Linux tray implementations vary by desktop environment (GNOME, KDE, etc.). |
| **Dependencies** | macOS port (3.1) shares 90% of work |

---

### 3.3 Remote Aspire Instance Monitoring

| Field | Value |
|-------|-------|
| **Theme** | Observability Depth |
| **Problem** | Today, monitoring is local-only (working folder polling via CLI). Users cannot monitor Aspire running on a remote VM, container, or team server. |
| **Proposed Approach** | Add `remoteEndpoint` config option. Detect if endpoint is HTTP URL vs local folder. For remote, call Aspire's HTTP API directly (already documented in `architecture.md`) instead of CLI. Add UI toggle between local/remote mode. |
| **Effort** | M (1 week; HTTP client already exists) |
| **Impact** | High — essential for cloud-native and team scenarios |
| **Risks** | Authentication/TLS may be required for production endpoints; need to handle untrusted certs. |
| **Dependencies** | None |

---

### 3.4 Multi-Instance Monitoring (Multiple AppHosts)

| Field | Value |
|-------|-------|
| **Theme** | Observability Depth |
| **Problem** | Developers running multiple Aspire apps simultaneously cannot see them all in one view. |
| **Proposed Approach** | Allow multiple working folders in config (array). Create tabs or accordion sections per AppHost in MiniMonitorWindow. Aggregate status: worst-case determines tray icon color. |
| **Effort** | M (1 week; UI changes + config schema) |
| **Impact** | Medium — power users, microservices teams |
| **Risks** | UI clutter if too many instances; need collapsible sections. |
| **Dependencies** | None |

---

### 3.5 Historical Metrics with Sparklines

| Field | Value |
|-------|-------|
| **Theme** | Observability Depth |
| **Problem** | Current view is point-in-time. Users can't see if CPU spiked 5 minutes ago or is trending up. |
| **Proposed Approach** | Add in-memory circular buffer (last 60 samples @ 2s = 2 minutes). Render mini sparkline (OxyPlot or ScottPlot) next to each resource. Persist to SQLite optionally for longer history. |
| **Effort** | M (1 week) |
| **Impact** | High — debugging requires trends |
| **Risks** | Memory footprint increases; must cap buffer size. |
| **Dependencies** | None |

---

### 3.6 Threshold-Based Alerting with Toast Notifications

| Field | Value |
|-------|-------|
| **Theme** | Observability Depth |
| **Problem** | Users must watch the tray icon to notice problems. No proactive notification when thresholds breach. |
| **Proposed Approach** | Use Windows `ToastNotificationManager` (already available in WinRT). Trigger toast when resource crosses critical threshold. Add "snooze" / "mute" per-resource options in config. |
| **Effort** | S (2-3 days) |
| **Impact** | High — operators need alerts |
| **Risks** | Notification fatigue if thresholds too sensitive; need hysteresis (don't re-fire for 60s). |
| **Dependencies** | None |

---

### 3.7 OTLP / Prometheus Telemetry Export

| Field | Value |
|-------|-------|
| **Theme** | Observability Depth |
| **Problem** | Organizations with existing monitoring stacks (Grafana, Prometheus, Azure Monitor) can't integrate AspireMonitor data. |
| **Proposed Approach** | Expose `/metrics` endpoint (Prometheus format) or push to OTLP collector. Reference OpenTelemetry.Exporter.Prometheus and OpenTelemetry.Exporter.OpenTelemetryProtocol packages. |
| **Effort** | L (1-2 weeks; new dependency surface) |
| **Impact** | Medium — enterprise/DevOps users |
| **Risks** | Bloats package size; optional feature flag recommended. |
| **Dependencies** | None |

---

### 3.8 Dashboard Deep-Links from Tray Menu

| Field | Value |
|-------|-------|
| **Theme** | Operator Experience |
| **Problem** | Users want one-click access to Aspire dashboard, but dashboard URL is buried in browser history. |
| **Proposed Approach** | Parse dashboard URL from `aspire describe` output (already available). Add "Open Dashboard" menu item to tray context menu. Store last-known dashboard URL in config. |
| **Effort** | S (1-2 days) |
| **Impact** | High — most-requested UX improvement |
| **Risks** | Dashboard URL changes per session; must refresh on poll. |
| **Dependencies** | None |

---

### 3.9 Settings UI Window (Replace JSON Editing)

| Field | Value |
|-------|-------|
| **Theme** | Operator Experience |
| **Problem** | Configuration requires editing a JSON file manually—error-prone and non-discoverable. |
| **Proposed Approach** | `SettingsWindow.xaml` already exists but is minimal. Expand to include all config options (thresholds, polling interval, pinned resources, remote endpoint). Add validation and "Apply" button. |
| **Effort** | S (2-3 days; UI already scaffolded) |
| **Impact** | High — removes friction for non-power users |
| **Risks** | None significant |
| **Dependencies** | None |

---

### 3.10 Auto-Launch with `aspire run` or IDE Integration

| Field | Value |
|-------|-------|
| **Theme** | Operator Experience |
| **Problem** | Users must manually launch `aspiremon` before monitoring. |
| **Proposed Approach** | **Option A:** VS/Rider extension that auto-launches monitor when Aspire project runs. **Option B:** Shell alias/script that wraps `aspire run && aspiremon`. **Option C:** Windows startup shortcut with config-specified default folder. |
| **Effort** | S (Option C) to L (Option A) |
| **Impact** | Medium — convenience for frequent users |
| **Risks** | IDE extensions require separate maintenance; Option C simplest. |
| **Dependencies** | None |

---

### 3.11 Launch `aspire run` from Tray

| Field | Value |
|-------|-------|
| **Theme** | Operator Experience |
| **Problem** | Users must switch to terminal to start Aspire. |
| **Proposed Approach** | Add "Start AppHost" context menu item. Call `aspire run` via `AspireCliService.ExecuteCommandAsync` with configured working folder. Show output in a small log window or toast. |
| **Effort** | S (2-3 days) |
| **Impact** | Medium — single-pane-of-glass experience |
| **Risks** | Long-running process management; need "Stop AppHost" too. |
| **Dependencies** | None |

---

### 3.12 winget / Chocolatey / Scoop Distribution

| Field | Value |
|-------|-------|
| **Theme** | Distribution & Onboarding |
| **Problem** | NuGet tool install is .NET-centric. Windows users searching `winget search aspire` won't find it. |
| **Proposed Approach** | Submit manifests to winget-pkgs repo, Chocolatey community feed, and Scoop extras bucket. Automate manifest generation in CI on release tag. |
| **Effort** | S (2-3 days; mostly YAML/JSON manifests) |
| **Impact** | High — discoverability for Windows devs |
| **Risks** | Maintenance burden: update manifests on each release. |
| **Dependencies** | None |

---

### 3.13 Code Signing for Executable

| Field | Value |
|-------|-------|
| **Theme** | Distribution & Onboarding |
| **Problem** | Windows SmartScreen blocks unsigned executables, scaring users. |
| **Proposed Approach** | Acquire code signing certificate (EV recommended for immediate SmartScreen trust). Sign in GitHub Actions using Azure Key Vault or SignPath. |
| **Effort** | M (1 week; cert acquisition + CI integration) |
| **Impact** | High — removes "unknown publisher" friction |
| **Risks** | Annual certificate cost ($200-$500); secure key storage required. |
| **Dependencies** | None |

---

### 3.14 Accessibility Improvements (Screen Readers, High Contrast)

| Field | Value |
|-------|-------|
| **Theme** | Operator Experience |
| **Problem** | WPF app lacks explicit accessibility attributes; screen reader users can't navigate resources. |
| **Proposed Approach** | Add `AutomationProperties.Name` and `AutomationProperties.HelpText` to all interactive elements. Test with Narrator and NVDA. Support Windows High Contrast themes. |
| **Effort** | S (2-3 days) |
| **Impact** | Medium — compliance and inclusivity |
| **Risks** | None significant |
| **Dependencies** | None |

---

### 3.15 Localization (i18n)

| Field | Value |
|-------|-------|
| **Theme** | Operator Experience |
| **Problem** | All strings are hardcoded in English. |
| **Proposed Approach** | Extract strings to `.resx` resource files. Use `CultureInfo.CurrentUICulture` for runtime selection. Start with Spanish, Portuguese, Japanese (ElBruno's audience). |
| **Effort** | M (1 week; string extraction + translation) |
| **Impact** | Medium — global reach |
| **Risks** | Layout issues with longer translations; need flexible UI. |
| **Dependencies** | None |

---

### 3.16 Fix CA2024: EndOfStream in Async Loop

| Field | Value |
|-------|-------|
| **Theme** | Architecture & Tech Debt |
| **Problem** | `AspireCliService.cs:256` uses `process.StandardOutput.EndOfStream` in an async loop—an analyzer warning (CA2024) and potential subtle bug. |
| **Proposed Approach** | Replace with `ReadLineAsync` returning `null` as EOF signal, wrapped in try/finally for process cleanup. Reference: [CA2024 docs](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2024). |
| **Effort** | S (1-2 hours) |
| **Impact** | Low (correctness, not user-facing) |
| **Risks** | None |
| **Dependencies** | None |

---

### 3.17 Increase Test Coverage for ViewModels and Views

| Field | Value |
|-------|-------|
| **Theme** | Architecture & Tech Debt |
| **Problem** | Test coverage is >80% overall but ViewModels and Views have fewer tests (only `MiniWindowResourcesTests`, `WorkingFolderTests`, `MainWindowUITests`, etc.). |
| **Proposed Approach** | Add ViewModel unit tests for `SettingsViewModel` command bindings, `ConfigurationViewModel` validation, and edge cases in `ResourceViewModel`. Use Moq to isolate dependencies. |
| **Effort** | S (2-3 days) |
| **Impact** | Low — quality/confidence |
| **Risks** | None |
| **Dependencies** | None |

---

### 3.18 Extract Shared Core Library

| Field | Value |
|-------|-------|
| **Theme** | Architecture & Tech Debt |
| **Problem** | Cross-platform ports (Avalonia, MAUI) would duplicate Services, Models, ViewModels. |
| **Proposed Approach** | Create `ElBruno.AspireMonitor.Core` class library targeting `net10.0` (no -windows). Move all non-WPF code. WPF project references Core. Avalonia project references Core. |
| **Effort** | M (3-5 days; refactoring + CI updates) |
| **Impact** | Low now, High if cross-platform pursued |
| **Risks** | Breaking changes if API surface not stable; freeze API before extracting. |
| **Dependencies** | Must precede macOS/Linux ports (3.1, 3.2) |

---

### 3.19 Add Diagnostic Logging for Troubleshooting

| Field | Value |
|-------|-------|
| **Theme** | Architecture & Tech Debt |
| **Problem** | Current logging is `System.Diagnostics.Debug.WriteLine`—invisible to end users. Troubleshooting requires attaching debugger. |
| **Proposed Approach** | Add `Microsoft.Extensions.Logging` with file sink (Serilog or NLog). Log to `%APPDATA%\Local\ElBruno\AspireMonitor\logs`. Add log-level config option. |
| **Effort** | S (2-3 days) |
| **Impact** | Medium — essential for support |
| **Risks** | Log rotation needed to prevent disk fill. |
| **Dependencies** | None |

---

### 3.20 Pinned Resources Persistence Across Sessions

| Field | Value |
|-------|-------|
| **Theme** | Operator Experience |
| **Problem** | Pinned resources (v1.2.0 feature) may not persist if config isn't saved properly. |
| **Proposed Approach** | Verify `pinnedResources` array in config.json is written on pin/unpin. Add migration for users upgrading from v1.1.x. |
| **Effort** | S (1 day) |
| **Impact** | Medium — polish |
| **Risks** | None |
| **Dependencies** | None |

---

## 4. Prioritized Roadmap

### 🟢 Now (v1.4.x — Next Release)

| Item | Justification |
|------|---------------|
| **3.8 Dashboard Deep-Links** | Most-requested, trivial effort, high daily utility |
| **3.9 Settings UI Window** | Removes JSON editing friction; UI scaffolding exists |
| **3.16 Fix CA2024 EndOfStream** | 1-hour fix, eliminates analyzer warning |
| **3.6 Toast Notifications** | High impact alerting, small effort |
| **3.19 Diagnostic Logging** | Essential for support tickets; blocks nothing |

### 🟡 Next (v1.5–2.0)

| Item | Justification |
|------|---------------|
| **3.3 Remote Aspire Monitoring** | Unlocks team/cloud scenarios; HTTP client already exists |
| **3.5 Historical Metrics / Sparklines** | Debugging requires trends; moderate effort |
| **3.12 winget/Chocolatey/Scoop** | Expands discoverability significantly |
| **3.13 Code Signing** | Removes SmartScreen friction for new users |
| **3.4 Multi-Instance Monitoring** | Power-user feature; builds on 3.3 |
| **3.11 Launch `aspire run` from Tray** | Single-pane-of-glass convenience |

### 🔵 Later / Aspirational

| Item | Justification |
|------|---------------|
| **3.1 macOS Support (Avalonia)** | Big investment; only if Bruno commits to cross-platform |
| **3.2 Linux Support** | Depends on 3.1; similar effort |
| **3.18 Extract Shared Core Library** | Prerequisite for cross-platform; do only if 3.1 approved |
| **3.7 OTLP / Prometheus Export** | Enterprise niche; optional feature flag |
| **3.15 Localization** | Nice-to-have; defer unless community demand |
| **3.10 IDE Integration (VS/Rider)** | High maintenance; defer unless funding |
| **3.14 Accessibility** | Should do eventually; not blocking adoption |

---

## 5. Decisions Needed from Bruno

Before any of this work can be prioritized, the following decisions are required:

### Decision 1: Cross-Platform Investment
**Question:** Stay Windows-only, or invest in macOS/Linux support via Avalonia?  
**Trade-off:** XL effort for macOS, but opens 40% more developers. If yes, must extract shared Core library first.  
**Recommendation:** Defer cross-platform until v2.0 unless there's strong community demand.

### Decision 2: Local-Only vs Remote/Cloud Monitoring
**Question:** Should AspireMonitor support monitoring Aspire instances running on remote machines, containers, or cloud VMs?  
**Trade-off:** Medium effort, but changes the product positioning from "local dev tool" to "lightweight observability companion."  
**Recommendation:** Yes — remote monitoring is high-value and HTTP client already exists.

### Decision 3: Telemetry / Anonymous Usage Stats
**Question:** Should AspireMonitor collect anonymous usage telemetry (opt-in) to understand adoption patterns?  
**Trade-off:** Privacy concerns vs. data-driven prioritization.  
**Recommendation:** No telemetry initially. Add opt-in later if needed.

### Decision 4: Code Signing Certificate
**Question:** Invest in EV code signing certificate (~$400/year)?  
**Trade-off:** Eliminates SmartScreen warnings immediately vs. ongoing cost.  
**Recommendation:** Yes — critical for distribution credibility.

### Decision 5: winget/Chocolatey/Scoop Maintenance
**Question:** Commit to maintaining package manifests for each release?  
**Trade-off:** Extra release step vs. discoverability.  
**Recommendation:** Yes — automate manifest generation in CI.

### Decision 6: OTLP/Prometheus Integration
**Question:** Should AspireMonitor expose metrics for external observability stacks?  
**Trade-off:** Adds dependency surface and complexity for enterprise niche.  
**Recommendation:** Defer — keep it simple unless enterprise users request it.

---

## Appendix: Code References

| Area | File | Notes |
|------|------|-------|
| CLI execution | `Services/AspireCliService.cs` | CA2024 warning at line 256 |
| Polling state machine | `Services/AspirePollingService.cs` | Well-designed, event-driven |
| Config persistence | `Services/ConfigurationService.cs` | JSON file in AppData |
| ViewModels | `ViewModels/MainViewModel.cs`, `MiniMonitorViewModel.cs` | MVVM binding, testable |
| Tray integration | `App.xaml.cs`, `MainWindow.xaml.cs` | NotifyIcon from WinForms |
| Test coverage | `Tests/Services/*`, `Tests/ViewModels/*` | >80% overall |

---

*Document prepared by Leia, Lead Architect — 2026-04-27*
