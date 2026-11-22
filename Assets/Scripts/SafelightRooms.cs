using UnityEngine;
using static Tishe.ReferenceArt;

namespace Tishe {
public sealed class SafelightRooms : MonoBehaviour {
    public Light[] lamps;
    float inspectionTime;
    float creakAt=float.NegativeInfinity;
    internal bool HeardDarkroomCreak=>!float.IsNegativeInfinity(creakAt);
    static Vector3 V(float x,float y,float z)=>new Vector3(x,y,z);
    static Transform Group(Transform p,string name,Vector3 at,float yaw=0){
        var t=new GameObject(name).transform;t.SetParent(p,false);t.localPosition=at;t.localRotation=Quaternion.Euler(0,yaw,0);return t;
    }
    static Material Finish(string name,Color c,float gloss,float metal=0){
        var m=new Material(Shader.Find("Standard")){name=name,color=c};m.SetFloat("_Glossiness",gloss);m.SetFloat("_Metallic",metal);return m;
    }
    struct Materials {
        public Material iron,brass,red,wood,enamel,paper,water;
    }
    public static void Build(Transform parent){
        var root=Group(parent,"Safelights and developing room",Vector3.zero);
        var kit=new Materials{
            iron=Finish("Blackened lamp guards",new Color(.115f,.12f,.125f),.39f,.54f),
            brass=Finish("Oxidised brass fittings",new Color(.47f,.33f,.17f),.39f,.47f),
            red=Finish("Ruby ribbed glass",new Color(.53f,.035f,.018f),.53f),
            wood=Surface("Wood",new Color(.42f,.36f,.28f)),enamel=Finish("Chipped enamel",new Color(.46f,.47f,.41f),.46f,.18f),
            water=Finish("Developing bath",new Color(.10f,.085f,.077f),.67f),paper=Finish("Fibre paper",new Color(.77f,.72f,.60f),.21f)
        };
        kit.red.EnableKeyword("_EMISSION");kit.red.SetColor("_EmissionColor",new Color(1.25f,.032f,.009f));
        var effect=root.gameObject.AddComponent<SafelightRooms>();
        var lab=Group(root,"Photographic bench",V(28.68f,-3.6f,9.54f),-90);
        effect.lamps=new[]{
            Fixture(root,"Exit lamp",V(0,2.87f,13.165f),180,kit),
            Fixture(root,"Passage lamp",V(.52f,2.43f,6.035f),0,kit),
            Fixture(root,"Stair lamp",V(10.77f,4.02f,.1f),-90,kit),
            Fixture(lab,"Darkroom safelight",V(-.34f,2.59f,.04f),0,kit)
        };
        Bench(lab,kit);
    }
    static Light Fixture(Transform p,string name,Vector3 at,float yaw,Materials k){
        var t=Group(p,name,at,yaw);
        Box(t,"Cast mounting plate",V(0,0,.037f),V(.44f,.28f,.08f),k.iron);
        Round(t,"Ruby lens",V(0,0,.092f),V(.35f,.193f,.19f),k.red);
        for(int i=-3;i<=3;i++){
            float x=i*.043f,bulge=Mathf.Sqrt(1-x*x/.034f);
            var a=V(x,-.121f*bulge,.094f);var b=V(x,-.078f*bulge,.164f);var c=V(x,.078f*bulge,.164f);
            Rod(t,a,b,.0075f,k.iron);Rod(t,b,c,.0075f,k.iron);Rod(t,c,V(x,.121f*bulge,.094f),.0075f,k.iron);
        }
        foreach(int side in new[]{-1,1})Round(t,"Mount screw",V(side*.19f,0,.081f),V(.023f,.023f,.012f),k.brass);
        Rod(t,V(.212f,0,.039f),V(.48f,0,.039f),.017f,k.iron);
        Rod(t,V(.48f,0,.039f),V(.48f,.49f,.039f),.017f,k.iron);
        Box(t,"Conduit box",V(.48f,.51f,.039f),V(.11f,.125f,.068f),k.iron);
        var spot=Group(t,"Red spill",V(0,.045f,.37f)).gameObject.AddComponent<Light>();
        spot.type=LightType.Spot;spot.transform.localRotation=Quaternion.LookRotation(V(0,-1,-.04f));
        spot.spotAngle=126;spot.innerSpotAngle=67;spot.range=4.1f;spot.color=new Color(1,.095f,.046f);
        spot.intensity=3.25f;spot.shadows=LightShadows.None;return spot;
    }
    static void Photograph(Transform p,Vector3 top,int i,Material photo,Materials k){
        float tilt=new[]{-4f,3.3f,-2.4f,5.2f}[i];var t=Group(p,"Drying print "+(i+1),top);
        t.localRotation=Quaternion.Euler(-3,i*2-2,tilt);
        Box(t,"Paper back",V(0,-.21f,0),V(.398f,.423f,.004f),k.paper);
        var q=GameObject.CreatePrimitive(PrimitiveType.Quad);q.name="Silver print";q.transform.SetParent(t,false);
        q.transform.localPosition=V(0,-.21f,.0035f);q.transform.localRotation=Quaternion.Euler(0,180,0);q.transform.localScale=V(.391f,.414f,1);
        q.GetComponent<Renderer>().sharedMaterial=photo;SceneObjects.Remove(q.GetComponent<Collider>());
        var clip=Box(t,"Wooden clip",V(.018f,.016f,.012f),V(.024f,.077f,.028f),k.wood);
        clip.transform.localRotation=Quaternion.Euler(-6,0,3);Rod(t,V(.003f,.016f,.023f),V(.033f,.016f,.023f),.005f,k.brass);
    }
    static void Bench(Transform room,Materials k){
        const float width=2.53f;
        Box(room,"Work shelf",V(0,.91f,.225f),V(width,.076f,.45f),k.wood);
        foreach(float x in new[]{-1f,.98f}){
            Rod(room,V(x,.49f,.036f),V(x,.87f,.39f),.025f,k.iron);
            Rod(room,V(x,.48f,.032f),V(x,.86f,.032f),.023f,k.iron);
        }
        var atlas=Resources.Load<Texture2D>("Art/DarkroomPrints");
        Vector3 prev=V(-1.19f,1.98f,.125f);
        foreach(float x in new[]{-1.19f,1.19f})Rod(room,V(x,1.979f,.005f),V(x,1.979f,.135f),.013f,k.brass);
        for(int j=1;j<=12;j++){
            float t=j/12f;var next=V(Mathf.Lerp(-1.19f,1.19f,t),1.98f-.085f*4*t*(1-t),.125f);
            Rod(room,prev,next,.005f,k.iron);prev=next;
        }
        for(int i=0;i<4;i++){
            float x=.83f-i*.55f,t=(x+1.19f)/2.38f;
            var mat=new Material(k.paper){name="Contact print "+i,color=Color.white,mainTexture=atlas,
                mainTextureScale=new Vector2(.496f,.496f),mainTextureOffset=new Vector2((i%2)*.5f+.002f,(1-i/2)*.5f+.002f)};
            Photograph(room,V(x,1.98f-.085f*4*t*(1-t),.129f),i,mat,k);
        }
        for(int i=0;i<2;i++){
            var tray=Group(room,"Enamel bath",V(-.69f+i*.66f,.966f,.255f),i==0?2:-4);
            Box(tray,"Tray bottom",Vector3.zero,V(.56f,.028f,.32f),k.enamel);
            Box(tray,"Developer",V(0,.021f,0),V(.53f,.013f,.286f),k.water);
            foreach(int side in new[]{-1,1}){
                Box(tray,"Rolled enamel lip",V(side*.272f,.035f,0),V(.019f,.07f,.32f),k.enamel);
                Box(tray,"Rolled enamel lip",V(0,.035f,side*.152f),V(.55f,.07f,.018f),k.enamel);
            }
        }
        // Rest the print tongs across the front lip of one bath.
        var tongs=Group(room,"Print tongs",V(-.65f,1.047f,.34f),16);
        for(int side=-1;side<=1;side+=2){
            Rod(tongs,V(-.13f,0,0),V(.052f,-.006f,side*.017f),.009f,k.iron);
            Rod(tongs,V(.052f,-.006f,side*.017f),V(.115f,-.024f,side*.025f),.009f,k.iron);
            Rod(tongs,V(.115f,-.024f,side*.025f),V(.135f,-.034f,side*.015f),.011f,k.enamel);
        }
        var can=Group(room,"Rolled film can",V(-1.067f,1.073f,.183f));
        Round(can,"Film spool",Vector3.zero,V(.084f,.087f,.084f),k.iron,PrimitiveType.Cylinder);
        foreach(float y in new[]{-.087f,.087f})Round(can,"Tin cap",V(0,y,0),V(.101f,.008f,.101f),k.brass,PrimitiveType.Cylinder);
        var leader=Box(room,"Film leader",V(-1.057f,.963f,.313f),V(.068f,.007f,.196f),k.iron);
        leader.transform.localRotation=Quaternion.Euler(0,10,0);
        for(int j=0;j<4;j++)foreach(float x in new[]{-.026f,.026f})
            Box(room,"Film perforation",V(-1.057f+x+(j-1.5f)*.006f,.968f,.255f+j*.034f),V(.006f,.003f,.011f),k.enamel);
        for(int i=0;i<3;i++){
            var bottle=Group(room,"Amber reagent bottle",V(.51f+.217f*i,.952f,.18f+(i%2)*.085f));
            Round(bottle,"Glass body",V(0,.13f,0),V(.145f,.117f,.145f),k.brass,PrimitiveType.Cylinder);
            Round(bottle,"Shoulder",V(0,.245f,0),V(.145f,.1f,.145f),k.brass);
            Round(bottle,"Threaded cap",V(0,.301f,0),V(.075f,.026f,.075f),k.iron,PrimitiveType.Cylinder);
            Box(bottle,"Paper label",V(0,.135f,.068f),V(.073f,.088f,.008f),k.paper);
        }
    }
    void LateUpdate(){
        var game=GameController.Instance;if(game==null||game.Player==null)return;
        var view=game.Player.view.transform;
        var eye=view.position;
        if(!game.Paused && !HeardDarkroomCreak){
            var toPrints=lamps[3].transform.parent.parent.TransformPoint(V(0,1.77f,.13f))-eye;
            bool inspecting=toPrints.sqrMagnitude<8f && Vector3.Dot(view.forward,toPrints.normalized)>.88f;
            inspectionTime=inspecting?inspectionTime+Time.deltaTime:0;
            if(inspectionTime>=2.15f){
                creakAt=Time.time;
                game.Audio.DarkroomCreak(V(21.9f,-2.65f,9.63f));
            }
        }
        for(int i=0;i<lamps.Length;i++){
            var light=lamps[i];bool near=(light.transform.position-eye).sqrMagnitude<144f;
            if(light.enabled!=near)light.enabled=near;
            if(near){
                float intensity=3.55f+.20f*Mathf.Sin(Time.time*1.65f+i*1.92f);
                float age=Time.time-creakAt;
                if(i==3 && age<2.8f)intensity*=1-.67f*Mathf.Pow(Mathf.Sin(Mathf.PI*age/2.8f),2);
                light.intensity=intensity;
            }
        }
    }
}
}
