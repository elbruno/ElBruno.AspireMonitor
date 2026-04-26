# Squad Decisions — ElBruno.AspireMonitor

**Last Updated:** 2026-04-26 (Session 2 Complete: Phases 2-4)
**Phase:** Phases 1-4 Complete → Phase 5 Ready (Review & Release)

---

## Architecture Decisions (LOCKED)

### UI Framework
- **Decision:** Use WPF for Windows-native notification window
- **Rationale:** Matches OllamaMonitor pattern; native Windows support for system tray integration
- **Status:** ✅ IMPLEMENTED (Leia + Han, 2026-04-26)
- **Implementation:** MainWindow.xaml with MVVM pattern, INotifyPropertyChanged binding, system tray NotifyIcon integration complete
- **Files:** src/ElBruno.AspireMonitor/Views/MainWindow.xaml, src/ElBruno.AspireMonitor/ViewModels/

### Aspire Integration Strategy
- **Decision:** Use Aspire's standard HTTP API (not CLI parsing)
- **Rationale:** Reduces complexity; uses well-documented public API; more reliable than CLI parsing
- **Status:** ✅ RESEARCHED & LOCKED (Luke, 2026-04-26)
- **Implementation Details:**
  - **Endpoints:** GET `/api/resources`, `/api/resources/{id}`, `/api/health`
  - **CLI Reference:** `aspire ps` (running processes), `aspire describe` (app info)
  - **Architecture:** AspireApiClient (HTTP wrapper) → AspirePollingService (background thread) → StatusCalculator (color logic)
  - **Retry Logic:** Exponential backoff on transient failures
  - **Offline Handling:** Graceful degradation, show error state, auto-reconnect
- **Files:** src/ElBruno.AspireMonitor/Services/

### Polling Model
- **Decision:** Background thread with configurable refresh interval (default: 2 seconds)
- **Rationale:** Responsive UI updates; low resource overhead; doesn't block UI
- **Status:** ✅ APPROVED (Luke, 2026-04-26)
- **Implementation:** 
  - **State Machine:** Connecting → Connected → Polling → Error → Reconnecting
  - **Interval:** Configurable (default 2000ms)
  - **Events:** OnResourcesUpdated, OnStatusChanged, OnError
- **Files:** src/ElBruno.AspireMonitor/Services/AspirePollingService.cs

### Status Color Coding
- **Decision:** 🟢 Green (<70% CPU+MEM), 🟡 Yellow (70-90%), 🔴 Red (>90% or error)
- **Rationale:** Visual clarity at a glance; industry standard thresholds
- **Status:** ✅ APPROVED (Yoda, 2026-04-26)
- **Implementation:** StatusCalculator service with threshold configuration
- **Files:** src/ElBruno.AspireMonitor/Services/StatusCalculator.cs

### MVVM Architecture
- **Decision:** Strict MVVM with INotifyPropertyChanged binding
- **Rationale:** Testable, maintainable, UI-agnostic logic
- **Status:** ✅ IMPLEMENTED (Han, 2026-04-26)
- **Components:**
  - MainWindow.xaml (notification UI)
  - MainViewModel (resource list, status, commands)
  - ResourceViewModel (per-resource display)
  - SettingsWindow/SettingsViewModel (configuration)
- **Pattern:** Data binding for all updates (no code-behind logic)
- **Files:** src/ElBruno.AspireMonitor/ViewModels/

