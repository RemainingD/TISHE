using UnityEngine;
using static Tishe.ReferenceArt;

namespace Tishe {
public sealed class LivingRoomEffects : MonoBehaviour {
    public Renderer screen;
    public Light firelight;
    AudioSource crackle,hiss;
    AudioClip fireLoop,snowLoop;
    bool powered;
    static Vector3 V(float x,float y,float z)=>new Vector3(x,y,z);
    static Transform Part(Transform t,string name,Vector3 at){
        var child=new GameObject(name).transform;child.SetParent(t,false);child.localPosition=at;return child;
    }
    static Material Finish(Color color,float gloss){
        var m=new Material(Shader.Find("Standard")){color=color};m.SetFloat("_Glossiness",gloss);return m;
    }
    static void Quad(Transform t,Vector3 at,float angle,float w,float h,Material m){
        var o=GameObject.CreatePrimitive(PrimitiveType.Quad);o.name="Flame tongues";o.transform.SetParent(t,false);
        o.transform.localPosition=at;o.transform.localRotation=Quaternion.Euler(0,angle,0);o.transform.localScale=V(w,h,1);
        SceneObjects.Remove(o.GetComponent<Collider>());var r=o.GetComponent<Renderer>();r.sharedMaterial=m;
        r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;
    }
    static Renderer TubeGlass(Transform t,Material material){
        const int nx=20,ny=16;var o=Part(t,"Convex CRT glass",V(2.30f,.92f,-2.234f));
        var vs=new Vector3[(nx+1)*(ny+1)];var uv=new Vector2[vs.Length];var faces=new int[nx*ny*6];int index=0;
        for(int j=0;j<=ny;j++)for(int i=0;i<=nx;i++){
            float u=i/(float)nx,v=j/(float)ny,x=u*2-1,y=v*2-1;int n=j*(nx+1)+i;
            vs[n]=V(.397f*x*(1-.075f*Mathf.Pow(y,6)),.282f*y*(1-.07f*Mathf.Pow(x,6)),.044f*(1-x*x)*(1-y*y));
            uv[n]=new Vector2(u,v);
            if(i<nx&&j<ny){faces[index++]=n;faces[index++]=n+1;faces[index++]=n+nx+1;faces[index++]=n+1;faces[index++]=n+nx+2;faces[index++]=n+nx+1;}
        }
        var mesh=new Mesh{name="Curved phosphor face"};mesh.vertices=vs;mesh.uv=uv;mesh.triangles=faces;mesh.RecalculateNormals();
        o.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;var r=o.gameObject.AddComponent<MeshRenderer>();r.sharedMaterial=material;return r;
    }
    public static void Install(Transform room){
        var group=Part(room,"Hearth and television",Vector3.zero);
        var fx=group.gameObject.AddComponent<LivingRoomEffects>();
        var iron=Finish(new Color(.11f,.10f,.094f),.28f);
        var wood=Surface("Bark",new Color(.25f,.19f,.15f),2);
        var brass=Finish(new Color(.55f,.38f,.20f),.36f);
        var ember=Finish(new Color(.40f,.085f,.022f),.13f);
        ember.EnableKeyword("_EMISSION");ember.SetColor("_EmissionColor",new Color(.78f,.105f,.011f));
        for(int j=0;j<5;j++)Rod(group,V(-.55f+j*.27f,.30f,-3.23f),V(-.55f+j*.27f,.32f,-2.79f),.026f,iron);
        foreach(float z in new[]{-3.16f,-2.84f})Rod(group,V(-.65f,.30f,z),V(.65f,.30f,z),.033f,iron);
        for(int j=0;j<3;j++){
            var a=V(-.58f+j*.04f,.38f+j*.044f,-3.18f+j*.13f);var b=V(.51f-j*.035f,.39f+j*.034f,-3.04f+j*.025f);
            Rod(group,a,b,.092f,wood);
            for(int k=1;k<=4;k++){
                var p=Vector3.Lerp(a,b,k*.19f)+V(0,.048f,.069f);
                Rod(group,p,p+V(.055f,.008f,.005f),.008f,ember);
            }
        }
        WallInstruments.PrintedFace(group,V(0,.83f,-3.308f),1.66f,1.31f,5);
        var flame=Resources.Load<Shader>("HearthFlame");
        for(int j=0;j<3;j++){
            var m=new Material(flame){name="Fire layer "+j};m.SetFloat("_Seed",4.2f*j);
            Quad(group,V((j-1)*.14f,.87f-j*.035f,-3.15f+j*.11f),176+j*5,1.22f-j*.14f,1.10f-j*.14f,m);
        }
        fx.firelight=Part(group,"Hearth glow",V(0,.73f,-2.65f)).gameObject.AddComponent<Light>();
        fx.firelight.color=new Color(1,.49f,.19f);fx.firelight.range=4.9f;fx.firelight.intensity=1.32f;fx.firelight.shadows=LightShadows.None;
        Box(group,"CRT rubber bezel",V(2.30f,.92f,-2.25f),V(.86f,.625f,.032f),iron);
        fx.screen=TubeGlass(group,new Material(Resources.Load<Shader>("TelevisionSnow")){name="Phosphor static"});
        foreach(float y in new[]{.97f,1.13f}){
            Round(group,"Channel knob",V(2.864f,y,-2.24f),V(.084f,.084f,.052f),brass);
            Rod(group,V(2.856f,y,-2.212f),V(2.862f,y+.03f,-2.212f),.005f,iron);
        }
        for(int j=0;j<7;j++)Box(group,"Television speaker",V(2.85f,.66f+j*.025f,-2.267f),V(.14f,.012f,.01f),iron);
        foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})
            Rod(group,V(2.4f+x*.49f,.032f,-2.60f+z*.23f),V(2.4f+x*.42f,.49f,-2.60f+z*.19f),.039f,wood);
    }
    static AudioClip MakeLoop(bool fire){
        const int rate=16000;var samples=new float[rate*7];var rng=new System.Random(fire?381:902);
        float low=0,last=0,pop=0;
        for(int i=0;i<samples.Length;i++){
            float n=2*(float)rng.NextDouble()-1;
            low+=.052f*(n-low);
            if(fire&&rng.NextDouble()<.00085)pop=.24f+.71f*(float)rng.NextDouble();
            samples[i]=fire?low*.66f+n*pop:(.68f*n-.31f*last);
            samples[i]*=Mathf.Clamp01(Mathf.Min(i,samples.Length-1-i)/160f);
            pop*=.974f;last=n;
        }
        var clip=AudioClip.Create(fire?"Wood embers":"Analog hiss",samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
    }
    static AudioSource Sound(Transform mount,AudioClip clip,float range){
        var s=mount.gameObject.AddComponent<AudioSource>();s.clip=clip;s.playOnAwake=false;s.loop=true;s.spatialBlend=1;s.dopplerLevel=0;
        s.rolloffMode=AudioRolloffMode.Linear;s.minDistance=.65f;s.maxDistance=range;return s;
    }
    void Start(){
        fireLoop=MakeLoop(true);snowLoop=MakeLoop(false);
        crackle=Sound(firelight.transform,fireLoop,6.2f);hiss=Sound(screen.transform,snowLoop,4.4f);
        SetPower(GameController.Instance.Solved[0]);
    }
    void SetPower(bool on){
        powered=on;var b=new MaterialPropertyBlock();b.SetFloat("_Power",on?1:0);screen.SetPropertyBlock(b);
    }
    void LateUpdate(){
        var game=GameController.Instance;if(game==null||game.Player==null)return;
        if(powered!=game.Solved[0])SetPower(game.Solved[0]);
        float t=Time.time;firelight.intensity=1.14f+.58f*Mathf.PerlinNoise(t*7.3f,.18f)+.26f*Mathf.PerlinNoise(.72f,t*13.7f);
        var ear=game.Player.view.transform.position;
        UpdateSound(crackle,ear,!game.Paused,game.Volume*.48f);
        UpdateSound(hiss,ear,powered&&!game.Paused,game.Volume*.085f);
    }
    static void UpdateSound(AudioSource src,Vector3 ear,bool play,float gain){
        src.volume=gain;bool inRange=play&&(src.transform.position-ear).sqrMagnitude<src.maxDistance*src.maxDistance;
        if(inRange&&!src.isPlaying)src.Play();
        else if(!inRange&&src.isPlaying)src.Stop();
    }
    void OnDestroy(){if(fireLoop!=null)Destroy(fireLoop);if(snowLoop!=null)Destroy(snowLoop);}
}
}
