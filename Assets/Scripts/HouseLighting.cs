using System.Linq;
using UnityEngine;

namespace Tishe {
// Only a nearby window needs a shadow map of the moving characters.
// Keep the other windows lit so their rooms still read through open doors.
public class HouseLighting : MonoBehaviour {
    Light[] windows;
    Light tableLamp;
    Renderer bulb;
    public void SetKitchenPower(bool on){
        if(tableLamp==null)return;
        tableLamp.enabled=on;
        if(bulb==null)return;
        var block=new MaterialPropertyBlock();
        if(!on)block.SetColor("_EmissionColor",Color.black);
        bulb.SetPropertyBlock(on?null:block);
    }
    Vector3 lastPosition=new Vector3(1000,1000,1000);
    void Start(){
        var lights=Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach(var light in lights){
            if(light.name=="Moon"){light.intensity=.64f;light.color=new Color(.48f,.57f,.72f);}
            if(light.name=="Cellar warm bulb"){light.intensity=.38f;light.range=6.8f;}
            if(light.name=="Cold window light")light.intensity=.46f;
            if(light.name=="Window shaft light")light.intensity=1.62f;
        }
        RenderSettings.fogColor=new Color(.067f,.055f,.078f);
        RenderSettings.fogDensity=.0155f;
        tableLamp=lights.FirstOrDefault(light=>light.name=="Table lamplight");
        if(tableLamp!=null)bulb=tableLamp.transform.parent.Find("Amber bulb").GetComponent<Renderer>();
        SetKitchenPower(GameController.Instance.Solved[0]);
        windows=lights.Where(light=>light.name=="Window shaft light").ToArray();
    }
    void LateUpdate(){
        var eye=transform.position;
        if((eye-lastPosition).sqrMagnitude<.16f)return;
        lastPosition=eye;
        Light nearest=null;float best=14f*14f;
        foreach(var light in windows){
            var delta=light.transform.position-eye;
            float distance=delta.sqrMagnitude+3f*delta.y*delta.y;
            if(distance<best){best=distance;nearest=light;}
        }
        foreach(var light in windows){
            var wanted=light==nearest?LightShadows.Hard:LightShadows.None;
            if(light.shadows!=wanted)light.shadows=wanted;
        }
    }
}
}
