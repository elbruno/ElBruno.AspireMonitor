# Troubleshooting Guide

## General Issues

### AspireMonitor won't start

**Symptoms:** App crashes or shows error message on launch

**Solutions:**
1. **Verify .NET 10 installed:**
   ```bash
   dotnet --version
   ```
   Should show version 10.0 or later. If not, [download .NET 10](https://dotnet.microsoft.com/en-us/download).

2. **Clear cache:**
   ```bash
   dotnet tool uninstall --global ElBruno.AspireMonitor
   dotnet tool install --global ElBruno.AspireMonitor
   ```

3. **Check Windows OS:**
   AspireMonitor requires **Windows 10 or later** (WPF is Windows-only).

4. **Check logs:**
   Look for errors in `%APPDATA%\Local\ElBruno\AspireMonitor\` or console output.

---

### System tray icon not visible

**Symptoms:** App runs but icon doesn't appear in system tray

**Solutions:**
1. **Click notification area:**
   - Look for arrow ▲ in bottom-right corner of taskbar
   - Click to show hidden icons
   - AspireMonitor might be hidden

2. **Adjust Windows settings:**
   - Settings → System → Notifications & actions
   - Scroll to "App and notification settings"
   - Find AspireMonitor, ensure it's allowed in system tray

3. **Restart app:**
   ```bash
   # Kill the process
   taskkill /IM "aspire-monitor.exe" /F
   
   # Restart
   aspire-monitor
   ```

4. **Check screen resolution:**
   Very high DPI displays sometimes cause rendering issues. Try restarting.

---

### Application crashes on startup

**Symptoms:** App crashes immediately after running `aspire-monitor`

**Solutions:**
1. **Check configuration file:**
   ```bash
   notepad %APPDATA%\Local\ElBruno\AspireMonitor\config.json
   ```
   Verify JSON is valid. Use online JSON validator if unsure.

2. **Reset configuration:**
   ```bash
   del %APPDATA%\Local\ElBruno\AspireMonitor\config.json
   ```
   App will recreate it on next run.

3. **Reinstall:**
   ```bash
   dotnet tool uninstall --global ElBruno.AspireMonitor
   dotnet tool install --global ElBruno.AspireMonitor
   ```

---

## Connection Issues

### Can't connect to Aspire

**Symptoms:** Tray icon stuck gray (🔴 gray), no resources shown

**Solutions:**
1. **Verify Aspire is running:**
   - Open browser: `http://localhost:5000` (or your configured endpoint)
   - Should see Aspire dashboard
   - If blank/error, Aspire isn't running

2. **Check endpoint configuration:**
   ```bash
   notepad %APPDATA%\Local\ElBruno\AspireMonitor\config.json
   ```
   Verify `aspireEndpoint` is correct:
   - ✅ `http://localhost:5000`
   - ❌ `https://localhost:5000` (HTTPS not supported yet)
   - ❌ `http://localhost:5000/` (no trailing slash)

3. **Test endpoint directly:**
   ```bash
   # Windows PowerShell
   Invoke-RestMethod -Uri "http://localhost:5000/api/resources"
   
   # Or open in browser
   http://localhost:5000/api/resources
   ```
   Should return JSON list of resources.

4. **Check firewall:**
   Windows Defender Firewall might block the connection:
   - Control Panel → Windows Defender Firewall → Allow apps through firewall
   - Check if `.NET Runtime` or `dotnet` is allowed
   - If not, add it to allowed apps

5. **Increase polling interval:**
   If Aspire API is slow, try:
   ```json
   {
     "pollingIntervalMs": 5000
   }
   ```

---

### Intermittent connection drops

**Symptoms:** Tray icon flickers between gray and colored states

**Solutions:**
1. **Check Aspire stability:**
   - Aspire might be crashing or restarting
   - Open Aspire dashboard and check for errors

2. **Increase polling interval:**
   ```json
   {
     "pollingIntervalMs": 3000
   }
   ```
   Reduces API load, may reduce connection instability.

3. **Check network:**
   - If Aspire is remote, verify network connection
   - Ping the machine: `ping aspire-server`
   - Check for packet loss

---

### "Connection refused" error

**Symptoms:** Error message mentions "connection refused" or port

**Solutions:**
1. **Port is in use:**
   ```bash
   # Find process using port 5000
   netstat -ano | findstr :5000
   
   # Kill the process (replace PID with actual number)
   taskkill /PID [PID] /F
   ```

2. **Wrong port number:**
   Verify Aspire is running on configured port:
   ```bash
   # Check Aspire process
   netstat -ano | findstr "dotnet"
   ```

3. **Aspire not started:**
   Start Aspire dashboard first, then AspireMonitor.

---

## Configuration Issues

### Configuration file not found

**Symptoms:** Error about missing config file

**Solution:** 
AspireMonitor auto-creates config on first run. If missing:
1. Run `aspire-monitor` — it will create the file
2. Or manually create `%APPDATA%\Local\ElBruno\AspireMonitor\config.json`

---

### Configuration changes not taking effect

**Symptoms:** Changed config but AspireMonitor still uses old values

**Solutions:**
1. **Restart AspireMonitor:**
   ```bash
   taskkill /IM "aspire-monitor.exe" /F
   aspire-monitor
   ```

2. **Verify file saved:**
   ```bash
   notepad %APPDATA%\Local\ElBruno\AspireMonitor\config.json
   ```
   Check changes are present and JSON is valid.

3. **Check file permissions:**
   Right-click config.json → Properties → Security
   Verify your user has "Write" permission.

---

### Invalid JSON in configuration file

**Symptoms:** App crashes after editing config

**Solution:**
1. **Open file:**
   ```bash
   notepad %APPDATA%\Local\ElBruno\AspireMonitor\config.json
   ```

2. **Validate JSON:**
   Copy content and paste into [jsonlint.com](https://jsonlint.com/)
   Check for:
   - Missing commas between properties
   - Mismatched quotes
   - Trailing commas

3. **Fix example:**
   ```json
   // ❌ Wrong — missing comma after "aspireEndpoint"
   {
     "aspireEndpoint": "http://localhost:5000"
     "pollingIntervalMs": 2000
   }
   
   // ✅ Correct
   {
     "aspireEndpoint": "http://localhost:5000",
     "pollingIntervalMs": 2000
   }
   ```

4. **Reset if needed:**
   ```bash
   del %APPDATA%\Local\ElBruno\AspireMonitor\config.json
   ```

---

## Resource Monitoring Issues

### Resources not updating

**Symptoms:** Resource list shows stale data

**Solutions:**
1. **Check polling interval:**
   ```json
   {
     "pollingIntervalMs": 2000
   }
   ```
   Is this reasonable (1000-10000ms)?

2. **Check Aspire API:**
   Test directly in browser:
   ```
   http://localhost:5000/api/resources
   ```
   Should return fresh resource list.

3. **Increase interval if Aspire is slow:**
   ```json
   {
     "pollingIntervalMs": 5000
   }
   ```

4. **Check CPU/memory calculations:**
   If numbers seem wrong, verify thresholds in config:
   ```json
   {
     "cpuThresholdWarning": 70,
     "cpuThresholdCritical": 90
   }
   ```

---

### Tray icon color always red

**Symptoms:** Tray icon always 🔴 red even when resources are low

**Solutions:**
1. **Check thresholds:**
   ```bash
   notepad %APPDATA%\Local\ElBruno\AspireMonitor\config.json
   ```
   Are thresholds reasonable?
   ```json
   {
     "cpuThresholdCritical": 90,
     "memoryThresholdCritical": 90
   }
   ```

2. **Lower thresholds if appropriate:**
   ```json
   {
     "cpuThresholdWarning": 60,
     "cpuThresholdCritical": 80
   }
   ```

3. **Check Aspire API:**
   Is it returning valid resource data?
   ```bash
   Invoke-RestMethod -Uri "http://localhost:5000/api/resources"
   ```

---

### Resource URLs not clickable

**Symptoms:** Resource names are not links

**Solutions:**
1. **Verify Aspire API returns URLs:**
   ```bash
   Invoke-RestMethod -Uri "http://localhost:5000/api/resources"
   ```
   Each resource should have a valid URL field.

2. **Check URL format:**
   Must be valid HTTP/HTTPS URL:
   - ✅ `http://localhost:3000`
   - ✅ `https://api.example.com`
   - ❌ `localhost:3000` (missing scheme)
   - ❌ `ftp://invalid` (FTP not supported)

3. **Restart AspireMonitor:**
   ```bash
   taskkill /IM "aspire-monitor.exe" /F
   aspire-monitor
   ```

---

## Performance Issues

### High CPU usage

**Symptoms:** AspireMonitor consuming significant CPU

**Solutions:**
1. **Increase polling interval:**
   ```json
   {
     "pollingIntervalMs": 5000
   }
   ```
   Default 2s might be aggressive.

2. **Check Aspire API response time:**
   Test in browser:
   ```
   http://localhost:5000/api/resources
   ```
   If slow, increase interval.

3. **Check resource count:**
   If monitoring 1000+ resources, increase interval:
   ```json
   {
     "pollingIntervalMs": 10000
   }
   ```

---

### High memory usage

**Symptoms:** AspireMonitor using significant RAM

**Solutions:**
1. **Restart the app:**
   ```bash
   taskkill /IM "aspire-monitor.exe" /F
   aspire-monitor
   ```

2. **Check resource count:**
   If monitoring many resources, this is normal.

3. **Report memory leak:**
   If memory keeps growing over time, [open an issue](https://github.com/elbruno/ElBruno.AspireMonitor/issues).

---

## Advanced Debugging

### Enable verbose logging

Create or edit `%APPDATA%\Local\ElBruno\AspireMonitor\config.json`:

```json
{
  "logLevel": "Debug"
}
```

Restart app and check console output for detailed logs.

### Check Windows Event Viewer

For system-level errors:
1. Press `Win + R`
2. Type `eventvwr`
3. Look for application errors related to AspireMonitor

### Test with different .NET versions

```bash
# List installed .NET runtimes
dotnet --info

# Install .NET 10 if missing
# https://dotnet.microsoft.com/en-us/download
```

---

## Getting Help

If you're stuck:

1. **Check this guide** for similar symptoms
2. **Review logs** in `%APPDATA%\Local\ElBruno\AspireMonitor\`
3. **Test manually** (e.g., open Aspire API in browser)
4. **Open a GitHub issue** with:
   - Your OS version (Windows 10/11/etc.)
   - .NET version output (`dotnet --version`)
   - AspireMonitor version
   - Error messages from logs
   - Steps to reproduce

### Resources

- [Architecture Guide](./architecture.md) — System design and components
- [Configuration Guide](./configuration.md) — All configuration options
- [Development Guide](./development-guide.md) — Building from source
- [GitHub Issues](https://github.com/elbruno/ElBruno.AspireMonitor/issues) — Report bugs
- [GitHub Discussions](https://github.com/elbruno/ElBruno.AspireMonitor/discussions) — Ask questions

---

*Last updated: 2026-04-26 (Phase 4)*
