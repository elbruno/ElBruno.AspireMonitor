# Copilot Instructions for ElBruno.AspireMonitor

## Build, test, and lint commands

Run commands from the repository root (`C:\src\ElBruno.AspireMonitor`).

```powershell
# Restore + build
dotnet restore .\ElBruno.AspireMonitor.slnx
dotnet build .\ElBruno.AspireMonitor.slnx -c Release --no-restore

# Main test suite
dotnet test .\src\ElBruno.AspireMonitor.Tests\ElBruno.AspireMonitor.Tests.csproj --no-restore --verbosity minimal

# Run a single test class (example)
dotnet test .\src\ElBruno.AspireMonitor.Tests\ElBruno.AspireMonitor.Tests.csproj --filter "FullyQualifiedName~ConfigurationServiceBehaviorTests" --no-restore --verbosity minimal

# Sample harness validation suite
dotnet test .\src\SampleHarness\SampleHarness.Tests\SampleHarness.Tests.csproj --verbosity minimal

# Services/Models coverage gate used by publish CI
.\build\Test-CoverageGate.ps1 -Configuration Release -Threshold 80 -NoBuild

# Formatting/lint-style check
dotnet format --verify-no-changes
```

## High-level architecture

- **Desktop monitor app (`src/ElBruno.AspireMonitor/`)** is a WPF tray application (MVVM) that monitors Aspire resources.
- **Runtime composition happens in `App.xaml.cs`**: `ConfigurationService` → `AspireCliService` → `AspirePollingService` + `AspireCommandService` → `MainViewModel`/views + tray icon + state notifications.
- **CLI-first backend**: resource data comes from `aspire describe --format json` (parsed in `Services/AspireCliService.cs`), not direct dashboard HTTP polling.
- **Global tool packaging (`src/ElBruno.AspireMonitor.Tool/`)**: `aspiremon` launches bundled WPF payload; `build/Pack-Tool.ps1` publishes desktop binaries and injects them into the NuGet tool package.
- **Canonical E2E validation target** is `src/SampleHarness/` (governed in `docs/governance.md`), and its tests are part of release validation.

## Key codebase conventions

- Use **“Aspire”** in user-facing text/docs (not “.NET Aspire”).
- Keep dashboard default endpoint centralized at `Configuration.DefaultAspireEndpoint` (`http://localhost:18888`) and consume through configuration services/viewmodels.
- Preserve Aspire CLI JSON backward compatibility in parsers:
  - resource type may be `resourceType` or `type`
  - endpoints may come from `urls` or `endpoints`
- Start/Stop UI flows are command-driven (`MainViewModel.StartAspireCommand` / `StopAspireCommand`) and route through `IAspireCommandService`; stop uses non-interactive flags (`aspire stop --all --non-interactive`).
- In XAML, use the single `BoolToVisibilityConverter`; for inverse behavior use `ConverterParameter=Inverse` (do not introduce a separate inverse converter type).
- Mini monitor pinning is prefix-based (`MiniWindowResources`), with default filtering favoring endpoint-bearing “main” resources; keep related behavior aligned with `MiniWindowResourceFilteringTests`.
- When monitor behavior changes, update related docs in the same change set (especially `README.md`, `docs/sample-harness.md`, and governance-linked docs when topology/validation expectations change).

## MCP servers (recommended)

If you use MCP-enabled Copilot clients, configure these first for this repo:

- **GitHub MCP** for issues/PRs/actions/code search while working in this repository.
- **Aspire tools/MCP** for local Aspire status and orchestration commands (`aspire start`, `aspire describe`, logs).

Example `.copilot/mcp-config.json` shape:

```json
{
  "mcpServers": {
    "github": {
      "command": "npx",
      "args": ["-y", "@anthropic/github-mcp-server"],
      "env": {
        "GITHUB_TOKEN": "${GITHUB_TOKEN}"
      }
    },
    "aspire": {
      "command": "npx",
      "args": ["-y", "@microsoft/aspire-mcp-server"]
    }
  }
}
```
