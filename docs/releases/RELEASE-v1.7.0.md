# ElBruno.AspireMonitor v1.7.0

Published: 2026-05-10

## Highlights

- Mini monitor pinned-resource telemetry now shows CPU, memory, disk, resource type, endpoints, environment, and status.
- The mini monitor no longer displays a fake GPU metric.
- Added a settings toggle to show or hide mini monitor telemetry; it defaults to enabled.
- Corrected Aspire CLI parsing/start-command documentation and locked Start button behavior with tests.
- Hardened backend telemetry parsing for resource and metric payloads.

## Validation

- `dotnet build ElBruno.AspireMonitor.slnx -c Release`
- `dotnet test src\ElBruno.AspireMonitor.Tests\ElBruno.AspireMonitor.Tests.csproj -c Release --no-build --verbosity normal`
- `build\Test-CoverageGate.ps1 -Configuration Release -Threshold 80 -NoBuild`
- `dotnet test src\SampleHarness\SampleHarness.Tests\SampleHarness.Tests.csproj -c Release --verbosity normal`
- `build\Pack-Tool.ps1 -Configuration Release -Version 1.7.0`
