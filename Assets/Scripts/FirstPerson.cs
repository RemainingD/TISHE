using UnityEngine;
namespace Tishe {
public class FirstPerson : MonoBehaviour {
    internal const float SprintSpeed=5f;
    public Camera view; public CharacterController body; public Light torch;
    public SprintClock Sprint=new SprintClock();public Wardrobe Shelter;public bool crouched, hidden; float pitch,vertical,step;
    public GameItem Target {get;private set;}
    void OnControllerColliderHit(ControllerColliderHit hit){var rb=hit.collider.attachedRigidbody;if(rb==null||rb.isKinematic||hit.moveDirection.y<-.3f)return;rb.AddForce(new Vector3(hit.moveDirection.x,0,hit.moveDirection.z)*1.8f,ForceMode.Impulse);}
    public void Teleport(Vector3 p) {if(Shelter!=null)Shelter.Cancel();Sprint.Reset();body.enabled=false;transform.position=p;transform.rotation=Quaternion.Euler(0,180,0);body.enabled=true;pitch=0;vertical=0;step=0;hidden=false;crouched=false;}
    // A footfall is driven by horizontal travel, including on high-refresh displays.
    internal void Footfall(Vector3 feet,bool running){
        var game=GameController.Instance;
        if(crouched){game.Audio?.Step(.06f);return;}
        game.Noise(feet,running?25f:8f,"step");
        if(!HorrorAudio.IsWoodFloor(feet))return;
        // The kitchen has loose boards beside the worktop and the doorway.
        bool kitchen=feet.y<2 && feet.x< -5.9f && Mathf.Abs(feet.z)<3.6f;
        bool kitchenBoard=kitchen && Mathf.Repeat(feet.z+.75f,2.4f)<1.65f;
        if(kitchenBoard || HouseMap.Nearest(feet)%7==0)
            game.Noise(feet,36f,"creak");
    }
    void Update() {
        var game=GameController.Instance;if(game==null||game.Paused||game.State!="playing"){Target=null;return;}
        if(Input.GetKeyDown(KeyCode.G))game.UseReceiver();
        if(Shelter!=null){Target=null;if(Input.GetKeyDown(KeyCode.E))Shelter.Exit();return;}
        if(Cursor.lockState!=CursorLockMode.Locked && Input.GetMouseButtonDown(0))Cursor.lockState=CursorLockMode.Locked;
        if(Cursor.lockState==CursorLockMode.Locked){transform.Rotate(0,Input.GetAxis("Mouse X")*game.Sensitivity,0);pitch=Mathf.Clamp(pitch-Input.GetAxis("Mouse Y")*game.Sensitivity,-80,80);}
        view.transform.localRotation=Quaternion.Euler(pitch,0,0);
        if(Input.GetKeyDown(KeyCode.F))torch.enabled=!torch.enabled;
        for(int i=0;i<5;i++)if(Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1+i)))game.SelectSlot(i);
        if(Input.GetKeyDown(KeyCode.C)&&!hidden)crouched=!crouched;
        if(Input.GetKeyDown(KeyCode.R))game.CheckReceiver();
        if(Input.GetKeyDown(KeyCode.Q)&&!hidden)game.Drop();
        body.height=crouched?1.15f:1.75f;body.center=Vector3.up*(body.height/2);
        view.transform.localPosition=Vector3.Lerp(view.transform.localPosition,new Vector3(0,crouched?1.0f:1.62f,0),Time.deltaTime*10);
        bool run=false;
        var move=new Vector3((Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.A)?1:0),0,(Input.GetKey(KeyCode.W)?1:0)-(Input.GetKey(KeyCode.S)?1:0));move=Vector3.ClampMagnitude(move,1);run=Sprint.Tick(Input.GetKey(KeyCode.LeftShift)&&!crouched&&!hidden&&move.sqrMagnitude>0,Time.deltaTime);
        if(body.isGrounded&&vertical<0)vertical=-2;vertical-=18*Time.deltaTime;
        if(!hidden) {Vector3 old=transform.position;body.Move((transform.TransformDirection(move)*(crouched?1.55f:run?SprintSpeed:3.0f)+Vector3.up*vertical)*Time.deltaTime);
            if(move.sqrMagnitude>0 && body.isGrounded){
                Vector3 travel=transform.position-old;travel.y=0;
                step+=travel.magnitude;
                float stride=crouched?1.24f:run?1.50f:1.56f;
                if(step>=stride){step%=stride;Footfall(transform.position,run);}
            }
        }
        Target=null;if(Physics.Raycast(view.transform.position,view.transform.forward,out var hit,3.2f,~(1<<2),QueryTriggerInteraction.Ignore))Target=hit.collider.GetComponentInParent<GameItem>();
        if(Input.GetKeyDown(KeyCode.E)){if(hidden){hidden=false;game.Say("Жасырынған жеріңнен шықтың.");}else if(Target!=null)Target.Use();}
        if(transform.position.y<-4)Teleport(HouseMap.Position(HouseMap.Id(1,11,2)));
    }
}
}

