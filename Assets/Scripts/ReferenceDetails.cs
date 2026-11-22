using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace Tishe {
public static partial class ReferenceArt {
    static void Details(){
        var g=Group("Finishing / architectural details",Vector3.zero);
        // Rails follow the actual collision ramp, with a clear central route.
        for(int side=-1;side<=1;side+=2){float x=9.6f+side*1.03f;
            for(int i=0;i<=22;i++){float t=i/22f;Vector3 p=new Vector3(x,4.2f*t,7.2f-14.4f*t);Rod(g,p,p+Vector3.up*.95f,i%11==0?.07f:.025f,wood);if(i%11==0)Round(g,"Stair newel",p+Vector3.up,new Vector3(.18f,.18f,.18f),wood);}
            Rod(g,new Vector3(x,1,7.2f),new Vector3(x,5.2f,-7.2f),.055f,wood);
        }
        for(int f=0;f<2;f++){
            float y=f*4.2f;var room=Group("Bedroom details",new Vector3(-9.6f,y,9.6f));Rug(room,new Vector3(.6f,.022f,1),3.2f,3.4f);Frame(room,new Vector3(0,2.25f,3.53f),0,1,1.25f);
            for(int i=0;i<8;i++)Book(room,new Vector3(-2.25f,.06f+i*.072f,-2.2f),i);
            // Crown and skirting remain shallow so corridors are not narrowed.
            foreach(float z in new[]{-13.16f,-5.96f,-3.56f,3.56f,5.96f,13.16f}){
                int row=z>0?8:4;
                if(Mathf.Abs(z)<12 && HouseMap.Walkable(f,row,2)){
                    foreach(float side in new[]{-1f,1f})
                        Box(g,"Room skirting",new Vector3(-9.6f+side*2.18f,y+.12f,z),new Vector3(2.72f,.24f,.07f),wood);
                }else{
                    Box(g,"Room skirting",new Vector3(-9.6f,y+.12f,z),new Vector3(7.1f,.24f,.07f),wood);
                }
                Box(g,"Room crown",new Vector3(-9.6f,y+4.02f,z),new Vector3(7.1f,.15f,.14f),cream);
            }
            // A low cost web, drawn as thin threads rather than transparent textures.
            var web=Group("Corner cobweb",new Vector3(-13.12f,y+4,13.1f));for(int ray=0;ray<7;ray++){float a=ray*Mathf.PI/12;Rod(web,Vector3.zero,new Vector3(Mathf.Sin(a)*1.4f,-Mathf.Cos(a)*1.4f,-.02f),.0025f,cream);}for(int ring=1;ring<5;ring++){var pts=new List<Vector3>();for(int j=0;j<7;j++){float a=j*Mathf.PI/12;pts.Add(new Vector3(Mathf.Sin(a)*ring*.28f,-Mathf.Cos(a)*ring*.28f,-.02f));}Tube(web,pts,.002f,cream);}
        }
        // Hall clock and recognizable little objects make the room silhouettes distinct.
        var clock=Group("Grandfather clock",new Vector3(2.85f,0,-3.4f));Box(clock,"Clock case",new Vector3(0,1.3f,0),new Vector3(.9f,2.6f,.5f),wood);
        Round(clock,"Dial",new Vector3(0,2,-.27f),new Vector3(.65f,.65f,.025f),paper);Rod(clock,new Vector3(0,2,-.29f),new Vector3(.18f,2.17f,-.29f),.014f,iron);Rod(clock,new Vector3(0,2,-.29f),new Vector3(-.1f,2.23f,-.29f),.01f,iron);Rod(clock,new Vector3(0,1.7f,-.27f),new Vector3(0,.65f,-.27f),.012f,cream);Round(clock,"Pendulum",new Vector3(0,.65f,-.27f),new Vector3(.25f,.25f,.03f),cream);
        // Stove flue / plates, framed nursery sketches, and a worn washroom rail.
        for(int i=0;i<5;i++)Frame(g,new Vector3(-9+i*.5f,6.35f,3.54f),0,.4f,.55f);
        Rod(g,new Vector3(7,6.7f,-12.9f),new Vector3(9,6.7f,-12.9f),.025f,iron);Sheet(g,"Old bath towel",new Vector3(8,6.7f,-12.84f),.85f,1.15f,cloth,true);
        ImportedFurniture.Place("CoffeeTable_01",LivingTablePosition,0,.55f);ExteriorTrim(g);Lighting();
    }
    static void ExteriorTrim(Transform g){
        var siding=Surface("Wood",new Color(.58f,.58f,.54f));siding.mainTextureScale=new Vector2(10,1);
        // Narrow overlapping boards give the facade a readable scale.
        for(int i=0;i<15;i++)Box(g,"Weatherboards",new Vector3(0,4.35f+i*.27f,15.68f),new Vector3(31.2f,.25f,.045f),siding);
        foreach(float x in new[]{-15.65f,15.65f})Box(g,"Corner timber",new Vector3(x,4.3f,15.82f),new Vector3(.28f,8.6f,.26f),wood);
        for(int side=-1;side<=1;side+=2)Rod(g,new Vector3(side*16,8.4f,16.05f),new Vector3(0,14.25f,16.05f),.11f,wood);
        Window(new Vector3(0,9.0f,15.74f),180,false);
        // Two boarded lower windows, and lintels around the entry.
        foreach(float x in new[]{-11f,11f})for(int i=0;i<3;i++){var b=Box(g,"Boarded window",new Vector3(x,1.65f+i*.48f,15.92f),new Vector3(2.65f,.17f,.09f),wood);b.transform.localRotation=Quaternion.Euler(0,0,(i-1)*8);}
        var rng=new System.Random(311);
        for(int i=0;i<165;i++){
            float z=18+(float)rng.NextDouble()*67,x=ForestYard.PathX(z)+(i%2==0?-1:1)*(2.1f+(float)rng.NextDouble()*20);
            float size=.12f+(float)rng.NextDouble()*.55f;
            var rock=Round(g,"Mossy fieldstone",new Vector3(x,ForestYard.GroundY(x,z),z),new Vector3(size*1.8f,size*.68f,size),stone);
            rock.transform.localRotation=Quaternion.Euler(5,i*47,11);
        }
        foreach(int side in new[]{-1,1}){
            for(int i=0;i<5;i++){
                float x=side*(3.4f+i*2.2f),z=26+i*.21f;
                var p=new Vector3(x,ForestYard.GroundY(x,z),z);
                Rod(g,p,p+Vector3.up*1.17f,.085f,wood);
                if(i==4)continue;
                Vector3 end=new Vector3(x+side*2.2f,ForestYard.GroundY(x+side*2.2f,z+.21f),z+.21f);
                foreach(float y in new[]{.38f,.88f})Rod(g,p+Vector3.up*y,end+Vector3.up*(y-.08f),.058f,wood);
            }
            float corner=side*15.83f;
            Rod(g,new Vector3(corner,8.31f,-16),new Vector3(corner,8.31f,16),.12f,iron);
            Rod(g,new Vector3(corner,8.31f,15.92f),new Vector3(corner,.22f,15.92f),.083f,iron);
            for(int k=0;k<5;k++)Rod(g,new Vector3(corner-.14f,1+k*1.55f,15.92f),new Vector3(corner+.14f,1+k*1.55f,15.92f),.033f,iron);
        }
        var faded=Surface("Wood",new Color(.32f,.38f,.35f));
        foreach(float x in new[]{-11f,-5.5f,5.5f,11f}){
            for(int side=-1;side<=1;side+=2){
                float panel=x+side*1.53f;
                for(int slat=0;slat<8;slat++){
                    var shutter=Box(g,"Louvered shutter",new Vector3(panel,5.43f+slat*.29f,15.88f),new Vector3(.75f,.29f,.085f),faded);
                    shutter.transform.localRotation=Quaternion.Euler(-18,0,x==5.5f&&side==1?-5:0);
                }
                foreach(float offset in new[]{-.38f,.38f})Box(g,"Shutter stile",new Vector3(panel+offset,6.45f,15.88f),new Vector3(.08f,2.48f,.13f),wood);
            }
            Box(g,"Stone windowsill",new Vector3(x,5.10f,15.89f),new Vector3(2.52f,.13f,.42f),stone);
        }
        foreach(int side in new[]{-1,1}){
            var p=new Vector3(side*2.8f,2.34f,18.50f);
            Rod(g,p,p+new Vector3(-side*.92f,.95f,0),.079f,wood);
            Rod(g,p,p+new Vector3(0,.95f,-.86f),.079f,wood);
            for(int i=1;i<=6;i++){
                float x=side*i*2.12f;
                Rod(g,new Vector3(x,8.52f,15.77f),new Vector3(x,14.07f-Mathf.Abs(x)*.36f,15.77f),.073f,wood);
            }
        }
        Box(g,"Corbelled chimney crown",new Vector3(-6,14.5f,-6),new Vector3(1.4f,.26f,1.6f),stone);
        Round(g,"Chimney pot",new Vector3(-6,14.91f,-6),new Vector3(.73f,.32f,.73f),iron,PrimitiveType.Cylinder);
        var flame=Flat(new Color(.95f,.62f,.22f));flame.EnableKeyword("_EMISSION");flame.SetColor("_EmissionColor",new Color(1.8f,.77f,.23f));
        var lamp=new Vector3(1.55f,2.4f,16.18f);
        Rod(g,lamp+new Vector3(0,.45f,-.36f),lamp+Vector3.up*.45f,.033f,iron);
        Rod(g,lamp+Vector3.up*.45f,lamp+Vector3.up*.22f,.018f,iron);
        Round(g,"Amber lantern glass",lamp,new Vector3(.23f,.38f,.23f),flame);
        foreach(float y in new[]{-.19f,.19f})Round(g,"Lantern cap",lamp+Vector3.up*y,new Vector3(.34f,.10f,.34f),iron);
        for(int k=0;k<4;k++){var offset=Quaternion.Euler(0,k*90,0)*Vector3.forward*.135f;Rod(g,lamp+offset+Vector3.down*.18f,lamp+offset+Vector3.up*.18f,.014f,iron);}
        var warm=Flat(new Color(.55f,.35f,.13f));warm.EnableKeyword("_EMISSION");warm.SetColor("_EmissionColor",new Color(.22f,.115f,.035f));Box(g,"Single lit upstairs window",new Vector3(5.5f,6.45f,15.735f),new Vector3(1.98f,2.5f,.03f),warm);
    }
    static void Lighting(){
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.17f,.21f,.25f);RenderSettings.fogColor=new Color(.065f,.09f,.115f);RenderSettings.fogDensity=.012f;
        var sky=Shader.Find("Tishe/NightSky");if(sky!=null)RenderSettings.skybox=new Material(sky);
        foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(light.type==LightType.Directional){light.intensity=1.2f;light.color=new Color(.55f,.69f,.85f);light.shadowStrength=.8f;light.shadowBias=.04f;}
        for(int f=0;f<2;f++)foreach(float z in new[]{-9.6f,0,9.6f}){var l=new GameObject("Window shaft light").AddComponent<Light>();l.transform.SetParent(root);l.transform.position=new Vector3(-12.8f,f*4.2f+3,z);l.transform.rotation=Quaternion.LookRotation(new Vector3(1,-.45f,.35f));l.type=LightType.Spot;l.spotAngle=75;l.range=11;l.intensity=3.5f;l.color=new Color(.65f,.79f,1);l.shadows=LightShadows.Hard;l.shadowResolution=LightShadowResolution.Low;l.shadowBias=.03f;}
        var porch=new GameObject("Porch lantern light").AddComponent<Light>();porch.transform.SetParent(root);porch.transform.position=new Vector3(1.5f,2.7f,17);porch.color=new Color(1,.62f,.29f);porch.intensity=1.6f;porch.range=6;
    }
}
}

