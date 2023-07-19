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
    private Collider _interactionTrigger;

    private List<InputAction> _actions = new List<InputAction>();
    private Vector3 _moveDir;

    private Stat _stat;

    void Awake()
    {
        Init();
    }

    private void Init()
    {
        _stat = new Stat(1, 1, 2, 1, 1, 5);
        _moveDir = Vector3.zero;
        
        _pi = Util.GetOrAddComponent<PlayerInput>(gameObject);
        _anim = Util.GetOrAddComponent<Animator>(gameObject);

        InitInputSystem();
    }

    private void InitInputSystem()
    {
        _actions.Add(_pi.actions.FindAction("Move"));
        _actions.Add(_pi.actions.FindAction("Attack"));
        _actions.Add(_pi.actions.FindAction("Interaction"));

        _actions[0].performed -= Move;
        _actions[0].performed += Move;

        _actions[0].canceled -= Idle;
        _actions[0].canceled += Idle;

        _actions[1].performed -= Attack;
        _actions[1].performed += Attack;

        _actions[1].canceled -= Idle;
        _actions[1].canceled += Idle;

        _actions[2].performed -= Interaction;
        _actions[2].performed += Interaction;

        _actions[2].canceled -= InteractionCancel;
        _actions[2].canceled += InteractionCancel;
    }

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

    private void Interaction(InputAction.CallbackContext ctx)
    {
        _actions[0].performed -= Move;
        _actions[0].canceled -= Idle;
        _actions[1].performed -= Attack;
        _actions[1].canceled -= Idle;
    }

    private void InteractionCancel(InputAction.CallbackContext ctx)
    {
        _actions[0].performed -= Move;
        _actions[0].performed += Move;

        _actions[0].canceled -= Idle;
        _actions[0].canceled += Idle;

        _actions[1].performed -= Attack;
        _actions[1].performed += Attack;

        _actions[1].canceled -= Idle;
        _actions[1].canceled += Idle;
    }

    public void OnDamaged(int damage)
    {
        _stat.Hp -= damage;
        //사운드
    }

    public void OnHealed(int heal)
    {
        _stat.Hp = _stat.Hp + heal < _stat.MaxHp ? _stat.Hp + heal : _stat.MaxHp;
    }

    private void FixedUpdate()
    {
        transform.position += Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * _moveDir * Time.deltaTime * _stat.Speed;
    }
}