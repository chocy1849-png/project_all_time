"""Create a deterministic append-only manifest without modifying production Yarn.

The additions JSON is an array of explicit entry records. New Yarn nodes must
already exist under --yarn-root; Unity's compiled-graph validator remains the
authority for executable nodes and successors. This tool never edits Yarn.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
DESCRIPTOR = ROOT / 'Assets/_Project/Resources/M10_15_BASELINE_DESCRIPTOR.json'
DESCRIPTOR_SHA = '126088f2c52fc7fc5c8eaa860c70240e345e937929578566a7967b59f984a8f0'
NODE = re.compile(r'(?ms)^title:\s*(\w+)\s*\r?\n---\s*\r?\n(.*?)^===\s*$')
LINE = re.compile(r'#line:([A-Za-z0-9_]+)\b')
JUMP = re.compile(r'<<\s*jump\s+(\w+)\s*>>')


def load_baseline():
    data = DESCRIPTOR.read_bytes().replace(b'\r\n', b'\n')
    if hashlib.sha256(data).hexdigest() != DESCRIPTOR_SHA:
        raise ValueError('Immutable M10 baseline descriptor does not match its approved hash')
    return json.loads(json.loads(data)['manifestJson'])['entries']


def extend(base_path, additions_path, yarn_root, output):
    base_path, additions_path, yarn_root, output = map(Path, (base_path, additions_path, yarn_root, output))
    if output.resolve() in {base_path.resolve(), additions_path.resolve(), DESCRIPTOR.resolve()}:
        raise ValueError('Output must not overwrite an input or the baseline descriptor')
    base_bytes = base_path.read_bytes()
    entries = json.loads(base_bytes)['entries']
    additions = json.loads(additions_path.read_text(encoding='utf-8'))
    if not isinstance(entries, list) or not entries or not isinstance(additions, list):
        raise ValueError('Expected a nonempty manifest and an additions array')
    by_id = {item['runtimeLineId']: item for item in entries}
    if len(by_id) != len(entries):
        raise ValueError('Duplicate existing line ID')
    for original in load_baseline():
        if by_id.get(original['runtimeLineId']) != original:
            raise ValueError('The original M10 mapping was removed or changed')
    nodes = {item['resumeEntryNode'] for item in entries}
    if len(nodes) != len(entries):
        raise ValueError('Duplicate existing resume node')
    sources = {}
    root = yarn_root.resolve()
    required = ('runtimeLineId', 'sourceYarnFile', 'sourceNode', 'resumeEntryNode', 'scene',
                'presentationBeat', 'eligibility', 'successorNodeOrFlow', 'chapterId', 'sceneTitle')
    for item in additions:
        if not isinstance(item, dict) or any(not isinstance(item.get(k), str) or not item[k].strip() for k in required):
            raise ValueError('Every new entry needs explicit line, source, scene, eligibility and chapter metadata')
        line, node = item['runtimeLineId'], item['resumeEntryNode']
        if not re.fullmatch(r'line:[A-Za-z0-9_]+', line) or line in by_id or node in nodes:
            raise ValueError('Invalid or duplicate new line ID or resume node')
        if item['eligibility'] not in ('SAVE_ELIGIBLE', 'SAVE_BLOCKED_TRANSIENT'):
            raise ValueError('Unsupported eligibility')
        reason = item.get('restrictionReason', '')
        if (item['eligibility'] == 'SAVE_BLOCKED_TRANSIENT') != bool(reason.strip()):
            raise ValueError('Restriction reason does not match eligibility')
        path = (root / item['sourceYarnFile']).resolve()
        if root not in path.parents or not path.is_file():
            raise ValueError('New source must be a file inside --yarn-root')
        if path not in sources:
            matches = list(NODE.finditer(path.read_text(encoding='utf-8-sig')))
            sources[path] = {m.group(1): m.group(2) for m in matches}
            if len(sources[path]) != len(matches):
                raise ValueError('Duplicate source node title')
        bodies = sources[path]
        if item['sourceNode'] not in bodies or node not in bodies:
            raise ValueError('New source or resume node is missing')
        body_lines = [s.strip() for s in bodies[node].splitlines() if s.strip() and not s.lstrip().startswith('//')]
        tags = LINE.findall(body_lines[0]) if body_lines else []
        jumps = JUMP.findall(bodies[node])
        if tags != [line[5:]] or jumps != [item['successorNodeOrFlow']]:
            raise ValueError('New resume line or successor does not match its source')
        if item['successorNodeOrFlow'] not in bodies and item['successorNodeOrFlow'] not in nodes:
            raise ValueError('New successor is not present in this source or the existing manifest')
        by_id[line] = item
        nodes.add(node)
    if not additions:
        output.write_bytes(base_bytes)
    else:
        output.write_text(json.dumps({'entries': entries + additions}, ensure_ascii=False, indent=2) + '\n',
                          encoding='utf-8', newline='\n')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('base', 'additions', 'yarn-root', 'output'):
        parser.add_argument('--' + name, required=True)
    args = parser.parse_args()
    extend(args.base, args.additions, args.yarn_root, args.output)
