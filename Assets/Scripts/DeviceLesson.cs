using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tishe {
// The notebook runs small worked examples separately from the live puzzle.
internal sealed class DeviceLesson {
    static readonly string[] Names={"Электр қалқаны","Механикалық құлып","Күзетшінің автоматы","Желдеткіш","Радиоқабылдағыш"};
    static readonly string[] Ideas={"Жол іздеу","Шектеулерді қанағаттандыру (CSP)","Minimax","Байес әдісі","Жасырын Марков моделі (HMM)"};
    static readonly string[] Inputs={"Сақтандырғыш","Тісті дөңгелек","Жетон","Ауа датчигі","Радио"};
    static readonly string[] Middle={"Тұтас сым жолы","Төрт таңбалы код","Жеңіс не тең ойын","Қауіпсіз ауа арнасы","Үш дыбыстың реті"};
    static readonly string[] End={"Реледе ток бар","Тұтқа бұрылады","Рұқсат берілді","Желдеткіш қосылды","Дайындық белгісі"};
    static readonly string[] Action={
        "Сақтандырғышты орнына қойдың. Әдісті таңда. Жол үзілсе, сымды жөндеп қайта тексер. Жол табылғанда «Релеге ток беру» батырмасын бас.",
        "Тісті дөңгелек тұтқаны дискілерге жалғады. Төрт шартты оқы. Кодты өзің енгіз немесе іздеуді іске қос. Содан кейін тұтқаны бұр.",
        "Жетонмен ойынды бастадың. Сен — X, автомат — O. Қарсыластың қатарын бөгеп, өз қатарыңды жина. Тең түссең де рұқсатты ала аласың.",
        "Датчикті орнаттың. C арнасынан белгі бар-жоғын тексер, қауіпті есепте. Үш мәнді салыстыр да, пайызы ең аз арнаны аш.",
        "Радиоға кемінде үш дыбысты ретімен енгіз. «Бақылауды қосу» батырмасын бас. Қызыл белгі күзетшінің соңғы дыбыс кезіндегі орнын көрсетеді. Пайыз оның жақын болу ықтималдығын білдіреді."
    };
    static readonly string[] FirstMove={
        "Алдымен сызбадағы + белгісін тап. Релеге дейін қай сымдар жалғасып тұр?",
        "Алдымен құлыптағы төрт шартты оқы. Соңғы цифр жұп болуы керек.",
        "Бос ұяшықты басып, X қой. Автомат O қояды. Үш белгіні бір қатарға жина.",
        "«Қауіпті есептеу» батырмасын бас. Ең кіші пайыз қай арнада?",
        "Пультте үш дыбысты ретімен таңда. Әр дыбыстан кейін «жақын» ықтималдығы қалай өзгереді?"
    };
    static readonly string[] Questions={
        "Қысқа жолдағы сым қатты қызған. Қай әдістер жалпы құны ең аз жолды табады?",
        "Цифрлары әртүрлі, қосындысы 18. Бірінші цифры соңғысынан үлкен, соңғысы жұп. Қай код жарайды?",
        "Сенің екі X белгің қатар тұр. Автоматтың бірден жеңетін жүрісі жоқ. Ол қай ұяшықты таңдауы керек?",
        "Газ қаупі: A — 5%, B — 18%, C — 77%. Қай арнаны ашасың?",
        "Сол үш дыбыстың ретін өзгерттің. Неге радиодағы пайыз өзгеруі мүмкін?"
    };
    static readonly string[,] Answers={{"BFS пен DFS","UCS пен A*"},{"8352","8334"},{"Кез келген бұрышты","Екі X жанындағы бос ұяшықты"},{"C арнасын","A арнасын"},{"Соңғы дыбыс алдыңғы бағамен бірге есептеледі","Модель тек дыбыс санын санайды"}};
    static readonly int[] Correct={1,0,1,1,0};
    static readonly string[] Reasons={
        "UCS пен A* қызған сымға кіру құнын да санайды. BFS қадамды азайтады, DFS бір тармақты терең тексереді.",
        "8 + 3 + 5 + 2 = 18. Цифрлар әртүрлі, 8 > 2, соңғы 2 жұп. 8334 кодында 3 қайталанады.",
        "Егер автомат жолыңды жаппаса, келесі жүрісте үшінші X қойып жеңесің. Minimax осы жауапты алдын ала ескереді.",
        "Пайыз сол арнада газ болуының бағасын көрсетеді. Ең кіші мән A арнасында.",
        "Әр жаңа дыбыс өткен қадамдағы бағадан басталады. Сондықтан соңынан естілген аяқ дыбысы мен соңынан орнаған тыныштық бөлек нәтиже береді."
    };
    static readonly string[] Stages={"Мақсат","Мысал","Тексеру"};
    static readonly string[] SoundNames={"Тыныштық","Сықыр","Аяқ дыбысы"};
    static readonly int[,] ExampleGrid={{0,2,0,0},{0,1,1,0},{0,0,0,0}};
    static readonly int[] Board={1,1,0,0,2,0,0,0,0};
    static readonly Color Paper=new Color32(237,233,220,255),Card=new Color32(249,247,238,255),
        Ink=new Color32(41,52,48,255),Muted=new Color32(78,93,84,255),Line=new Color32(189,194,178,255),
        Base=new Color32(222,226,212,255),Green=new Color32(44,85,71,255),Rust=new Color32(146,71,42,255),
        Seen=new Color32(187,205,187,255);
    readonly List<int> visited=new List<int>();
    readonly bool[] read=new bool[5];
    AiAlgorithms.SearchResult search;
    CircuitMethod method;
    int task,answer,ply,reply,nodes,cuts;
    bool alternate;
    internal int Page{get;private set;}
    internal int Step{get;private set;}
    internal bool Visible{get;private set;}
    internal int StepCount=>task==0?visited.Count:task==1?5:task==2?3:4;
    internal bool AnswerIsCorrect=>answer==Correct[task];

    internal void Enter(int topic,CircuitMethod selected,bool force=false){
        task=topic;Visible=force||!read[topic];Page=0;Step=0;answer=-1;alternate=false;ply=-1;
        if(topic==0)SelectMethod(selected);
        if(topic==2)reply=AiAlgorithms.Guardian(Board,out nodes,out cuts);
    }
    internal void SelectMethod(CircuitMethod value){method=value;Step=0;search=AiAlgorithms.Circuit(ExampleGrid,value,visited);}
    internal void Answer(int value){answer=value;}
    internal void Close(){Visible=false;}
    internal void Finish(){read[task]=true;Close();}
    internal void Forward(){
        if(Page==0)Page=1;
        else if(Page==1 && Step+1<StepCount)Step++;
        else if(Page==1)Page=2;
        else if(answer>=0)Finish();
    }
    internal void Back(){
        if(Page==1 && Step>0)Step--;
        else if(Page>0)Page--;
    }
    static void Block(float x,float y,float w,float h,Color color){
        var old=GUI.color;GUI.color=color;GUI.DrawTexture(new Rect(x,y,w,h),Texture2D.whiteTexture);GUI.color=old;
    }
    GUIStyle text,title,small,cell,buttonText,eyebrow;
    void Styles(GUIStyle body){
        if(text!=null)return;
        text=new GUIStyle(body){fontSize=23,alignment=TextAnchor.MiddleLeft,padding=new RectOffset()};
        title=new GUIStyle(text){fontSize=34,fontStyle=FontStyle.Bold};
        small=new GUIStyle(text){fontSize=19};
        cell=new GUIStyle(text){alignment=TextAnchor.MiddleCenter,fontSize=23,fontStyle=FontStyle.Bold};
        buttonText=new GUIStyle(cell){fontSize=21,fontStyle=FontStyle.Normal,padding=new RectOffset(9,9,0,0)};
        eyebrow=new GUIStyle(small){fontSize=17,fontStyle=FontStyle.Bold};
        text.normal.textColor=Ink;title.normal.textColor=Ink;small.normal.textColor=Muted;
    }
    void Label(Rect r,string label,GUIStyle style,Color color){
        var previous=style.normal.textColor;
        style.normal.textColor=color;
        GUI.Label(r,label,style);
        style.normal.textColor=previous;
    }
    void L(float x,float y,float w,float h,string label,GUIStyle style){GUI.Label(new Rect(x,y,w,h),label,style);}
    bool Control(Rect r,string label,Color fill,Color ink,bool outline=false){
        bool hover=r.Contains(Event.current.mousePosition);
        Color face=hover?Color.Lerp(fill,ink,.10f):fill;
        if(outline){
            Block(r.x,r.y,r.width,r.height,hover?Green:Line);
            Block(r.x+2,r.y+2,r.width-4,r.height-4,face);
        }else Block(r.x,r.y,r.width,r.height,face);
        Label(r,label,buttonText,ink);
        return GUI.Button(r,GUIContent.none,GUIStyle.none);
    }
    bool B(float x,float y,float w,float h,string label,bool active=false){
        return Control(new Rect(x,y,w,h),label,active?Green:Paper,active?Color.white:Ink,!active);
    }
    void Tile(float x,float y,float w,float h,string label,Color c){
        Block(x,y,w,h,c);
        Label(new Rect(x,y,w,h),label,cell,c==Rust||c==Green?Color.white:Ink);
    }
    void Keyboard(){
        var e=Event.current;
        if(e.type!=EventType.KeyDown)return;
        switch(e.keyCode){
            case KeyCode.Escape: Finish();break;
            case KeyCode.LeftArrow: Back();break;
            case KeyCode.Space: case KeyCode.Return: case KeyCode.KeypadEnter: case KeyCode.RightArrow: Forward();break;
            case KeyCode.Tab:
                if(Page!=1 || task==1)return;
                if(task==0)SelectMethod((CircuitMethod)(((int)method+(e.shift?3:1))%4));
                else alternate=!alternate;
                break;
            case KeyCode.Alpha1: case KeyCode.Keypad1: if(Page!=2)return;Answer(0);break;
            case KeyCode.Alpha2: case KeyCode.Keypad2: if(Page!=2)return;Answer(1);break;
            default:return;
        }
        e.Use();
    }
    internal void Draw(GUIStyle body,bool report){
        Styles(body);
        Keyboard();
        if(!Visible)return;
        Block(140,51,1088,696,new Color(0,0,0,.35f));
        Block(132,43,1092,694,Paper);
        Block(132,43,9,694,Green);
        Label(new Rect(178,64,585,26),$"ҚҰРЫЛҒЫ НҰСҚАУЛЫҒЫ     {task+1} / 5",eyebrow,Muted);
        L(177,95,780,55,Names[task],title);
        string leave=report?"Нәтижеге оралу":"Құрылғыға өту";
        if((Page!=2 || answer<0) && Control(new Rect(935,74,245,46),leave+"   Esc",Paper,Ink,true)){Finish();return;}

        for(int i=0;i<Stages.Length;i++){
            float x=178+334*i;
            var tab=new Rect(x,161,334,49);
            bool active=Page==i;
            if(tab.Contains(Event.current.mousePosition))Block(x,161,334,48,Base);
            Label(tab,(i+1)+"   "+Stages[i],buttonText,active?Green:Muted);
            Block(x,209,334,active?4:1,active?Green:Line);
            if(GUI.Button(tab,GUIContent.none,GUIStyle.none))Page=i;
        }

        Block(164,231,1030,365,Card);
        if(Page==0)DrawGoal();
        else if(Page==2)DrawQuiz();
        else{
            switch(task){
                case 0: DrawSearch();break;
                case 1: DrawLock();break;
                case 2: DrawBoard();break;
                case 3: DrawBayes();break;
                case 4: DrawRadio();break;
            }
        }
        DrawFooter(leave);
    }
    void DrawGoal(){
        Label(new Rect(188,248,956,29),"ҚАЛАЙ ЖҰМЫС ІСТЕЙДІ",eyebrow,Muted);
        L(187,285,962,48,Ideas[task],title);
        L(188,343,956,66,EscapeLessons.Concepts[task],text);
        Tile(188,425,267,53,Inputs[task],Base);
        Label(new Rect(456,425,47,53),"→",cell,Muted);
        Tile(504,425,330,53,Middle[task],Base);
        Label(new Rect(835,425,47,53),"→",cell,Muted);
        Tile(883,425,285,53,End[task],Green);
        Block(188,506,4,69,Rust);
        Label(new Rect(208,496,940,30),"ЕСІК ҚАЛАЙ АШЫЛАДЫ?",eyebrow,Rust);
        L(208,530,945,55,EscapeLessons.Payoffs[task],text);
        L(188,595,970,59,FirstMove[task],small);
    }
    void DrawQuiz(){
        Label(new Rect(188,244,970,27),"ЖАУАПТЫ ТАҢДА",eyebrow,Muted);
        L(188,276,973,82,Questions[task],text);
        for(int i=0;i<2;i++){
            bool selected=answer==i;
            var fill=selected?(AnswerIsCorrect?Green:Rust):Paper;
            if(Control(new Rect(188+i*496,367,480,69),(i+1)+"   "+Answers[task,i],fill,selected?Color.white:Ink,!selected))Answer(i);
        }
        if(answer<0)return;
        var accent=AnswerIsCorrect?Green:Rust;
        Block(188,460,4,111,accent);
        Label(new Rect(209,454,940,32),AnswerIsCorrect?"Дұрыс жауап":"Басқа жауапты байқап көр",eyebrow,accent);
        L(209,489,938,89,Reasons[task],text);
    }
    void DrawFooter(string leave){
        if(Page==1){
            Label(new Rect(179,601,180,41),$"Қадам {Step+1} / {StepCount}",eyebrow,Muted);
            float width=510f/StepCount;
            for(int i=0;i<StepCount;i++){
                float x=365+i*width;
                var hit=new Rect(x,601,width,40);
                Block(x,620,width-3,hit.Contains(Event.current.mousePosition)?9:5,i<=Step?Green:Line);
                if(GUI.Button(hit,GUIContent.none,GUIStyle.none))Step=i;
            }
        }else if(Page==2 && answer>=0)L(179,601,984,59,Action[task],small);
        if(Page>0 && Control(new Rect(178,665,170,50),"←  Артқа",Paper,Ink,true))Back();
        if(Page==0)L(179,665,665,50,"Enter — мысалды бастау",small);
        if(Page==1)L(379,665,480,50,task==1?"←  →   қадам ауыстыру":"←  →   қадам     ·     Tab   нұсқаны ауыстыру",small);
        if(Page==2 && answer<0)L(379,665,665,50,"1 немесе 2 — жауапты таңдау",small);
        if(Page==2 && answer<0)return;
        string next=Page==0?"Мысалды бастау":Page==2?leave:Step+1<StepCount?"Келесі қадам":"Өзіңді тексер";
        if(Control(new Rect(866,661,314,56),next+"   →",Green,Color.white))Forward();
    }
    void DrawSearch(){
        for(int i=0;i<4;i++)if(B(184+i*125,248,117,41,EscapeLessons.Names[i],(int)method==i) && (int)method!=i)SelectMethod((CircuitMethod)i);
        int current=visited[Step];bool done=Step==StepCount-1;
        for(int i=0;i<ExampleGrid.Length;i++){
            int value=ExampleGrid[i/4,i%4];int order=visited.IndexOf(i);
            string s=i==0?"Ток":i==11?"Реле":value==1?"×":value==2?"4":"1";
            Tile(190+i%4*83,322+i/4*71,75,60,s,i==current?Rust:done&&search.path.Contains(i)?Green:
                 order>=0&&order<Step?Seen:Base);
        }
        L(574,319,574,100,EscapeLessons.SearchTips[(int)method],text);
        L(574,424,571,122,done?$"Реле табылды. Жасыл жол: {search.path.Count-1} қадам, құны {search.cost}. Тексерілген ұяшық: {visited.Count}. Басқа әдісті таңдап салыстыр.":
            $"Қызғылт ұяшық қазір тексерілуде: {current/4+1}-қатар, {current%4+1}-баған. Тексерілгені: {Step+1}. Жасылдау ұяшықтар бұрын қаралды.",text);
        L(188,545,960,40,"×  үзілген сым        1  қалыпты сым        4  қызған сым",small);
    }
    static readonly string[] LockDigits={"8···","83··","833·","835·","8352"};
    static readonly string[] LockText={
        "Әр диск — 0 мен 9 арасындағы бір цифр. Алғашқы дискке 8 қойып көрейік.",
        "Келесі дискіге 3 қойдық. 8 бен 3 әртүрлі, әзірге қайшылық жоқ.",
        "Үшінші дискіге тағы 3 қойсақ, екі цифр қайталанады. Соңғы дискті тексермей-ақ осы тармақтан қайтамыз.",
        "Қайталанған 3-ті алып, орнына 5 қойдық. Қосынды қазір 16. 18-ге жету үшін 2 керек.",
        "8352 жарайды: қосынды 18, цифрлар әртүрлі, 8 > 2, соңғы 2 жұп. Енді тұтқаны бұруға болады."
    };
    void DrawLock(){
        L(186,258,970,66,"Мысал: қосынды 18. Цифрлар әртүрлі, бірінші цифр соңғысынан үлкен, соңғысы жұп.",text);
        for(int i=0;i<4;i++)Tile(188+i*131,346,116,78,LockDigits[Step][i].ToString(),Step==2&&i==2?Rust:LockDigits[Step][i]=='·'?Base:Green);
        L(186,464,958,102,LockText[Step],text);
    }
    void DrawBoard(){
        string turn=Step==0?"Үстіңгі қатарда екі X тұр. O қай ұяшықты таңдауы керек?":
            Step==1?"O өз жүрісін жасады. Енді X қалай жауап бере алады?":"Екі нұсқаны салыстыр. Қай жүріс X-тің бірден жеңуіне жол берді?";
        L(186,250,967,56,turn,text);
        var b=(int[])Board.Clone();
        if(Step>0){ply=alternate?6:reply;b[ply]=2;}
        if(Step==2)b[alternate?2:6]=1;
        for(int i=0;i<9;i++)Tile(190+i%3*66,320+i/3*66,60,60,b[i]==0?"·":b[i]==1?"X":"O",Step>0&&i==ply?Green:Base);
        if(B(441,318,290,45,"Minimax таңдауы",!alternate)){alternate=false;}
        if(B(742,318,375,45,"Басқа жүрісті сынау",alternate)){alternate=true;}
        string detail=Step==0?"Автомат әр O жүрісіне X қалай жауап бере алатынын қарайды. Екі нұсқаны кезекпен тексер.":
            Step==1?(alternate?"O төменгі сол жаққа жүрді. Үстіңгі қатар әлі ашық.":"O үстіңгі оң жаққа жүріп, екі X-тің жолын жапты."):
            alternate?"X үстіңгі қатарды аяқтады. Осы жауаптың өзі O жүрісінің қауіпті екенін көрсетеді.":
            "Мысалы, X төменгі сол жаққа жүрді. Үстіңгі қатар жабық, ойын жалғасады.";
        L(441,382,710,100,detail,text);
        L(441,490,699,94,$"O өзіне ең жақсы, X өзіне ең жақсы нәтижені іздейді. Альфа-бета таңдауды өзгерте алмайтын тармақтарды өткізіп жібереді. Қаралған түйін: {nodes}, қиылған тармақ: {cuts}.",small);
    }
    static readonly double[] Prior={1d/3,1d/3,1d/3}, Yes={.05,.20,.85}, No={.95,.80,.15};
    void DrawBayes(){
        if(B(186,248,216,45,"C: белгі бар",!alternate))alternate=false;
        if(B(410,248,222,45,"C: белгі жоқ",alternate))alternate=true;
        var likelihood=alternate?No:Yes;
        var weights=Prior.Zip(likelihood,(p,l)=>p*l).ToArray();
        var posterior=AiAlgorithms.Bayes(Prior,likelihood);
        double[] values=Step==0?Prior:Step==1?likelihood:Step==2?weights:posterior;
        string[] labels={"Бастапқы қауіп","Осы белгіні көру ықтималдығы","Бастапқы қауіп × белгі ықтималдығы","Жаңартылған қауіп"};
        L(187,310,523,63,labels[Step],text);
        for(int i=0;i<3;i++){
            L(187,377+i*54,125,43,$"{"ABC"[i]}   {values[i]:P1}",text);
            Block(316,387+i*54,315,12,Base);Block(316,387+i*54,315*(float)values[i],12,Green);
        }
        string detail=Step==0?"Датчикке қарамай тұрып, үш арнаны бірдей ықтимал деп аламыз. Газ бір арнадан шығады.":
            Step==1?"Газ әр арнада болса, датчик осындай белгі беруі қаншалықты мүмкін? Бұл үш сан датчиктің жұмысын сипаттайды.":
            Step==2?$"Әр арнаға салмақ береміз. Мысалы, C арнасы: 1/3 × {likelihood[2]:0.00} = {weights[2]:0.000}.":
            $"Әр салмақты үшеуінің қосындысына бөлеміз. C: {weights[2]:0.000} / {weights.Sum():0.000} ≈ {posterior[2]:P1}. Ең аз қауіп — {"ABC"[Array.IndexOf(posterior,posterior.Min())]} арнасында.";
        L(681,334,475,192,detail,text);
    }
    void DrawRadio(){
        if(B(186,247,434,47,"Тыныштық → сықыр → аяқ дыбысы",!alternate))alternate=false;
        if(B(630,247,450,47,"Аяқ дыбысы → сықыр → тыныштық",alternate))alternate=true;
        int[] order=alternate?new[]{2,1,0}:new[]{0,1,2};
        var values=new double[4];
        for(int i=0;i<4;i++)values[i]=AiAlgorithms.Hmm(order.Take(i))[1];
        for(int i=0;i<=Step;i++){
            float x=192+i*114,height=(float)values[i]*136;
            Block(x,494-height,73,height,i==Step?Rust:Green);
            L(x,499,109,31,values[i].ToString("P1"),small);
            L(x,530,115,50,i==0?"Бастапқы":SoundNames[order[i-1]],small);
        }
        string message=Step==0?"Радио әлі ештеңе естімеді. Бастапқы болжам: күзетші жақын болуының ықтималдығы 20%.":
            $"Жаңа дыбыс: {SoundNames[order[Step-1]]}. Бұрынғы баға {values[Step-1]:P1}, жаңа баға {values[Step]:P1}.";
        L(675,326,477,106,message,text);
        string update="Модель екі күйді ажыратады: алыс және жақын. Келесі қадамда күй өзгеруі мүмкін. Дыбыстарды кері ретпен енгізіп, соңғы бағанды салыстыр.";
        if(Step>0){
            double oldNear=values[Step-1],predicted=AiAlgorithms.PredictNear(1-oldNear,oldNear);
            update=$"Алдымен қозғалысты болжаймыз: алыс күйден жақындау мүмкіндігі 18%, жақын күйде қалу 75%. Келесі дыбысқа дейінгі баға: {predicted:P1}. Сосын жаңа дыбысты Байес әдісімен ескереміз.";
        }
        L(675,433,480,148,update,small);
    }
}
}
