using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MonsterDetect : MonoBehaviour
{
    private BehaviourTree _tree = null;
    private MonsterBlackBoard _board = null;
    private CancellationTokenSource _cts;
    private bool _bCheck;

    public void Init(BehaviourTree tree, MonsterBlackBoard board)
    {
        _tree = tree;
        _board = board;
        _bCheck = false;
        _cts = new CancellationTokenSource();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(GOTag.Player.ToString()))
        {
            _board.Target = other.transform;
            _tree.CheckSeq();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(GOTag.Player.ToString())
            && !_bCheck)
        {
            _bCheck = true;
            MissTarget().Forget();
        }
    }

    private async UniTaskVoid MissTarget()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: _cts.Token);
        
        if (Vector3.Distance(_board.Target.position, transform.position) > 3)
        {
            _board.Target = null;
            _tree.CheckSeq();
        }

        _bCheck = false;
    }

    public void Clear()
    {
        _tree = null;
        _board = null;
        _cts.Cancel();
        _cts.Dispose();
    }
}
