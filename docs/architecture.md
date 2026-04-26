# AspireMonitor Architecture

## Overview

AspireMonitor is a Windows system tray application that provides real-time monitoring of .NET Aspire distributed applications. It enables developers to track CPU, memory, and health status of Aspire resources at a glance through a lightweight, always-available tray icon with color-coded status indicators and clickable resource URLs.

## System Architecture

### High-Level Design

```
Aspire HTTP API
       ↓
[AspireApiClient] — HTTP wrapper with retry logic
       ↓
[AspirePollingService] — Background polling thread
       ↓
[StatusCalculator] — CPU/Memory threshold evaluation
       ↓
[MainViewModel] — MVVM data binding
       ↓
[WPF UI] — Notification window + system tray icon
```

### Key Components

#### 1. **AspireApiClient**
- **Purpose**: HTTP communication with Aspire dashboard API
- **Responsibility**: 
  - Fetches resource list from `/api/resources`
  - Retrieves individual resource details
  - Handles timeouts and HTTP errors gracefully
- **Dependencies**: None (uses standard HttpClient)
- **Key Methods**: `GetResourcesAsync()`, `GetResourceDetailsAsync(id)`

#### 2. **AspirePollingService**
- **Purpose**: Orchestrates continuous monitoring in background thread
- **Responsibility**:
  - Runs polling loop at configurable interval (default 2000ms)
  - Maintains state machine: `Connecting → Connected → Polling → Error → Reconnecting`
  - Triggers UI updates via `OnResourcesUpdated` event
  - Handles connection failures with exponential backoff
- **Dependencies**: AspireApiClient, StatusCalculator, ConfigurationService
- **State Machine**: 
  - **Connecting**: Initial state, attempting first connection
  - **Connected**: Successfully fetched resources at least once
  - **Polling**: Normal operation, updating resources
  - **Error**: Connection failed, retrying
  - **Reconnecting**: Attempting recovery from error state

#### 3. **StatusCalculator**
- **Purpose**: Evaluates resource health based on CPU/memory metrics
- **Responsibility**:
  - Calculates overall status (Green/Yellow/Red)
  - Applies configurable thresholds
  - Determines tray icon color
- **Dependencies**: ConfigurationService
- **Logic**:
  - **Green** 🟢: All resources < 70%
  - **Yellow** 🟡: Any resource 70-90%
  - **Red** 🔴: Any resource > 90% OR connection error

#### 4. **ConfigurationService**
- **Purpose**: Manages application configuration
- **Responsibility**:
  - Loads/saves settings to `AppData\Local\ElBruno\AspireMonitor\config.json`
  - Provides default values
  - Validates configuration
- **Dependencies**: FileSystem
- **Configuration Properties**:
  - `aspireEndpoint`: Aspire API base URL (required)
  - `pollingIntervalMs`: Refresh interval (default 2000)
  - `cpuThresholdWarning`: CPU warning % (default 70)
  - `cpuThresholdCritical`: CPU critical % (default 90)
  - `memoryThresholdWarning`: Memory warning % (default 70)
  - `memoryThresholdCritical`: Memory critical % (default 90)

#### 5. **MainViewModel** (MVVM)
- **Purpose**: Bridges services and UI via data binding
- **Responsibility**:
  - Exposes `ObservableCollection<ResourceViewModel>` for UI
  - Exposes current status color, connection state
  - Implements `INotifyPropertyChanged` for binding updates
  - Forwards user commands (refresh, navigate URL) to services
- **Dependencies**: AspirePollingService, ConfigurationService

#### 6. **WPF UI Layer**
- **Purpose**: Windows notification window and system tray integration
- **Responsibility**:
  - Renders resource list with status colors
  - Provides clickable URLs to open resources
  - System tray icon with context menu
  - Minimize/restore/exit functionality
- **Dependencies**: MainViewModel, NotifyIcon from Windows Forms

## Data Flow

### Normal Polling Cycle

```
1. AspirePollingService triggers at interval
2. AspireApiClient fetches /api/resources from Aspire
3. JSON response parsed into ResourceModel objects
4. StatusCalculator evaluates CPU/memory for each resource
5. Overall status (Green/Yellow/Red) determined
6. MainViewModel notifies WPF UI of updates
7. UI re-renders resource list and updates tray icon color
```

### Error Handling Flow

```
Connection Failure
    ↓
Exponential backoff retry (1s, 2s, 4s, 8s, max 30s)
    ↓
If recovery: Transition to Connected state
If persistent: Show error state in UI, red tray icon
    ↓
Auto-reconnect when user opens app or after max backoff
```

## Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| UI | WPF (XAML/C#) | Windows notification window + system tray |
| Framework | .NET 10 | Runtime environment |
| Architecture | MVVM | Data binding, testable logic separation |
| HTTP | HttpClient | API communication |
| Config | JSON | Settings persistence |
| Testing | xUnit + Moq | Unit and integration tests |

## Design Patterns

- **MVVM (Model-View-ViewModel)**: Complete separation of UI from business logic, enables testability
- **Service Locator** (implicit): ConfigurationService provides singleton configuration
- **Observer Pattern**: Event-driven updates from PollingService to ViewModel
- **Retry Pattern**: Exponential backoff in AspireApiClient for transient failures
- **State Machine**: PollingService maintains discrete connection states
- **Factory Pattern** (implicit): ResourceViewModel created from API resource data

## Deployment Architecture

### Local Development
- Single Windows machine with .NET 10 SDK
- Aspire running locally on configurable endpoint (default http://localhost:5000)
- Config file auto-created in user's AppData folder
- App runs as foreground process or system tray daemon

### Production / Distribution
- Packaged as .NET Global Tool (FrameworkDependentExecutable)
- Published to NuGet: `dotnet tool install --global ElBruno.AspireMonitor`
- Cross-machine: Any Windows 10/11 with .NET 10 runtime
- Can monitor Aspire instances on different machines (via endpoint URL config)

## Performance Considerations

### Scalability
- **Resource Count**: Tested with 1000+ resource list items
- **Polling Interval**: Configurable 500ms-30000ms, default 2000ms balances responsiveness vs. load
- **Memory**: Lightweight MVVM binding, minimal memory footprint
- **CPU**: Polling thread is I/O-bound, low CPU usage between requests

### Optimization
- **Incremental Updates**: Only re-render changed resources
- **Async/Await**: Non-blocking HTTP calls and polling
- **Lazy Loading**: Resource details fetched on-demand if needed
- **Caching**: Last-known state used during transient failures

### Monitoring
- Connection state visible in tray icon color
- Polling interval logged at debug level
- API response times tracked (future: analytics)
- Error states surfaced to user via UI

## Configuration Storage

Configuration persists at:
```
C:\Users\{Username}\AppData\Local\ElBruno\AspireMonitor\config.json
```

Example:
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

## Future Architecture Plans

- **Remote Monitoring**: Connect to Aspire instances on remote machines via URL
- **Multi-Instance**: Monitor multiple Aspire apps simultaneously
- **Advanced Metrics**: Historical trending, threshold-based alerts
- **Plug-in System**: Allow custom status calculators or integrations
- **Web Dashboard**: Companion web UI for detailed metrics

---

*For troubleshooting architecture-related issues, see [troubleshooting.md](./troubleshooting.md) or open an issue.*
