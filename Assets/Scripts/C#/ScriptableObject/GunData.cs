using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Weapon/Gun")]
public class GunData : ScriptableObject
{

    [Header("Info")]
    public new string name;

    [Header("Shooting")]
    public float damage;
    public float maxDistance;
    public int bulletsPerShoot;
    public float spread;
    [Tooltip("1초 당 발사 수")] public float fireRate;
    public bool isRapidFire;
    [Header("Reloading")]
    public int currentAmmo;
    public int magSize;
    public float reloadTime;
    [HideInInspector] public bool reloading;

}
