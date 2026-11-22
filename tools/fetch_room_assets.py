"""Download the selected CC0 room assets, preserving existing provenance records."""
import hashlib
import json
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def get(url):
    request = urllib.request.Request(url, headers={"User-Agent": "TisheStudentProject/1.0"})
    return urllib.request.urlopen(request, timeout=60).read()

manifest = ROOT / 'docs/FURNITURE_SOURCES.json'
records = json.loads(manifest.read_text(encoding='utf-8'))
for asset in ['old_bed_frame', 'wooden_bookshelf_worn']:
    data = json.loads(get('https://api.polyhaven.com/files/' + asset))
    maps = [('Model.fbx', data['fbx']['2k']['fbx']), ('Color.jpg', data['Diffuse']['2k']['jpg']), ('Normal.jpg', data['nor_gl']['2k']['jpg'])]
    folder = ROOT / 'Assets/Resources/Models' / asset
    folder.mkdir(parents=True, exist_ok=True)
    for name, entry in maps:
        path = folder / name
        content = get(entry['url'])
        path.write_bytes(content)
        relative = str(path.relative_to(ROOT))
        records = [r for r in records if r['file'] != relative]
        records.append(dict(asset=asset, file=relative, url=entry['url'], license='CC0', sha256=hashlib.sha256(content).hexdigest()))
    print('Downloaded ' + asset, flush=True)
manifest.write_text(json.dumps(records, indent=2), encoding='utf-8')
