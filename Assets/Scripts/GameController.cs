using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Tishe {
public partial class GameController : MonoBehaviour {
    public static GameController Instance {get;private set;}
    public HorrorAudio Audio;public StoryDirector Story;public FirstPerson Player;public Keeper Enemy;public List<HouseDoor> Doors=new List<HouseDoor>();
    public string State="menu",Panel="";public float Sensitivity=2,Volume=.45f,Brightness=1,Grace=10;
    public bool Scare=true;public bool Paused=>State!="playing"||Panel!="";
    public FrontEntrance Entrance;
    public Transform[] ExitBolts=new Transform[5];
    public bool[] Solved=new bool[5];public int Lives=3;
    readonly List<GameItem> inventory=new List<GameItem>();readonly PythonBridge python=new PythonBridge();
    int selected,taskIndex;float timer,noise,captionTime;string caption="",status="";
    readonly string[] titles={"Үйге қайта жарық бер","Төрт диск, бір құлып","Күзетшінің ойыны","Есіктің ар жағындағы ауа","Көрінбейді, бірақ естіледі"};
    readonly string[] topics={"02 / ЖОЛ ІЗДЕУ","03 / ШЕКТЕУЛЕР · CSP","04 / ҚАРСЫЛАСТЫҢ ЖҮРІСІ · MINIMAX","06 / ЫҚТИМАЛДЫҚ · БАЙЕС","07 / ЖАСЫРЫН КҮЙ · HMM"};
    readonly string[] needs={"fuse","gear","token","sensor","radio"};
    static readonly string[] deviceHints={
        "Сақтандырғышты асүйдегі электр қалқанына орнатуға болады.",
        "Тісті дөңгелекті механикалық құлыпқа орнатуға болады.",
        "Жетонды ойын автоматына салуға болады.",
        "Датчикпен желдеткіштің ауа арналарын тексеруге болады.",
        "Радионы бірінші қабаттағы баспалдақ жанындағы пультте баптауға болады."
    };
    bool announceAfterReport;
    int AvailableTask(int slot){
        int task=Array.IndexOf(needs,inventory[slot].id);
        return task>=0 && !Solved[task] ? task : -1;
    }
    int SuggestedTask(){
        if(inventory.Count==0)return -1;
        int task=AvailableTask(selected);
        for(int i=inventory.Count-1;task<0 && i>=0;i--)task=AvailableTask(i);
        return task;
    }
    internal string GoalHint(){
        if(Solved.All(done=>done))return "Бес ысырма ашылды. Сыртқы есікке бар.";
        int task=SuggestedTask();
        if(task>=0)return deviceHints[task];
        return !Solved[0] ? "Кез келген құрылғыдан баста. Мысалы, қалқан асүйде." :
            "Қалған құрылғылардың кез келгеніне қажетті затты ізде.";
    }
    internal void AnnounceGoal(bool afterCurrent=false){
        goalTime=8;
        if(Solved.All(done=>done)){Audio.PlayerLine("exit_next",afterCurrent);return;}
        int task=SuggestedTask();
        if(task>=0)Audio.PlayerLine(needs[task]+"_next",afterCurrent);
        else if(!Solved.Any(done=>done))Audio.PlayerLine("fuse_next",afterCurrent);
    }
    readonly string[] missing={"Қалқандағы ұя бос. Сақтандырғыш керек.","Құлыптың тісті дөңгелегі жетіспейді.","Автоматты іске қосу үшін жетон керек.","Ауаны тексеру үшін ауа датчигі керек.","Алдымен радиоқабылдағышты тап."};
    readonly int[,] wires=EscapeLessons.NewCircuit();
    CircuitMethod circuitMethod=CircuitMethod.Astar,solvedCircuitMethod=CircuitMethod.Astar;
    string DoneVoice(int n)=>n==0 && solvedCircuitMethod!=CircuitMethod.Astar?"relay_done":needs[n]+"_done";
    List<int> wirePath=new List<int>(),records=new List<int>();int[] board=new int[9];int sum=18;string pin="";double[] risks;double threat=.2;bool signal=true;
    internal readonly RadioRadar Receiver=new RadioRadar();
    internal bool RadarActive=>Solved[4]&&!Paused&&Has("radio");
    internal float NoiseLevel=>noise;
    internal void ClearNoise(){noise=0;nextFloorCreak=0;}
    readonly string[] taskReports=new string[5];
    int lastNodes,lastPruned; double circuitCost; int circuitNodes;
    readonly string[] reasons={
        "Сыртқы есіктің релесіне ток келмейінше, бірінші ысырма ашылмайды.",
        "Есіктің механикалық ысырмасын төрт диск ұстап тұр. Тісті дөңгелек оларды тұтқаға жалғады.",
        "Автоматтан ұтылмасаң, ол сыртқа шығуға рұқсат береді. Ойынды бастау үшін жетон сал.",
        "Қауіпсіз ауа арнасын таңдамайынша, желдеткішке жалғанған ысырма ашылмайды.",
        "Есік радиодан дайындық белгісін күтеді. Радионы баптасаң, экранда осы қабаттың картасы бар радар пайда болады."
    };
    float appliedBrightness=-1;
    GUIStyle title,heading,body,small,button,box,input,subtitle;Texture2D shade;
    void Awake(){Instance=this;var args=Environment.GetCommandLineArgs();if(args.Contains("--smoke-test")||args.Contains("--frame-test"))gameObject.AddComponent<RuntimeProbe>();}
    void Start(){SetGraphics(PlayerPrefs.GetInt("EconomyGraphics",0)==1);Sensitivity=PlayerPrefs.GetFloat("Sensitivity",2);Volume=PlayerPrefs.GetFloat("Volume",.45f);Brightness=PlayerPrefs.GetFloat("Brightness",1);Scare=PlayerPrefs.GetInt("Scare",1)==1;if(Player==null||Enemy==null){HouseBuilder.Build(this);var decor=GameObject.Find("REFERENCE INTERIORS / rooms and exterior");if(decor!=null)BatchStationaryDecor(decor);}Player.view.gameObject.AddComponent<HouseLighting>();Player.view.renderingPath=RenderingPath.DeferredShading;ForestYard.SetUpDistanceDetail(Player.view);Story=gameObject.AddComponent<StoryDirector>();Story.Initialize(this);Audio=gameObject.AddComponent<HorrorAudio>();titleScreen.SelectionChanged+=Audio.MenuFocus;Player.Teleport(new Vector3(-19,6,35));Player.view.transform.LookAt(new Vector3(0,4,13));Enemy.transform.position=new Vector3(0,0,16.7f);Enemy.transform.rotation=Quaternion.identity;python.Start();}
    static void BatchStationaryDecor(GameObject root){
        var stationary=root.GetComponentsInChildren<MeshRenderer>().Where(r=>r.GetComponentInParent<NurseryMobile>()==null).Select(r=>r.gameObject).ToArray();
        StaticBatchingUtility.Combine(stationary,root);
    }
    bool economy;
    void SetGraphics(bool low){
        economy=low;
        QualitySettings.vSyncCount=0;
        Application.targetFrameRate=60;
        QualitySettings.pixelLightCount=low?2:4;
        QualitySettings.shadows=low?ShadowQuality.HardOnly:ShadowQuality.All;
        QualitySettings.shadowDistance=low?16:28;
        QualitySettings.shadowCascades=low?1:2;
        QualitySettings.shadowResolution=low?ShadowResolution.Low:ShadowResolution.Medium;
        QualitySettings.antiAliasing=0;
        QualitySettings.lodBias=low?.85f:1.25f;
    }
    void OnDestroy(){DisposePause();titleScreen.Dispose();prologue.Dispose();dayTitle.Dispose();Receiver.Dispose();python.Dispose();Time.timeScale=1;Cursor.lockState=CursorLockMode.None;}
    void OnApplicationFocus(bool focus){if(!focus&&State=="playing"&&Panel=="")OpenPause();}
    public void Say(string value){caption=value;captionTime=7;}
    float nextFloorCreak;
    public void Noise(Vector3 point,float radius,string kind){
        if(kind=="creak"){
            if(Time.time<nextFloorCreak)return;
            nextFloorCreak=Time.time+.48f;
        }
        noise=Mathf.Max(noise,Mathf.Clamp01(radius/40f));
        Enemy?.Hear(point,radius);
        if(Audio==null)return;
        switch(kind){
            case "step": Audio.Step(radius/15);break;
            case "creak": Audio.FloorCreak(point);break;
            case "doorOpen": Audio.Door(point,true);break;
            case "doorClose": Audio.Door(point,false);break;
            case "drop": Audio.Impact(point);break;
        }
    }
    public bool Has(string id){
        foreach(var item in inventory)if(item.id==id)return true;
        return false;
    }
    public void SelectSlot(int slot){if(slot>=0&&slot<inventory.Count){selected=slot;itemTime=3;goalTime=8;}}
    internal readonly HouseArchive Archive=new HouseArchive();
    readonly int[] tapeForTask={-1,-1,-1,-1,-1};
    GameItem radioReceiver;
    internal int LastCallLives {get;private set;}=-1;
    internal string ReceiverHint=>LastCallLives==Lives?"Осы түнгі шақыру қолданылды.":Has("radio")?
        "G · Қабылдағышты осында қалдыру":"G · Қалдырған қабылдағышты шақыру";
    internal void ReceiveSound(Vector3 source,int observation,float range){
        if(!RadarActive)return;
        var ear=Player.transform.position;
        if(RadioRadar.FloorAt(ear)!=RadioRadar.FloorAt(source))range*=.55f;
        if((source-ear).sqrMagnitude<=range*range)
            Receiver.Hear(observation,ear,Enemy.transform.position);
    }
    internal void ListenFor(float elapsed){
        if(RadarActive)Receiver.Tick(elapsed,Player.transform.position,Enemy.transform.position);
    }
    const string CalibrationHint="Қабылдағыш қолыңда. Бірінші қабаттағы баспалдақ жанынан қабырға пультін тап та, E бас.";
    internal void CheckReceiver(){
        if(Paused)return;
        if(!Has("radio")){Say("Алдымен радиоқабылдағышты қолыңа ал.");return;}
        if(!Solved[4]){Say(CalibrationHint);return;}
        OpenTask(4);
    }
    internal void UseReceiver(){
        if(Paused||!Solved[4]||radioReceiver==null)return;
        if(LastCallLives==Lives){Say(ReceiverHint);return;}
        if(Has("radio")){
            if(Player.hidden){Say("Қабылдағышты шкафтың сыртында қалдыр.");return;}
            Vector3 feet=Player.transform.position;
            if(!Physics.Raycast(feet+Vector3.up*.6f,Vector3.down,out var floor,2f,~0,QueryTriggerInteraction.Ignore)){
                Say("Қабылдағышты еденге қой.");return;
            }
            inventory.Remove(radioReceiver);Receiver.Reset();
            selected=Mathf.Clamp(selected,0,Mathf.Max(0,inventory.Count-1));
            radioReceiver.transform.position=floor.point+Vector3.up*.18f;
            radioReceiver.gameObject.SetActive(true);
            var rb=radioReceiver.GetComponent<Rigidbody>();if(rb!=null)rb.isKinematic=true;
            Say("Қабылдағышты қалдырдың. Радар сөнді. Алыстаған соң G бас.");return;
        }
        Vector3 source=radioReceiver.transform.position;
        if(Vector3.Distance(source,Player.transform.position)<3){
            Say("Алдымен қабылдағыштан кемінде үш метр алыста.");return;
        }
        LastCallLives=Lives;Audio.ReceiverCall(source);Noise(source,80,"radioCall");
        Say("Сигнал кетті. Дыбыс қабылдағыш тұрған жерден шығады.");
    }
    readonly HousePrologue prologue=new HousePrologue();
    readonly WakeDayTitle dayTitle=new WakeDayTitle();
    internal int IntroShot=>prologue.Index;
    internal readonly HouseTitleScreen titleScreen=new HouseTitleScreen();
    bool quitting;
    System.Collections.IEnumerator QuitAfterClick(float duration){
        if(duration>0)yield return new WaitForSecondsRealtime(duration);
        Application.Quit();
    }
    void LateUpdate(){if(Player!=null && Player.view!=null)titleScreen.SetBackdrop(Player.view,State=="menu");}
    internal void MenuChoice(HouseTitleScreen.Choice choice){
        if(State!="menu" || Panel!="" || quitting || choice==HouseTitleScreen.Choice.None)return;
        float clickDuration=Audio.MenuConfirm();
        switch(choice){
            case HouseTitleScreen.Choice.Start: Begin();break;
            case HouseTitleScreen.Choice.Settings: Panel="settings";break;
            case HouseTitleScreen.Choice.Help: Panel="help";break;
            case HouseTitleScreen.Choice.Archive: Panel="archive";break;
            case HouseTitleScreen.Choice.Quit: quitting=true;StartCoroutine(QuitAfterClick(clickDuration));break;
        }
    }
    internal void Begin(){
        if(State!="menu" || Panel!="")return;
        Receiver.Reset();Solved=new bool[5];Lives=3;LastCallLives=-1;System.Array.Fill(tapeForTask,-1);Audio.ResetTapes();State="intro";prologue.Start(Player,Audio);Audio.StartPrologueScore();
    }
    internal void FinishIntro(){
        if(State!="intro")return;
        Audio.StopPrologueLine();prologue.Finish(Player);Story.Wake();
    }

    void Update(){
        if(State=="playing" && Input.GetKeyDown(KeyCode.Escape)){
            if(Panel=="")OpenPause();
            else if(Panel=="pause")PauseBack();
        }
        bool paused=Paused;Time.timeScale=paused?0:1;Cursor.visible=paused;if(paused)Cursor.lockState=CursorLockMode.None;float dt=Time.unscaledDeltaTime;captionTime-=dt;
        if(State=="menu" && !quitting){
            if(Panel=="")MenuChoice(titleScreen.ReadInput());
            else if(Input.GetKeyDown(KeyCode.Escape))LeavePanel();
        }
        else if(State=="intro" && (Input.GetKeyDown(KeyCode.Escape)||prologue.Advance(Player,Audio,dt)))FinishIntro();
        if(appliedBrightness!=Brightness){RenderSettings.ambientLight=new Color(.104f,.119f,.152f)*Brightness;appliedBrightness=Brightness;}
        if(State=="playing"&&!paused){goalTime=Mathf.Max(0,goalTime-dt);itemTime=Mathf.Max(0,itemTime-dt);Grace=Mathf.Max(0,Grace-dt);noise=Mathf.MoveTowards(noise,0,dt*1.35f);ListenFor(dt);}
        if(State=="caught"){
            timer+=dt;
            if(timer>1.6f){
                if(Lives>0)Story.Wake();
                else State="lost";
            }
        }

    }
    public void Capture(){if(State!="playing"||Grace>0)return;Lives--;State="caught";Panel="";timer=0;Player.hidden=false;Story.CaptureShock();Player.body.enabled=false;if(Scare&&Audio!=null)Audio.Capture();}
    public void Interact(GameItem item){
        if(item.kind=="door"){item.GetComponent<HouseDoor>().Toggle();return;}
        if(item.kind=="pickup"){if(inventory.Contains(item))return;inventory.Add(item);SelectSlot(inventory.Count-1);item.gameObject.SetActive(false);Say(item.title+" алынды.");if(item.id=="radio"){radioReceiver=item;if(Solved[4])Say("Радар қосылды. "+ReceiverHint);}return;}
        if(item.kind=="hide"){var wardrobe=item.GetComponentInParent<Wardrobe>();if(wardrobe!=null){wardrobe.Enter(Player);Say("Тыныш. Саңылаудан қара. Шығу үшін E пернесін бас.");}return;}
        if(item.kind=="station"){OpenTask(item.taskIndex);return;}
        if(item.kind=="exit"){if(Solved.All(v=>v)){Story.Escape();}else {goalTime=8;Say("Есіктегі бес ысырманы кез келген ретпен аш.");}}
    }
    public void Drop(){if(inventory.Count==0)return;selected=Mathf.Clamp(selected,0,inventory.Count-1);var item=inventory[selected];inventory.RemoveAt(selected);if(item.id=="radio")Receiver.Reset();selected=Mathf.Clamp(selected,0,Mathf.Max(0,inventory.Count-1));itemTime=3;goalTime=8;item.transform.position=Player.transform.position+Vector3.up*1.25f+Player.transform.forward*.7f;item.gameObject.SetActive(true);var rb=item.GetComponent<Rigidbody>();if(rb==null)rb=item.gameObject.AddComponent<Rigidbody>();rb.isKinematic=false;rb.linearVelocity=Player.transform.forward*2;var impact=item.GetComponent<NoiseImpact>();if(impact==null)impact=item.gameObject.AddComponent<NoiseImpact>();impact.armed=true;Say("Затты қайта көтере аласың. Еденге түскен дыбыс күзетшінің назарын аударады.");}
    public void OpenTask(int n){if((n==4||!Solved[n])&&!Has(needs[n])){Say(reasons[n]+" "+missing[n]);Audio.PlayerLine(needs[n]+"_need");return;}taskIndex=n;
        if(Solved[n]){Lesson.Close();Panel="report";Audio.PlayerLine(DoneVoice(n));return;}
        if(n==4 && caption==CalibrationHint)captionTime=0;
        StartPuzzle(n);}
    internal readonly DeviceLesson Lesson=new DeviceLesson();
    void StartPuzzle(int n){Lesson.Enter(n,circuitMethod);announceAfterReport=false;Audio.PlayerLine(needs[n]+"_start");Panel="task";status="";if(n==0)wirePath.Clear();if(n==2)board=new int[9];if(n==3)risks=null;if(n==4){records.Clear();threat=.2;}}
    void Complete(string report){
        if(!Solved[taskIndex] && ExitBolts[taskIndex]!=null)
            ExitBolts[taskIndex].localPosition+=Vector3.left*FrontEntrance.BoltTravel;
        announceAfterReport=!Solved[taskIndex];
        if(taskIndex==0)solvedCircuitMethod=circuitMethod;
        Audio.PlayerLine(DoneVoice(taskIndex));
        if(announceAfterReport){
            int fragment=Solved.Count(done=>done);
            tapeForTask[taskIndex]=fragment;Audio.QueueTape(fragment);
        }
        Solved[taskIndex]=true;
        if(taskIndex==0)Player.view.GetComponent<HouseLighting>()?.SetKitchenPower(true);
        taskReports[taskIndex]=report;
        Panel="report";
    }
    void LeavePanel(){
        bool advance=Panel=="report"&&announceAfterReport;
        announceAfterReport=false;
        Panel="";
        if(advance)AnnounceGoal(true);
        if(State=="playing"){Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}
    }
    PythonBridge.Response Request(PythonBridge.Request request){if(!python.Ready)return null;try{return python.Ask(request);}catch(Exception e){python.Dispose();python.Status="C# • балама режим";Debug.LogWarning(e.Message);Say("Python байланысы үзілді. Есептеу C# арқылы жалғасады.");return null;}}
    double[] Hmm(List<int> observations){var r=Request(new PythonBridge.Request{op="hmm",observations=observations.ToArray()});return r?.probabilities??AiAlgorithms.Hmm(observations);}
    internal void SelectCircuit(CircuitMethod method){
        circuitMethod=method;ComputeCircuit();
    }
    internal void ComputeCircuit(){
        var flat=new int[wires.Length];for(int i=0;i<flat.Length;i++)flat[i]=wires[i/6,i%6];
        var reply=Request(new PythonBridge.Request{op="circuit",grid=flat,method=circuitMethod.ToString().ToLowerInvariant()});
        if(reply==null){
            var result=AiAlgorithms.Circuit(wires,circuitMethod);
            wirePath=result.path;circuitCost=result.cost;circuitNodes=result.expanded;
        }else{
            wirePath=(reply.path??Array.Empty<int>()).ToList();circuitCost=reply.cost;circuitNodes=reply.nodes;
        }
        status=wirePath.Count>0?$"Қадам: {wirePath.Count-1}     Құны: {circuitCost}     Қаралған ұяшық: {circuitNodes}":
            $"Жол жоқ. Үзілген сымды жөнде. Қаралған ұяшық: {circuitNodes}";
    }
    internal void ComputerTurn(int cell){if(board[cell]!=0||AiAlgorithms.Outcome(board)!=0)return;board[cell]=1;var r=Request(new PythonBridge.Request{op="guardian",board=board});int move,nodes,cuts;if(r!=null){move=r.move;nodes=r.nodes;cuts=r.pruned;}else move=AiAlgorithms.Guardian(board,out nodes,out cuts);if(move>=0){lastNodes=nodes;lastPruned=cuts;board[move]=2;}int end=AiAlgorithms.Outcome(board);status=(end==2?"Автомат жеңді. Қайта ойнап көр.":end>0?"Ұтылған жоқсың. Шығуға рұқсат берілді.":"Енді сенің кезегің.");}
    void InitStyles(){if(body!=null)return;var font=Font.CreateDynamicFontFromOSFont(new[]{"Segoe UI","Arial","DejaVu Sans"},20) ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");body=new GUIStyle(GUI.skin.label){font=font,fontSize=20,wordWrap=true,normal={textColor=new Color(.83f,.83f,.77f)}};small=new GUIStyle(body){fontSize=14};heading=new GUIStyle(body){fontSize=34,fontStyle=FontStyle.Bold};title=new GUIStyle(heading){fontSize=96};button=new GUIStyle(GUI.skin.button){font=font,fontSize=20,alignment=TextAnchor.MiddleCenter,wordWrap=true,padding=new RectOffset(9,9,3,3)};subtitle=new GUIStyle(body){fontSize=18,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(0,0,0,0)};box=new GUIStyle(GUI.skin.box){padding=new RectOffset(24,24,24,24)};input=new GUIStyle(GUI.skin.textField){font=font,fontSize=28,padding=new RectOffset(10,10,6,6)};shade=new Texture2D(1,1);shade.SetPixel(0,0,Color.white);shade.Apply();}
    void Fill(Rect rect,Color color){var before=GUI.color;GUI.color=color;GUI.DrawTexture(rect,shade);GUI.color=before;}
    void Text(float x,float y,float w,float h,string value,GUIStyle style=null){GUI.Label(new Rect(x,y,w,h),value,style??body);}
    bool Button(float x,float y,float w,float h,string value)=>GUI.Button(new Rect(x,y,w,h),value,button);
    void OnGUI(){InitStyles();GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1360f,Screen.height/800f,1));GUI.color=Color.white;
        if(State=="menu")titleScreen.Draw(body.font,Panel=="");
        if(State=="intro" && prologue.Draw(body))FinishIntro();
        if(!Paused)DrawPlayHud();
        if(State=="caught"){float black=!Scare?1:Mathf.Clamp01((timer-.55f)*2.5f);Fill(new Rect(0,0,1360,800),new Color(0,0,0,black));}
        if(State=="won"||State=="lost"){Fill(new Rect(0,0,1360,800),new Color(.02f,.04f,.04f,.94f));Text(170,190,1000,120,State=="won"?"СОҢЫ":"ЖЕҢІЛДІҢ",title);Text(180,350,960,100,State=="won"?"Салқын ауа. Алдыңда жол жатыр. Артыңнан есік тарс жабылды. Бұл жолы селк етпедің.\nБес жүйенің бәрі іске қосылды.":"Үшінші күні күзетші сені ұстап алды. Үш мүмкіндігің де таусылды.");if(Button(180,510,350,58,"Жаңа ойын")){Time.timeScale=1;UnityEngine.SceneManagement.SceneManager.LoadScene(0);}}
        if(State=="escape")Text(50,680,1150,70,Story.FarewellStarted?"«Біз әлі кездесеміз, Берекет!»":Story.Clock<4?"Артыңа қарама.":"",heading);
        if(State=="waking"||State=="escape"){
            Fill(new Rect(0,0,1360,58),Color.black);Fill(new Rect(0,742,1360,58),Color.black);
            if(Story!=null && Story.Fade>0)Fill(new Rect(0,0,1360,800),new Color(0,0,0,Story.Fade));
        }
        if(State=="waking")dayTitle.Draw(body.font,Lives,Story.Clock);
        if(Panel!="")DrawPanel();
        if(State=="playing" && (Panel=="report"||Panel=="task") && Audio?.PlayerSubtitle!=null){
            DrawHudCaption(PlayerSpeech, 680, 795, 1020);
        }else if(Panel=="report" && Audio?.TapeSubtitle!=null){
            DrawHudCaption(Audio.TapeSubtitle,680,795,1020);
        }
    }

    float goalTime,itemTime;
    string PlayerSpeech => "Сен: "+Audio.PlayerSubtitle;
    GUIStyle hudLeft,hudCenter,hudSmall;
    readonly GUIContent hudContent=new GUIContent();

    void InitHud(){
        if(hudLeft!=null)return;
        hudLeft=new GUIStyle(subtitle){fontSize=20,normal={textColor=Color.white}};
        hudCenter=new GUIStyle(hudLeft){alignment=TextAnchor.MiddleCenter};
        hudSmall=new GUIStyle(hudLeft){fontSize=16};
    }

    void HudText(Rect rect,string value,GUIStyle style,float opacity=1){
        hudContent.text=value;
        GUI.color=new Color(0,0,0,.95f*opacity);
        foreach(var offset in outlineOffsets){
            var shadow=rect;
            shadow.position+=offset;
            GUI.Label(shadow,hudContent,style);
        }
        GUI.color=new Color(.98f,.97f,.92f,opacity);
        GUI.Label(rect,hudContent,style);
        GUI.color=Color.white;
    }
    static readonly Vector2[] outlineOffsets={new Vector2(-1,0),new Vector2(1,0),new Vector2(0,-1),new Vector2(0,1.5f)};

    float DrawHudCaption(string value,float center,float bottom,float width){
        InitHud();
        hudContent.text=value;
        float height=hudCenter.CalcHeight(hudContent,width)+4;
        HudText(new Rect(center-width/2,bottom-height,width,height),value,hudCenter);
        return bottom-height-5;
    }

    void DrawPlayHud(){
        if(Event.current.type!=EventType.Repaint)return;
        InitHud();
        var previous=GUI.matrix;
        float scale=Mathf.Min(Screen.width/1360f,Screen.height/800f);
        float w=Screen.width/scale,h=Screen.height/scale;
        GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
        if(goalTime>0){
            float alpha=Mathf.Clamp01(goalTime*2);
            HudText(new Rect(25,20,550,22),"ЕСІК ЫСЫРМАЛАРЫ   "+Solved.Count(x=>x)+" / 5     ·     Реті өз еркіңде",hudSmall,alpha);
            HudText(new Rect(25,44,550,48),GoalHint(),hudLeft,alpha);
        }
        if(RadarActive)Receiver.Draw(Player.transform.position,small,(Time.time*62)%360);
        float cx=w/2,cy=h/2;
        if(!Player.hidden){
            Fill(new Rect(cx-2.5f,cy-2.5f,5,5),new Color(0,0,0,.65f));
            Fill(new Rect(cx-1.5f,cy-1.5f,3,3),new Color(1,1,1,.9f));
        }
        string action=null;
        if(Player.hidden && !(Player.Shelter!=null && Player.Shelter.Discovering))action="E  ·  Шкафтан шығу";
        else if(!Player.hidden && Player.Target!=null){
            var target=Player.Target;
            action="E  ·  "+(target.kind=="pickup"&&target.id=="radio"?"Қабылдағышты алу":
                target.kind=="station"&&target.taskIndex==4?"Қабырға пульті · радионы баптау":target.title);
        }
        if(action!=null)HudText(new Rect(cx-280,cy+22,560,50),action,hudCenter);

        if(noise>.09f || Player.crouched){
            HudText(new Rect(25,h-59,200,24),Player.crouched?"Отырып жүрсің":"Дыбыс",hudSmall);
            DrawMeter(25,h-30,noise,new Color(.89f,.72f,.46f));
        }
        var sprint=Player.Sprint;
        if(sprint.Energy<SprintClock.Capacity || sprint.Recovery>0){
            HudText(new Rect(w-185,h-59,160,24),sprint.Recovery>0?"Тынығу · "+Mathf.CeilToInt(sprint.Recovery)+" с":"Төзімділік",hudSmall);
            DrawMeter(w-185,h-30,sprint.Energy/SprintClock.Capacity,new Color(.72f,.84f,.72f));
        }
        if(itemTime>0 && inventory.Count>0){
            HudText(new Rect(25,goalTime>0?96:20,350,42),(selected+1)+"  ·  "+inventory[selected].title,hudSmall,Mathf.Clamp01(itemTime*2));
        }
        float width=Mathf.Min(840,w-410),bottom=h-32;
        if(Audio?.TapeSubtitle!=null)bottom=DrawHudCaption(Audio.TapeSubtitle,cx,bottom,width);
        if(Audio?.PlayerSubtitle!=null)bottom=DrawHudCaption(PlayerSpeech,cx,bottom,width);
        if(captionTime>0)DrawHudCaption(caption,cx,bottom,width);
        GUI.matrix=previous;
    }
    void DrawMeter(float x,float y,float value,Color color){
        Fill(new Rect(x-1,y-1,162,5),new Color(.03f,.04f,.04f,.75f));
        Fill(new Rect(x,y,160*Mathf.Clamp01(value),3),color);
    }
    void DrawPanel(){
        if(Panel=="pause"){DrawPause();return;}
        if(Panel=="archive"){if(Archive.Draw(body))LeavePanel();return;}
        if((Panel=="task" || Panel=="report") && Lesson.Visible){
            Fill(new Rect(0,0,1360,800),new Color(.025f,.035f,.03f,.78f));
            var hudMatrix=GUI.matrix;
            float scale=Mathf.Min(Screen.width/1360f,Screen.height/800f);
            var offset=new Vector3((Screen.width-1360*scale)/2,(Screen.height-800*scale)/2,0);
            GUI.matrix=Matrix4x4.TRS(offset,Quaternion.identity,new Vector3(scale,scale,1));
            Lesson.Draw(body,Panel=="report");
            GUI.matrix=hudMatrix;
            return;
        }
        Fill(new Rect(0,0,1360,800),new Color(0,0,0,.8f));Fill(new Rect(140,45,1080,710),new Color(.075f,.095f,.09f,1));Text(180,65,850,40,Panel=="report"?"АШЫЛҒАН ЫСЫРМА  /  "+Solved.Count(x=>x)+" / 5":Panel=="task"?"ШЫҒАБЕРІС ҚҰРЫЛҒЫСЫ":"ТЫНЫШ / "+(Panel=="settings"?"БАПТАУЛАР":Panel=="help"?"ҮЙ ЕРЕЖЕЛЕРІ":"ҮЗІЛІС"),small);if(Button(1130,60,55,40,"×")){LeavePanel();return;}
        if(Panel=="task" || Panel=="report"){
            if(Button(863,58,246,44,"Қадамдық түсіндірме")){
                Lesson.Enter(taskIndex,circuitMethod,true);return;
            }
        }
        if(Panel=="settings"){Text(180,130,900,70,"Үйдің дыбысы",heading);Text(180,220,300,40,"Дыбыс деңгейі");Volume=GUI.HorizontalSlider(new Rect(500,235,500,25),Volume,0,1);Text(180,290,300,40,"Тінтуір сезімталдығы");Sensitivity=GUI.HorizontalSlider(new Rect(500,305,500,25),Sensitivity,.5f,4);Text(180,360,300,40,"Жарықтық");Brightness=GUI.HorizontalSlider(new Rect(500,375,500,25),Brightness,.6f,2);if(Button(180,430,480,48,Scare?"Кенет шошыту: қосулы":"Кенет шошыту: өшірулі"))Scare=!Scare;if(Button(680,430,410,48,economy?"Графика: үнемді":"Графика: қалыпты"))SetGraphics(!economy);
            if(Button(180,505,480,48,"Python-ды қосу")){python.Start();}Text(180,570,950,70,python.Status,small);if(Button(180,665,300,48,"Сақтау")){SaveOptions();Panel="";}return;}
        if(Panel=="help"){Text(180,130,900,70,"Ол еденнің сықырын естиді.",heading);Text(180,225,940,360,"WASD — жүру, тінтуір — айналаға қарау. E — затты алу, есікті, шкафты немесе құрылғыны пайдалану.\nC — отырып жүру. Бұл кезде еден сықырламайды. Shift — жеті секунд жүгіру, бес секунд тынығу. 1–5 — зат таңдау. Q — лақтыру. F — қолшам. Радионы қабырға пультінде E арқылы бапта. R — есебі. G — қалдыру, қайта бассаң — шақыру (түнде бір рет). Esc — үзіліс.\n\nЕсіктерді жауып жүр, сықырлайтын тақтайларды айналып өт. Затты лақтырып, күзетшіні алдарқатуға болады. Қуғын кезінде де шкафқа жасырына аласың. Қай шкафқа кіргеніңді көрсе, есігін ашады. Көз жазып қалса, соңғы дыбыс шыққан жерді тінтеді.\n\nБес затты тауып, бес ысырманы аш. Тапсырма кезінде күзетші тоқтайды. Алғашқы екі рет ұсталғанда, заттарың мен орындаған тапсырмаларың сақталып, жатын бөлмеде оянасың. Үшінші рет ұсталсаң, ойын аяқталады.");Text(180,635,920,60,"Осы ойын барысында жетістігің сақталады. Жаңа ойынды бастасаң, бәрі басынан басталады.",small);return;}
        if(Panel=="report"){
            Text(180,125,980,85,titles[taskIndex],heading);
            Text(180,235,980,60,topics[taskIndex],heading);
            Text(180,325,955,200,taskReports[taskIndex]);
            if(tapeForTask[taskIndex]>=0 && Button(180,535,465,42,"▶  "+HorrorAudio.TapeTitles[tapeForTask[taskIndex]]))
                Audio.ReplayTape(tapeForTask[taskIndex]);
            if(taskIndex==4)Text(665,533,474,60,ReceiverHint,small);
            if(taskIndex==4&&Has("radio")&&Receiver.LastSound>=0)Text(180,575,955,45,$"Соңғы белгі — {soundNames[Receiver.LastSound]}. Күзетшінің жақын болу ықтималдығы — {Receiver.Near:P0}.",small);
            if(Button(180,640,445,56,"Үйге оралу"))LeavePanel();
            if(Has(needs[taskIndex]) && Button(670,640,440,56,"Қайта шешу"))StartPuzzle(taskIndex);
            return;
        }
        if(Panel=="task")DrawTask();
    }
    static readonly string[] soundNames={"Тыныштық","Сықыр","Аяқ дыбысы"};
    internal void ApplyPower(){
        if(wirePath.Count==0)return;
        Complete($"{EscapeLessons.Names[(int)circuitMethod]}: {EscapeLessons.SearchTips[(int)circuitMethod]}\n\nСенің тізбегің: {wirePath.Count-1} қадам. Құны — {circuitCost}, қаралған ұяшық — {circuitNodes}. Ток релеге жетіп, сыртқы есіктің электрлі ысырмасы ашылды.");
    }
    internal void TurnLock(string digits){
        if(digits.Length==4&&digits.All(char.IsDigit)&&AiAlgorithms.ValidCode(digits.Select(c=>c-'0').ToArray(),sum))Complete($"Тұтқа бұрылып, ысырма ашылды. {digits} коды төрт шартқа да сай. Цифрларының қосындысы — {sum}. Бұл — шектеулерді қанағаттандыру есебі (CSP). Әр диск бір айнымалыны білдіреді, ал шарттар оның мәндерін шектейді. Кодты автоматты іздегенде бір шарт бұзылса, алгоритм кері шегініп, өзге цифрды сынайды.");else status="Код барлық шартқа сай емес. Қосындыны, қайталанған цифрларды және соңғы цифрды тексер.";
    }
    internal void ClaimPass(){
        int outcome=AiAlgorithms.Outcome(board);
        if(outcome!=1 && outcome!=3)return;
        Complete((outcome==3?"Тең ойын. ":"Жеңіс! ")+$"Автомат есікке рұқсат белгісін жіберді. Minimax сенің ең ұтымды жауабыңды ескеріп, жүріс таңдады. Альфа-бета әдісі бұл таңдауды өзгерте алмайтын тармақтарды қарастырмады. Соңғы жүрісте қаралған түйіндер — {lastNodes}, қиылған тармақтар — {lastPruned}.");
    }
    internal void MeasureAir(){
        var r=Request(new PythonBridge.Request{op="bayes",signal=signal});risks=r?.probabilities??AiAlgorithms.Bayes(new[]{1d,1d,1d},signal?new[]{.05,.2,.85}:new[]{.95,.8,.15});
    }
    internal void OpenVent(int i){
        if(risks==null)return;
        if(risks[i]<=risks.Min()+1e-9){Audio.ReleaseVent();Complete($"Ауа қақпақшасы ашылды. Есіктің тағы бір ысырмасы босады. Сен таңдаған арна — {"ABC"[i]}. Ондағы қауіп — {risks[i]:P1}. Байес формуласы датчик көрсеткішіне сүйеніп, бастапқыда бірдей болған үш ықтималдықты жаңартты. Нормалаудан кейінгі қауіп: A — {risks[0]:P1}, B — {risks[1]:P1}, C — {risks[2]:P1}.");}else status="Қауіп бұдан да төмен болуы мүмкін. Үш мәнді салыстыр.";
    }
    internal void RecordSound(int i){
        records.Add(i);if(records.Count>30)records.RemoveAt(0);threat=Hmm(records)[1];status=string.Join(" → ",records.Select(n=>soundNames[n]));
    }
    internal void EnableRadio(){
        if(Panel!="task"||taskIndex!=4||Solved[4]||!Has("radio")||records.Count<3)return;
        Receiver.Prepare();
        Complete($"Ысырма ашылды. Жасыл нүкте — сен. Бір қабатта болсаңдар, қызыл белгі күзетшінің дыбыс шыққан сәттегі орнын көрсетіп, өшеді. Радар астындағы пайыз — HMM бойынша күзетшінің жақын болу ықтималдығы.\n\nСен таңдаған {records.Count} дыбыс бойынша күзетшінің жақын болу ықтималдығы — {threat:P1}. Жасырын Марков моделі (HMM) әр дыбыс сайын «жақын» және «алыс» күйлерінің ауысуын болжайды. Содан кейін естілген дыбысқа қарай болжамын нақтылайды. Сондықтан дыбыстардың реті маңызды.");
    }
    void DrawTask(){Text(180,120,960,75,titles[taskIndex],heading);Text(180,174,980,70,EscapeLessons.Concepts[taskIndex]+" "+EscapeLessons.Payoffs[taskIndex]);
        if(taskIndex==0){
            Text(180,250,450,74,"+ көзін ↗ релесімен жалға. Сымды басып өзгерт: қалыпты (1), үзік (×), қызған (4).");
            for(int r=0;r<6;r++)for(int c=0;c<6;c++){
                int n=r*6+c;
                GUI.backgroundColor=wirePath.Contains(n)?new Color(.45f,.75f,.48f):wires[r,c]==1?new Color(.22f,.24f,.23f):wires[r,c]==2?new Color(.8f,.5f,.2f):Color.gray;
                if(Button(180+c*65,335+r*52,59,46,n==0?"+":n==35?"↗":wires[r,c]==1?"×":wires[r,c]==2?"4":"1")&&n!=0&&n!=35){
                    wires[r,c]=(wires[r,c]+1)%3;wirePath.Clear();status="Сызба өзгерді. Тізбекті қайта тексер.";
                }
            }
            GUI.backgroundColor=Color.white;
            Text(640,250,525,36,"ЖОЛДЫ ҚАЛАЙ ІЗДЕЙМІЗ?",small);
            for(int i=0;i<EscapeLessons.Names.Length;i++){
                GUI.backgroundColor=i==(int)circuitMethod?new Color(.65f,.82f,.66f):Color.gray;
                if(Button(640+i*123,288,115,46,EscapeLessons.Names[i]))SelectCircuit((CircuitMethod)i);
            }
            GUI.backgroundColor=Color.white;
            Text(640,352,500,110,EscapeLessons.SearchTips[(int)circuitMethod]);
            if(Button(640,465,480,48,"Тізбекті тексеру"))ComputeCircuit();
            Text(640,529,505,64,status);
            GUI.enabled=wirePath.Count>0;if(Button(640,603,480,48,"Релеге ток беру"))ApplyPower();GUI.enabled=true;
        }
        if(taskIndex==1){Text(180,245,480,210,$"Құлыптағы шарттар:\n• Цифрлардың қосындысы — {sum}.\n• Төрт цифр да әртүрлі.\n• Бірінші цифр соңғысынан үлкен.\n• Соңғы цифр жұп.");Text(700,250,400,35,"ТӨРТ ДИСКІНІҢ ҚАЛПЫ",small);pin=GUI.TextField(new Rect(700,300,390,55),pin,4,input);if(Button(700,380,390,50,"Тұтқаны бұру"))TurnLock(pin);if(Button(180,480,460,48,"Кодты іздеу")){var result=Request(new PythonBridge.Request{op="csp",sum=sum});int[] code;if(result!=null){code=result.code;}else code=AiAlgorithms.Csp(sum,out _);pin=code==null?"":string.Join("",code);status=code==null?"Шартқа сай код табылмады.":"Дискілер қойылды. Кодты шарттармен салыстыр да, тұтқаны бұр.";}if(Button(180,545,460,42,"Қосындыны өзгерту: "+sum)){sum=sum>=24?16:sum+2;pin="";status="Мән өзгерді. Жаңа код керек.";}Text(700,470,420,150,status);}
        if(taskIndex==2){Text(180,245,930,65,"Сен айқышпен ойнайсың. Ұяшық таңда да, үш белгіні қатар қой немесе тең түс. Содан кейін рұқсатыңды ал.");for(int i=0;i<9;i++){GUI.enabled=board[i]==0&&AiAlgorithms.Outcome(board)==0;if(Button(180+i%3*91,330+i/3*86,83,78,board[i]==0?"·":board[i]==1?"X":"O"))ComputerTurn(i);}GUI.enabled=true;Text(540,350,580,150,status==""?"Сенің кезегің. Қарсыласыңа үш белгіні қатар қоюға жол берме.":status);if(Button(540,530,280,48,"Қайта ойнау")){board=new int[9];status="";}GUI.enabled=AiAlgorithms.Outcome(board)==1||AiAlgorithms.Outcome(board)==3;if(Button(840,530,280,48,"Рұқсатты алу"))ClaimPass();GUI.enabled=true;}
        if(taskIndex==3){Text(180,245,940,100,"Үш арнаның бірінен газ шығып жатыр. Датчик жанама белгі береді. «Қауіпті есептеу» батырмасын бас та, үш мәнді салыстыр. Қауіп ең төмен арнаны аш.");if(Button(180,350,600,48,signal?"Тексеру: C арнасынан белгі бар":"Тексеру: C арнасынан белгі жоқ")){signal=!signal;risks=null;}if(Button(800,350,320,48,"Қауіпті есептеу"))MeasureAir();for(int i=0;i<3;i++){GUI.enabled=risks!=null;if(Button(180+i*315,450,295,90,"АРНА "+"ABC"[i]+"\n"+(risks==null?"—":$"{risks[i]*100:0.0}%")))OpenVent(i);}GUI.enabled=true;Text(180,570,930,55,status);}
        if(taskIndex==4){Text(180,245,940,95,"Төмендегі батырмалармен кемінде үш дыбысты ретімен енгіз. Ысырманы ашып, радарды қосу үшін «Бақылауды қосу» батырмасын бас.");for(int i=0;i<3;i++)if(Button(180+i*315,360,295,60,soundNames[i]))RecordSound(i);Text(180,450,930,65,$"Дыбыс саны: {records.Count} (кемінде 3). Күзетшінің жақын болу ықтималдығы: {threat*100:0.0}%");Text(180,515,930,80,status);GUI.enabled=records.Count>=3;if(Button(180,595,450,48,"Бақылауды қосу"))EnableRadio();GUI.enabled=true;}
    }
}
}



