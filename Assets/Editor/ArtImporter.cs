using UnityEditor;
namespace Tishe.Editor {
public class ArtImporter : AssetPostprocessor {
    public override uint GetVersion(){return 2;}
    void OnPreprocessTexture(){if(!assetPath.Contains("/Resources/Art/")&&!assetPath.Contains("/Resources/Models/"))return;var t=(TextureImporter)assetImporter;t.maxTextureSize=2048;t.wrapMode=UnityEngine.TextureWrapMode.Repeat;t.anisoLevel=8;t.filterMode=UnityEngine.FilterMode.Trilinear;t.mipmapEnabled=true;if(assetPath.Contains("Normal."))t.textureType=TextureImporterType.NormalMap;}
}
}

