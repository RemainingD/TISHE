using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Tishe.Editor {
public static class HouseSceneTools {
    [MenuItem("TISHE/Open House scene")]
    public static void Open(){if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene("Assets/Scenes/House.unity");}
    [InitializeOnLoadMethod] static void OnLoad(){EditorApplication.delayCall+=()=>{if(Application.isBatchMode||EditorApplication.isPlayingOrWillChangePlaymode||SessionState.GetBool("TisheOpened",false))return;SessionState.SetBool("TisheOpened",true);var scene=EditorSceneManager.GetActiveScene();if(scene.path==""&&!scene.isDirty&&File.Exists("Assets/Scenes/House.unity"))EditorSceneManager.OpenScene("Assets/Scenes/House.unity");};}
    // Produces a real editable scene rather than an empty runtime bootstrap.
    [MenuItem("TISHE/Rebuild reference house scene")]
    public static void Bake(){
        if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var director=new GameObject("TISHE • Game Director").AddComponent<GameController>();HouseBuilder.Build(director);
        Directory.CreateDirectory("Assets/Generated");
        var dependencies=new System.Collections.Generic.List<Object>(scene.GetRootGameObjects());if(RenderSettings.skybox!=null)dependencies.Add(RenderSettings.skybox);foreach(var obj in EditorUtility.CollectDependencies(dependencies.ToArray())){
            if(obj==null||EditorUtility.IsPersistent(obj))continue;
            string ext=obj is Material?".mat":obj is Mesh||obj is Texture2D?".asset":null;if(ext==null)continue;
            string name=obj.name;foreach(char c in Path.GetInvalidFileNameChars())name=name.Replace(c,'_');if(string.IsNullOrEmpty(name))name="Generated";
            AssetDatabase.CreateAsset(obj,AssetDatabase.GenerateUniqueAssetPath("Assets/Generated/"+name+ext));
        }
        Directory.CreateDirectory("Assets/Scenes");EditorSceneManager.SaveScene(scene,"Assets/Scenes/House.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/House.unity",true)};PrepareVisibility();AssetDatabase.SaveAssets();Debug.Log("TISHE_SCENE_BAKED");
    }
    public static void PrepareVisibility(){
        const string path="Assets/Scenes/House.unity";
        var scene=EditorSceneManager.GetActiveScene();
        if(scene.path!=path)scene=EditorSceneManager.OpenScene(path);
        var area=GameObject.Find("Interior visibility volume");
        if(area==null){
            area=new GameObject("Interior visibility volume");
            var volume=area.AddComponent<OcclusionArea>();
            volume.center=new Vector3(6,3,0);volume.size=new Vector3(49,17,35);
        }
        // The entire decorated root and standalone furniture are stationary.
        // Only solid structural surfaces may hide renderers behind them.
        var decor=GameObject.Find("REFERENCE INTERIORS / rooms and exterior").transform;
        var arch=GameObject.Find("Architecture").transform;
        foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)){
            if(!r.enabled || r.GetComponentInParent<NurseryMobile>()!=null)continue;
            bool structure=r.transform.IsChildOf(arch);
            bool furniture=r.transform.root.name.StartsWith("Detailed furniture");
            if(!structure && !furniture && !r.transform.IsChildOf(decor))continue;
            var flags=GameObjectUtility.GetStaticEditorFlags(r.gameObject)|StaticEditorFlags.OccludeeStatic;
            if(!structure)flags|=StaticEditorFlags.BatchingStatic;
            if(structure && (r.name=="Plaster wall"||r.name=="Timber floor"||r.name=="Upper ceiling"))
                flags|=StaticEditorFlags.OccluderStatic;
            GameObjectUtility.SetStaticEditorFlags(r.gameObject,flags);
        }
        EditorSceneManager.SaveScene(scene);
        StaticOcclusionCulling.Compute();
        EditorSceneManager.SaveScene(scene);
        Debug.Log("TISHE_VISIBILITY_BAKED");
    }
    public static void BakeAndBuild(){Bake();ProjectBuilder.Build();}
    [MenuItem("TISHE/4. Update house and build (keep scene backup)")]
    public static void UpdateWithBackup(){
        if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play mode before rebuilding.");
        var current=EditorSceneManager.GetActiveScene();
        if(current.isDirty&&!EditorSceneManager.SaveScene(current))return;
        Directory.CreateDirectory("Backups");
        if(!string.IsNullOrEmpty(current.path)&&File.Exists(current.path))File.Copy(current.path,"Backups/EditorScene-"+System.DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity",false);
        BakeAndBuild();
    }
}
}

