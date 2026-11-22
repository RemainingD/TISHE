using System.Linq;
using UnityEditor.SceneManagement;

namespace Tishe.Editor {
public static class DisturbanceSceneUpdate {
    public static void Install(){
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/House.unity");
        var decor=scene.GetRootGameObjects().Single(o=>o.name=="REFERENCE INTERIORS / rooms and exterior").transform;
        var previous=decor.Find(RoomHeirlooms.DisturbanceGroup);
        if(previous!=null){
            UnityEngine.Object.DestroyImmediate(previous.gameObject);
            foreach(var wardrobe in scene.GetRootGameObjects().Select(o=>o.GetComponent<Wardrobe>())
                .Where(w=>w!=null && (w.transform.position-RoomHeirlooms.StairCupboardFeet).sqrMagnitude<.001f))
                UnityEngine.Object.DestroyImmediate(wardrobe.gameObject);
        }
        new RoomHeirlooms().Disturbances(decor);
        EditorSceneManager.MarkSceneDirty(scene);
        HouseSceneTools.PrepareVisibility();
    }
}
}
