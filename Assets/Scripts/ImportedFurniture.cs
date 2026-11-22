using System.Collections.Generic;
using UnityEngine;
namespace Tishe {
public static class ImportedFurniture {
    static readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
    public static bool Place(string asset,Vector3 position,float yaw,float height,Vector3? fit=null,bool collision=true){var source=Resources.Load<GameObject>("Models/"+asset+"/Model");if(source==null)return false;var group=new GameObject("Detailed furniture / "+asset);var model=Object.Instantiate(source,group.transform);var rs=model.GetComponentsInChildren<Renderer>();if(rs.Length==0){SceneObjects.Remove(group);return false;}
        if(!materials.TryGetValue(asset,out var mat)){mat=new Material(Shader.Find("Standard"));mat.color=new Color(.85f,.85f,.85f);mat.mainTexture=Resources.Load<Texture2D>("Models/"+asset+"/Color");mat.SetTexture("_BumpMap",Resources.Load<Texture2D>("Models/"+asset+"/Normal"));mat.EnableKeyword("_NORMALMAP");mat.SetFloat("_BumpScale",.7f);mat.SetFloat("_Glossiness",.15f);materials[asset]=mat;}
        if(asset=="GothicCabinet_01"){
            // The four leaves ship ajar around their own hinges. Keep their imported pivots.
            var panels=model.GetComponentsInChildren<Transform>();
            var body=System.Array.Find(panels,t=>t.name=="GothicCabinet_01_cabinet");
            if(body!=null)foreach(var panel in panels)
                if(panel.name.StartsWith("GothicCabinet_01_door"))panel.localRotation=body.localRotation;
        }
        var bounds=rs[0].bounds;foreach(var r in rs){bounds.Encapsulate(r.bounds);var slots=r.sharedMaterials;for(int i=0;i<slots.Length;i++)slots[i]=mat;r.sharedMaterials=slots;}
        model.transform.localScale*=height/Mathf.Max(.01f,bounds.size.y);bounds=rs[0].bounds;foreach(var r in rs)bounds.Encapsulate(r.bounds);model.transform.localPosition-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
        if(collision){var collider=group.AddComponent<BoxCollider>();collider.center=Vector3.up*height/2;collider.size=new Vector3(bounds.size.x,height,bounds.size.z);}
        if(fit.HasValue){var size=fit.Value;group.transform.localScale=new Vector3(size.x/bounds.size.x,size.y/bounds.size.y,size.z/bounds.size.z);}
        group.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));return true;
    }
}
}
