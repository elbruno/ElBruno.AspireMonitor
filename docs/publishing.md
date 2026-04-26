# Publishing Guide

This guide covers the process of publishing AspireMonitor to NuGet.org using GitHub Actions with OIDC (OpenID Connect) trusted publisher.

## Overview

AspireMonitor uses a **secure, zero-secret approach** to publishing:
- GitHub Actions workflows trigger on version tags (e.g., `v1.0.0`)
- OIDC trusted publisher authenticates without storing API keys
- Package is automatically built, tested, and published to NuGet

## Pre-Publishing Checklist

Before creating a release, ensure:

- [ ] All tests passing (`dotnet test`)
- [ ] Code formatted and reviewed
- [ ] README.md reflects current feature set
- [ ] Architecture docs updated if needed
- [ ] CHANGELOG.md updated with release notes
- [ ] Version number updated in `.csproj`
- [ ] Build succeeds in Release mode (`dotnet build -c Release`)

## Step 1: Update Version

### Edit `.csproj` File

Open `src/ElBruno.AspireMonitor/ElBruno.AspireMonitor.csproj` and update:

```xml
<PropertyGroup>
  <Version>1.0.0</Version>
  <TargetFramework>net10.0</TargetFramework>
  <OutputType>Exe</OutputType>
  <PublishDir>bin/Release/net10.0/win-x64/publish</PublishDir>
  
  <!-- NuGet Package Metadata -->
  <PackageId>ElBruno.AspireMonitor</PackageId>
  <Title>AspireMonitor</Title>
  <Description>Real-time Windows system tray monitor for .NET Aspire distributed applications</Description>
  <Authors>Bruno Capuano</Authors>
  <RepositoryUrl>https://github.com/elbruno/ElBruno.AspireMonitor</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
  <PackageTags>aspire;monitoring;dotnet;system-tray;windows</PackageTags>
  <PackageProjectUrl>https://github.com/elbruno/ElBruno.AspireMonitor</PackageProjectUrl>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <PackageRequireLicenseAcceptance>false</PackageRequireLicenseAcceptance>
  
  <!-- Release Notes Link -->
  <PackageReleaseNotes>https://github.com/elbruno/ElBruno.AspireMonitor/releases/tag/v$(Version)</PackageReleaseNotes>
  
  <!-- Icon (if available) -->
  <PackageIcon>icon.png</PackageIcon>
</PropertyGroup>
```

### Follow Semantic Versioning

- **MAJOR** (X.0.0): Breaking changes to API or CLI
- **MINOR** (X.Y.0): New features (backward compatible)
- **PATCH** (X.Y.Z): Bug fixes

Examples:
- First release: `1.0.0`
- New feature: `1.1.0`
- Bug fix: `1.0.1`
- Breaking change: `2.0.0`

## Step 2: Create Release

### Option A: Via Command Line

```bash
# Create and push version tag
git tag v1.0.0
git push origin v1.0.0

# Or create signed tag (recommended)
git tag -s v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

### Option B: Via GitHub UI

1. Go to **Releases** → **Draft a new release**
2. Click **Choose a tag** → **Create new tag**
3. Enter version: `v1.0.0`
4. **Release title:** `AspireMonitor v1.0.0`
5. **Description:** Copy from CHANGELOG.md
6. Click **Publish release**

## Step 3: Monitor GitHub Actions

1. Go to repository **Actions** tab
2. Locate the triggered workflow (by tag)
3. Wait for workflow to complete:
   - ✅ Build
   - ✅ Test
   - ✅ Pack NuGet package
   - ✅ Publish to NuGet

### Troubleshooting Build Failures

If the workflow fails:

1. **Check logs:** Actions → [workflow name] → [run] → logs
2. **Common issues:**
   - Tests failing: Fix code, push new commit, re-tag
   - Build errors: Check `.csproj` syntax
   - Missing dependencies: Run `dotnet restore`

## Step 4: Verify Publication

After workflow completes successfully:

1. **Wait 5-10 minutes** for NuGet indexing
2. **Visit NuGet package page:**
   ```
   https://www.nuget.org/packages/ElBruno.AspireMonitor/1.0.0
   ```
3. **Verify:**
   - Version number matches
   - Package metadata is correct
   - Download button works

### Test Installation

```bash
# Uninstall any local version
dotnet tool uninstall --global ElBruno.AspireMonitor

# Install from NuGet
dotnet tool install --global ElBruno.AspireMonitor

