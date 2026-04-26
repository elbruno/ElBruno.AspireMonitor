# AspireMonitor Documentation

Welcome to the AspireMonitor documentation hub. This directory contains guides for users, developers, and contributors.

## 📚 Documentation Guides

### Getting Started
- **[QUICKSTART.md](./QUICKSTART.md)** — Get up and running in 5 minutes (users)
- **[configuration.md](./configuration.md)** — Setup options, environment variables, and customization

### Architecture & Design
- **[architecture.md](./architecture.md)** — System design, components, and how they interact
- **[API-CONTRACT.md](./API-CONTRACT.md)** — Service layer reference, data contracts, retry logic (developers)

### Development & Operations
- **[development-guide.md](./development-guide.md)** — Local dev environment setup and contribution workflow
- **[publishing.md](./publishing.md)** — NuGet package publishing and release process
- **[troubleshooting.md](./troubleshooting.md)** — Common issues, debugging tips, and solutions

### Promotional Content
The **[promotional/](./promotional/)** folder contains templates for blog posts, LinkedIn, and Twitter/X announcements.

## By Role

### 👤 **Users** — "I want to monitor my Aspire app"
1. Start with **[QUICKSTART.md](./QUICKSTART.md)** — 5-minute setup
2. Reference **[configuration.md](./configuration.md)** — Customize thresholds/interval
3. Check **[troubleshooting.md](./troubleshooting.md)** — If something breaks

### 👨‍💻 **Developers** — "I want to integrate/extend"
1. Read **[architecture.md](./architecture.md)** — Understand the design
2. Study **[API-CONTRACT.md](./API-CONTRACT.md)** — Service methods, data contracts, examples
3. Follow **[development-guide.md](./development-guide.md)** — Build, test, contribute

### 🔧 **DevOps/Operators** — "I'm deploying/monitoring in production"
1. Follow **[QUICKSTART.md](./QUICKSTART.md)** — Installation
2. Tune **[configuration.md](./configuration.md)** — Polling interval, thresholds
3. Reference **[troubleshooting.md](./troubleshooting.md)** — High CPU, timeouts, etc.

### 🚀 **Release Managers** — "I'm publishing a new version"
1. Check **[publishing.md](./publishing.md)** — NuGet workflow, OIDC, versioning
2. Update **[CHANGELOG.md](../CHANGELOG.md)** — Document changes
3. Review **[architecture.md](./architecture.md)** — Any design changes?

## Quick Links

- **Main README**: [../README.md](../README.md)
- **Source Code**: [../src/](../src/)
- **GitHub Issues**: https://github.com/elbruno/ElBruno.AspireMonitor/issues
- **GitHub Discussions**: https://github.com/elbruno/ElBruno.AspireMonitor/discussions
- **NuGet Package**: https://www.nuget.org/packages/ElBruno.AspireMonitor

## Documentation Structure

```
docs/
├── README.md                    ← You are here
├── QUICKSTART.md               ← Start here (users)
├── API-CONTRACT.md             ← Start here (developers)
├── architecture.md             ← System design
├── configuration.md            ← Setup & tuning
├── development-guide.md        ← Contributing
├── publishing.md               ← Release process
├── troubleshooting.md          ← Problem solving
├── wpf-implementation-summary.md
└── promotional/                ← Blog, LinkedIn, Twitter templates
    ├── blog-post.md
    ├── linkedin-post.md
    └── twitter-post.md
```

## Maintenance

### When Adding Features
- **User-facing:** Update QUICKSTART.md + configuration.md
- **API/Service:** Update API-CONTRACT.md + architecture.md
- **Internal:** Update development-guide.md
- **All changes:** Update CHANGELOG.md

### Documentation Review Checklist
- [ ] Links are valid (no typos)
- [ ] Code examples work
- [ ] Screenshots are current
- [ ] Tables are aligned
- [ ] No outdated version numbers
- [ ] Cross-references point to correct sections

## Contributing to Docs

When adding documentation:
1. **Choose location:** Use the guides above; don't create new top-level files
2. **Follow style:** See existing guides for format (headings, code blocks, tables)
3. **Include examples:** Code, JSON, configuration snippets help
4. **Add links:** Cross-reference related docs
5. **Update this README:** Add to appropriate section

### Style Guidelines
- **Headings:** Use `## Heading` (level 2) as top-level in guides
- **Code blocks:** ` ```csharp ` for C#, ` ```json ` for JSON
- **Tables:** Use markdown tables for properties, enums, options
- **Emphasis:** **bold** for important, *italic* for emphasis, `code` for symbols
- **Line length:** Wrap at ~80 characters for readability
- **Links:** Use relative paths (`./other.md`, not full URLs)

---

**Last Updated:** 2026-04-27  
**Version:** 1.0.0  
**Maintainer:** Chewie (DevRel/Docs Agent)

