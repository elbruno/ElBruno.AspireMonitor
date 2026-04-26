# ElBruno.AspireMonitor — API Contract & Service Documentation

Complete reference for service layers, data contracts, and API interactions.

## Table of Contents

1. [Service Layer Overview](#service-layer-overview)
2. [AspireApiClient](#aspireapiclient)
3. [AspirePollingService](#aspirepollingservice)
4. [StatusCalculator](#statuscalculator)
5. [Data Contracts](#data-contracts)
6. [Error Handling & Retry Logic](#error-handling--retry-logic)
7. [Configuration Contract](#configuration-contract)

---

## Service Layer Overview

ElBruno.AspireMonitor follows a **layered service architecture**:

```
┌─────────────────────────────────────────┐
│         WPF UI Layer                    │
│  (MainWindow, ViewModels, Binding)      │
└────────────────┬────────────────────────┘
                 │
┌────────────────▼────────────────────────┐
│       Business Logic Layer              │
│  (AspirePollingService, StatusCalc)     │
└────────────────┬────────────────────────┘
                 │
┌────────────────▼────────────────────────┐
│        Data Access Layer                │
│  (AspireApiClient, ConfigurationSvc)    │
└────────────────┬────────────────────────┘
                 │
┌────────────────▼────────────────────────┐
│      Aspire HTTP API / File System      │
│  (http://localhost:5000, AppData)       │
└─────────────────────────────────────────┘
```

**Key Principles:**
- **Separation of Concerns**: Each layer has a single responsibility
- **Dependency Injection**: Services are constructed with dependencies
- **Async/Await**: All I/O operations are non-blocking
- **Error Recovery**: Graceful degradation and retry policies
- **State Management**: Discrete polling states with event notifications

---

## AspireApiClient

HTTP client for communicating with Aspire Dashboard API.

### Purpose

- Fetch resource list from Aspire
- Retrieve individual resource details
- Check Aspire health status
- Handle network errors and timeouts with exponential backoff

### Constructor

```csharp
// Direct HttpClient creation (most common)
var client = new AspireApiClient(configuration);

// Dependency-injected HttpClient (for testing)
var client = new AspireApiClient(httpClient, configuration);
```

**Parameters:**
- `configuration` — Instance of `Configuration` class with `AspireEndpoint` URL

### Public Methods

#### `GetResourcesAsync()`

Fetches all resources from Aspire dashboard.

```csharp
public async Task<List<AspireResource>> GetResourcesAsync()
```

**Returns:**
- `List<AspireResource>` — List of all resources (empty list if error)
- Never throws; returns empty list on failure

**HTTP Details:**
- **Endpoint**: `GET /api/resources`
- **Timeout**: 5 seconds
- **Retries**: 3 attempts with exponential backoff

**Example:**
```csharp
var client = new AspireApiClient(config);
var resources = await client.GetResourcesAsync();
if (resources.Count > 0)
{
    foreach (var resource in resources)
    {
        Console.WriteLine($"{resource.Name}: {resource.Status}");
    }
}
```

#### `GetResourceAsync(string id)`

Fetches a single resource by ID.

```csharp
public async Task<AspireResource?> GetResourceAsync(string id)
```

**Parameters:**
- `id` — Resource identifier (string)

**Returns:**
- `AspireResource?` — The resource, or null if not found/error
- Never throws; returns null on failure

**HTTP Details:**
- **Endpoint**: `GET /api/resources/{id}`
- **Timeout**: 5 seconds
- **Retries**: 3 attempts with exponential backoff

**Example:**
```csharp
var client = new AspireApiClient(config);
var resource = await client.GetResourceAsync("api-service");
if (resource != null)
{
    Console.WriteLine($"CPU: {resource.Metrics.CpuUsagePercent}%");
}
```

#### `GetHealthAsync()`

Checks health status of Aspire API.

```csharp
public async Task<HealthStatus> GetHealthAsync()
```

**Returns:**
- `HealthStatus` — Contains status color and message
  - Green + "Healthy" if OK
  - Red + error message if failed

**HTTP Details:**
- **Endpoint**: `GET /api/health`
- **Timeout**: 5 seconds
- **Retries**: 3 attempts with exponential backoff

**Example:**
```csharp
var health = await client.GetHealthAsync();
Console.WriteLine($"Status: {health.Status}, Message: {health.Message}");
```

### Error Handling

All methods follow the same error handling pattern:

1. **Network Timeout** → Returns empty/null (not thrown)
2. **HTTP Error** (4xx, 5xx) → Returns empty/null
3. **JSON Parse Error** → Returns empty/null
4. **Generic Exception** → Returns empty/null, logged

**Retry Policy (Polly):**
- Retries 3 times with exponential backoff
- Backoff delays: 1s, 2s, 4s
- Handles: `HttpRequestException`, `TaskCanceledException`, HTTP errors

```csharp
var retryPolicy = Policy
    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .Or<HttpRequestException>()
    .Or<TaskCanceledException>()
    .WaitAndRetryAsync(
        3,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1))
    );
```

---

## AspirePollingService

Background polling orchestrator that continuously monitors resources.

### Purpose

- Run polling loop at configurable interval
- Manage polling state machine
- Trigger UI updates via events
- Handle connection failures with recovery

### Constructor

```csharp
var service = new AspirePollingService(apiClient, configuration);
```

**Parameters:**
- `apiClient` — Instance of `AspireApiClient`
- `configuration` — Instance of `Configuration` with `PollingIntervalMs`

### State Machine

Polling service maintains discrete states:

```
Idle ─→ Connecting ─→ Polling
                        ↑ ↓
                        Error → Reconnecting
```

| State | Meaning | Transitions |
|-------|---------|-------------|
| **Idle** | Not running | Start() → Connecting |
| **Connecting** | Initial connection attempt | Success → Polling, Failure → Error |
| **Polling** | Normal operation, fetching resources | Failure → Error |
| **Error** | Connection failed, waiting for retry | Auto-retry → Reconnecting |
| **Reconnecting** | Attempting recovery from error | Success → Polling, Failure → Error |

### Public Methods

#### `Start()`

Starts the polling loop.

```csharp
public void Start()
```

**Behavior:**
- Transitions to `Connecting` state
- Starts polling timer at `PollingIntervalMs` interval
- Resets reconnection attempt counter
- No-op if already running

**Example:**
```csharp
var service = new AspirePollingService(client, config);
service.Start();  // Begins polling
```

#### `Stop()`

Stops the polling loop.

```csharp
public void Stop()
```

**Behavior:**
- Stops polling timer
- Transitions to `Idle` state
- Preserves last known resources

**Example:**
```csharp
service.Stop();  // Stops polling
```

#### `RefreshAsync()`

Manually trigger a poll cycle.

```csharp
public async Task RefreshAsync()
```

**Behavior:**
- Immediately fetches resources via API
- Fires events as if automatic poll occurred
- Does not stop polling loop

**Example:**
```csharp
await service.RefreshAsync();  // Immediate refresh
```

#### `State` (Property)

Gets current polling state.

```csharp
public PollingServiceState State { get; }
```

**Returns:** One of `Idle`, `Connecting`, `Polling`, `Error`, `Reconnecting`

**Example:**
```csharp
if (service.State == PollingServiceState.Polling)
{
    Console.WriteLine("Actively polling");
}
```

### Events

#### `ResourcesUpdated`

Fired when resources are successfully fetched.

```csharp
public event EventHandler<List<AspireResource>>? ResourcesUpdated;
```

**Usage:**
```csharp
service.ResourcesUpdated += (sender, resources) =>
{
    Console.WriteLine($"Got {resources.Count} resources");
};
```

#### `StatusChanged`

Fired when state machine state changes.

```csharp
public event EventHandler<string>? StatusChanged;
```

**Message Examples:**
- "Connecting to Aspire..."
- "Connected. Polling..."
- "Connection error. Retrying..."
- "Idle"

**Usage:**
```csharp
service.StatusChanged += (sender, message) =>
{
    Console.WriteLine($"Status: {message}");
};
```

#### `ErrorOccurred`

Fired when an error occurs during polling.

```csharp
public event EventHandler<string>? ErrorOccurred;
```

**Message Examples:**
- "Failed to connect: Connection refused"
- "Timeout waiting for API response"
- "Invalid JSON response"

**Usage:**
```csharp
service.ErrorOccurred += (sender, errorMessage) =>
{
    Console.WriteLine($"Error: {errorMessage}");
};
```

### Polling Interval

Controlled by `Configuration.PollingIntervalMs`.

**Default:** 2000ms (2 seconds)  
**Range:** 1000ms (1 second) to 60000ms (60 seconds)

To change:
```json
{
  "pollingIntervalMs": 5000
}
```

Then restart the application.

---

## StatusCalculator

Evaluates resource health based on CPU/memory metrics.

### Purpose

- Calculate status color (Green/Yellow/Red)
- Apply configurable thresholds
- Determine overall system health

### Constructor

```csharp
// With configuration
var calc = new StatusCalculator(configuration);

// With defaults
var calc = new StatusCalculator();
```

**Parameters:**
- `configuration` (optional) — Instance of `Configuration` with thresholds

### Public Methods

#### `CalculateStatus(double cpuPercent, double memoryPercent)`

Calculates status based on CPU and memory percentages.

```csharp
public StatusColor CalculateStatus(double cpuPercent, double memoryPercent)
```

**Parameters:**
- `cpuPercent` — CPU usage percentage (0-100)
- `memoryPercent` — Memory usage percentage (0-100)

**Returns:**
- `StatusColor.Green` (🟢) — Both < warning threshold
- `StatusColor.Yellow` (🟡) — Any between warning and critical
- `StatusColor.Red` (🔴) — Any >= critical or negative input
- `StatusColor.Unknown` — Invalid/missing metrics

**Logic:**
```
If cpuPercent < 0 OR memoryPercent < 0
    → Return Unknown

If cpuPercent >= criticalThreshold OR memoryPercent >= criticalThreshold
    → Return Red

If cpuPercent >= warningThreshold OR memoryPercent >= warningThreshold
    → Return Yellow

Else
    → Return Green
```

**Example:**
```csharp
var status = calc.CalculateStatus(35, 62);  // Green (both < 70%)
var status = calc.CalculateStatus(72, 45);  // Yellow (CPU >= 70%)
var status = calc.CalculateStatus(91, 85);  // Red (CPU >= 90%)
```

#### `CalculateStatusFromMetrics(ResourceMetrics metrics)`

Convenience method using `ResourceMetrics` object.

```csharp
public StatusColor CalculateStatusFromMetrics(ResourceMetrics metrics)
```

**Parameters:**
- `metrics` — `ResourceMetrics` with CPU and memory percentages

**Returns:** Same as `CalculateStatus()`

**Example:**
```csharp
var status = calc.CalculateStatusFromMetrics(resource.Metrics);
```

#### `CalculateOverallStatus(IEnumerable<AspireResource> resources)`

Determines overall system status from all resources.

```csharp
public StatusColor CalculateOverallStatus(IEnumerable<AspireResource> resources)
```

**Parameters:**
- `resources` — Collection of resources to evaluate

**Returns:**
- `StatusColor.Red` (🔴) — Any resource is Red or stopped
- `StatusColor.Yellow` (🟡) — Any resource is Yellow
- `StatusColor.Green` (🟢) — All resources are Green
- `StatusColor.Unknown` — No resources provided

**Logic:**
```
Scan all resources:
  - Stopped or Unknown status → Mark as Red
  - Calculate individual status → Track highest
  
If any Red exists → Return Red
Else if any Yellow exists → Return Yellow
Else → Return Green
```

**Example:**
```csharp
var overallStatus = calc.CalculateOverallStatus(allResources);
// Updates tray icon color
trayIcon.SetColor(overallStatus);
```

### Threshold Configuration

Default thresholds (in Configuration):

```json
{
  "cpuThresholdWarning": 70,
  "cpuThresholdCritical": 90,
  "memoryThresholdWarning": 70,
  "memoryThresholdCritical": 90
}
```

To customize, edit the config file and restart:

```json
{
  "cpuThresholdWarning": 50,
  "cpuThresholdCritical": 80,
  "memoryThresholdWarning": 60,
  "memoryThresholdCritical": 85
}
```

---

## Data Contracts

### AspireResource

Represents a resource in Aspire.

```csharp
public class AspireResource
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Type { get; set; }
    public ResourceStatus Status { get; set; }
    public ResourceMetrics Metrics { get; set; }
    public List<string> Endpoints { get; set; }
}
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Id` | string | Unique resource identifier |
| `Name` | string | Display name (e.g., "api-service") |
| `Type` | string? | Resource type (e.g., "service", "database") |
| `Status` | ResourceStatus enum | Running, Stopped, Stopping, Unknown |
| `Metrics` | ResourceMetrics | CPU/memory usage |
| `Endpoints` | List<string> | HTTP endpoints (URLs) |

**Example JSON:**
```json
{
  "id": "api-service",
  "name": "API Service",
  "type": "service",
  "status": "running",
  "metrics": {
    "cpuUsagePercent": 35.2,
    "memoryUsagePercent": 245
  },
  "endpoints": ["http://localhost:5001"]
}
```

### ResourceMetrics

Container for resource utilization metrics.

```csharp
public class ResourceMetrics
{
    public double CpuUsagePercent { get; set; }
    public double MemoryUsagePercent { get; set; }
    public double DiskUsagePercent { get; set; }
}
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `CpuUsagePercent` | double | CPU usage (0-100%) |
| `MemoryUsagePercent` | double | Memory usage (0-100%) |
| `DiskUsagePercent` | double | Disk usage (0-100%, optional) |

**Example:**
```json
{
  "cpuUsagePercent": 35.2,
  "memoryUsagePercent": 245,
  "diskUsagePercent": 62.5
}
```

### ResourceStatus

Enum for resource state.

```csharp
public enum ResourceStatus
{
    Running,    // Resource is active
    Stopped,    // Resource is stopped
    Stopping,   // Resource is shutting down
    Unknown     // State unknown or unavailable
}
```

### StatusColor

Enum for health status visualization.

```csharp
public enum StatusColor
{
    Green,      // 🟢 Healthy (<70% usage)
    Yellow,     // 🟡 Warning (70-90% usage)
    Red,        // 🔴 Critical (>90% usage or error)
    Unknown     // ⚪ Unknown/disconnected
}
```

### HealthStatus

Container for API health check result.

```csharp
public class HealthStatus
{
    public StatusColor Status { get; set; }
    public string Message { get; set; }

    public HealthStatus(StatusColor status, string message)
    {
        Status = status;
        Message = message;
    }
}
```

**Example:**
```csharp
new HealthStatus(StatusColor.Green, "Healthy")
new HealthStatus(StatusColor.Red, "Connection refused")
```

---

## Error Handling & Retry Logic

### Retry Policy (Polly)

AspireApiClient uses **exponential backoff** for transient failures.

```csharp
_retryPolicy = Policy
    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .Or<HttpRequestException>()
    .Or<TaskCanceledException>()
    .WaitAndRetryAsync(
        3,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1))
    );
```

**Behavior:**

| Attempt | Delay | Reason |
|---------|-------|--------|
| 1st | 0s | Initial attempt |
| 2nd | 1s | First failure (2^0 = 1) |
| 3rd | 2s | Second failure (2^1 = 2) |
| 4th | 4s | Third failure (2^2 = 4) |
| Failure | — | Give up after 3 retries |

**Handled Exceptions:**
- `HttpRequestException` — Network/connectivity issues
- `TaskCanceledException` — Request timeout (5 seconds)
- HTTP error status codes — 4xx, 5xx responses

**Non-Retried:**
- `JsonException` — Malformed API response
- Other exceptions — Logged, returned as empty/null

### Graceful Degradation

When Aspire API is unavailable:

1. **UI remains responsive** — Polling continues in background
2. **Last known state** — Previous resource list retained
3. **Tray icon updates** — Shows disconnected/error state
4. **Manual refresh available** — User can retry immediately
5. **Auto-reconnect** — Polling service retries on next interval

### Timeout Behavior

- **HTTP Request Timeout**: 5 seconds
- **Polling Interval**: Configurable, default 2 seconds
- **Error Backoff**: Exponential, max 4 seconds
- **Overall**: System remains responsive during timeouts

---

## Configuration Contract

Complete configuration schema.

### File Location

```
%APPDATA%\Local\ElBruno\AspireMonitor\config.json
```

Example Windows path:
```
C:\Users\YourUsername\AppData\Local\ElBruno\AspireMonitor\config.json
```

### Schema

```csharp
public class Configuration
{
    public string AspireEndpoint { get; set; }
    public int PollingIntervalMs { get; set; }
    public int CpuThresholdWarning { get; set; }
    public int CpuThresholdCritical { get; set; }
    public int MemoryThresholdWarning { get; set; }
    public int MemoryThresholdCritical { get; set; }
}
```

### Properties

| Property | Type | Default | Range | Description |
|----------|------|---------|-------|-------------|
| `aspireEndpoint` | string | — | any URL | Aspire API base URL (required) |
| `pollingIntervalMs` | int | 2000 | 1000-60000 | Refresh interval in milliseconds |
| `cpuThresholdWarning` | int | 70 | 0-100 | CPU warning threshold (%) |
| `cpuThresholdCritical` | int | 90 | 0-100 | CPU critical threshold (%) |
| `memoryThresholdWarning` | int | 70 | 0-100 | Memory warning threshold (%) |
| `memoryThresholdCritical` | int | 90 | 0-100 | Memory critical threshold (%) |

### Valid Examples

**Local development:**
```json
{
  "aspireEndpoint": "http://localhost:5000",
  "pollingIntervalMs": 2000,
  "cpuThresholdWarning": 70,
  "cpuThresholdCritical": 90,
  "memoryThresholdWarning": 70,
  "memoryThresholdCritical": 90
}
```

**Fast polling:**
```json
{
  "aspireEndpoint": "http://localhost:5000",
  "pollingIntervalMs": 1000,
  "cpuThresholdWarning": 70,
  "cpuThresholdCritical": 90,
  "memoryThresholdWarning": 70,
  "memoryThresholdCritical": 90
}
```

**Sensitive thresholds:**
```json
{
  "aspireEndpoint": "http://localhost:5000",
  "pollingIntervalMs": 2000,
  "cpuThresholdWarning": 50,
  "cpuThresholdCritical": 75,
  "memoryThresholdWarning": 60,
  "memoryThresholdCritical": 80
}
```

**Remote HTTPS:**
```json
{
  "aspireEndpoint": "https://aspire-prod.example.com",
  "pollingIntervalMs": 5000,
  "cpuThresholdWarning": 70,
  "cpuThresholdCritical": 90,
  "memoryThresholdWarning": 70,
  "memoryThresholdCritical": 90
}
```

### Loading & Saving

```csharp
// Load from disk
var config = ConfigurationService.LoadConfiguration();

// Save to disk
ConfigurationService.SaveConfiguration(config);

// Get defaults
var defaults = new Configuration();
```

---

## Integration Examples

### Complete Polling Setup

```csharp
// Create configuration
var config = new Configuration
{
    AspireEndpoint = "http://localhost:5000",
    PollingIntervalMs = 2000
};

// Create services
var apiClient = new AspireApiClient(config);
var pollingService = new AspirePollingService(apiClient, config);
var statusCalculator = new StatusCalculator(config);

// Subscribe to events
pollingService.ResourcesUpdated += (sender, resources) =>
{
    var status = statusCalculator.CalculateOverallStatus(resources);
    Console.WriteLine($"Overall Status: {status}");
};

pollingService.StatusChanged += (sender, message) =>
{
    Console.WriteLine($"Status: {message}");
};

pollingService.ErrorOccurred += (sender, error) =>
{
    Console.WriteLine($"Error: {error}");
};

// Start polling
pollingService.Start();

// Later: Stop
pollingService.Stop();
```

### Manual Refresh

```csharp
// Poll on-demand
await pollingService.RefreshAsync();

// Or direct API call
var resources = await apiClient.GetResourcesAsync();
foreach (var resource in resources)
{
    var status = statusCalculator.CalculateStatusFromMetrics(resource.Metrics);
    Console.WriteLine($"{resource.Name}: {status}");
}
```

---

## See Also

- [Quick Start Guide](./QUICKSTART.md)
- [Architecture Guide](./architecture.md)
- [Configuration Guide](./configuration.md)
- [Troubleshooting Guide](./troubleshooting.md)

---

**Last Updated:** 2026-04-27  
**Version:** 1.0.0
