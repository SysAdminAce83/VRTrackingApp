import math
from PIL import Image, ImageDraw, ImageFont

# Canvas setup
WIDTH, HEIGHT = 1200, 600
BG_DARK = (8, 18, 38)        # deep navy
BG_MID = (12, 28, 56)        # slightly lighter navy for gradient
CYAN = (0, 229, 255)         # bright cyan accent
WHITE = (255, 255, 255)
LIGHT_CYAN = (180, 245, 255) # soft cyan for glow

FONT_DIR = r"C:\Users\krishnaramanan.S-SA\.kilocode\skills\canvas-design\canvas-fonts"

img = Image.new("RGBA", (WIDTH, HEIGHT), BG_DARK)
draw = ImageDraw.Draw(img)

# Create a vertical gradient background
for y in range(HEIGHT):
    r = int(BG_DARK[0] + (BG_MID[0] - BG_DARK[0]) * (y / HEIGHT) * 0.6)
    g = int(BG_DARK[1] + (BG_MID[1] - BG_DARK[1]) * (y / HEIGHT) * 0.6)
    b = int(BG_DARK[2] + (BG_MID[2] - BG_DARK[2]) * (y / HEIGHT) * 0.6)
    draw.line([(0, y), (WIDTH, y)], fill=(r, g, b, 255))

# --- Geometric shield / crosshair mark ---
cx, cy = 280, 300  # center of the mark
outer_r = 110
inner_r = 75
ring_r = 55

# Outer glow rings (concentric, subtle)
for r in range(outer_r + 40, outer_r, -4):
    alpha = int(30 * (1 - (r - outer_r) / 40))
    draw.ellipse([cx - r, cy - r, cx + r, cy + r], outline=(*CYAN, alpha))

# Main outer ring (solid cyan)
draw.ellipse([cx - outer_r, cy - outer_r, cx + outer_r, cy + outer_r], outline=CYAN, width=6)

# Inner ring (thin, precise)
draw.ellipse([cx - inner_r, cy - inner_r, cx + inner_r, cy + inner_r], outline=(*CYAN, 180), width=3)

# Crosshair lines (horizontal and vertical, precise)
line_w = 3
gap = 18  # gap around center
draw.line([(cx - outer_r, cy), (cx - gap, cy)], fill=CYAN, width=line_w)
draw.line([(cx + gap, cy), (cx + outer_r, cy)], fill=CYAN, width=line_w)
draw.line([(cx, cy - outer_r), (cx, cy - gap)], fill=CYAN, width=line_w)
draw.line([(cx, cy + gap), (cx, cy + outer_r)], fill=CYAN, width=line_w)

# Diagonal crosshair (45 degrees, shorter)
diag_len = int(outer_r * 0.65)
d = int(diag_len * math.cos(math.radians(45)))
for sign_x, sign_y in [(-1, -1), (-1, 1), (1, -1), (1, 1)]:
    x1 = cx + sign_x * gap * math.cos(math.radians(45))
    y1 = cy + sign_y * gap * math.sin(math.radians(45))
    x2 = cx + sign_x * d
    y2 = cy + sign_y * d
    draw.line([(x1, y1), (x2, y2)], fill=(*CYAN, 140), width=2)

# Center dot (the "target")
draw.ellipse([cx - 8, cy - 8, cx + 8, cy + 8], fill=CYAN)

# Inner checkmark (remediation = fixed/resolved)
# A clean checkmark inside the inner ring
ck_x, ck_y = cx, cy + 10
ck_size = 28
# Left stroke of check
draw.line([(ck_x - ck_size, ck_y + 4), (ck_x - 6, ck_y + ck_size - 4)], fill=WHITE, width=8)
# Right stroke of check
draw.line([(ck_x - 6, ck_y + ck_size - 4), (ck_x + ck_size + 2, ck_y - ck_size + 10)], fill=WHITE, width=8)

# --- Text section ---
text_x = 460
text_y_base = 210

# Load fonts
try:
    font_bold = ImageFont.truetype(FONT_DIR + "\\BigShoulders-Bold.ttf", 96)
    font_reg = ImageFont.truetype(FONT_DIR + "\\WorkSans-Regular.ttf", 32)
    font_mono = ImageFont.truetype(FONT_DIR + "\\JetBrainsMono-Regular.ttf", 22)
except Exception:
    font_bold = ImageFont.load_default()
    font_reg = ImageFont.load_default()
    font_mono = ImageFont.load_default()

# "Remediate" in bold white
draw.text((text_x, text_y_base), "Remediate", font=font_bold, fill=WHITE)

# "VR" in cyan, same baseline, same font
vr_text = "VR"
vr_bbox = draw.textbbox((0, 0), "Remediate", font=font_bold)
remediate_width = vr_bbox[2] - vr_bbox[0]
draw.text((text_x + remediate_width + 8, text_y_base), vr_text, font=font_bold, fill=CYAN)

# Tagline
tag_y = text_y_base + 120
draw.text((text_x, tag_y), "Vulnerability Remediation Console", font=font_reg, fill=(180, 200, 220))

# Version / build line
build_y = tag_y + 55
draw.text((text_x, build_y), "v1.0 · Nessus ingestion · Enterprise exception workflow", font=font_mono, fill=(100, 130, 160))

# Bottom accent line (thin cyan rule)
rule_y = build_y + 45
rule_width = remediate_width + 8 + draw.textbbox((0, 0), vr_text, font=font_bold)[2]
draw.line([(text_x, rule_y), (text_x + rule_width, rule_y)], fill=CYAN, width=3)

# Save
out_path = r"D:\Project_WebApp\VRTrackingApp\docs\branding\RemediateVR_Logo.png"
img.save(out_path, "PNG")
print(f"Logo saved to: {out_path}")