# Run the tool
aspire-monitor
```

## OIDC Setup (First Time Only)

If this is the first release and OIDC is not configured:

1. Go to NuGet.org account settings
2. Enable **OIDC trusted publishers**
3. Add trusted publisher:
   - **Service:** GitHub
   - **Organization:** `elbruno`
   - **Repository:** `ElBruno.AspireMonitor`
   - **Workflow file:** `.github/workflows/publish.yml`

The GitHub Actions workflow automatically uses OIDC. No API keys needed.

## GitHub Actions Workflow

The publish workflow (`.github/workflows/publish.yml`) automatically:

1. **Triggers on:** Version tag push (`v*`)
2. **Runs on:** Ubuntu latest
3. **Steps:**
   - Checkout code
   - Setup .NET 10
   - Restore dependencies
   - Run tests
   - Build release package
   - Publish to NuGet via OIDC

**No manual configuration needed** — just push a tag!

## Versioning Strategy

### Development Cycle

```
v1.0.0 (stable release)
  ↓ (bug fix)
v1.0.1 (patch)
  ↓ (new feature)
v1.1.0 (minor)
  ↓ (multiple new features)
v1.2.0 (minor)
  ↓ (breaking change)
v2.0.0 (major)
```

### Pre-Release Versions (Optional)

For beta releases, use pre-release tags:
```
v1.1.0-beta.1
v1.1.0-beta.2
v1.1.0-rc.1
```

NuGet will mark these as pre-release automatically.

## Release Notes

Create a `CHANGELOG.md` in the repository root:

```markdown
# Changelog

## [1.1.0] - 2026-05-01

### Added
- Real-time CPU/memory monitoring
- Color-coded status indicators
- System tray integration
- Configurable thresholds

### Fixed
- Connection retry logic on API timeout

### Changed
- Polling interval now configurable (default 2s)

## [1.0.0] - 2026-04-26

### Added
- Initial release
- Basic Aspire monitoring
```

Paste CHANGELOG entry into GitHub release description.

## Rollback / Un-listing

### Delete a NuGet Version

```bash
# Locally (requires authentication)
dotnet nuget delete ElBruno.AspireMonitor 1.0.0 --api-key [API_KEY] --non-interactive
```

Or use NuGet.org web interface:
1. Package page → **Manage Package**
2. Click **Delete** on the version
3. Confirm

### Re-Release After Rollback

Ensure version number is unique (can't re-use same version):
```xml
<!-- Was: 1.0.0 (deleted) -->
<!-- Now: 1.0.1 (new release) -->
<Version>1.0.1</Version>
```

## Publishing Best Practices

✅ **Do:**
- Use semantic versioning
- Write descriptive commit messages
- Test release locally before pushing tag
- Tag from `main` branch only
- Keep CHANGELOG up-to-date
- Review release notes before publishing

❌ **Don't:**
- Force-push after tagging (use new patch version instead)
- Publish without running tests
- Use inconsistent version numbering
- Include secrets in package

## Advanced: Custom Build/Pack Options

### Build for Specific Runtime

```bash
# Windows x64
dotnet publish -c Release -r win-x64

# Windows arm64
dotnet publish -c Release -r win-arm64
```

### Create Package Locally

```bash
dotnet pack -c Release -o ./nupkg
```

Output: `./nupkg/ElBruno.AspireMonitor.1.0.0.nupkg`

## Troubleshooting

### "OIDC authentication failed"

**Solution:** Verify trusted publisher configured correctly on NuGet.org

### "Version already exists on NuGet"

**Solution:** Increment version number, tag again:
```bash
# Was: v1.0.0 (published)
# Create: v1.0.1 (new version)
git tag v1.0.1
git push origin v1.0.1
```

### "Workflow failed during publish step"

**Solution:** Check NuGet OIDC status:
1. Go to NuGet.org → Account Settings
2. Verify OIDC trusted publisher is enabled
3. Check workflow logs for specific error

### "Package missing from NuGet after 10 minutes"

**Solution:** NuGet indexing can take up to 1 hour. Check:
1. Workflow completed successfully
2. Package shows in NuGet search (indexing delay)
3. Or verify via direct URL: `nuget.org/packages/ElBruno.AspireMonitor/1.0.0`

## Reference

- [NuGet Documentation](https://docs.microsoft.com/en-us/nuget/)
- [Semantic Versioning](https://semver.org/)
- [GitHub Actions](https://github.com/features/actions)
- [OIDC Trusted Publishers](https://docs.microsoft.com/en-us/nuget/nuget-org/security-best-practices)

---

*For help with publishing issues, see [troubleshooting.md](./troubleshooting.md) or open a GitHub issue.*
