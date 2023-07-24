using Cysharp.Threading.Tasks;
using System;
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

public class MonsterController : MonoBehaviour, IAttackable
{
    [SerializeField]
    private BehaviourTree _tree;
    [SerializeField]
    private MonsterBlackBoard _board;
    private MonsterDetect _detect;
    private Stat _stat;

    public void Init(MonsterSpawner spawner)
    {
        _stat = new Stat(1, 1, 5, 1, 1, 1);
        MakeBehaviour(spawner);
        ChildInit();
        _tree.CheckSeq();
    }

    private void ChildInit()
    {
        //_detect = gameObject.GetComponentInChildren<MonsterDetect>();
        //_detect.Init(_tree, _board);
    }

    private void MakeBehaviour(MonsterSpawner spawner)
    {
        _tree = new BehaviourTree();
        var agent = Util.GetOrAddComponent<NavMeshAgent>(gameObject);
        Animator animator = Util.GetOrAddComponent<Animator>(gameObject);
        _board = new MonsterBlackBoard(transform, animator, agent, _stat, spawner);

        agent.speed = _stat.Speed;
        
        //�������� �Դ� ���
        BehaviourSequence deadSeq = new BehaviourSequence(_tree);
        _tree.AddSeq(deadSeq);
        var deadSeqcts = new CancellationTokenSource();
        var deadNode = new BehaviourNormalSelector(deadSeqcts, deadSeq);
        MonsterDeadLeaf dead = new MonsterDeadLeaf(deadNode, deadSeqcts, _board);
        deadSeq.AddSequenceNode(deadNode);
        deadNode.AddNode(dead);

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

        //������ �ǵ��ƿ��� seq
        BehaviourSequence comeBackSeq = new BehaviourSequence(_tree);
        _tree.AddSeq(comeBackSeq);

        var comeBackSeqcts = new CancellationTokenSource();
        var comeBackSeqNormalSelector = new BehaviourNormalSelector(comeBackSeqcts, comeBackSeq);

        MonsterComeBackLeaf comeBack =
            new MonsterComeBackLeaf(
            comeBackSeqNormalSelector, comeBackSeqcts, _board);

        comeBackSeqNormalSelector.AddNode(comeBack);
        comeBackSeq.AddSequenceNode(comeBackSeqNormalSelector);

        //������ �ش� ������ �ٶ󺻴�
        BehaviourSequence LookSeq = new BehaviourSequence(_tree);
        _tree.AddSeq(deadSeq);
        var LookSeqcts = new CancellationTokenSource();
        var LookNode = new BehaviourNormalSelector(LookSeqcts, LookSeq);

        MonsterDamagedLeaf damage = new MonsterDamagedLeaf(LookNode, LookSeqcts, _board);
        LookNode.AddNode(damage);

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

    public void OnDamaged(int damage)
    {
        _board.Stat.Hp -= damage;
        _tree.CheckSeq();
    }

    public void OnHealed(int heal)
    {
        _board.Stat.Hp = _board.Stat.Hp + heal < _board.Stat.MaxHp ? _board.Stat.Hp + heal : _board.Stat.MaxHp;
    }
}

#region MonsterBehaviourBlackBoard
[Serializable]
public class MonsterBlackBoard : BlackBoard{
    public Transform Target = null;
    [field:SerializeField]
    public MonsterSpawner Spawner { get; private set; } = null;
    public NavMeshAgent Agent;
    public Vector3 SpawnerPos;
    public Vector3 HitDir;

    public MonsterBlackBoard(Transform creature, Animator anim, NavMeshAgent agent, Stat stat, MonsterSpawner spawner) : base(creature, anim, stat)
    {
        Agent = agent;
        Spawner = spawner;
        SpawnerPos = Spawner.transform.position;
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