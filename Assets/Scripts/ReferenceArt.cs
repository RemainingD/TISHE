using System.Collections.Generic;
using UnityEngine;
namespace Tishe {
// Reference-inspired room kit. Real geometry, reusable materials and traversable floor space.
public static partial class ReferenceArt {
    static Transform root;static Material wood,wall,cloth,iron,cream,stone,glass,paper,roof,carpet,bedding;static Material[] portraits;static int portraitId;
    public static Material Surface(string resource,Color tint,float tiles=1) {
        var m=new Material(Shader.Find("Standard"));m.color=tint;m.SetFloat("_Glossiness",.12f);
        var t=Resources.Load<Texture2D>("Art/"+resource+"Color");if(t!=null){m.mainTexture=t;m.mainTextureScale=Vector2.one*tiles;}
        var n=Resources.Load<Texture2D>("Art/"+resource+"Normal");if(n!=null){m.SetTexture("_BumpMap",n);m.EnableKeyword("_NORMALMAP");m.SetFloat("_BumpScale",.55f);}return m;
    }
    static Material Flat(Color c){var m=new Material(Shader.Find("Standard"));m.color=c;m.SetFloat("_Glossiness",.12f);return m;}
    static Transform Group(string name,Vector3 pos,float yaw=0){var g=new GameObject(name).transform;g.SetParent(root,false);g.position=pos;g.rotation=Quaternion.Euler(0,yaw,0);return g;}
    public static GameObject Box(Transform parent,string name,Vector3 pos,Vector3 size,Material mat,bool collision=false) {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;BeveledGeometry.Apply(g,size);if(!collision)SceneObjects.Remove(g.GetComponent<Collider>());return g;
    }
    public static GameObject Round(Transform parent,string name,Vector3 p,Vector3 scale,Material m,PrimitiveType type=PrimitiveType.Sphere){var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;SceneObjects.Remove(g.GetComponent<Collider>());return g;}
    public static void Rod(Transform p,Vector3 a,Vector3 b,float radius,Material mat){var g=Round(p,"Turned rail",(a+b)/2,new Vector3(radius*2,Vector3.Distance(a,b)/2,radius*2),mat,PrimitiveType.Cylinder);g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);}
    static void Tube(Transform p,List<Vector3> points,float radius,Material mat){for(int i=1;i<points.Count;i++)Rod(p,points[i-1],points[i],radius,mat);}
    static void Sheet(Transform p,string name,Vector3 origin,float width,float length,Material material,bool vertical=false){int nx=64,nz=56;var v=new Vector3[(nx+1)*(nz+1)];var uv=new Vector2[v.Length];var indices=new List<int>();
        for(int z=0;z<=nz;z++)for(int x=0;x<=nx;x++){int i=z*(nx+1)+x;float u=(float)x/nx,w=(float)z/nz;float ripple=(Mathf.Sin(u*28+w*8)*.018f+Mathf.Sin(w*19+u*9)*.012f);float drape=Mathf.SmoothStep(0,1,Mathf.Clamp01((Mathf.Abs(w-.5f)-.32f)/.18f))*.48f;v[i]=origin+(vertical?new Vector3((u-.5f)*width,-w*length,ripple*2):new Vector3((u-.5f)*width,ripple-drape,(w-.5f)*length));uv[i]=new Vector2(u,w);if(x<nx&&z<nz){int j=i+nx+1;indices.AddRange(new[]{i,j,i+1,i+1,j,j+1,j,i,i+1,j,j+1,i+1});}}
        // Back faces need their own vertices: shared opposite triangles cancel normals.
        var front=new List<int>();for(int i=0;i<indices.Count;i+=12)for(int j=0;j<6;j++)front.Add(indices[i+j]);
        var vv=new Vector3[v.Length*2];var uu=new Vector2[uv.Length*2];System.Array.Copy(v,vv,v.Length);System.Array.Copy(v,0,vv,v.Length,v.Length);System.Array.Copy(uv,uu,uv.Length);System.Array.Copy(uv,0,uu,uv.Length,uv.Length);
        int frontCount=front.Count;for(int i=0;i<frontCount;i+=3)front.AddRange(new[]{front[i+2]+v.Length,front[i+1]+v.Length,front[i]+v.Length});
        var mesh=new Mesh{name=name};mesh.vertices=vv;mesh.uv=uu;mesh.triangles=front.ToArray();mesh.RecalculateNormals();mesh.RecalculateTangents();var o=new GameObject(name);o.transform.SetParent(p,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=material;
    }
    static void Book(Transform p,Vector3 at,int i){var cover=i%3==0?cloth:wood;Box(p,"Book pages",at,new Vector3(.32f,.065f,.42f),paper);Box(p,"Book cover",at+Vector3.up*.04f,new Vector3(.35f,.018f,.44f),cover);}
    static void Lamp(Transform p,Vector3 at){Round(p,"Lamp base",at+Vector3.up*.04f,new Vector3(.3f,.035f,.3f),iron,PrimitiveType.Cylinder);Rod(p,at,at+Vector3.up*.45f,.025f,iron);Round(p,"Dusty lampshade",at+Vector3.up*.58f,new Vector3(.42f,.22f,.42f),cloth,PrimitiveType.Cylinder);}
    public static void Bed(Vector3 pos){
        var g=Group("Iron bed / wake-up anchor",pos);
        ImportedFurniture.Place("old_bed_frame",pos+new Vector3(-.5f,0,.5f),90,1.45f,new Vector3(1.75f,1.45f,3.1f),false);
        var blocker=Box(g,"Bed collision",new Vector3(-.5f,.37f,.5f),new Vector3(3,.64f,1.65f),wood,true);blocker.GetComponent<Renderer>().enabled=false;
        SoftFurnishing.Cushion(g,"Stitched old mattress",new Vector3(-.5f,.68f,.5f),new Vector3(2.96f,.28f,1.62f),bedding);
        SoftFurnishing.Cushion(g,"Linen pillow",new Vector3(-1.45f,.9f,.5f),new Vector3(.72f,.22f,1.16f),bedding);
        Sheet(g,"Soft draped blanket",new Vector3(.05f,.86f,.5f),2.12f,2.40f,cloth);
        var bedside=Group("Bedside cabinet",pos+new Vector3(-1.5f,0,-1.25f));
        ImportedFurniture.Place("ClassicNightstand_01",bedside.position,90,.8f,new Vector3(.72f,.8f,.65f));
        ImportedFurniture.Place("desk_lamp_arm_01",bedside.position+new Vector3(-.13f,.80f,-.07f),125,.56f,null,false);
        Book(bedside,new Vector3(.13f,.837f,.04f),2);
    }
    static void Rug(Transform g,Vector3 p,float w,float d){Box(g,"Faded carpet",p,new Vector3(w,.014f,d),carpet);for(int k=0;k<3;k++){float a=.05f+k*.08f;Box(g,"Carpet border",p+new Vector3(-w/2+a,.012f,0),new Vector3(.035f,.004f,d-a*2),cream);Box(g,"Carpet border",p+new Vector3(w/2-a,.012f,0),new Vector3(.035f,.004f,d-a*2),cream);}}
    static void Shelf(Vector3 pos,float yaw=0){if(ImportedFurniture.Place("wooden_bookshelf_worn",pos,yaw,2.8f,new Vector3(2.1f,2.8f,.65f)))return;var g=Group("Tall bookcase",pos,yaw);Box(g,"Back",new Vector3(0,1.4f,.25f),new Vector3(2.1f,2.8f,.08f),wood,true);for(int side=-1;side<=1;side+=2)Box(g,"Side",new Vector3(side*1.05f,1.4f,0),new Vector3(.12f,2.8f,.65f),wood,true);for(int row=0;row<5;row++){Box(g,"Shelf",new Vector3(0,.15f+row*.6f,0),new Vector3(2.2f,.09f,.7f),wood);for(int i=0;i<8;i++){var book=Box(g,"Book spine",new Vector3(-.87f+i*.23f,.39f+row*.6f,0),new Vector3(.12f,.4f+(i%3)*.04f,.38f),i%3==0?cream:cloth);book.transform.localRotation=Quaternion.Euler(0,0,(i%3-1)*7);}}}
    static void Chair(Vector3 pos,float yaw=0,bool rocking=false){if(ImportedFurniture.Place(rocking?"Rockingchair_01":"WoodenChair_01",pos,yaw,rocking?1.4f:1.05f))return;var g=Group(rocking?"Rocking chair":"Dining chair",pos,yaw);Box(g,"Seat collision",new Vector3(0,.55f,0),new Vector3(.85f,.2f,.85f),wood,true);Round(g,"Upholstery",new Vector3(0,.68f,0),new Vector3(.75f,.14f,.8f),cloth);var back=Box(g,"Upholstered back",new Vector3(0,1.15f,.38f),new Vector3(.85f,1.05f,.16f),cloth);back.transform.localRotation=Quaternion.Euler(-10,0,0);
        foreach(float x in new[]{-.34f,.34f}){Rod(g,new Vector3(x,.45f,-.3f),new Vector3(x,.05f,.45f),.035f,wood);Rod(g,new Vector3(x,.5f,.3f),new Vector3(x,.05f,-.45f),.035f,wood);Rod(g,new Vector3(x,.6f,0),new Vector3(x,.92f,0),.025f,wood);Rod(g,new Vector3(x,.92f,-.36f),new Vector3(x,.95f,.36f),.04f,wood);if(rocking){var rail=new List<Vector3>();for(int j=0;j<15;j++){float z=-.7f+j*.1f;rail.Add(new Vector3(x,.04f+z*z*.3f,z));}Tube(g,rail,.045f,wood);}}
    }
    static void Window(Vector3 pos,float yaw=0,bool curtains=true){var g=Group("Moonlit window",pos,yaw);var pane=Box(g,"Dusty window glass",new Vector3(0,2.25f,0),new Vector3(2.05f,2.55f,.035f),glass);pane.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        Rod(g,new Vector3(.42f,3.5f,-.035f),new Vector3(.25f,3.05f,-.035f),.003f,cream);Rod(g,new Vector3(.25f,3.05f,-.035f),new Vector3(.58f,2.8f,-.035f),.003f,cream);Rod(g,new Vector3(.25f,3.05f,-.035f),new Vector3(.05f,2.6f,-.035f),.002f,cream);foreach(float x in new[]{-1.08f,0,1.08f})Box(g,"Window stile",new Vector3(x,2.25f,-.06f),new Vector3(.08f,2.75f,.1f),cream);foreach(float y in new[]{.87f,2.25f,3.63f})Box(g,"Window rail",new Vector3(0,y,-.06f),new Vector3(2.25f,.08f,.1f),cream);Box(g,"Window sill",new Vector3(0,.87f,-.2f),new Vector3(2.4f,.12f,.45f),wood);
        if(!curtains)return;
        Sheet(g,"Curtain left",new Vector3(-1.25f,3.7f,-.16f),.68f,3.0f,cloth,true);Sheet(g,"Curtain right",new Vector3(1.25f,3.7f,-.16f),.68f,3.0f,cloth,true);
    }
    static void Frame(Transform g,Vector3 p,float yaw,float w= .75f,float h=1.0f){var frame=new GameObject("Faded family portrait").transform;frame.SetParent(g,false);frame.localPosition=p;frame.localRotation=Quaternion.Euler(0,yaw,0);Box(frame,"Frame",Vector3.zero,new Vector3(w,h,.07f),wood);
        if(portraits==null){portraits=new Material[4];var texture=Resources.Load<Texture2D>("Art/FamilyPortraits");for(int i=0;i<4;i++){portraits[i]=Flat(new Color(.85f,.81f,.75f));portraits[i].mainTexture=texture;portraits[i].mainTextureScale=Vector2.one*.495f;portraits[i].mainTextureOffset=new Vector2(i%2*.5f+.002f,i/2*.5f+.002f);}}
        var mesh=new Mesh{name="Portrait canvas"};float x=(w-.12f)/2,y=(h-.12f)/2;mesh.vertices=new[]{new Vector3(-x,-y,-.045f),new Vector3(-x,y,-.045f),new Vector3(x,y,-.045f),new Vector3(x,-y,-.045f)};mesh.uv=new[]{Vector2.zero,Vector2.up,Vector2.one,Vector2.right};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateNormals();var canvas=new GameObject("Oil portrait");canvas.transform.SetParent(frame,false);canvas.AddComponent<MeshFilter>().sharedMesh=mesh;canvas.AddComponent<MeshRenderer>().sharedMaterial=portraits[portraitId++%4];
        foreach(float side in new[]{-1f,1f}){Box(frame,"Inner frame bevel",new Vector3(side*(w/2-.045f),0,-.045f),new Vector3(.018f,h-.05f,.018f),cream);Box(frame,"Inner frame bevel",new Vector3(0,side*(h/2-.045f),-.045f),new Vector3(w-.05f,.018f,.018f),cream);}
    }
    static void Corridor(int f,RoomHeirlooms furnishings){float y=f*HouseMap.Height;var g=Group("Wainscoted hall / floor "+f,new Vector3(0,y,0));
        for(int side=-1;side<=1;side+=2)for(int r=1;r<12;r++){if(r==2||r==6||r==10)continue;float z=(r-6)*2.4f;Box(g,"Wood panelling",new Vector3(side*3.575f,.6f,z),new Vector3(.07f,1.2f,2.4f),wood);for(int k=0;k<6;k++)Box(g,"Panelling stile",new Vector3(side*3.52f,.6f,z-1+k*.4f),new Vector3(.05f,1.16f,.025f),iron);Box(g,"Chair rail",new Vector3(side*3.51f,1.25f,z),new Vector3(.14f,.07f,2.4f),wood);Box(g,"Crown moulding",new Vector3(side*3.5f,4.03f,z),new Vector3(.18f,.16f,2.4f),cream);if(r%2==1)Frame(g,new Vector3(side*3.47f,2.35f,z),side<0?-90:90);}
        Window(new Vector3(0,y,-13.16f),180);Rug(g,new Vector3(0,.025f,0),2.4f,6);Rug(g,new Vector3(0,.025f,9),2,5);Rug(g,new Vector3(0,.025f,-9),2,5);
        furnishings.HallCabinet(root,f);
        for(int i=0;i<14;i++)Book(g,new Vector3(-2.9f,.07f+i*.07f,5),i);
    }
    static void Kitchen(RoomHeirlooms furnishings){var g=Group("Kitchen / cupboards and sink",new Vector3(-9.6f,0,0));
        for(int k=0;k<3;k++){float z=-2+k*1.45f;Box(g,"Kitchen cabinet",new Vector3(-2.7f,.46f,z),new Vector3(1.1f,.92f,1.4f),cream,true);Box(g,"Worktop",new Vector3(-2.7f,.97f,z),new Vector3(1.18f,.1f,1.44f),stone);Box(g,"Inset cupboard",new Vector3(-2.12f,.45f,z),new Vector3(.03f,.72f,1.15f),wood);Rod(g,new Vector3(-2.06f,.65f,z-.2f),new Vector3(-2.06f,.65f,z+.2f),.025f,iron);}
        Tube(g,new List<Vector3>{new Vector3(-3,1,-.5f),new Vector3(-3,1.5f,-.5f),new Vector3(-2.7f,1.5f,-.5f),new Vector3(-2.7f,1.3f,-.5f)},.025f,iron);
        var fridge=Box(g,"Old refrigerator",new Vector3(2.5f,1.15f,-2.4f),new Vector3(1.2f,2.3f,1.25f),cream,true);Box(g,"Fridge seam",new Vector3(2.5f,1.6f,-1.76f),new Vector3(1.18f,.025f,.025f),iron);Rod(g,new Vector3(2,1.05f,-1.73f),new Vector3(2,1.5f,-1.73f),.027f,iron);
        ImportedFurniture.Place("dining_table",g.position+new Vector3(.1f,0,1.6f),0,.96f,new Vector3(2.1f,.96f,1.3f),false);
        var tableTop=Box(g,"Dining tabletop collision",new Vector3(.1f,.9f,1.6f),new Vector3(2.1f,.12f,1.3f),wood,true);
        tableTop.GetComponent<Renderer>().enabled=false;
        Chair(new Vector3(-7.2f,0,2),90);
        furnishings.Kitchen(g);
    }
    public static readonly Vector3 LivingRockerPosition=new Vector3(-9.75f,0,-9.60f);
    public static readonly Vector3 LivingTablePosition=new Vector3(-12.30f,0,-6.57f);
    static void Living(){var g=Group("Living room / fireplace",new Vector3(-9.6f,0,-9.6f));Box(g,"Fireplace hearth",new Vector3(0,.12f,-3.0f),new Vector3(2.5f,.24f,.8f),stone,true);Box(g,"Black firebox",new Vector3(0,.8f,-3.35f),new Vector3(1.65f,1.3f,.07f),iron);foreach(float x in new[]{-1.05f,1.05f})Box(g,"Fireplace column",new Vector3(x,.85f,-3.1f),new Vector3(.32f,1.5f,.55f),cream,true);Box(g,"Mantel",new Vector3(0,1.7f,-3.1f),new Vector3(2.65f,.22f,.7f),cream);Frame(g,new Vector3(0,2.7f,-3.35f),180,1.2f,.95f);Shelf(new Vector3(-12.6f,0,-9.6f),-90);Chair(LivingRockerPosition,180,true);
        var tv=Box(g,"Old television",new Vector3(2.4f,.9f,-2.6f),new Vector3(1.15f,.85f,.65f),wood,true);Rod(g,new Vector3(2.4f,1.3f,-2.6f),new Vector3(2,2,-2.6f),.01f,iron);Rug(g,new Vector3(0,.025f,0),3.1f,3.8f);LivingRoomEffects.Install(g);
    }
    static void Nursery(RoomHeirlooms furnishings){
        var g=Group("Nursery / abandoned toys",new Vector3(-9.6f,4.2f,0));
        for(int i=0;i<4;i++)Book(g,new Vector3(.5f+i*.4f,.06f,2.7f),i);
        Wardrobe.Build(new Vector3(-12.8f,4.2f,2),-90,wood,iron);
        furnishings.Nursery(g);
    }
    static void Study(){var g=Group("Keeper study",new Vector3(-9.6f,4.2f,-9.6f));Shelf(new Vector3(-12.6f,4.2f,-9.6f),-90);Box(g,"Panelled writing desk",new Vector3(0,.6f,-2.5f),new Vector3(2.7f,1.2f,1.1f),wood,true);Box(g,"Desk top",new Vector3(0,1.24f,-2.5f),new Vector3(2.9f,.13f,1.25f),wood);for(int i=0;i<4;i++)Book(g,new Vector3(-.6f+i*.4f,1.34f,-2.5f),i);Lamp(g,new Vector3(1,1.32f,-2.5f));ImportedFurniture.Place("ArmChair_01",new Vector3(-9.6f,4.2f,-10.5f),180,1.3f);Box(g,"Iron safe",new Vector3(2.4f,.65f,-2.5f),new Vector3(1,1.3f,1),iron,true);Round(g,"Safe dial",new Vector3(2.4f,.85f,-1.98f),new Vector3(.18f,.18f,.04f),cream);}
    static void Bathroom(){var g=Group("Bathroom / tiled surfaces",new Vector3(9.6f,4.2f,-10.0f));for(int r=0;r<5;r++)for(int c=0;c<5;c++)Box(g,"Floor tile",new Vector3(-2.4f+c*1.1f,.012f,-2.5f+r*.85f),new Vector3(1.07f,.025f,.82f),(r+c)%2==0?cream:iron);
        Round(g,"Bathtub body",new Vector3(1.6f,.55f,-1.5f),new Vector3(1.3f,.85f,2.7f),cream);Round(g,"Dark tub interior",new Vector3(1.6f,.88f,-1.5f),new Vector3(1.05f,.1f,2.4f),iron);Box(g,"Tub collision",new Vector3(1.6f,.4f,-1.5f),new Vector3(1.25f,.8f,2.5f),cream,true).GetComponent<Renderer>().enabled=false;Tube(g,new List<Vector3>{new Vector3(1.7f,.2f,-2.9f),new Vector3(1.7f,1.3f,-2.9f),new Vector3(1.7f,1.3f,-2.5f)},.035f,iron);Round(g,"Pedestal sink",new Vector3(-2,.8f,-2),new Vector3(.8f,.2f,.65f),cream);Rod(g,new Vector3(-2,0,-2),new Vector3(-2,.8f,-2),.15f,cream);Frame(g,new Vector3(-2,1.9f,-2.5f),180,.8f,1.1f);
    }
    public static void Build(){root=new GameObject("REFERENCE INTERIORS / rooms and exterior").transform;wood=Surface("Wood",new Color(.42f,.35f,.29f));wall=Surface("Plaster",new Color(.66f,.69f,.7f));cloth=Surface("Linen",new Color(.28f,.30f,.28f),3);bedding=Surface("Linen",new Color(.47f,.46f,.39f),2);carpet=Surface("Carpet",new Color(.48f,.40f,.33f),2);iron=Flat(new Color(.12f,.13f,.13f));cream=Flat(new Color(.52f,.52f,.46f));stone=Surface("Stone",new Color(.5f,.53f,.53f));paper=Flat(new Color(.48f,.44f,.34f));roof=Surface("Roof",new Color(.42f,.46f,.43f),5);glass=Flat(new Color(.16f,.23f,.28f,.18f));glass.SetFloat("_Mode",3);glass.SetInt("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);glass.SetInt("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);glass.SetInt("_ZWrite",0);glass.EnableKeyword("_ALPHABLEND_ON");glass.renderQueue=3000;glass.SetFloat("_Glossiness",.38f);
        var furnishings=new RoomHeirlooms();
        for(int f=0;f<2;f++){Corridor(f,furnishings);Bed(HouseMap.Position(HouseMap.Id(f,10,2)));Wardrobe.Build(new Vector3(-7,f*4.2f,7),180,wood,iron);for(int i=0;i<3;i++)Window(new Vector3(-13.16f,f*4.2f,(i*4-4)*2.4f),-90);}
        Kitchen(furnishings);Living();Nursery(furnishings);Study();Bathroom();furnishings.Pantry(Group("Attic / upper east wing",new Vector3(9.6f,4.2f,10)));Exterior();Basement();Details();WallInstruments.Install(root);SafelightRooms.Build(root);furnishings.Disturbances(root);
    }
    static void Exterior(){var g=Group("Moonlit exterior / gables and porch",Vector3.zero);
        Box(g,"Wood siding",new Vector3(15.63f,6.3f,0),new Vector3(.12f,4.2f,31.2f),wood);Box(g,"Stone foundation",new Vector3(15.63f,2.1f,0),new Vector3(.12f,4.2f,31.2f),stone);
        for(int floor=0;floor<2;floor++)for(int row=0;row<13;row++){float y=floor*4.2f,z=(row-6)*2.4f;var finish=floor==0?stone:wood;if(row==2||row==6||row==10){Box(g,"Exterior under window",new Vector3(-15.63f,y+.4875f,z),new Vector3(.12f,.975f,2.4f),finish);Box(g,"Exterior window lintel",new Vector3(-15.63f,y+3.8625f,z),new Vector3(.12f,.675f,2.4f),finish);foreach(float side in new[]{-1f,1f})Box(g,"Exterior window reveal",new Vector3(-15.63f,y+2.25f,z+side*1.1125f),new Vector3(.12f,2.55f,.175f),finish);}else Box(g,"Exterior masonry bay",new Vector3(-15.63f,y+2.1f,z),new Vector3(.12f,4.2f,2.4f),finish);}
        for(int side=-1;side<=1;side+=2){Box(g,"Facade stone",new Vector3(side*8.65f,2.05f,15.63f),new Vector3(13.9f,4.1f,.12f),stone);Box(g,"Facade timber",new Vector3(side*8,6.3f,15.63f),new Vector3(15.2f,4.2f,.12f),wood);}
        Box(g,"Rear timber",new Vector3(0,6.3f,-15.63f),new Vector3(31.2f,4.2f,.12f),wood);Box(g,"Rear stone",new Vector3(0,2.05f,-15.63f),new Vector3(31.2f,4.1f,.12f),stone);
        // Two pitched roof planes, central ridge and triangular gable fills.
        foreach(int side in new[]{-1,1}){var slab=Box(g,"Pitched slate roof",new Vector3(side*8,11.2f,0),new Vector3(17,.18f,33),roof);slab.transform.localRotation=Quaternion.Euler(0,0,side*-20);}
        var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-15.6f,8.4f,15.6f),new Vector3(15.6f,8.4f,15.6f),new Vector3(0,14.2f,15.6f),new Vector3(-15.6f,8.4f,-15.6f),new Vector3(15.6f,8.4f,-15.6f),new Vector3(0,14.2f,-15.6f)};mesh.uv=new[]{Vector2.zero,Vector2.right*5,Vector2.up*2,Vector2.zero,Vector2.right*5,Vector2.up*2};mesh.triangles=new[]{0,1,2,3,5,4};mesh.RecalculateNormals();mesh.RecalculateTangents();var go=new GameObject("Gables");go.transform.SetParent(g,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=wood;
        Box(g,"Chimney",new Vector3(-6,12,-6),new Vector3(1.1f,5,1.3f),stone);Box(g,"Porch deck",new Vector3(0,-.06f,17.1f),new Vector3(6.2f,.12f,4),wood,true);
        for(int side=-1;side<=1;side+=2){Rod(g,new Vector3(side*2.8f,0,18.5f),new Vector3(side*2.8f,3.4f,18.5f),.12f,wood);for(int i=0;i<8;i++)Rod(g,new Vector3(side*2.8f,.15f,15.8f+i*.4f),new Vector3(side*2.8f,1.05f,15.8f+i*.4f),.035f,wood);Rod(g,new Vector3(side*2.8f,1.1f,15.8f),new Vector3(side*2.8f,1.1f,18.6f),.06f,wood);}
        var awning=Box(g,"Porch roof",new Vector3(0,3.6f,17.1f),new Vector3(6.8f,.15f,4.4f),roof);awning.transform.localRotation=Quaternion.Euler(8,0,0);
        foreach(float x in new[]{-11f,-5.5f,5.5f,11f}){Window(new Vector3(x,4.2f,15.72f),180,false);Window(new Vector3(x,.1f,15.72f),180,false);}
        ForestYard.Build();
        var moonMaterial=Flat(new Color(.65f,.73f,.8f));moonMaterial.EnableKeyword("_EMISSION");moonMaterial.SetColor("_EmissionColor",new Color(.4f,.5f,.65f));var moon=Round(g,"Moon disc",new Vector3(-35,50,-40),Vector3.one*7,moonMaterial);moon.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
    }
    static void Basement(){var g=Group("Basement / supplies",Vector3.zero);var p=new Vector3(25.2f,-3.6f,9.6f);Box(g,"Cellar floor",p-Vector3.up*.12f,new Vector3(7.2f,.24f,7.2f),stone,true);Box(g,"Cellar ceiling",p+Vector3.up*3.2f,new Vector3(7.5f,.2f,7.5f),stone,true);
        foreach(float z in new[]{6f,13.2f})Box(g,"Cellar wall",new Vector3(25.2f,-2,z),new Vector3(7.5f,3.2f,.2f),stone,true);Box(g,"Cellar wall",new Vector3(28.8f,-2,9.6f),new Vector3(.2f,3.2f,7.2f),stone,true);
        foreach(float z in new[]{7.15f,12.05f})Box(g,"Cellar entry wall",new Vector3(21.6f,-2,z),new Vector3(.2f,3.2f,2.3f),stone,true);
        var ramp=Box(g,"Cellar stair collision",new Vector3(19.2f,-1.9f,9.6f),new Vector3(8.1f,.18f,2.25f),wood,true);ramp.transform.localRotation=Quaternion.Euler(0,0,-26.565f);ramp.GetComponent<Renderer>().enabled=false;
        for(int i=0;i<20;i++)Box(g,"Cellar stair tread",new Vector3(15.6f+i*.36f,-i*.18f-.1f,9.6f),new Vector3(.38f,.2f,2.25f),wood);
        foreach(float z in new[]{8.35f,10.85f}){var w=Box(g,"Cellar stair wall",new Vector3(19.2f,-.4f,z),new Vector3(8.1f,2.9f,.18f),stone,true);w.transform.localRotation=Quaternion.Euler(0,0,-26.565f);}
        var ceiling=Box(g,"Cellar stair ceiling",new Vector3(19.2f,1.05f,9.6f),new Vector3(8.1f,.16f,2.7f),wood,true);ceiling.transform.localRotation=Quaternion.Euler(0,0,-26.565f);
        Shelf(new Vector3(25,-3.6f,6.3f));for(int i=0;i<6;i++)Round(g,"Preserving jar",new Vector3(23.8f+i*.4f,-2.65f,6.25f),new Vector3(.18f,.24f,.18f),glass,PrimitiveType.Cylinder);
        var bulb=new GameObject("Cellar warm bulb").AddComponent<Light>();bulb.transform.position=new Vector3(25.2f,-.8f,9.6f);bulb.type=LightType.Point;bulb.color=new Color(1,.68f,.35f);bulb.range=8;bulb.intensity=1.8f;
    }
}
}





