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
    private SkinnedMeshRenderer _skinnedMeshRenderer;

    [ServerRpc]
    public void SetPlayerStatServerRPC(Stat stat)
    {
        _playerStat = stat;
    }

    public override void OnNetworkSpawn()
    {
        CinemachineVirtualCamera cam = null;
        _interact = GetComponentInChildren<PlayerInteract>();
        _interact.gameObject.SetActive(false);
        if (IsOwner) {
            cam = GameObject.Find("FollowPlayerCam").GetComponent<CinemachineVirtualCamera>();
            cam.Follow = _headTransform;
            _interact.gameObject.SetActive(true);
            _interact.Init(this, cam.transform);
            _playerController = new PlayerController(gameObject, IsOwner, cam, _iaa, _interact);
            _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            //6, 10 12 - 15
            for (int i = 0; i < _skinnedMeshRenderer.materials.Length; ++i)
            {
                if (i != 6 && i != 10 && (i < 12 || i > 15))
                    _skinnedMeshRenderer.materials[i].SetFloat("_Render", 2);
            }
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
        Debug.Log($"damaged {damage}");
        _playerStat.Hp -= damage;
        //사운드
    }

    public void OnHealed(int heal)
    {
        _playerStat.Hp = _playerStat.Hp + heal < _playerStat.MaxHp ? _playerStat.Hp + heal : _playerStat.MaxHp;
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            _playerController.Clear();
            _interact.Clear();
        }
    }
}
