using UnityEngine;
namespace Tishe {
public static class SceneObjects {
    public static void Remove(Object value){if(Application.isPlaying)Object.Destroy(value);else Object.DestroyImmediate(value);}
}
}
