using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public partial class Player : NetworkBehaviour, IAttackable
{
    public PlayerLock ServerLock { get => _playerController.ServerLock; }
    public Stat PlayerStat { get => _playerStat.Value; }
    [SerializeField]
    private NetworkVariable<Stat> _playerStat = new NetworkVariable<Stat>();
    public Inventory Inventory { get; private set; }
    private PlayerStatusUI _statusUI;
    private PlayerController _playerController;
    private PlayerInteract _interact;
    private Animator _anim;
    [SerializeField]
    private Transform _headTransform;
    [SerializeField]
    private Camera _armNWeaponCam;
    [SerializeField]
    private InputActionAsset _iaa;
    private Rigidbody _rigidbody;
    private SkinnedMeshRenderer _skinnedMeshRenderer;
    private Transform _rotationTransform;

    private Transform _gunTransform;

    [SerializeField]
    private CinemachineVirtualCamera _followPlayerCam;
    [SerializeField]
    private CinemachineVirtualCamera _deadPlayerCam;
    [SerializeField]
    private Camera _mainCam;

    private Vector2 _screenMid;


    private int _maxX = 80;
    private int _minX = -80;

    private float _rotationX = 0;
    private float _rotationY = 0;

    [Header("RotationIK")]
    [SerializeField]
    private float _sensitive = 0.5f;
    [SerializeField]
    private Transform _target;
    [SerializeField]
    private Transform _targetOriginAngle;
    [SerializeField]
    private Transform _rootTarget;

    [Header("Test")]
    [SerializeField] private bool checkTruefortest = false;

    public void SetPlayerStat(Stat stat)
    {
        _playerStat.Value = stat;
    }

    public override void OnNetworkSpawn()
    {
        Init();
    }

    private void Init()
    {
        _interact = GetComponentInChildren<PlayerInteract>();
        _interact.gameObject.SetActive(false);
        //if (IsServer)
        {
            _playerStat.Value = new Stat(5, 5, 5, 5, 5, 5);
        }
        if (IsOwner)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { OwnerClientId }
                }
            };
            InitClientRpc(clientRpcParams);
            InventoryInit();
        }

        else
            Destroy(_mainCam.transform.parent.gameObject);


        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        enabled = true;
    }

    private void InventoryInit()
    {
        var ui = GameManager.Resource.GetObject<GameObject>("UI/PlayerUI");
        var t = ui.GetComponentInChildren<InventoryUI>();
        t.gameObject.SetActive(false);

        ui = Instantiate(ui);

        var invenUi = ui.GetComponentInChildren<InventoryUI>(true);
        Inventory = Util.GetOrAddComponent<Inventory>(gameObject);
        invenUi.Init(Inventory);
        Inventory.InitInventoryUI(invenUi);
        _statusUI = ui.GetComponent<PlayerStatusUI>();
        _statusUI.Init(_playerStat.Value.Hp, _playerStat.Value.MaxHp, 0, 0);

        t.gameObject.SetActive(true);
    }

    private void TestThrowFlashBang()
    {
        var obj = Instantiate(GameManager.Resource.GetObject<GameObject>("Weapon/FlashBang"), _headTransform.position + _headTransform.forward, transform.rotation);
        obj.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    private void MoveCharacterServerRpc(Vector3 dir)
    {
        Vector3 move = (Quaternion.AngleAxis(transform.localEulerAngles.y, Vector3.up) * dir.normalized * PlayerStat.Speed);
        _rigidbody.velocity = move;
        //_rigidbody.AddForce(-9.81f * Vector3.up, ForceMode.VelocityChange);

        if (dir == Vector3.zero)
            InputClientRpc(PlayerInputs.Idle);
        else
            InputClientRpc(move, PlayerInputs.Move);
    }

    [ServerRpc]
    private void InputServerRpc(PlayerInputs pi)
    {
        InputClientRpc(pi);
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
        _statusUI.SetPlayerHP(stat.Hp);
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
                _skinnedMeshRenderer.materials[i].color = Color.white;
        }

        _deadPlayerCam.transform.position = transform.position + Vector3.up * 5;
        //_mainCam.GetComponent<UniversalAdditionalCameraData>().cameraStack.Clear();
        _mainCam.cullingMask = -1;

        _followPlayerCam.Priority = 0;
        _followPlayerCam.Follow = null;
        _deadPlayerCam.LookAt = _headTransform;
        //_followPlayerCam.gameObject.SetActive(false);
        //_playerController.Clear();
        //_interact.Clear();
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

    public bool Equip(ITEMNAME equip, EquipStat equipStat)
    {
        var stat = _playerStat.Value;
        switch (equip)
        {
            case var _ when equip > ITEMNAME.HEADSTART && equip <= ITEMNAME.HEADEND:
                stat.HeadEquip = equipStat;
                break;

            case var _ when equip > ITEMNAME.CLOTHSTART && equip <= ITEMNAME.CLOTHEND:
                stat.ClothEquip = equipStat;
                break;
        }

        _playerStat.Value = stat;
        return true;
    }

    public bool Equip(GunData gunData)
    {
        Gun gun = _gunTransform.GetComponent<Gun>();
        if (gun.GunData.gunNameStr != null) { gun.transform.Find(gun.GunData.gunNameStr).gameObject.SetActive(false); }
        Transform ikParent = gameObject.transform.Find("aimIK");
        Transform newgun = gun.transform.Find(gunData.gunNameStr);
        ikParent.Find("RightHandGrip").GetComponent<TwoBoneIKConstraint>().data.target = newgun.Find("RightHandGrip_Target");
        ikParent.Find("LeftHandGrip").GetComponent<TwoBoneIKConstraint>().data.target = newgun.Find("LeftHandGrip_Target");
        ikParent.Find("LeftThumb").GetComponent<TwoBoneIKConstraint>().data.target = newgun.Find("LeftThumb_Target");
        gun.SetMuzzleTransform(newgun.Find("Muzzle"));
        newgun.gameObject.SetActive(true);
        gun.SetGunData(gunData);
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
