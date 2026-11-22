using System.Collections.Generic;
using UnityEngine;
namespace Tishe {
public class CharacterRig : MonoBehaviour {
    public Transform[] Arms,Knees,Hips,Elbows;public Transform Head;public bool Boy;public SkinnedMeshRenderer Face;
    static Material M(Color c){var m=new Material(Shader.Find("Standard"));m.color=c;m.SetFloat("_Glossiness",.08f);return m;}
    static Transform Joint(string n,Transform parent,Vector3 p){var t=new GameObject(n).transform;t.SetParent(parent,false);t.localPosition=p;return t;}
    static void S(Transform p,string n,Vector3 at,Vector3 scale,Material mat){ReferenceArt.Round(p,n,at,scale,mat);}
    static void Segment(Transform p,string n,float length,float width,Material mat){
        var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();const int rings=13,sides=20;
        for(int j=0;j<=rings;j++)for(int i=0;i<=sides;i++){float t=j/(float)rings,a=i*Mathf.PI*2/sides;float muscle=.86f+.20f*Mathf.Sin(t*Mathf.PI);float folds=n=="Forearm"||n=="Finger"?0:Mathf.Sin(t*42+a*2)*.025f*Mathf.Pow(Mathf.Abs(t-.5f)*2,2);float r=width*.5f*(muscle+folds);vertices.Add(new Vector3(Mathf.Cos(a)*r,.018f-t*(length+.036f),Mathf.Sin(a)*r*.9f));uv.Add(new Vector2(i/(float)sides,t));if(j<rings&&i<sides){int k=j*(sides+1)+i;triangles.AddRange(new[]{k,k+1,k+sides+1,k+1,k+sides+2,k+sides+1});}}
        int endRing=rings*(sides+1);
        for(int i=1;i<sides-1;i++){
            triangles.AddRange(new[]{0,i+1,i,endRing,endRing+i,endRing+i+1});
        }
        var mesh=new Mesh{name=n+" shaped cloth"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateTangents();var obj=new GameObject(n);obj.transform.SetParent(p,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=mat;
    }
    public static CharacterRig Create(bool boy){var root=new GameObject(boy?"Boy / backpack and flashlight":"Keeper / reference silhouette");var rig=root.AddComponent<CharacterRig>();rig.Boy=boy;
        var skin=M(boy?new Color(.62f,.48f,.39f):new Color(.43f,.39f,.33f));var fabric=M(boy?new Color(.23f,.25f,.24f):new Color(.32f,.31f,.26f));var pants=M(new Color(.19f,.21f,.20f));var dark=M(new Color(.035f,.03f,.028f));var hair=M(boy?new Color(.12f,.085f,.065f):new Color(.29f,.3f,.28f));var white=M(new Color(.68f,.68f,.56f));var bag=M(new Color(.27f,.24f,.19f));
        float hip=boy?.66f:.84f,shoulder=boy?1.12f:1.48f,headY=boy?1.42f:1.82f;
        // Tapered torso mesh with an elliptical cross section instead of a block body.
        var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();int rings=24,segments=36;
        for(int j=0;j<rings;j++){float t=j/(float)(rings-1),y=Mathf.Lerp(hip-(boy?.08f:.22f),shoulder,t),radius=Mathf.Lerp(boy?.23f:.285f,boy?.22f:.30f,t)+(boy?.025f:-.038f)*Mathf.Sin(t*Mathf.PI)+Mathf.Sin(t*46)*.003f;for(int i=0;i<=segments;i++){float a=i*Mathf.PI*2/segments;vertices.Add(new Vector3(Mathf.Cos(a)*radius,y,Mathf.Sin(a)*(radius*.62f+.006f*Mathf.Sin(8*a+3*t))+(boy?0:t*t*.10f)));uv.Add(new Vector2(i/(float)segments,t));if(j<rings-1&&i<segments){int n=j*(segments+1)+i;triangles.AddRange(new[]{n,n+segments+1,n+1,n+1,n+segments+1,n+segments+2});}}}
        // Close the shoulders with a sloping yoke leading to the neck opening.
        int lastRing=(rings-1)*(segments+1);
        for(int i=0;i<=segments;i++){
            float angle=i*2*Mathf.PI/segments;
            vertices.Add(new Vector3(Mathf.Cos(angle)*.084f,shoulder+.085f,.068f+Mathf.Sin(angle)*.076f));
            uv.Add(new Vector2(i/(float)segments,1.12f));
            if(i<segments){int n=lastRing+i;triangles.AddRange(new[]{n,n+segments+1,n+1,n+1,n+segments+1,n+segments+2});}
        }
        var mesh=new Mesh{name="Tailored torso"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateTangents();var torso=new GameObject(boy?"Sweater":"Worn shirt");torso.transform.SetParent(root.transform,false);torso.AddComponent<MeshFilter>().sharedMesh=mesh;torso.AddComponent<MeshRenderer>().sharedMaterial=fabric;
        S(root.transform,"Neck",new Vector3(0,shoulder+.105f,boy?.04f:.102f),new Vector3(.156f,.255f,.182f),skin);
        if(!boy){
            var complexion=Resources.Load<Texture2D>("Art/KeeperFace");
            skin.mainTexture=complexion;skin.mainTextureScale=new Vector2(.09f,.12f);
            skin.mainTextureOffset=new Vector2(.54f,.72f);skin.color=new Color(.78f,.76f,.72f);
            var lapel=ReferenceArt.Surface("Linen",new Color(.42f,.40f,.35f),2);
            foreach(float side in new[]{-1f,1f}){
                var collar=ReferenceArt.Round(root.transform,"Faded turned collar",new Vector3(side*.088f,shoulder+.028f,.156f),new Vector3(.149f,.155f,.049f),lapel);
                collar.transform.localRotation=Quaternion.Euler(21,side*28,side*-29);
                var fold=ReferenceArt.Round(root.transform,"Collar fold",new Vector3(side*.109f,shoulder+.051f,.094f),new Vector3(.157f,.092f,.155f),lapel);
                fold.transform.localRotation=Quaternion.Euler(14,side*22,side*19);
            }
            foreach(float side in new[]{-1f,1f})
                S(root.transform,"Coat shoulder",new Vector3(side*.263f,shoulder-.035f,.07f),new Vector3(.19f,.17f,.23f),fabric);
        }
        rig.Head=Joint("Head joint",root.transform,new Vector3(0,headY,boy?.025f:.158f));
        if(boy){S(rig.Head,"Head",Vector3.zero,boy?new Vector3(.4f,.46f,.36f):new Vector3(.31f,.48f,.32f),skin);
        foreach(float side in new[]{-1f,1f}){S(rig.Head,"Ear",new Vector3(side*(boy?.205f:.175f),-.015f,0),new Vector3(.065f,.12f,.045f),skin);S(rig.Head,"Eye socket",new Vector3(side*.083f,.045f,.145f),new Vector3(boy?.14f:.10f,boy?.095f:.066f,.045f),dark);S(rig.Head,"Eye white",new Vector3(side*.083f,.045f,.169f),new Vector3(boy?.108f:.066f,boy?.076f:.033f,.025f),white);S(rig.Head,"Pupil",new Vector3(side*.083f,.045f,.185f),new Vector3(boy?.042f:.027f,boy?.051f:.032f,.014f),dark);
            var brow=ReferenceArt.Round(rig.Head,"Brow",new Vector3(side*.083f,.112f,.162f),new Vector3(.135f,.032f,.035f),boy?hair:skin);brow.transform.localRotation=Quaternion.Euler(0,0,side*(boy?18:-20));S(rig.Head,"Cheekbone",new Vector3(side*.10f,-.045f,.13f),new Vector3(.12f,.07f,.08f),skin);}
        S(rig.Head,"Nose",new Vector3(0,-.025f,.19f),new Vector3(.07f,.115f,.105f),skin);S(rig.Head,"Open mouth",new Vector3(0,-.12f,.149f),new Vector3(boy?.09f:.10f,boy?.045f:.045f,.025f),dark);
        }else rig.Face=KeeperHead.Build(rig.Head,skin);
        if(boy){for(int i=0;i<22;i++){float a=i*2.399f;float x=Mathf.Cos(a)*(.06f+i*.006f),z=Mathf.Sin(a)*(.06f+i*.005f);var lockHair=ReferenceArt.Round(rig.Head,"Tousled hair",new Vector3(x,.19f,z-.015f),new Vector3(.12f,.15f,.23f),hair);lockHair.transform.localRotation=Quaternion.Euler(20+15*Mathf.Sin(a),i*23,25*Mathf.Cos(a));}
            S(root.transform,"Backpack",new Vector3(0,.99f,-.23f),new Vector3(.43f,.5f,.2f),bag);S(root.transform,"Backpack pocket",new Vector3(0,.89f,-.34f),new Vector3(.34f,.19f,.06f),bag);foreach(float x in new[]{-.16f,.16f})ReferenceArt.Rod(root.transform,new Vector3(x,.72f,.15f),new Vector3(x,1.18f,.12f),.025f,bag);
        }else {for(int i=0;i<5;i++)S(root.transform,"Shirt button",new Vector3(0,hip+.13f+i*.11f,.20f),Vector3.one*.025f,dark);}
        rig.Arms=new Transform[2];rig.Elbows=new Transform[2];rig.Hips=new Transform[2];rig.Knees=new Transform[2];
        for(int i=0;i<2;i++){float side=i==0?-1:1;rig.Arms[i]=Joint("Shoulder",root.transform,new Vector3(side*(boy?.24f:.294f),shoulder-.034f,boy?.02f:.058f));float upper=boy?.28f:.36f,lower=boy?.27f:.39f;Segment(rig.Arms[i],"Sleeve",upper,boy?.13f:.19f,fabric);rig.Elbows[i]=Joint("Elbow",rig.Arms[i],new Vector3(0,-upper,0));Segment(rig.Elbows[i],boy?"Cuff":"Forearm",lower,boy?.11f:.095f,boy?fabric:skin);S(rig.Elbows[i],"Hand",new Vector3(0,-lower-.045f,0),new Vector3(.10f,.13f,.065f),skin);for(int j=0;j<4;j++){
                float length=(boy?.064f:.096f)*(1-.15f*Mathf.Abs(j-1));
                var finger=Joint("Knuckle",rig.Elbows[i],new Vector3(side*(-.033f+j*.022f),-lower-.086f,.001f));
                finger.localRotation=Quaternion.Euler(boy?-38:-14,0,(j-1.5f)*3*side);
                Segment(finger,"Finger",length*.55f,boy?.026f:.029f,skin);
                var tip=Joint("Second knuckle",finger,new Vector3(0,-length*.55f,0));
                tip.localRotation=Quaternion.Euler(boy?-52:-28,0,0);
                S(tip,"Fingertip",Vector3.down*length*.23f,new Vector3(.024f,length*.61f,.022f),skin);
            }
            var thumb=Joint("Thumb base",rig.Elbows[i],new Vector3(-side*.037f,-lower-.031f,.013f));
            thumb.localRotation=Quaternion.Euler(-41,0,-side*29);
            S(thumb,"Thumb",new Vector3(0,-.025f,0),new Vector3(.036f,.075f,.038f),skin);
            rig.Hips[i]=Joint("Hip",root.transform,new Vector3(side*(boy?.115f:.16f),hip,0));float leg=boy?.29f:.38f;Segment(rig.Hips[i],"Trouser",leg,boy?.17f:.19f,pants);rig.Knees[i]=Joint("Knee",rig.Hips[i],new Vector3(0,-leg,0));Segment(rig.Knees[i],"Shin",leg,boy?.14f:.13f,pants);S(rig.Knees[i],boy?"Sneaker":"Bare foot",new Vector3(0,-leg,.085f),new Vector3(.16f,.10f,.28f),boy?bag:skin);
        }
        Transform hand=rig.Elbows[1];float handY=boy?-.32f:-.45f;
        if(boy){var torch=ReferenceArt.Round(hand,"Handheld flashlight",new Vector3(0,handY,.05f),new Vector3(.09f,.14f,.09f),dark,PrimitiveType.Cylinder);torch.transform.localRotation=Quaternion.Euler(90,0,0);S(hand,"Torch lens",new Vector3(0,handY,.2f),new Vector3(.085f,.085f,.02f),white);}
        else {ReferenceArt.Rod(hand,new Vector3(0,handY,0),new Vector3(0,handY-.3f,0),.025f,dark);var pan=ReferenceArt.Round(hand,"Rusty frying pan",new Vector3(0,handY-.52f,0),new Vector3(.42f,.035f,.42f),bag,PrimitiveType.Cylinder);pan.transform.localRotation=Quaternion.Euler(85,0,0);}
        CharacterDetail.Apply(rig);return rig;
    }
    public void RunPose(float t,float strength=1){float swing=Mathf.Sin(t*10)*strength;for(int i=0;i<2;i++){float s=i==0?swing:-swing;Hips[i].localRotation=Quaternion.Euler(s*38,0,0);Knees[i].localRotation=Quaternion.Euler(Mathf.Max(0,-s)*70,0,0);Arms[i].localRotation=Quaternion.Euler(-s*48,0,i==0?-8:8);Elbows[i].localRotation=Quaternion.Euler(-65,0,0);}if(!Boy){
            Arms[0].localRotation*=Quaternion.Euler(-21,9,-5);
            Arms[1].localRotation*=Quaternion.Euler(12,-5,8);
            Elbows[0].localRotation=Quaternion.Euler(-24+9*swing,0,0);
            Elbows[1].localRotation=Quaternion.Euler(-38,0,0);
        }
        Head.localRotation=Boy?Quaternion.Euler(-6,Mathf.Sin(t*2)*3,0):Quaternion.Euler(8+Mathf.Sin(t*3)*2,Mathf.Sin(t*1.6f)*7,-6+Mathf.Sin(t)*2);}
    public void SitPose(float seated){for(int i=0;i<2;i++){Hips[i].localRotation=Quaternion.Euler(-75*seated,0,0);Knees[i].localRotation=Quaternion.Euler(85*seated,0,0);Arms[i].localRotation=Quaternion.Euler(-35*seated,0,i==0?-8:8);Elbows[i].localRotation=Quaternion.Euler(-20*seated,0,0);}}
}
}


