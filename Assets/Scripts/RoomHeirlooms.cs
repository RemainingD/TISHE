using System.Collections.Generic;
using UnityEngine;
using static Tishe.ReferenceArt;

namespace Tishe {
// The same furnishings are used by the saved house and by a fresh scene bake.
public sealed partial class RoomHeirlooms {
    readonly Material oak=Surface("Wood",new Color(.53f,.34f,.22f));
    readonly Material green=Surface("Wood",new Color(.36f,.48f,.40f));
    readonly Material linen=Surface("Linen",new Color(.78f,.72f,.57f),2);
    readonly Material plush=Surface("Linen",new Color(.52f,.33f,.21f),3);
    readonly Material iron=Glaze("Stove iron",new Color(.16f,.17f,.16f),.42f,.32f);
    readonly Material brass=Finish("Worn brass",new Color(.65f,.45f,.23f),.52f,.48f);
    readonly Material enamel=Glaze("Bottle-green enamel",new Color(.16f,.35f,.31f),.19f,.36f);
    readonly Material ivory=Glaze("Cream stoneware",new Color(.78f,.74f,.62f),.06f,.47f);
    readonly Material blue=Glaze("Cobalt glaze",new Color(.14f,.25f,.39f),.20f,.47f);
    readonly Material rust=Finish("Red lacquer",new Color(.50f,.20f,.12f),.15f,.32f);
    readonly Material drawing,recipe,quilt,tiles;
    public RoomHeirlooms(){
        var atlas=Resources.Load<Texture2D>("Art/HouseHeirlooms");
        drawing=Print(atlas,0,1);recipe=Print(atlas,1,1);quilt=Print(atlas,0,0);tiles=Print(atlas,1,0);
    }
    static Material Finish(string name,Color color,float metal,float gloss){
        var m=new Material(Shader.Find("Standard")){name=name,color=color};
        m.SetFloat("_Metallic",metal);m.SetFloat("_Glossiness",gloss);return m;
    }
    static Material Glaze(string name,Color color,float metal,float gloss){
        var m=Finish(name,color,metal,gloss);
        m.SetTexture("_BumpMap",Resources.Load<Texture2D>("Art/StoneNormal"));
        m.EnableKeyword("_NORMALMAP");m.SetFloat("_BumpScale",.095f);return m;
    }
    static Material Print(Texture atlas,int x,int y){
        var m=Finish("Heirloom print "+x+y,new Color(.9f,.88f,.80f),0,.13f);
        m.mainTexture=atlas;m.mainTextureScale=Vector2.one*.494f;m.mainTextureOffset=new Vector2(x*.5f+.003f,y*.5f+.003f);return m;
    }
    static Vector3 V(float x,float y,float z)=>new Vector3(x,y,z);
    static Transform Part(Transform p,string name,Vector3 at){
        var t=new GameObject(name).transform;t.SetParent(p,false);t.localPosition=at;return t;
    }
    static void PrintFace(Transform p,string name,Vector3 at,Vector3 normal,float w,float h,Material m){
        var face=GameObject.CreatePrimitive(PrimitiveType.Quad);face.name=name;face.transform.SetParent(p,false);
        face.transform.localPosition=at;face.transform.localRotation=Quaternion.LookRotation(-normal);
        face.transform.localScale=V(w,h,1);face.GetComponent<Renderer>().sharedMaterial=m;SceneObjects.Remove(face.GetComponent<Collider>());
    }
    // Closed surfaces of revolution. Each pair is a radius and height in metres.
    static Transform Turn(Transform p,string name,Vector3 at,Material m,params float[] profile){
        const int sides=24;
        var vs=new List<Vector3>();var uv=new List<Vector2>();var faces=new List<int>();
        for(int j=0;j<profile.Length/2;j++)for(int i=0;i<=sides;i++){
            float u=i/(float)sides,a=u*Mathf.PI*2,r=profile[j*2];
            vs.Add(V(r*Mathf.Cos(a),profile[j*2+1],r*Mathf.Sin(a)));uv.Add(new Vector2(u,j/(profile.Length*.5f-1)));
            if(j==0||i==0)continue;
            int n=vs.Count-1,b=n-sides-1;faces.AddRange(new[]{b-1,n-1,n,b-1,n,b});
        }
        var mesh=new Mesh{name=name};mesh.SetVertices(vs);mesh.SetUVs(0,uv);mesh.SetTriangles(faces,0);mesh.RecalculateNormals();mesh.RecalculateTangents();
        var t=Part(p,name,at);t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=m;return t;
    }
    static void Arch(Transform p,Vector3 center,Vector3 across,float height,float radius,Material mat){
        Vector3 prev=center+across;
        for(int i=1;i<=18;i++){
            float a=i*Mathf.PI/18;var cur=center+across*Mathf.Cos(a)+Vector3.up*(height*Mathf.Sin(a));
            Rod(p,prev,cur,radius,mat);prev=cur;
        }
    }
    void Dish(Transform p,Vector3 at,float scale){
        var t=Turn(p,"Stoneware plate",at,ivory,0,0,.10f,0,.17f,.018f,.205f,.038f,.207f,.044f,.196f,.046f,.147f,.023f,.10f,.014f,0,.014f);
        t.localScale=Vector3.one*scale;
        Turn(t,"Blue rim",Vector3.zero,blue,.197f,.045f,.197f,.048f,.19f,.046f);
    }
    void Cup(Transform p,Vector3 at){
        Turn(p,"Enamel mug",at,ivory,0,0,.064f,0,.080f,.145f,.076f,.15f,.070f,.145f,.060f,.023f,0,.023f);
        Turn(p,"Rolled blue rim",at,blue,.078f,.14f,.083f,.145f,.08f,.152f,.074f,.151f);
        var grip=Part(p,"Cup ear",at+V(.070f,.078f,0));grip.localRotation=Quaternion.Euler(0,0,-90);
        Arch(grip,Vector3.zero,Vector3.right*.048f,.057f,.009f,ivory);
    }
    void Jar(Transform p,Vector3 at,float h,Material glaze){
        var jar=Turn(p,"Preserving crock",at,glaze,0,0,.105f,0,.127f,.028f,.13f,.20f,.117f,.235f,.092f,.25f,.092f,.27f,0,.27f);
        jar.localScale=V(1,h/.27f,1);
        Turn(jar,"Glazed lid",V(0,.267f,0),ivory,0,0,.1f,0,.103f,.012f,.08f,.024f,.024f,.028f,.019f,.049f,0,.049f);
        Turn(jar,"Painted slip band",Vector3.zero,ivory,.13f,.12f,.131f,.123f,.131f,.135f,.13f,.137f);
    }
    void Kettle(Transform p,Vector3 at){
        var k=Part(p,"Enamel kettle",at);
        Turn(k,"Enamel body",Vector3.zero,enamel,0,0,.174f,0,.22f,.045f,.23f,.15f,.214f,.255f,.15f,.32f,.123f,.326f,0,.326f);
        Turn(k,"Fitted lid",V(0,.321f,0),iron,0,0,.145f,0,.15f,.012f,.113f,.028f,.037f,.036f,.029f,.063f,0,.065f);
        var spout=Turn(k,"Open spout",V(.165f,.105f,.055f),enamel,.103f,0,.088f,.095f,.060f,.20f,.058f,.265f,.050f,.267f,.043f,.22f);
        spout.localRotation=Quaternion.Euler(10,0,-46);
        Arch(k,V(0,.27f,0),V(.025f,0,.194f),.29f,.018f,brass);
        foreach(int side in new[]{-1,1})Round(k,"Handle rivet",V(.02f*side,.27f,.196f*side),Vector3.one*.054f,iron);
    }

