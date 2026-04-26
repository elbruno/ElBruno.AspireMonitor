# Design Assets — ElBruno.AspireMonitor

This document describes the design system and visual assets for the ElBruno.AspireMonitor project, including icons, promotional graphics, and branding guidelines.

## Overview

ElBruno.AspireMonitor uses a cohesive design system built on the **Microsoft Aspire** brand identity. All assets feature the official Aspire purple palette with a modern, minimalist aesthetic optimized for monitoring and dashboard applications.

### Brand Foundation

- **Primary Color:** Deep Purple #512BD4 (Microsoft Aspire official)
- **Design Metaphor:** Three-peak mountain (growth, stability, monitoring visibility)
- **Visual Style:** Modern minimalism, professional, clean lines
- **Target:** Developers, DevOps professionals, distributed systems teams

---

## Design Assets

### 1. NuGet Package Icons

#### Primary Icon (256×256)
- **File:** `aspire-monitor-icon-256.png`
- **Size:** 3.63 KB
- **Format:** PNG with transparency
- **Usage:** NuGet package primary icon, package managers, app stores
- **Design:** Three geometric peaks on Aspire purple gradient with subtle status indicator

#### Fallback Icon (128×128)
- **File:** `aspire-monitor-icon-128.png`
- **Size:** 1.78 KB
- **Format:** PNG with transparency
- **Usage:** Small displays, mobile previews, sidebar thumbnails
- **Design:** Simplified version of primary icon, optimized for small sizes

**Quality Standards:**
- Crisp and readable at 128px minimum
- Scalable without pixelation
- Professional appearance suitable for commercial packages
- Transparent background for flexible placement

### 2. Blog & Hero Images

#### Blog Post Header
- **File:** `aspire-monitor-blog.png`
- **Dimensions:** 1200×630 px
- **Size:** 127.41 KB
- **Format:** PNG (optimized for web)
- **Usage:** Blog post headers, Medium articles, documentation landing pages
- **Design:** Aspire purple gradient with title and monitoring theme

#### High-Resolution Dashboard Hero
- **File:** `aspire-monitor-dashboard-blog-hero-image-monitoring-analytic-20260426-105611.png`
- **Dimensions:** 1920×1080 px
- **Size:** 925.17 KB
- **Format:** PNG (lossless compression)
- **Usage:** Conference slides, hero sections, product page headers
- **Design:** Detailed dashboard mockup showing real-time monitoring interface

#### Architecture Visualization
- **File:** `aspire-monitor-distributed-application-architecture-visualiz-20260426-105652.png`
- **Dimensions:** 1920×1080 px
- **Size:** 820.60 KB
- **Format:** PNG (lossless compression)
- **Usage:** Documentation, technical blog posts, architecture explanations
- **Design:** Visual representation of distributed app monitoring architecture

### 3. Social Media Graphics

#### LinkedIn Professional Banner
- **File:** `aspire-monitor-linkedin.png`
- **Dimensions:** 1200×627 px
- **Size:** 18.79 KB
- **Format:** PNG (optimized for web)
- **Usage:** LinkedIn company page, LinkedIn article headers, professional promotion
- **Design:** Aspire purple gradient with messaging and status indicators
- **Platform Requirements:** LinkedIn recommends 1200×627 (16:8.4 aspect ratio)

#### Twitter/X Card
- **File:** `aspire-monitor-twitter.png`
- **Dimensions:** 1024×512 px
- **Size:** 64.20 KB
- **Format:** PNG (optimized for web)
- **Usage:** Twitter/X preview cards, social media shares
- **Design:** Bold gradient background with eye-catching headline
- **Platform Requirements:** Twitter card size (2:1 aspect ratio)

---

## Color Palette

### Aspire Official Colors

Extracted from Microsoft's official Aspire logo SVG:

```
Deep Purple (Primary):    #512BD4  RGB(81, 43, 212)
Gradient Light 1:         #7455DD  RGB(116, 85, 221)
Gradient Light 2:         #9780E5  RGB(151, 128, 229)
Gradient Light 3:         #B9AAEE  RGB(185, 170, 238)
```

**Usage Rule:** Always use exact hex values. Never approximate or substitute.

### Status Indicator Colors

For monitoring/dashboard context:

```
Healthy (Green):    #10B981  RGB(16, 185, 129)   ✓ Running
Warning (Amber):    #F59E0B  RGB(245, 158, 11)   ⚠ Warning
Critical (Red):     #EF4444  RGB(239, 68, 68)    ✗ Error
```

These colors are:
- **Accessible:** Distinguishable for color-blind users when paired with icons/labels
- **Universal:** Standard traffic-light system globally recognized
- **Professional:** High-contrast, suitable for serious applications

### Text Colors

- **On Dark Purple:** White or #F0F4FF (light gray) for contrast
- **On Light Background:** #512BD4 (Aspire purple) or black
- **Minimum Contrast:** 4.5:1 ratio for accessibility

---

## Design Standards

### Icon Guidelines

1. **Size Variants:**
   - 256×256 px — Primary (NuGet, app stores)
   - 128×128 px — Secondary (mobile, sidebars)

2. **Format:**
   - PNG with transparent background
   - Lossless compression
   - No background fills

3. **Visual Elements:**
   - Three-peak mountain geometry (Aspire reference)
   - Gradient fills using official palette
   - Rounded container corners (16px radius)
   - Status indicators (optional, for monitoring context)

4. **Quality Checks:**
   - Readable at 128px (squint test)
   - Scalable to 512×512+ without pixelation
   - Professional appearance
   - Consistent with Aspire branding

