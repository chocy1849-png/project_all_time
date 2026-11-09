#!/usr/bin/env python3
"""Compare the live LinePresenter capture with all canonical production Yarn paragraphs."""
import sys
from pathlib import Path
from urllib.parse import unquote

ROOT = Path(__file__).resolve().parents[1]
TEST_ROOT = Path(sys.argv[1])
CAPTURE = TEST_ROOT / "Reports" / "m10_12_continuous_story.tsv"
YARN_FILES = (
    ROOT / "Assets/_Project/Yarn/PROLOGUE_S001_S003.yarn",
    ROOT / "Assets/_Project/Yarn/PROLOGUE_S004_S006.yarn",
    ROOT / "Assets/_Project/Yarn/PROLOGUE_S007_S008.yarn",
)

expected = []
for yarn_path in YARN_FILES:
    for source_line in yarn_path.read_text(encoding="utf-8").splitlines():
        tag = "#line:prologue_s"
        if tag not in source_line:
            continue
        text, suffix = source_line.rsplit("#line:", 1)
        line_id = "line:" + suffix.strip()
        authored = text.strip()
        if ":" in authored:
            speaker, body = authored.split(":", 1)
            speaker, body = speaker.strip(), body.strip()
        else:
            speaker, body = "", authored
        expected.append((line_id, speaker, body))

actual = []
for line in CAPTURE.read_text(encoding="utf-8").splitlines():
    fields = line.split("\t")
    if fields[0] == "LINE":
        actual.append((fields[1], unquote(fields[2]), unquote(fields[3])))

assert len(expected) == 379, f"Yarn source paragraphs: expected 379, got {len(expected)}"
assert len(actual) == 379, f"Live LinePresenter paragraphs: expected 379, got {len(actual)}"
assert len({line_id for line_id, _, _ in expected}) == 379, "Duplicate explicit production line ID in source"
assert len({line_id for line_id, _, _ in actual}) == 379, "Duplicate or missing live line ID"

mismatches = []
for index, (expected_line, actual_line) in enumerate(zip(expected, actual), start=1):
    if expected_line != actual_line:
        mismatches.append((index, expected_line, actual_line))
        if len(mismatches) == 5:
            break

if mismatches:
    for index, source, live in mismatches:
        print(f"MISMATCH {index}\n  Yarn: {source!r}\n  Live: {live!r}")
    raise SystemExit(1)

scene_order = [line_id.split("_")[1] for line_id, _, _ in actual]
unique_scenes = []
for scene in scene_order:
    if not unique_scenes or unique_scenes[-1] != scene:
        unique_scenes.append(scene)
assert unique_scenes == ["s001", "s002", "s003", "s004", "s005", "s006", "s007", "s008"], unique_scenes

print("PASS: 379/379 live LinePresenter IDs, speakers and Korean paragraphs match production Yarn exactly.")
print("PASS: no missing/duplicate line ID; scene chain S001 → S002 → S003 → S004 → S005 → S006 → S007 → S008.")