    public void Kitchen(Transform room){
        var g=Part(room,"Kitchen heirlooms",Vector3.zero);
        // Existing worktops and cabinets keep their original collision envelopes.
        foreach(float z in new[]{-2.91f,-2.20f,-1.49f,1.49f,2.20f,2.91f})
            PrintFace(g,"Delft backsplash",V(-3.51f,1.37f,z),Vector3.right,.71f,.71f,tiles);
        var rail=Part(g,"Hanging cookware",V(-2.03f,2.25f,-3.43f));
        Rod(rail,V(-.61f,0,0),V(.61f,0,0),.022f,brass);
        foreach(float x in new[]{-.52f,.52f})Rod(rail,V(x,0,-.11f),V(x,0,.008f),.025f,iron);
        for(int i=0;i<2;i++){
            float x=i==0?-.28f:.27f;
            var pan=Part(rail,"Hanging skillet",V(x,-.68f-i*.12f,.047f));pan.localRotation=Quaternion.Euler(0,0,i==0?-5:6);
            var bowl=Turn(pan,"Hammered skillet",Vector3.zero,i==0?iron:enamel,0,0,.145f,0,.18f,.025f,.221f,.09f,.225f,.105f,.215f,.105f,.176f,.039f,.142f,.018f,0,.018f);
            bowl.localRotation=Quaternion.Euler(90,0,0);
            Rod(pan,V(0,.19f,.04f),V(0,.60f+i*.12f,.025f),.025f,oak);
            Arch(pan,V(0,.625f+i*.12f,.026f),Vector3.right*.035f,.045f,.009f,brass);
        }
        Box(g,"Range enamel fascia",V(-2.077f,.71f,-1.99f),V(.085f,.32f,1.25f),enamel);
        Box(g,"Oven door",V(-2.045f,.365f,-1.99f),V(.10f,.46f,1.24f),iron);
        Box(g,"Oven inset",V(-1.987f,.36f,-1.99f),V(.018f,.30f,.94f),enamel);
        Rod(g,V(-1.925f,.55f,-2.44f),V(-1.925f,.55f,-1.54f),.025f,brass);
        for(int i=0;i<4;i++)Round(g,"Bakelite knob",V(-2.005f,.74f,-2.39f+i*.26f),V(.08f,.096f,.096f),iron);
        Box(g,"Range top",V(-2.69f,1.046f,-2),V(1.08f,.048f,1.34f),ivory);
        for(int i=0;i<4;i++)Turn(g,"Cast burner",V(-2.93f+i%2*.48f,1.073f,-2.33f+i/2*.64f),iron,
            .105f,0,.177f,0,.18f,.015f,.169f,.027f,.110f,.02f,.105f,0);
        Kettle(g,V(-2.47f,1.101f,-1.69f));
        var rack=Part(g,"Plate shelf",V(-3.27f,1.99f,-2.25f));
        Box(rack,"Shelf board",Vector3.zero,V(.48f,.065f,1.60f),oak);
        foreach(float z in new[]{-.55f,.55f})Rod(rack,V(-.20f,-.37f,z),V(.18f,-.055f,z),.036f,iron);
        for(int i=0;i<4;i++)Jar(rack,V(0,.034f,-.56f+i*.33f),.22f+.05f*(i%3),i%2==0?enamel:blue);
        // A deep farmhouse basin sits over the old slab, with its own inner floor.
        var basin=Part(g,"Porcelain washbasin",V(-2.55f,1.031f,-.53f));
        Box(basin,"Basin floor",V(0,.028f,0),V(.82f,.055f,1.03f),ivory);
        foreach(int sign in new[]{-1,1}){
            Box(basin,"Raised lip",V(sign*.39f,.12f,0),V(.072f,.22f,1.06f),ivory);
            Box(basin,"End lip",V(0,.12f,sign*.50f),V(.76f,.22f,.064f),ivory);
        }
        Turn(basin,"Drain strainer",V(.09f,.06f,-.1f),iron,0,0,.045f,0,.046f,.004f,0,.004f);
        for(int i=0;i<4;i++)Dish(g,V(-2.58f,1.023f+i*.024f,.90f),1);
        Cup(g,V(-2.39f,1.025f,1.29f));
        // A single unfinished place setting.
        Dish(g,V(.00f,.994f,1.61f),1.08f);Cup(g,V(.40f,.994f,1.50f));
        Rod(g,V(.26f,1.009f,1.64f),V(.33f,1.009f,1.87f),.011f,brass);
        Round(g,"Spoon bowl",V(.34f,1.013f,1.90f),V(.061f,.020f,.093f),brass);
        PrintFace(g,"Preserves card",V(2.58f,1.95f,-1.765f),Vector3.forward,.49f,.47f,recipe);
        Round(g,"Fridge magnet",V(2.58f,2.16f,-1.742f),V(.06f,.06f,.03f),rust);
        var lamp=Part(g,"Pendant above table",V(.12f,2.72f,1.57f));
        Rod(lamp,Vector3.up*.18f,Vector3.up*1.43f,.013f,iron);
        Turn(lamp,"Enamel pendant shade",Vector3.zero,enamel,.322f,0,.333f,.015f,.29f,.052f,.15f,.17f,.066f,.19f,.044f,.26f,0,.26f);
        var bulb=Finish("Aging filament",new Color(.87f,.64f,.29f),0,.27f);
        bulb.EnableKeyword("_EMISSION");bulb.SetColor("_EmissionColor",new Color(1.15f,.52f,.13f));
        Round(lamp,"Amber bulb",V(0,.005f,0),V(.105f,.155f,.105f),bulb);
        var light=Part(lamp,"Table lamplight",V(0,-.06f,0)).gameObject.AddComponent<Light>();
        light.type=LightType.Spot;light.transform.localRotation=Quaternion.Euler(90,0,0);light.spotAngle=103;light.range=4.1f;light.intensity=1.42f;light.color=new Color(1,.68f,.38f);light.shadows=LightShadows.None;
    }
    void Star(Transform p,Vector3 at,Material m,float scale){
        var vertices=new List<Vector3>{V(0,0,-.028f),V(0,0,.028f)};
        var tris=new List<int>();
        for(int i=0;i<10;i++){
            float a=i*Mathf.PI/5,r=i%2==0?.13f:.056f;
            vertices.Add(V(r*Mathf.Sin(a),r*Mathf.Cos(a),0));
            int j=2+i,k=2+(i+1)%10;
            tris.AddRange(new[]{0,j,k,1,k,j});
        }
        var mesh=new Mesh{name="Faceted star"};mesh.SetVertices(vertices);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();
        var t=Part(p,"Carved star",at);t.localRotation=Quaternion.Euler(0,35,12);t.localScale=Vector3.one*scale;
        t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=m;
    }
    void Bear(Transform p,Vector3 at){
        var bear=Part(p,"Mended bear",at);bear.localRotation=Quaternion.Euler(-7,63,7);
        Round(bear,"Velvet body",V(0,.20f,0),V(.28f,.36f,.20f),plush);
        Round(bear,"Head",V(0,.46f,.014f),V(.30f,.275f,.244f),plush);
        Round(bear,"Muzzle",V(0,.42f,.119f),V(.155f,.105f,.079f),linen);
        Round(bear,"Snout",V(0,.451f,.155f),V(.052f,.034f,.028f),iron);
        foreach(int side in new[]{-1,1}){
            Round(bear,"Ear",V(side*.116f,.57f,.016f),Vector3.one*.107f,plush);
            Round(bear,"Velvet paw",V(side*.106f,.064f,.07f),V(.16f,.15f,.20f),plush);
            var arm=Round(bear,"Sleeve",V(side*.14f,.235f,.004f),V(.099f,.22f,.118f),plush);
            arm.transform.localRotation=Quaternion.Euler(-12,0,side*21);
            Round(bear,"Button eye",V(side*.064f,.485f,.118f),Vector3.one*.024f,iron);
        }
        Turn(bear,"Cloth neck ribbon",V(0,.342f,0),rust,.10f,0,.108f,.013f,.10f,.031f);
    }
    void Train(Transform g){
        var train=Part(g,"Clockwork wooden train",V(.49f,0,-2.05f));train.localRotation=Quaternion.Euler(0,13,0);
        for(int k=0;k<3;k++){
            var car=Part(train,k==0?"Locomotive":"Wagon",V(0,0,k*.51f));
            Box(car,"Undercarriage",V(0,.134f,0),V(.25f,.078f,.424f),oak);
            for(int side=-1;side<=1;side+=2)foreach(float z in new[]{-.139f,.139f}){
                var wheel=Turn(car,"Turned wheel",V(side*.143f,.090f,z),rust,0,0,.084f,0,.086f,.026f,.075f,.036f,0,.036f);
                wheel.localRotation=Quaternion.Euler(0,0,-90*side);
                Round(car,"Axle pin",V(side*.183f,.09f,z),Vector3.one*.030f,brass);
            }
            if(k==0){
                var boiler=Turn(car,"Boiler",V(0,.245f,-.20f),green,0,0,.108f,0,.108f,.30f,0,.30f);
                boiler.localRotation=Quaternion.Euler(90,0,0);
                Turn(car,"Funnel",V(0,.33f,-.11f),iron,.036f,0,.032f,.06f,.062f,.09f,.059f,.107f,0,.107f);
                Box(car,"Cab",V(0,.29f,.13f),V(.21f,.23f,.13f),oak);
                Box(car,"Cab roof",V(0,.417f,.13f),V(.28f,.036f,.19f),green);
            }else{
                Box(car,"Wooden wagon",V(0,.21f,0),V(.23f,.13f,.38f),k==1?green:blue);
                Round(car,"Cargo",V(0,.31f,.02f),V(.14f,.13f,.26f),k==1?rust:linen);
                Rod(car,V(0,.118f,-.30f),V(0,.118f,-.18f),.015f,brass);
            }
        }
    }
    public void Nursery(Transform room){
        var g=Part(room,"Nursery heirlooms",Vector3.zero);
        var bed=Part(g,"Spindle bed",V(-2.36f,0,-.30f));
        var collider=bed.gameObject.AddComponent<BoxCollider>();collider.center=V(0,.46f,0);collider.size=V(1.37f,.88f,2.37f);
        foreach(int side in new[]{-1,1}){
            foreach(int end in new[]{-1,1}){
                var post=Turn(bed,"Turned corner post",V(side*.685f,0,end*1.17f),green,0,0,.062f,.01f,.068f,.07f,.044f,.12f,.042f,.24f,.059f,.30f,.056f,.93f,.073f,.965f,.048f,1.01f,.033f,1.055f,.058f,1.08f,.063f,1.12f,.035f,1.153f,0,1.16f);
                if(end<0)post.localScale=V(1,1.18f,1);
            }
            Box(bed,"Side apron",V(side*.665f,.54f,0),V(.072f,.19f,2.32f),green);
            // Lower guard beside the player leaves the pillow and quilt in view.
            float rail=side<0?1.08f:.82f;
            Box(bed,"Guard rail",V(side*.68f,rail,0),V(.076f,.062f,2.32f),green);
            for(int i=0;i<10;i++){
                var bar=Turn(bed,"Spindle",V(side*.68f,.62f,-1.04f+i*.23f),oak,.019f,0,.025f,.04f,.016f,.10f,.016f,.77f,.025f,.89f,.019f,1);
                bar.localScale=V(1,rail-.62f,1);
            }
            float cap=side<0?1.13f:.89f;
            Box(bed,"Panel end",V(0,(cap+.45f)*.5f,side*1.17f),V(1.33f,cap-.45f,.064f),green);
            Arch(bed,V(0,cap,side*1.17f),Vector3.right*.64f,.13f,.034f,oak);
            for(int i=0;i<7;i++){
                float x=-.49f+i*.163f;
                Rod(bed,V(x,cap,side*1.17f),V(x,cap+.13f*Mathf.Sqrt(1-x*x/.41f),side*1.17f),.018f,oak);
            }
        }
        SoftFurnishing.Cushion(bed,"Ticking mattress",V(0,.671f,0),V(1.29f,.195f,2.25f),linen);
        SoftFurnishing.Cushion(bed,"Small pillow",V(0,.801f,-.79f),V(.92f,.16f,.51f),linen);
        // A broad folded quilt follows the padded mattress and dips over both edges.
        var verts=new List<Vector3>();var uvs=new List<Vector2>();var tri=new List<int>();
        const int nx=32,nz=32;
        for(int z=0;z<=nz;z++)for(int x=0;x<=nx;x++){
            float u=x/(float)nx,v=z/(float)nz,px=u*2-1;
            float edge=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.78f,1,Mathf.Abs(px)));
            float fold=.022f*Mathf.Sin(19*u+v*2)+.012f*Mathf.Sin(23*v-5*u);
            verts.Add(V(px*.659f,.807f+fold-.112f*edge+.034f*Mathf.Exp(-120*(v-.04f)*(v-.04f)),-.37f+v*1.44f+px*.045f));
            uvs.Add(new Vector2(u,v));int n=z*(nx+1)+x;
            if(x<nx&&z<nz)tri.AddRange(new[]{n,n+nx+1,n+1,n+1,n+nx+1,n+nx+2});
        }
        var mesh=new Mesh{name="Quilt folds"};mesh.SetVertices(verts);mesh.SetUVs(0,uvs);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateTangents();
        var blanket=Part(bed,"Star patchwork quilt",Vector3.zero);blanket.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;blanket.gameObject.AddComponent<MeshRenderer>().sharedMaterial=quilt;
        Bear(bed,V(.19f,.802f,-.73f));
        Rod(bed,V(-.62f,1.13f,-.81f),V(-.62f,2.32f,-.81f),.025f,brass);
        Arch(bed,V(-.33f,2.32f,-.81f),Vector3.right*.29f,.18f,.023f,brass);
        var mobile=Part(bed,"Hanging constellation",V(-.04f,2.32f,-.81f));
        mobile.gameObject.AddComponent<NurseryMobile>();
        Rod(mobile,Vector3.zero,Vector3.down*.19f,.006f,linen);
        for(int i=0;i<4;i++){
            float a=i*91*Mathf.Deg2Rad;var tip=V(.39f*Mathf.Cos(a),-.22f,.39f*Mathf.Sin(a));var drop=tip-Vector3.up*(.16f+i*.069f);
            Rod(mobile,Vector3.down*.19f,tip,.012f,oak);Rod(mobile,tip,drop,.0045f,linen);
            Star(mobile,drop-Vector3.up*.068f,i%2==0?brass:rust,.80f-i*.055f);
        }
        var picture=Part(g,"Pinned drawing",V(-1.97f,1.78f,-3.525f));
        picture.localRotation=Quaternion.Euler(0,0,-4);
        PrintFace(picture,"House in crayon",Vector3.zero,Vector3.forward,.81f,.79f,drawing);
        foreach(float x in new[]{-.34f,.32f})Round(picture,"Drawing pin",V(x,.35f,.013f),Vector3.one*.027f,brass);
        Train(g);
    }
}
}
