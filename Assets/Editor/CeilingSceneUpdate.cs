using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tishe.Editor {
public static class CeilingSceneUpdate {
    public static void Install() {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/House.unity");
        var architecture=GameObject.Find("Architecture").transform;
        var old=architecture.Cast<Transform>().Where(t=>t.name=="Upper ceiling").ToArray();
        var finish=old.First().GetComponent<Renderer>().sharedMaterial;
        foreach(var tile in old) Object.DestroyImmediate(tile.gameObject);
        HouseBuilder.BuildUpperCeiling(architecture,finish);

        var attic=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include)
            .Single(t=>t.name=="Attic / upper east wing");
        foreach(var rafter in attic.Cast<Transform>().Where(t=>t.name=="Turned rail").ToArray())
            Object.DestroyImmediate(rafter.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        HouseSceneTools.PrepareVisibility();
        Debug.Log("TISHE_CEILING_INSTALLED");
    }
}
}
