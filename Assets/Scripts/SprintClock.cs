using UnityEngine;
namespace Tishe {
public class SprintClock {
    public const float Capacity=7f;
    public float Energy=Capacity,Recovery;public bool Running;
    public bool Tick(bool requested,float dt){dt=Mathf.Max(0,dt);if(Recovery>0){Recovery=Mathf.Max(0,Recovery-dt);Running=false;if(Recovery==0)Energy=Capacity;return false;}Running=requested&&Energy>0;if(Running){Energy=Mathf.Max(0,Energy-dt);if(Energy==0)Recovery=5;}else Energy=Mathf.Min(Capacity,Energy+dt);return Running;}
    public void Reset(){Energy=Capacity;Recovery=0;Running=false;}
}
}
