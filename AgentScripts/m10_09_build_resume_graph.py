"""Add one executable line node per production paragraph, then emit an audit manifest.

The script is idempotent: existing line nodes (including the S001-016 prototype)
are retained. It never copies a paragraph or changes a #line tag. Run from the
repository root. The manifest records the authored information/diagram context
and identifies occurrences that cannot safely be resumed.
"""
import json
import pathlib
import re
import shlex

ROOT = pathlib.Path(__file__).resolve().parents[1]
FILES = [ROOT / 'Assets/_Project/Yarn' / f'PROLOGUE_S{a}_S{b}.yarn'
         for a, b in [('001', '003'), ('004', '006'), ('007', '008')]]
TARGET = ROOT / 'Assets/_Project/Resources/M10_09_LINE_RESUME_MANIFEST.json'
LINE = re.compile(r'#line:(prologue_s(\d{3})_(\d{3}))\b')
TITLE = re.compile(r'^title: (\w+)\s*$')
BEAT = re.compile(r'^PROLOGUE_S\d{3}_P\d{2}$')
ENTRY = re.compile(r'^PROLOGUE_LINE_S\d{3}_\d{3}$')


def main():
    entries = []
    successors = {}
    overlay = None
    diagram = False
    effect = False
    for path in FILES:
        lines = path.read_text(encoding='utf-8-sig').splitlines(keepends=True)
        output = []
        current = None
        source_node = None
        line_in_node = False
        for line in lines:
            title = TITLE.match(line.rstrip('\r\n'))
            if title:
                current = title.group(1)
                line_in_node = False
                if BEAT.match(current): source_node = current
            if '<<vn_info_show ' in line:
                args = shlex.split(line.split('<<vn_info_show ', 1)[1].split('>>', 1)[0])
                if len(args) != 4: raise RuntimeError(f'Unexpected information command: {line}')
                overlay = (args[0], '\n'.join(x for x in args[1:] if x))
            if '<<vn_info_hide>>' in line: overlay = None
            if '<<vn_medical_diagram_show>>' in line: diagram = True
            if '<<vn_medical_diagram_hide>>' in line: diagram = False
            if '<<vn_local_effect on ' in line: effect = True
            if '<<vn_local_effect off ' in line: effect = False
            match = LINE.search(line)
            if not match:
                output.append(line)
                continue
            tag, scene, number = match.groups()
            node = f'PROLOGUE_LINE_S{scene}_{number}'
            if not (current == node and not line_in_node):
                output.extend([f'<<jump {node}>>\n', '===\n\n', f'title: {node}\n', '---\n'])
                current = node
            output.append(line)
            line_in_node = True
            reason = ''
            if tag in ('prologue_s008_009', 'prologue_s008_010'):
                reason = 'profile browser review thought'
            elif tag == 'prologue_s008_041': reason = 'terminal one-shot audio sequence'
            elif tag == 'prologue_s004_034': reason = 'manual-only hold occurrence'
            elif effect: reason = 'localized white-door effect'
            entries.append({'runtimeLineId': 'line:' + tag,
                            'sourceYarnFile': path.name,
                            'sourceNode': source_node,
                            'resumeEntryNode': node,
                            'scene': 'S' + scene,
                            'presentationBeat': source_node,
                            'eligibility': 'SAVE_BLOCKED_TRANSIENT' if reason else 'SAVE_ELIGIBLE',
                            'restrictionReason': reason,
                            'informationTitle': overlay[0] if overlay else '',
                            'informationFacts': overlay[1] if overlay else '',
                            'medicalDiagram': diagram})
        transformed = ''.join(output)
        for match in re.finditer(r'(?ms)^title: (PROLOGUE_LINE_S\d{3}_\d{3})\n---\n(.*?)^===', transformed):
            jumps = re.findall(r'<<jump (\w+)>>', match.group(2))
            if len(jumps) != 1:
                raise RuntimeError(f'{match.group(1)} must have exactly one successor jump')
            successors[match.group(1)] = jumps[0]
        path.write_text(transformed, encoding='utf-8', newline='')
    if len(entries) != 379 or len({e['runtimeLineId'] for e in entries}) != 379:
        raise RuntimeError(f'Expected 379 unique paragraphs; got {len(entries)}')
    for entry in entries:
        entry['successorNodeOrFlow'] = successors[entry['resumeEntryNode']]
    TARGET.write_text(json.dumps({'entries': entries}, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    eligible = sum(e['eligibility'] == 'SAVE_ELIGIBLE' for e in entries)
    print(f'{len(entries)} paragraphs, {eligible} eligible, {len(entries)-eligible} restricted')

if __name__ == '__main__': main()
