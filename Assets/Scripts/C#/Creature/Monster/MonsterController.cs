using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;

public enum MonsterStates
{
    Dead,
    Damaged,
    Attack,
    Chase,
    Wander,
    Idle
}

public enum GOTag
{
    Player,
    Monster,
    Weapon,
    MonsterWeapon
}

public class MonsterController : Creature
{
    [SerializeField]
    private BehaviourTree _tree;
    private MonsterBlackBoard _board;
    private MonsterDetect _detect;

    void Start()
    {
        Init();
    }

    private void Init()
    {
        _stat = new Stat(1, 1, 0.5f, 1, 1, 1);
        MakeBehaviour();
        ChildInit();
        _tree.CheckSeq();
    }

    private void ChildInit()
    {
        //_detect = gameObject.GetComponentInChildren<MonsterDetect>();
        //_detect.Init(_tree, _board);
    }

    private void MakeBehaviour()
    {
        _tree = new BehaviourTree();
        var agent = Util.GetOrAddComponent<NavMeshAgent>(gameObject);
        Animator animator = Util.GetOrAddComponent<Animator>(gameObject);
        _board = new MonsterBlackBoard(transform, animator, agent, _stat);

        agent.speed = _stat.Speed;
        //�������� �Դ� ���
        BehaviourSequence deadSeq = new BehaviourSequence(_tree);
        _tree.AddSeq(deadSeq);
        var deadSeqcts = new CancellationTokenSource();
        var deadNode = new BehaviourNormalSelector(deadSeqcts, deadSeq);
        MonsterDeadLeaf dead = new MonsterDeadLeaf(deadNode, deadSeqcts, _board);
        MonsterDamagedLeaf damage = new MonsterDamagedLeaf(deadNode, deadSeqcts, _board);
        deadSeq.AddSequenceNode(deadNode);
        deadNode.AddNode(dead);
        deadNode.AddNode(damage);

        //�÷��̾� �߰� ��, ���� ���������� ���� ��嵵 ������ ��
        BehaviourSequence chaseSeq = new BehaviourSequence(_tree);
        _tree.AddSeq(chaseSeq);

        var chaseSeqcts = new CancellationTokenSource();
        var chaseNode = new BehaviourNormalSelector(chaseSeqcts, chaseSeq);

        MonsterChaseLeaf chase = new MonsterChaseLeaf(chaseNode, chaseSeqcts, _board);
        chaseNode.AddNode(chase);

        var attackSeqcts = new CancellationTokenSource();
        var attackNode = new BehaviourNormalSelector(attackSeqcts, chaseSeq);
        MonsterAttackLeaf attack = new MonsterAttackLeaf(attackNode, chaseSeqcts, _board);
        attackNode.AddNode(attack);

        chaseSeq.AddSequenceNode(chaseNode);
        chaseSeq.AddSequenceNode(attackNode);

        //��� ������ ������
        BehaviourSequence normalSeq = new BehaviourSequence(_tree);
        _tree.AddSeq(normalSeq);

        var normalSeqcts = new CancellationTokenSource();
        var normalSeqFirstRandSelector = new BehaviourRandomSelector(normalSeqcts, normalSeq);

        MonsterWanderLeaf wander =
            new MonsterWanderLeaf(
            normalSeqFirstRandSelector, normalSeqcts, _board);

        MonsterIdleLeaf idle = new MonsterIdleLeaf(normalSeqFirstRandSelector, normalSeqcts, _board);

        normalSeqFirstRandSelector.AddNode(wander);
        normalSeqFirstRandSelector.AddNode(idle);
        normalSeq.AddSequenceNode(normalSeqFirstRandSelector);
    }

    private void Clear()
    {
        _detect.Clear();
        _tree.Clear();
        _board.Clear();
    }

    public override void OnDamage(int damage)
    {
        _board.Stat.Hp -= damage;
        _tree.CheckSeq();
    }
}

#region MonsterBehaviourBlackBoard

public class MonsterBlackBoard : BlackBoard{
    public Transform Target = null;
    public NavMeshAgent Agent;

    public MonsterBlackBoard(Transform creature, Animator anim, NavMeshAgent agent, Stat stat) : base(creature, anim, stat)
    {
        Agent = agent;
    }

    public override void Clear()
    {
        base.Clear();
        Agent = null;
        Target = null;
    }
}

#endregion