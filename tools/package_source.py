"""Package source without caches, builds or user settings."""
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
root = Path(__file__).resolve().parents[1]
paths = [root / name for name in ('README.md', 'AGENTS.md', 'PLAY_WINDOWS.bat', '.gitignore')]
for folder in ('Assets', 'Packages', 'ProjectSettings', 'Tests', 'docs', 'tools'):
    paths.extend((root / folder).rglob('*'))
with ZipFile(root / 'TISHE_Unity_Source.zip', 'w', ZIP_DEFLATED) as archive:
    for path in sorted(paths):
        if path.is_file() and '__pycache__' not in path.parts and path.name != 'polyhaven-catalog.json':
            archive.write(path, path.relative_to(root))
with ZipFile(root / 'TISHE_Unity_Source.zip') as archive:
    assert archive.testzip() is None
    print(f'Source archive verified: {len(archive.namelist())} files')
