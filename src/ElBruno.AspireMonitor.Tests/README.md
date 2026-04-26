# Test Suite Documentation

## Overview
Test suite for ElBruno.AspireMonitor - Windows system tray application monitoring .NET Aspire dashboard resources.

## Test Strategy

### Target Coverage
- **Goal:** 80%+ code coverage
- **Focus:** Services and Models layers
- **Excluded:** UI code-behind (minimal business logic)

### Test Categories

#### 1. Unit Tests (Services/)
- **AspireApiClientTests**: API communication, error handling, retries
- **StatusCalculatorTests**: Threshold calculations, status determination
- **ConfigurationServiceTests**: Config persistence, validation
- **PollingServiceTests**: Polling lifecycle, intervals, state management

#### 2. Integration Tests
- End-to-end scenarios with mocked dependencies
- Configuration persistence across restarts
- Event propagation to UI layer

#### 3. Edge Cases
- Empty/large datasets (0 resources, 1000+ resources)
- Malformed/missing data
- Network failures and recovery
- Null handling

## Test Fixtures
Mock data located in `Fixtures/` directory:
- `aspire-response-healthy.json` - Normal operation (3 healthy resources)
- `aspire-response-stressed.json` - High CPU/memory (2 stressed resources)
- `aspire-response-empty.json` - Empty resource list
- `aspire-response-malformed.json` - Missing/null fields
- `config-valid.json` - Valid configuration
- `config-invalid.json` - Invalid values for validation testing

## Test Framework
- **xUnit** - Test runner
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library

## Running Tests
```bash
dotnet test src/ElBruno.AspireMonitor.Tests/ElBruno.AspireMonitor.Tests.csproj

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Next Steps
1. Han and Luke will implement service classes
2. Update test stubs with actual assertions
3. Add performance benchmarks for large datasets
4. Implement UI integration tests (if needed)

---
**Prepared by:** Yoda, the Tester
**Target:** 80%+ coverage on Services/Models
**Status:** Infrastructure ready, awaiting implementation
