"""
Generate bamboo_cluster.pixelspec.json
Canvas: 42x58, Anchor: [21,57]
"""

import json

W = 42
H = 58

# Build the idle pose grid as a list of 58 strings, each 42 chars
# Characters: k T t G g L H .
# Left stalk: cols 11-12, rows 18-56
# Right stalk: cols 27-28, rows 20-56
# Joints every 6-8 rows (bright t)

def make_empty():
    return [['.' for _ in range(W)] for _ in range(H)]

def put(grid, r, c, ch):
    if 0 <= r < H and 0 <= c < W:
        grid[r][c] = ch

def put_range(grid, r, c1, c2, ch):
    for c in range(c1, c2+1):
        put(grid, r, c, ch)

def grid_to_rows(grid):
    return [''.join(row) for row in grid]

def validate(rows):
    assert len(rows) == H, f"Expected {H} rows, got {len(rows)}"
    for i, row in enumerate(rows):
        assert len(row) == W, f"Row {i} length {len(row)} != {W}"
        for c_idx, ch in enumerate(row):
            assert ch in set('kTtGgLH.'), f"Row {i} col {c_idx}: invalid char '{ch}'"
    # Check border pixels
    assert all(ch == '.' for ch in rows[0]), "Row 0 must be all dots"
    assert all(ch == '.' for ch in rows[H-1]), f"Row {H-1} must be all dots"
    for r, row in enumerate(rows):
        assert row[0] == '.', f"Row {r} col 0 must be dot"
        assert row[W-1] == '.', f"Row {r} col {W-1} must be dot"
    return True

# ---- LEAF DRAWING HELPERS ----

def draw_leaf_left(grid, tip_r, tip_c, length=10, width_max=5):
    """Draw a leaf pointing left from tip at (tip_r, tip_c)"""
    # Tip -> base going right
    half = length // 2
    for i in range(length):
        w = int(width_max * min(i, length-i) / (length/2))
        w = max(1, w)
        for j in range(w):
            r = tip_r - w//2 + j
            c = tip_c + i
            if i == 0:
                ch = 'H'
            elif i < length//3:
                ch = 'L'
            elif i < 2*length//3:
                ch = 'G'
            else:
                ch = 'g'
            put(grid, r, c, ch)

def draw_leaf_right(grid, tip_r, tip_c, length=10, width_max=5):
    """Draw a leaf pointing right from tip at (tip_r, tip_c)"""
    for i in range(length):
        w = int(width_max * min(i, length-i) / (length/2))
        w = max(1, w)
        for j in range(w):
            r = tip_r - w//2 + j
            c = tip_c - i
            if i == 0:
                ch = 'H'
            elif i < length//3:
                ch = 'L'
            elif i < 2*length//3:
                ch = 'G'
            else:
                ch = 'g'
            put(grid, r, c, ch)

