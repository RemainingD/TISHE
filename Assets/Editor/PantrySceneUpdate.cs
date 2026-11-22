using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tishe.Editor {
public static class PantrySceneUpdate {
    public static void Install(){
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/House.unity");
        var roots=scene.GetRootGameObjects();
        var decor=roots.Single(o=>o.name=="REFERENCE INTERIORS / rooms and exterior").transform;
        var attic=decor.Cast<Transform>().Single(t=>t.name=="Attic / upper east wing");
        var kit=new RoomHeirlooms();
        foreach(var part in attic.Cast<Transform>().Where(t=>t.name=="Storage trunk" || t.name=="Trunk iron band" ||
            t.name=="Covered furniture" || t.name=="Dust sheet" || t.name=="Pantry dresser" || t.name=="Linen chest" ||
            t.name=="Herb drying print").ToArray())Object.DestroyImmediate(part.gameObject);
        kit.Pantry(attic);
        if(!decor.Cast<Transform>().Any(t=>t.name=="Hall cabinet keepsakes")){
            var old=roots.Where(o=>o.name=="Detailed furniture / GothicCabinet_01").ToArray();
            if(old.Length!=2)throw new System.InvalidOperationException("Expected the two original corridor cabinets");
            foreach(var cabinet in old)Object.DestroyImmediate(cabinet);
            for(int floor=0;floor<2;floor++)kit.HallCabinet(decor,floor);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        HouseSceneTools.PrepareVisibility();
        Debug.Log("TISHE_PANTRY_AND_HALL_INSTALLED");
    }
}
}
