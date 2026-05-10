# Project Context

- **Project:** ElBruno.AspireMonitor
- **Created:** 2026-04-26

## Core Context

Agent Ralph initialized and ready for work.

## Recent Updates

📌 Team initialized on 2026-04-26

## Learnings

Initial setup complete.

---

## Testing Notes

### 2026-04-26 — Endpoint Detection JSON Format

**For Testers/QA:**

Luke's endpoint detection has been updated to use spire ps --format json format. Key points for testing:

1. **JSON Format (Primary Method):**
   - Command: spire ps --format json
   - Output: Array of objects with properties including:
     - ppHostPath: Path to app host
     - ppHostPid: Process ID of app host
     - cliPid: CLI process ID
     - dashboardUrl: Full URL with authentication token, e.g., http://localhost:15295/login?t=7b7d4e7883655b455caa4a13b62a0427
   - JSON parsing preserves token in URL

2. **Fallback Text Format (Older CLI Versions):**
   - If JSON parsing fails, regex fallback is used
   - Updated regex preserves query strings: https?://(?:localhost|127\.0\.0\.1):\d+(?:/[^\s]*)?
   - Still extracts endpoint correctly from text output

3. **Testing Dashboard Link:**
   - Users should be able to click dashboard link in mini window
   - Link should open dashboard and auto-authenticate (token in URL)
   - No manual token entry required

4. **Verification:**
   - All 260 tests pass
   - Test: ParseEndpointFromAspirePs_WithQueryParams_PreservesQueryParams validates token preservation
   - Backward compatibility verified

**Related:**
- .squad/decisions/luke-dashboard-token-url.md (full decision record)
- Luke's commit ffec33e (implementation)
- Han's auto-resize feature (commit d90c563)
