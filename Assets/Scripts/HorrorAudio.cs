using System.Collections.Generic;
using UnityEngine;
namespace Tishe {
public partial class HorrorAudio : MonoBehaviour {
    AudioSource foley,voice,ambient,pursuit,shock,enemySteps,dialogue;
    AudioSource titleMusic,prologueMusic,menuFeedback,narrator;
    readonly AudioClip[] prologueLines=new AudioClip[4];
    AudioClip focusClick,confirmClick;
    float titleMix,prologueMix;
    readonly AudioSource[] worldSounds=new AudioSource[8];
    readonly float[] worldGains=new float[8];
    AudioClip[] steps,carpetSteps,stoneSteps,trailSteps,floorCreaks,phrases;
    AudioClip impact,doorOpen,doorClose,sting,laugh,farewell,breath;
    readonly Dictionary<string, AudioClip> heroLines = new Dictionary<string, AudioClip>();
    AudioSource hero;
    string nextHeroLine;
    internal string PlayerSubtitle {get;private set;}
    float subtitleUntil;
    AudioClip lastPlayerStep,lastKeeperStep,lastCreak;
    float voiceCooldown,stride,chaseMix,releaseTime,musicUnderSpeech=1;
    int lastPhrase=-1,runSteps,worldIndex;
    Vector3 previousKeeper;
    readonly float[] syllable=new float[128];
    SkinnedMeshRenderer keeperFace;
    float jaw;

