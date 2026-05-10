# What's New in ElBruno.AspireMonitor v1.6.0

**Release Date:** 2026-05-10  
**Focus:** Aspire 13.3 alignment, richer telemetry, sample harness validation

ElBruno.AspireMonitor v1.6.0 aligns with the local dashboard default shown in [Aspire 13.3](https://aspire.dev/whats-new/aspire-13-3/), adds richer resource visibility from `aspire describe`, and validates the monitor against a maintained sample harness.

## 🎯 What's New

### 1. **Aspire 13.3 Dashboard Alignment**

**What changed:**
- The monitor now defaults to the Aspire dashboard endpoint used by current local Aspire CLI/dashboard output: `http://localhost:18888`
- Configuration preserves this URL and any user overrides seamlessly
- The dashboard link opens the configured local dashboard URL, including saved overrides

**Why it matters:**
- Aspire 13.3 documents `http://localhost:18888` for the standalone dashboard command
- The monitor stays in sync with Aspire's defaults, reducing configuration friction
- Users with custom dashboard URLs keep their existing configuration

**Learn more:** See [Configuration Guide](./configuration.md) to customize the endpoint if needed.

---

### 2. **Rich Resource Telemetry in the Monitor UI**

**What's visible now:**

| Telemetry | Description | Example |
|-----------|-------------|---------|
| **Resource Type** | Aspire resource type when supplied by CLI output | `project` |
| **Disk Usage** | Disk usage percentage when supplied by Aspire data | `12.3%` |
| **Endpoint Count** | Number of exposed ports/routes | `3 endpoints` |
| **Environment Badges** | Compact environment-variable summary | `Env: Dev` or `Env: 2 vars` |
| **Live Status** | Color-coded health (green/yellow/red) | 🟢 Green when running and average CPU/MEM <70% |

**Why it matters:**
- You get more context without opening the Aspire dashboard
- Quickly identify resource type and health in the main resource list
- Environment badges help identify development-only resources that can be hidden by configuration

**Example in Practice:**
```
🌐 api-service (project)       [12.3% disk] [2 endpoints] [Env: Dev]    🟢
🌐 catalog-api (project)       [3.7% disk]  [1 endpoint]  [Env: 1 var]  🟢
⚙️ worker-service (project)    [0.0% disk]  [0 endpoints]               🟡
```

See [Configuration Guide](./configuration.md) → `hideDevelopmentResources` to filter by environment.

---

### 3. **Sample Harness for End-to-End Validation**

**What's new:**
- A maintained Aspire sample solution lives in `src/SampleHarness/`
- Includes two API services and a worker service, with explicit service references and wait dependencies
- Used for regression testing and feature validation across Aspire versions

**Why it matters:**
- The monitor is validated against a representative Aspire architecture
- Future Aspire updates can be tested against this harness before release
- Developers can use it as a reference for setting up test scenarios

**Reference:**
- See [Sample Harness](./sample-harness.md) for structure and services
- Inspect `src\SampleHarness\SampleHarness.AppHost\AppHost.cs` for the AppHost composition
- Run tests: `dotnet test src\SampleHarness\SampleHarness.Tests\SampleHarness.Tests.csproj`

---

## 🔄 Upgrade Guide

### For Current Users (v1.5.0 → v1.6.0)

1. **Update the global tool:**
   ```bash
   dotnet tool update --global ElBruno.AspireMonitor
   ```

2. **No configuration changes required** — your existing `config.json` is preserved. The new defaults apply only to fresh installations.

3. **If you customized the dashboard endpoint**, verify it still works:
   - Check Settings → "Aspire Endpoint URL"
   - Default: `http://localhost:18888`
   - Custom: (your override persists)

4. **Try the new telemetry:**
   - Open the main window (click tray icon)
   - Check resource rows for new fields: type, disk usage percentage, endpoints, environment summaries
   - Pin resources in Settings → `MiniWindowResources` to see selected resource names and URLs in the mini window

---

## 📊 Feature Alignment with Aspire 13.3

Aspire 13.3 introduced several major features. Here's what the monitor exposes:

| Aspire 13.3 Feature | Monitor Support | Notes |
|---|---|---|
| **`aspire destroy` command** | ℹ️ See CLI | Not integrated; use `aspire destroy` directly from CLI |
| **Browser logs & screenshots** | 📊 Dashboard only | View in the main Aspire dashboard |
| **`aspire deploy` to Kubernetes** | 📊 Dashboard only | Monitor tracks resources; deployment state visible on dashboard |
| **Standard dashboard endpoint** | ✅ Implemented | Defaults to `http://localhost:18888` |
| **Container tunnel (Docker/Podman)** | ℹ️ Transparent | Not directly integrated; monitor consumes resources and endpoints exposed by Aspire CLI output |
| **JavaScript publishing (Next.js, Vite)** | 📊 Dashboard / CLI | Not JavaScript-specific; monitor can display resources that appear in `aspire describe` output |
| **`aspire dashboard` standalone** | ℹ️ See CLI | Run separately; point monitor to its endpoint |
| **Richer resource metadata** | ✅ Implemented | Resource type, disk percentage, endpoint count, environment summary badges |

✅ = Monitor feature  
📊 = View on Aspire dashboard  
ℹ️ = Use via CLI or dashboard  

---

## 🎯 What's Coming Next

Track future improvement areas in [FUTURE-IMPROVEMENTS.md](./FUTURE-IMPROVEMENTS.md):
- Advanced filtering (by environment, service type)
- Log streaming integration
- Kubernetes-aware resource grouping
- Custom telemetry dashboards

---

## 🚀 Quick Start with v1.6.0

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

3. **Point to your Aspire AppHost:** When prompted, enter the path to your `*.AppHost` project

4. **Start monitoring:** Run `aspire run` in that directory; resources appear in the tray

5. **Explore new telemetry:** Open the main window to see resource types, disk usage percentage, endpoints, and environment summaries

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
**Version:** 1.6.0  
**Maintained by:** Chewie (DevRel/Docs)
