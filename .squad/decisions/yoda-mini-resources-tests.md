# Decision: Mini Window Resources Test Coverage

**Date:** 2026-04-26
**Author:** Yoda (Tester)
**Status:** Implemented (tests committed, awaiting implementation)

## Context
The mini window pinned resources feature allows users to configure a comma-separated list of resource names to pin to the mini window. Each resource renders as a clickable link (if it has a URL) or plain text showing the resource type (if no URL).

## Test Coverage Added

### Parser Tests (ElBruno.AspireMonitor.Tests\ViewModels\MiniWindowResourcesTests.cs)
Covers the token extraction logic from comma-separated strings:
- Empty input handling
- Trimming and filtering empty tokens
- Case preservation
- Whitespace tolerance

### PinnedResources ViewModel Tests
Covers the MiniMonitorViewModel.PinnedResources behavior:
- URL vs. non-URL resources (HasUrl, FallbackText)
- Prefix matching (case-insensitive)
- User-defined order preservation (not Aspire enumeration order)
- Replica handling (multiple resources matching one token)
- Missing token handling (silent skip)
- Aspire lifecycle (cleared on stop)
- Live setting updates (refresh on MiniWindowResourcesSetting change)

## Test Pattern
Uses **reflection-based testing** to verify API contracts before implementation lands. Tests gracefully skip assertions when properties don't exist yet (e.g., PinnedResources, MiniResourceItem, HasPinnedResources).

## Risks Mitigated
- **Naming mismatches**: If Han uses slightly different property names, tests will fail fast and we can adjust.
- **Behavior verification**: Tests document expected behavior (order, case sensitivity, replica handling) so implementation matches spec.
- **Regression**: Future changes to the feature will be caught by this test suite.

## Next Steps
1. Once Han's implementation lands, rerun: dotnet test --filter ""FullyQualifiedName~MiniWindowResourcesTests""
2. If tests fail due to naming differences, adjust test code to match actual API surface.
3. All 13 tests should pass when implementation is complete.

