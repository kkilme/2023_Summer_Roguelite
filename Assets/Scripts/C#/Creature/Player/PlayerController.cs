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

public class PlayerController : MonoBehaviour, IAttackable
{
    [SerializeField]
    private Animator _anim;
    [SerializeField]
    private PlayerInput _pi;
    [SerializeField]
    private InputActionReference _action;
    [SerializeField]
    private Collider _collider;

    private Vector3 _moveDir;
    IDisposable _moveChar;
    IDisposable _RotationChar;

    private Stat _stat;

    [SerializeField]
    private Transform[] _sightTransforms;

    void Start()
    {
        Init();
    }

    private void Init()
    {
        _stat = new Stat(1, 1, 2, 1, 1, 5);
        _moveDir = Vector3.zero;
        
        _pi = Util.GetOrAddComponent<PlayerInput>(gameObject);
        _anim = Util.GetOrAddComponent<Animator>(gameObject);
        _collider = Util.GetOrAddComponent<Collider>(gameObject);

        _pi.actions.FindAction("Move").performed += Move;
        _pi.actions.FindAction("Move").canceled += Idle;
        _pi.actions.FindAction("Attack").performed += Attack;
        _pi.actions.FindAction("Attack").canceled += Idle;

        transform.LookAt(Vector3.forward);
    }

#region Behaviour_tree
    //private void MakeBehaviour()
    //{
    //    _board = new PlayerBlackBoard(transform, _anim, _stat);
    //    _tree = new BehaviourTree();
    //
    //    var deadSeq = new BehaviourSequence(_tree);
    //    _tree.AddSeq(deadSeq);
    //
    //    var deadcts = new CancellationTokenSource();
    //    var deadNode = new BehaviourNormalSelector(deadcts, deadSeq);
    //    deadSeq.AddSequenceNode(deadNode);
    //
    //    PlayerDeadLeaf dead = new PlayerDeadLeaf(deadNode, deadcts, _board);
    //    PlayerDamagedLeaf damaged = new PlayerDamagedLeaf(deadNode, deadcts, _board);
    //    deadNode.AddNode(dead);
    //    deadNode.AddNode(damaged);
    //
    //    var attackSeq = new BehaviourSequence(_tree);
    //    _tree.AddSeq(attackSeq);
    //
    //    var attackcts = new CancellationTokenSource();
    //    var attackNode = new BehaviourNormalSelector(attackcts, attackSeq);
    //    attackSeq.AddSequenceNode(attackNode);
    //
    //    PlayerAttackLeaf attack = new PlayerAttackLeaf(attackNode, attackcts, _board);
    //    PlayerDashLeaf dash = new PlayerDashLeaf(attackNode, attackcts, _board);
    //    attackNode.AddNode(attack);
    //    attackNode.AddNode(dash);
    //
    //    var moveSeq = new BehaviourSequence(_tree);
    //    _tree.AddSeq(moveSeq);
    //
    //    var movects = new CancellationTokenSource();
    //    var moveNode = new BehaviourNormalSelector(movects, moveSeq);
    //    moveSeq.AddSequenceNode(moveNode);
    //
    //    PlayerMoveLeaf move = new PlayerMoveLeaf(moveNode, movects, _board);
    //    PlayerIdleLeaf idle = new PlayerIdleLeaf(moveNode, movects, _board);
    //    moveNode.AddNode(move);
    //    moveNode.AddNode(idle);
    //}
#endregion

    private void Move(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        _moveDir = new Vector3(input.x, 0, input.y);
    }

    private void Idle(InputAction.CallbackContext ctx)
    {
        _moveDir = Vector3.zero;
        //Idle 애니메이션
    }

    private void Attack(InputAction.CallbackContext ctx)
    {
        //Attack 애니메이션, 무기 공격
    }

    private void Clear()
    {
        _moveChar.Dispose();
        _RotationChar.Dispose();
    }

    public void OnDamage(int damage)
    {
        _stat.Hp -= damage;
    }

    public void OnDamaged(int damage)
    {
        _stat.Hp -= damage;
    }

    public void OnHealed(int heal)
    {
        _stat.Hp = _stat.Hp + heal < _stat.MaxHp ? _stat.Hp + heal : _stat.MaxHp;
    }

    private void FixedUpdate()
    {
        transform.position += _moveDir * Time.deltaTime * _stat.Speed;
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