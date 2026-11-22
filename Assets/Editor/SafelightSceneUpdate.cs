using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tishe.Editor {
public static class SafelightSceneUpdate {
    public static void Install(){
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/House.unity");
        var decor=GameObject.Find("REFERENCE INTERIORS / rooms and exterior").transform;
        var previous=decor.Find("Safelights and developing room");
        if(previous!=null)Object.DestroyImmediate(previous.gameObject);
        SafelightRooms.Build(decor);
        EditorSceneManager.MarkSceneDirty(scene);HouseSceneTools.PrepareVisibility();
        Debug.Log("TISHE_SAFELIGHTS_INSTALLED");
    }
}
}