### Promotional Graphics Standards

1. **Typography:**
   - Font: Professional sans-serif (Arial, Segoe, system sans-serif)
   - Heading weight: Bold, 36-64px
   - Body weight: Regular, 16-28px
   - Contrast: Minimum 4.5:1

2. **Spacing:**
   - Edge padding: 40-60px
   - Internal spacing: 20-30px for sections
   - Centered or balanced asymmetric layout

3. **Visual Hierarchy:**
   - Primary message: Large, bold, white or high-contrast
   - Secondary message: Medium, lighter shade
   - Visual elements: Dashboard mockups, icons, or screenshots

4. **File Optimization:**
   - PNG format for lossless compression
   - Target sizes: 1.78 KB (icon-128) to 925 KB (hero-1920)
   - Optimize with lossless tools (pngquant, ImageOptim)

---

## Asset Locations

All design assets are stored in the **repository root** for easy access:

```
C:\src\ElBruno.AspireMonitor\
├── aspire-monitor-icon-256.png                      (NuGet primary)
├── aspire-monitor-icon-128.png                      (NuGet fallback)
├── aspire-monitor-blog.png                          (Blog header)
├── aspire-monitor-linkedin.png                      (Social media)
├── aspire-monitor-twitter.png                       (Social media)
├── aspire-monitor-dashboard-blog-hero-*.png         (High-res hero)
├── aspire-monitor-distributed-application-*.png    (Architecture viz)
└── DESIGN.md                                        (This file)
```

**Naming Convention:**
- Format: `aspire-monitor-{type}-{size}.png`
- Examples: `aspire-monitor-icon-256.png`, `aspire-monitor-blog.png`
- Lowercase, hyphens for separation

---

## Usage Guidelines

### NuGet Package

1. **Configure in `.csproj`:**
   ```xml
   <PropertyGroup>
     <PackageIcon>aspire-monitor-icon-256.png</PackageIcon>
   </PropertyGroup>
   ```

2. **Icon Specifications:**
   - Size: 256×256 pixels minimum
   - Format: PNG with transparency
   - Display: Will appear in NuGet.org search, package details, Visual Studio

### Blog Headers

1. **Recommended Usage:**
   - Post cover image (1200×630)
   - Hero section background (1920×1080)
   - Open Graph/Twitter card preview (1200×630)

2. **Implementation:**
   - Use `aspire-monitor-blog.png` for standard blog headers
   - Use `aspire-monitor-dashboard-blog-hero-*.png` for hero sections
   - Always include alt text for accessibility

### Social Media

1. **LinkedIn:**
   - Dimensions: 1200×627 (16:8.4 aspect ratio)
   - File: `aspire-monitor-linkedin.png`
   - Use for: Company page updates, article headers, professional announcements

2. **Twitter/X:**
   - Dimensions: 1024×512 (2:1 aspect ratio)
   - File: `aspire-monitor-twitter.png`
   - Use for: Share preview cards, thread headers, promotional tweets

3. **Markdown/Docs:**
   - Embed using: `![Description](path/to/image.png)`
   - Always provide descriptive alt text
   - Consider dark mode (ensure contrast on dark backgrounds)

---

## Design Decisions

See `.squad/decisions/inbox/lando-aspire-brand-finalization.md` for detailed decision rationale.

### Key Decisions

1. **Color Palette:** Used official Aspire purple (#512BD4) for authenticity and brand alignment
2. **Mountain Icon:** Three-peak geometry references Aspire's upward-pointing symbolism
3. **Status System:** Traffic-light colors (green/yellow/red) provide monitoring context
4. **Modern Minimalism:** Clean, professional aesthetic appeals to developer audience
5. **Optimization:** Lossless PNG compression balances quality and file size

---

## Branding Guidelines

### Do's ✓

- ✓ Use exact Aspire color hex values (#512BD4, #7455DD, etc.)
- ✓ Maintain three-peak mountain visual motif
- ✓ Use professional, clean design language
- ✓ Include status color indicators (green/yellow/red) for monitoring context
- ✓ Ensure high contrast for text (minimum 4.5:1)
- ✓ Test icons at 128px and 256px sizes
- ✓ Optimize files with lossless PNG compression
- ✓ Use transparent PNG for flexibility

### Don'ts ✗

- ✗ Don't approximate colors — use exact hex values
- ✗ Don't use cartoon or playful styles
- ✗ Don't create low-contrast text
- ✗ Don't design overly complex icons that pixelate at small sizes
- ✗ Don't ignore status color system
- ✗ Don't add unnecessary design elements
- ✗ Don't use wrong aspect ratios for social media
- ✗ Don't skip file optimization (watch file sizes)

---

## Future Enhancements

1. **Demo GIF** — Animated screenshot showing app in action (tray interaction, status updates)
2. **Brand Style Guide** — Comprehensive visual identity document (fonts, spacing, patterns)
3. **Dark Mode Support** — Light versions of graphics for dark backgrounds
4. **Accessibility Variants** — High-contrast versions for accessibility
5. **Animation Specs** — Transition and motion guidelines
6. **Icon Library** — Reusable UI elements (status icons, charts, buttons)

---

## Questions?

For design-related questions or requests:
- Contact Lando (Design agent)
- See `.squad/skills/aspire-brand-design/SKILL.md` for design patterns
- Review `.squad/agents/lando/history.md` for design decisions and learnings

---

**Last Updated:** 2026-04-26  
**Design Lead:** Lando  
**Status:** Design system finalized and documented
