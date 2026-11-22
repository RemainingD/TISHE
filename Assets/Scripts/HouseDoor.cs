using UnityEngine;
namespace Tishe {
public class HouseDoor : MonoBehaviour {
    public Transform leaf;
    public bool opened;
    public void Toggle(){
        opened=!opened;
        GameController.Instance.Noise(transform.position,28f,opened?"doorOpen":"doorClose");
    }
    public void OpenByKeeper(){
        if(opened)return;
        opened=true;
        var g=GameController.Instance;g.Audio?.Door(transform.position,true);
        g.ReceiveSound(transform.position,1,15);
    }
    static float DistanceToLeafSquared(Vector3 offset,float angle){
        Vector3 along=Quaternion.Euler(0,angle,0)*Vector3.right;
        float closest=Mathf.Clamp(Vector3.Dot(offset,along),0,RoomPortal.LeafSize.x);
        return (offset-along*closest).sqrMagnitude;
    }
    float SafeAngle(float angle,float next,CharacterController body){
        if(body==null || !body.enabled)return next;
        var bounds=body.bounds;
        if(bounds.min.y>=leaf.position.y+RoomPortal.LeafSize.y || bounds.max.y<=leaf.position.y)return next;
        var offset=transform.InverseTransformDirection(bounds.center-leaf.position);
        offset.y=0;
        float clearance=body.radius+body.skinWidth+RoomPortal.LeafSize.z*.5f;
        float reach=RoomPortal.LeafSize.x+clearance;
        if(offset.sqrMagnitude>=reach*reach)return next;
        // A long frame can cover the whole swing. Check along the arc, permitting a retreat
        // from an existing contact even when the player is still within the skin margin.
        float previousDistance=DistanceToLeafSquared(offset,angle);
        while(angle!=next){
            float sample=Mathf.MoveTowards(angle,next,4f);
            float distance=DistanceToLeafSquared(offset,sample);
            if(distance<clearance*clearance && distance<previousDistance-.000001f)break;
            angle=sample;previousDistance=distance;
        }
        return angle;
    }
    void Update(){
        var game=GameController.Instance;
        if(game==null || game.Paused)return;
        float angle=Mathf.DeltaAngle(0,leaf.localEulerAngles.y);
        float target=opened?90:0;
        if(Mathf.Approximately(angle,target))return;
        float next=Mathf.Lerp(angle,target,Time.deltaTime*7);
        if(Mathf.Abs(target-next)<.05f)next=target;
        leaf.localRotation=Quaternion.Euler(0,SafeAngle(angle,next,game.Player.body),0);
    }
}
}
