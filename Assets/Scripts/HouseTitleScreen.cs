using UnityEngine;

namespace Tishe {
internal sealed class HouseTitleScreen {
    internal enum Choice { None=-1, Start, Settings, Help, Archive, Quit }
    internal event System.Action SelectionChanged;
    static readonly string[] Labels={"Ойынды бастау","Баптаулар","Ойын ережесі","Үй мұрағаты","Ойыннан шығу"};
    static readonly Color Paper=new Color32(234,223,203,255);
    static readonly Color Brass=new Color32(180,151,107,255);
    static readonly Color Vermilion=new Color32(218,77,59,255);
    Texture2D house, universityMark, rightShade, selectionWash;
    Font serif;
    GUIStyle masthead,subtitle,item,activeItem,legend,university,authors;
    int selected,pressed=-1;
    Vector2 lastPointer;
    bool pointerKnown,artLoaded,worldHidden;
    int worldMask;
    CameraClearFlags clearMode;

    // Use one uniform UI scale. Anchor the column to the right even on ultrawide displays.
    static float Scale=>Mathf.Min(Screen.height/800f,Screen.width/1070f);
    static float Width=>Screen.width/Scale;
    static float Height=>Screen.height/Scale;
    static float Centre=>Width-Mathf.Min(Width*.23f,335f);
    static float Top=>(Height-800)*.5f+125;
    internal static Rect Row(int i)=>new Rect(Centre-169,Top+218+56*i,338,49);

    internal void SetBackdrop(Camera camera,bool menu) {
        if(menu&&!artLoaded){
            house=Resources.Load<Texture2D>("Menu/KeeperHouse");
            universityMark=Resources.Load<Texture2D>("Menu/QairuWordmark");
            artLoaded=true;
        }
        bool hide=menu&&house!=null;
        if(hide==worldHidden)return;
        if(hide){
            worldMask=camera.cullingMask;clearMode=camera.clearFlags;
            camera.cullingMask=0;camera.clearFlags=CameraClearFlags.SolidColor;
        }else{camera.cullingMask=worldMask;camera.clearFlags=clearMode;}
        worldHidden=hide;
    }
    internal Choice Navigate(Vector2 pointer,bool down,bool up,int step,bool accept) {
        int previous=selected;
        int over=-1;
        for(int i=0;i<Labels.Length;i++)if(Row(i).Contains(pointer)){over=i;break;}
        if(!pointerKnown || (pointer-lastPointer).sqrMagnitude>.01f || down){
            if(over>=0)selected=over;
            lastPointer=pointer;pointerKnown=true;
        }
        if(step!=0){selected=(selected+Labels.Length+step)%Labels.Length;pressed=-1;}
        if(selected!=previous)SelectionChanged?.Invoke();
        if(down)pressed=over;
        if(up){
            int origin=pressed;pressed=-1;
            if(over>=0 && over==origin)return (Choice)over;
        }
        return accept?(Choice)selected:Choice.None;
    }
    internal Choice ReadInput() {
        Vector2 p=Input.mousePosition;
        p=new Vector2(p.x,Screen.height-p.y)/Scale;
        int step=0;
        if(Input.GetKeyDown(KeyCode.UpArrow)||Input.GetKeyDown(KeyCode.W))step=-1;
        if(Input.GetKeyDown(KeyCode.DownArrow)||Input.GetKeyDown(KeyCode.S))step=1;
        return Navigate(p,Input.GetMouseButtonDown(0),Input.GetMouseButtonUp(0),step,
            Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.KeypadEnter)||Input.GetKeyDown(KeyCode.Space));
    }

