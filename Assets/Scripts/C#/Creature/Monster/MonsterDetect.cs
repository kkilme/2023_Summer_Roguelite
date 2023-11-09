using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class MonsterDetect : MonoBehaviour
{
    private List<Transform> _playerT = new List<Transform>(10);
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
            RaycastHit hit;

            if (Physics.Raycast(transform.position, other.transform.position - transform.position, out hit))
            {
                if (hit.transform == other.transform)
                {
                    _playerT.Add(other.transform);

                    if (_board.Target == null)
                        _board.Target = other.transform;

                    else if (Vector3.Distance(_board.Target.position, transform.position) > Vector3.Distance(transform.position, other.transform.position))
                        _board.Target = other.transform;

                    _tree.CheckSeq();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (NetworkManager.Singleton.IsServer && other.CompareTag(GOTag.Player.ToString()))
        {
            _playerT.Remove(other.transform);

            if (other.transform == _board.Target)
            {
                if (_playerT.Count == 0 && !_bCheck)
                {
                    _bCheck = true;
                    MissTarget(other.transform).Forget();
                }

                else if (_playerT.Count != 0)
                    DecideTarget();
            }
        }
    }

    private void DecideTarget()
    {
        Transform temp = _playerT[0];
        float tempdis = Vector3.Distance(transform.position, temp.position);
        float comparedis;

        for (int i = 1; i < _playerT.Count; ++i)
        {
            comparedis = Vector3.Distance(transform.position, _playerT[i].position);
            if (comparedis < tempdis)
            {
                tempdis = comparedis;
                temp = _playerT[i];
            }
        }

        _board.Target = temp;
    }

    private async UniTaskVoid MissTarget(Transform target)
    {
        await UniTask.WhenAny(UniTask.WaitUntil(() => _playerT.Count != 0, cancellationToken: _cts.Token) , UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: _cts.Token));

        if (_playerT.Count == 0)
        {
            _board.Target = null;
            _tree.CheckSeq();
        }

        else if (_board.Target == target)
            DecideTarget();

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
