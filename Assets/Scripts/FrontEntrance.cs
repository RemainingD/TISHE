using UnityEngine;
using static Tishe.EntranceMesh;

namespace Tishe {
public sealed class FrontEntrance : MonoBehaviour {
    public const float BoltTravel=.14f;
    const float Width=1.16f,Height=2.23f,Face=.044f;
    public Transform hinge;
    public Transform handle;
    public void SetOpen(float seconds){
        float press=Mathf.SmoothStep(0,1,Mathf.Clamp01(seconds/.23f))*(1-Mathf.SmoothStep(0,1,Mathf.Clamp01((seconds-.4f)/.32f)));
        handle.localRotation=Quaternion.Euler(0,0,press*19);
        var target=Quaternion.Euler(0,-87*Mathf.SmoothStep(0,1,Mathf.Clamp01((seconds-.17f)/.85f)),0);
        if(hinge.localRotation!=target){
            hinge.localRotation=target;
            // Escape runs on unscaled time while the physics simulation is paused.
            Physics.SyncTransforms();
        }
    }
    static Transform Child(Transform parent,string name,Vector3 position){
        var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;
    }
    static Material Metal(string name,Color color,float metallic,float smoothness){
        var mat=new Material(Shader.Find("Standard")){name=name,color=color};
        mat.SetFloat("_Metallic",metallic);mat.SetFloat("_Glossiness",smoothness);return mat;
    }
    static Vector3 P(float x,float y,float z)=>new Vector3(x,y,z);
    static void BoxColliderAt(Transform t,Vector3 center,Vector3 size){
        var col=t.gameObject.AddComponent<BoxCollider>();col.center=center;col.size=size;
    }
    static void Masonry(Transform root,string name,Vector3 center,Vector3 size,Material mat){
        var block=GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name=name;block.transform.SetParent(root,false);block.transform.localPosition=center;block.transform.localScale=size;
        block.GetComponent<MeshRenderer>().sharedMaterial=mat;
    }
    public static void Build(GameController game,Transform parent,Material wall){
        var root=Child(parent,"Oak entrance and porch passage",P(0,0,5.5f*HouseMap.Cell));
        var door=root.gameObject.AddComponent<FrontEntrance>();game.Entrance=door;
        var oak=ReferenceArt.Surface("Wood",new Color(.57f,.43f,.32f));oak.name="Oiled oak";oak.SetFloat("_Glossiness",.32f);
        var trim=new Material(oak){name="Waxed oak moulding",color=new Color(.64f,.50f,.37f)};
        trim.SetFloat("_Glossiness",.38f);
        var iron=Metal("Forged iron",new Color(.28f,.295f,.28f),.59f,.4f);
        var brass=Metal("Antique brass",new Color(.70f,.50f,.26f),.66f,.55f);
        var shadow=Metal("Keyway and screw slots",new Color(.047f,.042f,.035f),.12f,.18f);
        var palette=new[]{oak,trim,iron,brass,shadow};
        const float opening=1.34f,head=2.34f,depth=HouseMap.Cell;
        float side=(HouseMap.Cell-opening)*.5f,wallTop=HouseMap.Height-.24f;
        foreach(int sign in new[]{-1,1})
            Masonry(root,"Plastered entrance reveal",P(sign*(opening+side)*.5f,head*.5f,depth*.5f),P(side,head,depth),wall);
        Masonry(root,"Entrance lintel",P(0,(head+wallTop)*.5f,depth*.5f),P(HouseMap.Cell,wallTop-head,depth),wall);
        var stone=ReferenceArt.Surface("Stone",new Color(.5f,.53f,.53f));stone.name="Porch stonework";
        foreach(int sign in new[]{-1,1})
            Masonry(root,"Porch stone jamb",P(sign*(1.70f+opening*.5f)*.5f,head*.5f,depth+.03f),P(1.70f-opening*.5f,head,.12f),stone);
        Masonry(root,"Porch stone lintel",P(0,(head+4.10f)*.5f,depth+.03f),P(3.40f,4.10f-head,.12f),stone);

        var frame=new EntranceMesh();
        foreach(int sign in new[]{-1,1})
            frame.Block(Oak,sign*.632f,Height/2,.073f,.084f,Height,.20f,true,sign*.21f);
        frame.Block(Oak,0,Height+.046f,.073f,opening,.082f,.20f,false);
        frame.Panel(0,-.14f,2.38f,1.47f,0,-1,-.032f,true);
        frame.Panel(0,-.14f,2.44f,1.56f,0,1,depth+.10f,true);
        frame.Block(Oak,0,2.39f,-.045f,1.52f,.046f,.10f,false);
        frame.Block(Brass,0,.011f,.072f,1.19f,.014f,.19f,false);
        door.hinge=Child(root,"Entrance hinge",P(-Width/2,.018f,.050f));
        var leaf=new EntranceMesh();
        leaf.Block(Oak,Width/2,Height/2,0,Width,Height,.066f,true);
        foreach(float x in new[]{.081f,Width-.081f,.58f})
            leaf.Block(Oak,x,Height/2,0,x==.58f?.092f:.16f,Height,.081f,true,x);
        foreach(var r in new[]{new Vector2(.088f,.176f),new Vector2(.91f,.16f),new Vector2(2.167f,.126f)})
            leaf.Block(Oak,Width/2,r.x,0,Width-.32f,r.y,.082f,false,r.x*.37f);
        foreach(int face in new[]{-1,1}){
            foreach(float x in new[]{.317f,.843f}){
                leaf.Panel(x,.192f,.825f,.442f,.0f,face,face*Face);
                leaf.Panel(x,1.005f,2.096f,.442f,.078f,face,face*Face);
            }
            LockPlate(leaf,face);
        }
        // Three hinge knuckles turn on the same vertical line as the slab.
        foreach(float y in new[]{.30f,1.21f,1.98f}){
            leaf.Block(Iron,.028f,y,.044f,.061f,.105f,.009f);
            leaf.Rod(Brass,P(0,y-.068f,.049f),P(0,y+.068f,.049f),.014f);
        }
        for(int i=0;i<5;i++){
            float y=.41f+i*.345f;
            // Leave the lever's travel clear.
            if(i>=2)y+=.13f;
            float z=-.070f;
            leaf.Block(Iron,1.007f,y,-.048f,.26f,.082f,.015f,false);
            foreach(float x in new[]{.925f,1.064f}){
                leaf.Block(Iron,x,y,-.057f,.034f,.046f,.021f);
                leaf.Rod(Brass,P(x-.017f,y,z),P(x+.017f,y,z),.023f);
                leaf.Fastener(x,y+.031f,-.064f,-1,.005f);
                leaf.Fastener(x,y-.031f,-.064f,-1,.005f);
            }
            float jambX=.635f;
            frame.Block(Iron,jambX,y+.018f,-.010f,.085f,.094f,.030f);
            frame.Rod(Brass,P(jambX-.022f,y+.018f,z+.050f),P(jambX+.022f,y+.018f,z+.050f),.025f);
            frame.Fastener(jambX+.031f,y+.049f,-.028f,-1,.006f);
            var moving=Child(door.hinge,"Retracting barrel bolt "+(i+1),P(1.025f,y,z));
            game.ExitBolts[i]=moving;
            var slider=new EntranceMesh();
            slider.Rod(Iron,P(-.145f,0,0),P(.217f,0,0),.014f);
            slider.Rod(Brass,P(-.042f,0,-.002f),P(-.042f,0,-.042f),.009f);
            slider.Rod(Brass,P(-.054f,0,-.042f),P(-.030f,0,-.042f),.012f);
            slider.Fit(moving,"Sliding pin "+(i+1),palette);
        }
        // Both handles turn on the same spindle. The keeper side also has a bronze ring knocker.
        door.handle=Child(door.hinge,"Lever spindle",P(1.055f,1.035f,0));
        var levers=new EntranceMesh();
        foreach(int face in new[]{-1,1}){
            levers.Tube(Brass,new[]{P(0,0,face*.055f),P(0,0,face*.091f),P(-.009f,.002f,face*.111f),
                P(-.026f,.004f,face*.122f),P(-.063f,.007f,face*.120f),P(-.102f,.015f,face*.114f),
                P(-.138f,.021f,face*.104f),P(-.153f,.019f,face*.09f)},new[]{.015f,.016f,.014f,.012f,.014f,.0175f,.014f,.010f});
        }
        levers.Fit(door.handle,"Cast brass levers",palette);
        Vector3 mount=P(.58f,1.70f,.075f);
        leaf.Rod(Brass,mount-Vector3.forward*.031f,mount,.041f);
        var ring=new Vector3[33];var radii=new float[ring.Length];
        for(int i=0;i<ring.Length;i++){float a=(.04f+1.92f*i/(ring.Length-1))*Mathf.PI;
            ring[i]=mount+P(.068f*Mathf.Sin(a),.090f*(Mathf.Cos(a)-1),.026f-.019f*Mathf.Cos(a));radii[i]=.009f-.002f*Mathf.Cos(a);}
        leaf.Tube(Brass,ring,radii);
        leaf.Rod(Brass,P(.58f,1.52f,.042f),P(.58f,1.52f,.088f),.025f);
        leaf.Fit(door.hinge,"Joined oak leaf and fittings",palette);frame.Fit(root,"Oak casing and strikes",palette);
        BoxColliderAt(door.hinge,P(Width/2,Height/2,0),P(Width,Height,.09f));
        var item=door.hinge.gameObject.AddComponent<GameItem>();item.kind="exit";item.title="Үйден шығу";
    }
    static void LockPlate(EntranceMesh leaf,int face){
        float x=1.055f;
        // A turned rosette, a separate key escutcheon and a recessed keyway.
        foreach(float y in new[]{1.035f,.937f}){
            leaf.Tube(Brass,new[]{P(x,y,face*.041f),P(x,y,face*.046f),P(x,y,face*.050f),P(x,y,face*.054f)},
                new[]{.036f,.041f,.037f,.029f},32);
            foreach(float dx in new[]{-.022f,.022f})leaf.Fastener(x+dx,y,face*.054f,face,.004f);
        }
        leaf.Rod(Shadow,P(x,.942f,face*.054f),P(x,.942f,face*.056f),.007f);
        leaf.Block(Shadow,x,.933f,face*.056f,.008f,.016f,.001f);
    }
}
}
