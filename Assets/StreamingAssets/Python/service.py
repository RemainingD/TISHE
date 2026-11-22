"""Local JSON-lines bridge for Unity. Only the standard Python library is required."""
import json
import sys
from ai_engine import search, csp, bayes
from horror_ai import guardian, listen

def dispatch(req):
    op = req.get('op')
    if op == 'hello':
        return {}
    if op == 'circuit':
        flat = req['grid']
        if len(flat) != 36:
            raise ValueError('Expected 36 cells')
        result = search([flat[i:i+6] for i in range(0,36,6)], [0,0], [5,5], req.get('method', 'astar'))
        return dict(found=result['found'], path=[r*6+c for r,c in result['path']], nodes=result['nodes'], cost=result['cost'] or 0)
    if op == 'csp':
        result = csp(dict(sum=req['sum'], max=9, unique=True, greater=True, even=True))
        return dict(code=result['code'] or [], checks=result['checks'])
    if op == 'guardian':
        result = guardian(['' if n==0 else 'X' if n==1 else 'O' for n in req['board']])
        return dict(move=result['move'] if result['move'] is not None else -1, nodes=result['nodes'], pruned=result['pruned'])
    if op == 'bayes':
        evidence=[None,None,bool(req['signal']),None,None]
        return dict(probabilities=bayes([1,1,1],evidence)['posterior'])
    if op == 'hmm':
        names=['silence','creak','steps']
        obs=req['observations']
        if any(type(n) is not int or n<0 or n>2 for n in obs):
            raise ValueError('Invalid observation')
        result=listen([names[n] for n in obs])
        return dict(probabilities=[result['far'],result['near']])
    raise ValueError('Unknown operation')

if __name__ == '__main__':
    for line in sys.stdin:
        try:
            value = dispatch(json.loads(line))
            print(json.dumps(dict(ok=True, **value), ensure_ascii=False), flush=True)
        except Exception as error:
            print(json.dumps(dict(ok=False,error=str(error)),ensure_ascii=False),flush=True)