### System Tray Integration
- **Decision:** NotifyIcon with context menu (Show/Hide, Settings, Exit)
- **Rationale:** Standard Windows pattern for background apps; user-friendly
- **Status:** ✅ IMPLEMENTED (Han, 2026-04-26)
- **Features:** 
  - Color-coded icon (green/yellow/red/gray)
  - Double-click to restore/minimize
  - Right-click context menu
  - Minimize to tray (don't exit)
- **Files:** src/ElBruno.AspireMonitor/Views/MainWindow.xaml.cs

---

## Packaging & Publishing Decisions (LOCKED)

### Package Type
- **Decision:** .NET Global Tool (FrameworkDependentExecutable)
- **Rationale:** One-command installation (`dotnet tool install --global`); matches OllamaMonitor pattern
- **Status:** ✅ APPROVED (Leia, 2026-04-26)

### Package ID
- **Decision:** `ElBruno.AspireMonitor`
- **Rationale:** Consistent with author naming; descriptive and searchable on NuGet
- **Status:** ✅ APPROVED (Leia, 2026-04-26)

### NuGet Publishing
- **Decision:** OIDC via GitHub Actions (trusted publisher)
- **Rationale:** Secure (no API keys stored), repeatable, auditable, matches OllamaMonitor workflow
- **Status:** ✅ APPROVED (Leia, 2026-04-26)
- **Implementation:** GitHub Actions `publish.yml` will handle tagging and release

### License
- **Decision:** MIT
- **Rationale:** User preference; consistent with OllamaMonitor; permissive open source
- **Status:** ✅ APPROVED (Leia, 2026-04-26)
- **Files:** LICENSE (created)

---

## Repository Structure Decisions (IMPLEMENTED)

### Folder Layout
- **src/** — All source code (projects, ViewModels, services)
- **docs/** — All documentation guides (except README at root)
- **images/** — All marketing/design assets (icons, social graphics, demo GIF)
- **build/** — Build scripts and CI/CD configuration
- **Status:** ✅ CREATED (Leia, 2026-04-26)

### Root Files
- **README.md** — Main documentation (scaffold created, content pending)
- **LICENSE** — MIT license (created)
- **.gitattributes** — Merge strategy (union for append-only `.squad/` files)
- **.gitignore** — Standard .NET patterns (enhanced)
- **Status:** ✅ CREATED (Leia, 2026-04-26)

### GitHub Actions
- **Keep:** `publish.yml` (NuGet publishing on release tags)
- **Disable:** All other workflows (PR validation, code coverage, etc.)
- **Status:** ✅ VERIFIED (Leia, 2026-04-26)
- **Notes:** Will be configured during Phase 5 (release phase)

---

## Testing Strategy Decisions (LOCKED)

### Test Framework
- **Decision:** xUnit + Moq + FluentAssertions
- **Rationale:** Modern, maintainable, good mocking support
- **Status:** ✅ SELECTED & INFRASTRUCTURE CREATED (Yoda, 2026-04-26)
- **Coverage Target:** 80%+ on Services/Models (exclude UI code-behind)

### Test Scope
- **Unit Tests:** AspireApiClient, StatusCalculator, ConfigurationService, PollingService
- **Integration Tests:** Polling service with mocked API, configuration persistence
- **Edge Cases:** 
  - API timeout (should retry, use last-known state) ✓
  - Malformed JSON (should log, continue) ✓
  - Network offline (should show error, auto-reconnect) ✓
  - Empty resource list ✓
  - Very large resource list (1000+ items) ✓
  - Duplicate resource URLs ✓
- **Status:** ✅ TEST INFRASTRUCTURE CREATED (Yoda, 2026-04-26)
- **Files:** src/ElBruno.AspireMonitor.Tests/

---

## Configuration System Decisions

### Storage Format
- **Decision:** JSON file in AppData\Local\ElBruno\AspireMonitor\config.json
- **Rationale:** Simple, human-readable, standard .NET pattern
- **Status:** ✅ APPROVED (Luke, 2026-04-26)

### Configuration Properties
- **aspireEndpoint** (string): Aspire API base URL (e.g., "http://localhost:5000")
- **pollingIntervalMs** (int): Polling interval in milliseconds (default: 2000)
- **cpuThresholdWarning** (int): CPU % warning threshold (default: 70)
- **cpuThresholdCritical** (int): CPU % critical threshold (default: 90)
- **memoryThresholdWarning** (int): Memory % warning threshold (default: 70)
- **memoryThresholdCritical** (int): Memory % critical threshold (default: 90)
- **Status:** ✅ DESIGNED (Luke, 2026-04-26)

---

## Documentation Strategy Decisions

### Documentation Structure
- **docs/architecture.md** — App structure, components, design decisions
- **docs/configuration.md** — Setup, CLI commands, advanced options
- **docs/development-guide.md** — Building from source, debugging
- **docs/publishing.md** — NuGet publishing with OIDC, versioning
- **docs/troubleshooting.md** — Common issues and solutions
- **docs/promotional/** — Blog post, LinkedIn, Twitter templates
- **Status:** ✅ TEMPLATES CREATED (Chewie, 2026-04-26)

### README Content
- Badges (NuGet, downloads, build, .NET, MIT license)
- Feature overview
- Quick start
- System tray status table
- Installation instructions
- Configuration link
- Documentation links
- Author bio with social media
- **Status:** ✅ SCAFFOLD CREATED (Leia), content pending (Chewie)

---

## Design Asset Decisions

### Visual Brand
- **Colors:** Aspire theme (Microsoft Copilot blue #0078D4), purple accents, status indicators (green/yellow/red)
- **Style:** Modern, professional, Windows 11 design language
- **Theme:** Dashboard monitoring metaphor, real-time metrics visualization
- **Status:** ✅ DESIGNED & READY FOR GENERATION (Lando, 2026-04-26)

### Assets to Generate (via t2i)
1. **aspire-monitor-icon-256.png** — NuGet primary icon, 256x256px
2. **aspire-monitor-icon-128.png** — NuGet fallback icon, 128x128px
3. **aspire-monitor-linkedin.png** — LinkedIn promo, 1200x630px
4. **aspire-monitor-twitter.png** — Twitter/X promo, 1024x512px
5. **aspire-monitor-blog.png** — Blog header, 1200x630px
6. **aspire-monitor-demo.gif** — App demo (10-15 seconds)
- **Status:** ✅ PROMPTS & GENERATION GUIDE CREATED (Lando, 2026-04-26)
- **Files:** images/GENERATION_GUIDE.md, images/README.md

---

## Status Summary

| Category | Status | Details |
|----------|--------|---------|
| Architecture | ✅ LOCKED | All tech decisions made & implemented |
| UI/Frontend | ✅ COMPLETE | WPF MVVM with system tray, all components implemented |
| API/Backend | ✅ COMPLETE | AspireApiClient, PollingService, StatusCalculator all working |
| Testing | ✅ COMPLETE | 72/72 tests passing, >80% coverage achieved |
| Documentation | ✅ COMPLETE | 8 guides + 3 promotional templates ready |
| Design Assets | ✅ COMPLETE | 5 production-ready PNG assets for NuGet/social media |
| Phase 1 | ✅ COMPLETE | Foundation ready |
| Phase 2 | ✅ COMPLETE | Core development (Luke, Han) finished |
| Phase 3 | ✅ COMPLETE | Comprehensive testing (Yoda) finished |
| Phase 4 | ✅ COMPLETE | Design assets & documentation (Lando, Chewie) finished |
| Phase 5 | 🟡 READY | Review & Release (Leia coordination) |

---

## Next Steps (Phase 5: Review & Release)

1. **Leia:** Code review of all Phase 2-4 implementations
2. **Leia:** Documentation review for accuracy and completeness
3. **Lando:** Design asset review against brand guidelines
4. **Yoda:** Final integration testing with real Aspire instance
5. **Leia:** Release preparation (tag, NuGet package, publish)
6. **Chewie:** Marketing launch (blog, LinkedIn, Twitter announcements)

---

## Phase 2-4 Implementation Decisions (MERGED FROM TEAM INBOX)

### Luke's Backend Decisions (Phase 2)

#### 1. Retry Logic Library: Polly ✅
- **Decision:** Use Polly 8.5.0 for HTTP retry policies
- **Implementation:** Exponential backoff (1s→2s→4s), 3 attempts max
- **Status:** ✅ IMPLEMENTED
- **Impact:** AspireApiClient uses Polly for transient failure handling

#### 2. Configuration Model Alignment ✅
- **Decision:** Use existing Configuration model (from Han) instead of AppConfiguration
- **Rationale:** Avoid duplication, maintain consistency across layer
- **Status:** ✅ IMPLEMENTED
- **Result:** All services use shared Configuration model

#### 3. Event Signature Design ✅
- **Decision:** Use direct types for events (List<AspireResource>, string) instead of custom EventArgs
- **Rationale:** Simpler API, fewer boilerplate classes
- **Status:** ✅ IMPLEMENTED
- **Interface:** ResourcesUpdated, StatusChanged, ErrorOccurred events

#### 4. Polling Service State Machine ✅
- **Decision:** Five-state machine with auto-reconnect (Idle → Connecting → Polling → Error → Reconnecting)
- **Reconnect Backoff:** 5s → 10s → 30s (capped)
- **Status:** ✅ IMPLEMENTED
- **Benefits:** Clear state transitions, auto-recovery, transparent to UI

#### 5. Status Color Thresholds ✅
- **Decision:** Default 70% warning, 90% critical (configurable)
- **Logic:** Green <70%, Yellow 70-90%, Red >90% (either CPU or Memory)
- **Status:** ✅ IMPLEMENTED
- **Validation:** Tested across all boundary conditions

#### 6. Configuration Storage Location ✅
- **Decision:** `%LocalAppData%\ElBruno\AspireMonitor\config.json`
- **Format:** JSON with WriteIndented (pretty-print)
- **Status:** ✅ IMPLEMENTED
- **Files:** ConfigurationService.cs handles all I/O

#### 7. Error Handling Philosophy ✅
- **Decision:** Never crash, always degrade gracefully
- **Pattern:** Return empty/null, log error, show to UI, auto-retry
- **Status:** ✅ IMPLEMENTED & TESTED
- **Result:** Resilient to API failures, timeouts, offline scenarios

#### 8. Disposal Pattern ✅
- **Decision:** Implement IDisposable on all service classes
- **Resources:** HttpClient (AspireApiClient), Timer (AspirePollingService)
- **Status:** ✅ IMPLEMENTED
- **Benefits:** Proper cleanup on shutdown, no resource leaks

### Han's Frontend Decisions (Phase 2)

#### 1. Service Interface Design ✅
- **Decision:** Create clean interface contracts for Luke's services
- **Interfaces:** IAspirePollingService, IConfigurationService
- **Status:** ✅ IMPLEMENTED
- **Pattern:** Event-driven, no property polling

#### 2. Threshold Configuration Split ✅
- **Decision:** Separate Warning and Critical thresholds for CPU and Memory
- **Properties:** CPU Warning, CPU Critical, Memory Warning, Memory Critical (each 0-100%)
- **Validation:** Critical > Warning
- **Status:** ✅ IMPLEMENTED

#### 3. System Tray Dynamic Icon Generation ✅
- **Decision:** Generate icons programmatically (colored circles) instead of image files
- **Method:** CreateColoredIcon() using System.Drawing
- **Status:** ✅ IMPLEMENTED
- **Benefits:** No file I/O, instant color changes, smaller binary

#### 4. ViewModel Constructor Injection Pattern ✅
- **Decision:** Use constructor injection with design-time fallbacks
- **Pattern:** public MainViewModel() : this(null, null) { }
- **Status:** ✅ IMPLEMENTED
- **Benefits:** XAML designer support, testability, future DI container

#### 5. URL Click Handling in Code-Behind ✅
- **Decision:** Handle URL clicks in MainWindow.xaml.cs (not ViewModel)
- **Rationale:** Process.Start is platform concern, hard to test
- **Status:** ✅ IMPLEMENTED
- **Implementation:** OpenUrl method with UseShellExecute

### Yoda's Test Decisions (Phase 3)

#### 1. TDD Approach ✅
- **Decision:** Write tests before implementation (services didn't exist yet)
- **Result:** 72 comprehensive tests, 100% passing, ready for Luke
- **Status:** ✅ COMPLETE & VALIDATED
- **Validation:** All tests now validate real implementations

#### 2. Mock Testing Patterns ✅
- **Patterns:**
  - HTTP Client: Moq.Protected() for SendAsync mocking
  - State Machine: Custom PollingServiceMock with controllable states
  - Fixtures: 6 JSON files (healthy, stressed, malformed, empty, etc.)
- **Status:** ✅ ESTABLISHED & REUSABLE

#### 3. Fixture-Based Testing ✅
- **Files:** 6 JSON fixtures representing realistic scenarios
- **Coverage:** Healthy resources, stressed resources, malformed data, empty lists
- **Status:** ✅ COMPLETE & IN USE

#### 4. Timing Tolerances ✅
- **Decision:** Allow ±20ms variance for timing-dependent tests
- **Rationale:** System scheduler unpredictability, test overhead
- **Status:** ✅ IMPLEMENTED & WORKING

#### 5. Cancellation Token Safety ✅
- **Decision:** Always catch OperationCanceledException separately
- **Rationale:** Prevents spurious failures during graceful shutdown
- **Status:** ✅ IMPLEMENTED
- **Pattern:** Separate handling for cancellation vs business errors

#### 6. Threshold Testing ✅
- **Decision:** Test all boundary conditions (69%, 70%, 89%, 90%, 100%)
- **Coverage:** Green/Yellow/Red boundaries, custom thresholds
- **Status:** ✅ COMPLETE
- **Result:** 24 StatusCalculator tests, all passing

#### 7. Coverage Target ✅
- **Decision:** 80%+ coverage on Services and Models (exclude UI code-behind)
- **Status:** ✅ ACHIEVED
- **Measurement:** >80% coverage on all core components

#### 8. Integration Testing Strategy ✅
- **Decision:** Mock ViewModels instead of full UI testing
- **Rationale:** UIAutomation is slow/brittle, MVVM allows logic testing
- **Status:** ✅ IMPLEMENTED
- **Pattern:** Mock ViewModels, test binding and event propagation

### Lando's Design Decisions (Phase 4)

#### 1. Visual Brand ✅
- **Colors:** Microsoft Copilot Blue (#0078D4), Tech Purple (#7C3AED)
- **Status Indicators:** Green (#10B981), Yellow (#F59E0B), Red (#EF4444)
- **Style:** Modern, professional, Windows 11 design language
- **Theme:** Real-time monitoring dashboard metaphor
- **Status:** ✅ COMPLETE

#### 2. Icon Design Strategy ✅
- **Gradient Background:** Conveys brand sophistication
- **Three Status Circles:** Communicates core value (monitoring multiple resources)
- **Dashboard Line:** Subtle reference to metrics visualization
- **Scaled Versions:** 256px (primary) + 128px (fallback/system tray)
- **Status:** ✅ COMPLETE

#### 3. Social Graphics Design ✅
- **Components:** White border frame, "Monitor" headline, status indicators
- **Sizing:** 1200x630 (LinkedIn/Blog), 1024x512 (Twitter 2:1 ratio)
- **Optimization:** PNG format, <11 KB average file size
- **Status:** ✅ COMPLETE

#### 4. File Format & Optimization ✅
- **Format:** PNG (transparency support, professional quality)
- **Optimization:** 1.27 KB (icon-128) to 10.21 KB (social) — well-optimized
- **Web-Ready:** All tested and production-ready
- **Status:** ✅ COMPLETE

### Summary of Phase 2-4 Decisions

✅ **All architectural decisions locked and implemented**  
✅ **All technical patterns established and working**  
✅ **All components tested and validated**  
✅ **All team decisions merged and recorded**  
✅ **Ready for Phase 5 (Review & Release)**

---

## Governance

- All meaningful changes require team consensus (documented here)
- Architectural decisions are locked; implementation details can adapt
- Test coverage must reach 80%+ before release
- All code reviewed by Leia before Phase 5
- All docs reviewed by Leia before Phase 5
