using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class Player : NetworkBehaviour, IAttackable
{
    public Stat PlayerStat { get => _playerStat; }
    private Stat _playerStat;
    public Inventory Inventory { get; private set; }
    private PlayerController _playerController;
    private PlayerInteract _interact;
    [SerializeField]
    private Transform _headTransform;
    [SerializeField]
    private InputActionAsset _iaa;
    private Rigidbody _rigidbody;

    [ServerRpc]
    public void SetPlayerStatServerRPC(Stat stat)
    {
        _playerStat = stat;
    }

    public override void OnNetworkSpawn()
    {
        CinemachineVirtualCamera cam = null;

        if (IsOwner) {
            cam = GameObject.Find("FollowPlayerCam").GetComponent<CinemachineVirtualCamera>();
            cam.Follow = _headTransform;
            _interact = GetComponentInChildren<PlayerInteract>();
            _interact.Init(this, cam.transform);
            _playerController = new PlayerController(gameObject, IsOwner, cam, _iaa, _interact);
        }

        _playerStat = new Stat(5, 5, 10, 5, 5, 5);
        Inventory = Util.GetOrAddComponent<Inventory>(gameObject);
        FindObjectOfType<Canvas>().gameObject.SetActive(true);
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (IsOwner)
            MoveCharacter(_playerController.MoveDir);
    }

    private void MoveCharacter(Vector3 dir)
    {
        _rigidbody.velocity = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * dir * PlayerStat.Speed;
    }

    public void OnDamaged(int damage)
    {
        _playerStat.Hp -= damage;
        //사운드
    }

    public void OnHealed(int heal)
    {
        _playerStat.Hp = _playerStat.Hp + heal < _playerStat.MaxHp ? _playerStat.Hp + heal : _playerStat.MaxHp;
    }

    public override void OnNetworkDespawn()
    {
        _playerController.Clear();
        _interact.Clear();
    }
}
