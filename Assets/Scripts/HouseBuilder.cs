using System.Collections.Generic;
using UnityEngine;
namespace Tishe {
public static class HouseBuilder {
    static Transform world,architecture;static Material plaster,wood,dark,metal,linen,paper,glass,skin;
    static GameObject Box(string name,Vector3 p,Vector3 scale,Material m,Transform parent=null,bool collision=true) {
        var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(parent==null?world:parent,false);o.transform.localPosition=p;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=m;if(name.Contains("Door")||name.Contains("door")||name.Contains("Table"))BeveledGeometry.Apply(o,scale);if(!collision)SceneObjects.Remove(o.GetComponent<Collider>());return o;
    }
    static GameObject Shape(string name,PrimitiveType type,Vector3 p,Vector3 scale,Material m,Transform parent) {var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=p;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=m;SceneObjects.Remove(o.GetComponent<Collider>());return o;}
    static Material Mat(Color color,bool timber=false,bool textured=false) {
        var m=new Material(Shader.Find("Standard"));m.color=color;m.SetFloat("_Glossiness",.12f);
        if(textured){var tex=new Texture2D(256,256);var random=new System.Random(timber?41:73);var pixels=new Color[256*256];
            for(int y=0;y<256;y++)for(int x=0;x<256;x++){float n=(float)random.NextDouble()*.2f+.8f;if(timber){if(y%32<2||((x+(y/32%2)*80)%160<2))n=.3f;n-=Mathf.PerlinNoise(x*.06f,y*.5f)*.13f;}else{float peel=Mathf.PerlinNoise(x*.06f,y*.06f);if(peel>.69f)n=.74f;else if(peel<.24f)n=1.08f;n-=Mathf.PerlinNoise(x*.15f,y*.012f)*.12f;if(y>170)n*=.72f;if(y==169||y==170)n=.50f;}pixels[y*256+x]=new Color(n,n,n,1);}tex.SetPixels(pixels);tex.Apply();tex.wrapMode=TextureWrapMode.Repeat;tex.anisoLevel=2;m.mainTexture=tex;
        }return m;
    }
    static Vector3 P(int f,int r,int c)=>HouseMap.Position(HouseMap.Id(f,r,c));
    const float FloorThickness=.24f;
    // End the structure below the walking surface at the underside of the floor slab.
    public static void BuildUpperCeiling(Transform parent, Material finish) {
        float span=13*HouseMap.Cell;
        Box("Upper ceiling",new Vector3(0,2*HouseMap.Height,0),new Vector3(span,.16f,span),finish,parent);
    }
    static float WallHeight(int floor)=>HouseMap.Height-(floor==0?FloorThickness:0);
    static void WindowWall(Vector3 p,float cell,float h){
        Box("Wall below window",p+Vector3.up*.4875f,new Vector3(cell,.975f,cell),plaster,architecture);
        Box("Wall above window",p+Vector3.up*((3.525f+h)/2),new Vector3(cell,h-3.525f,cell),plaster,architecture);
        foreach(float s in new[]{-1f,1f})Box("Window masonry jamb",p+new Vector3(0,2.25f,s*1.1125f),new Vector3(cell,2.55f,.175f),plaster,architecture);
        Box("Window safety pane",p+Vector3.right*1.17f+Vector3.up*2.25f,new Vector3(.035f,2.55f,2.05f),glass,architecture).GetComponent<Renderer>().enabled=false;
    }
    public static void Build(GameController game) {
        world=new GameObject("HOUSE • generated from HouseBuilder.cs").transform;architecture=new GameObject("Architecture").transform;architecture.SetParent(world);
        plaster=ReferenceArt.Surface("Plaster",new Color(.66f,.7f,.72f));wood=ReferenceArt.Surface("Floor",new Color(.5f,.45f,.4f));dark=Mat(new Color(.09f,.12f,.11f));metal=Mat(new Color(.32f,.34f,.29f));linen=Mat(new Color(.38f,.39f,.32f));paper=Mat(new Color(.76f,.69f,.50f));skin=Mat(new Color(.55f,.55f,.43f));
        glass=Mat(new Color(.25f,.37f,.46f));glass.EnableKeyword("_EMISSION");glass.SetColor("_EmissionColor",new Color(.17f,.26f,.33f));
        float cell=HouseMap.Cell,h=HouseMap.Height;
        for(int f=0;f<2;f++)for(int r=0;r<13;r++)for(int c=0;c<13;c++){
            Vector3 p=P(f,r,c);bool stairs=c==10&&r>=3&&r<=9;
            float wallHeight=WallHeight(f);
            if(c==0&&(r==2||r==6||r==10))WindowWall(p,cell,wallHeight);
            else if(!stairs&&!(f==0&&r==10&&c==12)&&!(f==0&&r==12&&c==6)&&!HouseMap.Walkable(f,r,c))Box("Plaster wall",p+Vector3.up*wallHeight/2,new Vector3(cell,wallHeight,cell),plaster,architecture);
            if(f==0||!stairs)Box("Timber floor",p-Vector3.up*(FloorThickness/2),new Vector3(cell,FloorThickness,cell),wood,architecture);
        }
        BuildUpperCeiling(architecture,plaster);
        // Rendered steps sit above an invisible ramp, so the controller climbs smoothly.
        float length=6*cell;var ramp=Box("Stair ramp",new Vector3(4*cell,h/2-.12f,0),new Vector3(cell-.12f,.16f,Mathf.Sqrt(length*length+h*h)+.3f),wood,architecture);ramp.transform.localRotation=Quaternion.Euler(Mathf.Atan2(h,length)*Mathf.Rad2Deg,0,0);ramp.GetComponent<Renderer>().enabled=false;
        for(int i=0;i<28;i++){float z=3*cell-i*length/28,y=(i+.5f)*h/28;Box("Stair tread",new Vector3(4*cell,y/2,z),new Vector3(cell-.15f,y,length/28+.02f),wood,architecture,false);}
        Box("Upper stair landing",new Vector3(4*cell,h-.12f,-3.25f*cell),new Vector3(cell,.24f,cell*.5f+.1f),wood,architecture);
        for(int f=0;f<2;f++){
            foreach(int row in new[]{2,6,10}){
                float z=(row-6)*cell;
                var light=new GameObject("Cold window light").AddComponent<Light>();light.transform.SetParent(world);light.transform.position=new Vector3(-5*cell,f*h+2.5f,z);light.type=LightType.Point;light.color=new Color(.52f,.66f,.8f);light.intensity=1.6f;light.range=8;
            }
            for(int row=1;row<12;row+=2)Box("Ceiling beam",new Vector3(0,f*h+WallHeight(f)-.15f,(row-6)*cell),new Vector3(7.2f,.3f,.25f),wood,architecture);
            foreach(var rc in new[]{new Vector2Int(2,4),new Vector2Int(6,4),new Vector2Int(10,4),new Vector2Int(2,8),new Vector2Int(10,8)})
                RoomPortal.Build(game,world,P(f,rc.x,rc.y),90,WallHeight(f),plaster,wood,metal);
            foreach(int row in new[]{4,8}){
                if(HouseMap.Walkable(f,row,2))
                    RoomPortal.Build(game,world,P(f,row,2),0,WallHeight(f),plaster,wood,metal);
            }
            RoomPortal.Build(game,world,P(f,4,5),0,WallHeight(f),plaster,wood,metal);
            RoomPortal.Build(game,world,P(f,8,7),0,WallHeight(f),plaster,wood,metal);

        }
        FrontEntrance.Build(game,world,plaster);
        for(int f=0;f<2;f++){var bottle=GameObject.CreatePrimitive(PrimitiveType.Cylinder);bottle.name="Loose bottle";bottle.transform.position=P(f,5,6)+new Vector3(2.9f,.3f,.6f);bottle.transform.localScale=new Vector3(.2f,.3f,.2f);bottle.GetComponent<Renderer>().sharedMaterial=glass;bottle.AddComponent<Rigidbody>().mass=.3f;bottle.AddComponent<NoiseImpact>();}
        RenderSettings.ambientLight=new Color(.20f,.24f,.27f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.035f,.052f,.055f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.014f;
        var moon=new GameObject("Moon").AddComponent<Light>();moon.type=LightType.Directional;moon.color=new Color(.57f,.68f,.83f);moon.intensity=.55f;moon.transform.rotation=Quaternion.Euler(35,-25,0);moon.shadows=LightShadows.Soft;
        ReferenceArt.Build();
        DomesticInterior.Install(world,GameObject.Find("REFERENCE INTERIORS / rooms and exterior").transform);
        BuildPlayer(game);BuildKeeper(game);StaticBatchingUtility.Combine(architecture.gameObject);
    }
    static void BuildPlayer(GameController game) {
        var obj=new GameObject("Player");obj.layer=2;var player=obj.AddComponent<FirstPerson>();player.body=obj.AddComponent<CharacterController>();player.body.radius=.24f;player.body.height=1.75f;player.body.center=Vector3.up*.875f;player.body.stepOffset=.3f;player.body.slopeLimit=48;
        var cam=new GameObject("Player camera");cam.transform.SetParent(obj.transform,false);cam.transform.localPosition=Vector3.up*1.62f;player.view=cam.AddComponent<Camera>();player.view.nearClipPlane=.05f;player.view.farClipPlane=100;player.view.fieldOfView=72;player.view.clearFlags=CameraClearFlags.Skybox;player.view.backgroundColor=new Color(.035f,.05f,.05f);cam.AddComponent<AudioListener>();cam.AddComponent<CinemaLook>();
        var lamp=new GameObject("Flashlight");lamp.transform.SetParent(cam.transform,false);player.torch=lamp.AddComponent<Light>();player.torch.type=LightType.Spot;player.torch.spotAngle=65;player.torch.range=22;player.torch.intensity=2.2f;player.torch.color=new Color(1,.9f,.73f);player.torch.shadows=LightShadows.Hard;player.torch.shadowResolution=UnityEngine.Rendering.LightShadowResolution.Low;game.Player=player;player.Teleport(P(1,11,2)+Vector3.up*.05f);
    }
    static void BuildKeeper(GameController game) {
        var rig=CharacterRig.Create(false);var keeper=rig.gameObject.AddComponent<Keeper>();game.Enemy=keeper;
        keeper.limbs=new[]{rig.Arms[0],rig.Arms[1],rig.Hips[0],rig.Hips[1]};keeper.ResetKeeper();
    }
}
}



