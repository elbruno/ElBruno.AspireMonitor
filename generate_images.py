#!/usr/bin/env python3
from PIL import Image, ImageDraw, ImageFont
import os

# Official Aspire brand colors extracted from logo
ASPIRE_DARK_PURPLE = (81, 43, 212)      # #512BD4
ASPIRE_PURPLE_1 = (116, 85, 221)        # #7455DD
ASPIRE_PURPLE_2 = (151, 128, 229)       # #9780E5
ASPIRE_PURPLE_3 = (185, 170, 238)       # #B9AAEE
ASPIRE_LIGHT_PURPLE = (220, 213, 246)   # #DCD5F6
WHITE = (255, 255, 255)
DARK_GRAY = (30, 30, 30)
GREEN = (16, 185, 129)                  # Status: healthy
YELLOW = (245, 158, 11)                 # Status: warning
RED = (239, 68, 68)                     # Status: critical

os.chdir('C:\\src\\ElBruno.AspireMonitor\\images')

# 1. Create 256x256 NuGet Icon
print("Creating aspire-monitor-icon-256.png...")
img_256 = Image.new('RGBA', (256, 256), (0, 0, 0, 0))
draw_256 = ImageDraw.Draw(img_256)

# Background with gradient effect
for i in range(256):
    ratio = i / 256
    r = int(ASPIRE_DARK_PURPLE[0] + (ASPIRE_PURPLE_2[0] - ASPIRE_DARK_PURPLE[0]) * ratio)
    g = int(ASPIRE_DARK_PURPLE[1] + (ASPIRE_PURPLE_2[1] - ASPIRE_DARK_PURPLE[1]) * ratio)
    b = int(ASPIRE_DARK_PURPLE[2] + (ASPIRE_PURPLE_2[2] - ASPIRE_DARK_PURPLE[2]) * ratio)
    draw_256.line([(0, i), (256, i)], fill=(r, g, b, 255))

# Draw rounded rectangle border (Aspire style container)
margin = 20
draw_256.rounded_rectangle(
    [(margin, margin), (256-margin, 256-margin)],
    radius=30,
    outline=ASPIRE_LIGHT_PURPLE,
    width=2
)

# Draw mountain/chevron icon (geometric, pointing upward)
cx, cy = 128, 110

# Mountain peaks
left_peak = [(cx-40, cy+40), (cx-20, cy-30), (cx, cy+40)]
draw_256.polygon(left_peak, fill=ASPIRE_PURPLE_1)

center_peak = [(cx, cy+40), (cx+30, cy-50), (cx+60, cy+40)]
draw_256.polygon(center_peak, fill=ASPIRE_PURPLE_2)

right_peak = [(cx+60, cy+40), (cx+80, cy-20), (cx+100, cy+40)]
draw_256.polygon(right_peak, fill=ASPIRE_PURPLE_3)

# Status indicator circles
status_y = 200
radius = 12
draw_256.ellipse([(cx-50, status_y-radius), (cx-50+radius*2, status_y+radius)], fill=GREEN)
draw_256.ellipse([(cx-10, status_y-radius), (cx-10+radius*2, status_y+radius)], fill=YELLOW)
draw_256.ellipse([(cx+30, status_y-radius), (cx+30+radius*2, status_y+radius)], fill=RED)

img_256.save('aspire-monitor-icon-256.png', 'PNG', optimize=True)
print("✓ Created: aspire-monitor-icon-256.png (256x256)")

# 2. Create 128x128 NuGet Icon
print("Creating aspire-monitor-icon-128.png...")
img_128 = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
draw_128 = ImageDraw.Draw(img_128)

# Background gradient
for i in range(128):
    ratio = i / 128
    r = int(ASPIRE_DARK_PURPLE[0] + (ASPIRE_PURPLE_2[0] - ASPIRE_DARK_PURPLE[0]) * ratio)
    g = int(ASPIRE_DARK_PURPLE[1] + (ASPIRE_PURPLE_2[1] - ASPIRE_DARK_PURPLE[1]) * ratio)
    b = int(ASPIRE_DARK_PURPLE[2] + (ASPIRE_PURPLE_2[2] - ASPIRE_DARK_PURPLE[2]) * ratio)
    draw_128.line([(0, i), (128, i)], fill=(r, g, b, 255))

# Rounded rectangle
draw_128.rounded_rectangle(
    [(10, 10), (118, 118)],
    radius=15,
    outline=ASPIRE_LIGHT_PURPLE,
    width=1
)

