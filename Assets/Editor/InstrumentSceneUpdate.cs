using System;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Tishe.Editor {
public static class InstrumentSceneUpdate {
    public static void Install(){
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/House.unity");
        var all=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
        var living=all.Single(t=>t.name=="Living room / fireplace");
        var oldKit=living.parent.Find("Wall instruments");
        if(oldKit!=null){
            Object.DestroyImmediate(oldKit.gameObject);
            Object.DestroyImmediate(GameObject.Find("Loose quest objects"));
            Object.DestroyImmediate(living.Find("Hearth and television").gameObject);
        }else{
            var items=Object.FindObjectsByType<GameItem>(FindObjectsSortMode.None);
            var pickups=items.Where(i=>i.kind=="pickup").ToArray();
            var centers=pickups.Select(i=>i.transform.position-1.12f*Vector3.up).ToArray();
            var parts=all.Where(t=>(t.name=="Table top"||t.name=="Table leg"||t.name=="Detailed furniture / CoffeeTable_01")
                && centers.Any(c=>Mathf.Abs(t.position.x-c.x)<.85f&&Mathf.Abs(t.position.z-c.z)<.45f&&Mathf.Abs(t.position.y-c.y)<1f)).ToArray();
            var stations=items.Where(i=>i.kind=="station").ToArray();
            var lamps=all.Where(t=>t.name=="Status lamp").ToArray();
            if(pickups.Length!=5||parts.Length!=30||stations.Length!=8||lamps.Length!=4)throw new InvalidOperationException($"Unexpected legacy layout {pickups.Length}/{parts.Length}/{stations.Length}/{lamps.Length}");
            foreach(var t in parts.Concat(lamps))Object.DestroyImmediate(t.gameObject);
            foreach(var item in pickups.Concat(stations))Object.DestroyImmediate(item.gameObject);
            Object.DestroyImmediate(living.Find("TV screen").gameObject);
        }
        WallInstruments.Install(living.parent);LivingRoomEffects.Install(living);
        EditorSceneManager.MarkSceneDirty(scene);HouseSceneTools.PrepareVisibility();
        Debug.Log("TISHE_INSTRUMENTS_INSTALLED");
    }
}
}
