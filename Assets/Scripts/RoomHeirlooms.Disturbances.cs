using System.Collections.Generic;
using UnityEngine;
using static Tishe.ReferenceArt;

namespace Tishe {
public sealed partial class RoomHeirlooms {
    public const string DisturbanceGroup="Traces of the household";
    public static readonly Vector3 StairCupboardFeet=new Vector3(9.60f,0,-7.94f);
    public void Disturbances(Transform decor){
        if(decor.Find(DisturbanceGroup)!=null)return;
        var root=Part(decor,DisturbanceGroup,Vector3.zero);
        // The underside of the last flight has non-solid display treads above a ramp.
        // Close the ground-floor end below the landing, leaving the stair surface clear.
        Box(root,"Understairs partition",V(9.60f,1.86f,-7.33f),V(2.40f,3.72f,.14f),green,true);
        Wardrobe.Build(StairCupboardFeet,0,oak,brass);
        var mud=Surface("Stone",new Color(.27f,.21f,.15f));
        mud.SetFloat("_Glossiness",.25f);
        var sole=BootImprint();
        // A trail turns away from the radio and ends at the closed wardrobe doors.
        Vector3[] printsToCupboard={V(12.12f,.008f,-11.38f),V(11.70f,.008f,-11.25f),V(11.29f,.008f,-10.75f),
            V(10.76f,.008f,-10.56f),V(10.46f,.008f,-10.04f),V(9.97f,.008f,-9.73f),V(9.82f,.008f,-9.11f)};
        for(int i=0;i<printsToCupboard.Length;i++){
            var foot=Part(root,"Dried boot print",printsToCupboard[i]);
            foot.localRotation=Quaternion.Euler(0,-67+i*8,0);
            foot.localScale=V(i%2==0?1:-1,1,.95f+(i%3)*.035f);
            foot.gameObject.AddComponent<MeshFilter>().sharedMesh=sole;foot.gameObject.AddComponent<MeshRenderer>().sharedMaterial=mud;
        }
        var grille=Part(root,"Hall ventilation grille",HorrorAudio.ReturnGrille);
        grille.localRotation=Quaternion.Euler(0,-90,0);
        Box(grille,"Cast iron bezel",Vector3.zero,V(.69f,.42f,.055f),brass);
        Box(grille,"Dark duct opening",V(0,0,.032f),V(.62f,.355f,.025f),iron);
        for(int i=0;i<6;i++)Box(grille,"Angled louver",V(0,(i-2.5f)*.053f,.055f),V(.58f,.025f,.043f),green).transform.localRotation=Quaternion.Euler(-27,0,0);

        var prints=Resources.Load<Texture2D>("Art/FamilyPortraits");
        var family=Print(prints,1,1);var sitter=Print(prints,0,0);
        var hall=Part(root,"Fallen family portrait",V(-2.23f,0,6.67f));
        hall.localRotation=Quaternion.Euler(0,-23,0);
        Box(hall,"Frame backing",V(0,.031f,0),V(.56f,.049f,.71f),oak);
        FlatPrint(hall,V(0,.059f,0),.47f,.61f,0,family);
        for(int i=-1;i<=1;i+=2){
            Box(hall,"Gilt frame moulding",V(i*.263f,.063f,0),V(.04f,.055f,.71f),brass);
            Box(hall,"Gilt frame moulding",V(0,.063f,i*.34f),V(.50f,.05f,.035f),brass);
        }
        FlatPrint(hall,V(.52f,.012f,-.21f),.19f,.24f,31,sitter);
        FlatPrint(hall,V(.43f,.019f,-.38f),.17f,.19f,-18,recipe);

        var spill=Part(root,"Broken blue saucer",V(-11.35f,0,1.40f));
        for(int i=0;i<5;i++){
            float a=i*72*Mathf.Deg2Rad;
            var sherd=Part(spill,"Glazed porcelain shard",V(Mathf.Cos(a)*(.12f+i*.021f),.012f,Mathf.Sin(a)*(.10f+i*.012f)));
            sherd.localRotation=Quaternion.Euler(0,-i*72+(i%2)*8,0);
            SaucerShard(sherd,ivory,blue);
        }
        var cup=Part(spill,"Spilled enamel cup",V(.40f,.094f,.03f));
        cup.localRotation=Quaternion.Euler(84,23,32);
        Turn(cup,"Enamel cup with visible interior",Vector3.zero,enamel,0,0,.061f,0,.081f,.135f,.081f,.143f,.072f,.143f,.053f,.012f,0,.012f);
        var grip=Part(cup,"Curved cup handle",V(.069f,.077f,0));
        grip.localRotation=Quaternion.Euler(0,0,-90);
        Arch(grip,Vector3.zero,Vector3.right*.044f,.052f,.009f,brass);

        var child=Part(root,"Abandoned pull rabbit",V(-10.80f,4.2f,1.90f));
        child.localRotation=Quaternion.Euler(0,-25,0);
        Box(child,"Wheeled toy base",V(0,.086f,0),V(.43f,.059f,.19f),green);
        foreach(float x in new[]{-.14f,.14f})foreach(float z in new[]{-.114f,.114f}){
            var wheel=Round(child,"Red wooden wheel",V(x,.069f,z),V(.137f,.027f,.137f),rust,PrimitiveType.Cylinder);
            wheel.transform.localRotation=Quaternion.Euler(90,0,0);
        }
        Round(child,"Carved rabbit body",V(-.027f,.23f,0),V(.28f,.28f,.165f),oak);
        Round(child,"Carved rabbit head",V(.10f,.348f,0),V(.167f,.17f,.145f),oak);
        foreach(float z in new[]{-.043f,.043f}){
            var ear=Round(child,"Rabbit ear",V(.085f,.470f,z),V(.060f,.198f,.048f),oak);
            ear.transform.localRotation=Quaternion.Euler(0,0,z>0?15:29);
            Round(child,"Glass eye",V(.132f,.365f,z*1.55f),Vector3.one*.021f,iron);
        }
        Vector3[] cord={V(.21f,.097f,0),V(.34f,.018f,.02f),V(.52f,.012f,.19f),V(.74f,.012f,.23f),V(.91f,.013f,.11f)};
        for(int i=1;i<cord.Length;i++)Rod(child,cord[i-1],cord[i],.006f,linen);
        Round(child,"Wooden pull bead",cord[4]+Vector3.up*.015f,Vector3.one*.055f,rust);
        FlatPrint(child,V(-.33f,.013f,.36f),.27f,.34f,34,drawing);

        var luggage=Part(root,"Unfinished packing",V(-7.43f,4.2f,11.70f));
        luggage.localRotation=Quaternion.Euler(0,-19,0);
        Box(luggage,"Old suitcase base",V(0,.088f,0),V(.85f,.17f,.58f),oak);
        Box(luggage,"Faded fabric interior",V(0,.177f,0),V(.80f,.017f,.53f),linen);
        foreach(float side in new[]{-1f,1f})Box(luggage,"Brass clasp",V(side*.246f,.132f,-.298f),V(.061f,.087f,.021f),brass);
        var lid=Part(luggage,"Open suitcase lid",V(0,.143f,.273f));
        lid.localRotation=Quaternion.Euler(17,0,0);
        Box(lid,"Leather lid",V(0,.269f,0),V(.85f,.55f,.065f),oak);
        Box(lid,"Lined lid",V(0,.269f,-.041f),V(.785f,.492f,.022f),linen);
        for(int i=0;i<3;i++)Box(luggage,"Folded clothing",V(-.19f,.212f+i*.047f,.016f),V(.34f-i*.017f,.048f,.39f),i==1?green:linen).transform.localRotation=Quaternion.Euler(0,i*4,0);
        FlatPrint(luggage,V(.20f,.190f,-.048f),.20f,.29f,-11,sitter);
        FlatPrint(luggage,V(.34f,.012f,-.46f),.18f,.24f,24,family);
    }
    static Mesh BootImprint(){
        var verts=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
        void Patch(float x,float z,float width,float length,float angle){
            int start=verts.Count;
            for(int i=0;i<9;i++){
                float a=i*Mathf.PI*2/9;
                var p=Quaternion.Euler(0,angle,0)*V(Mathf.Cos(a)*width*.5f,0,Mathf.Sin(a)*length*.5f)+V(x,0,z);
                verts.Add(p);uv.Add(new Vector2(p.x*7,p.z*5));
                if(i>1)tris.AddRange(new[]{start,start+i,start+i-1});
            }
        }
        Patch(.005f,-.091f,.082f,.053f,6);
        Patch(.001f,-.055f,.074f,.011f,-7);
        for(int row=0;row<4;row++){
            float z=.010f+row*.024f;
            Patch(-.022f,z,.047f,.018f,19);
            if(row!=1)Patch(.022f,z+.002f,.044f,.018f,-16);
        }
        Patch(-.004f,.105f,.081f,.032f,8);
        var mesh=new Mesh{name="Worn heel and broken tread"};mesh.SetVertices(verts);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);
        mesh.RecalculateNormals();mesh.RecalculateTangents();return mesh;
    }
    static void FlatPrint(Transform parent,Vector3 position,float w,float h,float yaw,Material image){
        var sheet=Part(parent,"Loose print",position);
        sheet.localRotation=Quaternion.Euler(0,yaw,0);
        var face=GameObject.CreatePrimitive(PrimitiveType.Quad);
        face.name="Printed paper";face.transform.SetParent(sheet,false);
        face.transform.localRotation=Quaternion.Euler(90,0,0);face.transform.localScale=V(w,h,1);
        face.GetComponent<Renderer>().sharedMaterial=image;SceneObjects.Remove(face.GetComponent<Collider>());
    }
    static void SaucerShard(Transform p,Material glaze,Material stripe){
        var verts=new List<Vector3>();var faces=new List<int>();var edge=new List<int>();
        float[] radius={.017f,.107f,.146f,.156f,.161f};
        float[] height={0,.008f,.035f,.039f,.037f};
        for(int j=0;j<radius.Length;j++)for(int i=0;i<=6;i++){
            float a=(i*10f-31+(i==0?3*(j%2):0))*Mathf.Deg2Rad;
            verts.Add(V(radius[j]*Mathf.Cos(a),height[j],radius[j]*Mathf.Sin(a)));
            if(i==0||j==0)continue;
            int k=verts.Count-1;(j==3?edge:faces).AddRange(new[]{k-8,k-1,k,k-8,k,k-7});
        }
        var m=new Mesh{name="Curved broken porcelain"};m.SetVertices(verts);m.subMeshCount=2;
        // Clockwise from above, with an exposed blue band on the lip.
        faces.Reverse();edge.Reverse();m.SetTriangles(faces,0);m.SetTriangles(edge,1);m.RecalculateNormals();
        p.gameObject.AddComponent<MeshFilter>().sharedMesh=m;p.gameObject.AddComponent<MeshRenderer>().sharedMaterials=new[]{glaze,stripe};
    }
}
}