    static Texture2D Gradient(bool centre) {
        var t=new Texture2D(128,1,TextureFormat.RGBA32,false){wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
        for(int i=0;i<t.width;i++){
            float x=(float)i/(t.width-1);
            float a=centre?1-Mathf.SmoothStep(0,1,Mathf.Abs(2*x-1)):Mathf.SmoothStep(0,1,Mathf.InverseLerp(.42f,.90f,x));
            t.SetPixel(i,0,new Color(1,1,1,a));
        }
        t.Apply(false,true);return t;
    }
    static GUIStyle Letter(Font font,int size,Color color,FontStyle style=FontStyle.Normal) {
        var label=new GUIStyle{font=font,fontSize=size,fontStyle=style,alignment=TextAnchor.MiddleCenter,wordWrap=false,clipping=TextClipping.Overflow};
        label.normal.textColor=color;return label;
    }
    void Prepare(Font ui){
        if(masthead!=null)return;
        serif=Font.CreateDynamicFontFromOSFont(new[]{"Georgia","Times New Roman","Cambria"},100);
        var face=serif!=null?serif:ui;
        masthead=Letter(face,99,Vermilion);
        subtitle=Letter(ui,20,Paper);
        item=Letter(ui,22,new Color32(231,226,215,255));
        activeItem=Letter(ui,23,new Color32(255,236,211,255),FontStyle.Bold);
        legend=Letter(ui,16,new Color32(223,216,201,255));
        university=Letter(ui,17,Paper);
        authors=Letter(face,26,Paper,FontStyle.Italic);
        university.alignment=authors.alignment=TextAnchor.MiddleLeft;
        rightShade=Gradient(false);selectionWash=Gradient(true);
    }
    static void Tint(Rect r,Texture2D texture,Color color){
        GUI.color=color;GUI.DrawTexture(r,texture);GUI.color=Color.white;
    }
    static void Rule(float x,float y,float w,float h,Color color){
        Tint(new Rect(x,y,w,h),Texture2D.whiteTexture,color);
    }
    static void Diamond(float x,float y,float size,Color color){
        int rows=Mathf.RoundToInt(size);
        for(int i=0;i<rows;i++){
            float span=size-2*Mathf.Abs(i+.5f-size*.5f);
            Rule(x-span*.5f,y-size*.5f+i,span,1,color);
        }
    }
    internal void Draw(Font ui,bool showChoices) {
        var saved=GUI.matrix;var tint=GUI.color;
        Prepare(ui);
        GUI.matrix=Matrix4x4.Scale(new Vector3(Scale,Scale,1));GUI.color=Color.white;
        float w=Width,h=Height,c=Centre,y=Top;
        if(house!=null){
            float drift=Mathf.Sin(Time.unscaledTime*.095f);
            GUI.DrawTexture(new Rect(-w*.012f+w*.005f*drift,-h*.010f,w*1.024f,h*1.02f),house,ScaleMode.ScaleAndCrop);
        }
        Tint(new Rect(0,0,w,h),rightShade,new Color(.018f,.012f,.014f,.69f));
        if(showChoices){
            const float margin=43;
            if(universityMark!=null)
                GUI.DrawTexture(new Rect(margin,h-143,146,35),universityMark,ScaleMode.ScaleToFit);
            GUI.Label(new Rect(margin,h-105,280,26),"Qazaq AI Research University",university);
            Rule(margin,h-75,44,1,Brass);
            GUI.Label(new Rect(margin,h-67,280,36),"Damir & Bereket",authors);
            GUI.Label(new Rect(c-225,y,450,115),"ТЫНЫШ",masthead);
            GUI.Label(new Rect(c-190,y+111,380,38),"К Ү З Е Т Ш І Н І Ң   Ү Й І",subtitle);
            Rule(c-106,y+176,89,1,Brass);Rule(c+17,y+176,89,1,Brass);
            Diamond(c,y+176,8,Brass);Diamond(c,y+176,4,new Color32(59,37,30,255));
            for(int i=0;i<Labels.Length;i++){
                Rect r=Row(i);bool active=i==selected;
                if(active){
                    Tint(r,selectionWash,new Color(.47f,.13f,.085f,.64f));
                    var accent=new Color32(213,118,84,255);
                    Rule(r.x+12,r.center.y,24,1,accent);Rule(r.xMax-36,r.center.y,24,1,accent);
                    Diamond(r.x+38,r.center.y,4,accent);Diamond(r.xMax-38,r.center.y,4,accent);
                }
                GUI.Label(r,Labels[i],active?activeItem:item);
            }
            Rule(c-143,h-94,286,1,new Color(.67f,.59f,.49f,.39f));
            GUI.Label(new Rect(c-175,h-83,350,33),"↑ ↓   Таңдау       ·       Enter   Растау",legend);
        }
        GUI.matrix=saved;GUI.color=tint;
    }
    internal void Dispose(){
        if(rightShade!=null)Object.Destroy(rightShade);
        if(selectionWash!=null)Object.Destroy(selectionWash);
        if(serif!=null)Object.Destroy(serif);
    }
}
}
