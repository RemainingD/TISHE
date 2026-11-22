using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Tishe.Editor {
public static class ProjectBuilder {
    [MenuItem("TISHE/1. Create start scene")]
    public static void CreateScene(){Directory.CreateDirectory("Assets/Scenes");var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);new GameObject("TISHE • Game Director").AddComponent<GameController>();EditorSceneManager.SaveScene(scene,"Assets/Scenes/House.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/House.unity",true)};PlayerSettings.companyName="Tishe Student Project";PlayerSettings.productName="Тыныш — Күзетшінің үйі";PlayerSettings.defaultScreenWidth=1360;PlayerSettings.defaultScreenHeight=800;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.runInBackground=false;PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);PlayerSettings.colorSpace=ColorSpace.Linear;
        Directory.CreateDirectory("Assets/Resources");if(AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/HouseStandard.mat")==null)AssetDatabase.CreateAsset(new Material(Shader.Find("Standard")),"Assets/Resources/HouseStandard.mat");AssetDatabase.SaveAssets();}
    [MenuItem("TISHE/2. Check algorithms")]
    public static void Validate(){SelfTests.Run();Debug.Log("TISHE_VALIDATION_OK");}
    [MenuItem("TISHE/3. Build Windows game")]
    public static void Build(){if(!File.Exists("Assets/Scenes/House.unity"))CreateScene();Validate();Directory.CreateDirectory("Builds/Windows");var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/House.unity"},locationPathName="Builds/Windows/TISHE.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+result.summary.result);Debug.Log("TISHE_BUILD_OK");}
    public static void PrepareAndBuild(){HouseSceneTools.Bake();Build();}
}
public static class SelfTests {
    static int checks;
    static void Check(bool success,string message){checks++;if(!success)throw new Exception("TEST FAILED: "+message);}
    static void AllHumanMoves(int[] b){for(int i=0;i<9;i++){if(b[i]!=0)continue;var next=(int[])b.Clone();next[i]=1;int move=AiAlgorithms.Guardian(next,out _,out _);if(move>=0)next[move]=2;int outcome=AiAlgorithms.Outcome(next);Check(outcome!=1,"Minimax must not lose");if(outcome==0)AllHumanMoves(next);}}
    public static void Run(){checks=0;
        var stamina=new SprintClock();stamina.Tick(true,5);Check(stamina.Energy==2&&stamina.Recovery==0,"Two more seconds of sprint");stamina.Tick(true,2);Check(stamina.Energy==0&&stamina.Recovery==5,"Sprint expires after seven seconds");stamina.Tick(true,4.8f);Check(!stamina.Running&&stamina.Recovery>0,"Exhausted sprint stays locked");stamina.Tick(false,.3f);Check(stamina.Energy==SprintClock.Capacity&&stamina.Recovery==0,"Five-second recovery");Check(stamina.Tick(true,.1f),"Sprint resumes after recovery");
        int[,] grid={{0,2,0},{0,1,0},{0,0,0}};var a=AiAlgorithms.Circuit(grid);var u=AiAlgorithms.Circuit(grid,false);Check(a.Found&&Math.Abs(a.cost-u.cost)<.001&&a.cost==4,"A* weighted optimum");
        Check(!AiAlgorithms.Circuit(new int[,]{{0,1},{1,0}}).Found,"No route");
        int[,] panel={{0,0,0,2,0,0},{0,1,0,1,0,0},{0,1,0,0,0,0},{0,0,1,1,0,0},{1,0,0,0,0,1},{0,0,0,1,0,0}};
        int[] costs={13,10,10,10}, expanded={28,27,22,28};
        foreach(CircuitMethod method in Enum.GetValues(typeof(CircuitMethod))){
            var result=AiAlgorithms.Circuit(panel,method);
            Check(result.Found && result.path.Count==11 && result.cost==costs[(int)method] &&
                result.expanded==expanded[(int)method],"Panel circuit "+method);
            Check(!AiAlgorithms.Circuit(new int[,]{{0,1},{1,0}},method).Found,"Broken circuit "+method);
        }
        var code=AiAlgorithms.Csp(18,out int count);Check(AiAlgorithms.ValidCode(code)&&count>0,"CSP constraints");Check(AiAlgorithms.Csp(35,out _)==null,"Impossible CSP");
        AllHumanMoves(new int[9]);AiAlgorithms.Guardian(new[]{1,0,0,0,0,0,0,0,0},out int nodes,out int pruned);Check(nodes>0&&pruned>0,"Alpha-beta pruning");
        var probabilities=AiAlgorithms.Bayes(new[]{1d,1d,1d},new[]{.05,.20,.85});Check(Math.Abs(probabilities[2]-.85/1.1)<1e-10,"Bayes posterior");bool rejected=false;try{AiAlgorithms.Bayes(new[]{0d},new[]{1d});}catch(ArgumentException){rejected=true;}Check(rejected,"Zero probability rejection");
        var hmm=AiAlgorithms.Hmm(new[]{0,1,2});Check(Math.Abs(hmm[0]+hmm[1]-1)<1e-10,"HMM normalization");Check(Math.Abs(hmm[1]-AiAlgorithms.Hmm(new[]{2,1,0})[1])>.01,"HMM temporal order");
        // Independent enumeration over all hidden-state sequences for three observations.
        double[] total=new double[2];double[,] transition={{.82,.18},{.25,.75}},emission={{.8,.2},{.16,.35},{.04,.45}};
        for(int s0=0;s0<2;s0++)for(int s1=0;s1<2;s1++)for(int s2=0;s2<2;s2++)for(int s3=0;s3<2;s3++)total[s3]+=(s0==0?.8:.2)*transition[s0,s1]*emission[0,s1]*transition[s1,s2]*emission[1,s2]*transition[s2,s3]*emission[2,s3];
        Check(Math.Abs(hmm[1]-total[1]/(total[0]+total[1]))<1e-10,"HMM independent reference");
        var start=HouseMap.Position(HouseMap.Id(1,11,2));int reachable=0;
        for(int f=0;f<2;f++)for(int r=1;r<12;r++)for(int c=1;c<12;c++)if(HouseMap.Navigable(f,r,c)){Check(HouseMap.Route(start,HouseMap.Position(HouseMap.Id(f,r,c))).Found,"House connected");reachable++;}
        Check(reachable>100,"Two-floor map size");Debug.Log("TISHE_SELF_TESTS: "+checks+" assertions passed");
        File.WriteAllText("validation-results.txt",DateTime.UtcNow.ToString("u")+"\n"+checks+" C# assertions passed: weighted search, CSP, exhaustive minimax, Bayesian posterior, HMM reference and house connectivity.\nNot a manual gameplay walkthrough.\n");
    }
}
}
