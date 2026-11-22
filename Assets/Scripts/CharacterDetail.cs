using System.Collections.Generic;
using UnityEngine;
namespace Tishe {
public static class CharacterDetail {
    public static void Apply(CharacterRig rig){bool boy=rig.Boy;if(boy){var head=rig.Head.Find("Head");var v=new List<Vector3>();var uv=new List<Vector2>();var tr=new List<int>();const int rows=32,cols=48;
        for(int r=0;r<=rows;r++)for(int c=0;c<=cols;c++){float phi=r*Mathf.PI/rows,a=c*Mathf.PI*2/cols,y=Mathf.Cos(phi),rad=Mathf.Sin(phi);float jaw=Mathf.Lerp(.66f,1,Mathf.InverseLerp(-1,.1f,y));float x=Mathf.Cos(a)*rad*.5f*jaw,z=Mathf.Sin(a)*rad*.5f;float front=Mathf.Clamp01(Mathf.Sin(a));z-=front*Mathf.Exp(-Mathf.Pow((y-.16f)*7,2))*.045f;z+=front*Mathf.Exp(-Mathf.Pow((y+.42f)*5,2))*.025f;v.Add(new Vector3(x,y*.5f,z));uv.Add(new Vector2(c/(float)cols,r/(float)rows));if(r<rows&&c<cols){int i=r*(cols+1)+c;tr.AddRange(new[]{i,i+1,i+cols+1,i+1,i+cols+2,i+cols+1});}}
        var m=new Mesh{name=boy?"Boy facial sculpt":"Keeper facial sculpt"};m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(tr,0);m.RecalculateNormals();head.GetComponent<MeshFilter>().sharedMesh=m;
        // Keep the face continuous: raised sphere cheeks and jaw pieces read as separate beads.
        foreach(var part in rig.Head.GetComponentsInChildren<Renderer>())if(part.name=="Cheekbone"||part.name=="Jaw")part.enabled=false;
        foreach(var part in rig.Head.GetComponentsInChildren<Transform>())if(part.name=="Eye socket"||part.name=="Eye white"||part.name=="Pupil"||part.name=="Brow")part.localPosition+=Vector3.back*(boy?.01f:.027f);
        var skin=head.GetComponent<Renderer>().sharedMaterial;var tex=new Texture2D(128,128);var rng=new System.Random(boy?4:8);var pixels=new Color[16384];for(int i=0;i<pixels.Length;i++){float n=.82f+(float)rng.NextDouble()*.18f;pixels[i]=new Color(n,n*.96f,n*.92f);}tex.SetPixels(pixels);tex.Apply();skin.mainTexture=tex;skin.SetFloat("_Glossiness",.24f);
        var iris=new Material(Shader.Find("Standard"));iris.color=boy?new Color(.24f,.17f,.09f):new Color(.32f,.36f,.22f);
        foreach(float side in new[]{-1f,1f}){ReferenceArt.Round(rig.Head,"Iris ring",new Vector3(side*.083f,.045f,boy?.172f:.155f),new Vector3(boy?.058f:.04f,boy?.065f:.036f,.012f),iris);ReferenceArt.Round(rig.Head,"Lower eyelid",new Vector3(side*.083f,boy?.004f:.023f,boy?.156f:.139f),new Vector3(.095f,.018f,.03f),skin);}
        }
        // Cloth weave is deterministic and small; no large textures per character.
        var cloth=new Texture2D(64,64);var cp=new Color[4096];for(int y=0;y<64;y++)for(int x=0;x<64;x++){float n=.8f+((x+y)%2)*.12f+Mathf.PerlinNoise(x*.16f,y*.16f)*.08f;cp[y*64+x]=new Color(n,n,n);}cloth.SetPixels(cp);cloth.Apply();
        foreach(var renderer in rig.GetComponentsInChildren<Renderer>()){if(renderer.name.Contains("shirt")||renderer.name.Contains("Sweater")||renderer.name.Contains("Trouser")||renderer.name.Contains("Sleeve")){renderer.sharedMaterial.mainTexture=Resources.Load<Texture2D>("Art/LinenColor")??cloth;renderer.sharedMaterial.SetTexture("_BumpMap",Resources.Load<Texture2D>("Art/LinenNormal"));renderer.sharedMaterial.EnableKeyword("_NORMALMAP");renderer.sharedMaterial.SetFloat("_BumpScale",.2f);renderer.sharedMaterial.mainTextureScale=Vector2.one*3;}}
    }
}
}
