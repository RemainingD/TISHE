using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Tishe.Editor {
public static class EntranceSceneUpdate {
    public static void Install(){
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/House.unity");
        var game=Object.FindFirstObjectByType<GameController>();
        Transform parent;
        Material plaster;
        if(game.Entrance!=null){
            parent=game.Entrance.transform.parent;
            plaster=game.Entrance.transform.Find("Plastered entrance reveal").GetComponent<Renderer>().sharedMaterial;
            Object.DestroyImmediate(game.Entrance.gameObject);
        }else{
            var all=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            var oldExit=Object.FindObjectsByType<GameItem>(FindObjectsSortMode.None).Single(item=>item.kind=="exit");
            parent=oldExit.transform.parent;
            var wall=all.Single(t=>t.name=="Plaster wall" && Vector3.Distance(t.position,new Vector3(0,1.98f,14.4f))<.02f);
            plaster=wall.GetComponent<MeshRenderer>().sharedMaterial;
            var obsolete=all.Where(t=>t==oldExit.transform||t==wall||t.name=="Exterior front door"||
                t.name=="Bolt keeper"||t.name.StartsWith("Exit bolt ")||t.name=="Door surround"||
                t.name=="Door recessed panel"||(t.name=="Door lintel" && Mathf.Abs(t.position.z-15.84f)<.02f)).ToArray();
            if(obsolete.Length!=20)throw new InvalidOperationException("Unexpected legacy door assembly: "+string.Join(", ",obsolete.Select(t=>t.name)));
            foreach(var o in obsolete)Object.DestroyImmediate(o.gameObject);
        }
        FrontEntrance.Build(game,parent,plaster);
        EditorUtility.SetDirty(game);EditorSceneManager.MarkSceneDirty(scene);
        if(!EditorSceneManager.SaveScene(scene))throw new IOException("Could not save the entrance");
        AssetDatabase.SaveAssets();StaticOcclusionCulling.Compute();EditorSceneManager.SaveScene(scene);
        Debug.Log("TISHE_FRONT_ENTRANCE_INSTALLED");
    }
}
}