# Simplified mountain icon
cx, cy = 64, 45
left_peak = [(cx-20, cy+20), (cx-10, cy-15), (cx, cy+20)]
draw_128.polygon(left_peak, fill=ASPIRE_PURPLE_1)

center_peak = [(cx, cy+20), (cx+15, cy-25), (cx+30, cy+20)]
draw_128.polygon(center_peak, fill=ASPIRE_PURPLE_2)

right_peak = [(cx+30, cy+20), (cx+40, cy-10), (cx+50, cy+20)]
draw_128.polygon(right_peak, fill=ASPIRE_PURPLE_3)

status_radius = 5
draw_128.ellipse([(cx+15-status_radius, cy+35-status_radius), (cx+15+status_radius, cy+35+status_radius)], fill=GREEN)

img_128.save('aspire-monitor-icon-128.png', 'PNG', optimize=True)
print("✓ Created: aspire-monitor-icon-128.png (128x128)")

# 3. Create 1200x630 LinkedIn Graphic
print("Creating aspire-monitor-linkedin.png...")
img_linkedin = Image.new('RGBA', (1200, 630), (0, 0, 0, 0))
draw_linkedin = ImageDraw.Draw(img_linkedin)

# Background with gradient
for i in range(630):
    ratio = i / 630
    r = int(ASPIRE_DARK_PURPLE[0] + (ASPIRE_PURPLE_1[0] - ASPIRE_DARK_PURPLE[0]) * ratio)
    g = int(ASPIRE_DARK_PURPLE[1] + (ASPIRE_PURPLE_1[1] - ASPIRE_DARK_PURPLE[1]) * ratio)
    b = int(ASPIRE_DARK_PURPLE[2] + (ASPIRE_PURPLE_1[2] - ASPIRE_DARK_PURPLE[2]) * ratio)
    draw_linkedin.line([(0, i), (1200, i)], fill=(r, g, b, 255))

# White content area
draw_linkedin.rectangle([(80, 100), (1120, 530)], fill=WHITE, outline=ASPIRE_DARK_PURPLE, width=3)

# Large mountain icon in center-left
icon_x, icon_y = 250, 315
scale = 3
left_peak = [(icon_x-40*scale, icon_y+40*scale), (icon_x-20*scale, icon_y-30*scale), (icon_x, icon_y+40*scale)]
draw_linkedin.polygon(left_peak, fill=ASPIRE_DARK_PURPLE)

center_peak = [(icon_x, icon_y+40*scale), (icon_x+30*scale, icon_y-50*scale), (icon_x+60*scale, icon_y+40*scale)]
draw_linkedin.polygon(center_peak, fill=ASPIRE_PURPLE_2)

# Status indicators
status_size = 25
draw_linkedin.ellipse([(800, 200), (800+status_size, 200+status_size)], fill=GREEN)
draw_linkedin.ellipse([(800, 280), (800+status_size, 280+status_size)], fill=YELLOW)
draw_linkedin.ellipse([(800, 360), (800+status_size, 360+status_size)], fill=RED)

# Text: "Monitor" and "Aspire Distributed Apps"
try:
    title_font = ImageFont.truetype("arial.ttf", 72)
    subtitle_font = ImageFont.truetype("arial.ttf", 42)
except:
    title_font = ImageFont.load_default()
    subtitle_font = ImageFont.load_default()

draw_linkedin.text((550, 180), "Monitor", fill=DARK_GRAY, font=title_font)
draw_linkedin.text((550, 280), "Aspire Distributed Apps", fill=ASPIRE_DARK_PURPLE, font=subtitle_font)

img_linkedin.save('aspire-monitor-linkedin.png', 'PNG', optimize=True)
print("✓ Created: aspire-monitor-linkedin.png (1200x630)")

# 4. Create 1024x512 Twitter Graphic
print("Creating aspire-monitor-twitter.png...")
img_twitter = Image.new('RGBA', (1024, 512), (0, 0, 0, 0))
draw_twitter = ImageDraw.Draw(img_twitter)

# Bold gradient background
for i in range(512):
    ratio = i / 512
    r = int(ASPIRE_DARK_PURPLE[0] + (ASPIRE_PURPLE_2[0] - ASPIRE_DARK_PURPLE[0]) * ratio)
    g = int(ASPIRE_DARK_PURPLE[1] + (ASPIRE_PURPLE_2[1] - ASPIRE_DARK_PURPLE[1]) * ratio)
    b = int(ASPIRE_DARK_PURPLE[2] + (ASPIRE_PURPLE_2[2] - ASPIRE_DARK_PURPLE[2]) * ratio)
    draw_twitter.line([(0, i), (1024, i)], fill=(r, g, b, 255))

