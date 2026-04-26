# Squad Decisions — ElBruno.AspireMonitor

**Last Updated:** 2026-04-26 (Session 1 Complete)
**Phase:** 1 Foundation (Complete) → 2 Core Development (Ready)

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
| UI/Frontend | ✅ IMPLEMENTED | WPF MVVM scaffold complete |
| API/Backend | ✅ RESEARCHED | Design ready, implementation pending |
| Testing | ✅ INFRASTRUCTURE | Test framework & stubs ready |
| Documentation | ✅ TEMPLATES | Framework ready, content pending |
| Design Assets | ✅ PROMPTS READY | Generation guide ready for t2i |
| Phase 1 | ✅ COMPLETE | Foundation ready |
| Phase 2 | 🟡 READY TO START | All preconditions met |

---

## Next Steps (Phase 2)

1. **Luke:** Begin AspireApiClient implementation
2. **Han:** Begin WPF-API integration (bind ViewModel to Luke's services)
3. **Yoda:** Implement unit tests as Luke completes services
4. **Lando:** Generate images using t2i tool with provided prompts
5. **Chewie:** Populate docs with architecture details from Luke/Han
6. **Leia:** Monitor progress, approve implementations

---

## Governance

- All meaningful changes require team consensus (documented here)
- Architectural decisions are locked; implementation details can adapt
- Test coverage must reach 80%+ before release
- All code reviewed by Leia before Phase 5
- All docs reviewed by Leia before Phase 5