def draw_leaf_up_left(grid, base_r, base_c, length=10, width_max=4):
    """Draw a diagonal leaf going up-left"""
    for i in range(length):
        w = max(1, int(width_max * min(i, length-i) / (length/2)))
        r = base_r - i
        c = base_c - i//2
        for j in range(w):
            if i == 0:
                ch = 'g'
            elif i < length//3:
                ch = 'G'
            elif i < 2*length//3:
                ch = 'L'
            else:
                ch = 'H'
            put(grid, r + j - w//2, c, ch)

def draw_leaf_up_right(grid, base_r, base_c, length=10, width_max=4):
    """Draw a diagonal leaf going up-right"""
    for i in range(length):
        w = max(1, int(width_max * min(i, length-i) / (length/2)))
        r = base_r - i
        c = base_c + i//2
        for j in range(w):
            if i == 0:
                ch = 'g'
            elif i < length//3:
                ch = 'G'
            elif i < 2*length//3:
                ch = 'L'
            else:
                ch = 'H'
            put(grid, r + j - w//2, c, ch)

# ---- BUILD IDLE POSE ----

def build_idle():
    grid = make_empty()

    # === LEFT STALK: cols 11-12, rows 18-56 ===
    # Joints at rows 18, 26, 34, 42, 50
    left_joints = {18, 26, 34, 42, 50}
    for r in range(18, 57):
        c1, c2 = 11, 12
        if r in left_joints:
            # joint: both t (bright)
            put(grid, r, c1, 't')
            put(grid, r, c2, 't')
        else:
            # alternating T/t
            if (r % 2 == 0):
                put(grid, r, c1, 'T')
                put(grid, r, c2, 't')
            else:
                put(grid, r, c1, 't')
                put(grid, r, c2, 'T')

    # === RIGHT STALK: cols 27-28, rows 20-56 ===
    right_joints = {20, 28, 36, 44, 52}
    for r in range(20, 57):
        c1, c2 = 27, 28
        if r in right_joints:
            put(grid, r, c1, 't')
            put(grid, r, c2, 't')
        else:
            if (r % 2 == 0):
                put(grid, r, c1, 'T')
                put(grid, r, c2, 't')
            else:
                put(grid, r, c1, 't')
                put(grid, r, c2, 'T')

    # === LEAVES ===
    # Leaf cluster 1: node at (18, 11-12), toa sang trai
    # x=2..12, y=4..14  => rows 4-14, cols 2-12
    # Leaf pointing left from col 11, row 9
    draw_leaf_left(grid, 9, 2, length=9, width_max=4)   # horizontal left
    draw_leaf_up_left(grid, 13, 10, length=10, width_max=4)  # up-left diagonal

    # Leaf cluster 2: node at (26, 11-12), toa sang hai ben
    # x=4..22, y=14..24
    draw_leaf_left(grid, 19, 4, length=8, width_max=4)   # left
    draw_leaf_right(grid, 21, 22, length=9, width_max=4)  # right

    # Leaf cluster 3: node at (28, 27-28), toa sang phai
    # x=22..38, y=8..18
    draw_leaf_right(grid, 13, 38, length=10, width_max=5)  # right tip at col38
    draw_leaf_up_right(grid, 17, 28, length=10, width_max=4) # up-right

    # Small center leaves: x=15..28, y=2..12
    draw_leaf_right(grid, 7, 28, length=9, width_max=4)
    draw_leaf_left(grid, 5, 15, length=8, width_max=3)

    # Extra leaves from joint nodes
    draw_leaf_up_right(grid, 25, 11, length=9, width_max=4)  # from left stalk joint
    draw_leaf_up_left(grid, 27, 28, length=8, width_max=4)   # from right stalk

    # Lower leaves
    draw_leaf_left(grid, 33, 9, length=7, width_max=3)
    draw_leaf_right(grid, 35, 30, length=7, width_max=3)

    rows = grid_to_rows(grid)
    return rows

# ---- SWAY: shift leaf pixels left by dx ----
# We define which rows are "leaf rows" (i.e. above the stalk starts, plus leaf regions)
# For simplicity: rows 1-17 are pure leaf, rows 18-56 have both stalk and leaf
# We shift only cols that are NOT the stalk columns (11,12,27,28) in rows 18+

STALK_COLS = {11, 12, 27, 28}

def sway_pose(idle_rows, dx):
    """Shift leaf pixels left by dx. Stalk pixels stay. dx>0 = shift left."""
    if dx == 0:
        return idle_rows
    
    result = []
    for r_idx, row in enumerate(idle_rows):
        pixels = list(row)
        new_row = ['.' for _ in range(W)]
        
        for c in range(W):
            ch = pixels[c]
            if ch == '.':
                continue
            
            # Determine if stalk pixel
            is_stalk = (r_idx >= 18 and c in STALK_COLS and ch in ('T', 't'))
            
            if is_stalk:
                new_row[c] = ch
            else:
                # shift left by dx
                nc = c - dx
                if 0 < nc < W-1:  # keep margin
                    new_row[nc] = ch
                # else drop pixel (out of margin)
        
        result.append(''.join(new_row))
    
    return result

# ---- GENERATE ALL POSES ----

idle_rows = build_idle()

# Validate idle
try:
    validate(idle_rows)
    print("Idle pose VALID")
except AssertionError as e:
    print(f"Idle validation ERROR: {e}")
    # Print problematic rows
    for i, row in enumerate(idle_rows):
        if len(row) != W:
            print(f"  Row {i}: len={len(row)}")
        if row[0] != '.' or row[-1] != '.':
            print(f"  Row {i}: border issue: [{row[0]}...{row[-1]}]")
        for c_idx, ch in enumerate(row):
            if ch not in set('kTtGgLH.'):
                print(f"  Row {i} col {c_idx}: bad char '{ch}'")

sway1_rows = sway_pose(idle_rows, 1)
sway2_rows = sway_pose(idle_rows, 2)

# Validate sway poses
for name, rows in [("sway1", sway1_rows), ("sway2", sway2_rows)]:
    try:
        validate(rows)
        print(f"{name} pose VALID")
    except AssertionError as e:
        print(f"{name} validation ERROR: {e}")

# ---- BUILD JSON ----

spec = {
    "schema": "codeart2d.pixelspec.v1",
    "name": "bamboo_cluster",
    "canvas": [W, H],
    "anchor_px": [21, 57],
    "palette": {
        "k": "#182010",
        "T": "#3a5a15",
        "t": "#5a8222",
        "G": "#4aaa28",
        "g": "#2d7010",
        "L": "#80d840",
        "H": "#b4f060"
    },
    "outline": {"mode": "solid", "color": "k"},
    "poses": {
        "idle":  {"rows": idle_rows},
        "sway1": {"rows": sway1_rows},
        "sway2": {"rows": sway2_rows}
    },
    "layers": [
        {
            "name": "scene",
            "origin": [0, 0],
            "rows": "idle"
        }
    ],
    "frames": [
        {"name": "f0", "layers": {"scene": {"rows": "idle"}}},
        {"name": "f1", "layers": {"scene": {"rows": "sway1"}}},
        {"name": "f2", "layers": {"scene": {"rows": "sway2"}}},
        {"name": "f3", "layers": {"scene": {"rows": "sway1"}}}
    ],
    "clips": {
        "sway": {
            "frames": ["f0", "f1", "f2", "f3"],
            "ticks": [8, 6, 9, 6],
            "tick_hz": 60,
            "loop_policy": "pingpong"
        }
    }
}

out_path = r"C:\UnityProject\MyRoadNeverEnd\art\bamboo\bamboo_cluster.pixelspec.json"
with open(out_path, 'w', encoding='utf-8') as f:
    json.dump(spec, f, ensure_ascii=False, indent=2)

print(f"Written to {out_path}")

# Print a sample of rows for visual inspection
print("\n--- Idle rows sample (rows 0-25) ---")
for i, row in enumerate(idle_rows[:26]):
    print(f"R{i:02d}: {row}")
