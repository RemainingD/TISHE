using UnityEngine;
using static Tishe.ReferenceArt;

namespace Tishe {
// Mounts use local +Z into the room. Only the shallow panel body receives the use ray.
public sealed class WallInstruments {
    static readonly string[] titles={"Электр қалқаны","Есіктің механикалық құлпы","Күзетшінің ойын автоматы","Желдеткіш","Радионы баптау панелі"};
    static readonly string[] items={"Сақтандырғыш","Тісті дөңгелек","Жетон","Ауа датчигі","Радиоқабылдағыш"};
    static readonly string[] ids={"fuse","gear","token","sensor","radio"};
    readonly Material oak=Surface("Wood",new Color(.52f,.32f,.21f));
    readonly Material brass=Metal("Brushed brass",new Color(.63f,.46f,.23f),.48f);
    readonly Material black=Metal("Aged iron",new Color(.12f,.14f,.14f),.36f);
    readonly Material enamel=Metal("Grey green enamel",new Color(.29f,.40f,.36f),.25f);
    readonly Material ivory=Metal("Porcelain",new Color(.82f,.76f,.61f),.08f);
    readonly Material oxblood=Metal("Bakelite",new Color(.41f,.14f,.085f),.12f);
    static Material Metal(string name,Color c,float metallic){
        var m=new Material(Shader.Find("Standard")){name=name,color=c};m.SetFloat("_Metallic",metallic);m.SetFloat("_Glossiness",.35f);return m;
    }
    static Vector3 V(float x,float y,float z)=>new Vector3(x,y,z);
    static Transform Mount(Transform parent,string name,Vector3 pos,float yaw){
        var t=new GameObject(name).transform;t.SetParent(parent,false);t.SetPositionAndRotation(pos,Quaternion.Euler(0,yaw,0));return t;
    }
    static void Disk(Transform t,string name,Vector3 p,float diameter,float depth,Material mat){
        var r=Round(t,name,p,V(diameter,depth*.5f,diameter),mat,PrimitiveType.Cylinder);
        r.transform.localRotation=Quaternion.Euler(90,0,0);
    }
    internal static void PrintedFace(Transform parent,Vector3 at,float width,float height,int cell){
        var m=Metal("Printed surface "+cell,Color.white,.06f);
        m.mainTexture=Resources.Load<Texture2D>("Art/InstrumentPlates");
        m.mainTextureScale=new Vector2(1f/3-.004f,.496f);
        m.mainTextureOffset=new Vector2(cell%3/3f+.002f,(cell<3?.5f:0)+.002f);
        m.SetFloat("_Glossiness",cell==5?.07f:.21f);
        var face=GameObject.CreatePrimitive(PrimitiveType.Quad);
        face.name=cell==5?"Soot-darkened brick":"Maker's faceplate";face.transform.SetParent(parent,false);
        face.transform.localPosition=at;face.transform.localRotation=Quaternion.Euler(0,180,0);
        face.transform.localScale=V(width,height,1);
        SceneObjects.Remove(face.GetComponent<Collider>());face.GetComponent<Renderer>().sharedMaterial=m;
    }
    void Dial(Transform t,Vector3 c,float d,float angle){
        Disk(t,"Bezel",c,d,.032f,brass);c.z+=.021f;Disk(t,"Ivory dial",c,d*.86f,.018f,ivory);c.z+=.012f;
        for(int i=0;i<=10;i++){
            float a=(i*25-125)*Mathf.Deg2Rad;var dir=V(Mathf.Sin(a),Mathf.Cos(a),0);
            Rod(t,c+dir*d*.31f,c+dir*d*.378f,.005f,black);
        }
        var pointer=Quaternion.Euler(0,0,angle)*Vector3.up*d*.32f;
        Rod(t,c,c+pointer,.0065f,oxblood);Disk(t,"Needle pivot",c+Vector3.forward*.003f,d*.082f,.016f,black);
    }
    static void Item(Transform t,string kind,string name,string id,int n){
        var item=t.gameObject.AddComponent<GameItem>();item.kind=kind;item.title=name;item.id=id;item.taskIndex=n;
    }
    void Panel(Transform g,int n,Vector3 p,float yaw,float width,float height,Material body){
        var t=Mount(g,titles[n],p,yaw);Item(t,"station",titles[n],"",n);
        var collider=t.gameObject.AddComponent<BoxCollider>();collider.center=V(0,0,.115f);collider.size=V(width,height,.23f);
        Box(t,"Flanged mounting rim",V(0,0,.032f),V(width,height,.07f),n==2||n==4?oak:black);
        Box(t,"Recessed instrument face",V(0,0,.084f),V(width-.085f,height-.085f,.06f),body);
        PrintedFace(t,V(0,0,.116f),width-.10f,height-.10f,n);
        foreach(int side in new[]{-1,1})foreach(int end in new[]{-1,1}){
            var c=V(side*(width/2-.067f),end*(height/2-.067f),.12f);
            Disk(t,"Slotted screw",c,.031f,.013f,brass);Rod(t,c+V(-.009f,-.004f,.008f),c+V(.009f,.004f,.008f),.0024f,black);
        }
        switch(n){
            case 0:
                Dial(t,V(0,.20f,.137f),.29f,-26);
                for(int i=0;i<3;i++){
                    float x=(i-1)*.195f;
                    foreach(float y in new[]{-.24f,-.018f})Disk(t,"Ceramic fuse socket",V(x,y,.137f),.093f,.048f,ivory);
                    Rod(t,V(x,-.24f,.176f),V(x,-.018f,.176f),.031f,i==1?black:brass);
                    if(i!=1)Rod(t,V(x,-.186f,.177f),V(x,-.07f,.177f),.033f,ivory);
                }
                Rod(t,V(0,height/2,.06f),V(0,height/2+.48f,.06f),.023f,black);
                break;
            case 1:
                Box(t,"Escutcheon",V(0,.065f,.127f),V(.88f,.27f,.035f),brass);
                for(int i=0;i<4;i++){
                    var c=V(-.31f+i*.205f,.065f,.158f);
                    Disk(t,"Combination wheel",c,.178f,.052f,black);
                    for(int j=0;j<8;j++){
                        float a=j*Mathf.PI*.25f;var dir=V(Mathf.Sin(a),Mathf.Cos(a),0);
                        Rod(t,c+dir*.057f+Vector3.forward*.029f,c+dir*.077f+Vector3.forward*.029f,.006f,ivory);
                    }
                    Disk(t,"Hub",c+Vector3.forward*.029f,.055f,.017f,brass);
                }
                Rod(t,V(-.32f,-.16f,.17f),V(.32f,-.16f,.17f),.027f,brass);
                break;
            case 2:
                Box(t,"Slate game board",V(-.06f,.035f,.127f),V(.49f,.49f,.028f),black);
                for(int i=-1;i<=1;i+=2){
                    Rod(t,V(-.28f,.035f+i*.078f,.148f),V(.16f,.035f+i*.078f,.148f),.006f,brass);
                    Rod(t,V(-.06f+i*.078f,-.19f,.148f),V(-.06f+i*.078f,.25f,.148f),.006f,brass);
                }
                foreach(var offset in new[]{V(-.21f,.19f,.151f),V(-.06f,.035f,.151f)}){
                    Rod(t,offset+V(-.039f,-.039f,0),offset+V(.039f,.039f,0),.008f,ivory);
                    Rod(t,offset+V(-.039f,.039f,0),offset+V(.039f,-.039f,0),.008f,ivory);
                }
                Disk(t,"Token escutcheon",V(.275f,.07f,.141f),.139f,.024f,brass);
                Box(t,"Coin slot",V(.275f,.07f,.158f),V(.018f,.081f,.012f),black);
                Disk(t,"Play key",V(.275f,-.15f,.15f),.105f,.055f,oxblood);break;
            case 3:
                Disk(t,"Cast grille surround",V(-.20f,0,.126f),.48f,.043f,brass);
                Disk(t,"Air duct",V(-.20f,0,.153f),.444f,.022f,black);
                for(int j=-3;j<=3;j++){
                    float y=j*.051f,half=Mathf.Sqrt(.204f*.204f-y*y);
                    var slat=Box(t,"Vent louver",V(-.20f,y,.183f),V(2*half,.029f,.032f),enamel);
                    slat.transform.localRotation=Quaternion.Euler(-22,0,0);
                }
                Dial(t,V(.23f,.09f,.135f),.224f,35);
                for(int j=0;j<3;j++)Disk(t,"Channel valve",V(.11f+j*.11f,-.143f,.144f),.081f,.048f,j==1?oxblood:brass);
                break;
            case 4:
                Box(t,"Tuning glass",V(0,.135f,.13f),V(.67f,.125f,.026f),ivory);
                for(int j=0;j<22;j++){float x=-.3f+j*.028f;Rod(t,V(x,.104f,.149f),V(x,j%5==0?.161f:.134f,.149f),.0028f,black);}
                Rod(t,V(.08f,.09f,.151f),V(.08f,.181f,.151f),.006f,oxblood);
                for(int j=0;j<6;j++)Box(t,"Speaker slot",V(0,-.034f-j*.025f,.124f),V(.40f,.012f,.014f),black);
                foreach(float x in new[]{-.32f,.32f})Disk(t,"Tuning knob",V(x,-.096f,.148f),.105f,.059f,brass);
                break;
        }
    }
    void Shelf(Transform decor,Transform loose,int i,Vector3 p,float yaw){
        var ledge=Mount(decor,"Item ledge / "+ids[i],p,yaw);
        Box(ledge,"Lipped shelf",V(0,0,.164f),V(.55f,.04f,.33f),oak);
        foreach(float x in new[]{-.19f,.19f})Rod(ledge,V(x,-.165f,.01f),V(x,-.019f,.257f),.012f,black);
        var t=Mount(loose,items[i],ledge.TransformPoint(V(0,.145f,.17f)),yaw);
        Item(t,"pickup",items[i],ids[i],-1);
        var c=t.gameObject.AddComponent<BoxCollider>();c.size=V(.31f,.25f,.20f);
        switch(i){
            case 0:
                Rod(t,V(-.125f,-.081f,0),V(.125f,-.081f,0),.043f,ivory);
                foreach(float x in new[]{-.117f,.117f})Rod(t,V(x-.022f,-.081f,0),V(x+.022f,-.081f,0),.045f,brass);
                break;
            case 1:
                Disk(t,"Gear web",Vector3.zero,.217f,.039f,brass);
                for(int j=0;j<12;j++){float a=j*30*Mathf.Deg2Rad;var tooth=Box(t,"Gear tooth",V(.111f*Mathf.Sin(a),.111f*Mathf.Cos(a),0),V(.034f,.034f,.04f),brass);tooth.transform.localRotation=Quaternion.Euler(0,0,-j*30);}
                Disk(t,"Axle bore",V(0,0,.022f),.064f,.006f,black);break;
            case 2: Disk(t,"Stamped token",V(0,-.027f,0),.19f,.021f,brass);Disk(t,"Token relief",V(0,-.027f,.014f),.144f,.009f,oxblood);StarStamp(t);break;
            case 3:
                Box(t,"Meter casing",V(0,-.012f,0),V(.22f,.213f,.085f),enamel);Dial(t,V(0,.022f,.051f),.132f,25);break;
            case 4:
                Box(t,"Portable radio",V(0,-.027f,0),V(.32f,.194f,.12f),oak);
                Disk(t,"Portable speaker",V(-.071f,-.027f,.063f),.136f,.012f,black);
                Disk(t,"Volume",V(.08f,-.04f,.064f),.065f,.024f,brass);
                Rod(t,V(.115f,.062f,0),V(.17f,.225f,0),.007f,brass);break;
        }
    }
    void StarStamp(Transform t){
        for(int i=0;i<3;i++){float a=i*60*Mathf.Deg2Rad;var d=V(Mathf.Sin(a),Mathf.Cos(a),0)*.045f;
            var p=V(0,-.027f,.021f);Rod(t,p-d,p+d,.005f,brass);}
    }
    public static void Install(Transform decor){
        var kit=new WallInstruments();
        var g=Mount(decor,"Wall instruments",Vector3.zero,0);
        var loose=Mount(null,"Loose quest objects",Vector3.zero,0);
        kit.Panel(g,0,V(-11.72f,1.52f,3.57f),180,.91f,1.00f,kit.enamel);
        kit.Panel(g,1,V(-11.76f,5.67f,-6.03f),180,1.05f,.58f,kit.oak);
        kit.Panel(g,2,V(-6.03f,5.66f,2.30f),270,.91f,.78f,kit.oak);
        kit.Panel(g,3,V(13.16f,5.72f,-9.44f),270,1.12f,.69f,kit.enamel);
        kit.Panel(g,4,V(13.16f,1.50f,-10.28f),270,.96f,.59f,kit.oak);
        kit.Shelf(g,loose,0,V(27.66f,-2.40f,13.08f),180);
        kit.Shelf(g,loose,1,V(-7.43f,5.41f,-3.56f),0);
        kit.Shelf(g,loose,2,V(-6.03f,1.18f,-7.52f),270);
        kit.Shelf(g,loose,3,V(-11.78f,5.39f,-13.16f),0);
        kit.Shelf(g,loose,4,V(13.16f,1.10f,-11.20f),270);
    }
}
}
