using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class MonsterIdleLeaf : BehaviourLeaf
{
    private System.Random _rand;
    private MonsterBlackBoard _board;

    public MonsterIdleLeaf(BehaviourSequenceNode parent, CancellationTokenSource cts, MonsterBlackBoard board) : base(parent, cts)
    {
        _rand = new System.Random();
        _board = board;
    }

    public override void CancelBehaviour(CancellationTokenSource cts)
    {
        _cts = cts;
    }

    public override SeqStates CheckLeaf()
    {
        Idle().Forget();
        return SeqStates.Running;
    }

    private async UniTaskVoid Idle()
    {
        int duration = _rand.Next(1000, 3000);

        await UniTask.Delay(TimeSpan.FromMilliseconds(duration), cancellationToken: _cts.Token);

        _parent.SeqState = SeqStates.Success;
        _parent.CompleteSeq();
    }

    public override void Clear()
    {
        _rand = null;
        _board = null;
    }
}

public class MonsterWanderLeaf : BehaviourLeaf
{
    private MonsterBlackBoard _board;

    public MonsterWanderLeaf(
        BehaviourSequenceNode parent, CancellationTokenSource cts,
        MonsterBlackBoard board) : base(parent, cts)
    {
        _board = board;
        _animHash = Animator.StringToHash(MonsterStates.Walk.ToString());
    }

    public override void CancelBehaviour(CancellationTokenSource cts)
    {
        _cts = cts;
        _board.Anim.SetBool(_animHash, false);
    }

    public override SeqStates CheckLeaf()
    {
        //조건 확인 후 내 state 정하기
        Wandering().Forget();
        return SeqStates.Running;
    }

    private async UniTaskVoid Wandering()
    {
        Vector3 next = _board.Spawner.GetRandomRoomPos();
        _board.Agent.destination = next;

        _board.CurCreature.rotation = Quaternion.LookRotation(next);
        _board.Anim.SetBool(_animHash, true);
        _board.Agent.speed = _board.Stat.Speed / 2;
        _board.Agent.isStopped = false;

        await UniTask.WaitUntil(() => Vector3.Distance(_board.Agent.destination, _board.CurCreature.position) < 0.5f,
            cancellationToken: _cts.Token);

        _board.Agent.isStopped = true;
        _board.Agent.speed = _board.Stat.Speed;
        _board.Agent.velocity = Vector3.zero;

        _board.Anim.SetBool(_animHash, false);
        
        await UniTask.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: _cts.Token);

        _parent.SeqState = SeqStates.Success;
        _parent.CompleteSeq();
    }

    public override void Clear()
    {
        _board = null;
    }
}

public class MonsterComeBackLeaf : BehaviourLeaf
{
    private MonsterBlackBoard _board;

    public MonsterComeBackLeaf(BehaviourSequenceNode parent, CancellationTokenSource cts,
        MonsterBlackBoard board) : base(parent, cts)
    {
        _board = board;
        _animHash = Animator.StringToHash(MonsterStates.Walk.ToString());
        //걷는 모션
    }

    public override void CancelBehaviour(CancellationTokenSource cts)
    {
        _cts = cts;
        _board.Anim.SetBool(_animHash, false);
    }

    public override SeqStates CheckLeaf()
    {
        if (!_board.Spawner.IsPosInRoom(_board.CurCreature.position))
        {
            ComeBack().Forget();
            return SeqStates.Running;
        }

        return SeqStates.Fail;
    }

    private async UniTaskVoid ComeBack()
    {
        _board.Agent.destination = _board.Spawner.GetRandomRoomPos();
        _board.Agent.isStopped = false;
        _board.Anim.SetBool(_animHash, true);

        await UniTask.WaitUntil(() => Vector3.Distance(_board.Agent.destination, _board.CurCreature.position) < 0.5f,
            cancellationToken: _cts.Token);

        _board.Agent.isStopped = true;
        _board.Agent.velocity = Vector3.zero;
        _board.Anim.SetBool(_animHash, false);
        _parent.CompleteSeq();
    }

    public override void Clear()
    {
        _board = null;
    }
}

public class MonsterChaseLeaf : BehaviourLeaf
{
    private MonsterBlackBoard _board;

    public MonsterChaseLeaf(BehaviourSequenceNode parent, CancellationTokenSource cts,
        MonsterBlackBoard board) : base(parent, cts)
    {
        _board = board;
        _animHash = Animator.StringToHash(MonsterStates.Run.ToString());
    }

    public override void CancelBehaviour(CancellationTokenSource cts)
    {
        _cts = cts;
        _board.Anim.SetBool(_animHash, false);
    }

    public override SeqStates CheckLeaf()
    {
        if (_board.Target != null)
        {
            if (Vector3.Distance(_board.Target.position, _board.CurCreature.position) < _board.Stat.Range)
                return SeqStates.Success;

            Chase().Forget();
            return SeqStates.Running;
        }

        else
            return SeqStates.Fail;
    }

