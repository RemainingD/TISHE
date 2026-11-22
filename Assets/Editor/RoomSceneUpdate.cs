using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tishe.Editor {
public static class RoomSceneUpdate {
    public static void Install(){
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/House.unity");
        var all=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
        var kitchen=all.Single(t=>t.name=="Kitchen / cupboards and sink");
        var nursery=all.Single(t=>t.name=="Nursery / abandoned toys");
        var oldNursery=nursery.Find("Nursery heirlooms");
        var oldKitchen=kitchen.Find("Kitchen heirlooms");
        if(oldNursery!=null)Object.DestroyImmediate(oldNursery.gameObject);
        if(oldKitchen!=null)Object.DestroyImmediate(oldKitchen.gameObject);
        foreach(var t in nursery.Cast<Transform>().Where(t=>t.name=="Cot mattress"||t.name=="Turned rail"||t.name=="Toy body"||t.name=="Toy head").ToArray())
            Object.DestroyImmediate(t.gameObject);
        foreach(var t in kitchen.Cast<Transform>().Where(t=>t.name=="Sink basin"||t.name=="Stove ring").ToArray())
            Object.DestroyImmediate(t.gameObject);
        var furnishings=new RoomHeirlooms();furnishings.Kitchen(kitchen);furnishings.Nursery(nursery);
        HouseSceneTools.PrepareVisibility();
        EditorSceneManager.MarkSceneDirty(scene);
        if(!EditorSceneManager.SaveScene(scene))throw new System.IO.IOException("Cannot save furnished rooms");
        Debug.Log("TISHE_FURNISHED_ROOMS_INSTALLED");
    }
}
}
