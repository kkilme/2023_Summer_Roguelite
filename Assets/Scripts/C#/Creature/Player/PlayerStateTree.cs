using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public enum States
{
    None = 0,
    Reset = 1,
    Dead = 4,
    Damaged = 16,
    Reload = 64,
    Attack = 65,
    Dash = 66,
    Move = 256,
    Idle = 257
}

//public class PlayerStateTree : MonoBehaviour
//{
//    private PlayerStateNode _root;
//    private PlayerStateNode _curNode;
//    private States _curState;
//
//    private void Start()
//    {
//        Init();
//    }
//
//    public void Init()
//    {
//        _root = new ResetNode(States.Reset, null, this);
//        States[] states = Enum.GetValues(typeof(States)) as States[];
//        for (int i = 2; i < states.Length; ++i)
//            _root.CreateChildNode(states[i]);
//        _curState = States.Idle;
//        _curNode = _root.ChangeNode(States.Idle);
//    }
//
//    public bool SetNode(States state)
//    {
//        if ((short)_curState << 2 < (short)state)
//            return false;
//
//        if (_curState != state)
//        {
//            _curNode.Exit();
//            _curNode = _root.ChangeNode(state);
//            _curState = _curNode.Enter();
//        }
//
//        return true;
//    }
//}
/*
public abstract class PlayerStateNode
{
    protected readonly States _state;
    protected List<PlayerStateNode> _childNodes;
    protected Animator _anim;
    protected PlayerController _pc;
    protected Transform _playerT;

    public PlayerStateNode(States state, Animator anim, 
        PlayerController pc, Transform playerT)
    {
        _state = state;
        _childNodes = new List<PlayerStateNode>();
        _anim = anim;
        _pc = pc;
        _playerT = playerT;
    }

    public void CreateChildNode(States state)
    {
        if ((short)state >= (short)(_state) << 2
            && (short)state < (short)(_state) << 3)
        {
            PlayerStateNode node;
            switch (state)
            {
                case States.Dead:
                    node = new DeadNode(States.Dead, _anim, _pc, _playerT);
                    break;
                
                case States.ChangeToBefore:
                    node = new ChangeToBeforeNode(States.ChangeToBefore, _anim, _pc, _playerT);
                    break;
                
                case States.Damaged:
                    node = new DamagedNode(States.Damaged, _anim, _pc, _playerT);
                    break;
                
                case States.Attack:
                    node = new AttackNode(States.Attack, _anim, _pc, _playerT);
                    break;

                case States.Dash:
                    node = new DashNode(States.Dash, _anim, _pc, _playerT);
                    break;

                case States.Move:
                    node = new MoveNode(States.Move, _anim, _pc, _playerT);
                    break;

                case States.Idle:
                    node = new IdleNode(States.Idle, _anim, _pc, _playerT);
                    break;

                default:
                    node = new ResetNode(state, _anim, _pc, _playerT);
                    break;
            }
            _childNodes.Add(node);
        }
        else
            _childNodes[0].CreateChildNode(state);
    }

    public PlayerStateNode ChangeNode(States state)
    {
        if (_state == state)
            return this;

        else if ((short)state >= (short)_state << 4)
            return _childNodes[0].ChangeNode(state);

        else 
            return _childNodes[(short)state % 4].ChangeNode(state);
    }

    public abstract States Enter();
    public abstract void Exit();
}

public class DeadNode : PlayerStateNode
{
    public DeadNode(States state, 
        Animator anim, PlayerController pc, Transform playerT) 
        : base(state, anim, pc, playerT) { }

    public override States Enter()
    {
        return _state;
    }

    public override void Exit() { }
}

public class ResetNode : PlayerStateNode
{
    public ResetNode(States state, 
        Animator anim, PlayerController pc, Transform playerT) 
        : base(state, anim, pc, playerT) { }

    public override States Enter()
    {
        return _state;
    }

    public override void Exit() { }
}

public class ChangeToBeforeNode : PlayerStateNode
{
    public ChangeToBeforeNode(States state, 
        Animator anim, PlayerController pc, Transform playerT) 
        : base(state, anim, pc, playerT) { }

    public override States Enter()
    {
        if (_pc.BeforeState == States.Move)
            _anim.SetBool(_pc.AnimParams[(int)AnimParam.bMove], true);
        return _pc.BeforeState;
    }

    public override void Exit() { }
}

public class DamagedNode : PlayerStateNode
{
    private short _duration = 0;

    public DamagedNode(States state, 
        Animator anim, PlayerController pc, Transform playerT) 
        : base(state, anim, pc, playerT) { }

    private async UniTaskVoid Stiffen()
    {
        while (_duration < 5)
        {
            await UniTask.Delay(TimeSpan.FromMilliseconds(100));
            ++_duration;
        }
        _pc.SetNode(States.ChangeToBefore);
    }

    public override States Enter()
    {
        _duration = 0;
        //_anim.SetTrigger(_pc.AnimParams[(int)AnimParam.tDamaged]);
        Stiffen().Forget();

        return _state;
    }

    public override void Exit()
    {
        _duration = 0;
    }
}

public class AttackNode : PlayerStateNode
{
    public AttackNode(States state, Animator anim, 
        PlayerController pc, Transform playerT) 
        : base(state, anim, pc, playerT) { }

    public override States Enter()
    {
        _anim.SetBool(_pc.AnimParams[(int)AnimParam.bShoot], true);
        return _state;
    }

    public override void Exit() 
    {
        _anim.SetBool(_pc.AnimParams[(int)AnimParam.bShoot], false);
    }
}

public class DashNode : PlayerStateNode
{
    private short _duration = 0;
    private short _dashcnt = 0;
    private short _maxdashcnt = 2;
    private CancellationTokenSource _cts = null;

    public DashNode(States state, Animator anim, 
        PlayerController pc, Transform playerT) 
        : base(state, anim, pc, playerT) 
    {
        _cts = new CancellationTokenSource();
        SetDashCool().Forget();
    }

    public override States Enter()
    {
        if (_dashcnt != 0)
        {
            //_anim.SetTrigger(_pc.AnimParams[(int)AnimParam.tDash]);
            _duration = 10;
            Dash().Forget();

            return _state;
        }
        else
            return _pc.BeforeState;
    }

    public override void Exit() 
    {
        _duration = 10;
    }

    private async UniTaskVoid Dash()
    {
        --_dashcnt;
        _playerT.rotation = Quaternion.LookRotation(_pc.Movedir);
        Vector3 dashDistance = _pc.Movedir / _duration;
        while (_duration > 0)
        {
            await UniTask.Delay(TimeSpan.FromMilliseconds(10));
            --_duration;
            _playerT.Translate(dashDistance, Space.World);
        }
        _pc.SetNode(States.ChangeToBefore);
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
}

public class MoveNode : PlayerStateNode
{
    public MoveNode(States state, Animator anim, 
        PlayerController pc, Transform playerT) 
        : base(state, anim, pc, playerT) 
    { }

    public override States Enter()
    {
        _anim.SetBool(_pc.AnimParams[(int)AnimParam.bMove], true);
        return _state;
    }

    public override void Exit()
    {
        _anim.SetBool(_pc.AnimParams[(int)AnimParam.bMove], false);
    }
}

public class IdleNode : PlayerStateNode
{
    public IdleNode(States state, Animator anim, 
        PlayerController pc, Transform playerT) 
        : base(state, anim, pc, playerT) { }

    public override States Enter()
    {
        return _state;
    }

    public override void Exit() { }
}
*/
/*
public class PlayerStateController
{
    private States _current;
    private Action _toIdle;

    private PlayerState _ps;
    private Idle _idle;
    private Move _move;
    private Act _act;
    private Damaged _damage;
    private Dead _dead;

    public PlayerStateController()
    {
        _current = States.Idle;
        _toIdle -= OnIdle;
        _toIdle += OnIdle;

        _idle = new Idle();
        _move = new Move();
        _act = new Act(ref _toIdle);
        _damage = new Damaged(ref _toIdle);
        _dead = new Dead();
        _ps = _idle;
    }

    public bool ChangeStates(States newStates)
    {
        if (newStates < _current)
            return false;

        _current = newStates;
        switch (newStates)
        {
            case States.Idle:
                _ps = _idle;
                break;
            case States.Move:
                _ps = _move;
                break;
            case States.Attack:
                _ps = _act;
                break;
            case States.Damaged:
                _ps = _damage;
                break;
            case States.Dead:
                _ps = _dead;
                break;
        }
        _ps.Enter();
        return true;
    }

    public void Update()
    {
        _ps.Update();
    }

    private void OnIdle()
    {
        _current = States.Idle;
        ChangeStates(States.Idle);
    }

    public void Clear()
    {
        _toIdle -= OnIdle;
        _idle = null;
        _move = null;
        _act = null;
        _damage = null;
        _dead = null;
    }
}

public abstract class PlayerState
{
    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update();
    protected Action ToIdle;
}

public class Idle : PlayerState
{
    public override void Enter()
    {

    }

    public override void Exit()
    {

    }

    public override void Update()
    {
    }
}

public class Move : PlayerState
{
    public override void Enter()
    {

    }

    public override void Exit()
    {

    }

    public override void Update()
    {
    }
}

public class Act : PlayerState
{
    public Act(ref Action act)
    {
        ToIdle = act;
    }

    public override void Enter()
    {

    }

    public override void Exit()
    {

    }

    public override void Update()
    {
    }
}

public class Damaged : PlayerState
{
    private short _duration = 0;
    private bool _bExecuted = false;

    public Damaged(ref Action act)
    {
        ToIdle = act;
    }

    public override void Enter()
    {
        _duration = 0;
        //비동기는 하나만 실행되도록, 시간은 초기화되도록
        if (!_bExecuted)
        {
            _bExecuted = true;
            Stiffen().Forget();
        }
    }

    public override void Exit()
    {
        _bExecuted = false;
        _duration = 0;
    }

    public override void Update()
    {
    }

    private async UniTaskVoid Stiffen()
    {
        while (_duration < 5)
        {
            await UniTask.Delay(TimeSpan.FromMilliseconds(50));
            ++_duration;
        }
        Exit();
        ToIdle.Invoke();
    }
}

public class Dead : PlayerState
{
    public override void Enter()
    {
    }

    public override void Exit()
    {

    }

    public override void Update()
    {
    }
}
*/