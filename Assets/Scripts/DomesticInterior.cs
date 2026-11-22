using System.Collections.Generic;
using UnityEngine;
using static Tishe.ReferenceArt;

namespace Tishe {
public static class DomesticInterior {
    static readonly HashSet<string> WallNames = new HashSet<string> {
        "Plaster wall", "Wall below window", "Wall above window", "Window masonry jamb",
        "Solid doorway reveal", "Solid lintel above doorway", "Plastered entrance reveal",
        "Entrance lintel", "Upper ceiling", "Cellar wall", "Cellar entry wall", "Cellar ceiling"
    };

    static Transform Group(Transform parent, string name, Vector3 pos, float yaw=0) {
        var t=new GameObject(name).transform;
        t.SetParent(parent,false);
        t.localPosition=pos;
        t.localRotation=Quaternion.Euler(0,yaw,0);
        return t;
    }
    static Vector3 V(float x,float y,float z) => new Vector3(x,y,z);
    static Material Enamel(string name,Color color,float shine,float metallic=0) {
        var m=new Material(Shader.Find("Standard")) {name=name,color=color};
        m.SetFloat("_Glossiness",shine);
        m.SetFloat("_Metallic",metallic);
        return m;
    }
    static void ApplyFinish(Transform root,Material walls,Material slabs) {
        foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true)) {
            if(r.name=="Timber floor") r.sharedMaterial=slabs;
            else if(WallNames.Contains(r.name)) r.sharedMaterial=walls;
        }
    }
    static void Cloth(Transform t,Vector3 at,float width,float length,Material fabric) {
        const int across=12, down=9;
        var points=new List<Vector3>();var uv=new List<Vector2>();var indices=new List<int>();
        for(int j=0;j<=down;j++) for(int i=0;i<=across;i++) {
            float u=(float)i/across,v=(float)j/down;
            float fold=Mathf.Sin(u*17+.6f*v);
            points.Add(V((u-.5f)*width*Mathf.Lerp(.20f,1,Mathf.Sqrt(v)),
                -length*v+.038f*fold*v,
                .035f+.045f*fold*Mathf.Sqrt(v)+.07f*Mathf.Sin(v*2.8f)));
            uv.Add(new Vector2(u,v));
            if(j==down||i==across)continue;
            int a=j*(across+1)+i,b=a+across+1;
            indices.AddRange(new[]{a,b,a+1,a+1,b,b+1});
        }
        var mesh=new Mesh {name="Gathered linen folds"};
        mesh.SetVertices(points);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.RecalculateTangents();
        var g=Group(t,"Hanging linen",at).gameObject;
        g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=fabric;
    }
    static void Hooks(Transform t,Material wood,Material metal,Material cloth) {
        Box(t,"Oak peg rail",Vector3.zero,V(1.25f,.13f,.065f),wood);
        for(int i=-1;i<=1;i++){
            float x=i*.42f;
            Rod(t,V(x,0,.035f),V(x,-.018f,.13f),.019f,metal);
            Rod(t,V(x,-.018f,.13f),V(x,.068f,.165f),.017f,metal);
            Round(t,"Rounded hook tip",V(x,.07f,.165f),V(.052f,.052f,.052f),metal);
        }
        Cloth(t,V(-.42f,.067f,.168f),.46f,.88f,cloth);
    }
    static void Radiator(Transform t,Material paint,Material iron) {
        for(int i=0;i<9;i++){
            float x=(i-4)*.105f;
            Round(t,"Cast radiator section",V(x,.52f,.105f),V(.093f,.67f,.18f),paint);
            Round(t,"Raised rib",V(x,.52f,.168f),V(.045f,.55f,.08f),paint);
        }
        foreach(float h in new[]{.24f,.81f}){
            Rod(t,V(-.55f,h,.10f),V(.56f,h,.10f),.043f,paint);
            Rod(t,V(.55f,h,.10f),V(.68f,h,.10f),.023f,iron);
            Rod(t,V(.68f,h,.10f),V(.68f,h,.008f),.023f,iron);
        }
        foreach(float x in new[]{-.32f,.32f})
            Rod(t,V(x,.03f,.12f),V(x,.32f,.12f),.032f,paint);
        Round(t,"Bakelite valve knob",V(.59f,.88f,.1f),V(.085f,.039f,.085f),iron);
    }
    static void Plate(Transform t,Material porcelain,Material iron,bool outlet) {
        Box(t,"Porcelain faceplate",Vector3.zero,V(.132f,.16f,.03f),porcelain);
        if(outlet)foreach(float x in new[]{-.025f,.025f})
            Round(t,"Recessed pin socket",V(x,0,.017f),V(.017f,.017f,.006f),iron);
        else Box(t,"Bakelite rocker",V(0,0,.029f),V(.065f,.067f,.027f),iron).transform.localRotation=Quaternion.Euler(-13,0,0);
        foreach(float y in new[]{-.062f,.062f}) Round(t,"Retaining screw",V(0,y,.018f),V(.012f,.012f,.006f),iron);
    }
    // Revolve a ceramic cross section to produce an open bowl with a rolled lip.
    static void Bowl(Transform t,Material ceramic,Vector2[] profile,string label) {
        const int segments=32;
        var pts=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
        foreach(var ring in profile) for(int i=0;i<segments;i++){
            float a=2*Mathf.PI*i/segments;
            pts.Add(V(ring.x*Mathf.Cos(a),ring.y,ring.x*Mathf.Sin(a)));
            uv.Add(new Vector2(.5f+.5f*ring.x*Mathf.Cos(a),.5f+.5f*ring.x*Mathf.Sin(a)));
        }
        for(int j=1;j<profile.Length;j++)for(int i=0;i<segments;i++){
            int a=(j-1)*segments+i,b=(j-1)*segments+(i+1)%segments;
            triangles.AddRange(new[]{a,a+segments,b,b,a+segments,b+segments});
        }
        var mesh=new Mesh{name=label};mesh.SetVertices(pts);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
        var g=Group(t,label,Vector3.zero).gameObject;
        g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=ceramic;
    }
    static void Bathroom(Transform root,Material cream,Material iron,Material cloth,Material silver) {
        var wc=Group(root,"Water closet",V(13.12f,4.2f,-8.02f),-90);
        var pan=Group(wc,"Ceramic pan",V(0,0,.35f));
        pan.localScale=V(.235f,1,.31f);
        var profile=new[]{new Vector2(.39f,.04f),new Vector2(.34f,.18f),new Vector2(.55f,.27f),
            new Vector2(.90f,.35f),new Vector2(1,.45f),new Vector2(.98f,.48f),
            new Vector2(.81f,.48f),new Vector2(.73f,.40f),new Vector2(.3f,.28f),new Vector2(.015f,.25f)};
        Bowl(pan,cream,profile,"Porcelain bowl");
        Box(wc,"Enamel cistern",V(0,.80f,.14f),V(.45f,.32f,.20f),cream);
        Box(wc,"Cistern lid",V(0,.97f,.14f),V(.47f,.044f,.22f),cream);
        Rod(wc,V(-.14f,.88f,.25f),V(-.206f,.85f,.26f),.013f,iron);
        Rod(wc,V(0,.64f,.13f),V(0,.32f,.13f),.039f,iron);
        var basin=Group(root,"Wall-mounted pedestal basin",V(7.61f,4.2f,-12.79f));
        var oval=Group(basin,"Oval bowl",Vector3.zero);
        oval.localScale=V(.405f,1,.29f);
        Bowl(oval,cream,new[]{new Vector2(.19f,.70f),new Vector2(.60f,.75f),new Vector2(.91f,.82f),
            new Vector2(1,.89f),new Vector2(.99f,.91f),new Vector2(.86f,.91f),
            new Vector2(.74f,.855f),new Vector2(.41f,.81f),new Vector2(.05f,.79f)},"Glazed basin");
        Round(basin,"Cast pedestal",V(0,.37f,.015f),V(.22f,.83f,.23f),cream);
        Round(basin,"Foot",V(0,.047f,.015f),V(.31f,.093f,.32f),cream);
        Round(basin,"Drain",V(0,.794f,0),V(.052f,.006f,.052f),iron);
        foreach(float x in new[]{-.16f,.16f}){
            Round(basin,"Tap flange",V(x,.92f,-.196f),V(.062f,.025f,.062f),iron);
            Rod(basin,V(x,.92f,-.196f),V(x,1.014f,-.196f),.020f,iron);
            Rod(basin,V(x,.969f,-.19f),V(x,.98f,-.105f),.015f,iron);
            Rod(basin,V(x,.98f,-.105f),V(x,.953f,-.090f),.015f,iron);
            Rod(basin,V(x-.038f,1.014f,-.196f),V(x+.038f,1.014f,-.196f),.010f,cream);
        }
        var mirror=Group(root,"Basin mirror",V(7.61f,6.06f,-13.13f));
        Box(mirror,"Silvered glass",Vector3.zero,V(.66f,.90f,.034f),silver);
        foreach(float x in new[]{-.344f,.344f})Box(mirror,"Metal side",V(x,0,0),V(.028f,.96f,.052f),iron);
        foreach(float y in new[]{-.464f,.464f})Box(mirror,"Metal rim",V(0,y,0),V(.71f,.028f,.052f),iron);
        var probe=Group(mirror,"Basin surroundings",V(0,0,.31f)).gameObject.AddComponent<ReflectionProbe>();
        probe.mode=UnityEngine.Rendering.ReflectionProbeMode.Custom;
        probe.resolution=64;probe.boxProjection=true;probe.blendDistance=.06f;
        probe.size=V(6.8f,3.9f,6.8f);probe.center=V(1.92f,.14f,3.08f);probe.nearClipPlane=.09f;probe.farClipPlane=18;
        probe.customBakedTexture=Resources.Load<Cubemap>("Art/BasinReflection");
        var shelf=Group(mirror,"Wash shelf",V(0,-.59f,.12f));
        Box(shelf,"Shelf",Vector3.zero,V(.74f,.034f,.24f),cream);
        Round(shelf,"Worn soap",V(-.2f,.047f,.016f),V(.137f,.059f,.079f),cream);
        var rail=Group(root,"Towel rail",V(8.74f,5.61f,-13.12f));
        Rod(rail,V(-.27f,0,.13f),V(.27f,0,.13f),.018f,iron);
        foreach(float x in new[]{-.27f,.27f})Rod(rail,V(x,0,0),V(x,0,.13f),.018f,iron);
        Cloth(rail,V(.04f,.008f,.13f),.37f,.61f,cloth);
    }
    static void Desk(Transform root, Material leaves, Material ink) {
        var pad=Group(root,"Unfinished correspondence",V(-9.32f,5.509f,-11.69f),-12);
        Box(pad,"Notebook spine",Vector3.zero,V(.31f,.015f,.22f),ink);
        Box(pad,"Yellowed pages",V(0,.012f,0),V(.304f,.010f,.217f),leaves);
        for(int j=0;j<6;j++){
            float length=.10f+.068f*Mathf.Abs(Mathf.Sin(j*2.7f));
            Box(pad,"Ink line",V(-.126f+length*.5f,.018f,.076f-j*.025f),V(length,.001f,.003f),ink);
        }
        Rod(pad,V(.11f,.025f,-.10f),V(.16f,.025f,.065f),.006f,ink);
    }
    static void AlarmClock(Transform root,Material metal,Material ivory,Material dark) {
        var clock=Group(root,"Bedside alarm clock",V(-6.016f,1.20f,11.54f),-90);
        Box(clock,"Small bedside shelf",V(0,-.025f,.17f),V(.66f,.05f,.32f),metal);
        foreach(float x in new[]{-.18f,.18f})Rod(clock,V(x,-.22f,0),V(x,-.052f,.24f),.014f,metal);
        Round(clock,"Clock case",V(0,.17f,.18f),V(.29f,.29f,.11f),dark);
        Round(clock,"Clock face",V(0,.17f,.234f),V(.248f,.248f,.012f),ivory);
        for(int i=0;i<12;i++){
            float a=Mathf.PI*i/6;
            var tick=Box(clock,"Hour mark",V(Mathf.Sin(a)*.099f,.17f+Mathf.Cos(a)*.099f,.242f),V(.008f,.020f,.005f),dark);
            tick.transform.localRotation=Quaternion.Euler(0,0,-i*30);
        }
        Rod(clock,V(0,.17f,.247f),V(-.039f,.224f,.247f),.005f,dark);
        Rod(clock,V(0,.17f,.248f),V(.079f,.19f,.248f),.004f,dark);
        foreach(float x in new[]{-.086f,.086f}){
            Round(clock,"Bell",V(x,.304f,.18f),V(.11f,.072f,.103f),metal);
            Rod(clock,V(x,.016f,.19f),V(x*.85f,.076f,.18f),.010f,metal);
        }
    }
    public static void Install(Transform world,Transform decor) {
        var walls=new Material(Shader.Find("Tishe/Interior finishes")){name="Weathered domestic walls"};
        walls.SetTexture("_Atlas",Resources.Load<Texture2D>("Art/DomesticFinishes"));
        walls.SetTexture("_Boards",Resources.Load<Texture2D>("Art/FloorColor"));
        walls.SetTexture("_FloorNormal",Resources.Load<Texture2D>("Art/FloorNormal"));
        walls.SetTexture("_PlasterNormal",Resources.Load<Texture2D>("Art/PlasterNormal"));
        var slabs=new Material(walls){name="Continuous boards and plaster soffit"};slabs.SetFloat("_Slab",1);
        ApplyFinish(world,walls,slabs);ApplyFinish(decor,walls,slabs);
        foreach(Transform room in decor){
            if(room.name=="Bathroom / tiled surfaces"){
                foreach(Transform part in room){
                    if(part.name=="Floor tile" || part.name=="Faded family portrait" || part.name=="Pedestal sink" ||
                       (part.name=="Turned rail" && (part.localPosition-V(-2,.4f,-2)).sqrMagnitude<.0001f))
                        part.gameObject.SetActive(false);
                }
            }else if(room.name=="Finishing / architectural details"){
                foreach(Transform part in room){
                    if(part.name=="Old bath towel")part.localPosition=V(3.5f,-.76f,0);
                    if(part.name=="Turned rail" && (part.localPosition-V(8,6.7f,-12.9f)).sqrMagnitude<.0001f)
                        part.localPosition=V(11.5f,5.94f,-12.9f);
                }
            }
        }
        var root=Group(decor,"Everyday household fittings",Vector3.zero);
        var oak=Surface("Wood",new Color(.45f,.34f,.22f));
        var iron=Enamel("Patinated iron",new Color(.19f,.17f,.14f),.33f,.45f);
        var ivory=Enamel("Ivory porcelain",new Color(.74f,.71f,.58f),.41f);
        var paint=Enamel("Old radiator enamel",new Color(.50f,.50f,.44f),.26f,.14f);
        var cloth=Surface("Linen",new Color(.53f,.43f,.37f));
        var silver=Enamel("Smoked old silver",new Color(.50f,.56f,.55f),.76f,.85f);
        Hooks(Group(root,"Hall coat hooks",V(2.32f,1.74f,13.12f),180),oak,iron,cloth);
        Radiator(Group(root,"Hall heating",V(3.53f,0,-1.85f),-90),paint,iron);
        Radiator(Group(root,"Nursery heating",V(-12.15f,4.2f,3.53f),180),paint,iron);
        Plate(Group(root,"Entrance light switch",V(1.22f,1.33f,13.15f),180),ivory,iron,false);
        Plate(Group(root,"Kitchen light switch",V(-7.40f,1.23f,3.55f),180),ivory,iron,false);
        Plate(Group(root,"Bedroom socket",V(-6.019f,.37f,11.40f),-90),ivory,iron,true);
        var kitchen=Group(root,"Kitchen apron hook",V(-7.86f,1.50f,3.53f),180);
        Rod(kitchen,V(0,0,0),V(0,.016f,.103f),.017f,iron);
        Cloth(kitchen,V(0,.022f,.101f),.46f,.70f,cloth);
        Bathroom(root,ivory,iron,cloth,silver);Desk(root,ivory,iron);AlarmClock(root,oak,ivory,iron);
    }
}
}
