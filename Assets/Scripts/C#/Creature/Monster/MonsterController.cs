using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UniRx.Triggers;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;

public enum MonsterStates
{
    Dead,
    Attack,
    Run,
    Walk,
    Idle
}

public enum GOTag
{
    Player,
    Monster,
    Weapon,
    Item
}

public class MonsterController : NetworkBehaviour, IAttackable
{
    [SerializeField]
    private BehaviourTree _tree;
    [SerializeField]
    private MonsterBlackBoard _board;
    private MonsterDetect _detect;
    private MonsterAttack _attack;
    private NetworkVariable<Stat> _stat = new NetworkVariable<Stat>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _stat.Value = new Stat(1, 1, 5, 1, 1, 5);
        }
        _tree.CheckSeq();
    }

    public void Init(MonsterSpawner spawner)
    {
        _attack = gameObject.GetComponentInChildren<MonsterAttack>();
        _detect = gameObject.GetComponentInChildren<MonsterDetect>();
        MakeBehaviour(spawner);
        ChildInit();
    }

    private void ChildInit()
    {
        _detect.Init(_tree, _board);
        _attack.Init(_stat.Value);
    }

    private void MakeBehaviour(MonsterSpawner spawner)
    {
        _tree = new BehaviourTree();
        var agent = Util.GetOrAddComponent<NavMeshAgent>(gameObject);
        Animator animator = Util.GetOrAddComponent<Animator>(gameObject);
        _board = new MonsterBlackBoard(transform, animator, agent, _stat.Value, spawner);

        agent.speed = _stat.Value.Speed;
        
        //데미지를 입고 죽을 경우
        BehaviourSequence deadSeq = new BehaviourSequence(_tree);
        _tree.AddSeq(deadSeq);
        var deadSeqcts = new CancellationTokenSource();
        var deadNode = new BehaviourNormalSelector(deadSeqcts, deadSeq);
        MonsterDeadLeaf dead = new MonsterDeadLeaf(deadNode, deadSeqcts, _board);
        deadSeq.AddSequenceNode(deadNode);
        deadNode.AddNode(dead);

        //플레이어가 시야에 있다면 쫒고 이후 범위 안에 들어오면 공격까지
        BehaviourSequence chaseSeq = new BehaviourSequence(_tree);
        _tree.AddSeq(chaseSeq);

        var chaseSeqcts = new CancellationTokenSource();
        var chaseNode = new BehaviourNormalSelector(chaseSeqcts, chaseSeq);

        MonsterChaseLeaf chase = new MonsterChaseLeaf(chaseNode, chaseSeqcts, _board);
        chaseNode.AddNode(chase);

        var attackSeqcts = new CancellationTokenSource();
        var attackNode = new BehaviourNormalSelector(attackSeqcts, chaseSeq);
        MonsterAttackLeaf attack = new MonsterAttackLeaf(attackNode, chaseSeqcts, _board, _attack.GetComponent<BoxCollider>());
        attackNode.AddNode(attack);

        chaseSeq.AddSequenceNode(chaseNode);
        chaseSeq.AddSequenceNode(attackNode);

        //플레이어를 놓치면 다시 스포너 근처로 돌아가기
        BehaviourSequence comeBackSeq = new BehaviourSequence(_tree);
        _tree.AddSeq(comeBackSeq);

        var comeBackSeqcts = new CancellationTokenSource();
        var comeBackSeqNormalSelector = new BehaviourNormalSelector(comeBackSeqcts, comeBackSeq);

        MonsterComeBackLeaf comeBack =
            new MonsterComeBackLeaf(
            comeBackSeqNormalSelector, comeBackSeqcts, _board);

        comeBackSeqNormalSelector.AddNode(comeBack);
        comeBackSeq.AddSequenceNode(comeBackSeqNormalSelector);

        //피격 시 그 방향을 돌아본다
        BehaviourSequence LookSeq = new BehaviourSequence(_tree);
        _tree.AddSeq(deadSeq);
        var LookSeqcts = new CancellationTokenSource();
        var LookNode = new BehaviourNormalSelector(LookSeqcts, LookSeq);

        MonsterDamagedLeaf damage = new MonsterDamagedLeaf(LookNode, LookSeqcts, _board);
        LookNode.AddNode(damage);

        //기본 상태. 방을 배회하거나 가만히 있음
        BehaviourSequence normalSeq = new BehaviourSequence(_tree);
        _tree.AddSeq(normalSeq);

        var normalSeqcts = new CancellationTokenSource();
        var normalSeqFirstRandSelector = new BehaviourRandomSelector(normalSeqcts, normalSeq);

        MonsterWanderLeaf wander =
            new MonsterWanderLeaf(
            normalSeqFirstRandSelector, normalSeqcts, _board);

        MonsterIdleLeaf idle = new MonsterIdleLeaf(normalSeqFirstRandSelector, normalSeqcts, _board);
        //MonsterPauseLeaf pause = new MonsterPauseLeaf(normalSeqFirstRandSelector, normalSeqcts, _board);

        normalSeqFirstRandSelector.AddNode(wander);
        normalSeqFirstRandSelector.AddNode(idle);
        //normalSeqFirstRandSelector.AddNode(pause);
        normalSeq.AddSequenceNode(normalSeqFirstRandSelector);
    }

    private void Clear()
    {
        _detect.Clear();
        _tree.Clear();
        _board.Clear();
    }

    public void OnDamaged(int damage)
    {
        _board.Stat.Hp -= damage;
        _tree.CheckSeq();
    }

    public bool OnHealed(int heal)
    {
        if (_board.Stat.Hp == _board.Stat.MaxHp)
        {
            return false;
        }

        _board.Stat.Hp = _board.Stat.Hp + heal < _board.Stat.MaxHp ? _board.Stat.Hp + heal : _board.Stat.MaxHp;
        return true;
    }

    public override void OnNetworkDespawn()
    {
        Clear();
    }
}

#region MonsterBehaviourBlackBoard
[Serializable]
public class MonsterBlackBoard : BlackBoard{
    public Transform Target = null;
    [field:SerializeField]
    public MonsterSpawner Spawner { get; private set; } = null;
    public NavMeshAgent Agent { get; private set; } = null;
    public Vector3 HitDir;

    public MonsterBlackBoard(Transform creature, Animator anim, NavMeshAgent agent, Stat stat, MonsterSpawner spawner) : base(creature, anim, stat)
    {
        Agent = agent;
        Spawner = spawner;
        //SpawnerPos = Spawner.transform.position;
    }

    public override void Clear()
    {
        base.Clear();
        Agent = null;
        Target = null;
        Spawner = null;
    }
}

#endregion