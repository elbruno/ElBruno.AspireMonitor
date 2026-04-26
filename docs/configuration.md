# AspireMonitor Configuration Guide

## Overview

AspireMonitor stores its configuration in a JSON file located at:
```
C:\Users\{Username}\AppData\Local\ElBruno\AspireMonitor\config.json
```

The configuration file is created automatically on first run. All settings are optional and have sensible defaults.

## Quick Start

### Step 1: Start AspireMonitor

```bash
aspire-monitor
```

### Step 2: Configure Aspire Endpoint

The first time you run AspireMonitor, it will prompt you to enter the Aspire endpoint URL:

```
Enter Aspire endpoint (e.g., http://localhost:5000): http://localhost:5000
```

### Step 3: Verify Configuration

Open `%APPDATA%\Local\ElBruno\AspireMonitor\config.json`:

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

## Configuration Properties

### Required

#### `aspireEndpoint` (string)
**Default:** None (must be set)

The HTTP base URL of your Aspire dashboard API.

```json
{
  "aspireEndpoint": "http://localhost:5000"
}
```

**Examples:**
- Local development: `http://localhost:5000`
- Remote machine: `http://192.168.1.100:5000`
- Docker container: `http://aspire-dashboard:5000`

---

### Optional

#### `pollingIntervalMs` (integer)
**Default:** 2000 (2 seconds)

How often AspireMonitor fetches updated resource data, in milliseconds.

```json
{
  "pollingIntervalMs": 2000
}
```

**Valid Range:** 500-30000ms

- **500ms**: Very responsive, but increases API load
- **2000ms** (default): Balanced responsiveness and low API load
- **5000ms**: Lower API load, slight delay in updates
- **30000ms**: Minimal API load, may miss rapid changes

---

#### `cpuThresholdWarning` (integer)
**Default:** 70 (%)

CPU usage threshold that triggers a yellow (warning) indicator.

```json
{
  "cpuThresholdWarning": 70
}
```

When any resource exceeds this percentage, the tray icon turns **yellow** 🟡.

---

#### `cpuThresholdCritical` (integer)
**Default:** 90 (%)

CPU usage threshold that triggers a red (critical) indicator.

```json
{
  "cpuThresholdCritical": 90
}
```

When any resource exceeds this percentage, the tray icon turns **red** 🔴.

---

#### `memoryThresholdWarning` (integer)
**Default:** 70 (%)

Memory usage threshold that triggers a yellow (warning) indicator.

```json
{
  "memoryThresholdWarning": 70
}
```

---

#### `memoryThresholdCritical` (integer)
**Default:** 90 (%)

Memory usage threshold that triggers a red (critical) indicator.

```json
{
  "memoryThresholdCritical": 90
}
```

---

## Full Configuration Example

```json
{
  "aspireEndpoint": "http://localhost:5000",
  "pollingIntervalMs": 3000,
  "cpuThresholdWarning": 75,
  "cpuThresholdCritical": 85,
  "memoryThresholdWarning": 75,
  "memoryThresholdCritical": 85
}
```

## Status Color Indicators

The tray icon color is determined by the highest resource usage:

| Color | Condition | Example |
|-------|-----------|---------|
| 🟢 Green | All resources below warning threshold | CPU 45%, Memory 60% |
| 🟡 Yellow | Any resource between warning and critical | CPU 75%, Memory 65% |
| 🔴 Red | Any resource above critical OR connection error | CPU 95%, Memory 60% |
| ⚫ Gray | Not connected to Aspire | Initial state, network down |

## Managing Configuration

### Edit Configuration File Directly

1. Press `Win + R` and type `%APPDATA%`
2. Navigate to `Local\ElBruno\AspireMonitor\`
3. Open `config.json` in your text editor
4. Save changes
5. Restart AspireMonitor for changes to take effect

### Reset to Defaults

Delete the configuration file:
```bash
del %APPDATA%\Local\ElBruno\AspireMonitor\config.json
```

AspireMonitor will regenerate it on next startup with default values.

## Common Scenarios

### Local Aspire Development

```json
{
  "aspireEndpoint": "http://localhost:5000",
  "pollingIntervalMs": 2000,
  "cpuThresholdWarning": 70,
  "cpuThresholdCritical": 90
}
```

### Docker-Based Aspire

```json
{
  "aspireEndpoint": "http://aspire-container:5000",
  "pollingIntervalMs": 2000
}
```

### Remote Aspire Instance

```json
{
  "aspireEndpoint": "http://aspire-server.example.com:5000",
  "pollingIntervalMs": 3000
}
```

### Performance-Focused (Lower API Load)

```json
{
  "pollingIntervalMs": 5000,
  "cpuThresholdWarning": 65,
  "cpuThresholdCritical": 85
}
```

### Aggressive Monitoring (Higher Sensitivity)

```json
{
  "pollingIntervalMs": 1000,
  "cpuThresholdWarning": 60,
  "cpuThresholdCritical": 80
}
```

## Troubleshooting

### "Can't find configuration file"

**Solution:** Run AspireMonitor once — it will auto-create `config.json` in the correct location.

### "Invalid endpoint URL"

**Solution:** Verify the URL is valid:
- Use `http://` (not `https://`)
- Include the port number
- No trailing slash

Example: ✅ `http://localhost:5000` vs ❌ `http://localhost:5000/`

### "Configuration not persisting"

**Solution:** Check AppData folder permissions:
1. Right-click `C:\Users\{Username}\AppData\Local\ElBruno\AspireMonitor\`
2. Click Properties → Security
3. Verify your user has "Write" permission
4. Restart AspireMonitor

### "Tray icon stays gray"

**Solution:** Verify Aspire is running and endpoint is correct:
1. Open browser: `http://localhost:5000` (or your configured endpoint)
2. If Aspire dashboard loads, check configuration
3. Increase `pollingIntervalMs` if API is slow

### "Updates are too slow"

**Solution:** Decrease `pollingIntervalMs`:
```json
{
  "pollingIntervalMs": 1000
}
```

---

*For more help, see [troubleshooting.md](./troubleshooting.md) or check the [architecture.md](./architecture.md) for system details.*