    private async UniTaskVoid Chase()
    {
        Transform target = _board.Target;
        _board.Agent.speed = _board.Stat.Speed * 2;
        _board.Agent.destination = target.position;
        _board.Agent.isStopped = false;
        _board.Anim.SetBool(_animHash, true);

        while (true)
        {
            _board.Agent.destination = target.position;
            _board.CurCreature.rotation = Quaternion.LookRotation(target.position - _board.CurCreature.position);

            await UniTask.Delay(TimeSpan.FromMilliseconds(10), cancellationToken: _cts.Token);

            if (_board.Target == null || Vector3.Distance(_board.Spawner.transform.position, _board.CurCreature.position) < 100) {
                _board.Agent.isStopped = true;
                _parent.SeqState = SeqStates.Fail;
                CancelBehaviour(_cts);
                break;
            }

            else if (Vector3.Distance(_board.Target.position, _board.CurCreature.position) <= _board.Stat.Range)
            {
                _board.Agent.isStopped = true;
                _parent.SeqState = SeqStates.Success;
                _board.Anim.SetBool(_animHash, false);
                break;
            }
        }
        _parent.CompleteSeq();
    }

    public override void Clear()
    {
        _board = null;
    }
}

public class MonsterAttackLeaf : BehaviourLeaf
{
    private MonsterBlackBoard _board;
    private AnimationEvent _evt;
    private float _clipLength;

    public MonsterAttackLeaf(BehaviourSequenceNode parent, CancellationTokenSource cts,
        MonsterBlackBoard board) : base(parent, cts)
    {
        _board = board;
        _animHash = Animator.StringToHash(MonsterStates.Attack.ToString());
        for (int i = 0; i < _board.Anim.runtimeAnimatorController.animationClips.Length; ++i)
            if (_board.Anim.runtimeAnimatorController.animationClips[i].name.CompareTo("Attack_1") == 0)
            {
                _clipLength = _board.Anim.runtimeAnimatorController.animationClips[i].length / 2;
                break;
            }
    }

    public override void CancelBehaviour(CancellationTokenSource cts)
    {
        _cts = cts;
        _board.Anim.SetBool(_animHash, false);
    }

    public override SeqStates CheckLeaf()
    {
        if (_board.Target != null)
        {
            Attack().Forget();
            return SeqStates.Running;
        }

        return SeqStates.Fail;
    }

    private async UniTaskVoid Attack()
    {
        _board.CurCreature.LookAt(_board.Target);
        _board.Anim.SetBool(_animHash, true);
        await UniTask.WhenAll(UniTask.WaitUntil(() => _parent.SeqState == SeqStates.Running, cancellationToken: _cts.Token), 
            UniTask.Delay(TimeSpan.FromSeconds(_clipLength), cancellationToken: _cts.Token));
        _board.Anim.SetBool(_animHash, false);
        await UniTask.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: _cts.Token);
        _parent.SeqState = SeqStates.Success;
        _parent.CompleteSeq();
    }

    public override void Clear()
    {
        _board = null;
    }
}

public class MonsterDamagedLeaf : BehaviourLeaf
{
    private MonsterBlackBoard _board;
    private int _beforeHp;

    public MonsterDamagedLeaf(BehaviourSequenceNode parent, CancellationTokenSource cts,
        MonsterBlackBoard board) : base(parent, cts)
    {
        _board = board;
        _beforeHp = _board.Stat.Hp;
    }

    public override void CancelBehaviour(CancellationTokenSource cts)
    {
        _cts = cts;
        _board.Anim.SetBool(_animHash, false);
    }

    public override SeqStates CheckLeaf()
    {
        if (_beforeHp > _board.Stat.Hp)
        {
            Damaged().Forget();
            return SeqStates.Running;
        }

        return SeqStates.Fail;
    }

    private async UniTaskVoid Damaged()
    {
        _board.CurCreature.LookAt(_board.HitDir);
        await UniTask.WhenAll(UniTask.WaitUntil(() => _parent.SeqState == SeqStates.Running, cancellationToken: _cts.Token),
            UniTask.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: _cts.Token));

        _parent.SeqState = SeqStates.Success;
        _beforeHp = _board.Stat.Hp;
        _parent.CompleteSeq();
    }

    public override void Clear()
    {
        _board = null;
    }
}

public class MonsterDeadLeaf : BehaviourLeaf
{
    private MonsterBlackBoard _board;

    public MonsterDeadLeaf(BehaviourSequenceNode parent, CancellationTokenSource cts,
        MonsterBlackBoard board) : base(parent, cts)
    {
        _board = board;
        _animHash = Animator.StringToHash(MonsterStates.Dead.ToString());
    }

    public override void CancelBehaviour(CancellationTokenSource cts)
    {
        _cts = cts;
        _board.Anim.SetBool(_animHash, false);
    }

    public override SeqStates CheckLeaf()
    {
        if (_board.Stat.Hp < 1)
        {
            _board.Anim.Play(_animHash);
            return SeqStates.Running;
        }

        return SeqStates.Fail;
    }

    public override void Clear()
    {
        _board = null;
    }
}