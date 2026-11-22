using System.Collections.Generic;
using UnityEngine;

namespace Tishe {
public static class ForestYard {
    public static float PathX(float z){
        float d=Mathf.Max(0,z-20);
        return Mathf.Sin(d*.105f)*Mathf.Min(d*.25f,5.4f)+.050f*d;
    }
    public static float GroundY(float x,float z){
        float wild=Mathf.SmoothStep(0,1,Mathf.InverseLerp(18,34,Mathf.Max(Mathf.Abs(x),Mathf.Abs(z))));
        float distance=Mathf.Abs(x-PathX(z));
        float amplitude=Mathf.Lerp(.25f,2.0f,Mathf.SmoothStep(0,1,Mathf.InverseLerp(2,8,distance)));
        return -.23f+wild*(Mathf.PerlinNoise(x*.07f+13,z*.065f+31)-.46f)*amplitude;
    }
    public static Vector3 Trail(float z)=>new Vector3(PathX(z),GroundY(PathX(z),z),z);

    sealed class Geometry {
        readonly List<Vector3> vertices=new List<Vector3>();
        readonly List<Vector2> uv=new List<Vector2>();
        readonly List<int> indices=new List<int>();
        public void Limb(Vector3 from,Vector3 to,float bottom,float top,int sides){
            Vector3 up=(to-from).normalized;
            Vector3 right=Vector3.Cross(up,Mathf.Abs(up.z)>.9f?Vector3.right:Vector3.forward).normalized;
            Vector3 around=Vector3.Cross(up,right);
            int start=vertices.Count;
            for(int row=0;row<2;row++)for(int i=0;i<=sides;i++){
                float a=i*2*Mathf.PI/sides;
                vertices.Add((row==0?from:to)+(right*Mathf.Cos(a)+around*Mathf.Sin(a))*(row==0?bottom:top));
                uv.Add(new Vector2(i/(float)sides,(row==0?from.y:to.y)*.65f));
                if(row==0 && i<sides){int n=start+i;indices.AddRange(new[]{n,n+1,n+sides+1,n+1,n+sides+2,n+sides+1});}
            }
        }
        public void Frond(Vector3 start,Vector3 mid,Vector3 end,Vector3 cross,Rect region){
            int n=vertices.Count;
            Vector3[] spine={start,mid,end};
            for(int i=0;i<3;i++){
                float v=region.y+region.height*i*.5f;
                vertices.Add(spine[i]-cross);vertices.Add(spine[i]+cross);
                uv.Add(new Vector2(region.x,v));uv.Add(new Vector2(region.xMax,v));
            }
            indices.AddRange(new[]{n,n+2,n+1,n+1,n+2,n+3,n+2,n+4,n+3,n+3,n+4,n+5});
        }
        public Mesh Finish(string name){
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);
            mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;
        }
    }
    static GameObject Draw(Transform parent,string name,Mesh mesh,Material mat){
        var o=new GameObject(name);o.transform.SetParent(parent,false);
        o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=mat;return o;
    }
    static Texture2D Texture(string name)=>Resources.Load<Texture2D>("Art/"+name);
    static Material Leaves(string name){
        var mat=new Material(Shader.Find("Tishe/Forest needles")){name=name+" foliage",mainTexture=Texture(name+"Color"),enableInstancing=true};
        mat.SetTexture("_Mask",Texture(name+"Alpha"));return mat;
    }
    static float Sample(System.Random rng,float min,float max)=>Mathf.Lerp(min,max,(float)rng.NextDouble());
    static void Tree(int variant,out Mesh timber,out Mesh needles,bool distant=false){
        var rng=new System.Random(477+variant);var wood=new Geometry();var foliage=new Geometry();
        bool fir=variant%2==0;float h=fir?14:17;
        var region=fir?Rect.MinMaxRect(.28f,.205f,.63f,.649f):Rect.MinMaxRect(.006f,.568f,.195f,.992f);
        Vector3 lean=new Vector3(Sample(rng,-.6f,.6f),0,Sample(rng,-.55f,.55f));
        Vector3 top=Vector3.up*h+lean;
        wood.Limb(Vector3.zero,top*.45f,.28f,.16f,11);wood.Limb(top*.45f,top,.16f,.022f,9);
        for(int r=0;r<7;r++){
            float a=r*2.4f+Sample(rng,-.22f,.22f);
            Vector3 spread=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*Sample(rng,.56f,.93f);
            wood.Limb(Vector3.up*.34f,spread+Vector3.down*.045f,.13f,.027f,6);
        }
        int levels=distant?6:fir?12:9;
        for(int layer=0;layer<levels;layer++){
            float t=layer/(float)levels,y=Mathf.Lerp(fir?.16f:.28f,.97f,t);
            int count=distant?5:fir?8:7;
            for(int b=0;b<count;b++){
                float a=b*2*Mathf.PI/count+layer*2.40f+Sample(rng,-.25f,.25f);
                Vector3 outwards=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)),side=Vector3.Cross(Vector3.up,outwards);
                Vector3 basePoint=top*y+Vector3.up*Sample(rng,-.22f,.22f);
                float span=(fir?3.1f:3.5f)*Mathf.Pow(1-t,.66f)*Sample(rng,.68f,1.18f);
                Vector3 tip=basePoint+outwards*span+Vector3.up*(fir?-.30f:.62f)*span;
                wood.Limb(basePoint,tip,.055f*(1-t)+.012f,.009f,5);
                int sprays=distant?2:5;
                for(int j=0;j<sprays;j++){
                    float along=distant?.25f+j*.45f:.19f+j*.17f;
                    Vector3 anchor=Vector3.Lerp(basePoint,tip,along);
                    for(int s=-1;s<=1;s+=2){
                        float len=(.52f+.95f*(1-along))*Mathf.Lerp(1.25f,.50f,t);
                        Vector3 forward=(outwards*.70f+side*s*.75f).normalized;
                        Vector3 end=anchor+forward*len+Vector3.up*(fir?-.31f:.40f)*len;
                        Vector3 width=Vector3.Cross(Vector3.up,forward)*len*(fir?.41f:.24f)*(distant?1.6f:1f);
                        foliage.Frond(anchor,(anchor+end)*.5f+Vector3.up*.16f,end,width,region);
                    }
                }
            }
        }
        timber=wood.Finish("Tapered trunk and boughs "+variant);needles=foliage.Finish("Needle sprays "+variant);
    }
    static Mesh Fern(){
        var geometry=new Geometry();
        for(int i=0;i<10;i++){
            float a=i*2.40f;Vector3 dir=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
            float len=.51f+.24f*Mathf.Sin(i*5),rise=.24f+.17f*Mathf.Cos(i*2.3f);
            geometry.Frond(Vector3.zero,dir*(len*.43f)+Vector3.up*rise,dir*len+Vector3.up*(rise*.64f),
                Vector3.Cross(Vector3.up,dir)*(.09f+len*.045f),Rect.MinMaxRect(.285f,.035f,.465f,.96f));
        }
        return geometry.Finish("Fern rosette");
    }
    public static void SetUpDistanceDetail(Camera camera){
        var yard=GameObject.Find("Forest surrounding the keeper's house");
        if(yard==null)return;
        const int groundCoverLayer=10;
        var limits=camera.layerCullDistances;limits[groundCoverLayer]=38f;
        camera.layerCullDistances=limits;camera.layerCullSpherical=true;
        var trunks=new Mesh[4];var crowns=new Mesh[4];
        for(int i=0;i<4;i++)Tree(i,out trunks[i],out crowns[i],true);
        foreach(Transform plant in yard.transform){
            if(plant.name=="Young spruce"||plant.name=="Forest fern"){
                plant.gameObject.layer=groundCoverLayer;continue;
            }
            if(plant.name!="Forest conifer")continue;
            var filter=plant.GetComponent<MeshFilter>();
            var meshName=filter.sharedMesh.name;
            int variant=meshName[meshName.Length-1]-'0';
            if(variant<0||variant>=4)continue;
            var trunk=plant.GetComponent<MeshRenderer>();
            var crown=plant.Find("Crown").GetComponent<MeshRenderer>();
            var lowTrunk=Draw(plant,"Distant trunk",trunks[variant],trunk.sharedMaterial).GetComponent<MeshRenderer>();
            var lowCrown=Draw(plant,"Distant crown",crowns[variant],crown.sharedMaterial).GetComponent<MeshRenderer>();
            var lod=plant.gameObject.AddComponent<LODGroup>();
            lod.SetLODs(new[]{new LOD(.30f,new Renderer[]{trunk,crown}),new LOD(.013f,new Renderer[]{lowTrunk,lowCrown})});
            lod.RecalculateBounds();
        }
    }
    static void Land(Transform root){
        const int width=108,length=124;const float step=1.25f;
        var v=new List<Vector3>();var uv=new List<Vector2>();var colors=new List<Color>();var tri=new List<int>();
        for(int j=0;j<=length;j++)for(int i=0;i<=width;i++){
            float x=(i-width/2)*step,z=-45+j*step;
            v.Add(new Vector3(x,GroundY(x,z),z));uv.Add(new Vector2(x,z)*.38f);
            float d=Mathf.Abs(x-PathX(z))+(Mathf.PerlinNoise(x*1.12f,z*.65f)-.5f)*.9f;
            float path=z>15?1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.7f,2.5f,d)):0;
            colors.Add(new Color(path,.69f+.27f*Mathf.PerlinNoise(x*.25f,z*.25f),0,1));
            if(i<width&&j<length){int n=j*(width+1)+i;tri.AddRange(new[]{n,n+width+1,n+1,n+1,n+width+1,n+width+2});}
        }
        var mesh=new Mesh{name="Undulating forest floor and worn footpath"};mesh.SetVertices(v);mesh.SetColors(colors);mesh.SetUVs(0,uv);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateTangents();
        var mat=new Material(Shader.Find("Tishe/Forest ground")){name="Damp litter and trail",mainTexture=Texture("ForestFloorColor")};
        mat.SetTexture("_Trail",Texture("TrailColor"));mat.SetTexture("_Normal",Texture("ForestFloorNormal"));Draw(root,"Hollow and forest trail",mesh,mat);
    }
    public static void Build(){
        var root=new GameObject("Forest surrounding the keeper's house").transform;Land(root);
        var bark=ReferenceArt.Surface("Bark",new Color(.66f,.62f,.57f));bark.enableInstancing=true;
        var leaves=new[]{Leaves("Fir"),Leaves("Pine")};var trunks=new Mesh[4];var crowns=new Mesh[4];
        for(int i=0;i<4;i++)Tree(i,out trunks[i],out crowns[i]);
        var rng=new System.Random(816);
        for(int row=0;row<18;row++)for(int col=0;col<18;col++){
            float x=-60+col*7+Sample(rng,-2.7f,2.7f),z=-39+row*7.8f+Sample(rng,-3,3);
            if(Mathf.Abs(x)<18 && z<22 && z>-18)continue;
            if(z>14 && z<77 && Mathf.Abs(x-PathX(z))<(z<32?5.5f:3.8f))continue;
            if(x>-22&&x<-15&&z>23&&z<39)continue; // view from the main menu
            if(z>21 && z<34 && Mathf.Abs(x-(-19f*(z-13f)/22f))<2.3f)continue;
            int k=rng.Next(4);float scale=Sample(rng,.55f,1.25f);
            var tree=Draw(root,"Forest conifer",trunks[k],bark);
            tree.transform.SetPositionAndRotation(new Vector3(x,GroundY(x,z)-.04f,z),Quaternion.Euler(0,rng.Next(360),0));
            tree.transform.localScale=new Vector3(scale,scale*Sample(rng,.85f,1.18f),scale);
            Draw(tree.transform,"Crown",crowns[k],leaves[k%2]);
        }
        var deadfall=new Geometry();
        deadfall.Limb(Vector3.zero,new Vector3(3.4f,.19f,.42f),.18f,.085f,10);
        for(int i=1;i<5;i++){
            Vector3 joint=new Vector3(.62f*i,.1f,.09f*i);
            deadfall.Limb(joint,joint+new Vector3(.18f,.31f,i%2==0?.56f:-.42f),.065f,.019f,6);
        }
        var fallen=deadfall.Finish("Fallen pine with snapped limbs");
        for(int i=0;i<29;i++){
            float z=Sample(rng,20,93),side=i%2==0?-1:1,x=PathX(z)+side*Sample(rng,4.1f,18);
            var log=Draw(root,"Windfallen pine",fallen,bark);
            log.transform.SetPositionAndRotation(new Vector3(x,GroundY(x,z)+.09f,z),Quaternion.Euler(0,side>0?Sample(rng,-55,55):Sample(rng,125,235),0));
        }
        var brush=new Geometry();
        for(int i=0;i<19;i++){
            float a=i*2.4f,len=Sample(rng,.36f,1.25f);
            Vector3 dir=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
            Vector3 stem=dir*len+Vector3.up*Sample(rng,.35f,1.3f);
            for(int j=0;j<3;j++){
                float t=.08f+j*.26f;
                Vector3 start=Vector3.Lerp(Vector3.zero,stem,t),end=start+stem*.38f;
                brush.Frond(start,(start+end)*.5f+Vector3.up*.11f,end,Vector3.Cross(Vector3.up,dir)*.16f,Rect.MinMaxRect(.28f,.205f,.63f,.649f));
            }
        }
        Mesh sapling=brush.Finish("Spruce undergrowth");
        for(int i=0;i<215;i++){
            float z=Sample(rng,19,92),x=PathX(z)+(i%2==0?-1:1)*Sample(rng,2.5f,21);
            var shrub=Draw(root,"Young spruce",sapling,leaves[0]);shrub.transform.SetPositionAndRotation(new Vector3(x,GroundY(x,z),z),Quaternion.Euler(0,rng.Next(360),0));
            shrub.transform.localScale=Vector3.one*Sample(rng,.48f,1.65f);
        }
        var fern=Fern();var fernMat=Leaves("Fern");
        for(int i=0;i<380;i++){
            float z=Sample(rng,18,90),x=PathX(z)+(i%2==0?1:-1)*Sample(rng,1.65f,19f);
            var f=Draw(root,"Forest fern",fern,fernMat);f.transform.SetPositionAndRotation(new Vector3(x,GroundY(x,z),z),Quaternion.Euler(0,rng.Next(360),0));f.transform.localScale=Vector3.one*Sample(rng,.33f,1.24f);
        }
    }
}
}
