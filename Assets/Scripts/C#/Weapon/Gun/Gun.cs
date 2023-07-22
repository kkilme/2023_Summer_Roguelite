using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
public class Gun : MonoBehaviour
{
    [SerializeField] private GunData gunData; // 총의 모든 정보 보유

    [SerializeField] private Transform camTransform; // 카메라
    private Camera cam;
    [SerializeField] private Recoil recoil; // 반동 담당

    [SerializeField] private GameObject effectparent; // 이펙트들 모아둘 부모 오브젝트
    [SerializeField] private TextMeshProUGUI AmmoleftText;

    private CancellationTokenSource cancellationTokenSource;

    private float timeSinceLastShot;


    private void Start()
    {
        SetGunData(gunData);
        gunData.reloading = false;
        gunData.currentAmmo = gunData.magSize;
        cam = camTransform.GetOrAddComponent<Camera>();
    }

    private void OnDisable() => gunData.reloading = false;
    public void SetGunData(GunData gunData)
    {
        if (!gunData.isFirstRef)
            this.gunData = gunData;
        else
            gunData.isFirstRef = false;
            this.gunData = gunData.Clone(); // 처음 gunData 사용 시 clone해야함
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
        gunData.reloading = true;

        yield return new WaitForSeconds(gunData.reloadTime);

        gunData.currentAmmo = gunData.magSize;

        gunData.reloading = false;
        Debug.Log("Reload finish");
    }

    // 총 발사 가능 여부 판단
    private bool CanShoot() => !gunData.reloading && gunData.currentAmmo > 0 && (timeSinceLastShot > 1f / gunData.fireRate || gunData.isAutofire);


    /// <summary>
    /// 총의 실제 발사.
    /// </summary>
    public void Shoot()
    {   
        if (CanShoot())
        {   
            for(int i = 0; i < gunData.bulletsPerShoot; i++)
            {
                float spreadx = Random.Range(-gunData.spread, gunData.spread) / 10; // 탄퍼짐
                float spready = Random.Range(-gunData.spread, gunData.spread) / 10;

                Vector3 bulletDir = camTransform.forward + new Vector3(spreadx, spready, 0);

                if (Physics.Raycast(camTransform.position, bulletDir, out RaycastHit hitInfo, gunData.maxDistance))
                {
                    //IDamageable damageable = hitInfo.transform.GetComponent<IDamageable>();
                    //damageable?.TakeDamage(gunData.damage);
                    //Debug.Log(hitInfo.transform.name);
                   Instantiate(gunData.bullethole, // effect생성
                        hitInfo.point + (hitInfo.normal * .01f),
                        Quaternion.LookRotation(hitInfo.normal), effectparent.transform);
                }
            }

            timeSinceLastShot = 0;
            gunData.currentAmmo -= gunData.bulletsPerShoot;

            recoil.MakeRecoil(gunData.recoilX, gunData.recoilY, gunData.recoilZ); // 반동 생성
        }
    }

    /// <summary>
    /// 자동사격. 좌클릭 누르고있으면 계속 발사. 가능한 총과 불가능한 총이 있음. StartShoot()에서 호출.
    /// </summary>
    public async UniTask AutoFire(CancellationToken fireCancellationToken)
    {   
        while (true)
        {
            for (int i = 0; i < gunData.bulletsPerShoot; i++)
            {
                Shoot();
            }
            await UniTask.Delay((int)(1000 / gunData.fireRate), cancellationToken: fireCancellationToken); // 다음 발사까지 걸리는 시간

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
        if (gunData.isAutofire)
        {
            if (cancellationTokenSource != null && !cancellationTokenSource.Token.IsCancellationRequested)
                return;

            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken fireCancellationToken = cancellationTokenSource.Token;

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
        cancellationTokenSource?.Cancel();
    }

    private void Update()
    {
        timeSinceLastShot += Time.deltaTime;
        Debug.DrawRay(camTransform.position, camTransform.forward * gunData.maxDistance, Color.red);
        AmmoleftText.text = $"Ammo left: {gunData.currentAmmo} / {gunData.magSize}";
    }
}