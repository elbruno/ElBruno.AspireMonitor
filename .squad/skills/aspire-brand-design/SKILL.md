# SKILL: Aspire Brand Design System
**Maintainer:** Lando (Design)  
**Created:** 2026-04-26  
**Category:** Design / Branding

## Purpose

Standardized design approach for creating Aspire-branded graphics, icons, and promotional materials. Ensures consistent visual identity across NuGet packages, marketing materials, and documentation.

## When to Use

- Creating new icons or graphics for Aspire Monitor or related projects
- Designing promotional materials (social media, blog headers, conference slides)
- Building marketing collateral or product documentation
- Updating existing assets to maintain brand consistency

## Core Principles

### 1. Aspire Color Palette

**Official Aspire RGB Values (extracted from official logo SVG):**
```
Primary: #512BD4 (RGB: 81, 43, 212) — Deep Purple
Gradient stops:
  - #512BD4 (darkest)
  - #7455DD
  - #9780E5
  - #B9AAEE (lightest)
```

**Always use these exact hex values.** Never approximate or use similar colors.

### 2. Visual Language: Three-Peak Mountain Icon

Aspire's logo features an upward-pointing mountain with three geometric peaks, symbolizing:
- **Growth & Aspiration** — Moving upward, reaching goals
- **Stability & Reliability** — Solid geometric shapes
- **Monitoring & Visibility** — Peak/summit = seeing from above

**Application:** Use three-peak geometry in icon designs to subtly reference Aspire brand.

### 3. Status Indicator System

For monitoring/dashboard applications, use consistent traffic-light colors:
```
Healthy/Good:  #10B981 (RGB: 16, 185, 129)  — Vibrant Green
Warning:       #F59E0B (RGB: 245, 158, 11)  — Amber/Orange
Critical:      #EF4444 (RGB: 239, 68, 68)   — Red
```

These colors are universally understood and accessible (color-blind friendly when combined with icons/labels).

### 4. Typography Standards

- **Font Family:** Professional sans-serif (Arial, Segoe, or system sans-serif)
- **Headings:** Bold weight, 18-32px, white or dark text depending on background
- **Body text:** Regular weight, 12-16px
- **Contrast:** Minimum 4.5:1 ratio for accessibility

### 5. Design Aesthetic

- **Modern minimalism:** Clean lines, ample whitespace
- **Rounded corners:** 8-16px border radius for modern, approachable feel
- **Depth:** Use gradients subtly (dark to light purple, top-left to bottom-right)
- **Professional:** Avoid cartoonish or overly playful elements
- **Scalability:** Design works from 128px icons to 1920px hero images

## Common Patterns

### Pattern 1: Icon Design (256×256 NuGet Primary)

```
Structure:
1. Square canvas with rounded corners (16px radius)
2. Gradient background: #512BD4 → #9780E5 (top-left to bottom-right)
3. Central icon: Three peaks geometry, white or light purple
4. Status indicators (optional): Small circles in corners (green/yellow/red)
5. Border: Subtle light purple outline (1-2px)

File format: PNG with transparency
Size: 256×256 pixels
File size target: 2-4 KB
```

### Pattern 2: Icon Fallback (128×128 NuGet Secondary)

```
Structure:
1. Same as primary icon but simplified
2. Remove fine details (less than 4px)
3. Ensure readability at 128×128 (squint test)
4. Can use single or dual-color instead of gradient if needed

File format: PNG with transparency
Size: 128×128 pixels
File size target: 1-2 KB
```

### Pattern 3: Social Media Graphics (1200×630)

```
Structure:
1. Gradient background: Dark purple (#512BD4) to medium (#7455DD)
2. White content frame or transparent overlay with dark text
3. Headline: Bold, 36-48px, white or dark
4. Subheading: 20-28px, light gray or white
5. Visual element: Icon, chart, or screenshot showing app in action
6. Spacing: 40px padding from edges

File format: PNG or JPEG
Aspect ratio: 1200×630 (16:8.4 ratio)
File size target: 50-150 KB
Platforms: LinkedIn (1200×627), Twitter (1024×512)
```

### Pattern 4: Blog Hero Image (1920×1080)

```
Structure:
1. Gradient background: Dark purple to lighter gradient
2. Large typography: Main title 48-64px, subtitle 24-32px
3. Visual content: Dashboard mockup, app screenshot, or conceptual graphic
4. Spacing: 60px padding, centered layout
5. CTA element (optional): Button or action text

File format: PNG
Aspect ratio: 1920×1080 (16:9)
File size target: 800-1500 KB (optimize with lossless compression)
Use case: Blog headers, hero sections, conference slides
```

