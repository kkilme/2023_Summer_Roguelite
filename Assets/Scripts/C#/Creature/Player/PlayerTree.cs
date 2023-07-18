using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerIdleLeaf : BehaviourLeaf
{
    private PlayerBlackBoard _board;

    public PlayerIdleLeaf(BehaviourSequenceNode parent, CancellationTokenSource cts, PlayerBlackBoard board) : base(parent, cts)
    {
        _board = board;
    }

    public override void CancelBehaviour(CancellationTokenSource cts)
    {
        _cts = cts;
    }

    public override SeqStates CheckLeaf()
    {
        return SeqStates.Success;
    }

    public override void Clear()
    {
        _board = null;
    }
}

public class PlayerMoveLeaf : BehaviourLeaf
{
    private PlayerBlackBoard _board;

    public PlayerMoveLeaf(BehaviourSequenceNode parent, CancellationTokenSource cts, PlayerBlackBoard board) : base(parent, cts)
    {
        _board = board;
        _animHash = Animator.StringToHash(AnimParam.Move.ToString());
    }

    public override void CancelBehaviour(CancellationTokenSource cts)
    {
        _board.Anim.SetBool(_animHash, false);
        _cts = cts;
    }

    public override SeqStates CheckLeaf()
    {
        if (_board.State == States.Move)
        {
            Move().Forget();
            return SeqStates.Running;
        }

        return SeqStates.Fail;
    }

    private async UniTaskVoid Move()
    {
        _board.Anim.SetBool(_animHash, true);

        while (_board.State == States.Move)
        {
            _board.CurCreature.position += _board.MoveDir * _board.Stat.Speed * Time.deltaTime;
            _board.CurCreature.rotation = Quaternion.LookRotation(_board.MoveDir);
            await UniTask.DelayFrame(1, cancellationToken: _cts.Token);
        }

        _board.Anim.SetBool(_animHash, false);
        _parent.SeqState = SeqStates.Success;
    }

    public override void Clear()
    {
        _board = null;
    }
}

public class PlayerDashLeaf : BehaviourLeaf
{
    private PlayerBlackBoard _board;
    private short _duration = 0;
    private short _dashcnt = 0;
    private short _maxdashcnt = 2;

    public PlayerDashLeaf(BehaviourSequenceNode parent, CancellationTokenSource cts, PlayerBlackBoard board) : base(parent, cts)
    {
        _board = board;
        _animHash = Animator.StringToHash(AnimParam.Dash.ToString());
        SetDashCool().Forget();
        _duration = 10;
    }

    public override void CancelBehaviour(CancellationTokenSource cts)
    {
        _board.Anim.SetBool(_animHash, false);
        _cts = cts;
    }

    public override SeqStates CheckLeaf()
    {
        if (_board.State == States.Dash)
        {
            Dash().Forget();
            return SeqStates.Running;
        }

        return SeqStates.Fail;
    }

    private async UniTaskVoid Dash()
    {
        --_dashcnt;
        _board.CurCreature.rotation = Quaternion.LookRotation(_board.MoveDir);
        Vector3 dashDistance = _board.MoveDir / _duration;
        //애니메이션 on
        while (_duration > 0)
        {
            await UniTask.Delay(TimeSpan.FromMilliseconds(10));
            --_duration;
            _board.CurCreature.Translate(dashDistance, Space.World);
        }
        //애니메이션 off
        _duration = 10;
        _parent.SeqState = SeqStates.Success;
    }

    private async UniTaskVoid SetDashCool()
    {
        while (true)
        {
            await UniTask.WaitUntil(() => _dashcnt < _maxdashcnt, cancellationToken: _cts.Token);
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: _cts.Token);
            ++_dashcnt;
        }
    }

    public override void Clear()
    {
        _board = null;
    }
}

public class PlayerAttackLeaf : BehaviourLeaf
{
    private PlayerBlackBoard _board;

    public PlayerAttackLeaf(BehaviourSequenceNode parent, CancellationTokenSource cts, PlayerBlackBoard board) : base(parent, cts)
    {
        _board = board;
        _animHash = Animator.StringToHash(AnimParam.Dash.ToString());
    }

    public override void CancelBehaviour(CancellationTokenSource cts)
    {
        _board.Anim.SetBool(_animHash, false);
        _cts = cts;
    }

    public override SeqStates CheckLeaf()
    {
        if (_board.State == States.Attack)
        {
            Attack().Forget();
            return SeqStates.Running;
        }

        return SeqStates.Fail;
    }

    private async UniTaskVoid Attack()
    {
        while (_board.State == States.Attack)
        {
            Vector2 pos = Mouse.current.position.ReadValue()
                - new Vector2(Screen.width >> 1, Screen.height >> 1);
            Vector3 lookDir = new Vector3(pos.x, 0, pos.y).normalized;
            _board.CurCreature.rotation = Quaternion.LookRotation(lookDir);
            await UniTask.DelayFrame(1, cancellationToken: _cts.Token);
            //발사
            _board.BulletCount -= _board.BulletCount > 0 ? 1 : 0;
        }

        _parent.SeqState = SeqStates.Success;
    }

    public override void Clear()
    {
        _board = null;
    }
}

public class PlayerDamagedLeaf : BehaviourLeaf
{
    private PlayerBlackBoard _board;
    private int _beforeHp;

    public PlayerDamagedLeaf(BehaviourSequenceNode parent, CancellationTokenSource cts, PlayerBlackBoard board) : base(parent, cts)
    {
        _board = board;
        _animHash = Animator.StringToHash(AnimParam.Dash.ToString());
        _beforeHp = board.Stat.Hp;
    }

    public override void CancelBehaviour(CancellationTokenSource cts)
    {
        _board.Anim.SetBool(_animHash, false);
        _cts = cts;
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
        //애니메이션
        await UniTask.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: _cts.Token);
        _parent.SeqState = SeqStates.Success;
        _beforeHp = _board.Stat.Hp;
    }

    public override void Clear()
    {
        _board = null;
    }
}

public class PlayerDeadLeaf : BehaviourLeaf
{
    private PlayerBlackBoard _board;

    public PlayerDeadLeaf(BehaviourSequenceNode parent, CancellationTokenSource cts,
        PlayerBlackBoard board) : base(parent, cts)
    {
        _board = board;
        _animHash = Animator.StringToHash(States.Dead.ToString());
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