using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GunDataFactory
{
    public static GunData GetGunData(ITEMNAME name)
    {
        switch (name)
        {
            case ITEMNAME.TESTASSAULTRIFLE:
                return new GunData(
                    gunName: ITEMNAME.TESTASSAULTRIFLE,
                    gunNameStr: "TestAssualtRifle",
                    damage: 10,
                    bulletSpeed: 50,
                    bulletsPerShoot: 1,
                    bulletLifetime: 5,
                    spreadRate: 0,
                    zoomSpeed: 5,
                    zoomRate: 1.2f,
                    fireRate: 5,
                    isAutofire: true,
                    availableAttachmentTypes: new AttachmentTypeList() { ATTACHMENT_TYPE.Scope, ATTACHMENT_TYPE.Stock, ATTACHMENT_TYPE.Mag, ATTACHMENT_TYPE.Muzzle, ATTACHMENT_TYPE.Grip, ATTACHMENT_TYPE.Flashlight },
                    recoilX: -1.2f,
                    recoilY: 0.4f,
                    recoilZ: 0,
                    aimRecoilX: -0.6f,
                    aimRecoilY: 0.2f,
                    aimRecoilZ: 0,
                    ammoType: AMMO_TYPE.AMMO_556,
                    currentAmmo: 180,
                    magSize: 180,
                    reloadTime: 1.5f,
                    basePosX: 0,
                    basePosY: 0,
                    basePosZ: 0,
                    baseRotX: 0,
                    baseRotY: 0,
                    baseRotZ: 0
                  ) ;
            case ITEMNAME.TESTMACHINEGUN:
                return new GunData(
                    gunName: ITEMNAME.TESTMACHINEGUN,
                    gunNameStr: "TestMachinegun",
                    damage: 1,
                    bulletSpeed: 50,
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
                    reloadTime: 4f,
                    basePosX: 0,
                    basePosY: 0,
                    basePosZ: 0,
                    baseRotX: 0,
                    baseRotY: 0,
                    baseRotZ: 0
                  );
            case ITEMNAME.SHOTGUN:
                return new GunData(
                    gunName: ITEMNAME.SHOTGUN,
                    gunNameStr: "Shotgun",
                    damage: 5,
                    bulletSpeed: 25,
                    bulletsPerShoot: 8,
                    bulletLifetime: 5,
                    spreadRate: 0.7f,
                    zoomSpeed: 4,
                    zoomRate: 1.2f,
                    fireRate: 0.4f,
                    isAutofire: false,
                    availableAttachmentTypes: new AttachmentTypeList() { ATTACHMENT_TYPE.Mag, ATTACHMENT_TYPE.Grip},
                    recoilX: -6f,
                    recoilY: 0,
                    recoilZ: 0,
                    aimRecoilX: -4f,
                    aimRecoilY: 0,
                    aimRecoilZ: 0,
                    ammoType: AMMO_TYPE.GAUGE_12,
                    currentAmmo: 30,
                    magSize: 30,
                    reloadTime: 3f,
                    basePosX: -0.538f,
                    basePosY: 0.13088f,
                    basePosZ: 3.28810f,
                    baseRotX: 4.843f,
                    baseRotY: -91.873f,
                    baseRotZ: 0.923f
                  );
            case ITEMNAME.ASSAULTRIFLE:
                return new GunData(
                    gunName: ITEMNAME.ASSAULTRIFLE,
                    gunNameStr: "AssaultRifle",
                    damage: 10,
                    bulletSpeed: 50,
                    bulletsPerShoot: 1,
                    bulletLifetime: 5,
                    spreadRate: 0,
                    zoomSpeed: 5,
                    zoomRate: 1.2f,
                    fireRate: 5,
                    isAutofire: true,
                    availableAttachmentTypes: new AttachmentTypeList() { ATTACHMENT_TYPE.Scope, ATTACHMENT_TYPE.Stock, ATTACHMENT_TYPE.Mag, ATTACHMENT_TYPE.Muzzle, ATTACHMENT_TYPE.Grip, ATTACHMENT_TYPE.Flashlight },
                    recoilX: -1.2f,
                    recoilY: 0.4f,
                    recoilZ: 0,
                    aimRecoilX: -0.6f,
                    aimRecoilY: 0.2f,
                    aimRecoilZ: 0,
                    ammoType: AMMO_TYPE.AMMO_556,
                    currentAmmo: 180,
                    magSize: 180,
                    reloadTime: 1.5f,
                    basePosX: 0,
                    basePosY: 0.0048f,
                    basePosZ: 0,
                    baseRotX: 0,
                    baseRotY: 0,
                    baseRotZ: 0
                  );
            case ITEMNAME.SNIPERRIFLE:
                return new GunData(
                    gunName: ITEMNAME.ASSAULTRIFLE,
                    gunNameStr: "SniperRifle",
                    damage: 30,
                    bulletSpeed: 100,
                    bulletsPerShoot: 1,
                    bulletLifetime: 5,
                    spreadRate: 0,
                    zoomSpeed: 5,
                    zoomRate: 1.2f,
                    fireRate: 5,
                    isAutofire: true,
                    availableAttachmentTypes: new AttachmentTypeList() { ATTACHMENT_TYPE.Scope, ATTACHMENT_TYPE.Stock, ATTACHMENT_TYPE.Mag, ATTACHMENT_TYPE.Muzzle, ATTACHMENT_TYPE.Grip, ATTACHMENT_TYPE.Flashlight },
                    recoilX: -4f,
                    recoilY: 0.4f,
                    recoilZ: 0,
                    aimRecoilX: -2f,
                    aimRecoilY: 0.2f,
                    aimRecoilZ: 0,
                    ammoType: AMMO_TYPE.AMMO_762,
                    currentAmmo: 5,
                    magSize: 5,
                    reloadTime: 3f,
                    basePosX: 0.077f,
                    basePosY: 1.501f,
                    basePosZ: 0.292f,
                    baseRotX: 0,
                    baseRotY: 180f,
                    baseRotZ: 0
                  );
            default:
                return new GunData();
        }
    }

    
}
