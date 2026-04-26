# Phase 1 Backend Tests - ElBruno.AspireMonitor

## Overview
This document describes the comprehensive test suite for the Phase 1 backend layer of ElBruno.AspireMonitor. The tests validate CLI command execution, data models, and service interactions with mock implementations.

## Test Structure

### 1. AspireCommandServiceTests.cs
**Location:** `Services/AspireCommandServiceTests.cs`
**Focus:** Unit tests for CLI command wrapper
**Coverage:**
- ✅ Command execution validation (start, stop, describe, ps, logs)
- ✅ Working folder validation (null, empty, invalid paths)
- ✅ Log callback invocation for real-time streaming
- ✅ Endpoint URL parsing from `aspire ps` output
- ✅ Query parameter removal from URLs
- ✅ Timeout handling and error recovery
- ✅ Concurrent command execution
- ✅ Regular expression validation for endpoint detection

**Key Test Cases:**
- `StartAspireAsync_WithValidFolder_ReturnsTrue` - Validates successful start command
- `StartAspireAsync_WithInvalidFolder_ReturnsFalse` - Tests folder validation
- `DetectAspireEndpointAsync_ParsesEndpointFromOutput` - URL extraction from CLI output
- `ParseEndpointFromAspirePs_ValidUrls_ExtractsCorrectly` - Regex parsing tests
- `CommandExecutionTimeout_HandlesGracefully` - Timeout handling

### 2. DataModelTests.cs
**Location:** `Models/DataModelTests.cs`
**Focus:** Unit tests for data models and DTOs
**Coverage:**

#### AspireResource Tests
- ✅ Default constructor initialization
- ✅ Parameterized constructor with id, name, status
- ✅ Property setters and getters
- ✅ Multiple endpoints storage
- ✅ All ResourceStatus enum values
- ✅ Nullable Type field handling

#### ResourceMetrics Tests
- ✅ Default constructor (zeros)
- ✅ Parameterized constructor (CPU, memory, disk)
- ✅ Optional disk parameter default
- ✅ Property value ranges (including negative and >100)
- ✅ Metric field updates

#### ResourceStatus Tests
- ✅ Enum value count verification
- ✅ Enum value ordering (Unknown=0, Running=1, etc.)
- ✅ String conversion
- ✅ Parsing from string
- ✅ All enum values validity

#### AspireConfiguration Tests
- ✅ Default value initialization (endpoint, intervals, thresholds)
- ✅ Property setters for all configuration fields
- ✅ Polling interval variations (1000ms - 10000ms)
- ✅ Endpoint URL format variations
- ✅ Threshold customization (CPU/memory warning/critical)

**Key Test Cases:**
- `AspireResource_PropertySetters_UpdateValues` - Full property validation
- `ResourceMetrics_VariousValues_StoredCorrectly` - Metric data integrity
- `ResourceStatus_AllValues_ParseCorrectly` - Enum parsing
- `Configuration_DefaultConstructor_InitializesWithDefaults` - Default config

### 3. Test Fixtures
**Location:** `Fixtures/`
**Files:**
- `aspire-describe-valid.json` - Valid JSON response with 3 resources
- `aspire-describe-empty.json` - Empty resource list
- `aspire-ps-output.txt` - Sample `aspire ps` command output
- `aspire-logs-sample.txt` - Sample log lines for streaming tests

## Test Execution

### Build Tests
```powershell
cd src\ElBruno.AspireMonitor.Tests
dotnet build --configuration Release
```

### Run All Phase 1 Tests
```powershell
dotnet test --filter "FullyQualifiedName~AspireCommandServiceTests|FullyQualifiedName~DataModelTests"
```

### Run Specific Test Categories
```powershell
# CLI Service Tests Only
dotnet test --filter "FullyQualifiedName~AspireCommandServiceTests"

# Data Model Tests Only
dotnet test --filter "FullyQualifiedName~DataModelTests"

# Endpoint Parsing Tests
dotnet test --filter "FullyQualifiedName~AspireEndpointParsingTests"
```

## Test Coverage Goals
- **Target:** >80% code coverage for Phase 1 backend services
- **Focus Areas:**
  - CLI command execution paths
  - JSON parsing and error handling
  - Event emission verification
  - Error recovery mechanisms

## Mock Strategy
- **No HTTP mocking** - Phase 1 uses CLI commands only
- **Process.Start mocking** - For CLI command tests (via reflection for private methods)
- **Realistic fixtures** - Sample JSON/text output matching actual Aspire CLI
- **Callback verification** - Log streaming callbacks tested with counters

## Integration with Existing Tests
This test suite complements:
- `PollingServiceTests.cs` - Timer-based polling with mock state machine
- `AspireApiClientTests.cs` - HTTP API client tests (Phase 2+)
- `EdgeCaseTests.cs` - Edge case validation
- `IntegrationTests.cs` - End-to-end integration scenarios

## Test Execution Notes
1. **Aspire CLI Required**: Some tests execute actual `aspire` commands
2. **Network Independence**: Tests do not require network connectivity
3. **Fast Execution**: All tests use short timeouts (100-300ms) for speed
4. **Isolated State**: Each test creates fresh service instances

## Key Testing Patterns

### 1. CLI Command Validation
```csharp
[Fact]
public async Task StartAspireAsync_WithInvalidFolder_ReturnsFalse()
{
    var service = new AspireCommandService();
    var result = await service.StartAspireAsync(@"C:\NonExistent");
    result.Should().BeFalse();
}
```

### 2. Endpoint URL Parsing
```csharp
[Theory]
[InlineData("http://localhost:18888")]
[InlineData("https://localhost:19999")]
public void ParseEndpoint_ValidFormats_ExtractsCorrectly(string url)
{
    var pattern = @"https?://(?:localhost|127\.0\.0\.1):\d+";
    var match = Regex.Match($"Dashboard: {url}", pattern);
    match.Success.Should().BeTrue();
}
```

### 3. Model Construction
```csharp
[Fact]
public void AspireResource_ParameterizedConstructor_SetsProperties()
{
    var resource = new AspireResource("api", "API Service", ResourceStatus.Running);
    resource.Id.Should().Be("api");
    resource.Status.Should().Be(ResourceStatus.Running);
}
```

## Future Enhancements
- [ ] Mock Process.Start for isolated CLI testing
- [ ] Add AspireLiveLogsService tests (log streaming)
- [ ] Integration tests with full CLI → Parse → Event cycle
- [ ] Performance benchmarks for polling frequency
- [ ] Stress tests for rapid start/stop cycles

## Test Maintenance
- **Update fixtures** when Aspire CLI output format changes
- **Add new enum values** when ResourceStatus expands
- **Verify regex patterns** if endpoint URL formats change
- **Review timeouts** if tests become flaky

## Contributors
- Finn (The Tester) - Phase 1 backend test suite author

---

**Last Updated:** 2026-04-26
**Test Framework:** xUnit 2.6.2
**Assertion Library:** FluentAssertions 6.12.0
**Mocking Framework:** Moq 4.20.70
