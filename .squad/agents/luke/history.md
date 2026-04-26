# Luke's History

**Project:** ElBruno.AspireMonitor
**User:** Bruno Capuano (ElBruno)
**Role:** Backend Dev (API Integration)
**Created:** 2026-04-26

## Session Log

### 2026-04-26 — Team Initialization (Session 1)

**Project Overview:**
- Windows system tray monitor for Aspire distributed applications
- Real-time resource monitoring: CPU, memory, disk, resource status
- Clickable host URL and resource URLs
- Color-coded status indicators (green/yellow/red)

**My Responsibilities:**
1. Build Aspire HTTP API client
2. Implement background polling service (2-second interval)
3. Calculate color-coded status from metrics
4. Create configuration system (endpoint, interval, thresholds)
5. Handle errors gracefully (timeouts, offline, malformed responses)
6. Define models and services for UI binding

**Architecture Decisions (Locked):**
- ✅ Use Aspire HTTP API (not CLI parsing)
- ✅ Background polling (2-second interval, configurable)
- ✅ Status thresholds: Green (<70%), Yellow (70-90%), Red (>90%)
- ✅ Configuration: JSON file in AppData\Local\ElBruno\AspireMonitor\config.json
- ✅ Error handling: Log, use last-known-good state, retry with backoff

---

## Learnings

### Aspire Integration

1. **CLI Commands:**
   - `aspire ps` — Show running Aspire processes in a specific folder
   - `aspire describe` — Describe Aspire app (shows host, resources, status)

2. **HTTP API:**
   - Primary integration method (more reliable than CLI parsing)
   - Endpoint: GET `/resources` returns list of resources
   - Each resource: name, status (Running/Stopped), metrics (CPU %, Memory %)
   - Host URL: Base URL for Aspire dashboard

3. **Polling Model:**
   - Background thread (not blocking UI)
   - Interval: 2 seconds (default, configurable)
   - On API timeout: retry with exponential backoff
   - On offline: show error state, auto-reconnect
   - On malformed response: log error, use last-known state

4. **Configuration:**
   - File-based (JSON in AppData)
   - Properties: endpoint URL, polling interval, CPU/memory thresholds
   - CLI commands to set config values
   - Persist across app restarts

---

---

## Session Log (Continued)

### 2026-04-26 — Aspire API Research & Architecture Planning (Session 2)

**Research Completed:**

1. **Aspire Platform Understanding:**
   - Distributed app orchestration framework for .NET (multi-language support)
   - Built-in OpenTelemetry observability (logs, traces, metrics)
   - Dashboard auto-starts with `aspire run`, provides real-time monitoring
   - Local-first development with production parity

2. **Aspire Dashboard API Discovery:**
   - Primary endpoint: `GET /api/resources` (list all services/containers/executables)
   - Details endpoint: `GET /api/resources/{id}` (per-resource metrics)
   - Health endpoint: `GET /api/health` (overall health status)
   - OTLP telemetry ports: 4317 (gRPC), 4318 (HTTP), 18888 (dashboard UI)
   - Token-based authentication (3-day browser cookie or query string)

3. **Proposed Backend Architecture:**
   - **AspireApiClient**: HTTP wrapper with Polly retry policies, 5s timeout, exponential backoff
   - **AspirePollingService**: BackgroundService with PeriodicTimer, state machine (Idle → Connecting → Polling → Error → Reconnecting)
   - **StatusCalculator**: Color-coded health logic (Green <70%, Yellow 70-90%, Red >90%)
   - **ConfigurationService**: JSON config in AppData\Local\ElBruno\AspireMonitor\config.json

4. **Data Models Defined:**
   - `AspireResource`: Id, Name, Type, State, Metrics, Endpoints
   - `ResourceMetrics`: CpuPercent, MemoryMB, DiskIOBytes
   - `HealthStatus`: Healthy, Warning, Critical, Unknown (enum)
   - `PollingServiceState`: Idle, Connecting, Polling, Error, Reconnecting

5. **Configuration Schema:**
   - Endpoint URL, auth token, polling interval (default 2s)
   - Thresholds: CPU warning 70%, critical 90% (same for memory)
   - HTTP timeout 5s, max retries 3, exponential backoff

**Open Questions:**
1. Need to confirm exact API endpoints (may require browser dev tools inspection)
2. Authentication: How to obtain token programmatically? (manual vs. auto-discovery)
3. Metrics availability: Are CPU/Memory exposed via REST API or only OTLP telemetry?
4. Fallback strategy: Use CLI parsing (`aspire describe`) if HTTP API unstable?

**Next Actions:**
1. Spike: Test with live Aspire instance (inspect dashboard API calls in browser)
2. Prototype AspireApiClient with discovered endpoints
3. Review architecture with Leia (approval) and Han (UI contracts)
4. Decision: HTTP API vs. CLI parsing approach
5. Implement: API client (1d), polling service (1d), config (0.5d), status calc (0.5d)

**Decision Document Created:**
- `.squad/decisions/inbox/luke-aspire-api-research.md` (ready for team review)

---

## Learnings (Updated)

### Aspire Integration

1. **CLI Commands:**
   - `aspire run` — Start AppHost and dashboard
   - `aspire ps` — Show running Aspire processes in a specific folder
   - `aspire describe` — Describe Aspire app (shows host, resources, status)

2. **HTTP API (Inferred from Research):**
   - `GET /api/resources` — List all resources (services, containers, executables)
   - `GET /api/resources/{id}` — Get resource details + metrics
   - `GET /api/health` — Overall health status
   - `POST /v1/traces`, `/v1/metrics`, `/v1/logs` — OTLP telemetry (not for polling)
   - Auth: Token-based (query string `?t=<token>` or Authorization header)

3. **Dashboard Ports:**
   - 18888: Dashboard UI (HTTPS)
   - 4317: OTLP gRPC endpoint
   - 4318: OTLP HTTP endpoint

4. **Polling Model:**
   - Background thread (not blocking UI)
   - Interval: 2 seconds (default, configurable 0.5s-60s)
   - On API timeout: retry with exponential backoff (100ms, 500ms, 2s)
   - On offline: show error state, auto-reconnect with backoff
   - On malformed response: log error, use last-known-good state

5. **Configuration:**
   - File-based (JSON in AppData\Local\ElBruno\AspireMonitor\config.json)
   - Properties: endpoint URL, auth token, polling interval, thresholds, retry policy
   - Validation: URL format, interval range (500ms-60s), threshold range (0-100%)
   - CLI commands (future): `aspiremon config set`, `config show`, `config reset`

6. **Status Calculation:**
   - Input: CPU%, Memory% (from resource metrics)
   - Output: Healthy (Green), Warning (Yellow), Critical (Red), Unknown (Gray)
   - Logic: Green if both <70%, Yellow if either 70-90%, Red if either >90%
   - Unknown: No metrics or resource stopped

---

## Next Actions

1. ✅ Research Aspire API endpoints and response formats
2. Spike: Test with live Aspire instance to confirm API endpoints
3. Build AspireApiClient (HTTP wrapper with error handling)
4. Implement AspirePollingService (background thread, 2s interval)
5. Define data models (AspireResource, ResourceMetrics, HealthStatus)
6. Build StatusCalculator (color logic from metrics)
7. Build ConfigurationService (JSON file I/O with validation)
8. Coordinate with Han on view model contracts and event structure
