using System.Collections.Generic;
using UnityEngine;

namespace Tishe {
public class Keeper : MonoBehaviour {
    public string mode="patrol";
    public Transform[] limbs;
    public Vector3 target;
    const float SearchSeconds=7f;
    const float RunSpeed=FirstPerson.SprintSpeed*.9f;
    readonly int[] stops={HouseMap.Id(0,2,6),HouseMap.Id(0,10,6),HouseMap.Id(1,2,6),HouseMap.Id(1,10,6),HouseMap.Id(1,6,2)};
    readonly List<int> nearby=new List<int>();
    List<int> route=new List<int>();
    int index,patrol,routeEnd=-1,searchIndex;
    float repath,searchLeft,beat;
    Wardrobe witnessed;
    CharacterRig rig;

    readonly List<Bounds> cabinets=new List<Bounds>();
    readonly HashSet<int> blockedNodes=new HashSet<int>();
    static Vector3 Waypoint(int node){
        // The bedroom corner between the bed and wardrobe joins two aisles.
        // Its grid centre is inside the wardrobe on both floors.
        var point=HouseMap.Position(node);
        if(node%169==HouseMap.Id(0,9,3)){point.x=-8.62f;point.z=8.30f;}
        return point;
    }
    void Start(){
        foreach(var w in FindObjectsByType<Wardrobe>(FindObjectsSortMode.None))cabinets.Add(w.KeeperObstacle);
        for(int i=0;i<349;i++)if(InsideCabinet(Waypoint(i)))blockedNodes.Add(i);
    }
    bool InsideCabinet(Vector3 point){
        foreach(var box in cabinets)if(box.Contains(point))return true;
        return false;
    }
    bool ClearSegment(Vector3 a,Vector3 b){
        var delta=b-a;float length=delta.magnitude;
        if(length<.0001f)return !InsideCabinet(a);
        var ray=new Ray(a,delta/length);
        foreach(var box in cabinets)
            if(box.Contains(a) || (box.IntersectRay(ray,out float hit) && hit<=length))return false;
        return true;
    }
    IEnumerable<KeyValuePair<int,float>> WalkEdges(int node){
        var from=Waypoint(node);
        foreach(var edge in HouseMap.Edges(node))
            if(!blockedNodes.Contains(edge.Key)&&ClearSegment(from,Waypoint(edge.Key)))
                yield return new KeyValuePair<int,float>(edge.Key,Vector3.Distance(from,Waypoint(edge.Key)));
    }
    int RouteNode(Vector3 point){
        int node=HouseMap.Nearest(point);
        bool inside=InsideCabinet(point);
        if(node%169!=HouseMap.Id(0,9,3)&&!blockedNodes.Contains(node)&&(inside||ClearSegment(point,Waypoint(node))))return node;
        // A sound can land inside a cabinet. Select a floor waypoint outside its footprint.
        float best=float.PositiveInfinity;int result=-1;
        for(int i=0;i<349;i++){
            if(blockedNodes.Contains(i)||(i<338&&!HouseMap.Navigable(i/169,i%169/13,i%13)))continue;
            var pos=Waypoint(i);float d=(pos-point).sqrMagnitude;
            if(d<best&&(inside||ClearSegment(point,pos))){best=d;result=i;}
        }
        return result;
    }
    void MoveSafely(Vector3 destination,float distance){
        var next=Vector3.MoveTowards(transform.position,destination,distance);
        if(ClearSegment(transform.position,next))transform.position=next;
    }
    public void ResetKeeper(){
        transform.position=HouseMap.Position(stops[0]);patrol=1;mode="patrol";
        target=HouseMap.Position(stops[patrol]);witnessed=null;route.Clear();index=0;routeEnd=-1;repath=0;
    }
    public bool SeesPlayer(){
        var p=GameController.Instance.Player;
        if(p.hidden && (p.Shelter==null || !p.Shelter.VisibleDuringEntry))return false;
        Vector3 from=transform.position+Vector3.up*1.6f;
        Vector3 delta=p.transform.position+Vector3.up*(p.crouched?.8f:1.1f)-from;
        if(delta.magnitude>(p.crouched?7:11) || Vector3.Dot(transform.forward,delta.normalized)<(delta.magnitude<2?.02f:.25f))return false;
        return !Physics.Raycast(from,delta.normalized,delta.magnitude,~(1<<2),QueryTriggerInteraction.Ignore);
    }
    public void Hear(Vector3 point,float radius){
        if(witnessed!=null)return;
        float damp=Mathf.Abs(point.y-transform.position.y)>2?.65f:1;
        if(Vector3.Distance(point,transform.position)>radius*damp)return;
        // A new sound is a position in the world. It does not reveal the hiding place.
        target=point;repath=0;
        if(mode!="chase" || !SeesPlayer())mode="sound";
    }
    public void SawEntry(Wardrobe cabinet){
        if(witnessed==cabinet)return;
        witnessed=cabinet;mode="wardrobe";target=cabinet.Approach;repath=0;
    }
    void BeginSearch(){
        int center=RouteNode(transform.position);
        nearby.Clear();
        foreach(var edge in WalkEdges(center)){
            if(Mathf.Abs(HouseMap.Position(center).y-HouseMap.Position(edge.Key).y)<.3f)nearby.Add(edge.Key);
        }
        if(nearby.Count==1)nearby.Add(center);
        searchIndex=0;searchLeft=SearchSeconds;mode="search";NextSearchPoint();
    }
    void NextSearchPoint(){
        if(nearby.Count>0){target=Waypoint(nearby[searchIndex%nearby.Count]);searchIndex++;repath=0;}
    }
    void Patrol(){
        mode="patrol";patrol=(patrol+1)%stops.Length;
        target=HouseMap.Position(stops[patrol]);repath=0;
    }
    bool Follow(float speed,float dt){
        repath-=dt;
        if(repath<=0){
            int goal=RouteNode(target);
            if(goal!=routeEnd){
                Vector3 start=transform.position;
                // The stairs are a single graph edge. A new sound can reverse the
                // route halfway along it, but the keeper must exit via a landing.
                if(start.y>.01f && start.y<HouseMap.Height-.01f &&
                   Mathf.Abs(start.x-9.6f)<1.15f && Mathf.Abs(start.z)<7.21f){
                    int landing=HouseMap.Position(goal).y>=HouseMap.Height?
                        HouseMap.Id(1,3,10):HouseMap.Id(0,9,10);
                    start=HouseMap.Position(landing);
                }else if(start.y<-.01f && start.x>15.6f && start.x<22.8f){
                    start=HouseMap.Position(HouseMap.Position(goal).y<0?340:339);
                }
                int first=RouteNode(start);
                route=AiAlgorithms.Search(first,goal,WalkEdges,n=>Vector3.Distance(Waypoint(n),Waypoint(goal))).path;
                index=0;routeEnd=goal;
                if(route.Count>1 && start==transform.position){
                    Vector3 from=Waypoint(route[0]),edge=Waypoint(route[1])-from;
                    float progress=Mathf.Clamp01(Vector3.Dot(transform.position-from,edge)/edge.sqrMagnitude);
                    if((from+edge*progress-transform.position).sqrMagnitude<.0064f)index++;
                }
            }
            repath=.3f;
        }
        if(index>=route.Count)return route.Count>0;
        Vector3 next=Waypoint(route[index]),delta=next-transform.position;
        MoveSafely(next,speed*dt);
        if((transform.position-next).sqrMagnitude<.0064f)index++;
        Turn(delta,dt);
        foreach(var door in GameController.Instance.Doors)
            if((door.transform.position-transform.position).sqrMagnitude<5.76f)door.OpenByKeeper();
        return index>=route.Count;
    }
    bool ReachSound(float dt){
        // Object impacts can originate inside furniture or above the floor.
        Vector3 delta=target-transform.position;delta.y=0;
        float distance=delta.magnitude;
        if(distance<=.18f)return true;
        Vector3 direction=delta/distance;
        float step=Mathf.Min(distance,RunSpeed*dt);
        if(Physics.SphereCast(transform.position+Vector3.up*.7f,.16f,direction,out _,step,~(1<<2),QueryTriggerInteraction.Ignore))return true;
        if(!ClearSegment(transform.position,transform.position+direction*step))return true;
        transform.position+=direction*step;
        Turn(direction,dt);
        return distance-step<=.18f;
    }
    void Turn(Vector3 direction,float dt){
        direction.y=0;
        if(direction.sqrMagnitude>.001f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(direction),dt*10);
    }
    void Update(){
        var g=GameController.Instance;
        if(g==null||g.Paused)return;
        float dt=Time.deltaTime;
        if(witnessed!=null && !witnessed.Occupied){witnessed=null;mode="investigate";repath=0;}
        if(g.Grace<=0 && witnessed==null && SeesPlayer()){
            if(g.Player.Shelter!=null && g.Player.Shelter.VisibleDuringEntry)SawEntry(g.Player.Shelter);
            else{
                if(mode!="chase")g.Audio?.Spotted();
                mode="chase";target=g.Player.transform.position;
                if(Vector3.Distance(transform.position,target)<1.05f){g.Capture();return;}
            }
        }else if(mode=="chase"){mode="investigate";repath=0;}
        Vector3 previous=transform.position;
        bool running=mode=="sound" || mode=="chase";
        if(witnessed!=null){
            if(witnessed.Discovering){Turn(witnessed.transform.position-transform.position,dt);return;}
            if(Follow(4.1f,dt)){
                MoveSafely(witnessed.Approach,2.7f*dt);
                Turn(witnessed.transform.position-transform.position,dt);
                if(Vector3.Distance(transform.position,witnessed.Approach)<.25f)witnessed.Discover();
            }
        }else if(mode=="search"){
            searchLeft-=dt;
            if(searchLeft<=0)Patrol();
            else if(Follow(1.2f,dt))NextSearchPoint();
        }else{
            bool arrived=Follow(running?RunSpeed:mode=="investigate"?2.65f:1.65f,dt);
            if(arrived && mode=="sound"){
                float remaining=Mathf.Max(0,dt-Vector3.Distance(previous,transform.position)/RunSpeed);
                if(routeEnd==RouteNode(target) && ReachSound(remaining))BeginSearch();
            }else if(arrived && mode=="investigate")BeginSearch();
            else if(arrived && mode=="patrol")Patrol();
            else if(arrived && mode=="chase"){
                // Close the final fraction of a cell only with direct sight to the player.
                float remaining=Mathf.Max(0,RunSpeed*dt-Vector3.Distance(previous,transform.position));
                MoveSafely(target,remaining);
                Turn(target-transform.position,dt);
            }
        }
        beat+=dt*(running?1.15f:.65f);
        if(rig==null)rig=GetComponent<CharacterRig>();
        if(rig!=null){
            rig.RunPose(beat,(transform.position-previous).sqrMagnitude<.000001f?.05f:running?.8f:.34f);
            if(mode=="search")rig.Head.localRotation*=Quaternion.Euler(0,Mathf.Sin(beat*4)*30,0);
        }
    }
}
}

