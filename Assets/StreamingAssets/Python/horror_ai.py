"""Two additional syllabus topics: minimax/alpha-beta and HMM filtering."""
from ai_engine import require, integer

LINES=((0,1,2),(3,4,5),(6,7,8),(0,3,6),(1,4,7),(2,5,8),(0,4,8),(2,4,6))
def outcome(board):
    for a,b,c in LINES:
        if board[a] and board[a]==board[b]==board[c]:return board[a]
    return 'draw' if all(board) else None

def guardian(board):
    require(isinstance(board,list) and len(board)==9 and all(v in ('','X','O') for v in board),'Поле должно содержать 9 клеток X/O.')
    require(board.count('X')==board.count('O')+1,'Сначала сделай ход X.')
    require(not any(all(board[i]=='O' for i in line) for line in LINES),'Игра уже закончилась.')
    b=board[:];nodes=0;cuts=0
    def solve(turn,depth,alpha,beta):
        nonlocal nodes,cuts
        nodes+=1;end=outcome(b)
        if end:return 10-depth if end=='O' else depth-10 if end=='X' else 0
        best=-100 if turn=='O' else 100
        for i in range(9):
            if b[i]:continue
            b[i]=turn;v=solve('X' if turn=='O' else 'O',depth+1,alpha,beta);b[i]=''
            if turn=='O':best=max(best,v);alpha=max(alpha,best)
            else:best=min(best,v);beta=min(beta,best)
            if alpha>=beta:cuts+=1;break
        return best
    end=outcome(b)
    if end:return dict(board=b,outcome=end,nodes=0,pruned=0,move=None)
    best=-100;move=None
    for i in range(9):
        if b[i]:continue
        b[i]='O';v=solve('X',1,-100,100);b[i]=''
        if v>best:best=v;move=i
    b[move]='O'
    return dict(board=b,outcome=outcome(b),nodes=nodes,pruned=cuts,move=move)

TRANSITION=((.82,.18),(.25,.75))
EMISSION={'silence':(.8,.2),'creak':(.16,.35),'steps':(.04,.45)}
def listen(observations):
    require(isinstance(observations,list) and 1<=len(observations)<=30 and all(isinstance(o,str) and o in EMISSION for o in observations),'Нужна последовательность звуков.')
    belief=[.8,.2];history=[]
    for o in observations:
        predicted=[sum(belief[i]*TRANSITION[i][j] for i in range(2)) for j in range(2)]
        scores=[predicted[i]*EMISSION[o][i] for i in range(2)];total=sum(scores)
        belief=[x/total for x in scores];history.append(dict(sound=o,near=belief[1]))
    return dict(far=belief[0],near=belief[1],history=history)
