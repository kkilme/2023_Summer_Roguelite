using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class Player : NetworkBehaviour, IAttackable
{
    public Stat PlayerStat { get => _playerStat.Value; }
    private NetworkVariable<Stat> _playerStat = new NetworkVariable<Stat>();
    public Inventory Inventory { get; private set; }
    private PlayerController _playerController;
    private PlayerInteract _interact;
    [SerializeField]
    private Transform _headTransform;
    [SerializeField]
    private Camera _armNWeaponCam;
    [SerializeField]
    private InputActionAsset _iaa;
    private Rigidbody _rigidbody;
    private SkinnedMeshRenderer _skinnedMeshRenderer;

    [SerializeField]
    private CinemachineVirtualCamera _followPlayerCam;
    [SerializeField]
    private CinemachineVirtualCamera _deadPlayerCam;
    [SerializeField]
    private Camera _mainCam;

    public void SetPlayerStat(Stat stat)
    {
        _playerStat.Value = stat;
    }

    public override void OnNetworkSpawn()
    {
        _interact = GetComponentInChildren<PlayerInteract>();
        _interact.gameObject.SetActive(false);
        if (IsOwner) {
            //오버레이 카메라 추가
            _mainCam.GetComponent<UniversalAdditionalCameraData>().cameraStack.Add(_armNWeaponCam);

            _followPlayerCam.Follow = _headTransform;
            _interact.gameObject.SetActive(true);
            _interact.Init(this, _followPlayerCam.transform);
            _playerController = new PlayerController(gameObject, IsOwner, _followPlayerCam, _iaa, _interact);
            _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            //6, 10 12 - 15
            for (int i = 0; i < _skinnedMeshRenderer.materials.Length; ++i)
            {
                if (i != 6 && i != 10 && (i < 12 || i > 15))
                    _skinnedMeshRenderer.materials[i].SetFloat("_Render", 2);
            }
        }

        else
            Destroy(_mainCam.transform.parent.gameObject);

        if (IsServer)
        {
            _playerStat.Value = new Stat(5, 5, 10, 5, 5, 5);
        }

        Inventory = Util.GetOrAddComponent<Inventory>(gameObject);
        FindObjectOfType<Canvas>().gameObject.SetActive(true);
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (IsOwner)
            MoveCharacter(_playerController.MoveDir);

        if (Input.GetKey(KeyCode.L))
            Dead();

        if (Input.GetKey(KeyCode.R))
            TestReturn();
    }

    private void MoveCharacter(Vector3 dir)
    {
        _rigidbody.velocity = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * dir * PlayerStat.Speed;
    }

    public void OnDamaged(int damage)
    {
        if (!IsServer)
        {
            Debug.LogError("Client Can't Modify Player Stat!");
            return;
        }

        Debug.Log($"damaged {damage}");
        var stat = _playerStat.Value;
        stat.Hp -= damage;
        _playerStat.Value = stat;
        CancelInteraction();
        //사운드
        if (_playerStat.Value.Hp <= 0 && IsOwner)
            Dead();
    }

    public void CancelInteraction()
    {
        _playerController.CancelInteraction();
    }

    private void Dead()
    {
        for (int i = 0; i < _skinnedMeshRenderer.materials.Length; ++i)
        {
            if (i != 6 && i != 10 && (i < 12 || i > 15))
                _skinnedMeshRenderer.materials[i].SetFloat("_Render", 0);
        }

        _deadPlayerCam.transform.position = transform.position + Vector3.up * 5;
        _mainCam.GetComponent<UniversalAdditionalCameraData>().cameraStack.Clear();
        _mainCam.cullingMask = -1;

        _followPlayerCam.Priority = 0;
        _followPlayerCam.Follow = null;
        _deadPlayerCam.LookAt = _headTransform;
        _followPlayerCam.gameObject.SetActive(false);
        _playerController.Clear();
        _interact.Clear();
    }


    //테스트용 함수
    private void TestReturn()
    {
        _followPlayerCam.Priority = 10;
        _followPlayerCam.Follow = _headTransform;
        for (int i = 0; i < _skinnedMeshRenderer.materials.Length; ++i)
        {
            if (i != 6 && i != 10 && (i < 12 || i > 15))
                _skinnedMeshRenderer.materials[i].SetFloat("_Render", 2);
        }
        _followPlayerCam.gameObject.SetActive(true);
    }

    public bool OnHealed(int heal)
    {
        if (!IsServer)
        {
            Debug.LogError("Client Can't Modify Player Stat!");
            return false;
        }

        if (_playerStat.Value.Hp == _playerStat.Value.MaxHp)
        {
            return false;
        }

        var stat = _playerStat.Value;
        stat.Hp = Mathf.Min(stat.MaxHp, stat.Hp + heal);

        _playerStat.Value = stat;
        return true;
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
