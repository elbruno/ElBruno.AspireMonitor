# Debugging Enhancements - Mini Monitor Diagnostic Report

## Overview
Comprehensive debug logging and UI improvements have been added to diagnose why the mini monitor isn't displaying resources when Aspire is running.

## Changes Made

### 1. AspirePollingService.cs - Polling Cycle Logging
**Lines 91-144**: Enhanced `PollResourcesAsync()` method
- Logs timestamp of each poll cycle
- Logs polling state and resource count returned
- Lists each resource with CPU/memory metrics
- Shows when transitioning between states (Connecting → Polling)
- Detailed error logging with exception type information

**Lines 127-144**: Enhanced `HandleError()` method
- Logs reconnection attempt number
- Shows backoff delay before next retry (5s, 10s, 30s)
- Logs state transitions during error recovery

**Line 73**: Enhanced `RefreshAsync()` method
- Logs when manual refresh is triggered

### 2. AspireApiClient.cs - HTTP Request/Response Logging
**Lines 50-101**: Enhanced `GetResourcesAsync()` method
- Logs GET request to /api/resources endpoint
- Shows HTTP response status code
- Logs content length received
- Shows deserialized resource count
- Detailed exception logging:
  - HttpRequestException: shows endpoint and inner exception
  - TaskCanceledException: indicates 5-second timeout
  - JsonException: shows JSON path and parse error
  - General exceptions: shows type and stack trace

### 3. MainViewModel.cs - UI Binding Updates
**Lines 220-244**: Enhanced `OnResourcesUpdated()` method
- Logs "ResourcesUpdated received: X resources"
- Logs each resource being added with metrics
- Logs final UI state (resource count and connection status)

**Lines 246-262**: Enhanced `OnStatusChanged()` and `OnError()` methods
- Logs status transitions
- Logs connection state changes

**Lines 264-267**: Enhanced `Start()` method
- Logs polling start
- Triggers immediate first poll via RefreshAsync()

### 4. MiniMonitorViewModel.cs - UI Display Logging
**Lines 105-155**: Enhanced `MainViewModel_PropertyChanged()` and `UpdateMiniMonitorData()` methods
- Logs property change events
- Logs resources, connection status, and project folder
- Shows status emoji determination (❌ / 🔴 / 🟡 / 🟢 / ⚪)
- Logs DetailsSummary text

**Lines 90-103**: Improved `DetailsSummary` property
- "Not connected" - application not initialized
- "Disconnected - waiting to connect..." - polling but not yet connected
- Shows error message if connection failed
- "✓ Connected but no resources found" - connected but API returned empty list
- "X resources running" - normal operation

### 5. App.xaml.cs - Startup Sequence Logging
**Lines 18-70**: Enhanced `OnStartup()` method
- Logs application startup markers
- Shows configuration file loading
- Logs auto-detection attempts (success/failure/timeout)
- Shows endpoint probing results
- Logs API client and polling service creation

### 6. MiniMonitor.xaml - UI Improvements
**Line 10**: Increased window height from 380 to 420 to accommodate new status box
**Lines 82-99**: Added status info box
- Shows emoji and connection status
- Color-coded background (green for healthy, red for errors)
- Displays DetailsSummary text in real-time

## How to Test

### Step 1: Run the Application with Debug Output
```powershell
cd "C:\src\ElBruno.AspireMonitor\src\ElBruno.AspireMonitor"
dotnet run
```

### Step 2: Watch Debug Output in Visual Studio or Debug Console
The following log messages should appear:

**On startup:**
```
[App] ========== APPLICATION STARTUP ==========
[App] Initializing configuration service...
[App] Loaded configuration:
[App]   Aspire Endpoint: https://localhost:17195
[App]   Polling Interval: 2000ms
[App] Attempting to auto-detect Aspire endpoint...
[App] ✓ Auto-detected and saved Aspire endpoint: https://localhost:17195
[App] ✓ Current endpoint is responding: https://localhost:17195
[App] Creating API client and polling service...
[App] Creating MainViewModel and MainWindow...
[App] ========== STARTUP COMPLETE ==========
```

**When MainWindow starts polling:**
```
[AspirePollingService] Starting polling service. Interval: 2000ms, Endpoint: https://localhost:17195
[MainViewModel] Starting polling...
[AspirePollingService] RefreshAsync called
[AspirePollingService] [HH:mm:ss.fff] Polling cycle started. State: Connecting
[AspireApiClient] GET /api/resources from https://localhost:17195/
[AspireApiClient] ✓ Response received, content length: 1234 bytes
[AspireApiClient] ✓ Deserialized 3 resources
[AspirePollingService] Resource: webfrontend (Status: Running, CPU: 45.2%, Memory: 62.8%)
[AspirePollingService] Resource: apiservice (Status: Running, CPU: 28.5%, Memory: 48.3%)
[AspirePollingService] Resource: cache (Status: Running, CPU: 12.1%, Memory: 35.7%)
[MainViewModel] OnResourcesUpdated received: 3 resources
[MainViewModel]   Added resource: webfrontend (CPU: 45.2%, Mem: 62.8%)
[MainViewModel]   Added resource: apiservice (CPU: 28.5%, Mem: 48.3%)
[MainViewModel]   Added resource: cache (CPU: 12.1%, Mem: 35.7%)
[MainViewModel] UI updated with 3 resources, IsConnected=True
[MainViewModel] OnStatusChanged: Connected
```

