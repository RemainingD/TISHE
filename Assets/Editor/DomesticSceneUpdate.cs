using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Tishe.Editor {
public static class DomesticSceneUpdate {
    public static void Install() {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/House.unity");
        var decor=GameObject.Find("REFERENCE INTERIORS / rooms and exterior").transform;
        var previous=decor.Find("Everyday household fittings");
        if(previous!=null)Object.DestroyImmediate(previous.gameObject);
        DomesticInterior.Install(GameObject.Find("Architecture").transform.parent,decor);
        var probe=decor.GetComponentInChildren<ReflectionProbe>(true);
        if(probe!=null && probe.name=="Basin surroundings"){
            const string path="Assets/Resources/Art/BasinReflection.exr";
            probe.mode=ReflectionProbeMode.Baked;
            if(!Lightmapping.BakeReflectionProbe(probe,path))
                throw new System.InvalidOperationException("Cannot capture the basin surroundings");
            AssetDatabase.ImportAsset(path);
            probe.mode=ReflectionProbeMode.Custom;
            probe.customBakedTexture=AssetDatabase.LoadAssetAtPath<Cubemap>(path);
            if(probe.customBakedTexture==null)throw new System.InvalidOperationException("Missing basin reflection cubemap");
        }
        EditorSceneManager.MarkSceneDirty(scene);
        HouseSceneTools.PrepareVisibility();
        Debug.Log("TISHE_DOMESTIC_INSTALLED");
    }
}
}
