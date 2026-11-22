using System.Collections.Generic;
using UnityEngine;

namespace Tishe {
public static class KeeperHead {
    // Half-widths sampled along the outline of the portrait, from crown to chin.
    static readonly float[] widths={.014f,.161f,.210f,.244f,.264f,.274f,.276f,.275f,.270f,.268f,.266f,.260f,.247f,.230f,.216f,.207f,.191f,.166f,.138f,.096f,.014f};

    static float Relief(float x,float y,float cx,float cy,float sx,float sy){
        float dx=(x-cx)/sx,dy=(y-cy)/sy;
        return Mathf.Exp(-.5f*(dx*dx+dy*dy));
    }

    public static SkinnedMeshRenderer Build(Transform joint,Material skin){
        const int rows=72,columns=96;
        var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
        for(int row=0;row<=rows;row++){
            float t=row/(float)rows;
            float index=t*(widths.Length-1);
            int i=Mathf.Min(Mathf.FloorToInt(index),widths.Length-2);
            float width=Mathf.Lerp(widths[i],widths[i+1],index-i);
            float depth=.13f*Mathf.Sqrt(Mathf.Max(.0001f,1-Mathf.Pow(2*t-1,2)));
            for(int col=0;col<=columns;col++){
                float angle=2*Mathf.PI*col/columns-Mathf.PI;
                float sine=Mathf.Sin(angle),cosine=Mathf.Cos(angle);
                float u=.5f-sine*width,v=Mathf.Lerp(.018f,.973f,t);
                float x=sine*width*.55f;
                // Align the cheekbones, eye hollows, muzzle and bent nose with the diffuse map.
                float cheek=Mathf.Abs(u-.5f);
                float shape=.014f*Relief(cheek,v,.135f,.43f,.064f,.032f)
                           -.014f*Relief(cheek,v,.133f,.48f,.045f,.032f)
                           +.017f*Relief(cheek,v,.16f,.565f,.054f,.046f)
                           -.016f*Relief(cheek,v,.165f,.68f,.042f,.070f)
                           +.030f*Relief(u,v,.50f,.53f,.029f,.081f)
                           +.043f*Relief(u,v,.497f,.627f,.034f,.030f)
                           +.012f*Relief(cheek,v,.043f,.632f,.020f,.018f)
                           +.012f*Relief(u,v,.50f,.745f,.080f,.04f)
                           -.008f*Relief(u,v,.50f,.773f,.065f,.012f)
                           +.020f*Relief(u,v,.50f,.875f,.083f,.042f);
                float z=cosine*depth+Mathf.Max(0,cosine)*shape-.012f*(1-t);
                vertices.Add(new Vector3(x,(.5f-t)*.445f,z));
                // Carry temple colour around to the scalp on the back of the head.
                float back=Mathf.SmoothStep(0,1,Mathf.Clamp01(-cosine*2.6f));
                uv.Add(new Vector2(Mathf.Lerp(u,.50f+.11f*sine,back),1-Mathf.Lerp(v,.14f+.13f*t,back)));
                if(row<rows&&col<columns){
                    int n=row*(columns+1)+col;
                    triangles.AddRange(new[]{n,n+columns+1,n+1,n+1,n+columns+1,n+columns+2});
                }
            }
        }
        var mesh=new Mesh{name="Keeper facial anatomy"};
        mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);
        mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
        var head=new GameObject("Head");head.transform.SetParent(joint,false);
        var jaw=new Vector3[vertices.Count];var skinning=new BoneWeight[jaw.Length];
        for(int i=0;i<jaw.Length;i++){
            Vector3 v=vertices[i];
            float lower=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.098f,-.153f,v.y));
            float front=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.03f,.085f,v.z));
            jaw[i]=new Vector3(0,-.026f,-.012f)*lower*front;
            skinning[i]=new BoneWeight{boneIndex0=0,weight0=1};
        }
        mesh.boneWeights=skinning;mesh.bindposes=new[]{Matrix4x4.identity};
        mesh.AddBlendShapeFrame("Speech jaw",100,jaw,null,null);
        var portrait=head.AddComponent<SkinnedMeshRenderer>();
        portrait.rootBone=joint;portrait.bones=new[]{joint};portrait.sharedMesh=mesh;
        portrait.localBounds=new Bounds(Vector3.zero,new Vector3(.5f,.58f,.42f));
        var face=new Material(Shader.Find("Standard")){name="Keeper aged skin",mainTexture=Resources.Load<Texture2D>("Art/KeeperFace")};
        face.color=new Color(.82f,.83f,.79f);face.SetFloat("_Glossiness",.12f);
        portrait.sharedMaterial=face;
        foreach(float side in new[]{-1f,1f}){
            var ear=ReferenceArt.Round(joint,"Ear",new Vector3(side*.147f,-.014f,-.017f),new Vector3(.044f,.094f,.044f),skin);
            var earMesh=Object.Instantiate(ear.GetComponent<MeshFilter>().sharedMesh);
            var earUV=earMesh.uv;var points=earMesh.vertices;
            for(int k=0;k<earUV.Length;k++){
                earUV[k]=new Vector2((side>0?.20f:.79f)+points[k].x*.048f,.444f+points[k].y*.18f);
            }
            earMesh.uv=earUV;
            ear.GetComponent<MeshFilter>().sharedMesh=earMesh;
            ear.GetComponent<Renderer>().sharedMaterial=face;
            ear.transform.localRotation=Quaternion.Euler(-12,0,side*-9);
        }
        return portrait;
    }
}
}
