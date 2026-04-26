# ElBruno.AspireMonitor - Image Generation Guide

This document contains detailed prompts and specifications for all design assets to be generated via AI text-to-image tools.

---

## 1. NuGet Package Icon - Large (aspire-monitor-icon-256.png)

**Specifications:**
- **Dimensions:** 256x256 pixels
- **Format:** PNG with transparent background
- **Purpose:** Primary NuGet package icon
- **Quality:** High DPI, crisp edges, professional finish

**AI Generation Prompt:**
```
Modern Windows system tray app icon for Aspire monitor. Create a professional, clean design 
featuring a monitoring dashboard theme. Include:
- Status indicators with green, yellow, and red circles (representing healthy, warning, critical states)
- Dashboard metaphor showing real-time monitoring concept
- Microsoft Copilot blue color (#0078D4) as primary brand color
- Secondary accent in tech purple
- Windows 11 design language (clean, modern, minimal)
- Should scale well and remain clear at smaller sizes
- Transparent background
- No text/labels on icon
- Subtle gradient or depth for visual interest
- SVG-quality line clarity

Style: Professional tech, modern, enterprise software, scalable vector-like appearance
Mood: Trustworthy, professional, technical competence
```

**Design Notes:**
- Ensure readability at 32x32 and 16x16 sizes
- Use clear color contrast for accessibility
- Avoid overly complex details that blur when scaled down
- Consider the app's purpose: real-time monitoring of distributed applications

---

## 2. NuGet Package Icon - Small (aspire-monitor-icon-128.png)

**Specifications:**
- **Dimensions:** 128x128 pixels
- **Format:** PNG with transparent background
- **Purpose:** Fallback NuGet icon when large icon unavailable
- **Quality:** High clarity, readable at all Windows UI scales

**AI Generation Prompt:**
```
Simplified version of the aspire-monitor Windows system tray icon, optimized for 128x128 display.
Create a professional icon featuring:
- Monitoring dashboard concept
- Status color indicators (green, yellow, red)
- Microsoft Copilot blue primary color (#0078D4)
- MUST be crystal clear and readable at 128x128 size
- Simplified design compared to 256x256 version - remove fine details
- Transparent background
- Maintain same visual identity and brand colors
- Windows 11 style, modern and clean

Style: Professional, minimalist, tech-forward
Requirement: Extreme clarity and legibility at small size
```

**Design Notes:**
- Can be simplified version of the 256x256 icon
- Must maintain visual identity and brand colors
- Critical: Clarity at small display sizes
- No anti-aliasing artifacts

---

## 3. LinkedIn Promotional Graphic (aspire-monitor-linkedin.png)

**Specifications:**
- **Dimensions:** 1200x630 pixels (LinkedIn standard)
- **Format:** PNG
- **Purpose:** Professional social media sharing and promotion
- **Quality:** High-res, optimized for web

**AI Generation Prompt:**
```
Professional promotional graphic for ElBruno.AspireMonitor - a Windows system tray application
for real-time monitoring of Aspire distributed applications. Create:
- Realistic mockup of the app interface showing monitoring dashboard
- Real-time resource metrics display (CPU percentage, memory usage, status icons)
- Status color indicators: green (healthy), yellow (warning), red (critical)
- Multiple distributed app services visible in the monitoring view
- Aspire branding/reference (can show "Aspire" in the monitored apps)
- Color scheme: Microsoft Copilot blue (#0078D4), tech purple, white, dark background
- Modern tech aesthetic with clean UI elements
- Subtle glow effects and shadows for depth
- Text overlay: "Monitor Your Aspire Apps in Real-Time" (top or center)
- Bottom text: "ElBruno.AspireMonitor for Windows"
- Technology feel: professional dashboards, metrics, modern design
- 16:9 aspect ratio friendly layout

Style: Enterprise software, professional, modern tech
Use Case: LinkedIn posts about project launch or feature announcements
```

