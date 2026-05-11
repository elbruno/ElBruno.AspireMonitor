# Release Plan: ElBruno.AspireMonitor v1.9.0

**Created:** 2026-05-11T17:50:12.055-04:00  
**Lead:** Leia (Release Manager)  
**Requested by:** Bruno Capuano  
**Status:** AWAITING FEATURE SPECIFICATION  

---

## 1. Version & Release Gates

| Item | Status | Details |
|------|--------|---------|
| **Current Version** | v1.8.0 | Latest published release (2026-05-11) |
| **Next Release** | v1.9.0 | Per constraint; semantic versioning |
| **Branch Strategy** | main | No separate release branch; tag-based |
| **Publish Trigger** | GitHub Release | `.github/workflows/publish.yml` on `release: published` |

---

## 2. Implementation Readiness Checklist

### Pre-Implementation ✋
- [ ] Feature specification provided and approved
- [ ] Architectural impact assessed
- [ ] UI/UX mockups reviewed (if applicable)
- [ ] SampleHarness test strategy defined

### Implementation Phase (Chewie/Code Agents)
- [ ] Feature code complete in development branch
- [ ] Unit tests written (>80% coverage gate enforced)
- [ ] SampleHarness integration tests passing
- [ ] Documentation updated (inline comments + docs/)
- [ ] CHANGELOG entry drafted

### Code Review Phase (Leia + Team)
- [ ] PR created against `main`
- [ ] All CI checks green:
  - ✅ dotnet build (Release config)
  - ✅ Unit tests (80% coverage gate via `.\build\Test-CoverageGate.ps1`)
  - ✅ SampleHarness tests
- [ ] Code review approval from 1+ team member
- [ ] Architecture alignment verified

### Release Phase (Leia)
- [ ] PR merged to `main`
- [ ] Version bumped in both `.csproj` files:
  - `src/ElBruno.AspireMonitor/ElBruno.AspireMonitor.csproj`
  - `src/ElBruno.AspireMonitor.Tool/ElBruno.AspireMonitor.Tool.csproj`
- [ ] CHANGELOG.md finalized
- [ ] Release notes prepared (docs/releases/RELEASE-v1.9.0.md)
- [ ] Git tag created: `v1.9.0`
- [ ] GitHub Release published
- [ ] NuGet publish workflow auto-triggered
- [ ] NuGet package verification: https://www.nuget.org/packages/ElBruno.AspireMonitor/

---

## 3. Release Gates & Quality Standards

### Build Gate
```powershell
dotnet build ElBruno.AspireMonitor.slnx -c Release --no-restore -p:Version=1.9.0
```
**Requirement:** 0 errors, 0 warnings in Release config

### Test Gate: Unit Tests
```powershell
dotnet test src/ElBruno.AspireMonitor.Tests/ElBruno.AspireMonitor.Tests.csproj -c Release --verbosity normal
```
**Requirement:** All tests pass

### Test Gate: Coverage (Services/Models Only)
```powershell
.\build\Test-CoverageGate.ps1 -Configuration Release -Threshold 80 -NoBuild
```
**Requirement:** ≥80% code coverage for src/ElBruno.AspireMonitor/Services/ and src/ElBruno.AspireMonitor/Models/

### Test Gate: SampleHarness Integration
```powershell
dotnet test src/SampleHarness/SampleHarness.Tests/SampleHarness.Tests.csproj -c Release --verbosity normal
```
**Requirement:** All end-to-end regression tests pass

### Pack & Deploy
```powershell
.\build\Pack-Tool.ps1 -Configuration Release -Version 1.9.0
```
**Deliverables:**
- `artifacts/packages/ElBruno.AspireMonitor.Tool.1.9.0.nupkg`
- `artifacts/packages/ElBruno.AspireMonitor.1.9.0.nupkg` (desktop)

---

## 4. PR & Merge Criteria

| Criterion | Detail |
|-----------|--------|
| **Branch Protection** | Requires code review + CI green |
| **Commit Message Format** | Conventional commits; include `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>` |
| **CHANGELOG Entry** | Added under `[Unreleased]` or `[1.9.0]` section |
| **Docs Updates** | Any user-facing changes documented in `docs/` or `README.md` |
| **SampleHarness Alignment** | Any breaking model/API changes validated against sample harness |

---

## 5. Publish Workflow Details

**Trigger:** Manual GitHub Release publish → `.github/workflows/publish.yml` fires

**Steps:**
1. Extract version from tag (e.g., `v1.9.0` → `1.9.0`)
2. Restore & Build (`-c Release`, version property set)
3. Run unit tests + coverage gate
4. Run SampleHarness tests
5. Pack tool + payload injection
6. Push to NuGet.org (OIDC authentication)
7. Upload artifact to GitHub Actions

**NuGet Destination:** https://www.nuget.org/packages/ElBruno.AspireMonitor/

---

## 6. Documentation Updates Required

After implementation agents complete:

- [ ] **CHANGELOG.md** — Add v1.9.0 section with:
  - `### Added` — New features
  - `### Fixed` — Bug fixes
  - `### Changed` — Breaking/behavioral changes
  
- [ ] **docs/releases/RELEASE-v1.9.0.md** — Follow v1.8.0 template:
  - Highlights section
  - Detailed feature descriptions
  - Links to GitHub release
  
- [ ] **README.md** — Update "What's New in v1.9.0" section if applicable
  
- [ ] **docs/whats-new.md** — Add to release timeline

---

## 7. Next Steps

### If Feature Specification Provided:
1. Leia routes feature to appropriate agent (Chewie for implementation, Yoda/Luke for architecture review)
2. Create orchestration log entry with agent spawn
3. Implementation agents work autonomously; Leia monitors PR readiness

### If This Is a General Release Readiness Plan:
- This document serves as the template for v1.9.0 release process
- Await Bruno's specific feature request to begin implementation phase

---

## Notes

- **Architecture decisions** are locked in `.squad/decisions.md` (Phases 1–4 complete)
- **Test coverage enforcement** happens automatically in publish workflow (80% threshold)
- **No production code changes** from Leia; coordinate with Chewie/code agents
- **Release process is automated** once tag is created; NuGet publish requires `release` environment approval

---

**Status:** ⏳ AWAITING FEATURE SPECIFICATION FOR v1.9.0

