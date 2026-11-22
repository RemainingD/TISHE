using UnityEngine;
namespace Tishe {
// Uses unscaled time deliberately: gameplay, physics and the keeper freeze during cinematics.
public class StoryDirector : MonoBehaviour {
    public float Fade {get;private set;}public float Clock {get;private set;}public CharacterRig Boy {get;private set;}
    GameController g;CharacterRig wakingBody;Vector3 scarePosition;Quaternion scareRotation;const float WakeDuration=7.8f,RunDuration=10.6f;
    public bool FarewellStarted {get;private set;}
    float escapeEnd,nextFoot,normalFov;
    Vector3 entryFeet;
    CharacterRig endingKeeper;
    Renderer[] playerRenderers;
    Vector3 lastFootPosition;
    float gait,torchBrightness;
    Light captureLight;
    Vector3 keeperAtCatch;
    const int FirstPersonLayer=9;
    public void Initialize(GameController game){
        g=game;
        Boy=CharacterRig.Create(true);
        playerRenderers=Boy.GetComponentsInChildren<Renderer>();
        SetWalkingShadow(true);
        Boy.gameObject.SetActive(false);
        wakingBody=CharacterRig.Create(true);
        wakingBody.name="First-person waking body";
        foreach(var part in wakingBody.GetComponentsInChildren<Renderer>()){
            if(part.transform.IsChildOf(wakingBody.Head)||part.name=="Neck"||part.name.Contains("Backpack")||part.name.Contains("flashlight")||part.name=="Torch lens")
                part.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
        wakingBody.gameObject.SetActive(false);
        // The torch is held by the player. Other room lights still cast the complete silhouette.
        captureLight=new GameObject("Scare sidelight").AddComponent<Light>();
        captureLight.transform.SetParent(transform,false);captureLight.type=LightType.Point;
        captureLight.color=new Color(.62f,.73f,.78f);captureLight.range=1.65f;
        captureLight.intensity=.72f;captureLight.enabled=false;
        g.Player.torch.cullingMask &= ~(1<<FirstPersonLayer);
    }
    void SetWalkingShadow(bool walking){
        foreach(var part in playerRenderers){
            part.gameObject.layer=walking?FirstPersonLayer:0;
            part.shadowCastingMode=walking?UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly:UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }
    void UpdateWalkingBody(){
        var player=g.Player;
        Boy.gameObject.SetActive(!player.hidden);
        if(player.hidden)return;
        Vector3 feet=player.transform.position;
        Vector3 step=feet-lastFootPosition;step.y=0;
        float distance=step.magnitude;
        lastFootPosition=feet;
        if(g.Paused)return;
        if(distance<1)gait+=distance*.22f;
        Boy.transform.SetPositionAndRotation(feet,player.transform.rotation);
        Boy.transform.localScale=Vector3.one*1.05f;
        Boy.RunPose(gait,Mathf.Clamp01(distance/Mathf.Max(.001f,Time.deltaTime)/4));
        if(player.crouched){
            Boy.transform.position=feet+Vector3.down*.20f;
            Boy.SitPose(.65f);
        }
    }
    static float Ease(float value)=>Mathf.SmoothStep(0,1,Mathf.Clamp01(value));
    public void Wake(){g.ClearNoise();g.Receiver.Reset();g.Player.Shelter?.Cancel();if(captureLight.enabled){captureLight.enabled=false;g.Player.torch.intensity=torchBrightness;}Clock=0;Fade=1;g.State="waking";g.Panel="";g.Player.body.enabled=false;g.Player.hidden=false;g.Player.crouched=false;g.Player.torch.enabled=false;g.Enemy.ResetKeeper();Boy.gameObject.SetActive(false);}
    public void Escape(){
        Clock=0;Fade=0;FarewellStarted=false;escapeEnd=float.PositiveInfinity;nextFoot=.18f;
        entryFeet=g.Player.transform.position;
        entryFeet.x=Mathf.Clamp(entryFeet.x,-.8f,.8f);entryFeet.z=Mathf.Clamp(entryFeet.z,10.3f,12.25f);entryFeet.y=0;
        g.Audio.ExitCreak();normalFov=g.Player.view.fieldOfView;g.State="escape";g.Panel="";
        g.Player.body.enabled=false;g.Player.torch.enabled=true;
        Boy.gameObject.SetActive(false);wakingBody.gameObject.SetActive(true);
        g.Enemy.transform.SetPositionAndRotation(new Vector3(-1.3f,.02f,8.6f),Quaternion.identity);
        endingKeeper=g.Enemy.GetComponent<CharacterRig>();
    }
    public void CaptureShock(){Clock=0;scarePosition=g.Player.view.transform.position;scareRotation=g.Player.view.transform.rotation;Boy.gameObject.SetActive(false);if(!g.Scare)return;var view=g.Player.view.transform;
        torchBrightness=g.Player.torch.intensity;g.Player.torch.intensity=.16f;
        captureLight.transform.position=scarePosition+view.right*.37f+view.up*.27f+view.forward*.15f;
        captureLight.enabled=true;
        g.Enemy.transform.rotation=Quaternion.LookRotation(-view.forward,view.up);
        keeperAtCatch=scarePosition+view.forward*.72f-g.Enemy.transform.rotation*(Vector3.up*1.82f);
        g.Enemy.transform.position=keeperAtCatch;}
    void LateUpdate(){if(g==null)return;if(g.State=="playing"){UpdateWalkingBody();return;}if(g.State=="caught"){Clock+=Time.unscaledDeltaTime;if(g.Scare){g.Enemy.transform.position=keeperAtCatch-(scareRotation*Vector3.forward)*(.12f*Ease(Clock/.32f));
            g.Player.view.transform.position=scarePosition+Random.insideUnitSphere*.018f;g.Player.view.transform.rotation=scareRotation*Quaternion.Euler(Mathf.Sin(Clock*60)*1.4f,0,0);}return;}if(g.State!="waking"&&g.State!="escape")return;Clock+=Time.unscaledDeltaTime;var camera=g.Player.view.transform;
        if(g.State=="waking"){
            Vector3 standing=new Vector3(-7.2f,4.28f,12);
            Vector3 lyingEyes=new Vector3(-10.8f,5.17f,10.1f),sittingEyes=new Vector3(-8.65f,5.85f,10.15f);
            float sit=Ease((Clock-2.5f)/2.6f),rise=Ease((Clock-5.1f)/2.7f);
            wakingBody.gameObject.SetActive(Clock>2.2f);
            wakingBody.transform.position=Vector3.Lerp(Vector3.Lerp(new Vector3(-9.15f,4.98f,10.1f),new Vector3(-8.65f,4.43f,10.15f),sit),standing,rise);
            wakingBody.transform.rotation=Quaternion.Slerp(Quaternion.Slerp(Quaternion.LookRotation(Vector3.up,Vector3.left),Quaternion.Euler(0,90,0),sit),Quaternion.Euler(0,180,0),rise);
            wakingBody.SitPose(sit*(1-rise));
            camera.position=Vector3.Lerp(Vector3.Lerp(lyingEyes,sittingEyes,sit),standing+Vector3.up*1.62f,rise);
            camera.rotation=Quaternion.Slerp(Quaternion.Slerp(Quaternion.Euler(-82,90,-8),Quaternion.Euler(12,90,0),sit),Quaternion.Euler(0,180,0),rise);
            Fade=Clock<2.2f?1:1-Ease((Clock-2.2f)/1.1f);            if(Clock>=WakeDuration){wakingBody.gameObject.SetActive(false);Boy.gameObject.SetActive(false);g.Player.Teleport(standing);g.Player.view.transform.localPosition=Vector3.up*1.62f;g.Player.view.transform.localRotation=Quaternion.identity;g.Player.torch.enabled=true;lastFootPosition=standing;g.Grace=12;g.State="playing";g.AnnounceGoal();Fade=0;Cursor.lockState=CursorLockMode.Locked;g.Say(g.Lives==3?"Бөтен бөлмеде ояндың. Шығатын жолды табу керек.":"Тағы да сол төсек. Тапқан заттарың мен орындаған тапсырмаларың сақталды.");}
        }else {
            if(Clock<RunDuration){
                g.Entrance.SetOpen(Clock);
                float launch=Mathf.Max(0,Clock-.37f);
                float z=entryFeet.z+5.05f*(launch-.54f*(1-Mathf.Exp(-launch/.54f)));
                Vector3 feet=ForestYard.Trail(z);
                feet.x+=entryFeet.x*(1-Ease((z-entryFeet.z)/(13.1f-entryFeet.z)));
                feet.y=Mathf.Lerp(0,feet.y,Ease((z-19)/1.1f));
                Vector3 direction=ForestYard.Trail(z+2.1f)-feet;direction.y=0;
                Quaternion facing=Quaternion.LookRotation(direction);
                float stride=Clock*9.5f,effort=Ease(Clock/.8f);
                wakingBody.transform.SetPositionAndRotation(feet,facing);
                wakingBody.RunPose(stride/10f,effort);
                for(int i=0;i<2;i++){
                    float swing=Mathf.Sin(stride)*(i==0?1:-1);
                    wakingBody.Arms[i].localRotation=Quaternion.Euler(-29-43*swing,0,i==0?-7:7);
                    wakingBody.Elbows[i].localRotation=Quaternion.Euler(-88+8*swing,0,0);
                }
                camera.position=feet+facing*new Vector3(.035f*Mathf.Sin(stride),1.48f+.032f*Mathf.Cos(2*stride),.045f);
                camera.rotation=facing*Quaternion.Euler(7+Mathf.Cos(2*stride)*.95f,Mathf.Sin(stride)*.65f,Mathf.Sin(stride)*1.15f);
                g.Player.view.fieldOfView=Mathf.Lerp(normalFov,77,effort);
                if(Clock>=nextFoot){g.Audio.RunStep(feet);nextFoot=Clock+Mathf.PI/9.5f;}
            }else{
                float shot=Clock-RunDuration;
                g.Enemy.transform.SetPositionAndRotation(new Vector3(0,.02f,17.15f),Quaternion.identity);
                wakingBody.gameObject.SetActive(false);g.Player.torch.enabled=false;
                var keeper=g.Enemy.transform;
                float approach=Ease(shot/3.25f);
                camera.position=keeper.position+Vector3.Lerp(new Vector3(-.85f,1.51f,3.45f),new Vector3(-.34f,1.70f,1.32f),approach);
                camera.LookAt(keeper.position+Vector3.up*Mathf.Lerp(1.42f,1.80f,approach));
                g.Player.view.fieldOfView=Mathf.Lerp(60,49,approach);
                captureLight.transform.position=camera.position+camera.right*.45f+Vector3.up*.3f;
                captureLight.enabled=true;captureLight.range=4;captureLight.intensity=1.17f;
                endingKeeper.RunPose(shot*.18f,.09f);
                endingKeeper.Arms[0].localRotation=Quaternion.Euler(-64+51*Ease(shot/2.4f),0,-8);
                endingKeeper.Head.localRotation=Quaternion.Euler(7+2*Mathf.Sin(shot*2.7f),Mathf.Lerp(32,-12,Ease(shot/2.1f)),-7);
                if(shot>=2.05f && !FarewellStarted){FarewellStarted=true;escapeEnd=Clock+g.Audio.Farewell()+1.15f;}
                Fade=Ease((Clock-(escapeEnd-.9f))/.9f);
                if(Clock>=escapeEnd){g.State="won";Fade=0;captureLight.enabled=false;captureLight.range=1.65f;captureLight.intensity=.72f;g.Player.view.fieldOfView=normalFov;}
            }
        }
    }
}
}