    AudioSource Source(string label,Transform parent,float spatial=0){
        var host=new GameObject(label);host.transform.SetParent(parent,false);
        var src=host.AddComponent<AudioSource>();src.playOnAwake=false;
        src.spatialBlend=spatial;src.dopplerLevel=0;src.minDistance=1.6f;
        src.maxDistance=20;src.rolloffMode=AudioRolloffMode.Linear;
        return src;
    }
    AudioSource Music(string key,bool loop=true){
        var src=Source(key,transform);src.loop=loop;src.volume=0;
        src.clip=Resources.Load<AudioClip>("Audio/Music/"+key);
        return src;
    }
    void Awake(){
        var enemy=GameController.Instance.Enemy.transform;
        dialogue=Source("Keeper dialogue",transform);dialogue.pitch=1;
        laugh=Resources.Load<AudioClip>("Audio/kk/WardrobeLaugh");farewell=Resources.Load<AudioClip>("Audio/kk/EscapeFarewell");
        foley=Source("Player footsteps",transform);shock=Source("Capture sting",transform);
        InstallAir();ambient=Music("HouseAtNight");pursuit=Music("Pursuit");
        titleMusic=Music("WindowWaltz");prologueMusic=Music("BeforeTheBolts",false);
        narrator=Source("Prologue narration",transform);
        for(int i=0;i<prologueLines.Length;i++)
            prologueLines[i]=Resources.Load<AudioClip>("Audio/kk/Prologue/intro_0"+(i+1));
        menuFeedback=Source("Menu controls",transform);
        focusClick=Resources.Load<AudioClip>("Audio/Interface/Select");
        confirmClick=Resources.Load<AudioClip>("Audio/Interface/Engage");
        voice=Source("Keeper voice",enemy,.7f);voice.minDistance=3;voice.maxDistance=26;voice.pitch=1;
        enemySteps=Source("Keeper footsteps",enemy,1);previousKeeper=enemy.position;
        for(int i=0;i<worldSounds.Length;i++)worldSounds[i]=Source("House sound "+i,transform,1);
        steps=Resources.LoadAll<AudioClip>("Audio/BootsWood");carpetSteps=Resources.LoadAll<AudioClip>("Audio/BootsSoft");
        stoneSteps=Resources.LoadAll<AudioClip>("Audio/BootsStone");trailSteps=Resources.LoadAll<AudioClip>("Audio/BootsTrail");
        floorCreaks=Resources.LoadAll<AudioClip>("Audio/Floorboards");
        doorOpen=Resources.Load<AudioClip>("Audio/Doors/open");doorClose=Resources.Load<AudioClip>("Audio/Doors/close");
        impact=Resources.Load<AudioClip>("Audio/impact");
        hero=Source("Player thoughts",transform);LoadTapes();
        foreach(var clip in Resources.LoadAll<AudioClip>("Audio/kk/PlayerVoice"))heroLines[clip.name]=clip;
        breath=heroLines.TryGetValue("run_breath",out var recordedBreath)?recordedBreath:Breathing();sting=CaptureSting();phrases=new AudioClip[4];
        for(int i=0;i<phrases.Length;i++)phrases[i]=Resources.Load<AudioClip>("Audio/kk/keeper_"+i);
    }
    AudioClip Breathing(){
        const int rate=22050;var samples=new float[(int)(rate*.67f)];
        var rng=new System.Random(913);float low=0,band=0;
        for(int i=0;i<samples.Length;i++){
            float t=(float)i/samples.Length;
            float noise=(float)rng.NextDouble()*2-1;
            low+=.24f*(noise-low);band+=.055f*(low-band);
            float envelope=Mathf.Sin(Mathf.PI*t)*Mathf.Exp(-1.7f*t);
            samples[i]=(low-band)*envelope*(.55f+.12f*Mathf.Sin(t*23));
        }
        var clip=AudioClip.Create("Runner breath",samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
    }
    AudioClip CaptureSting(){
        const int rate=22050;var data=new float[(int)(rate*1.4f)];var rng=new System.Random(19);
        for(int i=0;i<data.Length;i++){
            float t=i/(float)rate,n=(float)rng.NextDouble()*2-1;
            data[i]=(n*.22f+Mathf.Sin(t*(900-t*220))*.13f)*Mathf.Exp(-t*3)*Mathf.Min(t*100,1);
        }
        var clip=AudioClip.Create("Capture sting",data.Length,1,rate,false);clip.SetData(data,0);return clip;
    }
    void MenuCue(AudioClip clip){
        menuFeedback.clip=clip;
        menuFeedback.volume=GameController.Instance.Volume;
        if(clip!=null)menuFeedback.Play();
    }
    internal void MenuFocus()=>MenuCue(focusClick);
    internal float MenuConfirm(){
        MenuCue(confirmClick);
        return confirmClick!=null && menuFeedback.volume>0?confirmClick.length:0;
    }
    internal float PlayPrologueLine(int index){
        narrator.Stop();
        narrator.clip=prologueLines[index];
        if(narrator.clip==null)return 0;
        narrator.volume=GameController.Instance.Volume;
        narrator.PlayDelayed(.3f);
        return narrator.clip.length;
    }
    internal void StopPrologueLine()=>narrator.Stop();
    internal void StartPrologueScore(){
        prologueMix=1;
        prologueMusic.volume=GameController.Instance.Volume*.30f;
        if(prologueMusic.clip!=null)prologueMusic.Play();
    }
    void UpdateMusic(GameController g){
        bool exploring=g.State=="playing" && g.Panel=="";
        bool spotted=exploring && g.Enemy.mode=="chase";
        float dt=Time.unscaledDeltaTime;
        bool menu=g.State=="menu";
        titleMix=Mathf.MoveTowards(titleMix,menu?1:0,dt*(menu?.9f:1.6f));
        titleMusic.volume=g.Volume*.62f*titleMix;
        if(menu && !titleMusic.isPlaying && titleMusic.clip!=null)titleMusic.Play();
        else if(!menu && titleMix==0 && titleMusic.isPlaying)titleMusic.Stop();
        if(g.State!="intro")prologueMix=Mathf.MoveTowards(prologueMix,0,dt*2.3f);
        narrator.volume=g.Volume;
        prologueMusic.volume=g.Volume*.30f*prologueMix;
        if(prologueMix==0 && prologueMusic.isPlaying)prologueMusic.Stop();
        menuFeedback.volume=g.Volume;
        if(spotted)releaseTime=.45f;
        else releaseTime=exploring?Mathf.Max(0,releaseTime-dt):0;
        float target=spotted||releaseTime>0?1:0;
        chaseMix=Mathf.MoveTowards(chaseMix,target,dt*(target>chaseMix?4.5f:.85f));
        bool speaking=hero.isPlaying||voice.isPlaying||dialogue.isPlaying||tape.isPlaying;
        musicUnderSpeech=Mathf.MoveTowards(musicUnderSpeech,speaking?.55f:1f,dt*(speaking?4f:1.2f));
        float bed=exploring?g.Volume*.22f*(1-chaseMix)*musicUnderSpeech:0;
        ambient.volume=Mathf.MoveTowards(ambient.volume,bed,dt*.65f);
        pursuit.volume=exploring?g.Volume*.69f*chaseMix*musicUnderSpeech:0;
        PlayLoop(ambient);PlayLoop(pursuit);
    }
    static void PlayLoop(AudioSource source){
        if(source.volume>.0001f){if(!source.isPlaying)source.Play();}
        else if(source.isPlaying)source.Stop();
    }
    void Update(){var g=GameController.Instance;voiceCooldown-=Time.unscaledDeltaTime;
        if(keeperFace==null)keeperFace=g.Enemy.GetComponent<CharacterRig>().Face;
        if(keeperFace!=null){
            float level=0;
            if(dialogue.isPlaying && dialogue.clip!=null){
                dialogue.clip.GetData(syllable,Mathf.Clamp(dialogue.timeSamples,0,dialogue.clip.samples-syllable.Length));
                foreach(float v in syllable)level+=v*v;
            }
            float amount=Mathf.Clamp01(Mathf.Sqrt(level/syllable.Length)*7);
            jaw=Mathf.MoveTowards(jaw,amount,Time.unscaledDeltaTime*(amount>jaw?13:8));
            keeperFace.SetBlendShapeWeight(0,100*jaw);
        }
        dialogue.volume=g.Volume;
        if(g.Panel!="")dialogue.Pause();else dialogue.UnPause();
        hero.volume=g.Volume;
        if(g.State!="playing" || (g.Panel!="" && g.Panel!="task" && g.Panel!="report")){
            if(hero.isPlaying)hero.Stop();
            nextHeroLine=null;
            PlayerSubtitle=null;
        }else if(Time.unscaledTime>=subtitleUntil && !hero.isPlaying){
            PlayerSubtitle=null;
            if(nextHeroLine!=null && !TapeWaiting)PlayerLine(nextHeroLine);
        }
        TickTape(g);UpdateMusic(g);MixAir(g);
        voice.volume=g.Paused?0:g.Volume*.8f;enemySteps.volume=g.Paused?0:g.Volume;
        foley.volume=g.State=="intro"||g.State=="escape"||!g.Paused?g.Volume:0;
        for(int i=0;i<worldSounds.Length;i++)worldSounds[i].volume=g.Paused?0:g.Volume*worldGains[i];
        Vector3 current=g.Enemy.transform.position;float travelled=Vector3.Distance(current,previousKeeper);
        previousKeeper=current;
        if(g.Paused||travelled>1){stride=0;return;}
        stride+=travelled;
        bool running=g.Enemy.mode=="chase" || g.Enemy.mode=="sound";
        float stepLength=running?1.78f:.90f;
        if(stride>=stepLength){stride-=stepLength;PlayStep(enemySteps,current,running?.9f:.67f,true);
            g.ReceiveSound(current,2,running?16:10);}
    }
    static AudioClip Pick(AudioClip[] clips,AudioClip previous){
        if(clips.Length==0)return null;
        int i=Random.Range(0,clips.Length);
        if(clips.Length>1 && clips[i]==previous)i=(i+1+Random.Range(0,clips.Length-1))%clips.Length;
        return clips[i];
    }
    static bool IsStone(Vector3 p)=>p.y<-.5f || (p.x>6 && p.z< -8 && p.y>3);
    static bool IsCarpet(Vector3 p)=>(Mathf.Abs(p.x)<1.15f && Mathf.Abs(p.z)<11.5f) ||
        (p.x> -11.5f && p.x< -8 && Mathf.Abs(p.z)>7);
    internal static bool IsWoodFloor(Vector3 p)=>p.z<=19 && !IsStone(p) && !IsCarpet(p);
    void PlayStep(AudioSource source,Vector3 point,float loudness,bool keeper=false){
        var clips=steps;
        if(point.z>19)clips=trailSteps;
        else if(IsStone(point))clips=stoneSteps;
        else if(IsCarpet(point))clips=carpetSteps;
        var clip=Pick(clips,keeper?lastKeeperStep:lastPlayerStep);
        if(clip==null)return;
        if(keeper)lastKeeperStep=clip;else lastPlayerStep=clip;
        source.pitch=keeper?Random.Range(.85f,.94f):Random.Range(.96f,1.04f);
        source.volume=GameController.Instance.Volume;
        source.PlayOneShot(clip,Mathf.Lerp(.23f,.95f,loudness));
    }
    public void Step(float loudness){PlayStep(foley,GameController.Instance.Player.transform.position,loudness);}
    float Speak(AudioClip clip){
        StopPlayerSpeech();
        voice.Stop();dialogue.clip=clip;dialogue.volume=GameController.Instance.Volume;
        if(clip==null)return 4;
        dialogue.Play();return clip.length/Mathf.Abs(dialogue.pitch)+.15f;
    }
    public float LaughAtWardrobe()=>Speak(laugh);
    public float Farewell()=>Speak(farewell);
    public void RunStep(Vector3 feet){
        PlayStep(foley,feet,.68f);if(++runSteps%4==0)foley.PlayOneShot(breath,.75f);
    }
    void WorldSound(AudioClip clip,Vector3 point,float gain,float pitch){
        if(clip==null)return;
        int i=worldIndex;worldIndex=(worldIndex+1)%worldSounds.Length;
        var src=worldSounds[i];src.Stop();src.transform.position=point;
        worldGains[i]=gain;src.volume=GameController.Instance.Volume*gain;
        src.pitch=pitch;src.clip=clip;src.Play();
    }
    public void Impact(Vector3 point){WorldSound(impact,point,.65f,Random.Range(.9f,1.06f));}
    internal void DarkroomCreak(Vector3 stair){
        WorldSound(Pick(floorCreaks,null),stair,.54f,.84f);
    }
    public void FloorCreak(Vector3 point){
        lastCreak=Pick(floorCreaks,lastCreak);
        WorldSound(lastCreak,point,.78f,Random.Range(.94f,1.06f));
    }
    internal void PrologueFloorboard(){
        var clip=Pick(floorCreaks,null);
        if(clip!=null){foley.pitch=1;foley.PlayOneShot(clip,.43f);}
    }
    public void ExitCreak(){if(doorOpen!=null)foley.PlayOneShot(doorOpen,.76f);}
    public void Door(Vector3 hinge,bool opening,float gain=.75f){
        WorldSound(opening?doorOpen:doorClose,hinge+Vector3.up,gain,Random.Range(.95f,1.04f));
    }
    internal void PlayerLine(string key,bool afterCurrent=false){
        if(afterCurrent && (PlayerSubtitle!=null||TapeWaiting)){nextHeroLine=key;return;}
        nextHeroLine=null;
        hero.Stop();
        KazakhDialogue.Player.TryGetValue(key,out var line);
        PlayerSubtitle=line;
        heroLines.TryGetValue(key,out var clip);
        subtitleUntil=Time.unscaledTime+Mathf.Max(clip!=null?clip.length:0,Mathf.Clamp((line?.Length??0)/16f,2.5f,12));
        if(clip!=null){hero.volume=GameController.Instance.Volume;hero.clip=clip;hero.Play();}
    }
    void StopPlayerSpeech(){
        hero.Stop();nextHeroLine=null;PlayerSubtitle=null;
    }
    public void Spotted(){
        if(voiceCooldown>0||voice.isPlaying||dialogue.isPlaying)return;
        StopPlayerSpeech();
        int i=lastPhrase<0?Random.Range(0,phrases.Length):(lastPhrase+Random.Range(1,phrases.Length))%phrases.Length;
        lastPhrase=i;voiceCooldown=2;
        voice.clip=phrases[i];
        if(voice.clip!=null)voice.Play();
        GameController.Instance.Say(new[]{"Күзетші: «Әр қадамыңды естіп тұрмын».","Күзетші: «Алысқа ұзай алмайсың».","Күзетші: «Жасырынба. Бәрібір тауып аламын».","Күзетші: «Ә, мұнда екенсің ғой. Бері кел»."}[i]);
    }
    public void Capture(){voice.Stop();shock.PlayOneShot(sting,GameController.Instance.Volume*.65f);}
}
}

