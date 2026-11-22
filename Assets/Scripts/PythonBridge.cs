using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
namespace Tishe {
// Local stdin/stdout JSON protocol: no network port and no third-party packages.
public sealed class PythonBridge : IDisposable {
    [Serializable] public class Request { public string op;public string method="astar";public int[] grid,board,observations;public int sum=18;public bool signal=true; }
    [Serializable] public class Response {public bool ok,found;public string error;public int[] path,code;public int nodes,pruned,move=-1,checks;public float cost;public double[] probabilities;}
    Process process;public bool Ready=>process!=null&&!process.HasExited;public string Status="C# • дербес режим";
    public bool Start() {
        Dispose();try {
            string script=Path.Combine(Application.streamingAssetsPath,"Python","service.py");
            string configured=Environment.GetEnvironmentVariable("TISHE_PYTHON");bool windows=Application.platform==RuntimePlatform.WindowsPlayer||Application.platform==RuntimePlatform.WindowsEditor;
            string executable=configured??(windows?"py":"python3");string arguments=(configured==null&&windows?"-3 ":"")+"-u \""+script+"\"";
            var start=new ProcessStartInfo(executable,arguments){UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardOutput=true,RedirectStandardError=true,StandardOutputEncoding=Encoding.UTF8,StandardErrorEncoding=Encoding.UTF8};
            start.EnvironmentVariables["PYTHONIOENCODING"]="utf-8";process=Process.Start(start);process.ErrorDataReceived+=(s,e)=>{};process.BeginErrorReadLine();
            Ask(new Request{op="hello"});Status="Python • оқу есептері / C# • ойын әлемі";return true;
        }catch(Exception e){UnityEngine.Debug.LogWarning(e.Message);Status="C# • Python іске қосылмады";Dispose();return false;}
    }
    public Response Ask(Request request) {
        if(!Ready)throw new InvalidOperationException("Python қосылмаған");process.StandardInput.WriteLine(JsonUtility.ToJson(request));process.StandardInput.Flush();var reading=process.StandardOutput.ReadLineAsync();
        if(!reading.Wait(2500)){Dispose();throw new TimeoutException("Python 2,5 секунд ішінде жауап бермеді");}
        var response=JsonUtility.FromJson<Response>(reading.Result??"{}");if(response==null||!response.ok)throw new InvalidOperationException(response?.error??"Python-нан жауап келмеді");return response;
    }
    public void Dispose(){if(process!=null){try{if(!process.HasExited)process.Kill();}catch{}process.Dispose();process=null;}}
}
}
