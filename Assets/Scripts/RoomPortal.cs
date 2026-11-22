using UnityEngine;
namespace Tishe {
// Fits the existing graph cell, but leaves a human-sized, framed opening.
public static class RoomPortal {
    internal static readonly Vector3 LeafSize=new Vector3(1.36f,2.7f,.085f);
    public static void Build(GameController game, Transform parent, Vector3 position, float yaw, float wallHeight, Material wall, Material wood, Material iron) {
        var root=new GameObject("Framed room doorway").transform;
        root.SetParent(parent,false);root.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));
        const float opening=1.44f, height=2.75f, depth=2.4f;
        float jamb=(HouseMap.Cell-opening)/2;
        foreach(float side in new[]{-1f,1f}) {
            ReferenceArt.Box(root,"Solid doorway reveal",new Vector3(side*(opening+jamb)/2,height/2,0),new Vector3(jamb,height,depth),wall,true);
        }
        ReferenceArt.Box(root,"Solid lintel above doorway",new Vector3(0,(height+wallHeight)/2,0),new Vector3(HouseMap.Cell,wallHeight-height,depth),wall,true);
        foreach(float face in new[]{-1f,1f}) {
            foreach(float side in new[]{-1f,1f}) {
                ReferenceArt.Box(root,"Door casing",new Vector3(side*(opening/2+.035f),height/2,face*1.205f),new Vector3(.14f,height,.10f),wood);
                ReferenceArt.Box(root,"Carved casing bead",new Vector3(side*(opening/2+.06f),height/2,face*1.27f),new Vector3(.035f,height,.025f),iron);
            }
            ReferenceArt.Box(root,"Door casing header",new Vector3(0,height+.04f,face*1.205f),new Vector3(opening+.24f,.17f,.10f),wood);
        }
        // Keep the open leaf inside the reveal so it cannot block the adjacent navigation cell.
        var hinge=new GameObject("Door hinge").transform;hinge.SetParent(root,false);hinge.localPosition=new Vector3(-.68f,.015f,0);
        var door=root.gameObject.AddComponent<HouseDoor>();door.leaf=hinge;
        var item=root.gameObject.AddComponent<GameItem>();item.kind="door";item.title="Есікті ашу / жабу";
        ReferenceArt.Box(hinge,"Solid oak door leaf",new Vector3(LeafSize.x/2,LeafSize.y/2,0),LeafSize,wood,true);
        foreach(float face in new[]{-1f,1f}) {
            for(int row=0;row<3;row++) {
                var p=new Vector3(.68f,.48f+row*.82f,face*.055f);
                ReferenceArt.Box(hinge,"Inset oak panel",p,new Vector3(1.08f,.65f,.028f),wood);
                foreach(float side in new[]{-1f,1f})ReferenceArt.Box(hinge,"Panel moulding",p+new Vector3(side*.54f,0,face*.02f),new Vector3(.035f,.69f,.025f),wood);
                foreach(float edge in new[]{-1f,1f})ReferenceArt.Box(hinge,"Panel moulding",p+new Vector3(0,edge*.33f,face*.02f),new Vector3(1.11f,.035f,.025f),wood);
            }
            ReferenceArt.Box(hinge,"Handle escutcheon",new Vector3(1.21f,1.22f,face*.065f),new Vector3(.07f,.22f,.025f),iron);
            ReferenceArt.Rod(hinge,new Vector3(1.21f,1.26f,face*.14f),new Vector3(1.04f,1.26f,face*.14f),.022f,iron);
        }
        game.Doors.Add(door);
    }
}
}
