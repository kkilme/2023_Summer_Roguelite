using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class Gun : NetworkBehaviour
{
    // 추후에 따로 Gundata Struct를 만들어 네트워크로 관리할 수 있도록 개선
    [SerializeField] private GunData _gunData; // 총의 모든 정보 보유

    [SerializeField] private GunCamera _cam; // 카메라
    [SerializeField] private Recoil _recoil; // 반동 담당

    [SerializeField] private Transform _effectparent; // 이펙트들 모아둘 부모 오브젝트
    [SerializeField] private TextMeshProUGUI _ammoleftText;

    [SerializeField] private Transform _muzzleTransform; // 총알이 생성되는 위치

    private Animator _animator;

    private CancellationTokenSource _cancellationTokenSource;
    private float _timeSinceLastShot;
    private bool _isaiming = false;

    //private void Start()
    //{
    //    _animator = GetComponent<Animator>();
    //    _effectparent = GameObject.Find("Effect").transform;
    //    _recoil = GameObject.Find("recoil").GetComponent<Recoil>();
    //    _ammoleftText = GameObject.Find("Ammo left").GetComponent<TextMeshProUGUI>();
    //    //_cam = GameObject.Find("FollowPlayerCam").GetComponent<GunCamera>();
    //    transform.LookAt(_cam.transform.position + (_cam.transform.forward * 30));
    //    Init();
    //}

    public override void OnNetworkSpawn()
    {
        _cam = GameObject.Find("FollowPlayerCam").GetComponent<GunCamera>();
        if (IsOwner)
        {
            _animator = GetComponent<Animator>();
            _effectparent = GameObject.Find("Effect").transform;
            _recoil = GameObject.Find("recoil").GetComponent<Recoil>();
            //_ammoleftText = GameObject.Find("Ammo left").GetComponent<TextMeshProUGUI>();
            transform.LookAt(_cam.transform.position + (_cam.transform.forward * 30));
            Init();
        }

        //if (!IsOwner) _cam.DisableCamera(); // 내 플레이어 아니면 카메라 비활성화
    }

    private void Init()
    {
        SetGunData(_gunData);
        _gunData.reloading = false;
        _gunData.currentAmmo = _gunData.magSize;
        _cam.SetZoomSpeed(_gunData.zoomSpeed);

        //SetAnimatorTransitionDuration();
    }

    private void OnDisable() => _gunData.reloading = false;
    public void SetGunData(GunData gunData)
    {
        if (!gunData.isFirstRef)
            this._gunData = gunData;
        else
            gunData.isFirstRef = false;
            this._gunData = gunData.Clone(); // 처음 gunData 사용 시 clone해야함
    }

    private void SetAnimatorTransitionDuration() 
    {
        // 총 별로 우클릭 시 총을 들어올리는 애니메이션 속도를 조절하고 싶었으나, 애니메이터 컨트롤러의 트랜지션을 직접 수정할 수 있는 방법은 없어보임.
    }


    /// <summary>
    /// 재장전 코루틴 시작
    /// </summary>
    public void StartReload(Inventory inventory)
    {
        // 서버에서 리로드 관리
        ReloadServerRPC(inventory.GetComponent<NetworkObject>());
    }

    [ServerRpc]
    private void ReloadServerRPC(NetworkObjectReference networkObjectReference, ServerRpcParams serverRpcParams = default)
    {
        if (!_gunData.reloading)
        {
            NetworkObject networkObj = networkObjectReference;
            var inventory = networkObj.GetComponent<Inventory>();

            int requiredAmmo = _gunData.magSize - _gunData.currentAmmo;
            int fillAmount = 0;

            while (fillAmount < requiredAmmo)
            {
                if (inventory.HasItem((ITEMNAME)Enum.Parse(typeof(ITEMNAME), _gunData.ammoType.ToString()), out InventoryItem item))
                {
                    if (fillAmount + item.currentCount < requiredAmmo)
                    {
                        // 총알 아이템의 현재 갯수를 모두 소모해도 탄약이 더 필요하다면 선택된 총알 아이템 제거
                        fillAmount += item.currentCount;
                        inventory.RemoveItem(item);
                    }
                    else
                    {
                        int removedCount = requiredAmmo - fillAmount;
                        item.currentCount -= removedCount;
                        fillAmount = requiredAmmo;
                        inventory.items[inventory.FindIndex(item)] = item;
                    }
                }
                else
                    break;
            }

            if (fillAmount > 0)
                Reload(fillAmount, serverRpcParams.Receive.SenderClientId).Forget();
        }
    }

    /// <summary>
    /// 재장전
    /// </summary>
    private async UniTaskVoid Reload(int amount, ulong targetId)
    {
        Debug.Log("Reload Start");
        _gunData.reloading = true;

        await UniTask.Delay((int)(1000 * _gunData.reloadTime));

         _gunData.currentAmmo += amount;
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { targetId }
            }
        };
        ReloadClientRPC(amount, clientRpcParams);

        _gunData.reloading = false;
        Debug.Log("Reload finish");
    }

    [ClientRpc]
    private void ReloadClientRPC(int amount, ClientRpcParams clientRpcParams)
    {
        _gunData.currentAmmo = amount;
    }

    // 총 발사 가능 여부 판단
    private bool CanShoot() => !_gunData.reloading && _gunData.currentAmmo > 0 && (_timeSinceLastShot > 1f / _gunData.fireRate || _gunData.isAutofire);


    /// <summary>
    /// 총의 실제 발사.
    /// </summary>
    public void Shoot()
    {   
        if (CanShoot())
        {   
            for(int i = 0; i < _gunData.bulletsPerShoot; i++)
            {
                float spreadx = Random.Range(-_gunData.spread, _gunData.spread) / 10; // 탄퍼짐
                float spready = Random.Range(-_gunData.spread, _gunData.spread) / 10;
                Vector3 bulletDir;
                Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);

                if (Physics.Raycast(ray, out RaycastHit hit, 1000))
                {   
                    bulletDir = hit.point - _muzzleTransform.position;
                }
                else
                {
                    bulletDir = ray.GetPoint(1000) - _muzzleTransform.position;
                }

                bulletDir += new Vector3(spreadx, spready, 0);
        
                if (IsServer)
                {
                    SpawnBulletServerRPC(bulletDir, _cam.transform.rotation);
                }
                else
                {
                    SpawnClientBullet(bulletDir);
                }

            }

            _timeSinceLastShot = 0;
            _gunData.currentAmmo -= 1;

            if (!_isaiming)                
                 _recoil.MakeRecoil(_gunData.recoilX, _gunData.recoilY, _gunData.recoilZ); // 반동 생성 
            else
                 _recoil.MakeRecoil(_gunData.aimRecoilX, _gunData.aimRecoilY, _gunData.aimRecoilZ);
                
        }
    }
    /// <summary>
    /// 클라이언트의 로컬 총알을 먼저 생성한 후, ServerRPC로 서버 총알 생성
    /// </summary>
    /// <param name="dir"></param>
    private void SpawnClientBullet(Vector3 dir)
    {
        //Debug.Log("SpawnClientBullet");
        GameObject bullet = Instantiate(_gunData.clientBulletPrefab, _muzzleTransform.position, _cam.transform.rotation);
        bullet.GetComponent<ClientBullet>().Init(dir, _gunData.bulletSpeed, _gunData.bulletLifetime);
        SpawnBulletServerRPC(dir, _cam.transform.rotation);
    }

    /// <summary>
    /// 서버 총알 생성하고, ClientRPC로 모든 클라이언트에 총알 생성 지시
    /// </summary>
    [ServerRpc]
    private void SpawnBulletServerRPC(Vector3 dir, Quaternion rot)
    {
        //Debug.Log("SpawnBulletServerRPC");
        GameObject bullet = Instantiate(_gunData.serverBulletPrefab, _muzzleTransform.position, rot);
        bullet.GetComponent<ServerBullet>().Init(dir, _gunData.bulletSpeed, _gunData.bulletLifetime, _gunData.damage);
        bullet.GetComponent<NetworkObject>().Spawn();
        SpawnBulletClientRPC(dir);
    }

    /// <summary>
    /// 한 클라이언트가 총알을 쏠 시 다른 클라이언트들도 그 총알을 생성하도록 함
    /// </summary>
    /// <param name="dir"></param>
    [ClientRpc]
    private void SpawnBulletClientRPC(Vector3 dir)
    {
        if (IsOwner) return;
        //Debug.Log("SpawnBulletClientRPC");
        GameObject bullet = Instantiate(_gunData.clientBulletPrefab, _muzzleTransform.position, _cam.transform.rotation);
        bullet.GetComponent<ClientBullet>().Init(dir, _gunData.bulletSpeed, _gunData.bulletLifetime);
    }


    /// <summary>
    /// 자동사격. 좌클릭 누르고있으면 계속 발사. 가능한 총과 불가능한 총이 있음. StartShoot()에서 호출.
    /// </summary>
    public async UniTask AutoFire(CancellationToken fireCancellationToken)
    {   
        while (true)
        {
            for (int i = 0; i < _gunData.bulletsPerShoot; i++)
            {
                Shoot();
            }
            await UniTask.Delay((int)(1000 / _gunData.fireRate), cancellationToken: fireCancellationToken); // 다음 발사까지 걸리는 시간

            if (fireCancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Start to shoot. gundata의 Autofire 여부에 따라, 좌클릭을 누르고 있으면 자동사격을 하거나 안함.
    /// </summary>
    public void StartShoot()
    {
        if (!IsOwner) return;
        if (_gunData.isAutofire)
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
                return;

            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken fireCancellationToken = _cancellationTokenSource.Token;

            UniTask.Void(async () =>
            {
                await AutoFire(fireCancellationToken);
            });
        }
        else
        {
            Shoot();
        }
        
    }
    
    /// <summary>
    /// Stop to shoot.
    /// </summary>
    public void StopShoot()
    {
        _cancellationTokenSource?.Cancel();
    }

    public void Aim()
    {   
        _isaiming = true;
        _animator.SetBool("Aiming", true);
        _cam.SetTargetFOV(_gunData.aimingZoomrate);
    }

    public void StopAim()
    {
        _isaiming = false;
        _animator.SetBool("Aiming", false);
        _cam.SetTargetFOV();
    }

    private void Update()
    {
        if (IsOwner)
        {
            _timeSinceLastShot += Time.deltaTime;
            _ammoleftText.text = $"Ammo left: {_gunData.currentAmmo} / {_gunData.magSize}";
            Debug.DrawRay(_cam.transform.position, _cam.transform.forward * 5, Color.red);
        }
        
    }
}