using System.Collections.Generic;
using UnityEngine;
namespace Tishe {
public static class SoftFurnishing {
    // A sewn rectangular cushion: flat perimeter seam, curved padded faces.
    public static void Cushion(Transform parent,string name,Vector3 at,Vector3 size,Material material) {
        const int count=40;var v=new List<Vector3>();var uv=new List<Vector2>();var tri=new List<int>();
        for(int side=0;side<2;side++)for(int z=0;z<=count;z++)for(int x=0;x<=count;x++) {
            float u=x/(float)count,w=z/(float)count,px=u*2-1,pz=w*2-1;
            float bulge=Mathf.Pow(Mathf.Max(0,(1-px*px)*(1-pz*pz)),.42f);
            float wrinkles=Mathf.Sin(px*28+pz*8)*.008f*(1-bulge)*bulge;
            v.Add(new Vector3(px*size.x*.5f,(side==0?1:-1)*(size.y*.5f*bulge+wrinkles),pz*size.z*.5f));uv.Add(new Vector2(u,w));
            if(x<count&&z<count){int n=side*(count+1)*(count+1)+z*(count+1)+x;if(side==0)tri.AddRange(new[]{n,n+count+1,n+1,n+1,n+count+1,n+count+2});else tri.AddRange(new[]{n,n+1,n+count+1,n+1,n+count+2,n+count+1});}
        }
        var mesh=new Mesh{name=name};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateTangents();
        var obj=new GameObject(name);obj.transform.SetParent(parent,false);obj.transform.localPosition=at;obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=material;
        var a=size*.5f;ReferenceArt.Rod(obj.transform,new Vector3(-a.x,0,-a.z),new Vector3(a.x,0,-a.z),.008f,material);ReferenceArt.Rod(obj.transform,new Vector3(-a.x,0,a.z),new Vector3(a.x,0,a.z),.008f,material);
    }
}
}
