using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tishe {
// The course algorithms are implemented here, not delegated to NavMesh or an AI API.
public enum CircuitMethod { Bfs, Dfs, Astar, Ucs }
public static class AiAlgorithms {
    public sealed class SearchResult { public List<int> path = new List<int>(); public int expanded; public float cost; public bool Found => path.Count > 0; }
    public static SearchResult Search(int start, int goal, Func<int,IEnumerable<KeyValuePair<int,float>>> edges, Func<int,float> heuristic,bool renewTieOrder=false,List<int> visitOrder=null) {
        var result = new SearchResult(); var open = new List<int>{start};
        var cost = new Dictionary<int,float>{{start,0}}; var parent = new Dictionary<int,int>();
        while(open.Count > 0) {
            int current = open[0]; float score = cost[current] + heuristic(current);
            foreach(int v in open) { float f = cost[v] + heuristic(v); if(f < score) {score=f; current=v;} }
            open.Remove(current); result.expanded++;visitOrder?.Add(current);
            if(current == goal) { result.cost=cost[current]; result.path.Add(current); while(parent.ContainsKey(current)){current=parent[current];result.path.Add(current);}result.path.Reverse();return result; }
            foreach(var edge in edges(current)) {
                float nextCost = cost[current]+edge.Value;
                if(!cost.ContainsKey(edge.Key) || nextCost < cost[edge.Key]) {cost[edge.Key]=nextCost;parent[edge.Key]=current;if(renewTieOrder)open.Remove(edge.Key);if(!open.Contains(edge.Key))open.Add(edge.Key);}
            }
        }
        return result;
    }
    public static SearchResult Circuit(int[,] grid,bool useHeuristic=true) =>
        Circuit(grid,useHeuristic?CircuitMethod.Astar:CircuitMethod.Ucs);

    public static SearchResult Circuit(int[,] grid,CircuitMethod method,List<int> visitOrder=null) {
        visitOrder?.Clear();
        int rows=grid.GetLength(0),cols=grid.GetLength(1),goal=grid.Length-1;
        var result=new SearchResult();
        if(grid[0,0]==1 || grid[rows-1,cols-1]==1)return result;
        if(method==CircuitMethod.Astar || method==CircuitMethod.Ucs)
            return Search(0,goal,n=>GridEdges(grid,n),
                n=>method==CircuitMethod.Astar?rows-1-n/cols+cols-1-n%cols:0,true,visitOrder);

        // Same neighbour and discovery order as the Python service.
        var frontier=new LinkedList<int>();
        var parent=new Dictionary<int,int>{{0,-1}};
        frontier.AddLast(0);
        while(frontier.Count>0){
            var next=method==CircuitMethod.Dfs?frontier.Last:frontier.First;
            int cell=next.Value;frontier.Remove(next);result.expanded++;visitOrder?.Add(cell);
            if(cell==goal){
                for(int n=cell;n!=-1;n=parent[n]){
                    result.path.Add(n);
                    if(n!=0)result.cost+=grid[n/cols,n%cols]==2?4:1;
                }
                result.path.Reverse();break;
            }
            foreach(var edge in GridEdges(grid,cell)){
                if(parent.ContainsKey(edge.Key))continue;
                parent.Add(edge.Key,cell);frontier.AddLast(edge.Key);
            }
        }
        return result;
    }
    static IEnumerable<KeyValuePair<int,float>> GridEdges(int[,] grid,int n) {
        int w=grid.GetLength(1),h=grid.GetLength(0),r=n/w,c=n%w;
        int[] dr={0,1,0,-1},dc={1,0,-1,0};
        for(int i=0;i<4;i++){int rr=r+dr[i],cc=c+dc[i];if(rr>=0&&cc>=0&&rr<h&&cc<w&&grid[rr,cc]!=1)yield return new KeyValuePair<int,float>(rr*w+cc,grid[rr,cc]==2?4:1);}
    }
    public static bool ValidCode(int[] code,int sum=18) => code!=null && code.Length==4 && code.All(n=>n>=0&&n<=9) && code.Sum()==sum && code.Distinct().Count()==4 && code[0]>code[3] && code[3]%2==0;
    public static int[] Csp(int sum,out int checks) {
        int count=0;var digits=new int[4];int[] answer=null;
        bool Visit(int k,int total) {
            if(total>sum || total+(4-k)*9<sum)return false;
            if(k==4) {if(ValidCode(digits,sum)){answer=(int[])digits.Clone();return true;}return false;}
            for(int v=0;v<=9;v++){count++;bool duplicate=false;for(int i=0;i<k;i++)if(digits[i]==v)duplicate=true;if(duplicate)continue;if(k==3&&(v%2!=0||digits[0]<=v))continue;digits[k]=v;if(Visit(k+1,total+v))return true;}return false;
        }
        Visit(0,0);checks=count;return answer;
    }
    static readonly int[,] lines={{0,1,2},{3,4,5},{6,7,8},{0,3,6},{1,4,7},{2,5,8},{0,4,8},{2,4,6}};
    public static int Outcome(int[] b) { for(int i=0;i<8;i++)if(b[lines[i,0]]!=0 && b[lines[i,0]]==b[lines[i,1]] && b[lines[i,0]]==b[lines[i,2]])return b[lines[i,0]];return b.All(x=>x!=0)?3:0; }
    public static int Guardian(int[] board,out int nodes,out int pruned) {
        if(board==null||board.Length!=9||board.Any(n=>n<0||n>2)||board.Count(n=>n==1)!=board.Count(n=>n==2)+1)throw new ArgumentException("Алдымен X жүреді");
        int visited=0,cuts=0;var b=(int[])board.Clone();
        int Solve(int turn,int depth,int alpha,int beta) {
            visited++;int end=Outcome(b);if(end!=0)return end==2?10-depth:end==1?depth-10:0;
            int best=turn==2?-100:100;
            for(int i=0;i<9;i++){if(b[i]!=0)continue;b[i]=turn;int value=Solve(3-turn,depth+1,alpha,beta);b[i]=0;if(turn==2){best=Math.Max(best,value);alpha=Math.Max(alpha,best);}else{best=Math.Min(best,value);beta=Math.Min(beta,best);}if(alpha>=beta){cuts++;break;}}
            return best;
        }
        int move=-1,score=-100;if(Outcome(b)==0)for(int i=0;i<9;i++){if(b[i]!=0)continue;b[i]=2;int s=Solve(1,1,-100,100);b[i]=0;if(s>score){score=s;move=i;}}
        nodes=visited;pruned=cuts;return move;
    }
    public static double[] Bayes(double[] prior,double[] likelihood) {
        if(prior==null||likelihood==null||prior.Length!=likelihood.Length||prior.Length==0)throw new ArgumentException("Ықтималдық мәндері дұрыс емес");
        var p=new double[prior.Length];double total=0;
        for(int i=0;i<p.Length;i++){if(double.IsNaN(prior[i])||double.IsInfinity(prior[i])||prior[i]<0||double.IsNaN(likelihood[i])||likelihood[i]<0||likelihood[i]>1)throw new ArgumentException("Ықтималдық мәндері дұрыс емес");total+=p[i]=prior[i]*likelihood[i];}
        if(total<=0||double.IsInfinity(total))throw new ArgumentException("Бұл бақылау нәтижесі мүмкін емес");for(int i=0;i<p.Length;i++)p[i]/=total;return p;
    }
    internal static double PredictNear(double far,double near)=>far*.18+near*.75;
    static readonly double[,] soundLikelihood={{.8,.2},{.16,.35},{.04,.45}};
    internal static double UpdateNear(double previous,int sound) {
        if(sound<0 || sound>2)throw new ArgumentException("Белгісіз дыбыс");
        double predicted=PredictNear(1-previous,previous);
        double near=predicted*soundLikelihood[sound,1];
        double far=(1-predicted)*soundLikelihood[sound,0];
        return near/(near+far);
    }
    public static double[] Hmm(IEnumerable<int> observations) {
        double near=.2;
        foreach(int sound in observations)near=UpdateNear(near,sound);
        return new[]{1-near,near};
    }
}
public static class HouseMap {
    public const float Cell=2.4f, Height=4.2f;
    public static int Id(int floor,int row,int col)=>floor*169+row*13+col;
    public static Vector3 Position(int id){if(id==338)return new Vector3(14.4f,0,9.6f);if(id==339)return new Vector3(15.6f,0,9.6f);if(id==340)return new Vector3(22.8f,-3.6f,9.6f);if(id>=341){int n=id-341;return new Vector3(22.8f+n%3*Cell,-3.6f,7.2f+n/3*Cell);}return new Vector3((id%13-6)*Cell,id/169*Height,(id%169/13-6)*Cell);}
    public static bool Walkable(int f,int r,int c) {
        if(f<0||f>1||r<1||r>11||c<1||c>11)return false;
        if(c==4&&r!=2&&r!=6&&r!=10)return false;
        if(c==8&&r!=2&&r!=10)return false;
        // The upstairs bedroom opens only onto the hall.
        if(f==1&&r==8&&c==2)return false;
        if((r==4||r==8)&&c<4&&c!=2)return false;
        if(r==4&&c>=5&&c<=7&&c!=5)return false;
        if(r==8&&c>=5&&c<=7&&c!=7)return false;
        if(r>=3&&r<=9&&(c==9||c==11))return false;
        if(c==10&&r>=3&&r<=9)return r==(f==0?9:3);
        return true;
    }
    public static bool Navigable(int f,int r,int c) {
        if(!Walkable(f,r,c))return false;
        if(c==1&&(r==2||r==6||r==10))return false;
        if(c==2&&(r==2||r==10||(f==1&&r==6)))return false;
        if(f==0&&((c==10&&r==2)||(c==11&&r==11)))return false;
        return true;
    }
    public static IEnumerable<KeyValuePair<int,float>> Edges(int id) {
        if(id>=338){foreach(int other in ExtraNeighbors(id))yield return new KeyValuePair<int,float>(other,Vector3.Distance(Position(id),Position(other)));yield break;}
        int f=id/169,r=id%169/13,c=id%13;int[] dr={1,-1,0,0},dc={0,0,1,-1};
        for(int i=0;i<4;i++)if(Navigable(f,r+dr[i],c+dc[i]))yield return new KeyValuePair<int,float>(Id(f,r+dr[i],c+dc[i]),Cell);
        if(id==Id(0,9,10))yield return new KeyValuePair<int,float>(Id(1,3,10),Vector3.Distance(Position(id),Position(Id(1,3,10))));
        if(id==Id(1,3,10))yield return new KeyValuePair<int,float>(Id(0,9,10),Vector3.Distance(Position(id),Position(Id(0,9,10))));
        if(id==Id(0,10,11))yield return new KeyValuePair<int,float>(338,Cell);
    }
    static IEnumerable<int> ExtraNeighbors(int id){if(id==338){yield return Id(0,10,11);yield return 339;}else if(id==339){yield return 338;yield return 340;}else if(id==340){yield return 339;yield return 344;}else {if(id==344)yield return 340;int n=id-341,r=n/3,c=n%3;int[] dr={1,-1,0,0},dc={0,0,1,-1};for(int i=0;i<4;i++){int rr=r+dr[i],cc=c+dc[i];if(rr>=0&&rr<3&&cc>=0&&cc<3&&rr*3+cc!=8)yield return 341+rr*3+cc;}}}
    public static int Nearest(Vector3 p) {int best=0;float d=float.MaxValue;for(int f=0;f<2;f++)for(int r=1;r<12;r++)for(int c=1;c<12;c++)if(Navigable(f,r,c)){int id=Id(f,r,c);float dd=(Position(id)-p).sqrMagnitude;if(dd<d){d=dd;best=id;}}for(int id=338;id<349;id++){float dd=(Position(id)-p).sqrMagnitude;if(dd<d){d=dd;best=id;}}return best;}
    public static AiAlgorithms.SearchResult Route(Vector3 a,Vector3 b) {int end=Nearest(b);return AiAlgorithms.Search(Nearest(a),end,Edges,n=>Vector3.Distance(Position(n),Position(end)));}
}
}

