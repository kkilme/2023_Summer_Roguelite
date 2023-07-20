using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
public class Gun : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private GunData gunData;
    [SerializeField] private Transform cam;
    [SerializeField] private GameObject bulletHole;
    [SerializeField] private GameObject effectparent;
    [SerializeField] private TextMeshProUGUI AmmoleftText;
    private CancellationTokenSource cancellationTokenSource;

    private float timeSinceLastShot;

    private void Start()
    {
        gunData.reloading = false;
        gunData.currentAmmo = gunData.magSize;
    }

    private void OnDisable() => gunData.reloading = false;

    public void StartReload()
    {
        if (!gunData.reloading)
        {
            try
            {
                StartCoroutine(Reload());
            }
            catch
            {
                Debug.Log("reload failed");
            }
        }
    }

    private IEnumerator Reload()
    {
        Debug.Log("Reload Start");
        Debug.Log($"Cur ammo: {gunData.currentAmmo}");
        gunData.reloading = true;

        yield return new WaitForSeconds(gunData.reloadTime);

        gunData.currentAmmo = gunData.magSize;

        gunData.reloading = false;
        Debug.Log($"Cur ammo: {gunData.currentAmmo}");
        Debug.Log("Reload finish");
    }

    private bool CanShoot() => !gunData.reloading && gunData.currentAmmo > 0 && (timeSinceLastShot > 1f / gunData.fireRate || gunData.isRapidFire);

    public void Shoot()
    {   
        if (CanShoot())
        {   
            for(int i=0; i<gunData.bulletsPerShoot; i++)
            {
                float spreadx = Random.Range(-gunData.spread, gunData.spread);
                float spready = Random.Range(-gunData.spread, gunData.spread);

                Vector3 bulletDir = cam.forward + new Vector3(spreadx, spready, 0);

                if (Physics.Raycast(cam.position, bulletDir, out RaycastHit hitInfo, gunData.maxDistance))
                {
                    //IDamageable damageable = hitInfo.transform.GetComponent<IDamageable>();
                    //damageable?.TakeDamage(gunData.damage);
                    //Debug.Log(hitInfo.transform.name);
                    GameObject effect = Instantiate(bulletHole,
                        hitInfo.point + (hitInfo.normal * .01f),
                        Quaternion.LookRotation(hitInfo.normal));
                    effect.transform.parent = effectparent.transform;
                }
            }
            timeSinceLastShot = 0;
            gunData.currentAmmo -= gunData.bulletsPerShoot;
            AmmoleftText.text = $"Ammo left: {gunData.currentAmmo}";
        }
    }
    public async UniTask RapidFire(CancellationToken fireCancellationToken)
    {   
        while (true)
        {
            //Debug.Log("shoot");
            for (int i = 0; i < gunData.bulletsPerShoot; i++)
            {
                Shoot();
            }
            await UniTask.Delay((int)(1000 / gunData.fireRate), cancellationToken: fireCancellationToken);

            if (fireCancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }


    public void StartShoot()
    {
        //Debug.Log("StartShoot");
        if (gunData.isRapidFire)
        {
            if (cancellationTokenSource != null && !cancellationTokenSource.Token.IsCancellationRequested)
                return;

            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken fireCancellationToken = cancellationTokenSource.Token;

            UniTask.Void(async () =>
            {
                await RapidFire(fireCancellationToken);
            });
        }
        else
        {
            Shoot();
        }
        
    }
    
    public void StopShoot()
    {
        //Debug.Log("StopShoot");
        cancellationTokenSource?.Cancel();
    }

    private void Update()
    {
        timeSinceLastShot += Time.deltaTime;
        Debug.DrawRay(cam.position, cam.forward * gunData.maxDistance, Color.red);
    }
}