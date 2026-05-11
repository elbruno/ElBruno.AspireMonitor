# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.9.0] - 2026-05-11

### Added
- Aspire state-change notifications: Windows notifications when Aspire transitions between running and not running states.
- Settings toggle `notifyOnStateChange` (enabled by default) to control state-change notification delivery.

### Changed
- Notification center integration for Windows state-change alerts without disrupting user workflow.

## [1.8.0] - 2026-05-11

### Added
- Mini-window main-resource filter setting that defaults to endpoint-bearing/main resource entries while preserving all-resource display as an option.
- Settings toggle for showing only main mini-window resources.
- SampleHarness duplicate resource validation for endpoint and no-endpoint resources with the same prefix.

### Fixed
- Normalized application version display so the main and mini windows stay aligned without double `v` prefixes.

## [1.7.0] - 2026-05-10

### Added
- Mini monitor pinned-resource telemetry for CPU, memory, disk, resource type, endpoints, environment, and status without showing a fake GPU metric.
- Settings toggle to show or hide mini monitor telemetry, enabled by default.

### Changed
- Corrected Aspire CLI parsing and start-command documentation for current Aspire behavior.
- Hardened Start button coverage with a test lock to prevent regressions.
- Improved backend telemetry parsing resilience for resource details and metric payloads.

## [1.6.0] - 2026-05-10

### Added
- Aspire 13.3-aligned dashboard endpoint defaults using `http://localhost:18888`.
- Rich resource telemetry in the monitor UI, including resource type, disk usage percentage, endpoint counts, and environment badges.
- A maintained Aspire sample harness under `src/SampleHarness/` for end-to-end and regression validation.

### Changed
- Main monitor configuration and settings now preserve the standard Aspire dashboard URL and development-resource filtering option.
- Documentation and governance now identify the sample harness as the canonical validation target for monitor features.

## [1.5.0] - 2026-04-27

### Changed
- **Mini window header is now compact.** Start/Stop actions moved onto the header row beside the close button as small single-glyph buttons (▶ / ⏹ / ✕, ~24×22 px) with tooltips. The dedicated Control Buttons row and its dividers were removed, reducing window height noticeably while keeping color cues (green Start, red Stop).

### Added
- Promotional refresh for the v1.4 line: updated blog, LinkedIn and Twitter copy plus AI-generated visuals under `docs/promotional/`.

### Notes
- No public API or NuGet packaging changes. The global tool (`aspiremon`) is published from the same `ElBruno.AspireMonitor.Tool` package, now versioned 1.5.0.

[1.9.0]: https://github.com/elbruno/ElBruno.AspireMonitor/releases/tag/v1.9.0
[1.8.0]: https://github.com/elbruno/ElBruno.AspireMonitor/releases/tag/v1.8.0
[1.7.0]: https://github.com/elbruno/ElBruno.AspireMonitor/releases/tag/v1.7.0
[1.6.0]: https://github.com/elbruno/ElBruno.AspireMonitor/releases/tag/v1.6.0
[1.5.0]: https://github.com/elbruno/ElBruno.AspireMonitor/releases/tag/v1.5.0