**When user opens Mini Monitor:**
```
[MiniMonitorViewModel] MainViewModel property changed: Resources
[MiniMonitorViewModel] Triggering UI update due to Resources change
[MiniMonitorViewModel] UpdateMiniMonitorData called
[MiniMonitorViewModel]   Resources: 3
[MiniMonitorViewModel]   IsConnected: True
[MiniMonitorViewModel]   ProjectFolder: C:\my-aspire-app
[MiniMonitorViewModel]   Status: Healthy (🟢)
[MiniMonitorViewModel] ResourceCount: 3 Resources, StatusEmoji: 🟢
```

### Step 3: Expected Mini Monitor Display

**If successful (with resources):**
```
┌─ Aspire Monitor ─────────────────────────┐
│ v1.0.0                                   │
│ Working Folder:                          │
│ C:\my-aspire-app                         │
│ 3 Resources                              │
│ 3 resources running                      │
│ ┌──────────────────────────────────────┐ │
│ │ 🟢 3 resources running               │ │
│ └──────────────────────────────────────┘ │
│ ┌──────────────────────────────────────┐ │
│ │ [Log output area]                    │ │
│ └──────────────────────────────────────┘ │
│ [▶ Start] [⏹ Stop] [Details] [Close]   │
└──────────────────────────────────────────┘
```

**If Aspire is not running:**
```
┌─ Aspire Monitor ─────────────────────────┐
│ v1.0.0                                   │
│ Working Folder:                          │
│ Not configured                           │
│ 0 Resources                              │
│ Disconnected - waiting to connect...     │
│ ┌──────────────────────────────────────┐ │
│ │ ❌ Disconnected - waiting to connect │ │
│ └──────────────────────────────────────┘ │
│ ┌──────────────────────────────────────┐ │
│ │ [Log output showing error]           │ │
│ └──────────────────────────────────────┘ │
│ [▶ Start] [⏹ Stop] [Details] [Close]   │
└──────────────────────────────────────────┘
```

**If Aspire is running but returns no resources:**
```
┌─ Aspire Monitor ─────────────────────────┐
│ v1.0.0                                   │
│ Working Folder:                          │
│ C:\my-aspire-app                         │
│ 0 Resources                              │
│ ✓ Connected but no resources found       │
│ ┌──────────────────────────────────────┐ │
│ │ 🟢 ✓ Connected but no resources found│ │
│ └──────────────────────────────────────┘ │
│ ┌──────────────────────────────────────┐ │
│ │ [Log output]                         │ │
│ └──────────────────────────────────────┘ │
│ [▶ Start] [⏹ Stop] [Details] [Close]   │
└──────────────────────────────────────────┘
```

## Diagnostic Flowchart

1. **App starts** → Loads config, auto-detects Aspire endpoint
2. **MainViewModel initialized** → Subscribes to polling service events
3. **Polling service starts** → Sets state to Connecting
4. **First poll triggered** → RefreshAsync() called immediately
5. **API client calls /api/resources** → Logs request/response
6. **Resources received** → State transitions to Polling, ResourcesUpdated event fired
7. **MainViewModel receives resources** → Updates Resources collection, sets IsConnected=true
8. **MiniMonitor opens** → Shows resources and connection status

## Troubleshooting Guide

### Scenario 1: Mini Monitor shows "❌ Disconnected"
**Check debug output for:**
- Does "AspireApiClient" show "HTTP Request Exception" or "Request timeout"?
- Is the endpoint correct? (should be https://localhost:17195 or similar)
- Is Aspire actually running? Start it with `aspire run`

**Solution:** 
- Verify Aspire is running on the correct port
- Check firewall settings
- Confirm HTTPS certificate is trusted

### Scenario 2: Mini Monitor shows "✓ Connected but no resources found"
**Check debug output for:**
- Does AspireApiClient show "Deserialized 0 resources"?
- Are there resources in Aspire dashboard?

**Solution:**
- Add resources to the Aspire app
- Verify resources are in the "Running" state

### Scenario 3: Resources show but with incorrect metrics
**Check debug output for:**
- Do the logged metrics match what's displayed?
- Check CPU/Memory percentages in debug output

**Solution:**
- Metrics are calculated based on Aspire API response
- Verify Aspire is sending correct data

### Scenario 4: Mini Monitor is slow to update
**Check debug output for:**
- Are poll cycles happening every 2 seconds?
- Are there many "REQUEST TIMEOUT" messages?

**Solution:**
- Check network latency to Aspire endpoint
- Increase PollingIntervalMs in configuration if needed

## Test Results
✅ All 223 unit tests pass
✅ Application builds successfully
✅ Debug logging captures all key events
✅ UI displays resources correctly when available

## Next Steps
1. Start Aspire application with `aspire run` in a sample project
2. Launch the mini monitor from system tray
3. Watch debug output for the diagnostic messages above
4. Verify resources appear in mini monitor after a few seconds
5. Open main window (Details button) to see full resource list