**Design Notes:**
- Should evoke confidence in the tool's capabilities
- Show realistic UI with monitoring data
- Make the value proposition clear (real-time monitoring)
- Professional enough for enterprise audience

---

## 4. Twitter/X Promotional Graphic (aspire-monitor-twitter.png)

**Specifications:**
- **Dimensions:** 1024x512 pixels (Twitter/X standard - 2:1 aspect ratio)
- **Format:** PNG
- **Purpose:** Eye-catching social media sharing
- **Quality:** High-res, optimized for web and mobile feeds

**AI Generation Prompt:**
```
Compact, eye-catching promotional graphic for ElBruno.AspireMonitor Windows application.
Create for Twitter/X audience:
- Show app dashboard with monitoring interface
- Status indicators and resource metrics prominently visible
- Color scheme: Blue and purple tech theme
- Modern, clean design with good contrast for dark mode/light mode compatibility
- Emphasize the monitoring/dashboard concept
- 2:1 aspect ratio (1024x512)
- Should stand out in a fast-scrolling social feed
- Can include minimal text or keep mostly visual
- Professional tech aesthetic
- Include subtle Aspire references or app name

Style: Modern, eye-catching, tech-forward, minimal text
Requirement: Must grab attention in a social media feed
Mood: Innovation, technical excellence, real-time monitoring capability
```

**Design Notes:**
- Must be readable on mobile devices
- Colors should work in both light and dark Twitter themes
- Avoid small text - focus on visual impact
- Center composition for best mobile display

---

## 5. Blog Header Graphic (aspire-monitor-blog.png)

**Specifications:**
- **Dimensions:** 1200x630 pixels (Standard blog header)
- **Format:** PNG
- **Purpose:** Blog post featured image
- **Quality:** High-res, professional

**AI Generation Prompt:**
```
Blog post header image for ElBruno.AspireMonitor technical blog. Create an educational,
professional design showing:
- Application interface with distributed app monitoring visualization
- Multiple services/containers being monitored
- Real-time metrics and status displays
- Aspire logo reference or branding element (can show "Aspire" in services)
- Monitoring network connections or service topology
- Color palette: Blue, purple, white on professional background
- Modern, clean design language
- Educational tone - shows capability and sophistication
- Text overlay: "Real-Time Aspire Monitoring for Windows"
- Subtitle possible: "Monitor distributed applications in real-time"
- Could show dashboard with metrics: response time, CPU, memory, status
- Modern tech aesthetic with professional polish

Style: Educational, professional, technical but accessible
Use Case: Blog post header for article about app monitoring, real-time dashboards, or Aspire ecosystem
Tone: Expertise, reliability, modern solutions
```

**Design Notes:**
- Should serve as professional blog header
- Balance visual interest with legibility of text overlay
- Show the sophistication of what the app can do
- Professional enough to build credibility with technical audience

---

## Generation Workflow

1. **Asset Creation Order:** Icons first (256/128), then social graphics
2. **Tool:** Use AI text-to-image generation (e.g., DALL-E, Midjourney, or similar)
3. **Export Format:** All as PNG files
4. **Quality Check:** 
   - Icons: Verify clarity at multiple zoom levels
   - Social graphics: Test in actual social media preview tools
5. **Storage:** Place all files in the `images/` folder

## Color Reference

- **Primary Brand Color:** Microsoft Copilot Blue #0078D4
- **Secondary Color:** Tech Purple (e.g., #7C3AED or #6B21A8)
- **Accent Colors:** Green #10B981 (healthy), Yellow #F59E0B (warning), Red #EF4444 (critical)
- **Background:** Dark tech backgrounds or transparent

## Branding Guidelines

- Maintain professional, enterprise software appearance
- Use Windows 11 design language for UI elements
- Ensure all assets feel part of the same product family
- Consistent use of status color indicators (green/yellow/red)
- Always reference Aspire partnership/ecosystem

