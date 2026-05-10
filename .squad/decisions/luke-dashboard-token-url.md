# Dashboard URL Must Preserve Login Token

**Date:** 2025-04-26  
**Author:** Luke (Backend Dev)  
**Status:** Implemented  
**Commit:** ffec33e

## Problem

The dashboard URL detected from `aspire ps` was being stripped of its query parameter (`?t=<token>`), causing the mini window's dashboard link to open the login page instead of auto-logging in.

## Root Cause

- `AspireCommandService.ParseEndpointFromAspirePs` explicitly removed query parameters: `endpoint.Split('?')[0]`
- The regex pattern only matched `https?://(?:localhost|127\.0\.0\.1):\d+` without capturing path or query string
- Aspire CLI emits URLs like `http://localhost:15295/login?t=7b7d4e7883655b455caa4a13b62a0427` where the token is required for passwordless login

## Solution

### 1. JSON Format (Primary)
- Call `aspire ps --format json` to get structured output
- Parse JSON array and extract `dashboardUrl` field (includes full URL with token)
- JSON shape:
  ```json
  [{
    "appHostPath": "...",
    "appHostPid": 73968,
    "cliPid": 77692,
    "dashboardUrl": "http://localhost:15295/login?t=7b7d4e7883655b455caa4a13b62a0427"
  }]
  ```

### 2. Text Format (Fallback)
- If JSON parsing fails (older CLI versions), fall back to text regex
- Updated regex to preserve path and query string: `https?://(?:localhost|127\.0\.0\.1):\d+(?:/[^\s]*)?`
- Removed `endpoint.Split('?')[0]` line that stripped query parameters

### 3. Testing
- Updated test `ParseEndpointFromAspirePs_WithQueryParams_PreservesQueryParams` to assert query strings are preserved (was `RemovesQueryParams` before)
- All 260 tests pass

## Impact

- Users can now click the dashboard link in the mini window and land directly on the dashboard (auto-authenticated)
- No more manual token entry required
- Backward compatible: falls back to text parsing if JSON unavailable

## Files Changed

- `AspireCommandService.cs`: Added `ParseEndpointFromAspirePsJson`, updated `DetectAspireEndpointAsync` and `ParseEndpointFromAspirePs`
- `AspireCommandServiceTests.cs`: Updated test assertion to verify query strings are preserved

## Related

- User feedback: "the url for the dashboard does include the token, so we can't login while clicking on the link"
- Decision: Always preserve `?t=<token>` for auto-login functionality
