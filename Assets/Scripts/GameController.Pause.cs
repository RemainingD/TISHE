using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tishe {
public partial class GameController {
    internal enum PausePage { Journal, Controls, Options, Exit }
    internal PausePage PauseTab {get;private set;}
    int pauseFocus;
    bool leavingHouse, pointerMoved;
    Vector2 lastPausePointer;
    Font pauseSerif;
    Texture2D pauseFade;
    GUIStyle pauseLetter;
    static readonly Color parchment=new Color32(241,232,214,255), antique=new Color32(224,212,186,255),
        ember=new Color32(225,120,92,255), divider=new Color32(138,120,98,107);
    static readonly string[] pauseActions={"Жалғастыру","Басқару","Баптаулар","Басты мәзірге шығу"};
    static readonly string[] controlKeys={"W A S D   /   Тінтуір","Shift   /   C","E","1–5   /   Q","F   /   R","G"};
    static readonly string[] controlNames={"Жүру және айналаға қарау","Жүгіру / отырып жүру","Затты алу, пайдалану, жасырыну","Затты таңдау / лақтыру","Қолшам / радио есебі","Қабылдағышты қалдыру немесе шақыру"};

    internal void OpenPause(){
        if(State!="playing"||Panel!="")return;
        Panel="pause";PauseTab=PausePage.Journal;pauseFocus=0;
        lastPausePointer=new Vector2(-1,-1);Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
    }
    internal void PauseBack(){
        if(leavingHouse)return;
        if(PauseTab==PausePage.Journal){LeavePanel();return;}
        pauseFocus=(int)PauseTab;PauseTab=PausePage.Journal;
    }
    internal void PauseAction(int id){
        if(leavingHouse||Panel!="pause")return;
        Audio.MenuConfirm();
        if(id==0){LeavePanel();return;}
        PauseTab=(PausePage)id;pauseFocus=4;
    }
    void SaveOptions(){
        PlayerPrefs.SetInt("EconomyGraphics",economy?1:0);PlayerPrefs.SetFloat("Volume",Volume);
        PlayerPrefs.SetFloat("Sensitivity",Sensitivity);PlayerPrefs.SetFloat("Brightness",Brightness);
        PlayerPrefs.SetInt("Scare",Scare?1:0);PlayerPrefs.Save();
    }
    internal void LeaveHouse(){
        if(Panel!="pause"||PauseTab!=PausePage.Exit||leavingHouse)return;
        leavingHouse=true;StartCoroutine(ReturnToTitle(Audio.MenuConfirm()));
    }
    IEnumerator ReturnToTitle(float click){
        yield return new WaitForSecondsRealtime(click);
        yield return SceneManager.LoadSceneAsync(gameObject.scene.buildIndex);
    }
    void DisposePause(){
        if(pauseSerif!=null)Destroy(pauseSerif);
        if(pauseFade!=null)Destroy(pauseFade);
    }
    void PreparePause(){
        if(pauseLetter!=null)return;
        pauseSerif=Font.CreateDynamicFontFromOSFont(new[]{"Georgia","Cambria","Times New Roman"},72);
        pauseLetter=new GUIStyle(body){padding=new RectOffset(),margin=new RectOffset(),alignment=TextAnchor.MiddleLeft};
        pauseFade=new Texture2D(96,1,TextureFormat.RGBA32,false){wrapMode=TextureWrapMode.Clamp};
        for(int i=0;i<96;i++)pauseFade.SetPixel(i,0,new Color(.036f,.030f,.030f,Mathf.Lerp(.94f,.33f,Mathf.SmoothStep(0,1,i/95f))));
        pauseFade.Apply(false,true);
    }
    void Ink(Rect r,string s,int size,Color color,bool serif=false,bool bold=false){
        pauseLetter.font=serif&&pauseSerif!=null?pauseSerif:body.font;
        pauseLetter.fontSize=size;pauseLetter.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;
        pauseLetter.normal.textColor=color;GUI.Label(r,s,pauseLetter);
    }
    void FocusPause(int id){
        if(id==pauseFocus)return;pauseFocus=id;Audio.MenuFocus();
    }
    bool PauseRow(Rect r,string label,int id,bool navigation=false){
        if(pointerMoved&&r.Contains(Event.current.mousePosition))FocusPause(id);
        bool focus=pauseFocus==id,active=navigation&&PauseTab!=PausePage.Journal&&(int)PauseTab==id;
        if(focus||active){
            Fill(r,focus?new Color32(101,50,41,225):new Color32(70,57,47,176));
            Fill(new Rect(r.x,r.y,3,r.height),focus?ember:antique);
        }
        Fill(new Rect(r.x,r.yMax-1,r.width,1),divider);
        Ink(new Rect(r.x+16,r.y,r.width-45,r.height),label,navigation?23:18,focus||active?parchment:antique,false,focus);
        if(focus)Ink(new Rect(r.xMax-28,r.y,22,r.height),"›",25,parchment);
        var e=Event.current;
        bool key=focus&&e.type==EventType.KeyDown&&(e.keyCode==KeyCode.Return||e.keyCode==KeyCode.KeypadEnter||e.keyCode==KeyCode.Space);
        if(key){e.Use();return true;}
        return GUI.Button(r,GUIContent.none,GUIStyle.none);
    }
    void DrawPause(){
        PreparePause();
        GUI.DrawTexture(new Rect(0,0,1360,800),pauseFade);
        var previous=GUI.matrix;
        float s=Mathf.Min(Screen.width/1120f,Screen.height/700f);
        GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1120*s)/2,(Screen.height-700*s)/2,0),Quaternion.identity,new Vector3(s,s,1));
        try{
            var e=Event.current;
            bool samplePointer=e.isMouse||e.type==EventType.Repaint;
            pointerMoved=samplePointer&&(lastPausePointer-e.mousePosition).sqrMagnitude>1;
            if(samplePointer)lastPausePointer=e.mousePosition;
            int count=PauseTab==PausePage.Exit?6:PauseTab==PausePage.Options?11:PauseTab==PausePage.Controls?5:4+inventory.Count;
            if(e.type==EventType.KeyDown&&(e.keyCode==KeyCode.UpArrow||e.keyCode==KeyCode.DownArrow||e.keyCode==KeyCode.Tab)){
                bool prev=e.keyCode==KeyCode.UpArrow||(e.keyCode==KeyCode.Tab&&e.shift);
                FocusPause((pauseFocus+count+(prev?-1:1))%count);e.Use();
            }
            if(PauseTab==PausePage.Journal&&e.type==EventType.KeyDown&&e.keyCode>=KeyCode.Alpha1&&e.keyCode<=KeyCode.Alpha5){
                SelectSlot((int)e.keyCode-(int)KeyCode.Alpha1);e.Use();
            }
            Ink(new Rect(74,99,292,29),"Т Ы Н Ы Ш",18,ember,false,true);
            Ink(new Rect(69,139,385,93),"Үзіліс",75,parchment,true);
            for(int i=0;i<4;i++){
                var r=new Rect(74,269+i*54+(i==3?16:0),330,48);
                if(PauseRow(r,pauseActions[i],i,true)){PauseAction(i);return;}
            }
            Ink(new Rect(74,592,365,28),PauseTab==PausePage.Journal?"Esc   ·   Ойынға оралу":"Esc   ·   Артқа",16,antique);
            Fill(new Rect(522,99,524,527),new Color32(26,27,25,241));
            Fill(new Rect(522,99,524,2),antique);
            Ink(new Rect(553,121,460,32),$"{4-Lives}-КҮН   /   3",16,antique,false,true);
            switch(PauseTab){
                case PausePage.Journal: DrawPauseJournal();break;
                case PausePage.Controls: DrawPauseControls();break;
                case PausePage.Options: DrawPauseOptions();break;
                case PausePage.Exit:
                    Ink(new Rect(552,174,460,72),"Басты мәзірге шығу",36,parchment,true);
                    Ink(new Rect(553,259,447,82),"Осы ойынның барысы сақталмайды. Келесі ойын бірінші күннен басталады.",21,parchment);
                    if(PauseRow(new Rect(553,435,458,55),"Үзіліске оралу",4)){Audio.MenuConfirm();PauseBack();}
                    else if(PauseRow(new Rect(553,510,458,55),"Басты мәзірге шығу",5))LeaveHouse();
                    break;
            }
        }finally{GUI.matrix=previous;}
    }
    void DrawPauseJournal(){
        Ink(new Rect(552,165,460,55),"Есіктің бес ысырмасы",35,parchment,true);
        Ink(new Rect(553,224,457,71),GoalHint(),23,parchment);
        int bolts=0;foreach(bool done in Solved)if(done)bolts++;
        Ink(new Rect(553,307,450,26),$"Ашылды: {bolts} / 5    ·    Реті өз еркіңде",17,antique);
        for(int i=0;i<5;i++)Fill(new Rect(553+i*93,343,84,4),i<bolts?antique:divider);
        Ink(new Rect(553,373,456,35),"Қолдағы заттар",22,parchment,true);
        if(inventory.Count==0)Ink(new Rect(553,421,450,30),"Әзірге бос",17,antique);
        for(int i=0;i<inventory.Count;i++){
            var r=new Rect(553+(i%2)*231,422+(i/2)*49,222,43);
            if(PauseRow(r,(i+1)+"  "+inventory[i].title,4+i)){SelectSlot(i);Audio.MenuConfirm();}
            if(i==selected)Fill(new Rect(r.x+1,r.y+9,2,25),antique);
        }
    }
    void DrawPauseControls(){
        Ink(new Rect(552,158,456,61),"Басқару",38,parchment,true);
        for(int i=0;i<controlKeys.Length;i++){
            int y=228+i*48;
            Ink(new Rect(553,y,165,42),controlKeys[i],17,antique,false,true);
            Ink(new Rect(715,y,295,42),controlNames[i],17,parchment);
            Fill(new Rect(553,y+44,458,1),divider);
        }
        if(PauseRow(new Rect(553,545,458,46),"Артқа",4)){Audio.MenuConfirm();PauseBack();}
    }
    float PauseSlider(int id,int y,string name,float value,float min,float max,string display){
        var r=new Rect(553,y,458,51);
        if(pointerMoved&&r.Contains(Event.current.mousePosition))FocusPause(id);
        Ink(new Rect(553,y,300,28),name,17,pauseFocus==id?parchment:antique);
        Ink(new Rect(943,y,72,28),display,17,parchment);
        var e=Event.current;
        if(pauseFocus==id&&e.type==EventType.KeyDown&&(e.keyCode==KeyCode.LeftArrow||e.keyCode==KeyCode.RightArrow)){
            value=Mathf.Clamp(value+(e.keyCode==KeyCode.LeftArrow?-1:1)*(max-min)/20,min,max);e.Use();
        }
        return GUI.HorizontalSlider(new Rect(553,y+32,458,17),value,min,max);
    }
    void DrawPauseOptions(){
        Ink(new Rect(552,153,460,61),"Баптаулар",38,parchment,true);
        Volume=PauseSlider(4,219,"Дыбыс деңгейі",Volume,0,1,Mathf.RoundToInt(Volume*100)+"%");
        Sensitivity=PauseSlider(5,274,"Тінтуір сезімталдығы",Sensitivity,.5f,4,Sensitivity.ToString("0.0"));
        Brightness=PauseSlider(6,329,"Жарықтық",Brightness,.6f,2,Mathf.RoundToInt(Brightness*100)+"%");
        if(PauseRow(new Rect(553,391,458,39),Scare?"Кенет шошыту · қосулы":"Кенет шошыту · өшірулі",7))Scare=!Scare;
        if(PauseRow(new Rect(553,433,458,39),economy?"Графика · үнемді":"Графика · қалыпты",8))SetGraphics(!economy);
        if(PauseRow(new Rect(553,475,458,39),"Python-ды қосу",9))python.Start();
        Ink(new Rect(554,514,455,23),python.Status,14,antique);
        if(PauseRow(new Rect(553,548,458,45),"Сақтау",10)){SaveOptions();Audio.MenuConfirm();PauseBack();}
    }
}
}
