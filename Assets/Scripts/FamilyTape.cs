using System.Collections.Generic;
using UnityEngine;

namespace Tishe {
public partial class HorrorAudio {
    internal static readonly string[] TapeTitles={"Шам жанған кеш","Дәліздегі қадам","Тығылмақ","Ескі есік","Үйге шақыру"};
    static readonly string[] TapeLines={
        "Сен: «Шерт етті! Ток бар.»",
        "Атасы: «Әр қадамыңды естіп тұрмын.»",
        "Атасы: «Жасырынба. Бәрібір тауып аламын.»",
        "Сен: «Тұтқа бос айналып тұр.»",
        "Атасы: «Ә, мұнда екенсің ғой. Бері кел.»"
    };
    readonly Queue<int> tapeQueue=new Queue<int>();
    readonly AudioClip[] memories=new AudioClip[5];
    AudioSource tape;AudioClip receiverCall;
    int currentTape=-1;
    bool tapePaused;
    internal string TapeSubtitle=>currentTape<0||tapePaused?null:"Отбасылық таспа  ·  "+(currentTape+1)+"/5   "+TapeLines[currentTape];
    bool TapeWaiting=>currentTape>=0 || tapeQueue.Count>0;

    void LoadTapes(){
        tape=Source("Family cassette",transform);
        for(int i=0;i<5;i++)memories[i]=Resources.Load<AudioClip>("Audio/FamilyTape/Memory"+(i+1));
        receiverCall=Resources.Load<AudioClip>("Audio/FamilyTape/ReceiverCall");
    }
    internal void ReceiverCall(Vector3 at){WorldSound(receiverCall,at,.92f,1);}
    internal void QueueTape(int index){tapeQueue.Enqueue(index);}
    internal void ReplayTape(int index){
        // The replay button is an explicit request to listen now.
        hero.Stop();PlayerSubtitle=null;subtitleUntil=0;
        if(currentTape!=index && currentTape>=0)tapeQueue.Enqueue(currentTape);
        int pending=tapeQueue.Count;
        for(int i=0;i<pending;i++){int next=tapeQueue.Dequeue();if(next!=index)tapeQueue.Enqueue(next);}
        StartTape(index);
    }
    void StartTape(int index){
        currentTape=index;tapePaused=false;
        tape.clip=memories[index];tape.volume=GameController.Instance.Volume*.9f;tape.Play();
    }
    internal void ResetTapes(){
        tapeQueue.Clear();currentTape=-1;tapePaused=false;tape.Stop();
    }
    void TickTape(GameController g){
        bool audible=g.State=="playing" && (g.Panel==""||g.Panel=="report") && !hero.isPlaying && PlayerSubtitle==null;
        tape.volume=g.Volume*.9f;
        if(currentTape>=0){
            if(!audible && !tapePaused){tape.Pause();tapePaused=true;}
            if(audible && tapePaused){tape.UnPause();tapePaused=false;}
            if(audible && !tape.isPlaying)currentTape=-1;
        }
        if(audible && currentTape<0 && tapeQueue.Count>0)StartTape(tapeQueue.Dequeue());
    }
}
}
