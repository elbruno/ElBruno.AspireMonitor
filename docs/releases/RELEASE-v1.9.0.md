# Release Notes: ElBruno.AspireMonitor v1.9.0

**Release Date:** 2026-05-11  
**Focus:** Aspire state-change notifications, improved user workflow awareness

## What's New

### 🔔 Aspire State-Change Notifications

**What changed:**
- Windows notifications now alert you when your Aspire instance transitions between running and not-running states
- Notification triggers on:
  - **Aspire starts:** Resources become available after you click the Start button or launch manually
  - **Aspire stops:** Resources disappear, indicating your AppHost has shut down
- Notifications integrate seamlessly with Windows Notification Center
- No interruption to your workflow — notifications appear in the system tray area

**Why it matters:**
- You no longer need to keep AspireMonitor visible to know when Aspire is ready
- Perfect for multi-monitor setups or when you're focused on your IDE
- Reduces unnecessary tray-icon checking during development
- Enables non-intrusive monitoring of AppHost lifecycle events

**How to use:**
1. Install or update to v1.9.0: `dotnet tool update --global ElBruno.AspireMonitor`
2. Notifications are **enabled by default**
3. To disable: Open Settings and toggle "Notify on Aspire State Change" or edit `config.json`:
   ```json
   {
     "notifyOnStateChange": false
   }
   ```
4. Changes apply immediately

**Example notification sequence:**
```
Click Start in tray
↓
[Notification] "Aspire is running" (appears in notification center)
↓
Resources appear in monitor
↓
Click Stop in tray
↓
[Notification] "Aspire stopped" (appears in notification center)
```

---

## 🔄 Upgrade Guide

### For Current Users (v1.8.x → v1.9.0)

1. **Update the global tool:**
   ```bash
   dotnet tool update --global ElBruno.AspireMonitor
   ```

2. **Notifications are enabled by default** — you'll immediately see Windows notifications when Aspire starts/stops. If you prefer silent operation, disable in Settings.

3. **No configuration changes required** — your existing `config.json` is fully compatible. The new `notifyOnStateChange` setting defaults to `true`.

4. **Verify the feature:**
   - Click the Start button in the tray to launch Aspire
   - Watch for a Windows notification: "Aspire is running"
   - Click Stop to shut down
   - Another notification will appear: "Aspire stopped"

---

## 📊 v1.9.0 Quality & Completeness

| Feature | Status |
|---------|--------|
| **State-Change Notifications** | ✅ Implemented & Tested |
| **Settings Toggle** | ✅ Working (enabled by default) |
| **Notification Center Integration** | ✅ Windows-native |
| **Config Backward Compatibility** | ✅ Existing configs preserved |
| **Documentation** | ✅ Complete (config guide updated) |

---

## 🚀 Quick Start with v1.9.0

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

3. **Point to your Aspire AppHost folder:** When prompted, enter the path (or use existing config)

4. **Start Aspire and watch for notifications:**
   - Click Start in the tray
   - A Windows notification confirms when Aspire is running
   - Resources appear in the monitor
   - Click Stop to shut down and see the stopped notification

---

## 📚 Documentation Updates

- **[Configuration Guide](../configuration.md)** — See `notifyOnStateChange` setting documentation
- **[CHANGELOG.md](../../CHANGELOG.md)** — Full release history
- **[README.md](../../README.md)** — Updated features table with notifications

---

## 🙏 Feedback & Questions

- **GitHub Issues:** https://github.com/elbruno/ElBruno.AspireMonitor/issues
- **GitHub Discussions:** https://github.com/elbruno/ElBruno.AspireMonitor/discussions
- **Author:** Bruno Capuano ([@elbruno](https://github.com/elbruno))

---

**Version:** 1.9.0  
**Release Date:** 2026-05-11  
**Maintained by:** Han (Frontend Dev)
