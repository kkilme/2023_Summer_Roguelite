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

public enum AMMOTYPE
{
    AMMO_9,
    AMMO_556,
    AMMO_762,
    GAUGE_12
}

public class Gun : NetworkBehaviour
{
    [SerializeField] private GunData _gunData; // 총의 모든 정보 보유

    [SerializeField] private GunCamera _cam; // 카메라
    [SerializeField] private Recoil _recoil; // 반동 담당

    [SerializeField] private Transform _effectparent; // 이펙트들 모아둘 부모 오브젝트
    [SerializeField] private TextMeshProUGUI _ammoleftText;

    [SerializeField] private Transform _muzzleTransform;

    private Animator _animator;

    private CancellationTokenSource _cancellationTokenSource;
    private float _timeSinceLastShot;
    private bool _isaiming = false;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _effectparent = GameObject.Find("Effect").transform;
        _ammoleftText = GameObject.Find("Ammo left").GetComponent<TextMeshProUGUI>();

        Init();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) _cam.DisableCamera(); // 내 플레이어 아니면 카메라 비활성화
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
        //_animator.SetFloat("test" + "_duration", 3f);
    }


    /// <summary>
    /// 재장전 코루틴 시작
    /// </summary>
    public void StartReload()
    {
        StartCoroutine(Reload());
    }


    /// <summary>
    /// 재장전
    /// </summary>
    private IEnumerator Reload()
    {
        Debug.Log("Reload Start");
        _gunData.reloading = true;

        yield return new WaitForSeconds(_gunData.reloadTime);

        _gunData.currentAmmo = _gunData.magSize;

        _gunData.reloading = false;
        Debug.Log("Reload finish");
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

                Vector3 bulletDir = _cam.transform.forward + new Vector3(spreadx, spready, 0);

                MakeBulletServerRPC(bulletDir);
            }

            _timeSinceLastShot = 0;
            _gunData.currentAmmo -= _gunData.bulletsPerShoot;

            if(!_isaiming)
                _recoil.MakeRecoil(_gunData.recoilX, _gunData.recoilY, _gunData.recoilZ); // 반동 생성
            else
                _recoil.MakeRecoil(_gunData.aimRecoilX, _gunData.aimRecoilY, _gunData.aimRecoilZ);
        }
    }
    /// <summary>
    /// make bullet
    /// </summary>
    [ServerRpc]
    private void MakeBulletServerRPC(Vector3 dir)
    {
        GameObject bullet = Instantiate(_gunData.bulletprefab, _muzzleTransform.position, _cam.transform.rotation);
        bullet.GetComponent<Bullet>().Init(dir, _gunData.maxDistance, _gunData.damage);
        bullet.GetComponent<NetworkObject>().Spawn(true);
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
        _timeSinceLastShot += Time.deltaTime;

        Debug.DrawRay(_cam.transform.position, _cam.transform.forward * _gunData.maxDistance, Color.red);
        _ammoleftText.text = $"Ammo left: {_gunData.currentAmmo} / {_gunData.magSize}";
    }
}