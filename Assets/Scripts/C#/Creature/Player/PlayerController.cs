using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum AnimParam
{
    Move,
    Shoot,
    Damaged,
    Dash,
    Dead
}

public class PlayerController : Creature
{
    [SerializeField]
    private Animator _anim;
    [SerializeField]
    private PlayerInput _pi;
    [SerializeField]
    private InputActionReference _action;
    [SerializeField]
    private Collider _collider;

    [SerializeField]
    private Gun _weapon;

    private PlayerBlackBoard _board;
    private BehaviourTree _tree;

    private Vector3 _beforemovedir;
    IDisposable _moveChar;
    IDisposable _RotationChar;
    private double _moveduration;

    void Start()
    {
        Init();
    }

    private void Init()
    {
        _stat = new Stat(1, 1, 2, 1, 1, 5);

        _anim = Util.GetOrAddComponent<Animator>(gameObject);
        _collider = Util.GetOrAddComponent<Collider>(gameObject);

        //_action.action.performed += Move;
        //_action.action.canceled += Idle;
        //_pi.actions.FindAction("Dash").performed += Dash;
        _pi.actions.FindAction("Attack").started += _ => _weapon.StartShoot();
        _pi.actions.FindAction("Attack").canceled += _ => _weapon.StopShoot();
        _pi.actions.FindAction("Reload").performed += _ => _weapon.StartReload();
        
        //_pi.actions.FindAction("Attack").canceled += Idle;

        transform.LookAt(Vector3.forward);
        MakeBehaviour();
    }

    private void MakeBehaviour()
    {
        _board = new PlayerBlackBoard(transform, _anim, _stat);
        _tree = new BehaviourTree();

        var deadSeq = new BehaviourSequence(_tree);
        _tree.AddSeq(deadSeq);

        var deadcts = new CancellationTokenSource();
        var deadNode = new BehaviourNormalSelector(deadcts, deadSeq);
        deadSeq.AddSequenceNode(deadNode);

        PlayerDeadLeaf dead = new PlayerDeadLeaf(deadNode, deadcts, _board);
        PlayerDamagedLeaf damaged = new PlayerDamagedLeaf(deadNode, deadcts, _board);
        deadNode.AddNode(dead);
        deadNode.AddNode(damaged);

        var attackSeq = new BehaviourSequence(_tree);
        _tree.AddSeq(attackSeq);

        var attackcts = new CancellationTokenSource();
        var attackNode = new BehaviourNormalSelector(attackcts, attackSeq);
        attackSeq.AddSequenceNode(attackNode);

        PlayerAttackLeaf attack = new PlayerAttackLeaf(attackNode, attackcts, _board);
        PlayerDashLeaf dash = new PlayerDashLeaf(attackNode, attackcts, _board);
        attackNode.AddNode(attack);
        attackNode.AddNode(dash);

        var moveSeq = new BehaviourSequence(_tree);
        _tree.AddSeq(moveSeq);

        var movects = new CancellationTokenSource();
        var moveNode = new BehaviourNormalSelector(movects, moveSeq);
        moveSeq.AddSequenceNode(moveNode);

        PlayerMoveLeaf move = new PlayerMoveLeaf(moveNode, movects, _board);
        PlayerIdleLeaf idle = new PlayerIdleLeaf(moveNode, movects, _board);
        moveNode.AddNode(move);
        moveNode.AddNode(idle);
    }

    private void Move(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        _beforemovedir = _board.MoveDir;
        _board.MoveDir = new Vector3(input.x, 0, input.y);
        _moveduration = Time.realtimeSinceStartup;
        _board.State = States.Move;
        _tree.CheckSeq();
    }

    private void Idle(InputAction.CallbackContext ctx)
    {
        if (ctx.startTime - _moveduration < 0.025f)
                transform.rotation = Quaternion.LookRotation(_beforemovedir);
        _board.State = States.Idle;
        _tree.CheckSeq();
    }

    private void Dash(InputAction.CallbackContext ctx)
    {
        _board.State = States.Dash;
        _tree.CheckSeq();
    }

    private void Attack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;
        
        _board.State = States.Attack;
        _weapon?.Shoot();
        _tree.CheckSeq();
    }

    private void Reload(InputAction.CallbackContext ctx)
    {
        _board.State = States.Reload;
        _weapon?.StartReload();
        _tree.CheckSeq();
    }

    private void Clear()
    {
        _moveChar.Dispose();
        _RotationChar.Dispose();
    }

    public override void OnDamage(int damage)
    {
        _stat.Hp -= damage;
        _board.State = States.Damaged;
        _tree.CheckSeq();
    }
}

#region PlayerBlackBoard
public class PlayerBlackBoard : BlackBoard
{
    public States State;
    public Vector3 MoveDir;
    public int MaxbulletCount;
    public int BulletCount;

    public PlayerBlackBoard(Transform creature, Animator anim, Stat stat) : base(creature, anim, stat)
    {
        MaxbulletCount = 12;
        BulletCount = 12;
    }

    public override void Clear()
    {
        base.Clear();
    }
}

#endregion