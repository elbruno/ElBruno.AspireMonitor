# Contributing to ElBruno.AspireMonitor

## Repository Structure

This project follows strict layout rules to keep the repository organized:

### Root Directory
Only these files belong at the repository root:
- `README.md` — Project overview and quick start
- `LICENSE` — MIT license
- `aspire.config.json` — Aspire AppHost configuration

### `docs/` — All Documentation
All documentation lives under `docs/` with appropriate subfolders:
- `docs/` — General guides (architecture, configuration, quickstart, etc.)
- `docs/design/` — Design system, visual assets documentation
- `docs/releases/` — Release notes per version
- `docs/promotional/` — Blog posts, social media content

### `images/` — All Images
All image assets (PNG, JPG, GIF, SVG) belong in `images/`.
- Exception: Images embedded inside NuGet packages are referenced from `images/` via the `.csproj`

### `src/` — All Code
All source code, tests, and projects belong in `src/`:
- `src/ElBruno.AspireMonitor/` — Main application
- `src/ElBruno.AspireMonitor.Tests/` — Tests

### `scripts/` — Utility Scripts
Standalone scripts (Python, PowerShell, shell) for tooling and automation.

### `nupkg/` — NuGet Package Artifacts
Extracted NuGet package contents for inspection and validation.

---

## Development Workflow

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Make changes — follow the structure rules above
4. Write or update tests in `src/ElBruno.AspireMonitor.Tests/`
5. Run tests: `dotnet test`
6. Commit with clear messages
7. Open a Pull Request

See [Development Guide](./development-guide.md) for detailed setup and debugging instructions.

---

## Code Style

- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise

## Testing

- All new features should include tests
- Bug fixes should include regression tests
- Aim for high code coverage
- Tests should be fast and deterministic

## Pull Request Guidelines

- Keep PRs focused on a single feature or fix
- Include clear descriptions of changes
- Reference related issues
- Ensure all tests pass
- Update documentation as needed

---

Thank you for contributing to ElBruno.AspireMonitor!
