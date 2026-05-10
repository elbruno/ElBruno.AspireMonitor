# What's New in ElBruno.AspireMonitor v1.7.0

**Release Date:** 2026-05-10  
**Focus:** Mini monitor telemetry, telemetry visibility control, hardened CLI parsing, test alignment

ElBruno.AspireMonitor v1.7.0 enhances the mini window with rich pinned-resource telemetry, adds a settings toggle to control visibility, and hardens Aspire CLI parsing with comprehensive test coverage.

## 🎯 What's New

### 1. **Mini Monitor Pinned-Resource Telemetry**

**What's new:**
- Each pinned resource in the mini window now displays CPU, memory, disk, resource type, endpoints, environment, and live status
- Telemetry is shown inline without clutter or fake GPU metrics
- Mini window remains compact while providing actionable monitoring data at a glance

**Why it matters:**
- You can now monitor resource health directly from the mini window without opening the main dashboard
- Pinned resources show real metrics (CPU %, memory, disk) updated every 5 seconds
- Environment badges and endpoint counts help you quickly identify service configuration

**Example in the mini window:**
```
📌 web-api (project)       [CPU: 12% | Mem: 45% | Disk: 3.2%] [2 endpoints] [Env: Dev]    🟢
📌 cache-svc (project)     [CPU: 8%  | Mem: 23% | Disk: 0.1%] [1 endpoint]  [Env: Dev]    🟢
```

**Learn more:** See [Configuration Guide](./configuration.md) → `MiniWindowResources` to pin your monitored resources.

---

### 2. **Telemetry Visibility Toggle (Settings)**

**What changed:**
- New settings option to show or hide mini monitor telemetry without removing pinned resources
- Telemetry display is **enabled by default** for all users
- Toggle is available in Settings → "Show Mini Monitor Telemetry"

**Why it matters:**
- Some users prefer a minimal pinned window with just resource names and status lights
- Others want detailed metrics inline; now both preferences are supported
- Existing pinned resources are preserved when toggling telemetry off

**How to use:**
1. Open the app Settings
2. Locate "Show Mini Monitor Telemetry" (enabled by default)
3. Toggle off if you prefer a compact mini window with names and status only
4. Changes apply immediately without restart

**Learn more:** See [Configuration Guide](./configuration.md) → `ShowMiniWindowResourceTelemetry` setting.

---

### 3. **Hardened Aspire CLI Parsing & Start-Command Alignment**

**What changed:**
- Improved parsing of Aspire CLI output from `aspire describe --format json` for robustness
- Corrected documentation and behavior for `aspire start` command alignment
- Added comprehensive test coverage for the Start button to prevent regressions
- Enhanced backend telemetry parsing to handle edge cases in resource and metric payloads

**Why it matters:**
- The monitor is now more resilient when parsing resources with unusual or optional fields
- Start button behavior is locked in with automated tests, reducing breakage across Aspire updates
- Documentation now accurately reflects current Aspire CLI conventions

**How this helps:**
- If Aspire CLI output changes subtly, the monitor gracefully handles missing or reordered fields
- The Start button countdown (`⏳ Starting Aspire... (12 / 90s)`) is now thoroughly tested
- Resource discovery and metric collection are more stable across different Aspire configurations

**Reference:**
- See [Troubleshooting](./troubleshooting.md) if you encounter parsing issues
- Review the test suite: `dotnet test src\ElBruno.AspireMonitor.Tests\ElBruno.AspireMonitor.Tests.csproj`

---

## 🔄 Upgrade Guide

### For Current Users (v1.6.x → v1.7.0)

1. **Update the global tool:**
   ```bash
   dotnet tool update --global ElBruno.AspireMonitor
   ```

2. **Telemetry visibility is enabled by default** — your mini window will now display metrics for pinned resources. If you prefer the compact view, disable it in Settings → "Show Mini Monitor Telemetry".

3. **No configuration changes required** — your existing `config.json` is preserved. Existing pinned resources continue to work.

4. **Verify Start button behavior:**
   - Click Start to launch your AppHost
   - Watch the countdown: `⏳ Starting Aspire... (12 / 90s)`
   - Resources will appear in the tray and mini window once Aspire starts
   - This behavior is now locked in with comprehensive tests

---

## 📊 v1.7.0 Improvements & Quality

| Area | Enhancement | Status |
|------|-------------|--------|
| **Mini Window Telemetry** | Pinned resources display CPU, memory, disk, type, endpoints, environment, and status | ✅ Delivered |
| **Telemetry Control** | Settings toggle to show/hide mini monitor metrics (enabled by default) | ✅ Delivered |
| **CLI Parsing** | Hardened Aspire CLI output parsing for robustness | ✅ Hardened |
| **Start Button Tests** | Comprehensive test coverage for Start button lifecycle | ✅ Locked in |
| **Metric Resilience** | Improved handling of resource and metric payloads in edge cases | ✅ Enhanced |
| **Dashboard Integration** | Compatible with `http://localhost:18888` and custom endpoints | ✅ Working |
| **Resource Discovery** | Robust `aspire describe --format json` parsing | ✅ Resilient |

✅ = Implemented & Tested

---

## 🎯 What's Coming Next

Track future improvement areas in [FUTURE-IMPROVEMENTS.md](./FUTURE-IMPROVEMENTS.md):
- Advanced filtering (by environment, service type)
- Log streaming integration
- Kubernetes-aware resource grouping
- Custom telemetry dashboards

---

## 🚀 Quick Start with v1.7.0

1. **Install or update:**
   ```bash
   dotnet tool install --global ElBruno.AspireMonitor
   # or
   dotnet tool update --global ElBruno.AspireMonitor
   ```

2. **Launch:**
   ```bash
   aspiremon
   ```

3. **Point to your Aspire AppHost folder:** When prompted, enter the path to your Aspire AppHost folder

4. **Start monitoring:** Use the Start button in the tray; resources appear in the tray and mini window

5. **Explore mini window telemetry:** Open Settings → "Show Mini Monitor Telemetry" (enabled by default) to see or hide CPU, memory, disk, endpoints, and environment for pinned resources

---

## 📚 Documentation

- **[QUICKSTART.md](./QUICKSTART.md)** — 5-minute setup
- **[Configuration Guide](./configuration.md)** — All settings, including new telemetry options
- **[Architecture Guide](./architecture.md)** — System design and data flows
- **[Troubleshooting](./troubleshooting.md)** — Common issues and solutions
- **[Sample Harness](./sample-harness.md)** — Reference Aspire application under `src\SampleHarness\`

---

## 🙏 Feedback & Questions

- **GitHub Issues:** https://github.com/elbruno/ElBruno.AspireMonitor/issues
- **GitHub Discussions:** https://github.com/elbruno/ElBruno.AspireMonitor/discussions
- **Author:** Bruno Capuano ([@elbruno](https://github.com/elbruno))

---

**Last Updated:** 2026-05-10  
**Version:** 1.7.0  
**Maintained by:** Chewie (DevRel/Docs)
