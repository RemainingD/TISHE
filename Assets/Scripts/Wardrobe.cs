using UnityEngine;
namespace Tishe {
public class Wardrobe : MonoBehaviour {
    public Transform Left,Right;FirstPerson player;Vector3 outside;Quaternion outsideRotation;float clock;bool exiting;public bool Occupied=>player!=null && !exiting;
    public bool VisibleDuringEntry=>Occupied && !Discovering && clock<1.05f;
    public Vector3 Approach=>transform.TransformPoint(new Vector3(0,0,-1.55f));
    // Includes clearance for the keeper's shoulders and coat on every side.
    internal Bounds KeeperObstacle {
        get {
            var box=new Bounds(transform.TransformPoint(new Vector3(0,1.38f,0)),Vector3.zero);
            foreach(float x in new[]{-1.47f,1.47f})
                foreach(float z in new[]{-1.12f,1.12f})
                    foreach(float y in new[]{-.15f,2.9f})
                        box.Encapsulate(transform.TransformPoint(new Vector3(x,y,z)));
            return box;
        }
    }
    public bool Discovering {get;private set;}
    float revealClock,revealDuration;
    public void Discover(){
        if(!Occupied || Discovering || clock<1.5f)return;
        Discovering=true;revealClock=0;
        var g=GameController.Instance;
        revealDuration=g.Audio.LaughAtWardrobe();
        g.Audio.Door(transform.position,true,.52f);
        g.Say("Күзетші: «А-ха-ха! Ұсталдың!»");
    }
    public static void Build(Vector3 p,float yaw,Material wood,Material metal){var g=new GameObject("Hollow wardrobe / playable hiding");g.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));var w=g.AddComponent<Wardrobe>();var item=g.AddComponent<GameItem>();item.kind="hide";item.title="Шкафқа жасырыну";
        foreach(float x in new[]{-.86f,.86f})ReferenceArt.Box(g.transform,"Carved side",new Vector3(x,1.4f,0),new Vector3(.13f,2.8f,1.0f),wood,true);
        ReferenceArt.Box(g.transform,"Back panel",new Vector3(0,1.4f,.46f),new Vector3(1.8f,2.8f,.10f),wood,true);
        foreach(float y in new[]{.06f,2.8f})ReferenceArt.Box(g.transform,"Moulded plinth",new Vector3(0,y,0),new Vector3(1.9f,.16f,1.12f),wood,true);
        for(int side=0;side<2;side++){float s=side==0?-1:1;var pivot=new GameObject("Door hinge").transform;pivot.SetParent(g.transform,false);pivot.localPosition=new Vector3(s*.8f,0,-.5f);if(side==0)w.Left=pivot;else w.Right=pivot;
            ReferenceArt.Box(pivot,"Paneled wardrobe door",new Vector3(-s*.39f,1.42f,0),new Vector3(.75f,2.66f,.075f),wood,true);foreach(float y in new[]{.8f,2f}){ReferenceArt.Box(pivot,"Recessed wooden panel",new Vector3(-s*.39f,y,-.05f),new Vector3(.59f,.95f,.04f),wood);}
            float panelCenter=-s*.39f;
            foreach(float y in new[]{.8f,2f}){
                foreach(float edge in new[]{-1f,1f}){
                    ReferenceArt.Box(pivot,"Raised walnut stile",new Vector3(panelCenter+edge*.295f,y,-.076f),new Vector3(.035f,.99f,.037f),wood);
                    ReferenceArt.Box(pivot,"Panel cross rail",new Vector3(panelCenter,y+edge*.48f,-.076f),new Vector3(.60f,.038f,.037f),wood);
                }
            }
            foreach(float y in new[]{.36f,2.43f}){
                ReferenceArt.Round(pivot,"Hinge barrel",new Vector3(-s*.046f,y,-.057f),new Vector3(.027f,.060f,.027f),metal,PrimitiveType.Cylinder);
            }
            ReferenceArt.Round(pivot,"Keyhole plate",new Vector3(-s*.68f,1.22f,-.079f),new Vector3(.065f,.17f,.025f),metal);
            ReferenceArt.Round(pivot,"Door handle",new Vector3(-s*.68f,1.3f,-.09f),new Vector3(.06f,.16f,.07f),metal);
        }
    }
    public void Enter(FirstPerson p){if(player!=null)return;var game=GameController.Instance;bool seen=game.Grace<=0 && game.Enemy.SeesPlayer();if(!p.crouched)game.Noise(p.transform.position,3,"step");if(seen)game.Enemy.SawEntry(this);player=p;outside=p.transform.position;outsideRotation=p.transform.rotation;clock=0;exiting=false;p.Shelter=this;p.body.enabled=false;p.hidden=true;p.torch.enabled=false;}
    public void Exit(){if(player==null||exiting||Discovering||clock<1.6f)return;clock=0;exiting=true;}
    public void Cancel(){if(player==null)return;var p=player;player=null;Discovering=false;p.Shelter=null;p.hidden=false;p.body.enabled=true;Left.localRotation=Right.localRotation=Quaternion.identity;}
    void LateUpdate(){if(player==null||GameController.Instance.Paused)return;if(Discovering){
            revealClock+=Time.deltaTime;
            float swing=Mathf.SmoothStep(0,1,Mathf.Clamp01((revealClock-.22f)/.85f));
            Left.localRotation=Quaternion.Euler(0,92*swing,0);Right.localRotation=Quaternion.Euler(0,-83*swing,0);
            var keeper=GameController.Instance.Enemy;
            var rig=keeper.GetComponent<CharacterRig>();
            rig.Arms[0].localRotation=Quaternion.Euler(-76+45*swing,0,-12);
            rig.Head.localRotation=Quaternion.Euler(12+swing*9,Mathf.Sin(revealClock*7)*3,-7);
            if(revealClock>=revealDuration)GameController.Instance.Capture();
            return;
        }
        float previous=clock;clock+=Time.deltaTime;
        if(previous==0)GameController.Instance.Audio?.Door(transform.position,true,.47f);
        if(previous<.75f && clock>=.75f)GameController.Instance.Audio?.Door(transform.position,false,.40f);
        float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(clock/1.5f));float open=Mathf.Sin(t*Mathf.PI)*75;Left.localRotation=Quaternion.Euler(0,open,0);Right.localRotation=Quaternion.Euler(0,-open,0);
        Vector3 inside=transform.TransformPoint(new Vector3(0,.1f,.05f));Quaternion face=transform.rotation*Quaternion.Euler(0,180,0);player.transform.position=Vector3.Lerp(exiting?inside:outside,exiting?outside:inside,t);player.transform.rotation=Quaternion.Slerp(exiting?face:outsideRotation,exiting?outsideRotation:face,t);player.view.transform.localPosition=new Vector3(0,1.5f,0);player.view.transform.localRotation=Quaternion.identity;
        if(exiting&&t>=1){var p=player;Cancel();p.torch.enabled=true;GameController.Instance.Say("Шкафтан шықтың.");}
    }
}
}
