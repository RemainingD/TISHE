using UnityEngine;

namespace Tishe {
internal sealed class HousePrologue {
    readonly struct Shot {
        internal readonly string title, story;
        internal readonly Vector3 from,to,look;
        internal readonly float duration;
        internal Shot(string title,string story,float duration,Vector3 from,Vector3 to,Vector3 look){
            this.title=title;this.story=story;this.duration=duration;
            this.from=from;this.to=to;this.look=look;
        }
    }

    static readonly Shot[] shots={
        new Shot("Терезедегі жарық","Орманда адасып жүріп, терезеден жарық көрдім. Есікті үш рет қақтым. Есімде қалған соңғы дыбыс — іштен бұралған кілттің сыңғыры.",9.3f,
            new Vector3(-8.2f,2.95f,33),new Vector3(-6.8f,2.75f,30.5f),new Vector3(.3f,4.6f,15.4f)),
        new Shot("Үй тыңдап тұр","Бөтен үйде ояндым. Бұл үйден қашуға үш-ақ күнім бар. Аяғымның астында тақтай сықыр етті. Төмендегі аяқ дыбысы тына қалды. Ол мені естіді. Шкафқа жасырынуым керек.",11.5f,
            new Vector3(-7.15f,5.73f,2.54f),new Vector3(-7.72f,5.69f,2.28f),new Vector3(-12.48f,5.54f,1.16f)),
        new Shot("Бірінші ысырма","Сақтандырғыштың ұясы бос. Бөлмелерден сақтандырғыш тауып, қалқандағы тізбекті тұйықтасам, сыртқы есіктің бірінші ысырмасы ашылады.",9.6f,
            new Vector3(-10.87f,1.71f,1.46f),new Vector3(-11.17f,1.67f,1.88f),new Vector3(-11.76f,1.56f,3.56f)),
        new Shot("Есіктің ар жағында","Күзетшіге ұсталған сайын бір күнімнен айырыламын. Қалған төрт ысырма құлыпқа, ойын автоматына, радио мен желдеткішке жалғанған. Керек заттарды тауып, бес ысырманы да ашсам, орманға қашамын.",12.2f,
            new Vector3(-.72f,1.63f,10.51f),new Vector3(-.38f,1.53f,11.07f),new Vector3(.04f,1.38f,13.45f))
    };

    internal int Index {get;private set;}
    float elapsed,torchPower,shotDuration;
    bool torchWasOn;
    Texture2D gradient;
    GUIStyle heading,body,skip;
    readonly GUIContent copy=new GUIContent();
    static readonly Color parchment=new Color(.96f,.91f,.79f);
    internal void Start(FirstPerson player,HorrorAudio audio){
        Index=0;elapsed=0;torchPower=player.torch.intensity;torchWasOn=player.torch.enabled;
        StartShot(audio);
        Pose(player);
    }
    void StartShot(HorrorAudio audio){
        elapsed=0;
        shotDuration=Mathf.Max(shots[Index].duration,audio.PlayPrologueLine(Index)+.8f);
    }
    internal void Finish(FirstPerson player){
        player.torch.intensity=torchPower;player.torch.enabled=torchWasOn;
    }
    internal bool Advance(FirstPerson player,HorrorAudio audio,float dt){
        float before=elapsed;
        elapsed+=dt;
        if(elapsed>=shotDuration){
            if(Index==shots.Length-1)return true;
            Index++;
            StartShot(audio);
            before=0;
        }
        if(Index==1 && before<6.1f && elapsed>=6.1f)audio.PrologueFloorboard();
        Pose(player);return false;
    }
    void Pose(FirstPerson player){
        var shot=shots[Index];var camera=player.view.transform;
        float t=Mathf.SmoothStep(0,1,elapsed/shotDuration);
        camera.position=Vector3.Lerp(shot.from,shot.to,t);
        camera.rotation=Quaternion.LookRotation(shot.look-camera.position,Vector3.up);
        player.torch.enabled=Index>0;
        player.torch.intensity=torchPower*(Index==1?.54f:.34f);
    }
    static float Ease(float t)=>Mathf.SmoothStep(0,1,Mathf.Clamp01(t));
    void Styles(GUIStyle baseLabel){
        if(heading!=null)return;
        heading=new GUIStyle(baseLabel){font=Font.CreateDynamicFontFromOSFont(new[]{"Georgia","Cambria","Segoe UI"},51),fontSize=51,
            alignment=TextAnchor.MiddleLeft,padding=new RectOffset(),normal={textColor=parchment}};
        body=new GUIStyle(baseLabel){fontSize=24,wordWrap=true,alignment=TextAnchor.UpperLeft,padding=new RectOffset(),normal={textColor=new Color(.96f,.95f,.9f)}};
        skip=new GUIStyle(baseLabel){alignment=TextAnchor.MiddleCenter,fontSize=19,wordWrap=false,padding=new RectOffset(),normal={textColor=Color.white}};
        gradient=new Texture2D(1,64,TextureFormat.RGBA32,false){wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
        for(int y=0;y<64;y++){
            float alpha=1-Mathf.SmoothStep(0,1,y/63f);gradient.SetPixel(0,y,new Color(.023f,.029f,.031f,alpha*.96f));
        }
        gradient.Apply(false,true);
    }
    static void Solid(Rect rect,Color color){
        GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=Color.white;
    }
    internal bool Draw(GUIStyle baseLabel){
        Styles(baseLabel);
        var prev=GUI.matrix;float scale=Mathf.Min(Screen.width/1360f,Screen.height/800f);
        float w=Screen.width/scale,h=Screen.height/scale;
        GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
        var shot=shots[Index];float remaining=shotDuration-elapsed;
        float picture=Ease(elapsed/.85f)*Ease(remaining/.8f);
        GUI.DrawTexture(new Rect(0,h*.40f,w,h*.60f),gradient);
        Solid(new Rect(0,0,w,24),new Color(.025f,.03f,.032f,.92f));
        float left=Mathf.Max(48,w*.064f),width=Mathf.Min(w-left*2,870);
        copy.text=shot.story;
        float textHeight=body.CalcHeight(copy,width)+4;
        float baseline=h-76-textHeight;
        float opacity=Ease((elapsed-.38f)/.65f)*Ease((remaining-.2f)/.6f);
        float lift=(1-Ease((elapsed-.32f)/.95f))*10;
        GUI.color=new Color(1,1,1,opacity);
        GUI.Label(new Rect(left,baseline-73+lift,width,65),shot.title,heading);
        GUI.Label(new Rect(left,baseline+lift,width,textHeight),copy,body);
        GUI.color=Color.white;
        for(int i=0;i<shots.Length;i++){
            var mark=new Rect(left+i*44,h-45,36,2);
            Solid(mark,new Color(1,1,1,.25f));mark.width*=i<Index?1:i==Index?Mathf.Clamp01(elapsed/shotDuration):0;
            if(mark.width>0)Solid(mark,parchment);
        }
        Solid(new Rect(0,0,w,h),new Color(0,0,0,1-picture));
        var area=new Rect(w-left-212,41,212,45);
        Solid(area,new Color(.024f,.033f,.035f,area.Contains(Event.current.mousePosition)?.87f:.65f));
        bool pressed=GUI.Button(area,"Кіріспені өткізу",skip);
        GUI.matrix=prev;return pressed;
    }
    internal void Dispose(){if(gradient!=null)Object.Destroy(gradient);if(heading!=null && heading.font!=null)Object.Destroy(heading.font);}
}
}
