"""Validate the bamboo pixelspec JSON before rendering."""
import json
import sys

path = r"C:\UnityProject\MyRoadNeverEnd\art\bamboo\bamboo_cluster.pixelspec.json"
with open(path, encoding='utf-8') as f:
    spec = json.load(f)

W, H = spec['canvas']
palette_chars = set(spec['palette'].keys()) | {'.'}
errors = []

for pose_name, pose in spec['poses'].items():
    rows = pose['rows']
    if len(rows) != H:
        errors.append(f"POSE {pose_name}: has {len(rows)} rows, expected {H}")
        continue
    for i, row in enumerate(rows):
        if len(row) != W:
            errors.append(f"POSE {pose_name} row {i}: len={len(row)} != {W}  [{row!r}]")
        for ci, ch in enumerate(row):
            if ch not in palette_chars:
                errors.append(f"POSE {pose_name} row {i} col {ci}: bad char {ch!r}")
    # Border checks
    if any(c != '.' for c in rows[0]):
        errors.append(f"POSE {pose_name} row 0: not all dots: {rows[0]!r}")
    if any(c != '.' for c in rows[H-1]):
        errors.append(f"POSE {pose_name} row {H-1}: not all dots: {rows[H-1]!r}")
    for i, row in enumerate(rows):
        if len(row) >= 1 and row[0] != '.':
            errors.append(f"POSE {pose_name} row {i} col 0: {row[0]!r}")
        if len(row) == W and row[W-1] != '.':
            errors.append(f"POSE {pose_name} row {i} col {W-1}: {row[W-1]!r}")

if errors:
    print(f"ERRORS ({len(errors)}):")
    for e in errors:
        print(f"  {e}")
    sys.exit(1)
else:
    print(f"All {len(spec['poses'])} poses VALID ({W}x{H})")
