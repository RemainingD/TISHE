using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tishe.Editor {
public static class LivingPassageUpdate {
    public static void Install(){
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/House.unity");
        var roots=scene.GetRootGameObjects();
        var rocker=roots.Single(o=>o.name=="Detailed furniture / Rockingchair_01");
        var table=roots.Single(o=>o.name=="Detailed furniture / CoffeeTable_01");
        rocker.transform.position=ReferenceArt.LivingRockerPosition;
        table.transform.position=ReferenceArt.LivingTablePosition;
        EditorSceneManager.MarkSceneDirty(scene);
        HouseSceneTools.PrepareVisibility();
    }
}
}
