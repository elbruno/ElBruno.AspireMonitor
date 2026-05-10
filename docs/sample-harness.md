# SampleHarness — Aspire E2E Validation Target

## Why it exists

AspireMonitor is a monitor for .NET Aspire applications, so it needs a real Aspire
application to test against. **SampleHarness** is that application: a small, fully
deterministic Aspire solution that lives alongside the monitor code and serves as the
**canonical end-to-end validation target** for all AspireMonitor features.

Having a committed harness means:

- Every pull request can be validated against a realistic Aspire topology without
  depending on an external project.
- The monitor's connection, polling, and display logic can be exercised against
  services whose responses are fully known.
- New features (e.g. new resource types, new dashboard fields) can be driven by
  first adding them to the harness.

---

## Solution layout

```
src/SampleHarness/
├── SampleHarness.AppHost/          # Aspire AppHost — orchestrates all services
├── SampleHarness.ServiceDefaults/  # Shared OpenTelemetry / health-check defaults
├── SampleHarness.ApiService/       # Minimal API: /weatherforecast, /
├── SampleHarness.CatalogApi/       # Minimal API: /api/products, /api/products/{id}, /
├── SampleHarness.WorkerService/    # BackgroundService heartbeat worker
└── SampleHarness.Tests/            # xUnit topology + endpoint tests
```

### Resource topology registered in AppHost

| Aspire resource name | Project | Notes |
|---|---|---|
| `api-service` | SampleHarness.ApiService | External HTTP endpoints, weather data |
| `catalog-api` | SampleHarness.CatalogApi | External HTTP endpoints, static product catalog |
| `worker-service` | SampleHarness.WorkerService | WaitFor api-service + catalog-api; heartbeat every 5 s |

---

## How to run the harness

### Prerequisites

- .NET 10 SDK (or later)
- [.NET Aspire workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling)
  or Aspire tooling installed in Visual Studio 2022 17.9+

### Start the AppHost

```bash
cd src/SampleHarness/SampleHarness.AppHost
dotnet run
```

The Aspire dashboard opens automatically (default: `http://localhost:18888`).

### Point AspireMonitor at the harness

1. Launch AspireMonitor.
2. Open **Settings** and set the Aspire endpoint to `http://localhost:18888`.
3. The monitor should immediately show three resources: `api-service`, `catalog-api`,
   and `worker-service`.

---

## Running the tests

The test project (`SampleHarness.Tests`) contains two categories of fast tests:

### Topology tests (`AppHostTopologyTests`)

Use `DistributedApplicationTestingBuilder` to build the AppHost in memory and assert
that the correct resources are registered — **no Docker or network required**.

```bash
cd src/SampleHarness
dotnet test SampleHarness.Tests/SampleHarness.Tests.csproj
```

### In-process endpoint tests

`ApiServiceEndpointTests` and `CatalogApiEndpointTests` use `WebApplicationFactory`
to hit the service HTTP endpoints directly, also with no external infrastructure.

---

## How the harness validates AspireMonitor features

| AspireMonitor feature | How SampleHarness exercises it |
|---|---|
| Resource discovery | AppHost registers 3 named resources the monitor must display |
| Endpoint display | `api-service` and `catalog-api` expose external HTTP endpoints |
| Worker resource type | `worker-service` is a Project resource with no HTTP endpoint |
| Resource relationships | Worker declares `WaitFor` + `WithReference` dependencies on both APIs |
| Polling stability | Worker emits a heartbeat log every 5 s, keeping resource state `Running` |
| Error detection | Stopping any service surfaces it as `Exited` in the dashboard |

---

## Maintenance contract

> **This harness is the maintained validation target for AspireMonitor.**  
> Any change that adds a new AspireMonitor feature must be accompanied by a
> corresponding update to the harness or its tests. See
> [governance.md](./governance.md) for details.

If a service in the harness stops building or its tests fail, the PR should be
blocked until fixed.
