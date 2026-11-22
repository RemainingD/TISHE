using System.Collections.Generic;
using UnityEngine;

namespace Tishe {
// Coordinates for both faces are in metres. Each rigid part has a single mesh with shared materials.
internal sealed class EntranceMesh {
    internal const int Oak=0, Moulding=1, Iron=2, Brass=3, Shadow=4;
    readonly List<Vector3> vertices=new List<Vector3>();
    readonly List<Vector2> uv=new List<Vector2>();
    readonly List<int>[] faces={new List<int>(),new List<int>(),new List<int>(),new List<int>(),new List<int>()};

    int Vertex(Vector3 p, Vector2 tex){int i=vertices.Count;vertices.Add(p);uv.Add(tex);return i;}
    static Vector2 Grain(Vector3 p,bool upright,float shift){
        return upright?new Vector2(p.y*.66f+shift,(p.x+p.z)*.61f+shift*.37f):
            new Vector2(p.x*.66f+shift,(p.y+p.z)*.61f+shift*.37f);
    }
    void Quad(int mat,Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector3 normal,bool upright,float shift){
        int i=vertices.Count;
        foreach(Vector3 p in new[]{a,b,c,d})Vertex(p,Grain(p,upright,shift));
        var t=faces[mat];
        if(Vector3.Dot(Vector3.Cross(b-a,c-a),normal)>0)t.AddRange(new[]{i,i+1,i+2,i,i+2,i+3});
        else t.AddRange(new[]{i,i+2,i+1,i,i+3,i+2});
    }
    internal void Block(int mat,float x,float y,float z,float w,float h,float depth,bool upright=true,float shift=0){
        var center=new Vector3(x,y,z);var size=new Vector3(w,h,depth)*.5f;
        for(int axis=0;axis<3;axis++)for(int sign=-1;sign<=1;sign+=2){
            Vector3 n=Vector3.zero,a=Vector3.zero,b=Vector3.zero;
            n[axis]=sign;a[(axis+1)%3]=size[(axis+1)%3];b[(axis+2)%3]=size[(axis+2)%3];
            Vector3 p=center+n*size[axis];Quad(mat,p-a-b,p+a-b,p+a+b,p-a+b,n,upright,shift);
        }
    }
    // A continuous mitred profile with an optional segmental arch across the top.
    internal void Panel(float x,float bottom,float top,float width,float arch,int face,float plane,bool casing=false) {
        float[] inset=casing?new[]{0f,.008f,.02f,.035f,.052f,.080f,.099f,.114f}:
            new[]{0f,.006f,.015f,.024f,.034f,.044f,.052f,.063f,.082f};
        float[] relief=casing?new[]{0f,.011f,.014f,.033f,.038f,.02f,.017f,.004f}:
            new[]{0f,.009f,.017f,.010f,.002f,-.004f,-.002f,.001f,.014f};
        Vector3[] previous=null;
        for(int ring=0;ring<inset.Length;ring++){
            float d=inset[ring],half=width/2-d,lo=bottom+d,hi=top-d;
            var loop=new List<Vector3>();float z=plane+face*relief[ring];
            loop.Add(new Vector3(x-half,lo,z));loop.Add(new Vector3(x+half,lo,z));
            int steps=arch>0?16:1;
            for(int i=0;i<=steps;i++){float t=1f-2f*i/steps;loop.Add(new Vector3(x+half*t,hi-arch*t*t,z));}
            var current=loop.ToArray();
            if(previous!=null)for(int j=0;j<current.Length;j++){
                int k=(j+1)%current.Length;bool vertical=j==1||k==0;
                Quad(Moulding,previous[j],previous[k],current[k],current[j],Vector3.forward*face,vertical,x+.32f);
            }
            previous=current;
        }
        if(casing)return;
        Vector3 center=new Vector3(x,(top+bottom)/2,previous[0].z);int ci=Vertex(center,Grain(center,true,x));
        int first=vertices.Count;
        foreach(var p in previous)Vertex(p,Grain(p,true,x));
        for(int i=0;i<previous.Length;i++){
            int next=(i+1)%previous.Length;
            faces[Oak].AddRange(face>0?new[]{ci,first+i,first+next}:new[]{ci,first+next,first+i});
        }
    }
    internal void Tube(int mat,Vector3[] path,float[] radius,int sides=16){
        int start=vertices.Count;
        Vector3 axis=(path[path.Length-1]-path[0]).normalized;
        Vector3 reference=Mathf.Abs(axis.y)>.92f?Vector3.right:Vector3.up;
        Vector3 lastNormal=Vector3.zero;
        for(int i=0;i<path.Length;i++){
            Vector3 tangent=(path[Mathf.Min(i+1,path.Length-1)]-path[Mathf.Max(0,i-1)]).normalized;
            Vector3 n=i==0?Vector3.Cross(reference,tangent):Vector3.ProjectOnPlane(lastNormal,tangent);
            if(n.sqrMagnitude<.00001f)n=Vector3.Cross(Vector3.forward,tangent);
            n.Normalize();lastNormal=n;Vector3 v=Vector3.Cross(tangent,n);
            for(int j=0;j<sides;j++){
                float a=2*Mathf.PI*j/sides;
                Vertex(path[i]+radius[i]*(n*Mathf.Cos(a)+v*Mathf.Sin(a)),new Vector2((float)j/sides,i*.19f));
                if(i==0)continue;
                int a0=start+(i-1)*sides+j,b0=start+(i-1)*sides+(j+1)%sides;
                faces[mat].AddRange(new[]{a0,b0,b0+sides,a0,b0+sides,a0+sides});
            }
        }
        for(int j=1;j<sides-1;j++){
            int end=start+(path.Length-1)*sides;
            faces[mat].AddRange(new[]{start,start+j+1,start+j,end,end+j,end+j+1});
        }
    }
    internal void Rod(int mat,Vector3 a,Vector3 b,float r){
        Tube(mat,new[]{a,Vector3.Lerp(a,b,.06f),Vector3.Lerp(a,b,.94f),b},new[]{r*.8f,r,r,r*.8f});
    }
    internal void Fastener(float x,float y,float z,int face,float radius=.008f){
        var p=new Vector3(x,y,z);Rod(Brass,p,p+Vector3.forward*(face*.004f),radius);
        Block(Shadow,x,y,z+face*.0043f,radius*1.18f,radius*.20f,.0006f,false);
    }
    internal void Fit(Transform parent,string name,Material[] palette){
        var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);
        var materials=new List<Material>();int count=0;
        foreach(var f in faces)if(f.Count>0)count++;mesh.subMeshCount=count;
        int sub=0;for(int i=0;i<faces.Length;i++)if(faces[i].Count>0){mesh.SetTriangles(faces[i],sub++);materials.Add(palette[i]);}
        mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
        var go=new GameObject(name);go.transform.SetParent(parent,false);
        go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterials=materials.ToArray();
    }
}
}
