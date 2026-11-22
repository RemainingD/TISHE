using UnityEngine;

namespace Tishe {
internal sealed class WakeDayTitle {
    static readonly string[] Days={"Бірінші күн","Екінші күн","Үшінші күн"};
    static readonly string[] Thoughts={"Қашуға үш күнім бар.","Екі күнім қалды.","Бүгін — соңғы мүмкіндігім."};
    static readonly Color[] Cuts={new Color32(210,164,105,255),new Color32(219,143,91,255),new Color32(233,104,81,255)};
    const float End=3.12f;
    Texture2D door,notch;
    Font serif;
    GUIStyle headline,thought;

    static float Fade(float from,float to,float time)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(from,to,time));
    static Texture2D CutMark(){
        // A tapered cut with an irregular edge and a thin ridge of exposed wood.
        const int width=32,height=96;
        var texture=new Texture2D(width,height,TextureFormat.RGBA32,false){wrapMode=TextureWrapMode.Clamp};
        var pixels=new Color32[width*height];
        for(int y=0;y<height;y++){
            float t=y/(height-1f);
            float centre=width*(.25f+.51f*t)+1.15f*Mathf.Sin(t*12);
            float radius=Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*Mathf.PI)),.62f)*(3.25f+.65f*Mathf.Sin(t*37));
            for(int x=0;x<width;x++){
                float side=x-centre;
                float alpha=Mathf.Clamp01(radius-Mathf.Abs(side));
                float grain=.79f+.12f*Mathf.Sin(t*89)+.09f*Mathf.Sin(t*47);
                float value=side<-.45f*radius?.39f:grain;
                pixels[y*width+x]=new Color(value,value,value,alpha);
            }
        }
        texture.SetPixels32(pixels);texture.Apply(false,true);return texture;
    }
    void Prepare(Font ui){
        if(headline!=null)return;
        door=Resources.Load<Texture2D>("Menu/WakeDoor");
        notch=CutMark();
        serif=Font.CreateDynamicFontFromOSFont(new[]{"Georgia","Cambria","Times New Roman","Segoe UI"},92);
        headline=new GUIStyle{font=serif!=null?serif:ui,fontSize=92,alignment=TextAnchor.MiddleLeft};
        headline.normal.textColor=Color.white;
        thought=new GUIStyle(headline){font=ui,fontSize=26};
    }
    static void DrawLabel(Rect rect,string label,GUIStyle style,Color ink,float alpha){
        GUI.color=new Color(ink.r,ink.g,ink.b,alpha);
        GUI.Label(rect,label,style);
    }
    internal void Draw(Font ui,int lives,float time){
        if(time>=End || Event.current.type!=EventType.Repaint)return;
        Prepare(ui);
        int day=Mathf.Clamp(3-lives,0,2);
        float scale=Mathf.Min(Screen.width/1360f,Screen.height/800f);
        float w=Screen.width/scale,h=Screen.height/scale;
        var oldMatrix=GUI.matrix;var oldColor=GUI.color;
        GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));

        float outFade=1-Fade(2.05f,End,time);
        float photo=Fade(0,.42f,time)*outFade;
        float drift=1+.023f*Fade(0,End,time);
        GUI.color=new Color(1,1-day*.035f,1-day*.065f,photo);
        if(door!=null)GUI.DrawTexture(new Rect((1-drift)*w*.5f,(1-drift)*h*.5f,w*drift,h*drift),door,ScaleMode.ScaleAndCrop);

        float left=(w-1360)*.5f+148;
        float middle=h*.50f;
        float titleAlpha=Fade(.17f,.57f,time)*outFade;
        float cutTop=middle-114;
        var accent=Cuts[day];
        for(int i=0;i<=day;i++){
            float reveal=i==day?Fade(.12f,.68f,time):1;
            if(reveal<=0)continue;
            float height=52-i%2*5;
            GUI.color=new Color(accent.r,accent.g,accent.b,photo);
            GUI.DrawTextureWithTexCoords(new Rect(left+19*i,cutTop+i%2*3,19,height*reveal),notch,new Rect(0,1-reveal,1,reveal));
        }
        var titleRect=new Rect(left-3,middle-57+7*(1-Fade(.17f,.65f,time)),1030,122);
        var shadow=titleRect;shadow.x+=2;shadow.y+=3;
        DrawLabel(shadow,Days[day],headline,Color.black,.8f*titleAlpha);
        DrawLabel(titleRect,Days[day],headline,new Color32(242,225,198,255),titleAlpha);

        float subAlpha=Fade(.37f,.80f,time)*outFade;
        GUI.color=new Color(accent.r,accent.g,accent.b,subAlpha);
        GUI.DrawTexture(new Rect(left,middle+92,35,2),Texture2D.whiteTexture);
        DrawLabel(new Rect(left+51,middle+70,810,46),Thoughts[day],thought,new Color32(233,214,189,255),subAlpha);
        GUI.color=oldColor;GUI.matrix=oldMatrix;
    }
    internal void Dispose(){
        if(notch!=null)Object.Destroy(notch);
        if(serif!=null)Object.Destroy(serif);
    }
}
}
