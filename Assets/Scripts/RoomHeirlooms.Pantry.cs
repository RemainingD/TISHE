using UnityEngine;
using static Tishe.ReferenceArt;

namespace Tishe {
public sealed partial class RoomHeirlooms {
    public void HallCabinet(Transform decor,int floor){
        var feet=V(-3.00f,floor*HouseMap.Height,2.52f);
        var keepsakes=Part(decor,"Hall cabinet keepsakes",feet);
        keepsakes.localRotation=Quaternion.Euler(0,90,0);
        const float top=1.35f;
        if(!ImportedFurniture.Place("GothicCabinet_01",feet,90,top)){
            Box(keepsakes,"Oak press",V(0,top*.5f,0),V(.98f,top,.48f),oak,true);
            for(int i=0;i<2;i++)Box(keepsakes,"Fielded oak door",V(-.24f+i*.48f,.66f,.25f),V(.45f,1.12f,.035f),green);
        }
        // Both objects sit on the shut cabinet, well clear of the corridor mouldings.
        Dish(keepsakes,V(.18f,top,-.02f),.64f);
        var stick=Part(keepsakes,"Bedchamber candlestick",V(-.21f,top,-.025f));
        Turn(stick,"Brass foot and stem",Vector3.zero,brass,0,0,.09f,0,.098f,.011f,.076f,.023f,.039f,.038f,.028f,.057f,.031f,.118f,.046f,.131f,.049f,.145f,0,.145f);
        Turn(stick,"Unlit candle",V(0,.141f,0),ivory,0,0,.027f,0,.025f,.128f,.019f,.135f,.008f,.128f,0,.126f);
        Rod(stick,V(0,.267f,0),V(.004f,.278f,0),.002f,iron);
    }

    public void Pantry(Transform room){
        var textures=Resources.Load<Texture2D>("Art/PantryPrints");
        var biscuitPrint=Print(textures,0,1);
        var teaPrint=Print(textures,1,1);
        var flourPrint=Print(textures,0,0);
        var herbPrint=Print(textures,1,0);
        // The door opens from the west. Keep the central aisle and the grid paths along it clear.
        var hutch=Part(room,"Pantry dresser",V(3.25f,0,.86f));
        hutch.localRotation=Quaternion.Euler(0,-90,0);
        const float width=3.42f;
        for(int i=0;i<11;i++)
            Box(hutch,"Painted backboard",V((i-5)*.30f,1.13f,-.272f),V(.297f,2.22f,.046f),green);
        foreach(int side in new[]{-1,1}){
            Box(hutch,"Dresser end",V(side*1.67f,1.12f,-.012f),V(.083f,2.24f,.545f),oak);
            Box(hutch,"Front stile",V(side*1.644f,1.12f,.269f),V(.122f,2.24f,.066f),green);
        }
        Box(hutch,"Stepped cornice",V(0,2.27f,-.008f),V(width+.10f,.075f,.65f),oak);
        Box(hutch,"Cornice bead",V(0,2.216f,.277f),V(width,.036f,.069f),brass);
        foreach(float height in new[]{.21f,.83f,1.46f}){
            Box(hutch,"Pantry shelf",V(0,height,-.004f),V(width,.06f,.58f),oak);
            Box(hutch,"Shelf front lip",V(0,height-.019f,.28f),V(width,.08f,.035f),green);
        }
        var blocker=hutch.gameObject.AddComponent<BoxCollider>();
        blocker.center=V(0,1.14f,0);blocker.size=V(3.46f,2.28f,.61f);

        SlattedBin(hutch,V(-1.04f,.24f,0),flourPrint);
        SlattedBin(hutch,V(.14f,.24f,0),teaPrint);
        Jar(hutch,V(1.00f,.24f,-.036f),.39f,blue);
        Jar(hutch,V(1.33f,.24f,.012f),.33f,enamel);
        PantryTin(hutch,V(-1.30f,.86f,-.03f),.32f,.40f,rust,biscuitPrint,-7);
        PantryTin(hutch,V(-.89f,.86f,.019f),.28f,.33f,enamel,teaPrint,6);
        for(int i=0;i<5;i++)Dish(hutch,V(-.27f,.86f+i*.022f,.007f),.94f);
        Jug(hutch,V(.45f,.86f,-.022f),ivory);
        Jar(hutch,V(1.20f,.86f,.025f),.36f,blue);
        for(int i=0;i<3;i++)Jar(hutch,V(-1.26f+i*.33f,1.49f,(i%2)*.041f),.25f+(i%2)*.08f,i==1?ivory:enamel);
        PantryTin(hutch,V(.026f,1.49f,0),.31f,.34f,enamel,teaPrint,-4);
        PantryTin(hutch,V(.408f,1.49f,.015f),.31f,.29f,rust,biscuitPrint,3);
        Jug(hutch,V(1.17f,1.49f,-.046f),blue);

        var chest=Part(room,"Linen chest",V(-.76f,0,2.74f));
        chest.localRotation=Quaternion.Euler(0,180,0);
        Box(chest,"Trunk carcass",V(0,.365f,0),V(1.54f,.73f,.73f),oak,true);
        Box(chest,"Fitted trunk lid",V(0,.765f,0),V(1.58f,.07f,.76f),oak,true);
        foreach(float x in new[]{-.60f,.60f}){
            Box(chest,"Leather trunk binding",V(x,.391f,.368f),V(.062f,.687f,.018f),iron);
            Box(chest,"Binding over lid",V(x,.801f,0),V(.062f,.011f,.759f),iron);
        }
        Box(chest,"Trunk latch",V(-.39f,.684f,.381f),V(.095f,.19f,.029f),brass);
        LinenRunner(chest);
        var print=Part(room,"Herb drying print",V(-.67f,1.63f,3.145f));
        print.localRotation=Quaternion.Euler(0,180,0);
        Box(print,"Beech frame",Vector3.zero,V(.64f,.78f,.035f),oak);
        PrintFace(print,"Pressed herbs",V(0,0,.02f),Vector3.forward,.58f,.71f,herbPrint);
    }

