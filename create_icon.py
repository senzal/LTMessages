from PIL import Image, ImageDraw, ImageFont
import os

# Create a 128x128 image with transparency
size = 128
img = Image.new('RGBA', (size, size), color=(0, 0, 0, 0))
draw = ImageDraw.Draw(img)

# GW2-style colors
bg_color = (25, 20, 15, 220)  # Dark brown background
border_outer = (80, 60, 40, 255)  # Bronze/copper outer border
border_inner = (120, 100, 70, 255)  # Lighter bronze inner border
text_color = (220, 200, 150, 255)  # Gold/cream text color
shadow_color = (10, 8, 5, 180)  # Dark shadow

# Draw rounded background with border (GW2 style)
margin = 8
# Outer border
draw.rounded_rectangle([margin, margin, size-margin, size-margin],
                       radius=12, fill=border_outer)
# Inner border
draw.rounded_rectangle([margin+3, margin+3, size-margin-3, size-margin-3],
                       radius=10, fill=border_inner)
# Background
draw.rounded_rectangle([margin+5, margin+5, size-margin-5, size-margin-5],
                       radius=8, fill=bg_color)

# Try to use a bold system font, fall back to default if not available
try:
    font = ImageFont.truetype("arialbd.ttf", 52)
except:
    try:
        font = ImageFont.truetype("arial.ttf", 52)
    except:
        font = ImageFont.load_default()

# Draw "LT" text in GW2 gold color, centered
text = "LT"
# Get text bounding box for centering
bbox = draw.textbbox((0, 0), text, font=font)
text_width = bbox[2] - bbox[0]
text_height = bbox[3] - bbox[1]

# Calculate position to center text
x = (size - text_width) // 2
y = (size - text_height) // 2 - 4

# Draw text with multiple shadows for depth (GW2 style embossed effect)
draw.text((x+2, y+2), text, fill=shadow_color, font=font)  # Main shadow
draw.text((x-1, y-1), text, fill=(255, 240, 200, 100), font=font)  # Highlight
draw.text((x, y), text, fill=text_color, font=font)  # Main text

# Save to ref folder
output_path = os.path.join("ref", "icon.png")
img.save(output_path, "PNG")
print(f"GW2-style icon created at: {output_path}")