# Large mountain icon
icon_x, icon_y = 200, 256
scale = 4
left_peak = [(icon_x-40*scale, icon_y+40*scale), (icon_x-20*scale, icon_y-30*scale), (icon_x, icon_y+40*scale)]
draw_twitter.polygon(left_peak, fill=WHITE)

center_peak = [(icon_x, icon_y+40*scale), (icon_x+30*scale, icon_y-50*scale), (icon_x+60*scale, icon_y+40*scale)]
draw_twitter.polygon(center_peak, fill=ASPIRE_LIGHT_PURPLE)

# Status indicators
for idx, color in enumerate([GREEN, YELLOW, RED]):
    draw_twitter.ellipse([(550+idx*120, 200), (600+idx*120, 250)], fill=color)

# Text
try:
    big_font = ImageFont.truetype("arial.ttf", 64)
    small_font = ImageFont.truetype("arial.ttf", 32)
except:
    big_font = ImageFont.load_default()
    small_font = ImageFont.load_default()

draw_twitter.text((550, 120), "Real-time Monitoring", fill=WHITE, font=big_font)
draw_twitter.text((550, 320), "for Aspire Apps", fill=ASPIRE_LIGHT_PURPLE, font=small_font)

img_twitter.save('aspire-monitor-twitter.png', 'PNG', optimize=True)
print("✓ Created: aspire-monitor-twitter.png (1024x512)")

# 5. Create 1200x630 Blog Header
print("Creating aspire-monitor-blog.png...")
img_blog = Image.new('RGBA', (1200, 630), (0, 0, 0, 0))
draw_blog = ImageDraw.Draw(img_blog)

# Split design: gradient on left, solid on right
for i in range(1200):
    ratio = i / 1200
    for j in range(630):
        if i < 600:
            r = int(ASPIRE_DARK_PURPLE[0] + (ASPIRE_PURPLE_2[0] - ASPIRE_DARK_PURPLE[0]) * (i / 600))
            g = int(ASPIRE_DARK_PURPLE[1] + (ASPIRE_PURPLE_2[1] - ASPIRE_DARK_PURPLE[1]) * (i / 600))
            b = int(ASPIRE_DARK_PURPLE[2] + (ASPIRE_PURPLE_2[2] - ASPIRE_DARK_PURPLE[2]) * (i / 600))
        else:
            r, g, b = ASPIRE_PURPLE_2
        draw_blog.point((i, j), fill=(r, g, b, 255))

# Mountain icons scattered
scales = [2, 2.5, 2]
positions = [(150, 200), (350, 350), (500, 150)]
colors = [ASPIRE_PURPLE_1, ASPIRE_PURPLE_3, ASPIRE_LIGHT_PURPLE]

for pos, s, color in zip(positions, scales, colors):
    icon_x, icon_y = pos
    left_peak = [(icon_x-40*s, icon_y+40*s), (icon_x-20*s, icon_y-30*s), (icon_x, icon_y+40*s)]
    draw_blog.polygon(left_peak, fill=color)

# Title and subtitle
try:
    header_font = ImageFont.truetype("arial.ttf", 68)
    subheader_font = ImageFont.truetype("arial.ttf", 40)
except:
    header_font = ImageFont.load_default()
    subheader_font = ImageFont.load_default()

draw_blog.text((700, 150), "Aspire Monitor", fill=WHITE, font=header_font)
draw_blog.text((700, 260), "Real-time Dashboard for", fill=ASPIRE_LIGHT_PURPLE, font=subheader_font)
draw_blog.text((700, 330), "Distributed Applications", fill=WHITE, font=subheader_font)

img_blog.save('aspire-monitor-blog.png', 'PNG', optimize=True)
print("✓ Created: aspire-monitor-blog.png (1200x630)")

print("\n" + "="*50)
print("All Aspire-branded images created successfully!")
print("="*50)
print("\nDesign Elements Applied:")
print("  • Primary Color: Deep Purple #512BD4")
print("  • Gradient Accents: #7455DD → #9780E5 → #B9AAEE")
print("  • Mountain/Chevron Icon: Aspire visual language")
print("  • Status Indicators: Green (healthy), Yellow (warning), Red (critical)")
print("  • Professional, clean, modern aesthetic")