    void SlattedBin(Transform shelf,Vector3 feet,Material label){
        var bin=Part(shelf,"Shallow provision crate",feet);
        Box(bin,"Crate base",V(0,.024f,0),V(.94f,.048f,.46f),oak);
        foreach(int side in new[]{-1,1}){
            Box(bin,"Crate end",V(side*.452f,.175f,0),V(.036f,.35f,.46f),oak);
            for(int i=0;i<3;i++)Box(bin,"Crate slat",V(0,.065f+i*.108f,side*.219f),V(.88f,.081f,.031f),oak);
        }
        Box(bin,"Label holder",V(0,.173f,.242f),V(.205f,.133f,.014f),brass);
        PrintFace(bin,"Crate label",V(0,.174f,.251f),Vector3.forward,.177f,.107f,label);
    }
    void PantryTin(Transform shelf,Vector3 at,float width,float height,Material paint,Material label,float yaw){
        var tin=Part(shelf,"Provision tin",at);
        tin.localRotation=Quaternion.Euler(0,yaw,0);
        Box(tin,"Tin body",V(0,height*.5f,0),V(width,height,.23f),paint);
        Box(tin,"Tin lid",V(0,height+.009f,0),V(width+.014f,.032f,.243f),brass);
        PrintFace(tin,"Lithographed label",V(0,height*.50f,.1165f),Vector3.forward,width*.90f,height*.82f,label);
    }
    void Jug(Transform shelf,Vector3 at,Material glaze){
        var vessel=Turn(shelf,"Earthenware jug",at,glaze,0,0,.103f,0,.145f,.045f,.168f,.17f,.151f,.254f,.079f,.325f,.062f,.363f,.072f,.39f,.058f,.397f,.052f,.352f);
        var ear=Part(vessel,"Jug handle",V(.096f,.242f,0));ear.localRotation=Quaternion.Euler(0,0,-88);
        Arch(ear,Vector3.zero,V(.075f,0,0),.102f,.012f,glaze);
    }
    void LinenRunner(Transform chest){
        // The runner follows the lid, rolls over its front edge, then hangs against the wood.
        const int across=32,down=40;
        var vertices=new Vector3[(across+1)*(down+1)];var uv=new Vector2[vertices.Length];
        var triangles=new int[across*down*6];
        for(int row=0;row<=down;row++)for(int col=0;col<=across;col++){
            float u=col/(float)across,fall=Mathf.Clamp01((row-24)/16f);
            float ripple=.5f+.5f*Mathf.Sin(u*23+.8f*Mathf.Sin(row*.14f));
            float x=.15f+(u-.5f)*.66f;
            float y=.805f+.0035f*ripple,z;
            if(row<=20)z=Mathf.Lerp(-.30f,.34f,row/20f);
            else if(row<=24){
                float angle=(row-20)*Mathf.PI/8;
                z=.34f+.047f*Mathf.Sin(angle);y-=.017f*(1-Mathf.Cos(angle));
            }else{
                z=.387f+fall*(.009f+.034f*ripple);
                y-=.017f+fall*(.41f+.016f*Mathf.Cos(u*8));
            }
            int n=row*(across+1)+col;vertices[n]=V(x,y,z);uv[n]=new Vector2(u,row/(float)down);
            if(row==down||col==across)continue;
            int i=(row*across+col)*6,next=n+across+1;
            triangles[i]=n;triangles[i+1]=next;triangles[i+2]=n+1;
            triangles[i+3]=n+1;triangles[i+4]=next;triangles[i+5]=next+1;
        }
        var mesh=new Mesh{name="Linen on trunk lid"};mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;
        mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
        var runner=Part(chest,"Linen runner",Vector3.zero);
        runner.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
        runner.gameObject.AddComponent<MeshRenderer>().sharedMaterial=linen;
    }
}
}
