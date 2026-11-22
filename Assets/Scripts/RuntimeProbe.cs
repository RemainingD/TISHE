using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
namespace Tishe {
// Explicit test-only command-line mode, never activated by normal Play.
public class RuntimeProbe : MonoBehaviour {
    string folder;bool failed;int frames;
    void Awake(){Application.runInBackground=true;}
    IEnumerator CaptureDayTitle(GameController g,int day){
        while(g.State=="waking" && g.Story.Clock<1.16f)yield return null;
        if(g.State!="waking" || 4-g.Lives!=day || g.Story.Clock>=2.05f){
            Fail("Missing day title "+day);yield break;
        }
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"day-"+day+".png"));
        yield return new WaitForEndOfFrame();
    }
    void Room(string name,Vector3 from,Vector3 to){var c=GameController.Instance.Player.view.transform;var p=c.position;var r=c.rotation;c.position=from;c.LookAt(to);RenderScene(name);c.position=p;c.rotation=r;}
    void RenderScene(string name){var camera=GameController.Instance.Player.view;var rt=new RenderTexture(1360,800,24);var previous=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(1360,800,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1360,800),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name),image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=previous;Destroy(image);rt.Release();Destroy(rt);}
    IEnumerator MeasureFrames(GameController g){
        QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;
        g.Player.enabled=false;g.Enemy.enabled=false;g.Grace=10000;g.State="playing";g.Panel="";
        var points=new[]{new Vector3(-7.2f,4.28f,11.2f),new Vector3(0,.06f,7.2f),ForestYard.Trail(44)};
        var names=new[]{"bedroom","hall","forest"};
        var target=new RenderTexture(1360,800,24);
        target.antiAliasing=Mathf.Max(1,QualitySettings.antiAliasing);
        var pixel=new Texture2D(1,1,TextureFormat.RGB24,false);
        var watch=new System.Diagnostics.Stopwatch();
        var camera=g.Player.view;
        camera.targetTexture=target;
        var samples=new double[150];
        for(int i=0;i<points.Length;i++){
            g.Player.Teleport(points[i]);g.Player.torch.enabled=true;
            g.Player.view.transform.localPosition=Vector3.up*1.62f;
            for(int frame=-45;frame<samples.Length;frame++){
                float t=(frame+45)*.027f;
                g.Player.transform.position=points[i]+Vector3.forward*(.55f*Mathf.Sin(t));
                g.Player.transform.rotation=Quaternion.Euler(0,(i==2?0:180)+28*Mathf.Sin(t*.7f),0);
                yield return null;
                watch.Restart();
                camera.Render();
                RenderTexture.active=target;
                pixel.ReadPixels(new Rect(680,400,1,1),0,0,false);
                RenderTexture.active=null;
                watch.Stop();
                if(frame>=0)samples[frame]=watch.Elapsed.TotalMilliseconds;
            }
            Array.Sort(samples);
            Debug.LogFormat("FRAME_TEST {0} {1}x{2} median_ms={3:F2} p95_ms={4:F2}",names[i],Screen.width,Screen.height,samples[75],samples[142]);
        }
        camera.targetTexture=null;target.Release();Destroy(target);Destroy(pixel);Application.Quit(0);
    }
    RadioRadar testRadar;
    float testSweepAngle;
    void OnGUI(){
        if(testRadar==null || Event.current.type!=EventType.Repaint)return;
        GUI.depth=-1000;
        GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.blackTexture);
        testRadar.Draw(Vector3.zero,GUI.skin.label,testSweepAngle);
    }
    IEnumerator CheckSweep(GameController g){
        g.enabled=false;
        testRadar=new RadioRadar();testRadar.Prepare();
        int w=Screen.width,h=Screen.height;
        float scale=Mathf.Min(w/1360f,h/800f);
        // These are display pixels measured from the top-left corner.
        var center=new Vector2(w-(38+81)*scale,(22+81)*scale);
        float radius=81*scale;
        var shots=new Color32[4][];
        for(int i=0;i<4;i++){
            testSweepAngle=i*90;
            yield return null;yield return new WaitForEndOfFrame();
            var shot=ScreenCapture.CaptureScreenshotAsTexture();
            shots[i]=shot.GetPixels32();Destroy(shot);
        }
        for(int phase=0;phase<4;phase++){
            int count=0;Vector2 moment=Vector2.zero;
            var a=shots[phase];var b=shots[(phase+2)%4];
            for(int i=0;i<a.Length;i++){
                if(a[i].g-b[i].g<=8)continue;
                var delta=new Vector2(i%w+.5f,h-i/w-.5f)-center;
                if(delta.sqrMagnitude>(radius+1)*(radius+1)){Fail("Sweep escaped dial at "+delta+" in "+w+"x"+h);yield break;}
                count++;moment+=delta;
            }
            float angle=(phase*90-12)*Mathf.Deg2Rad;
            if(count<radius*radius*.035f || Vector2.Dot(moment.normalized,new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)))<.9f){
                Fail("Missing or off-centre sweep at "+phase*90+" degrees in "+w+"x"+h+" pixels="+count);yield break;
            }
        }
        testRadar.Dispose();testRadar=null;
        Debug.Log("TISHE_RADAR_SWEEP_INSIDE_DISC "+w+"x"+h+" four quadrants");Application.Quit(0);
    }

    IEnumerator CheckLivingDoor(GameController g){
        var door=g.Doors.Single(d=>(d.transform.position-new Vector3(-9.6f,0,-4.8f)).sqrMagnitude<.01f);
        var body=g.Player.body;var saved=g.Player.transform.position;
        var panel=door.leaf.GetComponentInChildren<BoxCollider>();
        var inSwing=door.transform.TransformPoint(new Vector3(.06f,.035f,-.72f));
        var safe=new Vector3(-9.55f,.04f,-7.35f);
        g.Player.Teleport(safe);door.opened=false;door.leaf.localRotation=Quaternion.identity;
        g.Player.Teleport(inSwing);Physics.SyncTransforms();door.Toggle();
        yield return new WaitForSecondsRealtime(1.1f);
        float angle=Quaternion.Angle(door.leaf.localRotation,Quaternion.identity);
        if(angle<3 || angle>75 || Vector3.Distance(body.bounds.center,panel.ClosestPoint(body.bounds.center))<body.radius){
            Fail("Living room door opened into player");yield break;
        }
        door.Toggle();yield return new WaitForSecondsRealtime(1.1f);
        if(Quaternion.Angle(door.leaf.localRotation,Quaternion.identity)>1){Fail("Obstructed door could not reverse");yield break;}
        g.Player.Teleport(safe);door.OpenByKeeper();yield return new WaitForSecondsRealtime(1.1f);
        g.Player.Teleport(inSwing);Physics.SyncTransforms();door.Toggle();
        yield return new WaitForSecondsRealtime(1.1f);
        angle=Quaternion.Angle(door.leaf.localRotation,Quaternion.identity);
        if(angle<12 || angle>88 || Vector3.Distance(body.bounds.center,panel.ClosestPoint(body.bounds.center))<body.radius){
            Fail("Living room door closed into player");yield break;
        }
        door.Toggle();g.Player.Teleport(safe);yield return new WaitForSecondsRealtime(1.1f);
        float originalHeight=body.height;var originalCenter=body.center;
        for(int pass=0;pass<2;pass++){
            var pocket=new Vector3(-10.17f,.04f,-6.55f);
            var approach=new Vector3(-9.55f,.04f,-6.55f);
            var otherSide=new Vector3(-9.55f,.04f,-3.14f);
            g.Player.Teleport(pocket);body.height=pass==0?1.75f:1.15f;body.center=Vector3.up*(body.height*.5f);
            Physics.SyncTransforms();
            foreach(var target in new[]{approach,otherSide,approach,pocket}){
                for(int i=0;i<70;i++){
                    var delta=target-g.Player.transform.position;delta.y=0;
                    if(delta.magnitude<.065f)break;
                    body.Move(Vector3.ClampMagnitude(delta,.075f)+Vector3.down*.02f);
                }
                var error=g.Player.transform.position-target;error.y=0;
                if(error.magnitude>.08f){Fail("Player wedged between rocker and door at "+g.Player.transform.position);yield break;}
            }
        }
        body.height=originalHeight;body.center=originalCenter;g.Player.Teleport(saved);
        Debug.Log("TISHE_LIVING_DOOR_SWING_AND_RETURN_OK standing and crouched");
    }
    IEnumerator CapturePause(string filename){
        yield return null;
        yield return new WaitForEndOfFrame();
        var frame=ScreenCapture.CaptureScreenshotAsTexture();
        File.WriteAllBytes(Path.Combine(folder,filename),frame.EncodeToPNG());Destroy(frame);
    }
    static bool waitingForTitle;
    IEnumerator CheckPauseMenu(GameController g){
        if(waitingForTitle){
            if(g.State!="menu"||g.Panel!=""||g.Solved.Any(x=>x)||g.Has("radio")||g.Lives!=3){Fail("Returning from pause did not start a clean title scene");yield break;}
            yield return CapturePause("pause-title.png");
            g.MenuChoice(HouseTitleScreen.Choice.Start);
            if(g.State!="intro"){Fail("Play on returned title screen did not start");yield break;}
            Debug.Log("TISHE_PAUSE_RETURN_TO_TITLE_AND_NEW_GAME_OK");Application.Quit(0);yield break;
        }
        g.State="playing";g.Grace=1000;g.Lives=2;g.Solved[0]=true;
        g.Player.Teleport(new Vector3(-6.6f,.04f,-6.7f));g.Player.transform.LookAt(new Vector3(-10.8f,.04f,-11.2f));g.Player.torch.enabled=true;
        foreach(var item in FindObjectsByType<GameItem>(FindObjectsSortMode.None).Where(i=>i.kind=="pickup"&&(i.id=="radio"||i.id=="gear")))g.Interact(item);
        g.OpenPause();var before=g.Enemy.transform.position;
        yield return new WaitForSecondsRealtime(.3f);
        if(g.Panel!="pause"||Time.timeScale!=0||g.Enemy.transform.position!=before){Fail("Opening pause did not freeze house");yield break;}
        yield return CapturePause("pause-journal.png");
        g.PauseAction(1);
        yield return CapturePause("pause-controls.png");
        if(g.PauseTab!=GameController.PausePage.Controls){Fail("Missing controls");yield break;}
        g.PauseBack();g.PauseAction(2);
        yield return CapturePause("pause-options.png");
        if(g.PauseTab!=GameController.PausePage.Options){Fail("Missing options");yield break;}
        g.PauseBack();g.PauseAction(3);
        yield return CapturePause("pause-exit.png");
        g.PauseBack();g.PauseBack();yield return null;yield return null;
        if(g.Paused||!g.Has("radio")||!g.Solved[0]||g.Lives!=2){Fail("Cancelling return lost the run");yield break;}
        g.OpenPause();g.PauseAction(3);waitingForTitle=true;g.LeaveHouse();
    }
    IEnumerator Start(){folder=Path.GetFullPath(Path.Combine(Application.dataPath,"..","QA"));Directory.CreateDirectory(folder);yield return new WaitForSecondsRealtime(3);var g=GameController.Instance;
        if(Environment.GetCommandLineArgs().Contains("--pause-menu-test")){yield return CheckPauseMenu(g);yield break;}
        if(Environment.GetCommandLineArgs().Contains("--radar-layout-test")){yield return CheckSweep(g);yield break;}
        if(Environment.GetCommandLineArgs().Contains("--frame-test")){yield return MeasureFrames(g);yield break;}
        var uiFont=Font.CreateDynamicFontFromOSFont(new[]{"Segoe UI","Arial","DejaVu Sans"},20);
        const string alphabet="ӘәҒғҚқҢңӨөҰұҮүҺһІі";
        if(uiFont==null || alphabet.Any(c=>!uiFont.HasCharacter(c))){Fail("Kazakh alphabet unavailable");yield break;}
        Destroy(uiFont);
        Debug.Log("TISHE_KAZAKH_GLYPHS_OK");
        if(Resources.Load<Texture2D>("Menu/KeeperHouse")==null){Fail("Missing title artwork");yield break;}
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"01-menu.png"));yield return new WaitForSecondsRealtime(1);
        var menu=g.titleScreen;
        var away=new Vector2(-10,-10);
        var titleTrack=g.transform.Find("WindowWaltz").GetComponent<AudioSource>();
        var introTrack=g.transform.Find("BeforeTheBolts").GetComponent<AudioSource>();
        var uiSound=g.transform.Find("Menu controls").GetComponent<AudioSource>();
        var bed=g.transform.Find("HouseAtNight").GetComponent<AudioSource>();
        var chase=g.transform.Find("Pursuit").GetComponent<AudioSource>();
        if(!titleTrack.isPlaying || !titleTrack.loop || titleTrack.clip.length<40 || introTrack.isPlaying || bed.isPlaying || chase.isPlaying){
            Fail("Title score routing");yield break;
        }
        menu.Navigate(HouseTitleScreen.Row(0).center,false,false,0,false);
        int focusEvents=0;
        System.Action countFocus=()=>focusEvents++;
        menu.SelectionChanged+=countFocus;
        var settingsRow=HouseTitleScreen.Row(1).center;
        menu.Navigate(settingsRow,false,false,0,false);
        menu.Navigate(settingsRow+Vector2.right,false,false,0,false);
        menu.Navigate(away,false,false,0,false);
        if(focusEvents!=1 || !uiSound.isPlaying || uiSound.clip.name!="Select"){Fail("Mouse hover feedback");yield break;}
        menu.Navigate(away,false,false,-1,false);
        menu.SelectionChanged-=countFocus;
        if(focusEvents!=2 || uiSound.clip.name!="Select"){Fail("Keyboard focus feedback");yield break;}
        g.MenuChoice(menu.Navigate(away,false,false,1,true));
        if(g.Panel!="settings" || !uiSound.isPlaying || uiSound.clip.name!="Engage"){Fail("Title keyboard confirmation");yield break;}
        float menuPosition=titleTrack.time;
        float oldVolume=g.Volume;
        g.Volume=0;yield return new WaitForSecondsRealtime(.22f);
        if(titleTrack.volume!=0 || uiSound.volume!=0 || titleTrack.time<=menuPosition){Fail("Muted settings restarted or leaked title audio");yield break;}
        g.Volume=oldVolume;
        g.Panel="";
        var help=HouseTitleScreen.Row(2).center;
        menu.Navigate(help,true,false,0,false);
        g.MenuChoice(menu.Navigate(away,false,true,0,false));
        if(g.Panel!="" || uiSound.clip.name!="Select"){Fail("Title drag away confirmed");yield break;}
        menu.Navigate(help,true,false,0,false);
        g.MenuChoice(menu.Navigate(help,false,true,0,false));
        if(g.Panel!="help" || uiSound.clip.name!="Engage"){Fail("Title mouse confirmation");yield break;}
        g.Panel="";
        g.MenuChoice(menu.Navigate(away,false,false,1,true));
        if(g.Panel!="archive"){Fail("Archive unreachable");yield break;}
        g.Archive.Page=4;g.Archive.Listen(0);g.Archive.Listen(1);g.Archive.Listen(2);
        if(System.Math.Abs(g.Archive.LastBelief-.885812588)>.0001 || g.Solved.Any(x=>x)){Fail("Archive HMM or isolation");yield break;}
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"house-archive.png"));yield return new WaitForSecondsRealtime(.20f);
        g.Archive.Page=0;g.Archive.SelectMethod(CircuitMethod.Bfs);g.Archive.SelectMethod(CircuitMethod.Astar);
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"archive-search.png"));yield return new WaitForSecondsRealtime(.2f);
        g.Archive.Tile(6);g.Archive.Lock(1);g.Archive.Move(0);
        g.Panel="";
        Debug.Log("TISHE_INTERACTIVE_ARCHIVE_OK");
        if(menu.Navigate(away,false,false,1,true)!=HouseTitleScreen.Choice.Quit){Fail("Title quit selection");yield break;}
        g.MenuChoice(menu.Navigate(away,false,false,1,true));yield return null;
        if(g.Player.view.cullingMask==0){Fail("Prologue camera still hidden");yield break;}
        Debug.Log("TISHE_TITLE_MENU_OK");
        if(g.State!="intro" || !g.Paused){Fail("Play did not start prologue");yield break;}
        yield return new WaitForSecondsRealtime(.88f);
        if(titleTrack.isPlaying || !introTrack.isPlaying || introTrack.loop || Mathf.Abs(introTrack.clip.length-42.6f)>.1f || bed.isPlaying || chase.isPlaying){
            Fail("Authored prologue score transition");yield break;
        }
        var narrator=g.transform.Find("Prologue narration").GetComponent<AudioSource>();
        if(!narrator.isPlaying){Fail("Missing opening narration");yield break;}
        g.FinishIntro();g.FinishIntro();
        if(narrator.isPlaying){Fail("Narration continued after skipping");yield break;}
        if(g.State!="waking" || g.Lives!=3){Fail("Skipped intro did not enter first wake");yield break;}
        if(Resources.Load<Texture2D>("Menu/WakeDoor")==null){Fail("Missing day title artwork");yield break;}
        yield return new WaitForSecondsRealtime(.8f);
        if(introTrack.isPlaying || titleTrack.isPlaying){Fail("Skipped score continued into wake");yield break;}
        yield return CaptureDayTitle(g,1);
        yield return new WaitForSecondsRealtime(7.55f);
        if(g.State!="playing"){Fail("Skipped intro never returned control");yield break;}
        g.State="menu";g.Panel="";g.Begin();
        float introDeadline=Time.realtimeSinceStartup+65f;int shotsSeen=0;
        while(g.State=="intro" && Time.realtimeSinceStartup<introDeadline){
            if(g.IntroShot==shotsSeen){
                yield return new WaitForSecondsRealtime(2.2f);
                if(!introTrack.isPlaying || titleTrack.isPlaying || bed.isPlaying || chase.isPlaying){Fail("Wrong music at prologue shot "+shotsSeen);yield break;}
                if(!narrator.isPlaying || narrator.clip.name!="intro_0"+(shotsSeen+1)){Fail("Wrong narration at prologue shot "+shotsSeen);yield break;}
                ScreenCapture.CaptureScreenshot(Path.Combine(folder,"intro-"+(shotsSeen+1)+".png"));shotsSeen++;
            }
            yield return null;
        }
        if(shotsSeen!=4 || g.State!="waking" || g.Lives!=3){Fail("Automatic prologue did not run all four shots and wake");yield break;}
        yield return new WaitForSecondsRealtime(8.35f);
        if(g.State!="playing" || !g.Player.body.enabled || g.Solved.Any(value=>value)){Fail("Prologue failed to hand control to the player");yield break;}
        if(introTrack.isPlaying || titleTrack.isPlaying){Fail("Title music remained in gameplay");yield break;}
        Debug.Log("TISHE_TITLE_FOCUS_CONFIRM_AND_PROLOGUE_SCORE_OK");
        Debug.Log("TISHE_PROLOGUE_SKIP_AND_AUTOPLAY_OK");
        g.State="playing";g.Panel="";g.Grace=10000;g.Enemy.enabled=false;g.Player.enabled=false;
        // The first spoken objective deliberately ducks the score. Test the full-volume mix after it finishes.
        var openingThought=g.transform.Find("Player thoughts").GetComponent<AudioSource>();
        float speechDeadline=Time.realtimeSinceStartup+openingThought.clip.length+2;
        while(openingThought.isPlaying && Time.realtimeSinceStartup<speechDeadline)yield return null;
        if(openingThought.isPlaying){Fail("First objective recording never finished");yield break;}
        g.Volume=.60f;
        yield return new WaitForSecondsRealtime(.8f);
        var valveAir=g.transform.Find("Ventilation at valves").GetComponent<AudioSource>();
        var hallAir=g.transform.Find("Hall air return").GetComponent<AudioSource>();
        var wave=new float[4096];
        valveAir.clip.GetData(wave,0);
        if(!hallAir.isPlaying||!valveAir.isPlaying||hallAir.volume<.24f||wave.Average(x=>x*x)<.009f ||
           Vector3.Distance(hallAir.transform.position,HorrorAudio.ReturnGrille)>.01f){Fail("Vent air absent in gameplay");yield break;}
        g.Panel="pause";yield return null;yield return null;
        if(hallAir.isPlaying||valveAir.isPlaying){Fail("Air did not pause");yield break;}
        g.Panel="";yield return null;yield return null;
        g.Noise(g.Player.transform.position,80,"meter");
        if(g.NoiseLevel!=1){Fail("Meter exceeded maximum");yield break;}
        yield return new WaitForSecondsRealtime(1.2f);
        if(g.NoiseLevel!=0){Fail("Meter stuck after a loud event");yield break;}
        g.Noise(g.Player.transform.position,8,"step");
        if(Mathf.Abs(g.NoiseLevel-.2f)>.001f){Fail("Walking meter");yield break;}
        g.Noise(g.Player.transform.position,25,"step");
        if(Mathf.Abs(g.NoiseLevel-.625f)>.001f){Fail("Running meter");yield break;}
        if(!bed.isPlaying||bed.volume<=0||chase.isPlaying||bed.clip.length<30||chase.clip.length<20){Fail("Exploration music routing");yield break;}
        foreach(string bank in new[]{"BootsWood","BootsSoft","BootsStone","BootsTrail","Floorboards"}){
            var clips=Resources.LoadAll<AudioClip>("Audio/"+bank);
            if(clips.Length<4 || clips.Any(c=>c.length<.08f)){Fail("Missing foley bank "+bank);yield break;}
        }
        var soundDoor=g.Doors[0];soundDoor.opened=false;soundDoor.Toggle();
        var sounds=g.GetComponentsInChildren<AudioSource>();
        if(!sounds.Any(a=>a.isPlaying && a.clip!=null && a.clip.name=="open" && a.spatialBlend==1)){Fail("Door opening recording");yield break;}
        soundDoor.Toggle();
        if(!sounds.Any(a=>a.isPlaying && a.clip!=null && a.clip.name=="close")){Fail("Door closing recording");yield break;}
        g.Noise(soundDoor.transform.position,3,"creak");
        if(!sounds.Any(a=>a.isPlaying && a.clip!=null && a.clip.name.StartsWith("creak"))){Fail("Floorboard recording");yield break;}
        g.Noise(g.Player.transform.position,3,"step");
        if(!g.transform.Find("Player footsteps").GetComponent<AudioSource>().isPlaying){Fail("Audible player step");yield break;}
        g.Enemy.mode="sound";yield return new WaitForSecondsRealtime(.4f);
        if(chase.isPlaying){Fail("Noise alone started the pursuit score");yield break;}
        g.Enemy.mode="chase";yield return new WaitForSecondsRealtime(.85f);
        if(!chase.isPlaying||chase.volume<g.Volume*.55f||bed.volume>.01f){Fail("Chase score transition");yield break;}
        g.Enemy.mode="search";yield return new WaitForSecondsRealtime(2);
        if(chase.isPlaying||!bed.isPlaying||bed.volume<.05f){Fail("House music did not return");yield break;}
        g.Enemy.ResetKeeper();Debug.Log("TISHE_RECORDED_FOLEY_AND_MUSIC_TRANSITIONS_OK");
        var safe=FindFirstObjectByType<SafelightRooms>();
        if(safe==null||safe.lamps.Length!=4||safe.GetComponentsInChildren<Collider>().Length!=0){Fail("Safelight scenery blocks a route or is missing");yield break;}
        bool torchWasOn=g.Player.torch.enabled;
        var camera=g.Player.view.transform;
        g.Player.Teleport(new Vector3(26.37f,-3.55f,9.64f));
        camera.localPosition=Vector3.up*1.57f;camera.LookAt(new Vector3(28.59f,-2.02f,9.62f));
        g.Player.torch.enabled=false;yield return new WaitForSecondsRealtime(.15f);
        if(!safe.lamps[3].enabled||safe.lamps[0].enabled){Fail("Safelight distance limit failed");yield break;}
        RenderScene("darkroom-red.png");
        g.Player.torch.enabled=true; yield return null;
        RenderScene("darkroom-prints.png");
        yield return new WaitForSecondsRealtime(2.8f);
        if(!safe.HeardDarkroomCreak){Fail("Darkroom encounter did not trigger while examining prints");yield break;}
        g.Player.Teleport(new Vector3(-.64f,.05f,9.34f));
        camera.localPosition=Vector3.up*1.56f;camera.LookAt(new Vector3(0,1.62f,13.27f));
        g.Player.torch.enabled=false;yield return new WaitForSecondsRealtime(.15f);
        RenderScene("red-exit.png");
        g.Player.torch.enabled=torchWasOn;
        Debug.Log("TISHE_SAFELIGHTS_DISTANCE_AND_CLEARANCE_OK");
        g.Player.Teleport(HouseMap.Position(HouseMap.Id(1,11,3))+Vector3.up*.05f);
        Vector3 partition=HouseMap.Position(HouseMap.Id(1,8,2));
        foreach(float offset in new[]{-.9f,0f,.9f})
            foreach(float height in new[]{.35f,1.65f,3.8f}){
                Vector3 from=partition+new Vector3(offset,height,2);
                if(!Physics.Raycast(from,Vector3.back,out var hit,1f)||hit.collider.name!="Plaster wall"){
                    Fail("Open bedroom partition at "+from);yield break;
                }
            }
        yield return null;
        var shadowHead=g.Story.Boy.Head.Find("Head").GetComponent<Renderer>();
        if(!shadowHead.enabled||!shadowHead.gameObject.activeInHierarchy||shadowHead.shadowCastingMode!=UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly){
            Fail("Walking head does not cast its shadow");yield break;
        }
        var thoughts=g.transform.Find("Player thoughts").GetComponent<AudioSource>();
        var taskKeys=new[]{"fuse","gear","token","sensor","radio"};
        foreach(var key in KazakhDialogue.Player.Keys){
            var clip=Resources.Load<AudioClip>("Audio/kk/PlayerVoice/"+key);
            if(clip==null||clip.length<.3f){Fail("Missing player recording: "+key);yield break;}
        }
        foreach(var key in new[]{"keeper_0","keeper_1","keeper_2","keeper_3","WardrobeLaugh","EscapeFarewell"}){
            if(Resources.Load<AudioClip>("Audio/kk/"+key)==null){Fail("Missing keeper recording: "+key);yield break;}
        }
        g.Player.view.transform.LookAt(HouseMap.Position(HouseMap.Id(1,9,4))+Vector3.up*1.5f);
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"hud-exploration.png"));
        yield return new WaitForSecondsRealtime(.25f);
        g.AnnounceGoal();
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"hud-objective.png"));
        yield return new WaitForSecondsRealtime(.25f);
        for(int task=0;task<4;task++){
            g.OpenTask(task);
            if(!thoughts.isPlaying||thoughts.clip.name!=taskKeys[task]+"_need"){Fail("Missing-item speech: "+task);yield break;}
        }
        var kitchenLamp=GameObject.Find("Table lamplight").GetComponent<Light>();
        if(kitchenLamp.enabled){Fail("Kitchen lamp has power before fuse repair");yield break;}
        var entrance=g.Entrance;
        if(entrance==null || !Physics.Raycast(new Vector3(0,1.47f,11.75f),Vector3.forward,out var doorHit,2.5f) ||
            doorHit.collider.GetComponentInParent<GameItem>()?.kind!="exit"){Fail("Front door cannot be reached by interaction ray");yield break;}
        g.Interact(doorHit.collider.GetComponentInParent<GameItem>());
        if(g.State!="playing"||Quaternion.Angle(entrance.hinge.localRotation,Quaternion.identity)>.05f){Fail("Locked entrance opened");yield break;}
        Room("exit-door-locked.png",new Vector3(-.42f,1.48f,11.16f),new Vector3(.01f,1.21f,13.25f));
        var pickups=FindObjectsByType<GameItem>(FindObjectsSortMode.None).Where(item=>item.kind=="pickup").ToArray();
        var stations=FindObjectsByType<GameItem>(FindObjectsSortMode.None).Where(item=>item.kind=="station").OrderBy(item=>item.taskIndex).ToArray();
        if(!stations.Select(item=>item.taskIndex).SequenceEqual(Enumerable.Range(0,5))||pickups.Length!=5){Fail("Missing room instruments or loose items");yield break;}
        Physics.SyncTransforms();
        foreach(var item in stations.Concat(pickups)){
            var t=item.transform;
            var floor=t.position.y>3?4.2f:t.position.y<0?-3.6f:0;
            var feet=t.position+t.forward*1.15f;feet.y=floor;
            if(Physics.CheckCapsule(feet+Vector3.up*.27f,feet+Vector3.up*1.45f,.24f,~(1<<2),QueryTriggerInteraction.Ignore)){
                Fail("Blocked standing space for "+item.title);yield break;
            }
            var useOrigin=feet+Vector3.up*1.62f;
            if(!Physics.Raycast(useOrigin,(t.position-useOrigin).normalized,out var use,3.2f,~(1<<2),QueryTriggerInteraction.Ignore)||use.collider.GetComponentInParent<GameItem>()!=item){
                Fail("Obstructed use ray for "+item.title+" at "+t.position+" hit "+use.collider?.name);yield break;
            }
            if(item.kind=="station"){
                if(!Physics.Raycast(t.position+t.forward*.015f,-t.forward,out var backing,.27f,~(1<<2),QueryTriggerInteraction.Ignore)||backing.collider.name!="Plaster wall"){Fail("Panel has no wall behind it: "+item.title);yield break;}
                Room("instrument-"+item.taskIndex+".png",useOrigin+t.forward*.51f+t.right*.34f,t.position);
            }
        }
        var hearth=FindFirstObjectByType<LivingRoomEffects>();
        var crt=new MaterialPropertyBlock();hearth.screen.GetPropertyBlock(crt);
        if(crt.GetFloat("_Power")!=0||!hearth.firelight.enabled){Fail("Hearth or TV initial state");yield break;}
        Debug.Log("TISHE_FIVE_WALL_MOUNTS_AND_PICKUPS_REACHABLE");
        stations[4].Use();g.CheckReceiver();
        if(g.Panel!=""||g.Has("radio")||g.RadarActive){Fail("Unpowered radio opened without receiver");yield break;}
        var firstReceiver=pickups.First(pickup=>pickup.id=="radio");
        g.Interact(firstReceiver);
        if(!g.GoalHint().Contains("баспалдақ")){Fail("First pickup did not suggest the radio panel");yield break;}
        g.CheckReceiver();
        if(g.Panel!=""||!g.Has("radio")){Fail("Taking receiver or pressing R bypassed wall calibration");yield break;}
        if(g.RadarActive){Fail("Radar unlocked by pickup alone");yield break;}
        foreach(int task in new[]{4,1,2,3,0}){
            if(task==1){
                if(!g.RadarActive || g.Solved[0] || g.Solved.Count(v=>v)!=1){Fail("Radio-first campaign progress");yield break;}
                foreach(var pickup in pickups)g.Interact(pickup);
            }
            stations[task].Use();
            if(g.Panel!="task"||!g.Paused||g.ExitBolts[task]==null){Fail("Task did not open: "+task);yield break;}
            if(!g.Lesson.Visible || g.Lesson.Page!=0){Fail("Missing lesson introduction "+task);yield break;}
            if(task==0){ScreenCapture.CaptureScreenshot(Path.Combine(folder,"lesson-entry.png"));yield return new WaitForSecondsRealtime(.15f);}
            g.Lesson.Forward();
            g.Lesson.Forward();
            g.Lesson.Back();
            if(g.Lesson.Page!=1 || g.Lesson.Step!=0){Fail("Lesson back navigation "+task);yield break;}
            if(task==0){
                foreach(CircuitMethod m in Enum.GetValues(typeof(CircuitMethod))){
                    g.Lesson.SelectMethod(m);
                    if(g.Lesson.StepCount<2){Fail("Empty circuit walkthrough "+m);yield break;}
                    for(int i=1;i<g.Lesson.StepCount;i++)g.Lesson.Forward();
                }
            }else for(int i=1;i<g.Lesson.StepCount;i++)g.Lesson.Forward();
            if(g.Lesson.Step!=g.Lesson.StepCount-1 || g.Solved[task]){Fail("Walkthrough changed campaign");yield break;}
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"lesson-"+task+".png"));yield return new WaitForSecondsRealtime(.14f);
            g.Lesson.Forward();
            g.Lesson.Forward();
            if(g.Lesson.Page!=2 || !g.Lesson.Visible){Fail("Unanswered lesson quiz "+task);yield break;}
            g.Lesson.Back();
            if(g.Lesson.Step!=g.Lesson.StepCount-1 || g.Lesson.Page!=1){Fail("Lost example position "+task);yield break;}
            g.Lesson.Forward();g.Lesson.Answer(0);
            bool firstChoice=g.Lesson.AnswerIsCorrect;g.Lesson.Answer(1);
            if(task==2){ScreenCapture.CaptureScreenshot(Path.Combine(folder,"lesson-quiz.png"));yield return new WaitForSecondsRealtime(.15f);}
            if(firstChoice==g.Lesson.AnswerIsCorrect){Fail("Lesson feedback "+task);yield break;}
            g.Lesson.Forward();
            if(g.Lesson.Visible || g.Panel!="task" || g.Solved[task]){Fail("Lesson to device "+task);yield break;}
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"task-"+task+".png"));yield return new WaitForSecondsRealtime(.12f);
            if(!thoughts.isPlaying||thoughts.clip.name!=taskKeys[task]+"_start"){Fail("Task instruction speech: "+task);yield break;}
            float oldX=g.ExitBolts[task].position.x;
            switch(task){
                case 0:
                    foreach(CircuitMethod method in System.Enum.GetValues(typeof(CircuitMethod)))g.SelectCircuit(method);
                    g.SelectCircuit(CircuitMethod.Bfs);
                    ScreenCapture.CaptureScreenshot(Path.Combine(folder,"panel-search.png"));yield return new WaitForSecondsRealtime(.14f);
                    g.ApplyPower();break;
                case 1: g.TurnLock(string.Concat(AiAlgorithms.Csp(18,out _)));break;
                case 2: foreach(int cell in new[]{0,1,6,5,8})g.ComputerTurn(cell);g.ClaimPass();break;
                case 3:
                    g.MeasureAir();g.OpenVent(2);
                    if(g.Solved[3]){Fail("Unsafe vent opened");yield break;}
                    g.OpenVent(0);
                    if(!g.transform.Find("Vent pressure release").GetComponent<AudioSource>().isPlaying){Fail("Missing audible valve release");yield break;}
                    break;
                case 4:
                    g.RecordSound(0);g.RecordSound(1);g.EnableRadio();
                    if(g.Solved[4]){Fail("Two samples unlocked the radar");yield break;}
                    g.RecordSound(2);g.EnableRadio();break;
            }
            if(kitchenLamp.enabled!=g.Solved[0]){Fail("Kitchen lamp is out of sync with fuse repair");yield break;}
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"report-"+task+".png"));yield return new WaitForSecondsRealtime(.12f);
            if(!thoughts.isPlaying||thoughts.clip.name!=(task==0?"relay_done":taskKeys[task]+"_done")){Fail("Task result speech: "+task);yield break;}
            if(g.Panel!="report"||!g.Paused||!g.Solved[task]||oldX-g.ExitBolts[task].position.x<FrontEntrance.BoltTravel-.001f){
                Fail("Missing result or bolt movement: "+task);yield break;
            }
            if(task==4){
                var cassette=GameObject.Find("Family cassette").GetComponent<AudioSource>();
                float until=Time.realtimeSinceStartup+thoughts.clip.length+5;
                while(!cassette.isPlaying && Time.realtimeSinceStartup<until)yield return null;
                if(!cassette.isPlaying || cassette.clip.name!="Memory1" || thoughts.isPlaying || g.Audio.TapeSubtitle==null){
                    Fail("First bolt did not automatically play its family recording after dialogue");yield break;
                }
                ScreenCapture.CaptureScreenshot(Path.Combine(folder,"first-family-tape.png"));yield return new WaitForSecondsRealtime(.18f);
                Debug.Log("TISHE_AUTOMATIC_FIRST_TAPE_OK");
            }
            g.Panel="";
            g.OpenTask(task);
            if(g.Panel!="report"){Fail("Completed task report was lost: "+task);yield break;}
            g.Panel="";
        }
        var deck=GameObject.Find("Family cassette").GetComponent<AudioSource>();
        for(int i=0;i<5;i++){
            var memory=Resources.Load<AudioClip>("Audio/FamilyTape/Memory"+(i+1));
            if(memory==null || memory.length<4f){Fail("Missing family recording "+i);yield break;}
        }
        g.Audio.ReplayTape(4);yield return null;
        if(!deck.isPlaying || deck.clip.name!="Memory5" || g.Audio.TapeSubtitle==null){Fail("Family recording playback");yield break;}
        g.Panel="pause";yield return null;yield return null;
        if(deck.isPlaying){Fail("Cassette did not pause");yield break;}
        g.Panel="";yield return null;yield return null;
        if(!deck.isPlaying){Fail("Cassette did not resume");yield break;}
        Debug.Log("TISHE_FAMILY_TAPE_OK");
        hearth.screen.GetPropertyBlock(crt);
        if(crt.GetFloat("_Power")!=1){Fail("TV did not power on");yield break;}
        Room("hearth-and-tv.png",new Vector3(-8.85f,1.48f,-10.12f),new Vector3(-8.69f,.88f,-12.63f));
        Room("fireplace.png",new Vector3(-9.05f,1.23f,-11.18f),new Vector3(-9.60f,.78f,-12.68f));
        Room("tv-snow.png",new Vector3(-7.46f,1.12f,-10.65f),new Vector3(-7.23f,.93f,-11.90f));
        g.AnnounceGoal();
        if(!thoughts.isPlaying||thoughts.clip.name!="exit_next"){Fail("Exit objective speech");yield break;}
        Debug.Log("TISHE_KAZAKH_26_RECORDINGS_AND_TASK_PLAYBACK_OK");
        if(!g.RadarActive){Fail("Completed radio did not enable radar");yield break;}
        Vector3 upper=HouseMap.Position(HouseMap.Id(1,11,3)), lower=HouseMap.Position(HouseMap.Id(0,2,3)), cellar=HouseMap.Position(345);
        if(RadioRadar.FloorAt(upper)!=1||RadioRadar.FloorAt(lower)!=0||RadioRadar.FloorAt(cellar)!=-1||
           RadioRadar.ShowsKeeper(upper,lower)||RadioRadar.ShowsKeeper(lower,upper)||RadioRadar.ShowsKeeper(lower,cellar)||
           !RadioRadar.ShowsKeeper(cellar,HouseMap.Position(341))||!RadioRadar.ShowsKeeper(upper,upper+Vector3.forward*4)){
            Fail("Radio floor filter");yield break;
        }
        Vector2 uv=RadioRadar.Project(upper,1);
        if(uv.x>=.5f||uv.y<=.5f){Fail("Radio room projection");yield break;}
        g.Panel="pause";
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"hud-pause.png"));
        yield return new WaitForSecondsRealtime(.2f);
        if(g.RadarActive){Fail("Radar leaked over pause");yield break;}
        g.Panel="";
        yield return null;
        Vector3 savedKeeper=g.Enemy.transform.position;
        g.Player.Teleport(upper+Vector3.up*.05f);
        g.Receiver.Reset();
        var firstEcho=HouseMap.Position(HouseMap.Id(1,10,6));
        g.Enemy.transform.position=firstEcho;
        g.ReceiveSound(firstEcho,2,10);
        if(!g.Receiver.TryEcho(upper,out var echo) || echo!=firstEcho || g.Receiver.LastSound!=2 ||
           System.Math.Abs(g.Receiver.Near-.8240936838171173)>1e-9){Fail("First audible step did not immediately produce a fix and HMM update");yield break;}
        g.Enemy.transform.position=firstEcho+Vector3.right*2.4f;
        if(!g.Receiver.TryEcho(upper,out echo) || echo!=firstEcho){Fail("Blip tracked a silent keeper");yield break;}
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"radar-upper.png"));
        yield return new WaitForSecondsRealtime(.2f);
        var secondEcho=HouseMap.Position(HouseMap.Id(1,10,5));
        g.Enemy.transform.position=secondEcho;g.ReceiveSound(secondEcho,2,10);
        if(!g.Receiver.TryEcho(upper,out echo)||echo!=secondEcho||g.Receiver.Near<.94){Fail("Next signal did not move fix and update probability");yield break;}
        g.ListenFor(2.9f);
        if(g.Receiver.TryEcho(upper,out echo)||g.Receiver.LastSound!=2){Fail("Signal afterimage did not fade");yield break;}
        g.ListenFor(1.15f);
        if(g.Receiver.LastSound!=0||g.Receiver.Near>=.6){Fail("Quiet observation did not reduce HMM probability");yield break;}
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"radar-silence.png"));
        yield return new WaitForSecondsRealtime(.14f);
        g.Player.Teleport(lower);
        var otherFloor=lower+Vector3.up*HouseMap.Height;
        g.Enemy.transform.position=otherFloor;g.ReceiveSound(otherFloor,2,16);
        if(g.Receiver.TryEcho(lower,out echo)||g.Receiver.LastSound!=2){Fail("Sound on other floor exposed a position");yield break;}
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"radar-ground.png"));
        yield return new WaitForSecondsRealtime(.2f);
        g.Player.Teleport(cellar);
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"radar-cellar.png"));
        yield return new WaitForSecondsRealtime(.2f);
        g.Player.Teleport(upper+Vector3.up*.05f);
        g.Enemy.transform.position=savedKeeper;
        var receiver=pickups.Single(p=>p.id=="radio");
        g.UseReceiver();
        if(g.RadarActive || g.Has("radio") || !receiver.gameObject.activeSelf){Fail("Receiver placement");yield break;}
        var point=receiver.transform.position;
        g.UseReceiver();
        if(g.LastCallLives==g.Lives){Fail("Call consumed at own feet");yield break;}
        g.Player.Teleport(lower);
        g.Enemy.transform.position=point+Vector3.forward*2;g.Enemy.mode="patrol";
        g.UseReceiver();
        if(g.LastCallLives!=g.Lives || Vector3.Distance(g.Enemy.target,point)>.01f || g.Enemy.mode!="sound"){
            Fail("Keeper did not hear remote receiver");yield break;
        }
        g.Enemy.mode="patrol";g.UseReceiver();
        if(g.Enemy.mode!="patrol"){Fail("Receiver reused on same night");yield break;}
        g.Lives--;g.UseReceiver();
        if(g.LastCallLives!=g.Lives){Fail("Receiver charge did not renew");yield break;}
        g.Lives++;stations[4].Use();
        if(g.Panel!=""||g.RadarActive){Fail("Bare wall panel supplied radar");yield break;}
        g.Interact(receiver);
        if(!g.RadarActive||g.Receiver.LastSound!=-1||g.Receiver.TryEcho(upper,out echo)){Fail("Picking receiver up did not restore a clean radar");yield break;}
        g.Player.Teleport(upper+Vector3.up*.05f);
        g.Enemy.transform.position=savedKeeper;
        g.Drop();g.CheckReceiver();
        if(g.Has("radio")||g.RadarActive||g.Panel!=""){Fail("Throwing receiver left radar active");yield break;}
        g.Interact(receiver);
        var radioDoor=g.Doors[0];bool wasOpen=radioDoor.opened;
        g.Player.Teleport(radioDoor.transform.position+Vector3.right*.8f);
        g.Enemy.transform.position=radioDoor.transform.position+Vector3.forward*.55f;
        radioDoor.opened=false;radioDoor.OpenByKeeper();
        if(g.Receiver.LastSound!=1||!g.Receiver.TryEcho(g.Player.transform.position,out echo)||echo!=g.Enemy.transform.position){Fail("Real door creak missing from HMM");yield break;}
        g.ReceiveSound(g.Player.transform.position+Vector3.right*30,2,10);g.ListenFor(4);
        if(g.Receiver.LastSound!=0||g.Receiver.TryEcho(g.Player.transform.position,out echo)){Fail("Out-of-range sound replaced silence");yield break;}
        g.ReceiveSound(g.Player.transform.position,2,10);
        if(g.Receiver.LastSound!=2||!g.Receiver.TryEcho(g.Player.transform.position,out echo)){Fail("Close keeper footstep missing from HMM");yield break;}
        radioDoor.opened=wasOpen;g.Player.Teleport(upper+Vector3.up*.05f);
        g.Enemy.transform.position=savedKeeper;
        Debug.Log("TISHE_SIGNAL_SNAPSHOTS_AND_HMM_OK");
        Debug.Log("TISHE_LIVE_RECEIVER_OBSERVATIONS_OK");
        Debug.Log("TISHE_REMOTE_RECEIVER_OK");
        Debug.Log("TISHE_RADAR_UNLOCK_FLOORS_OK");
        Debug.Log("TISHE_FIVE_TASK_REPORTS_AND_HEAD_OK");
        var upperDoors=g.Doors.Where(door=>door.transform.position.y>1).ToArray();
        var bedroomDoor=upperDoors.OrderBy(door=>Vector3.Distance(door.transform.position,HouseMap.Position(HouseMap.Id(1,10,4)))).First();
        Vector3 sill=bedroomDoor.transform.position;
        Room("threshold-closed.png",sill+new Vector3(-2.6f,1.48f,.35f),sill+Vector3.up*.12f);
        foreach(var door in upperDoors){
            foreach(float x in new[]{-.48f,0f,.48f})foreach(float z in new[]{-.9f,.9f}){
                Vector3 above=door.transform.TransformPoint(new Vector3(x,.035f,z));
                var surfaces=Physics.RaycastAll(above,Vector3.down,.07f);
                if(surfaces.Length!=1 || surfaces[0].collider.name!="Timber floor"){
                    Fail("Overlapping door threshold: "+above+" / "+string.Join(", ",surfaces.Select(hit=>hit.collider.name)));yield break;
                }
            }
        }
        Debug.Log("TISHE_CLEAR_THRESHOLDS: "+upperDoors.Length+" upstairs doors");
        foreach(var door in g.Doors){if(!Physics.Raycast(door.transform.TransformPoint(new Vector3(0,3.5f,-2)),door.transform.forward,4)){Fail("Unsealed doorway lintel: "+door.transform.position);yield break;}door.opened=true;}
        for(int f=0;f<2;f++)for(int r=1;r<12;r++)for(int c=1;c<12;c++)if(HouseMap.Navigable(f,r,c)&&c!=10){var floor=HouseMap.Position(HouseMap.Id(f,r,c));if(!Physics.Raycast(floor+Vector3.up*.18f,Vector3.down,.5f)){Fail("Missing walkable floor at "+floor);yield break;}}yield return new WaitForSecondsRealtime(1);
        yield return CheckLivingDoor(g);if(failed)yield break;
        Room("threshold-open.png",sill+new Vector3(-2.3f,1.38f,-.72f),sill+Vector3.up*.10f);
        if(FindFirstObjectByType<NurseryMobile>()==null || GameObject.Find("Enamel kettle")==null || GameObject.Find("Star patchwork quilt")==null){Fail("Room furnishings missing");yield break;}
        var fittings=GameObject.Find("Everyday household fittings");
        var surface=GameObject.Find("Plaster wall").GetComponent<Renderer>().sharedMaterial;
        if(fittings==null || fittings.GetComponentsInChildren<Collider>().Length!=0 || surface==null ||
           surface.shader.name!="Tishe/Interior finishes" || !surface.shader.isSupported || surface.GetTexture("_Atlas")==null){
            Fail("Interior surfaces or fittings missing");yield break;
        }
        // The attic and landing used to have an open corner above the upper storey.
        foreach(var x in new[]{5.05f,7.2f,9.6f,12.95f})
            foreach(var z in new[]{5.05f,7.2f,9.6f,12.95f}){
                var origin=new Vector3(x,2*HouseMap.Height-.45f,z);
                if(!Physics.Raycast(origin,Vector3.up,out var overhead,.55f) || overhead.collider.name!="Upper ceiling" ||
                   Mathf.Abs(overhead.point.y-(2*HouseMap.Height-.08f))>.02f){
                    Fail("Missing upper ceiling at "+origin);yield break;
                }
            }
        Room("attic-ceiling.png",new Vector3(12.17f,5.88f,11.83f),new Vector3(7.40f,8.03f,9.59f));
        Room("stair-ceiling.png",new Vector3(9.6f,5.76f,-7.42f),new Vector3(9.63f,8.32f,-3.70f));
        Debug.Log("TISHE_ATTIC_CEILING_OK");
        var dresser=GameObject.Find("Pantry dresser");
        if(dresser==null || GameObject.Find("Linen runner")==null || GameObject.Find("Lithographed label")==null ||
           Resources.Load<Texture2D>("Art/PantryPrints")==null){Fail("Missing pantry fittings");yield break;}
        var aisle=new Vector3(5.95f,4.2f,9.6f);
        if(Physics.CapsuleCast(aisle+Vector3.up*.43f,aisle+Vector3.up*1.48f,.32f,Vector3.right,out var obstruction,6.18f)){
            Fail("Pantry aisle blocked by "+obstruction.collider.name);yield break;
        }
        var cabinets=FindObjectsByType<BoxCollider>(FindObjectsSortMode.None).Where(c=>c.name=="Detailed furniture / GothicCabinet_01").ToArray();
        if(cabinets.Length!=2){Fail("Missing carved hall cabinets");yield break;}
        foreach(var c in cabinets){
            var footprint=c.bounds;
            if(footprint.max.z>=3.56f || footprint.min.x<=-3.43f){Fail("Carved cabinet clips plaster or chair rail");yield break;}
        }
        var stairHide=FindObjectsByType<Wardrobe>(FindObjectsSortMode.None).Single(w=>(w.transform.position-RoomHeirlooms.StairCupboardFeet).sqrMagnitude<.001f);
        var beforeHide=g.Player.transform.position;
        var approach=stairHide.Approach;g.Player.Teleport(approach);
        Physics.SyncTransforms();
        if(Physics.CheckCapsule(approach+Vector3.up*.27f,approach+Vector3.up*1.45f,.24f,~(1<<2),QueryTriggerInteraction.Ignore)||
           !Physics.Raycast(approach+Vector3.up*1.45f+Vector3.right*.25f,Vector3.forward,out var doorsHit,2,~(1<<2))||
           doorsHit.collider.GetComponentInParent<Wardrobe>()!=stairHide){Fail("Understairs hiding place unreachable");yield break;}
        doorsHit.collider.GetComponentInParent<GameItem>().Use();
        yield return new WaitForSecondsRealtime(1.75f);
        if(!g.Player.hidden||!stairHide.Occupied){Fail("Understairs wardrobe entry");yield break;}
        stairHide.Exit();yield return new WaitForSecondsRealtime(1.72f);
        if(g.Player.hidden||!g.Player.body.enabled||Vector3.Distance(g.Player.transform.position,approach)>.12f){Fail("Understairs wardrobe exit");yield break;}
        g.Player.Teleport(beforeHide);g.Player.torch.enabled=true;
        Room("understairs-wardrobe.png",new Vector3(10.8f,1.62f,-11.65f),new Vector3(9.6f,1.4f,-7.9f));
        Room("scattered-portraits.png",new Vector3(-.70f,1.4f,7.65f),new Vector3(-2.0f,.03f,6.67f));
        Room("broken-saucer.png",new Vector3(-10.82f,1.1f,2.19f),new Vector3(-11.26f,.05f,1.4f));
        Room("lost-rabbit.png",new Vector3(-9.65f,5.15f,2.75f),new Vector3(-10.69f,4.37f,1.9f));
        Room("abandoned-suitcase.png",new Vector3(-7.91f,5.45f,10.38f),new Vector3(-7.43f,4.44f,11.7f));
        Debug.Log("TISHE_UNDERSTAIRS_HIDE_AND_ROOM_TRACES_OK");
        Room("hall-carved-cabinet.png",new Vector3(-1.18f,5.78f,1.84f),new Vector3(-3.00f,4.99f,2.52f));
        Room("pantry-entry.png",new Vector3(5.35f,5.79f,9.62f),new Vector3(12.20f,5.26f,10.79f));
        Room("pantry-shelves.png",new Vector3(9.39f,5.73f,9.72f),new Vector3(12.66f,5.33f,10.88f));
        Room("pantry-trunk.png",new Vector3(7.71f,5.66f,10.55f),new Vector3(8.86f,5.08f,12.74f));
        Debug.Log("TISHE_PANTRY_AISLE_AND_CABINETS_OK");
        Room("interior-entry.png",new Vector3(.80f,1.69f,10.58f),new Vector3(1.86f,1.65f,13.15f));
        Room("interior-bath.png",new Vector3(8.93f,5.91f,-9.64f),new Vector3(8.07f,5.59f,-12.91f));
        Room("interior-clock.png",new Vector3(-7.87f,1.70f,10.80f),new Vector3(-6.11f,1.37f,11.53f));
        Room("room-cot.png",new Vector3(-10.30f,5.77f,.64f),new Vector3(-12.00f,5.15f,-.68f));
        Room("room-stove.png",new Vector3(-10.07f,1.67f,-.43f),new Vector3(-12.49f,1.39f,-1.88f));
        Room("room-bedroom.png",new Vector3(-6.5f,5.9f,11.5f),new Vector3(-10.2f,5.1f,9));Room("room-bedroom-portal.png",new Vector3(-9.6f,5.9f,9),new Vector3(-9.6f,5.7f,3));Room("room-hall.png",new Vector3(1.4f,1.7f,9),new Vector3(0,1.7f,-10));Room("room-kitchen.png",new Vector3(-6.5f,1.75f,3),new Vector3(-11,1.1f,-.5f));Room("room-living.png",new Vector3(-6.5f,1.75f,-6.5f),new Vector3(-11,1,-11));Room("room-nursery.png",new Vector3(-6.5f,5.9f,3),new Vector3(-11,5.1f,0));Room("room-study.png",new Vector3(-6.4f,5.9f,-6.4f),new Vector3(-10.5f,5.3f,-11));Room("room-basement.png",new Vector3(22.5f,-1.9f,11),new Vector3(26,-2.1f,7));
        // Start beside the bed, and test the real CharacterController through the corridor and staircase.
        int[] goals={HouseMap.Id(1,7,2),HouseMap.Id(1,3,2),HouseMap.Id(1,10,5),HouseMap.Id(1,2,6),HouseMap.Id(0,10,10),HouseMap.Id(0,10,6),345,HouseMap.Id(1,2,6)};
        foreach(int goal in goals){var path=HouseMap.Route(g.Player.transform.position,HouseMap.Position(goal)).path;foreach(int id in path){Vector3 target=HouseMap.Position(id);float elapsed=0;
            while(Vector2.Distance(new Vector2(g.Player.transform.position.x,g.Player.transform.position.z),new Vector2(target.x,target.z))>.1f){g.Panel="";Vector3 delta=target-g.Player.transform.position;delta.y=0;g.Player.transform.rotation=Quaternion.LookRotation(delta);g.Player.view.transform.localRotation=Quaternion.Euler(6,0,0);float dt=Mathf.Min(Time.unscaledDeltaTime,.04f);g.Player.body.Move((delta.normalized*6+Vector3.down*4)*dt);elapsed+=dt;frames++;if(elapsed>9){Fail("Blocked near "+g.Player.transform.position+" towards "+target);yield break;}yield return null;}
            if(Mathf.Abs(g.Player.transform.position.y-target.y)>.4f){Fail("Incorrect floor at "+target+": "+g.Player.transform.position);yield break;}
        }}
        RenderScene("02-house-world.png");ScreenCapture.CaptureScreenshot(Path.Combine(folder,"02-house.png"));yield return new WaitForSecondsRealtime(1);
        // Check state transitions, pause contract and retained progress without faking a manual playthrough.
        var keeper=g.Enemy;
        keeper.ResetKeeper();keeper.enabled=true;
        Vector3 originalPlayer=g.Player.transform.position;
        Vector3 stairFoot=HouseMap.Position(HouseMap.Id(0,9,10));
        Vector3 stairHead=HouseMap.Position(HouseMap.Id(1,3,10));
        Vector3 kitchen=HouseMap.Position(HouseMap.Id(0,6,2));
        g.Player.Teleport(kitchen);
        keeper.transform.position=stairFoot;
        var kitchenDoor=g.Doors.OrderBy(d=>Vector3.Distance(d.transform.position,new Vector3(-4.8f,0,0))).First();
        kitchenDoor.Toggle();
        if(keeper.mode!="sound" || keeper.target!=kitchenDoor.transform.position){
            Fail("Kitchen door creak did not reach stairs");yield break;
        }
        kitchenDoor.Toggle();
        keeper.Hear(stairHead,80);
        float setupUntil=Time.time+3;
        while(keeper.transform.position.y<.75f && Time.time<setupUntil)yield return null;
        Vector3 beforeCreak=keeper.transform.position;
        if(beforeCreak.y<.75f || keeper.SeesPlayer()){Fail("Blind kitchen / stairs setup");yield break;}
        Vector3 previousTarget=keeper.target;
        g.ClearNoise();g.Player.crouched=true;g.Player.Footfall(kitchen,false);
        if(g.NoiseLevel!=0 || keeper.target!=previousTarget){Fail("Keeper heard crouched kitchen step");yield break;}
        g.Player.crouched=false;g.Player.Footfall(kitchen,false);
        if(Mathf.Abs(g.NoiseLevel-.90f)>.001f){Fail("Creak meter out of scale");yield break;}
        if(keeper.mode!="sound" || keeper.target!=kitchen){Fail("Kitchen floorboard did not reach stairs");yield break;}
        g.Player.Teleport(HouseMap.Position(HouseMap.Id(1,10,2)));
        yield return null;
        var firstMotion=keeper.transform.position-beforeCreak;
        float firstDt=Time.deltaTime;
        float expectedRunSpeed=FirstPerson.SprintSpeed*.9f;
        if(Mathf.Abs(Vector3.Dot(firstMotion,(stairFoot-beforeCreak).normalized)/firstDt-expectedRunSpeed)>.1f){
            Fail("Keeper failed to reverse at running speed on hearing the kitchen");yield break;
        }
        float kitchenUntil=Time.time+12;
        while(keeper.mode=="sound" && Time.time<kitchenUntil)yield return null;
        if(keeper.mode!="search" || Vector3.Distance(keeper.transform.position,kitchen)>.25f){
            Fail("Keeper did not reach the actual kitchen creak: "+keeper.transform.position);yield break;
        }
        Debug.Log("TISHE_KITCHEN_CREAK_OK distance="+Vector3.Distance(beforeCreak,kitchen)+" firstSpeed="+firstMotion.magnitude/Mathf.Max(.0001f,firstDt));
        g.Player.Teleport(originalPlayer);keeper.ResetKeeper();
        Vector3 soundStart=HouseMap.Position(HouseMap.Id(0,9,6));
        Vector3 impact=HouseMap.Position(HouseMap.Id(0,11,6))+Vector3.forward*.55f;
        keeper.transform.position=soundStart;
        g.Noise(impact,1,"step");
        if(keeper.mode!="patrol"){Fail("Inaudible step interrupted patrol");yield break;}
        g.Noise(impact,16,"drop");
        if(keeper.mode!="sound" || keeper.target!=impact){Fail("Audible impact did not interrupt patrol immediately");yield break;}
        yield return null;
        float speed=Vector3.Dot(keeper.transform.position-soundStart,Vector3.forward)/Mathf.Max(.0001f,Time.deltaTime);
        if(Mathf.Abs(speed-expectedRunSpeed)>.1f){Fail("First sound response frame speed: "+speed);yield break;}
        float arrivalLimit=Time.time+2;
        while(keeper.mode=="sound" && Time.time<arrivalLimit)yield return null;
        if(keeper.mode!="search"||Vector3.Distance(keeper.transform.position,impact)>.20f){Fail("Did not reach impact before searching");yield break;}
        g.Noise(soundStart,11,"step");
        if(keeper.mode!="sound"||keeper.target!=soundStart){Fail("New audible step did not interrupt search");yield break;}
        Debug.Log("TISHE_IMMEDIATE_SOUND_SPRINT_OK speed="+speed);
        keeper.ResetKeeper();keeper.transform.position=soundStart;keeper.transform.rotation=Quaternion.identity;
        g.Player.Teleport(impact);g.Grace=0;Physics.SyncTransforms();
        yield return null;
        float chaseSpeed=Vector3.Distance(soundStart,keeper.transform.position)/Mathf.Max(.0001f,Time.deltaTime);
        if(keeper.mode!="chase" || Mathf.Abs(chaseSpeed-expectedRunSpeed)>.1f){
            Fail("Visible chase speed: "+chaseSpeed+" mode="+keeper.mode);yield break;
        }
        Debug.Log("TISHE_CHASE_BALANCE_OK player="+FirstPerson.SprintSpeed+" keeper="+chaseSpeed);
        g.Grace=10000;
        foreach(var cabinet in FindObjectsByType<Wardrobe>(FindObjectsSortMode.None)){
            keeper.ResetKeeper();keeper.transform.position=cabinet.Approach;
            keeper.Hear(cabinet.transform.position,10);
            float until=Time.time+9;
            while(keeper.mode=="sound" && Time.time<until){
                if(cabinet.KeeperObstacle.Contains(keeper.transform.position)){Fail("Keeper crossed cabinet while investigating");yield break;}
                yield return null;
            }
            if(keeper.mode!="search"){Fail("Keeper failed to investigate cabinet from outside at "+cabinet.transform.position);yield break;}
        }
        Debug.Log("TISHE_ALL_WARDROBES_BLOCK_KEEPER_OK");
        var wardrobe=FindObjectsByType<Wardrobe>(FindObjectsSortMode.None).OrderBy(w=>Vector3.Distance(w.transform.position,new Vector3(-7,4.2f,7))).First();
        Vector3 outside=wardrobe.Approach+Vector3.up*.1f;
        g.Enemy.ResetKeeper();g.Enemy.transform.position=HouseMap.Position(HouseMap.Id(1,10,6));
        g.Player.Teleport(outside);g.Enemy.transform.rotation=Quaternion.LookRotation(Vector3.right);Physics.SyncTransforms();
        if(g.Enemy.SeesPlayer()){Fail("Blind corner setup has line of sight");yield break;}
        g.Grace=0;g.Enemy.enabled=true;g.Enemy.mode="chase";g.Enemy.Hear(outside,12);
        if(g.Enemy.target!=outside){Fail("Pursuit ignored last audible step");yield break;}
        g.Interact(wardrobe.GetComponent<GameItem>());
        float entrySettled=Time.time+1.6f;
        float deadline=Time.time+20;
        while(g.Enemy.mode!="search" && Time.time<deadline && g.State=="playing")yield return null;
        if(!g.Player.hidden||g.Player.body.enabled||g.Enemy.mode!="search"){Fail("Unseen entry failed: "+g.Enemy.mode+" at "+g.Enemy.transform.position);yield break;}
        float started=Time.time;Vector3 firstSearch=g.Enemy.transform.position;
        while(Time.time<entrySettled)yield return null;
        Physics.SyncTransforms();
        RenderScene("03-wardrobe.png");
        if(Physics.Raycast(g.Player.view.transform.position,g.Player.view.transform.forward,out var peek,.8f)){Fail("Wardrobe viewing slit obstructed by "+peek.collider.name);yield break;}
        while(g.Enemy.mode=="search" && Time.time<started+8){
            var relative=wardrobe.transform.InverseTransformPoint(g.Enemy.transform.position);
            if(Mathf.Abs(relative.x)<1.45f && Mathf.Abs(relative.z)<1.10f && Mathf.Abs(relative.y)<1){
                Fail("Searching keeper entered the occupied wardrobe at "+relative);yield break;
            }
            yield return null;
        }
        if(Mathf.Abs(Time.time-started-7)>.36f||g.Enemy.mode!="patrol"||g.Lives!=3||Vector3.Distance(firstSearch,g.Enemy.transform.position)<.12f){Fail("Seven second search failed: "+(Time.time-started)+" "+g.Enemy.mode);yield break;}
        g.Enemy.enabled=false;wardrobe.Exit();yield return new WaitForSecondsRealtime(2);
        if(g.Player.hidden||!g.Player.body.enabled||Vector3.Distance(outside,g.Player.transform.position)>.12f){Fail("Wardrobe exit failed");yield break;}
        g.Enemy.ResetKeeper();g.Enemy.transform.position=outside-wardrobe.transform.forward*2.1f-Vector3.up*.1f;
        g.Enemy.transform.rotation=Quaternion.LookRotation(wardrobe.transform.forward);g.Enemy.mode="chase";Physics.SyncTransforms();
        if(!g.Enemy.SeesPlayer()){Fail("Witness setup has no line of sight");yield break;}
        g.Enemy.enabled=true;g.Interact(wardrobe.GetComponent<GameItem>());
        if(g.Enemy.mode!="wardrobe"||!g.Player.hidden){Fail("Witnessed chase entry rejected");yield break;}
        deadline=Time.time+12;while(!wardrobe.Discovering && Time.time<deadline)yield return null;
        if(!wardrobe.Discovering || Vector3.Distance(g.Enemy.transform.position,wardrobe.Approach)>.26f){Fail("Keeper did not open the witnessed wardrobe from outside");yield break;}
        yield return new WaitForSecondsRealtime(1.35f);
        if(Quaternion.Angle(wardrobe.Left.localRotation,Quaternion.identity)<55||g.State!="playing"){Fail("Missing door reveal animation");yield break;}
        RenderScene("wardrobe-reveal.png");g.Solved[0]=true;
        float wait=Time.realtimeSinceStartup+8;while(g.State=="playing"&&Time.realtimeSinceStartup<wait)yield return null;
        if(g.State!="caught"||Quaternion.Angle(wardrobe.Left.localRotation,Quaternion.identity)<55){Fail("Wardrobe doors shut during scare");yield break;}
        Debug.Log("TISHE_WARDROBE_CHASE_AND_SEVEN_SECOND_SEARCH_OK");
        yield return new WaitForSecondsRealtime(.2f);RenderScene("keeper-closeup.png");yield return new WaitForSecondsRealtime(2f);if(g.State!="waking"||g.Player.Shelter!=null){Fail("Missing wake-up or wardrobe release");yield break;}RenderScene("03-wake.png");
        yield return CaptureDayTitle(g,2);
        yield return new WaitForSecondsRealtime(3);RenderScene("03b-sitting.png");yield return new WaitForSecondsRealtime(5);if(g.Lives!=2||!g.Solved[0]||g.State!="playing"){Fail("Capture did not preserve progress");yield break;}
        g.Panel="pause";Vector3 before=g.Enemy.transform.position;g.Enemy.enabled=true;yield return new WaitForSecondsRealtime(.5f);if(g.Enemy.transform.position!=before){Fail("Enemy moved during pause");yield break;}
        g.Panel="";g.Grace=0;g.Capture();yield return new WaitForSecondsRealtime(2.2f);
        if(g.State!="waking"||g.Lives!=1){Fail("Second capture did not start the third day");yield break;}
        yield return CaptureDayTitle(g,3);
        yield return new WaitForSecondsRealtime(8);
        if(g.State!="playing"||!g.Solved[0]){Fail("Third day did not resume with progress");yield break;}
        g.Grace=0;g.Capture();
        while(g.State=="caught")yield return null;
        if(g.State!="lost"||g.Lives!=0){Fail("Final capture started another day");yield break;}
        yield return new WaitForSecondsRealtime(1);
        if(g.State!="lost"){Fail("Defeat screen restarted automatically");yield break;}
        Debug.Log("TISHE_THREE_DAYS_AND_FINAL_DEFEAT_OK");
        g.Panel="";g.State="playing";g.Interact(FindObjectsByType<GameItem>(FindObjectsSortMode.None).First(item=>item.kind=="exit"));yield return new WaitForSecondsRealtime(1.1f);RenderScene("escape-porch.png");
        if(Quaternion.Angle(entrance.hinge.localRotation,Quaternion.identity)<80){Fail("Exit leaf did not open");yield break;}
        if(Physics.CapsuleCast(new Vector3(0,.36f,12.6f),new Vector3(0,1.5f,12.6f),.24f,Vector3.forward,out var passageHit,3.7f,~(1<<2),QueryTriggerInteraction.Ignore)){
            Fail("Unlocked passage blocked by "+passageHit.collider.name+" at "+passageHit.point);yield break;
        }
        yield return new WaitForSecondsRealtime(5.5f);
        Vector3 eye=g.Player.view.transform.position;Vector3 trail=ForestYard.Trail(eye.z);
        if(eye.z<37||Mathf.Abs(eye.x-trail.x)>.2f||Mathf.Abs(eye.y-trail.y-1.48f)>.13f){Fail("First-person run left the trail");yield break;}
        RenderScene("escape-forest.png");wait=Time.realtimeSinceStartup+12;
        while(!g.Story.FarewellStarted && Time.realtimeSinceStartup<wait)yield return null;
        if(!g.Story.FarewellStarted){Fail("Missing farewell cue");yield break;}
        var face=g.Enemy.GetComponent<CharacterRig>().Face;
        float lipMotion=0;wait=Time.realtimeSinceStartup+1.45f;
        while(Time.realtimeSinceStartup<wait){if(face!=null)lipMotion=Mathf.Max(lipMotion,face.GetBlendShapeWeight(0));yield return null;}
        if(Resources.Load<AudioClip>("Audio/kk/EscapeFarewell")!=null && lipMotion<3){Fail("Final speech did not animate the jaw");yield break;}
        RenderScene("escape-keeper.png");
        wait=Time.realtimeSinceStartup+10;while(g.State=="escape"&&Time.realtimeSinceStartup<wait)yield return null;
        if(g.State!="won"){Fail("Escape did not reach THE END");yield break;}
        File.WriteAllText(Path.Combine(folder,"runtime-results.txt"),"PASS: five task completions, debriefs, physical bolts, full player shadow, generated scene, physical navigation including basement and both stair directions; wake-up animation, capture retention, paused enemy seven-second active search, witnessed wardrobe reveal and capture, first-person forest escape and farewell cue. Frames: "+frames+"\nPython mode and all minigame GUI clicks still require manual acceptance.\n");Debug.Log("TISHE_RUNTIME_OK");Application.Quit(0);
    }
    void Fail(string message){failed=true;RenderScene("failure-world.png");var pp=GameController.Instance.Player.transform.position;foreach(var collider in Physics.OverlapCapsule(pp+Vector3.up*.3f,pp+Vector3.up*1.5f,.65f))Debug.Log("NEAR BLOCK: "+collider.name+" "+collider.bounds);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"failure.png"));File.WriteAllText(Path.Combine(folder,"runtime-results.txt"),"FAIL: "+message);Debug.LogError("TISHE_RUNTIME_FAIL: "+message);Application.Quit(2);}
    void Update(){if(Time.realtimeSinceStartup>300&&!failed){Fail("Runtime test timeout");}}
}
}


