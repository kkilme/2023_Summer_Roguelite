using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UniRx;
using UniRx.Triggers;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerLock
{
    No,
    Yes
}

public class PlayerController : NetworkBehaviour
{
    //서버만 상태를 들고 있도록
    public PlayerLock ServerLock { get; private set; }
    private Player _player;
    private PlayerInteract _interact;

    public Animator Anim { get; private set; }
    public PlayerInput Pi { get; private set; }

    public MouseInput MouseInput { get; private set; }

    private Gun _weapon;

    private List<InputAction> _actions = new List<InputAction>();
    public Vector3 MoveDir { get; private set; }
    

    public void Init(CinemachineVirtualCamera cam, InputActionAsset iaa, PlayerInteract interact)
    {
        _player = Util.GetOrAddComponent<Player>(gameObject);
        _weapon = gameObject.GetComponentInChildren<Gun>();
        MoveDir = Vector3.zero;
        Pi = Util.GetOrAddComponent<PlayerInput>(gameObject);
        Pi.actions = null;
        Pi.actions = iaa;
        Anim = Util.GetOrAddComponent<Animator>(gameObject);
        MouseInput = gameObject.GetComponentInChildren<MouseInput>();
        
        if (IsOwner)
        {
            MouseInput = gameObject.GetComponentInChildren<MouseInput>();
            InitInputSystem();
            MouseInput.Init(cam.transform);
            _interact = interact;
        }
    }

    private void InitInputSystem()
    {
        _actions.Add(Pi.actions.FindAction("Move"));
        _actions.Add(Pi.actions.FindAction("Attack"));
        _actions.Add(Pi.actions.FindAction("Interaction"));
        _actions.Add(Pi.actions.FindAction("Reload"));
        _actions.Add(Pi.actions.FindAction("Aim"));
        _actions.Add(Pi.actions.FindAction("Inventory"));

        _actions[0].performed -= Move;
        _actions[0].performed += Move;

        _actions[0].canceled -= Idle;
        _actions[0].canceled += Idle;

        _actions[1].started -= Attack;
        _actions[1].started += Attack;

        _actions[1].canceled -= StopAttack;
        _actions[1].canceled += StopAttack;

        _actions[2].performed -= Interaction;
        _actions[2].performed += Interaction;

        _actions[2].canceled -= InteractionCancel;
        _actions[2].canceled += InteractionCancel;

        _actions[3].performed -= Reload;
        _actions[3].performed += Reload;

        _actions[4].started -= Aim;
        _actions[4].started += Aim;

        _actions[4].canceled -= StopAim;
        _actions[4].canceled += StopAim;

        _actions[5].performed -= SwitchInventoryPannel;
        _actions[5].performed += SwitchInventoryPannel;
    }

    private void Move(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        MoveDir = new Vector3(input.x, 0, input.y);
    }

    private void Idle(InputAction.CallbackContext ctx)
    {
        MoveDir = Vector3.zero;
        //_weapon?.StopShoot();
        //Idle 애니메이션
    }

    private void Attack(InputAction.CallbackContext ctx)
    {
        //Attack 애니메이션, 무기 공격
        _weapon?.StartShoot();
    }

    private void StopAttack(InputAction.CallbackContext ctx)
    {
        _weapon?.StopShoot();
    }

    private void Reload(InputAction.CallbackContext ctx)
    {
        _weapon?.StartReload();
    }

    private void Aim(InputAction.CallbackContext ctx)
    {
        _weapon.Aim();
    }

    private void StopAim(InputAction.CallbackContext ctx)
    {
        _weapon.StopAim();
    }

    private void Interaction(InputAction.CallbackContext ctx)
    {
        if (_interact.Item != null)
        {
            _interact?.Item?.Interact(_player);
            _actions[0].performed -= Move;
            _actions[0].canceled -= Idle;
            _actions[1].started -= Attack;
            _actions[1].canceled -= StopAttack;
            _actions[3].performed -= Reload;
            _actions[4].started -= Aim;
            MouseInput.enabled = false;
            InteractionServerRPC(true);
        }
    }

    [ServerRpc]
    private void InteractionServerRPC(bool block)
    {
        if (block)
            ServerLock = PlayerLock.Yes;
        else
            ServerLock = PlayerLock.No;
    }

    private void InteractionCancel(InputAction.CallbackContext ctx)
    {
        CancelInteraction();
    }

    public void CancelInteraction()
    {
        if (_interact.Item != null)
        {
            _interact.Item?.InteractComplete(false);

            _actions[0].performed -= Move;
            _actions[0].performed += Move;

            _actions[0].canceled -= Idle;
            _actions[0].canceled += Idle;

            _actions[1].started -= Attack;
            _actions[1].started += Attack;

            _actions[1].canceled -= StopAttack;
            _actions[1].canceled += StopAttack;

            _actions[3].performed -= Reload;
            _actions[3].performed += Reload;

            _actions[4].started -= Aim;
            _actions[4].started += Aim;
            MouseInput.enabled = true;
            InteractionServerRPC(false);
        }
    }

    private void SwitchInventoryPannel(InputAction.CallbackContext ctx)
    {
        bool invopen = _player.Inventory.SwitchInventoryPanel();
        if (invopen)
        {
            _actions[1].started -= Attack;
            _actions[4].started -= Aim;
        }
        else
        {
            _actions[1].started -= Attack;
            _actions[4].started -= Aim;
            _actions[1].started += Attack;
            _actions[4].started += Aim;
        }
        
        MouseInput.OnOffSettingUI(invopen);
    }


    public void Clear()
    {
        if (_actions.Count == 0) return;
        _actions[0].performed -= Move;
        _actions[0].canceled -= Idle;
        _actions[1].started -= Attack;
        _actions[1].canceled -= StopAttack;
        _actions[3].performed -= Reload;
        _actions[4].started -= Aim;
        _actions[4].canceled -= StopAim;
        _actions[5].performed -= SwitchInventoryPannel;
        MouseInput.Clear();
        //_weapon.Clear();
    }
}