using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public enum SeqStates
{
    Running,
    Fail,
    Success
}

public class BlackBoard
{
    public Transform CurCreature;
    public Animator Anim;
    public Stat Stat;

    public BlackBoard(Transform creature, Animator anim, Stat stat)
    {
        CurCreature = creature;
        Anim = anim;
        Stat = stat;
    }

    public virtual void Clear()
    {
        Anim = null;
    }
}

[Serializable]
public class BehaviourTree
{
    private List<BehaviourSequence> _seqList = null;

    public string CurLeaf = null;

    public BehaviourTree() 
    {
        _seqList = new List<BehaviourSequence>();
    }

    public void AddSeq(BehaviourSequence seq)
    {
        _seqList.Add(seq);
        
    }

    public void CheckSeq()
    {
        SeqStates state = SeqStates.Fail;

        for (int i = 0; i < _seqList.Count; ++i)
        {
            state = _seqList[i].CheckSeq();

            if (state == SeqStates.Running)
            {
                for (int j = i + 1; j < _seqList.Count; ++j)
                    _seqList[j].CancelSeq();
                
                break;
            }
            
            else if (state == SeqStates.Success) 
                --i;
        }
    }

    public void Clear()
    {
        for (int i = 0; i < _seqList.Count; ++i)
        {
            _seqList[i].Clear();
            _seqList[i] = null;
        }
    }
}

[Serializable]
public class BehaviourSequence
{
    private List<BehaviourSequenceNode> _nodeList = null;
    private BehaviourTree _parent = null;

    public BehaviourSequenceNode CurNode = null;

    public BehaviourSequence(BehaviourTree parent)
    {
        _nodeList = new List<BehaviourSequenceNode>();
        _parent = parent;
    }

    public void AddSequenceNode(BehaviourSequenceNode node)
    {
        _nodeList.Add(node);
    }

    public SeqStates CheckSeq()
    {
        SeqStates state = SeqStates.Success;

        for (int i = 0; i < _nodeList.Count; ++i)
        {
            state = _nodeList[i].CheckNode();
            if (state != SeqStates.Success)
            {
                _parent.CurLeaf = _nodeList[i].CurLeaf;
                return state;
            }
        }

        //완료 시 초기화
        for (int i = 0; i < _nodeList.Count; ++i)
            _nodeList[i].SeqState = SeqStates.Fail;

        return state;
    }

    public void CancelSeq()
    {
        for (int i = 0; i < _nodeList.Count; ++i)
            _nodeList[i].CancelNode();
    }

    public void CompleteSeq()
    {
        _parent.CheckSeq();
    }

    public void Clear()
    {
        for (int i = 0; i < _nodeList.Count; ++i)
        {
            _nodeList[i].Clear();
            _nodeList[i] = null;
        }
    }
}

public abstract class BehaviourSequenceNode
{
    public string CurLeaf;
    protected BehaviourLeaf _curLeaf;
    protected BehaviourSequence _parent;
    protected List<BehaviourLeaf> _nodes = null;

    public SeqStates SeqState;
    protected CancellationTokenSource _cts;

    public BehaviourSequenceNode(CancellationTokenSource cts, BehaviourSequence parent) 
    {
        SeqState = SeqStates.Fail;
        _cts = cts;
        _parent = parent;
        _nodes = new List<BehaviourLeaf>();
    }

    public abstract SeqStates CheckNode();

    public virtual void CancelNode() 
    {
        if (SeqState == SeqStates.Running)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            _curLeaf.CancelBehaviour(_cts);
            _curLeaf = null;
            SeqState = SeqStates.Fail;
        }
    }

    public abstract void CompleteSeq();

    public virtual void Clear()
    {
        _cts.Cancel();
        _cts.Dispose();
        for (int i = 0; i < _nodes.Count; ++i)
        {
            _nodes[i].Clear();
            _nodes[i] = null;
        }
    }
}

public abstract class BehaviourLeaf
{
    protected BehaviourSequenceNode _parent;
    protected CancellationTokenSource _cts;
    protected int _animHash;

    public BehaviourLeaf(BehaviourSequenceNode parent, CancellationTokenSource cts)
    {
        _parent = parent;
        _cts = cts;
    }

    public abstract SeqStates CheckLeaf();

    public abstract void CancelBehaviour(CancellationTokenSource cts);

    public abstract void Clear();
}

[Serializable]
public class BehaviourNormalSelector : BehaviourSequenceNode
{
    public BehaviourNormalSelector(CancellationTokenSource cts, BehaviourSequence parent) : base(cts, parent)
    {
    }

    public void AddNode(BehaviourLeaf node)
    {
        _nodes.Add(node);
    }

    public override void CancelNode()
    {
        base.CancelNode();
    }

    public override SeqStates CheckNode()
    {
        if (SeqState == SeqStates.Fail)
        {
            for (int i = 0; i < _nodes.Count; ++i)
            {
                SeqState = _nodes[i].CheckLeaf();
                if (SeqState != SeqStates.Fail)
                {
                    CurLeaf = _nodes[i].ToString();
                    _curLeaf = _nodes[i];
                    break;
                }
            }
        }

        return SeqState;
    }

    public override void CompleteSeq()
    {
        _parent.CompleteSeq();
    }

    public override void Clear()
    {
        base.Clear();
    }
}

public class BehaviourRandomSelector : BehaviourSequenceNode
{
    System.Random _rand;

    public BehaviourRandomSelector(CancellationTokenSource cts, BehaviourSequence parent) : base(cts, parent) 
    {
        _rand = new System.Random();
    }

    public override void CancelNode()
    {
        base.CancelNode();
    }

    public void AddNode(BehaviourLeaf node)
    {
        _nodes.Add(node);
    }

    public override SeqStates CheckNode()
    {
        if (SeqState == SeqStates.Fail)
        {
            int rand = _rand.Next(0, _nodes.Count);
            SeqState = _nodes[rand].CheckLeaf();
            _curLeaf = _nodes[rand];
            CurLeaf = _nodes[rand].ToString();
            //for (int i = 0; i < _nodes.Count; ++i)
            //{
            //    SeqState = _nodes[i].CheckLeaf();
            //    if (SeqState != SeqStates.Fail)
            //    {
            //        _curLeaf = _nodes[i];
            //        break;
            //    }
            //}
        }

        return SeqState;
    }

    public override void CompleteSeq()
    {
        _parent.CompleteSeq();
    }

    public override void Clear()
    {
        base.Clear();
        _rand = null;
    }
}