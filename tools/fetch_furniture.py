import json, hashlib, urllib.request
from pathlib import Path
root=Path(__file__).resolve().parents[1]
def get(url):
    return urllib.request.urlopen(urllib.request.Request(url,headers={'User-Agent':'TisheStudentProject/1.0'}),timeout=60).read()
records=[]
for asset in ['Rockingchair_01','WoodenChair_01','GothicCabinet_01','ArmChair_01','CoffeeTable_01']:
    data=json.loads(get('https://api.polyhaven.com/files/'+asset))
    entries=[('Model.fbx',data['fbx']['2k']['fbx'])]
    color=next(v for k,v in data.items() if k.lower() in ('diffuse','diff','albedo'))
    entries += [('Color.jpg',color['2k']['jpg']),('Normal.jpg',data['nor_gl']['2k']['jpg'])]
    folder=root/'Assets/Resources/Models'/asset;folder.mkdir(parents=True,exist_ok=True)
    for name,entry in entries:
        path=folder/name
        content=get(entry['url']);path.write_bytes(content)
        records.append(dict(asset=asset,file=str(path.relative_to(root)),url=entry['url'],license='CC0',sha256=hashlib.sha256(content).hexdigest()))
    print(asset,flush=True)
(root/'docs/FURNITURE_SOURCES.json').write_text(json.dumps(records,indent=2),encoding='utf-8')
