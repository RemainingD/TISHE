using System.Collections.Generic;
using UnityEngine;
namespace Tishe {
public static class BeveledGeometry {
    static readonly Dictionary<Vector3,Mesh> cache=new Dictionary<Vector3,Mesh>();
    public static void Apply(GameObject obj,Vector3 size){float r=Mathf.Min(.035f,Mathf.Min(size.x,Mathf.Min(size.y,size.z))*.18f);if(r<.008f||size.magnitude>12)return;if(!cache.TryGetValue(size,out var mesh)){
        var v=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var tri=new List<int>();Vector3 half=size*.5f,inner=half-Vector3.one*r;
        for(int axis=0;axis<3;axis++)for(int sign=-1;sign<=1;sign+=2){int u=(axis+1)%3,w=(axis+2)%3;int start=v.Count;float[] aa={-half[u],-inner[u],0,inner[u],half[u]},bb={-half[w],-inner[w],0,inner[w],half[w]};for(int j=0;j<5;j++)for(int i=0;i<5;i++){Vector3 p=Vector3.zero;p[axis]=sign*half[axis];p[u]=aa[i];p[w]=bb[j];var q=new Vector3(Mathf.Clamp(p.x,-inner.x,inner.x),Mathf.Clamp(p.y,-inner.y,inner.y),Mathf.Clamp(p.z,-inner.z,inner.z));var normal=(p-q).normalized;var rounded=q+normal*r;v.Add(new Vector3(rounded.x/size.x,rounded.y/size.y,rounded.z/size.z));normals.Add(new Vector3(normal.x*size.x,normal.y*size.y,normal.z*size.z).normalized);uv.Add(new Vector2((p[u]/size[u]+.5f),(p[w]/size[w]+.5f)));if(i<4&&j<4){int n=start+j*5+i;if(sign>0)tri.AddRange(new[]{n,n+1,n+5,n+1,n+6,n+5});else tri.AddRange(new[]{n,n+5,n+1,n+1,n+5,n+6});}}}
        mesh=new Mesh{name="Beveled joinery"};mesh.SetVertices(v);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetTriangles(tri,0);mesh.RecalculateTangents();mesh.RecalculateBounds();cache[size]=mesh;}
        obj.GetComponent<MeshFilter>().sharedMesh=mesh;
    }
}
}
