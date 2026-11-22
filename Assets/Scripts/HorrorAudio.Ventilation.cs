using UnityEngine;

namespace Tishe {
public partial class HorrorAudio {
    AudioSource upstairsAir,hallAir,valve;
    static readonly Vector3 airPanel=new Vector3(12.86f,5.72f,-9.64f);
    internal static readonly Vector3 ReturnGrille=new Vector3(3.45f,1.88f,-1.55f);
    AudioSource AirAt(string label,Vector3 point,AudioClip clip){
        var source=Source(label,transform,1);
        source.transform.position=point;source.clip=clip;source.loop=true;
        source.minDistance=2.5f;source.maxDistance=13;source.volume=0;
        return source;
    }
    void InstallAir(){
        var air=Resources.Load<AudioClip>("Audio/DuctAir");
        upstairsAir=AirAt("Ventilation at valves",airPanel,air);
        hallAir=AirAt("Hall air return",ReturnGrille,air);
        hallAir.pitch=.92f;
        valve=Source("Vent pressure release",transform,.45f);
        valve.transform.position=airPanel;valve.minDistance=3;
        valve.clip=Resources.Load<AudioClip>("Audio/VentRelease");
    }
    void MixAir(GameController g){
        float gain=g.Paused?0:g.Volume*(g.Solved[3]?.82f:.68f);
        upstairsAir.volume=gain;hallAir.volume=gain*.81f;
        PlayLoop(upstairsAir);PlayLoop(hallAir);
        valve.volume=g.Volume;
        if(g.State!="playing"||g.Panel=="pause")valve.Stop();
    }
    internal void ReleaseVent(){valve.volume=GameController.Instance.Volume;valve.Play();}
}
}
