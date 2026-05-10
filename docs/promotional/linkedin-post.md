# LinkedIn Posts — AspireMonitor v1.4.0

## Post 1: Quick Install Hook

**[Attach: ./screenshots/hero-banner.png]**

Tired of bouncing to a browser tab to check your Aspire resources?

```bash
dotnet tool install --global ElBruno.AspireMonitor
aspiremon
```

That's it. Tray app. Your AppHost status one click away. Pin the resources you care about, see their live URLs, Start/Stop without context switching.

v1.4.0 is live: https://www.nuget.org/packages/ElBruno.AspireMonitor

#dotnet #aspire #devtools

---

## Post 2: The Mini Window Problem / Solution

**[Attach: ./screenshots/social-card.png]**

The problem: You have 15 Aspire resources. You care about 3.

The solution: AspireMonitor's mini window. Pin them once (`web, store, gateway`), get a compact panel showing exactly those three with their live URLs and Start/Stop buttons.

Prefix match, case-insensitive. Works with Aspire replica suffixing too.

Grab it: https://github.com/elbruno/ElBruno.AspireMonitor

#aspire #dotnet #opensource

---

## Post 3: The Start/Stop Fix Nobody Asked For (But Everyone Needed)

v1.4.0 fixed two genuinely annoying UX bugs in AspireMonitor:

**Start button lied.** Enabled itself ~60s before resources showed up. Now: `⏳ Starting Aspire... (12 / 90s)` until everything's actually ready.

**Stop button didn't stop.** Now it does. Disables itself during shutdown so you know it's working.

Small polish. Huge difference in daily use.

github.com/elbruno/ElBruno.AspireMonitor

#dotnet #aspire #ux
