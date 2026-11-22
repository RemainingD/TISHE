using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tishe {
// The archive owns its examples. It never writes to inventory, bolts or campaign tasks.
internal sealed class HouseArchive {
    static readonly string[] Rooms={"Электр қалқаны","Төрт дискілі құлып","Ойын автоматы","Желдеткіш","Радиоқабылдағыш"};
    static readonly string[] Topics={"Сымдардағы жолды іздеу","CSP  /  Шектеулер","Minimax  /  Альфа-бета","Байес  /  Ықтималдық","HMM  /  Жасырын күй"};
    static readonly string[] Sounds={"Тыныштық","Сықыр","Аяқ дыбысы"};
    static readonly Color Ivory=new Color32(238,225,204,255),Gold=new Color32(210,172,110,255),Muted=new Color32(185,188,184,255);
    internal int Page;
    int[,] grid=EscapeLessons.NewCircuit();
    CircuitMethod method=CircuitMethod.Astar;
    readonly int[] board=new int[9];
    readonly List<int> sounds=new List<int>();
    readonly List<double> history=new List<double>();
    AiAlgorithms.SearchResult path,previous;
    int sum=18,checks,nodes,cuts;string code;
    bool signal=true;
    GUIStyle main,caption,text,control;
    internal HouseArchive(){
        path=AiAlgorithms.Circuit(grid,method);Lock(0);
    }
    internal void Tile(int n){
        if(n==0||n==35)return;
        grid[n/6,n%6]=(grid[n/6,n%6]+1)%3;
        previous=path;path=AiAlgorithms.Circuit(grid,method);
    }
    internal void SelectMethod(CircuitMethod value){
        previous=path;method=value;path=AiAlgorithms.Circuit(grid,method);
    }
    internal void Lock(int delta){
        sum=Mathf.Clamp(sum+delta,6,30);
        var digits=AiAlgorithms.Csp(sum,out checks);code=digits==null?"—":string.Join("   ",digits);
    }
    internal void Move(int i){
        if(board[i]!=0||AiAlgorithms.Outcome(board)!=0)return;
        board[i]=1;
        int reply=AiAlgorithms.Guardian(board,out nodes,out cuts);if(reply>=0)board[reply]=2;
    }
    internal void Listen(int i){
        if(sounds.Count>=8)return;
        sounds.Add(i);history.Add(AiAlgorithms.Hmm(sounds)[1]);
    }
    internal double LastBelief=>history.Count==0?.2:history[history.Count-1];
    void Reset(){
        switch(Page){
            case 0:grid=EscapeLessons.NewCircuit();method=CircuitMethod.Astar;path=AiAlgorithms.Circuit(grid,method);previous=null;break;
            case 1:sum=18;Lock(0);break;
            case 2:System.Array.Clear(board,0,9);nodes=cuts=0;break;
            case 3:signal=true;break;
            case 4:sounds.Clear();history.Clear();break;
        }
    }
    static void RectFill(float x,float y,float w,float h,Color c){
        var old=GUI.color;GUI.color=c;GUI.DrawTexture(new Rect(x,y,w,h),Texture2D.whiteTexture);GUI.color=old;
    }
    bool Btn(float x,float y,float w,float h,string label,bool selected=false){
        var rect=new Rect(x,y,w,h);
        bool over=rect.Contains(Event.current.mousePosition);
        RectFill(x,y,w,h,selected?new Color32(104,58,40,255):over?new Color32(68,68,61,255):new Color32(47,54,53,255));
        if(selected)RectFill(x,y,3,h,Gold);
        return GUI.Button(rect,label,control);
    }
    void Label(float x,float y,float w,float h,string value,GUIStyle style){GUI.Label(new Rect(x,y,w,h),value,style);}
    internal bool Draw(GUIStyle body){
        if(main==null){
            main=new GUIStyle(body){fontSize=36,fontStyle=FontStyle.Bold,normal={textColor=Ivory}};
            caption=new GUIStyle(body){fontSize=17,normal={textColor=Gold}};
            text=new GUIStyle(body){fontSize=22,normal={textColor=Muted}};
            control=new GUIStyle(body){fontSize=20,alignment=TextAnchor.MiddleCenter,normal={textColor=Ivory}};
        }
        RectFill(70,48,1220,700,new Color32(21,28,29,250));
        Label(110,78,620,62,"Үй мұрағаты",main);
        bool back=Btn(1090,87,150,46,"Артқа  ·  Esc");
        RectFill(110,160,1130,1,new Color32(133,115,84,255));
        for(int i=0;i<5;i++)if(Btn(110,193+i*76,260,59,Rooms[i],Page==i))Page=i;
        Label(417,193,780,55,Topics[Page],main);
        Label(110,585,264,138,EscapeLessons.Payoffs[Page],caption);
        switch(Page){
            case 0:
                Label(417,257,769,65,"Әдісті таңда немесе сымды өзгерт: қалыпты (1), үзік (×), қызған (4).",text);
                for(int i=0;i<36;i++){
                    int cell=grid[i/6,i%6];bool on=path.path.Contains(i),old=previous!=null&&previous.path.Contains(i);
                    if(Btn(420+i%6*48,335+i/6*47,44,42,i==0?"A":i==35?"B":cell==1?"×":cell==2?"4":on?"●":old?"○":"·",on))Tile(i);
                }
                for(int i=0;i<4;i++)if(Btn(731+i*119,337,113,46,EscapeLessons.Names[i],i==(int)method))SelectMethod((CircuitMethod)i);
                Label(731,398,478,112,EscapeLessons.SearchTips[(int)method],text);
                Label(731,516,486,124,$"Қадам: {(path.Found?(path.path.Count-1).ToString():"—")}     Құны: {(path.Found?path.cost.ToString():"—")}\nҚаралған ұяшық: {path.expanded}     Бұрынғы құны: {(previous!=null&&previous.Found?previous.cost.ToString():"—")}\n● Қазіргі жол     ○ Бұрынғы жол",caption);
                break;
            case 1:
                Label(417,266,780,103,"Төрт цифр әртүрлі. Біріншісі соңғысынан үлкен, соңғысы жұп. Қосындыны өзгерт те, құлып кодын салыстыр.",text);
                if(Btn(420,388,70,52,"−"))Lock(-1);if(Btn(650,388,70,52,"+"))Lock(1);
                Label(519,391,130,55,"Σ  "+sum,main);
                Label(420,480,740,140,$"Код    {code}\nТексерілген нұсқалар: {checks}",text);break;
            case 2:
                Label(417,264,780,66,"Бос торға X қой. Автомат ықтимал жауаптарды салыстырып, O қояды.",text);
                for(int i=0;i<9;i++)if(Btn(420+i%3*76,352+i/3*74,69,66,board[i]==1?"X":board[i]==2?"O":"·",board[i]==2))Move(i);
                int end=AiAlgorithms.Outcome(board);
                Label(704,360,480,180,$"{(end==3?"Тең ойын":end==2?"Автомат жеңді":end==1?"Сен жеңдің":"Сенің жүрісің")}\n\nҚаралған түйіндер: {nodes}\nҚиылған тармақтар: {cuts}",text);break;
            case 3:
                Label(417,265,775,78,"Үш арнаның бірінен газ шығады. C арнасындағы датчиктің көрсеткішін өзгертіп көр.",text);
                if(Btn(420,360,290,54,"Белгі бар",signal))signal=true;
                if(Btn(730,360,290,54,"Белгі жоқ",!signal))signal=false;
                var risk=AiAlgorithms.Bayes(new[]{1d,1d,1d},signal?new[]{.05,.2,.85}:new[]{.95,.8,.15});
                for(int i=0;i<3;i++){
                    Label(420,458+49*i,120,42,"ABC"[i]+"    "+risk[i].ToString("P1"),text);
                    RectFill(576,469+49*i,480,9,new Color32(51,56,52,255));
                    RectFill(576,469+49*i,480*(float)risk[i],9,Gold);
                }break;
            case 4:
                Label(417,261,780,68,"Дыбыстарды ретімен қос. Әр қадамнан кейін күзетшінің жақын болу ықтималдығы өзгереді.",text);
                for(int i=0;i<3;i++)if(Btn(420+i*245,352,230,52,Sounds[i]))Listen(i);
                Label(420,423,777,62,sounds.Count==0?"Дыбыс күтілуде…":string.Join(" → ",sounds.Select(i=>Sounds[i])),text);
                for(int i=0;i<history.Count;i++){
                    float height=(float)history[i]*86;RectFill(423+91*i,586-height,53,height,Gold);
                    Label(420+91*i,592,86,39,history[i].ToString("P0"),caption);
                }
                Label(832,638,363,48,"Жақын болуы: "+LastBelief.ToString("P1"),text);break;
        }
        if(Btn(420,653,292,48,"Тәжірибені қайта бастау"))Reset();
        return back;
    }
}
}
