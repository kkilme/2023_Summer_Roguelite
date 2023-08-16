using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GunDataFactory
{
    public static GunData GetGunData(GUN_NAME name)
    {
        switch (name)
        {
            case GUN_NAME.TestAssaultRifle:
                return new GunData(
                    gunName: GUN_NAME.TestAssaultRifle,
                    damage: 10,
                    bulletSpeed: 3,
                    bulletsPerShoot: 1,
                    bulletLifetime: 5,
                    spreadRate: 0,
                    zoomSpeed: 5,
                    zoomRate: 1.2f,
                    fireRate: 5,
                    isAutofire: true,
                    availableAttachmentTypes: new AttachmentTypeList() { ATTACHMENT_TYPE.Scope, ATTACHMENT_TYPE.Stock, ATTACHMENT_TYPE.Mag, ATTACHMENT_TYPE.Muzzle, ATTACHMENT_TYPE.Grip, ATTACHMENT_TYPE.Flashlight},
                    recoilX: -1.2f,
                    recoilY: 0.4f,
                    recoilZ: 0,
                    aimRecoilX: -0.6f,
                    aimRecoilY: 0.2f,
                    aimRecoilZ: 0,
                    ammoType: AMMO_TYPE.AMMO_556,
                    currentAmmo: 180,
                    magSize: 180,
                    reloadTime: 1.5f
                  ) ;
            case GUN_NAME.TestMachinegun:
                return new GunData(
                    gunName: GUN_NAME.TestMachinegun,
                    damage: 1,
                    bulletSpeed: 30,
                    bulletsPerShoot: 1,
                    bulletLifetime: 5,
                    spreadRate: 0.7f,
                    zoomSpeed: 3,
                    zoomRate: 1.2f,
                    fireRate: 15,
                    isAutofire: true,
                    availableAttachmentTypes: new AttachmentTypeList() { ATTACHMENT_TYPE.Mag, ATTACHMENT_TYPE.Grip },
                    recoilX: -2f,
                    recoilY: 1f,
                    recoilZ: 0,
                    aimRecoilX: -1.5f,
                    aimRecoilY: 0.8f,
                    aimRecoilZ: 0,
                    ammoType: AMMO_TYPE.AMMO_762,
                    currentAmmo: 300,
                    magSize: 300,
                    reloadTime: 4f
                  );
            case GUN_NAME.TestShotgun:
                
                return new GunData(
                    gunName: GUN_NAME.TestMachinegun,
                    damage: 5,
                    bulletSpeed: 20,
                    bulletsPerShoot: 5,
                    bulletLifetime: 5,
                    spreadRate: 0.7f,
                    zoomSpeed: 4,
                    zoomRate: 1.2f,
                    fireRate: 0.4f,
                    isAutofire: false,
                    availableAttachmentTypes: new AttachmentTypeList() { ATTACHMENT_TYPE.Mag, ATTACHMENT_TYPE.Grip, ATTACHMENT_TYPE.Scope },
                    recoilX: -6f,
                    recoilY: 0,
                    recoilZ: 0,
                    aimRecoilX: -4f,
                    aimRecoilY: 0,
                    aimRecoilZ: 0,
                    ammoType: AMMO_TYPE.GAUGE_12,
                    currentAmmo: 30,
                    magSize: 30,
                    reloadTime: 3f
                  );
            default:
                return new GunData();
        }
    }

    
}
