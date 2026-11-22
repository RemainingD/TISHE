using UnityEngine;
namespace Tishe {
[RequireComponent(typeof(Camera))] public class CinemaLook : MonoBehaviour {
    Material material;
    void OnEnable(){var shader=Resources.Load<Shader>("Cinema");if(shader!=null&&shader.isSupported)material=new Material(shader);}
    void OnRenderImage(RenderTexture source,RenderTexture destination){if(material!=null)Graphics.Blit(source,destination,material);else Graphics.Blit(source,destination);}
    void OnDisable(){if(material!=null)Destroy(material);}
}
}
