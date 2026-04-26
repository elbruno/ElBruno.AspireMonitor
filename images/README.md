# ElBruno.AspireMonitor - Design Assets

This directory contains all design assets for the ElBruno.AspireMonitor project, including icons for NuGet distribution and promotional graphics for social media.

## Asset Inventory

### 1. **NuGet Package Icons**

#### aspire-monitor-icon-256.png
- **Size:** 256x256 pixels
- **Purpose:** Primary icon for NuGet package display
- **Usage:** 
  - NuGet.org package page
  - Visual Studio package manager UI
  - Primary display resolution
- **Format:** PNG with transparent background
- **Status:** [PENDING GENERATION]

#### aspire-monitor-icon-128.png
- **Size:** 128x128 pixels
- **Purpose:** Fallback icon for display when 256x256 unavailable
- **Usage:**
  - Legacy NuGet clients
  - Small display contexts
  - Secondary fallback
- **Format:** PNG with transparent background
- **Status:** [PENDING GENERATION]

### 2. **Social Media Promotional Graphics**

#### aspire-monitor-linkedin.png
- **Size:** 1200x630 pixels (16:9 aspect ratio)
- **Purpose:** Professional social media sharing on LinkedIn
- **Usage:**
  - LinkedIn project announcement posts
  - Project showcase
  - Professional networking
- **Format:** PNG (web-optimized)
- **Status:** [PENDING GENERATION]
- **Platform Details:** LinkedIn recommends 1200x627 for best results

#### aspire-monitor-twitter.png
- **Size:** 1024x512 pixels (2:1 aspect ratio)
- **Purpose:** Eye-catching graphic for Twitter/X feeds
- **Usage:**
  - Twitter/X posts
  - Social media sharing
  - Quick-scroll engagement
- **Format:** PNG (web-optimized)
- **Status:** [PENDING GENERATION]
- **Platform Details:** Twitter/X optimized for 1024x512 (2:1 ratio)

### 3. **Blog & Content Graphics**

#### aspire-monitor-blog.png
- **Size:** 1200x630 pixels
- **Purpose:** Blog post featured image/header
- **Usage:**
  - Blog post headers
  - Technical documentation
  - Medium/DEV.to posts
- **Format:** PNG (web-optimized)
- **Status:** [PENDING GENERATION]
- **Platform Details:** Standard blog header dimension (1.91:1 ratio)

## Design Specifications

### Branding Colors
| Element | Color | Hex Value |
|---------|-------|-----------|
| Primary Brand | Microsoft Copilot Blue | #0078D4 |
| Secondary | Tech Purple | #7C3AED |
| Status - Healthy | Green | #10B981 |
| Status - Warning | Yellow | #F59E0B |
| Status - Critical | Red | #EF4444 |

### Design Philosophy
- **Professional:** Enterprise-grade software appearance
- **Modern:** Windows 11 design language
- **Clear:** Maximum legibility at all sizes
- **Technical:** Conveys monitoring and real-time capabilities
- **Consistent:** Visual family across all assets

### Icon Design Notes
- Status color indicators (green/yellow/red) for system state
- Monitoring dashboard metaphor
- System tray compatibility (Windows)
- Scale-friendly design (readable at 16x16 to 256x256)
- Transparent backgrounds for universal application

### Social Media Graphic Notes
- Show app interface/dashboard in action
- Include real-time metrics visualization
- Aspire ecosystem reference/branding
- Professional, polished appearance
- Text overlays communicating key value proposition

## Generation Process

For detailed generation prompts and AI text-to-image specifications, see **GENERATION_GUIDE.md**.

### Steps:
1. Review prompts in GENERATION_GUIDE.md
2. Use AI text-to-image generation tool (DALL-E, Midjourney, etc.)
3. Generate each asset following specifications
4. Save with specified filenames to this folder
5. Verify quality and on-brand appearance

## Usage Guidelines

### NuGet Icons
- Place both icon files in project's `.nuspec` or project file reference
- Icon URL should point to CDN or embedded resource
- Ensure transparency for proper display on NuGet.org

### Social Media Graphics
- Resize as needed for specific platform posting
- Maintain aspect ratios for consistency
- Test preview before posting
- Update links/references when sharing

### Blog Graphics
- Use as featured image in blog platform
- Resize to 1200x630 for optimal web performance
- Can crop for mobile if necessary

## Asset Status

| Asset | Size | Format | Status |
|-------|------|--------|--------|
| aspire-monitor-icon-256.png | 256x256 | PNG | ⏳ Generation Pending |
| aspire-monitor-icon-128.png | 128x128 | PNG | ⏳ Generation Pending |
| aspire-monitor-linkedin.png | 1200x630 | PNG | ⏳ Generation Pending |
| aspire-monitor-twitter.png | 1024x512 | PNG | ⏳ Generation Pending |
| aspire-monitor-blog.png | 1200x630 | PNG | ⏳ Generation Pending |

## Next Steps

1. ✅ Design specifications documented
2. ⏳ Generate images using AI text-to-image tool with GENERATION_GUIDE.md prompts
3. ⏳ Validate generated assets for brand compliance
4. ⏳ Integrate icons into NuGet package
5. ⏳ Use social graphics for marketing campaigns

## Contact

For design questions or modifications, contact the Design/Image Generation Engineer.
