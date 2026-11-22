"""Exercise the exact stdin/stdout protocol used by the native Unity game."""
import json
from pathlib import Path
import subprocess
import sys
import unittest

SERVICE = Path(__file__).resolve().parents[1] / 'Assets/StreamingAssets/Python/service.py'

class ProtocolTests(unittest.TestCase):
    def run_requests(self, requests):
        p = subprocess.run([sys.executable, '-X', 'utf8', '-u', str(SERVICE)],
            input=''.join(json.dumps(r)+'\n' for r in requests),
            capture_output=True, text=True, encoding='utf-8', timeout=10)
        self.assertEqual(p.returncode, 0, p.stderr)
        replies = [json.loads(line) for line in p.stdout.splitlines()]
        self.assertEqual(len(replies), len(requests))
        return replies

    def test_five_topics(self):
        results = self.run_requests([
            {'op':'circuit','grid':[0]*36},
            {'op':'csp','sum':18},
            {'op':'guardian','board':[1]+[0]*8},
            {'op':'bayes','signal':True},
            {'op':'hmm','observations':[0,1,2]}])
        self.assertTrue(all(r['ok'] for r in results))
        self.assertEqual(results[0]['cost'],10)
        code=results[1]['code']
        self.assertEqual(sum(code),18)
        self.assertEqual(len(set(code)),4)
        self.assertGreater(code[0],code[-1])
        self.assertEqual(code[-1]%2,0)
        self.assertEqual(results[2]['move'],4)
        self.assertGreater(results[2]['pruned'],0)
        self.assertAlmostEqual(results[3]['probabilities'][2],.85/1.1)
        self.assertAlmostEqual(results[4]['probabilities'][1],.885812588039885)

    def test_circuit_methods(self):
        grid = [0,0,0,2,0,0, 0,1,0,1,0,0, 0,1,0,0,0,0,
                0,0,1,1,0,0, 1,0,0,0,0,1, 0,0,0,1,0,0]
        methods = ['bfs','dfs','astar','ucs']
        results = self.run_requests([dict(op='circuit', grid=grid, method=m) for m in methods])
        self.assertEqual([(len(r['path'])-1,r['cost'],r['nodes']) for r in results],
                         [(10,13,28),(10,10,27),(10,10,22),(10,10,28)])
        for r in results:
            self.assertEqual((r['path'][0],r['path'][-1]),(0,35))
        self.assertFalse(self.run_requests([dict(op='circuit',grid=grid,method='unknown')])[0]['ok'])

    def test_bad_request_does_not_kill_process(self):
        replies=self.run_requests([{'op':'circuit','grid':[]},{'op':'hello'}])
        self.assertFalse(replies[0]['ok'])
        self.assertTrue(replies[1]['ok'])

    def test_absence_of_signal(self):
        p=self.run_requests([{'op':'bayes','signal':False}])[0]['probabilities']
        self.assertAlmostEqual(sum(p),1)
        self.assertEqual(p.index(min(p)),2)

if __name__=='__main__':
    unittest.main()