## Image Generation with t2i

### Effective Prompts

**For Icons:**
```
"Modern icon for Aspire monitoring application. Three geometric peaks pointing upward. 
Deep purple gradient background #512BD4 to #7455DD. Clean, professional, scalable design. 
White mountain icon on dark purple background. Rounded square container, modern aesthetic."
```

**For Hero Images:**
```
"Professional hero image for Aspire monitoring dashboard application. Dark purple gradient 
background (#512BD4 to #7455DD). Modern desktop UI mockup showing real-time monitoring. 
Large white title, subtitle, and dashboard visualization with green/yellow/red status indicators. 
Professional, tech-forward design. Suitable for blog header."
```

**Key principles:**
1. Specify exact hex colors (#512BD4, #7455DD, etc.)
2. Mention "Aspire" and "monitoring" to guide style
3. Describe layout and content clearly
4. Request PNG format for transparency
5. Specify dimensions (e.g., "1920×1080")
6. Use 50+ steps for high-quality results

## File Naming Convention

```
aspire-monitor-{type}-{size}.png

Examples:
- aspire-monitor-icon-256.png (primary icon)
- aspire-monitor-icon-128.png (fallback icon)
- aspire-monitor-blog.png (blog hero)
- aspire-monitor-linkedin.png (social media)
- aspire-monitor-twitter.png (social media)
- aspire-monitor-demo.gif (animated demo)

Naming rules:
- Use lowercase, hyphens for separation
- Include size if relevant (256, 128, 1920x1080)
- Save to repository root unless archived in /images subfolder
```

## Quality Checklist

Before delivering any design asset:

- [ ] **Colors:** Exact Aspire palette used (hex values verified)
- [ ] **Transparency:** PNG with transparent background (if needed)
- [ ] **Readability:** Minimum 4.5:1 contrast ratio for text
- [ ] **Scalability:** Icons look good at 128px minimum size
- [ ] **File size:** Optimized with lossless compression (PNG)
- [ ] **Modern aesthetic:** Clean lines, rounded corners, professional look
- [ ] **Aspire alignment:** Visual elements (peaks, purple) reference official brand
- [ ] **Status system:** Correct colors for monitoring indicators (green/yellow/red)
- [ ] **Consistency:** Matches other Aspire Monitor assets in style/tone

## Common Mistakes to Avoid

❌ **Using approximate colors** — Always use exact hex: #512BD4, not a "similar purple"  
❌ **Cartoon or playful style** — Keep professional and modern  
❌ **Low contrast text** — Minimum 4.5:1 ratio, test with contrast checker  
❌ **Pixelated icons** — Ensure icons scale smoothly (avoid thin lines < 2px)  
❌ **Ignoring status colors** — Always use green/yellow/red for monitoring context  
❌ **Overly complex design** — Minimalism is key, remove unnecessary details  
❌ **Wrong aspect ratios** — Double-check social media dimensions (1200×630, 1024×512)  
❌ **High file sizes** — Optimize with lossless PNG compression, target 1-150 KB for most assets  

## Tools & Resources

- **Image generation:** t2i CLI (text-to-image via Microsoft Foundry)
- **Color verification:** Use hex color picker to confirm #512BD4 and gradients
- **Contrast checking:** WebAIM contrast checker (minimum 4.5:1)
- **Icon testing:** View at 128px, 256px to ensure readability
- **File optimization:** PNG compression tools (pngquant, ImageOptim)

## Examples

See repository assets:
- `aspire-monitor-icon-256.png` — Primary icon, 256×256, uses three-peak mountain
- `aspire-monitor-icon-128.png` — Fallback icon, 128×128, simplified peaks
- `aspire-monitor-blog.png` — Blog hero, 1200×630, Aspire purple gradient
- `aspire-monitor-linkedin.png` — Social card, 1200×627, promotional layout
- `aspire-monitor-twitter.png` — Twitter card, 1024×512, eye-catching format

## Future Enhancements

1. **Brand Guidelines PDF** — Comprehensive visual identity document
2. **Design System Components** — Reusable UI elements (buttons, charts, status icons)
3. **Dark/Light Mode** — Adapt Aspire purple for different backgrounds
4. **Accessibility Guide** — Color blindness accommodations, high-contrast variants
5. **Animation Specs** — Transition and motion guidelines for GIFs/videos

---

**Questions?** Contact Lando (Design) or refer to `.squad/decisions/inbox/lando-aspire-brand-finalization.md`
